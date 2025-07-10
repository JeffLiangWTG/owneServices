using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseFromXMLForm : BaseImportFileForm
	{
		public ImportLicenseFromXMLForm()
		{
		}

		public ImportLicenseFromXMLForm(ImportLicenseLoadingObjectParent importLicenseLoadingObjectParent)
		: base(importLicenseLoadingObjectParent)
		{
			ImportLicenseLoadingObjectParent.AddLog = SetProgressAndLog;
			ImportLicenseLoadingObjectParent.UnknownSupplierCodeFound += ImportLicenseLoading_UnknownSupplierCodeFound;
			ImportLicenseLoadingObjectParent.UnknownManufacturerCodeFound += ImportLicenseLoading_UnknownManufacturerCodeFound;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		ImportLicenseLoadingObjectParent ImportLicenseLoadingObjectParent => base.BusinessEntity as ImportLicenseLoadingObjectParent;

		public override string FormVerb => string.Empty;

		public override string FormCaption => Res.GetString("8376DA10-8CC2-4C36-9B80-A1575769AAB1", "Load Import License(s)");

		protected override bool ShowSaveProgressOnBrowsing => false;

		BusinessObjectFactory FactoryForFindOrganisation => factoryForFindOrganisation ?? (factoryForFindOrganisation = new BusinessObjectFactory());
		BusinessObjectFactory factoryForFindOrganisation;

		protected void ImportLicenseLoading_UnknownSupplierCodeFound(object sender, UnknownOrganisationCodeEventArgs e)
		{
			using (var popUp = new FindSimilarSupplierForm(NewOrganisationFinder(e)))
			{
				ZFormModaliser.ShowDialogWithoutDispose(popUp);
				e.Code = popUp.OrganisationCode;
				ImportLicenseLoadingObjectParent.SkipAllUnknownSuppliers = popUp.SkipAll;
			}
		}

		protected void ImportLicenseLoading_UnknownManufacturerCodeFound(object sender, UnknownOrganisationCodeEventArgs e)
		{
			using (var popUp = new FindSimilarManufacturerForm(NewOrganisationFinder(e)))
			{
				ZFormModaliser.ShowDialogWithoutDispose(popUp);
				e.Code = popUp.OrganisationCode;
				ImportLicenseLoadingObjectParent.SkipAllUnknownManufacturer = popUp.SkipAll;
			}
		}

		OrganisationFinder NewOrganisationFinder(UnknownOrganisationCodeEventArgs e)
		{
			var finder = new OrganisationFinder(e, FactoryForFindOrganisation);
			finder.UnknownOrg.OH_IsConsignor = true;
			finder.UnknownOrg.OH_RL_NKClosestPort = ZString.Empty;
			finder.UnknownOrg.AllowEmptyAddresses = true;
			finder.FindSimilarOrganisations();
			return finder;
		}

		#region Events On Click

		protected override string MessageForStartLoadingFile => Res.GetString("9E19A817-4791-4676-AEE5-AFDED8C9267B", "Loading XML NF-e");

		protected override void OnClickImportButton()
		{
			if (ImportLicenseLoadingObjectParent.CreateDataFromXml())
			{
				HideProgressMediator();
				Globals.Message.Show(Res.GetString("BB784E4E-6060-480A-8B0E-7D39CDED9320", "The file has been imported."));
				Close();
			}
			else
			{
				HideProgressMediator();
				Globals.Message.Show(Res.GetString("507B6917-0D2C-4C5F-BDEA-51700ACA7539", "Error importing the file. Please check the Log Details."));
			}
		}

		protected override void OnClickBrowseButton()
		{
			using (var fs = OpenFileDialog.OpenFile())
			{
				LogDetailsListBox.Items.Clear();
				ImportLicenseLoadingObjectParent.LoadAndValidateXML(FileNameTextBox.Text, fs);
			}
		}

		#endregion
	}
}
