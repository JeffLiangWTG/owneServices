using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.GUI.eNett;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class ContainerStorageInvoiceUserControl : ZUserControl, IDataGridLayoutIdentifierRoot
	{
		public ZGroupBox HeaderGroupBox;
		public ZTextBox AH_DescTextbox;
		ZGuidFindBox CreditorGuidFindBox;
		ZTextBox PortCodeTextBox;
		ZDateEdit PickupDateDateEdit;
		ZCalcFindBox StorageChargesCalcFindBox;
		ZButton RetrieveStorageFeesButton;
		ZDropEdit ApportionmentMethodDropEdit;
		ZDropEdit zDropEdit1;
		ZGuidFindBox BankAccountGuidFindBox;
		ZTextBox ChequeOrReferenceTextBox;
		ZButton SelectOrgButton;
		ZGroupBox PaymentDetailsGroupBox;
		ZGroupBox OrganisationDetailsGroupBox;
		ZGroupBox InvoiceDetailsGroupBox;
		ZTextBox APInvoiceNumberTextBox;
		ZGuidFindBox ChargeCodeGuidFindBox;
		ZTextBox ContainerNumberTextBox;
		ZLabel zLabel1;
		public ZGrid TransactionLinesGrid;
		ZGroupBox InvoiceLinesGroupBox;

		public ContainerStorageInvoiceUserControl()
		{
			fReadOnly = false;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			InitializeAdditionalCaptions();
		}

		void InitializeAdditionalCaptions()
		{
		}

		bool fReadOnly;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory)]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				if (fReadOnly != value)
				{
					fReadOnly = value;

					this.AH_DescTextbox.ReadOnly = fReadOnly;
				}
			}
		}

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				ZForm form = FindForm() as ZForm;

				string result = string.Empty;

				if (form != null)
				{
					result = form.Name;

					if (form.BusinessEntity != null)
					{
						result += form.BusinessEntity.GetType().Name;
					}
				}

				return result;
			}
		}

		#endregion

		void SelectOrgButton_Click(object sender, EventArgs e)
		{
			using (ContainerStoragePaymentOrgSelectionForm orgSelectionForm = new ContainerStoragePaymentOrgSelectionForm(DataSource.OrgSelectionDataSource))
			{
				ZFormModaliser.ShowDialogWithoutDispose(orgSelectionForm);

				if (DataSource.OrgSelectionDataSource.SelectedOrganisation != null)
				{
					if (DataSource.OrgSelectionDataSource.SelectedOrganisation.OrgHeaderPKs.Count > 0)
					{
						DataSource.Invoice.AH_OH = DataSource.OrgSelectionDataSource.SelectedOrganisation.OrgHeaderPKs[0];
					}
					else
					{
						DataSource.Invoice.AH_OH = ZGuid.Empty;
					}

					if (!DataSource.OrgSelectionDataSource.SelectedOrganisation.TerminalCode.IsEmpty)
					{
						DataSource.PortCode = DataSource.OrgSelectionDataSource.SelectedOrganisation.TerminalCode;
					}
				}
			}
		}

		void RetrieveStorageFeesButton_Click(object sender, EventArgs e)
		{
			eNettResponseWithMessages response = DataSource.GetStorageFeeFromWebService();

			if (response.Messages.Length > 0)
			{
				Globals.Message.ShowError(new ZStringBuilder(response.Messages).ToStringWithDelimiterBetweenAppends(System.Environment.NewLine));
			}
		}

		new StorageFeeInvoicePayment DataSource
		{
			get { return (StorageFeeInvoicePayment)base.DataSource; }
		}
	}
}

