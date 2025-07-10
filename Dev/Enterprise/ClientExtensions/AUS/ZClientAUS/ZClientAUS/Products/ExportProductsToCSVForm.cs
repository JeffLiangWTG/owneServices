using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Products
{
	public partial class ExportProductsToCSVForm : KForm
	{
		public ExportProductsToCSVForm()
		{
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
				if (fRegistryData == null)
				{
					if (Importer != null && Supplier != null)
					{
						ZQuery registryFilter = new ZQuery(ClientAUSProductImportRegistrySchema.T6_OH_Importer, Importer.PK);
						registryFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Supplier, Supplier.PK);
						fRegistryData = (ClientAUSProductImportRegistry)Factory.LoadTop1(typeof(ClientAUSProductImportRegistry), registryFilter);
					}
				}

				return fRegistryData;
			}
		}

		ClientAUSProductImportRegistry fRegistryData;

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void OKBoundButton_Click(object sender, EventArgs e)
		{
			if (ValidImporterAndSupplier())
			{
				try
				{
					ZString updateEmailAddress = ConfirmEmailAddress();
					ZFormModaliser.Show(ProgressForm, this);
					Application.DoEvents();
					Exporter = new ExportProducts(Importer.PK, Supplier.PK, IncludeManuallyAddedCheckBox.Checked, CloseTemporaryRecordsCheckBox.Checked, updateEmailAddress);
					Exporter.Export(new ProcessedEventHandler(SetProgressBar));
				}
				finally
				{
					ProgressForm.Close();
					Close();
				}
			}
		}

		ProgressForm fProgressForm;
		protected ProgressForm ProgressForm
		{
			get
			{
				if (fProgressForm == null)
				{
					fProgressForm = new ProgressForm();
					fProgressForm.ShowProgressBar = true;
					fProgressForm.ShowCancelButton = true;
					fProgressForm.Status = "Exporting Products for Austin csv";
					fProgressForm.Cancelled += new EventHandler(fProgressForm_Cancelled);
				}
				return fProgressForm;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void SetProgressBar(object sender, ProcessedEventArgs arg)
		{
			ProgressForm.SetStatusAndPercentComplete(arg.LogEntry, arg.PercentageComplete);
			ProgressForm.Text = arg.LogEntry;
			Application.DoEvents();
		}

		protected virtual void fProgressForm_Cancelled(object sender, EventArgs e)
		{
			Exporter.CancelExport();
		}

		void CancelBoundButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		OrgHeader Importer
		{
			get { return Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, ImporterFindBox.CodeBox.Text); }
		}

		OrgHeader Supplier
		{
			get { return Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, SupplierFindBox.CodeBox.Text); }
		}

		bool ValidImporterAndSupplier()
		{
			bool importerAndSupplierProvided = true;

			if (string.IsNullOrEmpty(ImporterFindBox.CodeBox.Text) || string.IsNullOrEmpty(SupplierFindBox.CodeBox.Text))
			{
				importerAndSupplierProvided = false;
				string orgError = "Both Importer and Supplier must be entered.";
				Globals.Message.ShowError(orgError, "Austin Export Products");
			}

			if (importerAndSupplierProvided)
			{
				if (Importer == null)
				{
					importerAndSupplierProvided = false;
					string orgError = "The Importer Code entered is not valid";
					Globals.Message.ShowError(orgError, "Austin Export Products");
				}

				if (Supplier == null)
				{
					importerAndSupplierProvided = false;
					string orgError = "The Supplier Code entered is not valid";
					Globals.Message.ShowError(orgError, "Austin Export Products");
				}

				if (Importer != null && Supplier != null)
				{
					if (RegistryData == null)
					{
						importerAndSupplierProvided = false;
						string orgError = "This Importer and Supplier combination has not been established yet to enable use via Austin csv options." +
							System.Environment.NewLine + "Set up these details using the Setup Product Import and Export option.";
						Globals.Message.ShowError(orgError, "Austin Export Products");
					}
					else
					{
						ClientAUSProductInterfaceMediator interfaceMediator = new ClientAUSProductInterfaceMediator(Importer.PK, Supplier.PK);
						if (!interfaceMediator.CreateExportFile())
						{
							importerAndSupplierProvided = false;
							string orgError = "This Importer and Supplier combination does not meet the required criteria to create export files." +
								System.Environment.NewLine + "No records currently exist in the Imported Products Interface Table.";
							Globals.Message.ShowError(orgError, "Austin Export Products");
						}
					}
				}
			}

			return importerAndSupplierProvided;
		}

		ZString ConfirmEmailAddress()
		{
			return Globals.Message.QueryDefaultValue(RegistryData.UpdateEmailAddress, "Please confirm the email address to send this update file to:", "Austin Products Interface", 5);
		}

		ExportProducts Exporter;

#endregion
	}
}
