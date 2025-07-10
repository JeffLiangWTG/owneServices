using System;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public partial class ManageLayoutsPage : ZIFramePageWithFilterStripBizO
	{
		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#region FilterStripBizO

		public override string QueryStringKey
		{
			get { return ManageLayoutsPopup.DataSourceIndexerQueryStringKey; }
		}

		#endregion

		#region ManageLayoutsBizO

		public ManageLayoutsBusinessObject ManageLayoutsBizO
		{
			get { return (ManageLayoutsBusinessObject)DataSource; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new ManageLayoutsBusinessObject(FilterStripBizO, Page.SiteUser);
		}

		#endregion

		#region Bind

		protected override void OnPreBind()
		{
			base.OnPreBind();

			DeleteButton.Click += new EventHandler(DeleteButton_Click);
			RenameButton.Click += new EventHandler(RenameButton_Click);
			RenameButton.OnClientClick = RenameButtonOnClientClickCode;

			FilterLayoutsListBox.AutoPostBack = true;
			ManageLayoutsBizO.SelectedFilterLayoutNameInfo.ValueChanged += new EventHandler(SelectedFilterLayoutInfo_ValueChanged);
		}

		string RenameButtonOnClientClickCode
		{
			get
			{
				#region SuppressResourceStringsCheckRegion

				string rexp = "var rsb = /^\\[/;\nvar reb = /\\]$/\n";
				string hiddenControlValue = "document.getElementById('" + HiddenFilterNameInput.ClientID + "').value";
				string selectedLayoutName = "document.getElementById('" + FilterLayoutsListBox.ClientID + "').value.replace(rsb, '').replace(reb, '')";

				return
					rexp +
					hiddenControlValue + " = prompt('" + Res.GetString("29211DA8-8E02-4AC7-B231-F2AA8E99B4AF", "Enter a new description for this filter layout:") + "', " + selectedLayoutName + ");" +
					"if (" + hiddenControlValue + " == 'null')" +
					"{" +
					"	return false;" +
					"};";

				#endregion
			}
		}

		void RebindFilterLayoutsListBox()
		{
			if (!Globals.IsTest)
			{
				FilterLayoutsListBox.Bind(DataSource);
			}
		}

		#endregion

		#region Delete

		void DeleteButton_Click(object sender, EventArgs e)
		{
			DeleteFilterLayout(ManageLayoutsBizO.GetSelectedFilterLayout());
		}

		protected void DeleteFilterLayout(StmModuleFilter layout)
		{
			if (layout != null && CheckUserRights(layout) && CheckUserRightsForCompany(layout))
			{
				ManageLayoutsBizO.MarkFilterLayoutForDelete(layout);
				RebindFilterLayoutsListBox();
			}
		}

		#endregion

		#region Rename

		void RenameButton_Click(object sender, EventArgs e)
		{
			RenameFilterLayout(ManageLayoutsBizO.GetSelectedFilterLayout());
		}

		protected void RenameFilterLayout(StmModuleFilter layout)
		{
			if (layout != null && CheckUserRights(layout) && CheckUserRightsForCompany(layout))
			{
				ZString newName = HiddenFilterNameInput.Value;
				bool userClickedCancel = (newName == "null");

				if (!userClickedCancel)
				{
					newName = newName.SubstringSafe(0, StmModuleFilterSchema.S9_FilterName.MaxLength);
					if (newName != ManageLayoutsBizO.SelectedFilterLayoutName)
					{
						if (ManageLayoutsBizO.FilterLayoutExistsWithName(newName, layout.S9_IsPublished))
						{
							ScriptNotificationMessage(Res.GetString("a6d5edd2-575e-48f2-bfc9-b28c1600ad79", "Duplicate layout names are not allowed."));
						}
						else
						{
							ManageLayoutsBizO.MarkFilterLayoutForRename(layout, newName);
							RebindFilterLayoutsListBox();
						}
					}
				}
			}
		}

		#endregion

		#region CheckUserRights

		bool CheckUserRights(StmModuleFilter layout)
		{
			if (layout.IsPublishedForOrganisationInWeb && !((OrgContactWebUser)Page.SiteUser).CanPublishLayouts)
			{
				ScriptNotificationMessage(Res.GetString("b31c165f-1423-485f-8de3-4c93c3133472", "You do not have permissions to delete or rename Published Layout."));
				return false;
			}
			return true;
		}

		#endregion

		bool CheckUserRightsForCompany(StmModuleFilter layout)
		{
			if (layout.IsPublishedForCompanyInWeb && !((OrgContactWebUser)Page.SiteUser).CanPublishCompanyLayouts)
			{
				ScriptNotificationMessage(Res.GetString("CC697237-85E2-4139-AF81-78DB3AF3684A", "You do not have permissions to delete or rename Published Company Layout."));
				return false;
			}
			return true;
		}

		#region UpdateSelectedFilterLayoutContents

		void SelectedFilterLayoutInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateSelectedFilterLayoutContents();
		}

		void UpdateSelectedFilterLayoutContents()
		{
			FilterContentsTable.Rows.Clear();

			StmModuleFilter layout = ManageLayoutsBizO.GetSelectedFilterLayout();

			if (layout != null)
			{
				FilterStripCollection collection = FilterStripBizO.GetFilterStrips(layout);

				if (collection != null)
				{
					foreach (FilterStrip strip in collection)
					{
						if (IsMaxFilterContentItemsToListReached)
						{
							AddFilterContentRow("...");
							break;
						}

						if (!strip.IsFilterDescriptionEmpty)
						{
							AddCategoryRowFromModuleFilter(strip.CurrentModuleFilter);
							AddFilterContentRowFromFilterStrip(strip);
						}
					}
				}
			}
		}

		void AddFilterContentRowFromFilterStrip(FilterStrip strip)
		{
			AddFilterContentRow(strip.FilterDescription);
		}

		void AddFilterContentRow(ZString text)
		{
			if (text.Length > MaxCharsForFilterContentItem)
			{
				text = text.Substring(0, MaxCharsForFilterContentItem - 3) + "...";
			}

			ZTextLabel label = new ZTextLabel(text);

			HtmlTableCell cell = new HtmlTableCell();
			cell.Controls.Add(label);

			HtmlTableRow row = new HtmlTableRow();
			row.Cells.Add(cell);

			FilterContentsTable.Rows.Add(row);
		}

		void AddCategoryRowFromModuleFilter(ModuleFilter moduleFilter)
		{
			string categoryText = (moduleFilter != null) ? moduleFilter.Category.Description.ToString() : Res.GetString("02a5968d-6d01-4e48-be15-bc221d2e84da", "Other"); // "Other" if the filter name no longer exists

			if (!CategoryRowForModuleFilterExists(categoryText))
			{
				ZTextLabel label = new ZTextLabel(categoryText);
				label.Font.Bold = true;

				HtmlTableCell cell = new HtmlTableCell();
				cell.Controls.Add(label);

				HtmlTableRow row = new HtmlTableRow();
				row.Cells.Add(cell);

				FilterContentsTable.Rows.Add(row);
			}
		}

		bool CategoryRowForModuleFilterExists(string categoryText)
		{
			foreach (HtmlTableRow row in FilterContentsTable.Rows)
			{
				if (row.Cells.Count > 0 && row.Cells[0].Controls.Count > 0)
				{
					ZTextLabel label = row.Cells[0].Controls[0] as ZTextLabel;
					if (label != null && label.Text == categoryText && label.Font.Bold)
					{
						return true;
					}
				}
			}

			return false;
		}

		bool IsMaxFilterContentItemsToListReached
		{
			get { return FilterContentsTable.Rows.Count >= MaxFilterContentItemsToList; }
		}

		const int MaxFilterContentItemsToList = 9;
		const int MaxCharsForFilterContentItem = 40;

		#endregion

		#region Unload

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);

			if (ManageLayoutsBizO != null)
			{
				ManageLayoutsBizO.SelectedFilterLayoutNameInfo.ValueChanged -= new EventHandler(SelectedFilterLayoutInfo_ValueChanged);
			}
		}

		#endregion

		#region OK

		protected override void HandleOkButtonClick()
		{
			base.HandleOkButtonClick();
			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
		}

		protected override string[] OKFunctionArguments
		{
			get { return null; }
		}

		#endregion

		#region Cancel

		protected override void HandleCancelButtonClick()
		{
			ManageLayoutsBizO.ResetChanges();
			RebindFilterLayoutsListBox();
			base.HandleCancelButtonClick();
		}

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion
	}
}
