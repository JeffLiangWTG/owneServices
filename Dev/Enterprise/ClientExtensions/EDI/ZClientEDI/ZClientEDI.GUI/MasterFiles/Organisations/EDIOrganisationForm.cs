using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI.Deduplication;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public interface IEdiOrgViewController
	{
		ILicenceViewController LicViewController { get; }
	}

	public partial class EDIOrganisationForm : ZOrganisationsForm
	{
		protected EDIOrganisationForm()
		{
		}

		public EDIOrganisationForm(EDIOrgHeader organisation, IEdiOrgViewController viewController)
			: base(organisation)
		{
			orgViewController = viewController;
			if (builderTabPage != null)
			{
				builderTabPage.LicViewController = orgViewController?.LicViewController;
			}

			var moveLicEnterpriseMenu = ZFormMenuStrategy.AddActionsMenuItem(this, "Move Licences To New Enterprise...", OnMoveLicencesToNewEnterpriseCode);
			var syncLicCompanyCountryMenu = ZFormMenuStrategy.AddActionsMenuItem(this, "Sync Licence Company Country", OnSyncLicenceCompanyCountry);

			if (organisation.LicCompany == null)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, "Create Licence", OnCreateLicenceEnterprise);
				moveLicEnterpriseMenu.Enabled = false;
			}
			else
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("376883c0-9d3a-42bd-9a33-1fb9643965cf", "Translate Text Elements"), OnTranslateTextElements);
			}

			ActionsMenuItem.Popup += (sender, e) => ModifyAbilityOfSyncLicCompanyCountryMenu(syncLicCompanyCountryMenu);

			OrganisationsTabControl.SelectedIndexChanging += new EventHandler(OrganisationsTabControl_SelectedIndexChanging);

			organisation.TakeSnapshotIfNeeded();
		}

		readonly IEdiOrgViewController orgViewController;

		protected override ContactsUserControl GetContactsUserControlForm()
		{
			return new EdiContactsUserControl();
		}

		public new EDIOrgHeader Organisation
		{
			get { return base.Organisation as EDIOrgHeader; }
		}

		protected void OnMoveLicencesToNewEnterpriseCode(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.OrgLicenceMoveLicencesToNewEnterprise.IsAllowed)
			{
				var bizOForPopup = new MoveLicencesToNewEnterpriseIDBizO(Organisation.LicEnterprise, Organisation, new MoveLicencesToNewEnterpriseIDBizOCallbacks());
				ZFormModaliser.ShowDialogAndDispose(new MoveLicencesToNewEnterpriseIDPopupForm(bizOForPopup));
#if DEBUG
				MoveLicencesToNewEnterpriseCodePopupFormWasShown = true;
#endif
			}
			else
			{
				EDISecurityCheckpoints.OrgLicenceMoveLicencesToNewEnterprise.ShowError();
			}
		}

		void OnSyncLicenceCompanyCountry(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.OrgLicenceModify.IsAllowed)
			{
				var response = Globals.Message.Show(Organisation.GetSyncLicenceCompanyCountryMessage(), "Sync Licence Company Country", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (response == DialogResult.OK)
				{
					Organisation.SyncLicenceCompanyCountry();
				}
			}
			else
			{
				EDISecurityCheckpoints.OrgLicenceModify.ShowError();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "The value's calculated from scaled value (button.Size/Location)")]
		protected void OnCreateLicenceEnterprise(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.OrgLicenceModify.IsAllowed)
			{
				var products = EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode.Value;
				var productsAsString = products.Length > 1 ? string.Join(", ", products.Take(products.Length - 1)) + " and " + products.Last() : products.FirstOrDefault();
				var message = ZString.Format("Would you like to generate a new Licence Enterprise Code?\r\nEnterprise Code is only necessary for products {0}.", productsAsString);

				using (var notification = new ZMessageBox(message, "Create New Licence", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					var buttonYes = notification.Controls.OfType<ZButton>().First(x => x.DialogResult == DialogResult.Yes);
					buttonYes.Text = "Yes - Create New";
					buttonYes.AutoSize = true;
					buttonYes.AutoSizeMode = AutoSizeMode.GrowOnly;
					buttonYes.Left -= buttonYes.Height / 2;

					var buttonNo = notification.Controls.OfType<ZButton>().First(x => x.DialogResult == DialogResult.No);
					buttonNo.Text = "No";
					buttonNo.AutoSize = true;
					buttonNo.AutoSizeMode = AutoSizeMode.GrowOnly;
					buttonNo.Left = buttonYes.Left + buttonYes.Width + buttonYes.Height / 2;

					var buttonCancel = notification.Controls.OfType<ZButton>().First(x => x.DialogResult == DialogResult.Cancel);
					buttonCancel.Left = buttonNo.Left + buttonNo.Width + buttonYes.Height / 2;

					var response = ZFormModaliser.ShowMessageBoxWithoutDispose(notification);

					if (response != DialogResult.Cancel)
					{
						if (Organisation != null && Organisation.IsInDatabase)
						{
							Organisation.CreateAndLoadLicenceForOrg();
							if (response == DialogResult.Yes)
							{
								Organisation.GenerateNewLicenceCode();
							}
							BuilderTabPage.HideCoveringLabel();
						}
					}
				}
			}
			else
			{
				EDISecurityCheckpoints.OrgLicenceModify.ShowError();
			}
		}

		void OnTranslateTextElements(object sender, EventArgs e)
		{
			var propertyInfos = new List<ZPropertyInfo>();

			var selfBilling = Organisation?.LicCompany?.SelfBilling;
			if (selfBilling != null)
			{
				propertyInfos.Add(selfBilling.L4_InvoiceCommentInfo);
			}

			var fees = Organisation?.LicCompany?.Fees.Select(x => x.L8_DescriptionInfo).ToArray();
			if (fees != null)
			{
				propertyInfos.AddRange(fees);
			}

			var source = new MultipleDataCaptionSource(propertyInfos.ToArray(), Organisation.OH_Code);
			ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(new CustomizableDataResourceStrings(source), null, DataSource);
		}

