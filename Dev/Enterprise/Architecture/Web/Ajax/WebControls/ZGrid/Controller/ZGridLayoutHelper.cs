using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.ZGridInternals
{
	public class ZGridLayoutManager
	{
		public ZGridLayoutManager(ZGrid grid)
		{
			this.grid = grid;
		}
		readonly ZGrid grid;

		public GridColumnProvider ColumnProvider
		{
			get { return columnProvider ?? (columnProvider = new GridColumnProvider()); }
			set { columnProvider = value; }
		}
		GridColumnProvider columnProvider;

		public string Layout { get; set; }

		public void PopulateColumns()
		{
			grid.Columns.Clear();
			if (!ColumnProvider.IsEmpty)
			{
				int[] keys;
				if (SelectedColumnsKeys != null)
				{
					int[] additonalKeys = CheckSelectedContainsRequiredColumns(SelectedColumnsKeys);
					keys = new int[SelectedColumnsKeys.Length + additonalKeys.Length];
					additonalKeys.CopyTo(keys, 0);
					SelectedColumnsKeys.CopyTo(keys, additonalKeys.Length);
				}
				else
				{
					keys = ColumnProvider.RequiredAndDefaultColumns;
				}
				PopulateColumns(keys);
				if (grid.Columns.Count == 0)
				{
					PopulateColumns(ColumnProvider.RequiredAndDefaultColumns);
				}
			}
		}

		protected void PopulateColumns(int[] keys)
		{
			List<DataGridColumn> newRowColumns = new List<DataGridColumn>();
			if (keys != null)
			{
				List<int> lastCols = new List<int>();
				foreach (var key in keys)
				{
					if (ColumnProvider.ContainsKey(key))
					{
						if (ColumnProvider[key] is ZNewRowColumn)//ZNewRowColumn should be at the end of the grid
						{
							if (!lastCols.Contains(key))
							{
								lastCols.Add(key);
							}
						}
						else
						{
							grid.Columns.Add(ColumnProvider[key]);
						}
					}
				}
				foreach (var key in lastCols)
				{
					if (ColumnProvider.ContainsKey(key))
					{
						grid.Columns.Add(ColumnProvider[key]);
					}
				}
			}
		}

		int[] CheckSelectedContainsRequiredColumns(int[] selectedColumnsKeys)
		{
			List<int> selectedColumnsKeys1 = new List<int>(selectedColumnsKeys);
			List<int> result = new List<int>();
			foreach (var key in ColumnProvider.RequiredColumns)
			{
				if (!selectedColumnsKeys1.Contains(key))
				{
					result.Add(key);
				}
			}
			return result.ToArray();
		}

		public int[] SelectedColumnsKeys
		{
			get { return string.IsNullOrEmpty(GridLayout) ? null : ExplodeLayout(GridLayout); }
		}

		int[] ExplodeLayout(string layout)
		{
			if (string.IsNullOrEmpty(layout))
			{
				return null;
			}
			var splitResults = GridLayout.Split(',');
			var result = new List<int>();
			foreach (var splitResult in splitResults)
			{
				if (!int.TryParse(splitResult, out var key))
				{
					return null;
				}

				result.Add(key);
			}

			return result.ToArray();
		}

		#region Grid Layout As String

		public string GridLayout
		{
			get { return LoadGridLayout(); }
			set
			{
				SaveGridLayout(value);
				PopulateColumns();
			}
		}

		public void SaveAsGridLayout(string layoutName, bool isPublishedForOrganisation, bool isPublishedForCompany)
		{
			SaveGridLayout(GridLayout, layoutName, isPublishedForOrganisation, isPublishedForCompany);
		}

#if DEBUG

		public void SaveGridLayoutForTest(string layout, string layoutName)
		{
			SaveGridLayout(layout, layoutName);
		}

#endif

		void SaveGridLayout(string layout, string layoutName = null, bool isPublishedForOrganisation = false, bool isPublishedForCompany = false)
		{
			var gridColumnsLayoutKey = GetGridColumnsLayoutKey(layoutName);
			if (WebEnv.AppInstance != null && WebEnv.AppInstance.Session != null)
			{
				if (WebEnv.AppInstance.Session[gridColumnsLayoutKey] == null)
				{
					WebEnv.AppInstance.Session.Add(gridColumnsLayoutKey, layout);
				}
				else
				{
					WebEnv.AppInstance.Session[gridColumnsLayoutKey] = layout;
				}

				if (WebEnv.CurrentUser != null)
				{
					if (string.IsNullOrEmpty(layout))
					{
						GridLayoutRegistry.DeleteLayout(gridColumnsLayoutKey, WebEnv.CurrentUser.PK.ToGuid());
					}
					else
					{
						using (MemoryStream layoutStream = new MemoryStream())
						{
							using (TextWriter layoutWriter = new StreamWriter(layoutStream))
							{
								layoutWriter.Write(layout);
							}

							if (isPublishedForOrganisation)
							{
								var loggedInOrg = ((OrgContactWebUser)(WebEnv.AppInstance?.SiteUser))?.LoggedInOrganisation;
								if (loggedInOrg != null)
								{
									GridLayoutRegistry.SetGridLayout(gridColumnsLayoutKey, loggedInOrg.PK.ToGuid(), layoutStream);
								}
							}

							if (isPublishedForCompany)
							{
								GridLayoutRegistry.SetGridLayout(gridColumnsLayoutKey, GlbCompany.CurrentCompany.PK.ToGuid(), layoutStream);
							}

							GridLayoutRegistry.SetGridLayout(gridColumnsLayoutKey, WebEnv.CurrentUser.PK.ToGuid(), layoutStream);
						}
					}
				}
			}
		}

		protected string LoadGridLayout(string layoutName = null)
		{
			string layout = null;
			bool isOldLayout = false;
			var gridColumnsLayoutKey = GetGridColumnsLayoutKey(layoutName);

			if (WebEnv.AppInstance != null && WebEnv.AppInstance.Session != null && WebEnv.CurrentUser != null)
			{
				layout = WebEnv.AppInstance.Session[gridColumnsLayoutKey] as string;

				if (string.IsNullOrEmpty(layout))
				{
					layout = GetRegistryGridLayout(gridColumnsLayoutKey, WebEnv.CurrentUser.PK.ToGuid());
					if (string.IsNullOrEmpty(layout))
					{
						var loggedInOrg = ((OrgContactWebUser)(WebEnv.AppInstance?.SiteUser))?.LoggedInOrganisation;
						if (loggedInOrg != null)
						{
							layout = GetRegistryGridLayout(gridColumnsLayoutKey, loggedInOrg.PK.ToGuid());
						}
					}

					if (string.IsNullOrEmpty(layout))
					{
						layout = GetRegistryGridLayout(gridColumnsLayoutKey, GlbCompany.CurrentCompany.PK.ToGuid());
					}

					if (string.IsNullOrEmpty(layout))
					{
						gridColumnsLayoutKey = GetOldStyleLayoutKey();
						isOldLayout = true;
						layout = GetRegistryGridLayout(gridColumnsLayoutKey, WebEnv.CurrentUser.PK.ToGuid());
					}

					if (!string.IsNullOrEmpty(layout))
					{
						if (isOldLayout)
						{
							layout = ColumnProvider.FixOldLayout(layout);
							if (!string.IsNullOrEmpty(layout))
							{
								SaveGridLayout(layout);
							}
						}
						WebEnv.AppInstance.Session.Add(gridColumnsLayoutKey, layout);
					}
					else
					{
						WebEnv.AppInstance.Session.Add(gridColumnsLayoutKey, "");
					}
				}
			}

			return layout;
		}

		string GetRegistryGridLayout(string columnsLayoutKey, Guid categoryPK)
		{
			using (MemoryStream layoutStream = GridLayoutRegistry.GetGridLayout(columnsLayoutKey, categoryPK))
			{
				if (layoutStream != null)
				{
					using (TextReader layoutReader = new StreamReader(layoutStream))
					{
						return layoutReader.ReadToEnd();
					}
				}
			}

			return null;
		}

		string GetOldStyleLayoutKey()
		{
			string result = string.Empty;
			if (Page != null)
			{
				result = Page.ControlKeyIdentifier(grid) + ".GridLayout";
			}
			return result;
		}

		string GetGridColumnsLayoutKey(string layout = null)
		{
			string result = string.Empty;
			if (string.IsNullOrEmpty(layout))
			{
				layout = Layout;
			}
			if (Page != null)
			{
				result = Page.ControlKeyIdentifier(grid);
			}
			if (string.IsNullOrEmpty(result))
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return "GridIDForTest";
				}
#endif
				throw new NotImplementedException(string.Format("Page {0} does not implement GetControlKeyIdentifier for grid {1}.", Page.GetType().Name, grid.ID));// developer exception
			}
			return string.IsNullOrEmpty(layout) ? result : string.Format("{0}.GridLayout.{1}", result, layout); // Constant for layout storage
		}

		#endregion

		ZPage Page
		{
			get { return grid == null ? null : grid.Page as ZPage; }
		}

		GridLayoutRegistry GridLayoutRegistry
		{
			get
			{
				return gridLayoutRegistry ?? (gridLayoutRegistry = new GridLayoutRegistry());
			}
		}
		GridLayoutRegistry gridLayoutRegistry;
	}
}
