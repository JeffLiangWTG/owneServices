using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public partial class GridLayoutPage : ZIFramePage
	{
		#region Layout Customisation Button Click Handlers

		protected override void CancelButton_Click(object sender, EventArgs e)
		{
			((GridLayoutContainer)DataSource).RollbackChanges();
			Bind();
			base.CancelButton_Click(sender, e);
		}

		void RemoveOneButton_Click(object sender, EventArgs e)
		{
			LayoutContainer.RemoveFromLayout();
			Bind();
		}

		void SelectOneButton_Click(object sender, EventArgs e)
		{
			LayoutContainer.AddToLayout();
			Bind();
		}

		void DefaultButton_Click(object sender, EventArgs e)
		{
			LayoutContainer.RestoreDefault();
			Bind();
		}

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			LayoutContainer.MoveSelectedUp();
			Bind();
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			LayoutContainer.MoveSelectedDown();
			Bind();
		}

		#endregion

		#region Ok&Cancel Arguments

		protected override string[] OKFunctionArguments
		{
			get { return new string[] { '\'' + ((GridLayoutContainer)DataSource).CurrentLayoutString + '\'' }; }
		}

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			GridLayoutContainer result = null;

			string layoutString = RequestQueryString[ZGridLayoutControl.GridCurrentLayoutKey];

			WebModuleID moduleID = ZWebModuleFactory.GetWebModuleIDByName(RequestQueryString[ZGridLayoutControl.GridModuleIDKey]);
			if (moduleID != null)
			{
				if (moduleID != WebModuleIDs.NotAssigned)
				{
					using (ZFilterGridModule module = GetWebModule(moduleID))
					{
						StringCollectionX allColumnHeaders = new StringCollectionX();
						StringBuilder indicesToExclude = new StringBuilder();

						for (int i = 0; i < module.GridColumnFields.Length; i++)
						{
							DataGridColumn column = module.GridColumnFields[i];

							if (((IList)module.GroupMemberColumnFields).Contains(column))
							{
								indicesToExclude.Append(i + ",");
							}

							allColumnHeaders.Add(column.HeaderText);
						}

						string defaultLayout = GetLayoutFromColumns(module.GridColumnFields, module.DefaultGridColumnFields);
						string currentLayout = string.IsNullOrEmpty(layoutString) ? defaultLayout : layoutString;
						string requiredColumns = GetLayoutFromColumns(module.GridColumnFields, module.RequiredGridColumnFields);
						string groupMemberColumns = indicesToExclude.ToString().TrimEnd(',');

						result = new GridLayoutContainer(allColumnHeaders, currentLayout, defaultLayout, requiredColumns, groupMemberColumns);
					}
				}
			}
			else
			{
				throw new ZException("Module ID : " + RequestQueryString[ZGridLayoutControl.GridModuleIDKey] + " resolves to null. Web GridLayoutControl Module must be ZGenericFilterGridModule.");
			}

			return result;
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected virtual ZFilterGridModule GetWebModule(WebModuleID moduleID)
		{
			return ZWebModuleFactory.Create(moduleID, Factory, this);
		}

		protected GridLayoutContainer LayoutContainer
		{
			get { return (GridLayoutContainer)DataSource; }
		}

		#endregion

		#region Implementation

		protected string GetLayoutFromColumns(DataGridColumn[] allColumns, DataGridColumn[] layoutColumns)
		{
			StringBuilder result = new StringBuilder();

			foreach (DataGridColumn column in layoutColumns)
			{
				for (int i = 0; i < allColumns.Length; i++)
				{
					if (allColumns[i] == column)
					{
						result.Append(Convert.ToString(i) + ",");
						break;
					}
				}
			}

			return result.ToString().TrimEnd(',');
		}

		#endregion

		#region Internal Properties

		internal NameValueCollection RequestQueryStringInternal => RequestQueryString;

		#endregion
	}
}
