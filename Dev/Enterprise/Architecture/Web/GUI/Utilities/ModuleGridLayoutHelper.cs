using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.Utilities
{
	public class ModuleGridLayoutHelper : IDisposable
	{
		public ModuleGridLayoutHelper(WebModuleID moduleID, BusinessObjectFactory factory)
		{
			this.module = ZWebModuleFactory.Create(moduleID, factory, null);
			this.moduleShouldBeDisposed = true;
			this.filterStripBizObj = null;
		}

		public ModuleGridLayoutHelper(SearchControl searchControl)
		{
			this.module = searchControl.Module;
			this.filterStripBizObj = searchControl.FilterStripBizO;
		}

		readonly ZFilterGridModule module;
		readonly FilterStripBusinessObject filterStripBizObj;
		readonly bool moduleShouldBeDisposed;

		#region GetGridColumns

		public List<DataGridColumn> GetGridColumns()
		{
			List<DataGridColumn> columnsToAdd = new List<DataGridColumn>();

			List<int> indexes = GetColumnIndexes(CurrentGridLayout, true);
			foreach (int index in indexes)
			{
				if (index >= 0 && index < module.GridColumnFields.Length)
				{
					columnsToAdd.AddRange(ExtractGroupMembersIfGroupColumn(module.GridColumnFields[index]));
				}
			}

			if (columnsToAdd.Count == 0)
			{
				foreach (DataGridColumn column in module.DefaultGridColumnFields)
				{
					columnsToAdd.AddRange(ExtractGroupMembersIfGroupColumn(column));
				}
			}
			foreach (var column in module.RequiredGridColumnFields)
			{
				if (!columnsToAdd.Contains(column))
				{
					columnsToAdd.Add(column);
				}
			}

			return columnsToAdd;
		}

		#endregion

		#region GridLayout

		public string CurrentGridLayout
		{
			get
			{
				return GetCurrentGridLayout();
			}
			set
			{
				SetGridLayout(value);
			}
		}

		public string GetGridColumnsLayoutKey()
		{
			string currentFilterLayoutPk = "";

			if (filterStripBizObj != null)
			{
				StmModuleFilter layout = filterStripBizObj.LastUsedLayout;
				if (layout != null && layout.S9_SaveColumnLayout)
				{
					currentFilterLayoutPk = layout.PK.ToString();
				}
			}

			return module.ID.ToString() + ".GridLayout" + currentFilterLayoutPk;
		}

		#endregion

		#region Implementation

		string GetCurrentGridLayout()
		{
			string result = null;

			string gridColumnsLayoutKey = GetGridColumnsLayoutKey();

			if (HttpContext.Current != null && HttpContext.Current.Session != null && WebEnv.CurrentUser != null && module != null)
			{
				result = HttpContext.Current.Session[gridColumnsLayoutKey] as string;

				bool isCurrentFilterLayoutLinkedToColumnLayout = false;

				if (filterStripBizObj != null)
				{
					StmModuleFilter layout = filterStripBizObj.LastUsedLayout;
					if (layout != null)
					{
						isCurrentFilterLayoutLinkedToColumnLayout = layout.S9_SaveColumnLayout;
					}
				}

				if (string.IsNullOrEmpty(result) || isCurrentFilterLayoutLinkedToColumnLayout)
				{
					using (MemoryStream layoutStream = GridLayoutRegistry.GetGridLayout(gridColumnsLayoutKey, WebEnv.CurrentUser.PK.ToGuid()))
					{
						if (layoutStream != null)
						{
							using (TextReader layoutReader = new StreamReader(layoutStream))
							{
								result = CheckStoredLayoutsForGroupMembers(layoutReader.ReadToEnd());
								HttpContext.Current.Session.Add(gridColumnsLayoutKey, result);
							}
						}
						else
						{
							HttpContext.Current.Session.Add(gridColumnsLayoutKey, "");
						}
					}
				}
			}

			return result;
		}

		void SetGridLayout(string layout)
		{
			string gridColumnsLayoutKey = GetGridColumnsLayoutKey();

			if (HttpContext.Current != null && HttpContext.Current.Session != null && WebEnv.CurrentUser != null)
			{
				if (HttpContext.Current.Session[gridColumnsLayoutKey] == null)
				{
					HttpContext.Current.Session.Add(gridColumnsLayoutKey, layout);
				}
				else
				{
					HttpContext.Current.Session[gridColumnsLayoutKey] = layout;
				}

				using (MemoryStream layoutStream = new MemoryStream())
				{
					using (TextWriter layoutWriter = new StreamWriter(layoutStream))
					{
						layoutWriter.Write(layout);
					}
					GridLayoutRegistry.SetGridLayout(gridColumnsLayoutKey, WebEnv.CurrentUser.PK.ToGuid(), layoutStream);
				}
			}
		}

		GridLayoutRegistry GridLayoutRegistry
		{
			get
			{
				return gridLayoutRegistry ?? (gridLayoutRegistry = new GridLayoutRegistry());
			}
		}
		GridLayoutRegistry gridLayoutRegistry;

		List<int> GetColumnIndexes(string layout, bool ignoreIncorrectValues)
		{
			List<int> indexes = new List<int>();

			if (!string.IsNullOrEmpty(layout))
			{
				string[] strIndexes = layout.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string strIndex in strIndexes)
				{
					int index;
					if (int.TryParse(strIndex, out index))
					{
						indexes.Add(index);
					}
					else if (!ignoreIncorrectValues)
					{
						return new List<int>();
					}
				}
			}

			return indexes;
		}

		string CheckStoredLayoutsForGroupMembers(string layout)
		{
			StringBuilder result = new StringBuilder(",");

			List<int> indexes = GetColumnIndexes(layout, false);
			foreach (int index in indexes)
			{
				if (index >= 0 && index < module.GridColumnFields.Length)
				{
					DataGridColumn column = module.GridColumnFields[index];

					if (((IList)module.GroupMemberColumnFields).Contains(column))
					{
						int groupIndex = GetGroupIndexForMember(column, module.GridColumnFields);
						if (groupIndex >= 0 && !result.ToString().Contains("," + groupIndex + ","))
						{
							result.Append(groupIndex + ",");
						}
					}
					else
					{
						result.Append(index + ",");
					}
				}
			}

			return result.ToString().Trim(',');
		}

		int GetGroupIndexForMember(DataGridColumn column, DataGridColumn[] gridColumnFields)
		{
			for (int i = 0; i < gridColumnFields.Length; i++)
			{
				ZGroupColumn groupColumn = gridColumnFields[i] as ZGroupColumn;
				if (groupColumn != null && ((IList)groupColumn.GroupMembers).Contains(column))
				{
					return i;
				}
			}
			return -1;
		}

		DataGridColumn[] ExtractGroupMembersIfGroupColumn(DataGridColumn column)
		{
			if (column is ZGroupColumn)
			{
				return ((ZGroupColumn)column).GroupMembers;
			}
			else
			{
				return new DataGridColumn[] { column };
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (this.moduleShouldBeDisposed)
			{
				module.Dispose();
			}
		}

		#endregion
	}
}
