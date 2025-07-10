using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	[CodeAlive("Will be used very soon")]
	public sealed partial class JobDocumentRecipientConfigurationForm : ZForm, IButtonPostTextOverride, IButtonCloseTextOverride
	{
		public JobDocumentRecipientConfigurationForm(JobDocumentRecipientConfiguration businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl1);

			HideEditMenuItems();
			HideSuggestedOrganisationsIfUnavailable();
			AddEmailRecipientsColumns(JobDocumentRecipientsGrid);
			AddEmailSubjectMacroColumn(JobDocumentRecipientsGrid);
			AddExcludeMenuItem();
		}

		internal JobDocumentRecipientConfiguration Configuration => (JobDocumentRecipientConfiguration)BusinessEntity;

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => false;

		public override string FormHeading => Res.GetString("JobDocumentRecipientConfigurationForm|a6224eb6-d680-487e-a3a2-07ecf4cebece", "Setup Job Specific Recipients - {0}", Configuration.DocumentSupportable.DocumentSupporter.BusinessObject.HumanReadableName);

		void HideEditMenuItems()
		{
			ActionsMenuItem.Visible = false;

			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileNewMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileDeleteMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileSeperator1MenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileSeperator2MenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileSeperator3MenuItemName, false);
		}

		void HideSuggestedOrganisationsIfUnavailable()
		{
			if (!Configuration.SupportsSuggestions)
			{
				MainSplitContainer.Panel1Collapsed = true;
				MainSplitContainer.Panel1.Hide();
			}
		}

		void AddEmailRecipientsColumns(ZGrid grid)
		{
			var emailToRecipientsColumnStyleInfo = new NonPersistentCopyRecipientsColumnStyleInfo<JobDocumentRecipientWrapperBase>()
			{
				CaptionResourceString = Res.GetData("JobDocumentRecipientConfigurationForm|feb08274-f9a8-4bd9-a4ef-f91408f81fb6", "Email TO"),
				ColumnName = nameof(JobDocumentRecipientWrapperBase.EmailToRecipientsAsString),
				EmailAddressPropertyName = "EmailAddress",
				GetCopyRecipients = wrapper => wrapper.EmailToRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
			};
			var carbonCopyRecipientsColumnStyleInfo = new NonPersistentCopyRecipientsColumnStyleInfo<JobDocumentRecipientWrapperBase>()
			{
				CaptionResourceString = Res.GetData("JobDocumentRecipientConfigurationForm|24229f18-1cf7-417b-8dc1-dfb15c0fd418", "Email CC"),
				ColumnName = nameof(JobDocumentRecipientWrapperBase.CarbonCopyRecipientsAsString),
				EmailAddressPropertyName = "EmailAddress",
				GetCopyRecipients = wrapper => wrapper.CarbonCopyRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
			};
			var blindCarbonCopyRecipientsColumnStyleInfo = new NonPersistentCopyRecipientsColumnStyleInfo<JobDocumentRecipientWrapperBase>()
			{
				CaptionResourceString = Res.GetData("JobDocumentRecipientConfigurationForm|c8d200a8-1635-40e3-9b89-3729a0c7fe7b", "Email BCC"),
				ColumnName = nameof(JobDocumentRecipientWrapperBase.BlindCarbonCopyRecipientsAsString),
				EmailAddressPropertyName = "EmailAddress",
				GetCopyRecipients = wrapper => wrapper.BlindCarbonCopyRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
			};
			grid.ColumnStyles.Add(emailToRecipientsColumnStyleInfo);
			grid.ColumnStyles.Add(carbonCopyRecipientsColumnStyleInfo);
			grid.ColumnStyles.Add(blindCarbonCopyRecipientsColumnStyleInfo);
		}

		void AddEmailSubjectMacroColumn(ZGrid grid)
		{
			var emailSubjectColumnInfo = new ZMacrosFindBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("88478b8b-7b64-415a-8b89-f88a718c43ea", "Email Subject"),
				ColumnName = nameof(JobDocumentRecipientWrapperBase.EmailSubjectMacro),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				IsUsedForExpressions = true,
				AllowMultipleMacroses = true,
				UsePredefinedRoots = true,
				RootTypes = new[]
				{
					typeof(OrgHeader),
					typeof(OrgContact),
					typeof(StmMenuItem),
					typeof(JobDocumentDelivery),
					ObjectFactory.GetType<Forwarding.IForwardingConsol>(),
					ObjectFactory.GetType<Forwarding.IForwardingShipment>(),
					ObjectFactory.GetType<IARInvoice>(),
					ObjectFactory.GetType<IWorkItem>(),
					ObjectFactory.GetType<IDtbBooking>(),
					GenericWrapperLoader.GetFromDataContext(Core.Constants.DataContext.GenericFreightJob).GetWrapperType()
				}
			};
			grid.ColumnStyles.Add(emailSubjectColumnInfo);

			grid.AfterBind += (s, a) =>
			{
				SetRootsForMacroColumn();
				grid.ListManager.CurrentChanged += (sender, args) => { SetRootsForMacroColumn(); };
			};

			void SetRootsForMacroColumn()
			{
				if (grid.ListManager.GetCurrent() is JobDocumentRecipientWrapperForJobDocumentDelivery wrapper)
				{
					var roots = new List<BusinessObject>();
					AddToRootsIfNotNull(roots, Configuration.DocumentSupportable.DocumentSupporter.BusinessObject);
					AddToRootsIfNotNull(roots, wrapper.JobDocumentDelivery);
					AddToRootsIfNotNull(roots, wrapper.JobDocumentDelivery.Contact);
					AddToRootsIfNotNull(roots, wrapper.JobDocumentDelivery.Contact?.ParentOrg);
					AddToRootsIfNotNull(roots, wrapper.Document);
					emailSubjectColumnInfo.Roots = roots.ToArray();
				}

				void AddToRootsIfNotNull(List<BusinessObject> roots, BusinessObject bizo)
				{
					if (bizo != null)
					{
						roots.Add(bizo);
					}
				}
			}
		}

		void AddExcludeMenuItem()
		{
			var menuItem = new ZMenuItem(ResString.GetMultilingualString("JobDocumentRecipientConfigurationForm|6a5052ab-ddbc-4463-bfa3-c8c2e2b4cfde", "Exclude"), new EventHandler(ExcludeRecord));
			OrgDocumentRecipientsGrid.ContextMenu.MenuItems.Add(0, menuItem);
			OrgDocumentRecipientsGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem("-"));
			OrgDocumentRecipientsGrid.ContextMenu.Popup += (o, e) => menuItem.Enabled = GetSelectedRecordsThatCanBeExcluded().Any();
		}

		#region Exclusion

		void ExcludeRecord(object sender, EventArgs e)
		{
			var recordsToExclude = GetSelectedRecordsThatCanBeExcluded().ToList();
			if (recordsToExclude.Count > 0)
			{
				foreach (var recordToExcluse in recordsToExclude)
				{
					Configuration.JobDocumentRecipients.AddNewExclusion(recordToExcluse.OrgDocument);
				}

				Configuration.HasChanges = true;
				SelectLastGridElement(JobDocumentRecipientsGrid);
			}
		}

		IEnumerable<JobDocumentRecipientWrapperForOrgDocument> GetSelectedRecordsThatCanBeExcluded()
		{
			var selectedRecords = GetCurrentlySelectedElements<JobDocumentRecipientWrapperForOrgDocument>(OrgDocumentRecipientsGrid);
			var currentExclusions = Configuration.JobDocumentRecipients.OfType<JobDocumentRecipientWrapperForJobDocumentExclusion>();
			return selectedRecords.Where(record => !currentExclusions.Any(exclusion => exclusion.WrappedBizoPK == record.WrappedBizoPK));
		}

		#endregion

		#region Suggestions

		void AddSuggestedOrganisationsButton_Click(object sender, EventArgs e)
		{
			var selectedSuggestions = GetCurrentlySelectedElements<SuggestedOrganisation>(SuggestedOrganisationsGrid);
			AddSelectedSuggestions(selectedSuggestions);
			SelectLastGridElement(JobDocumentRecipientsGrid);
		}

		void RecipientsGrid_DragEnter_DragOver(object sender, DragEventArgs e)
		{
			if (CanAcceptDragDropData(e.Data))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		void RecipientsGrid_DragDrop(object sender, DragEventArgs e)
		{
			if (CanAcceptDragDropData(e.Data))
			{
				var selectedSuggestions = ((GridRowsDataObject)e.Data).Elements.Select(bizo => (SuggestedOrganisation)bizo.BaseBusinessObject);
				AddSelectedSuggestions(selectedSuggestions);
				SelectLastGridElement(JobDocumentRecipientsGrid);
			}
		}

		void AddSelectedSuggestions(IEnumerable<SuggestedOrganisation> suggestions)
		{
			foreach (var suggestion in suggestions)
			{
				var recipient = Configuration.JobDocumentRecipients.AddNew();
				recipient.OrganisationPK = suggestion.OrgHeader.PK;
				recipient.RefreshBinding();
			}
			Configuration.HasChanges = true;
		}

		bool CanAcceptDragDropData(IDataObject dataObject)
		{
			if (dataObject.GetDataPresent(GridRowsDataObject.DataFormatType) && dataObject is GridRowsDataObject gridRowsDataObject)
			{
				return gridRowsDataObject.Elements.Any(bizo => bizo.BaseBusinessObject is SuggestedOrganisation);
			}

			return false;
		}

		#endregion

		#region Filtering

		void FilterFindButton_Click(object sender, System.EventArgs e)
		{
			Configuration.FilterOrgDocumentRecipients();
		}

		void FilterClearButton_Click(object sender, System.EventArgs e)
		{
			FilterDocumentFindBox.CodeBox.Text = string.Empty;
			FilterOrganisationtFindBox.CodeBox.Text = string.Empty;
			FilterDocumentGroupDropEdit.Text = string.Empty;
			Configuration.ClearOrgDocumentRecipientsFilters();
		}

		#endregion

		#region Utility methods

		IEnumerable<T> GetCurrentlySelectedElements<T>(ZGrid grid) where T : BusinessObject
		{
			var results = grid.SelectedElements.Cast<T>();
			if (!results.Any())
			{
				var selectedElement = GetCurrentlySelectedElement<T>(grid);
				if (selectedElement != null)
				{
					return new T[] { selectedElement };
				}
			}
			return results;
		}

		T GetCurrentlySelectedElement<T>(ZGrid grid) where T : BusinessObject
		{
			T result = null;
			if ((grid.ListManager != null))
			{
				result = (T)grid.ListManager.GetCurrent();
			}
			return result;
		}

		void SelectLastGridElement(ZGrid grid)
		{
			if (grid.ListManager != null && grid.ListManager.Count > 0)
			{
				var indexToSelect = grid.ListManager.Count - 1;
				grid.ListManager.Position = indexToSelect;
				grid.Select(indexToSelect);
				grid.Update();
			}
		}

		#endregion

		#region IButtonTextOverride Members

		string IButtonPostTextOverride.PostButtonText => ResString.GetMultilingualString("PostingButtonText|SaveAndClose", "Save && Close");

		string IButtonCloseTextOverride.CloseButtonText => ResString.GetMultilingualString("PostingButtonText|Close", "Close");

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
