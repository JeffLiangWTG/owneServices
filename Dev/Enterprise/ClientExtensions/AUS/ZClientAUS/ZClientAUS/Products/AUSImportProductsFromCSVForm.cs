using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Products
{
	public partial class AUSImportProductsFromCSVForm : DataLoaderForm
	{
		public AUSImportProductsFromCSVForm(AUSImportProductBusinessObject bizObj) : base(bizObj)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		#endregion

		#region Registry Data

		public ClientAUSProductImportRegistry RegistryData
		{
			get
			{
				if (Importer != null && Supplier != null)
				{
					ZQuery registryFilter = new ZQuery(ClientAUSProductImportRegistrySchema.T6_OH_Importer, Importer.PK);
					registryFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Supplier, Supplier.PK);
					fRegistryData = Factory.LoadTop1<ClientAUSProductImportRegistry>(registryFilter);
				}
				return fRegistryData;
			}
		}

		ClientAUSProductImportRegistry fRegistryData;

		#endregion

		#region Implementation

		new AUSImportProductBusinessObject BusinessEntity
		{
			get { return (AUSImportProductBusinessObject)base.BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Product Import - Austin csv-file"; }
		}

		public override bool ConfirmLoadData()
		{
			return ValidateOrgCodesEntered();
		}

		OrgHeader Importer
		{
			get { return BusinessEntity.Importer; }
		}

		OrgHeader Supplier
		{
			get { return BusinessEntity.Supplier; }
		}

		bool ValidateOrgCodesEntered()
		{
			bool importerAndSupplierProvided = true;

			if (string.IsNullOrEmpty(ImporterFindBox.CodeBox.Text) || string.IsNullOrEmpty(SupplierFindBox.CodeBox.Text))
			{
				importerAndSupplierProvided = false;
				string orgError = "Both Importer and Supplier must be entered.";
				Globals.Message.ShowError(orgError, "Austin Product Load");
			}

			if (importerAndSupplierProvided)
			{
				if (Importer == null)
				{
					importerAndSupplierProvided = false;
					string orgError = "The Importer Code entered is not valid";
					Globals.Message.ShowError(orgError, "Austin Product Load");
				}

				if (Supplier == null)
				{
					importerAndSupplierProvided = false;
					string orgError = "The Supplier Code entered is not valid";
					Globals.Message.ShowError(orgError, "Austin Product Load");
				}

				if (Importer != null && Supplier != null)
				{
					if (RegistryData == null)
					{
						importerAndSupplierProvided = false;
						string orgError = "This Importer and Supplier combination has not been established yet to enable product import by this option." +
							System.Environment.NewLine + "Set up these details using the Setup Product Import and Export option.";
						Globals.Message.ShowError(orgError, "Austin Product Load");
					}
				}
			}

			return importerAndSupplierProvided;
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new AUSOrgSupplierPartDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			if (Importer != null && Supplier != null)
			{
				((AUSOrgSupplierPartDataLoad)dataLoader).ImportProductData(dataToLoad, false, Importer.PK, Supplier.PK);
			}
		}

#endregion

		// expose attributes for tests
		internal KProgressBar InternalProgressBar
		{
			get { return ProgressBar; }
			set { ProgressBar = value; }
		}
		internal ZTextBox InternalFileNameTextBox
		{
			get { return FileNameTextBox; }
			set { FileNameTextBox = value; }
		}
		internal ZGuidFindBox InternalImporterFindBox
		{
			get { return ImporterFindBox; }
			set { ImporterFindBox = value; }
		}
		internal ZGuidFindBox InternalSupplierFindBox
		{
			get { return SupplierFindBox; }
			set { SupplierFindBox = value; }
		}
		internal ZButton InternalStartButton
		{
			get { return StartButton; }
			set { StartButton = value; }
		}
		internal ZButton InternalCopyLogToClipboardButton
		{
			get { return CopyLogToClipboardButton; }
			set { CopyLogToClipboardButton = value; }
		}
		internal ZButton InternalCloseButton
		{
			get { return CloseButton; }
			set { CloseButton = value; }
		}
		internal KListBox InternalOutputListBox
		{
			get { return OutputListBox; }
			set { OutputListBox = value; }
		}
		internal string InternalGetLog()
		{
			return GetLog();
		}
		internal string InternalCreateLogInDataDirectory(string logData)
		{
			return CreateLogInDataDirectory(logData);
		}
	}
}