#if DEBUG
		public bool MoveLicencesToNewEnterpriseCodePopupFormWasShown;
#endif
		protected override void InitializeDetailsPageChildTabs(ZTemplateTabControl tabControl)
		{
			AddMembershipTabPage(tabControl);
			AddCompanyNameColumnInStaffAssignmentsTabPage(tabControl);
		}

		void AddCompanyNameColumnInStaffAssignmentsTabPage(ZTemplateTabControl tabControl)
		{
			var staffAssignmentsTabPage = tabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "StaffAssignmentsTabPage");
			if (staffAssignmentsTabPage != null)
			{
				staffAssignmentsTabPage.BindingOrFirstShown += (sender, args) =>
				{
					var staffAssignmentsGrid = tabControl.FindSingleOrDefault<ZGrid>("StaffAssignmentsGrid");
					if (staffAssignmentsGrid != null)
					{
						var columnStyle = new ZTextBoxColumnStyleInfo
						{
							ColumnName = "Company+GC_Name",
							IsVisible = true,
							IsReadOnly = true,
							Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
							CaptionResourceString = Res.GetData(
								"StaffAssignmentsControl|3161fe4b-f444-4ab0-b81d-a0ab7580ef2f", "Company Name")
						};
						staffAssignmentsGrid.ColumnStyles.Add(columnStyle);

						var productColumnStyle = new ZDropEditColumnStyleInfo
						{
							ColumnName = "O8_Product",
							IsVisible = true,
							IsReadOnly = false,
							Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
							CaptionResourceString = Res.GetData("6C653644-8D48-4C65-A731-232F0D551986", "Product")
						};
						staffAssignmentsGrid.ColumnStyles.Add(productColumnStyle);

						var productDescriptionStyle = new ZTextBoxColumnStyleInfo
						{
							ColumnName = "ProductDescription",
							IsVisible = true,
							IsReadOnly = true,
							Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
							CaptionResourceString = Res.GetData("47CBE437-D043-4BBF-952F-870AAE0BD782", "Product Name")
						};
						staffAssignmentsGrid.ColumnStyles.Add(productDescriptionStyle);
					}
				};
			}
		}

		void AddMembershipTabPage(ZTemplateTabControl tabControl)
		{
			var tabPage = new ZTabPage();
			tabPage.Name = "Memberships";
			tabPage.Text = "Memberships";
			tabControl.TabPages.Add(tabPage);
			tabPage.RunWhenBindingOrFirstShown(MembershipTabPage_InitializeTab);
		}

		void MembershipTabPage_InitializeTab(object sender, EventArgs e)
		{
			var tabPage = (ZTabPage)sender;
			tabPage.Controls.Add(new EdiOrgMembershipControl { Dock = DockStyle.Fill });
		}

		void ModifyAbilityOfSyncLicCompanyCountryMenu(MenuItem syncLicCompanyCountryMenu)
		{
			syncLicCompanyCountryMenu.Enabled = Organisation.ShouldSyncLicenceCompanyCountry;
		}

		protected override DuplicateAlertControlHelper GetDeduplicationHelper()
		{
			return new EDIDuplicateAlertControlHelper();
		}

		#region EDI Specific Controls

		public LicenceKeyBuilderTabPage BuilderTabPage
		{
			get
			{
				if (builderTabPage == null)
				{
					builderTabPage = new LicenceKeyBuilderTabPage(Organisation);
					builderTabPage.LicViewController = orgViewController?.LicViewController;
				}
				return builderTabPage;
			}
		}

		LicenceKeyBuilderTabPage builderTabPage;

		protected override Dictionary<ZTabPage, int> TabPagesWithOrders
		{
			get
			{
				var result = base.TabPagesWithOrders;
				result.Add(BuilderTabPage, -1);
				return result;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!BuilderTabPage.IsDisposed)
			{
				BuilderTabPage.Dispose();
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			Organisation.SetSnapshotCompanyDataChangedIfNeeded();

			var warningMsg = EDIOrgHeaderValidationHelper.ValidateENTCodeForOrganisation(Organisation);
			if (!string.IsNullOrEmpty(warningMsg) && Globals.Message.Show(warningMsg, "Warning Message - Valid ENT Code", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
			{
				return ContinueWithSave.No;
			}

			var result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				Organisation.SendMessageToEHubAndUpdateSnapshotIfNeeded();
			}

			return result;
		}

		public void DisplayErrorDialog()
		{
			ShowErrorsDialog();
		}

		public void OrganisationsTabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (OrganisationsTabControl.SelectedTab == BuilderTabPage)
			{
				if (Organisation != null && Organisation.IsInDatabase)
				{
					if (BuilderTabPage.Controls.Count == 2)
					{
						BuilderTabPage.Controls.RemoveAt(0);
					}
				}
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;

			if (ShouldDisplayGSTChangedWarning)
			{
				var confirmationResult = Globals.Message.ShowConfirmation(GSTRegisteredChanged, GSTRegisteredCaptionChanged, "To continue, type: ", "Yes", MessageBoxIcon.Question);
				if (confirmationResult != DialogResult.OK)
				{
					result = ContinueWithSave.No;
				}
			}

			var currentPriceHeader = builderTabPage?.LicenceControl?.BillingPricesControl?.GetCurrentPriceHeader();
			if (currentPriceHeader != null && currentPriceHeader.HasChanges && currentPriceHeader.HasRowWarnings && currentPriceHeader.IsCargoWiseNext && currentPriceHeader.IsNextCargoWiseNextPriceList)
			{
				var warningMessage = string.Join("\r\n", currentPriceHeader.RowWarnings.Select(x => x.Message));
				var confirmationResult = Globals.Message.Show(warningMessage, "Missing Exchange Rate(s)", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel);

				if (confirmationResult != DialogResult.OK)
				{
					result = ContinueWithSave.No;
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				result = base.ShowPreSaveDialogs();
			}

			return result;
		}

		protected const string GSTRegisteredChanged = "You have changed the GST Registration Status of this organisation, which can have dire consequences if it was not intended. Are you sure you want to do this?";
		const string GSTRegisteredCaptionChanged = "Changing GST Registration For Company";

		bool ShouldDisplayGSTChangedWarning
		{
			get
			{
				return Organisation.LicCompany != null &&
						(
							(Organisation.LicCompany.LC_IsGSTRegisteredInfo.HasChanges) ||
							(!Organisation.IsInDatabase && Organisation.LicCompany.GSTRegisteredDifferentToDefault)
						);
			}
		}

		#endregion
	}

	public sealed class EDIOrganisationTabPageType
	{
		EDIOrganisationTabPageType()
		{
		}

		public static OrganisationTabPageType Licence => new OrganisationTabPageType("LicenceKeyBuilderTabPage");

		public static OrganisationTabPageType LicenceSTL => new OrganisationTabPageType("LicenceKeyBuilderTabPage+stlTabPage");
	}
}
