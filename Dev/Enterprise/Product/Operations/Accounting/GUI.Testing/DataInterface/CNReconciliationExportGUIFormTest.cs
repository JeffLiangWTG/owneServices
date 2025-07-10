using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.DataInterface.Testing
{
	[TestedType(typeof(CNReconciliationExportGUI))]
	public class CNReconciliationExportGUIFormTest : ZFormBasherTest
	{
		public void TestDataExportWizardFormUseUTF8Encoding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				PrepareTestEnvironment();

				var wrapper = new ChinaReconciliationExportWrapper();
				wrapper.PostDateFrom = new ZDate(2016, 1, 1);
				wrapper.PostDateTo = new ZDate(2016, 2, 1);
				wrapper.ComplianceSubType = "ALL";
				wrapper.ExportStatus = "BTH";

				using (var form = new CNReconciliationExportGUI(wrapper))
				{
					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					form.NextButton_Click(null, new EventArgs());
					AssertEquals(typeof(DataExportWizardForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals(Encoding.UTF8, ((ExportWizard)ZFormModaliser.LastIBusinessShownOnDialogForTest).FileExportEncoding);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CNReconciliationExportGUI(new ChinaReconciliationExportWrapper(Factory));
		}

		void PrepareTestEnvironment()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			ARInvoice testArInvoice;
			APInvoice testApInvoice;
			AccGLAccountDescriptor arControlLocal;
			AccGLAccountDescriptor apControlLocal;
			AccGLHeader apControl;
			AccGLHeader arControl;

			testObjectCreator.CreateTestPeriodsForEntireYear(2016);

			apControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			apControl.AG_AccountNum = "10010000";
			arControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			arControl.AG_AccountNum = "10100000";

			arControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			arControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			arControlLocal.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			arControlLocal.ParentGLHeaderPK = arControl.PK;
			arControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
			arControlLocal.AJ_AccountDescription = "ARControlDescription";

			apControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			apControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			apControlLocal.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			apControlLocal.ParentGLHeaderPK = apControl.PK;
			apControlLocal.AJ_LocalAccountNumber = "APControlAccount";
			apControlLocal.AJ_AccountDescription = "APControlDescription";

			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControl.PK.ToGuid());

			testArInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testArInvoice.AH_TransactionNum = "100111";
			testArInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			testArInvoice.AH_OH = testObjectCreator.AALSHI.PK;
			testArInvoice.AH_PostDate = new ZDateTime(2016, 1, 13);
			testArInvoice.AH_DueDate = new ZDateTime(2016, 1, 18);
			testArInvoice.AH_TransactionReference = "Invoice No";
			testArInvoice.AH_ExchangeRate = 1m;
			testArInvoice.AH_InvoiceAmount = 111m;
			testArInvoice.AH_OutstandingAmount = 111m;
			testArInvoice.AH_Desc = "Invoice Desc";
			testArInvoice.AH_ConsolidatedInvoiceRef = "AR1001";

			testApInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testApInvoice.AH_TransactionNum = "100222";
			testApInvoice.AH_OH = testObjectCreator.AALSHI.PK;
			testApInvoice.AH_PostDate = new ZDateTime(2016, 1, 13);
			testApInvoice.AH_DueDate = new ZDateTime(2016, 1, 18);
			testApInvoice.AH_TransactionReference = "Invoice No";
			testApInvoice.AH_ExchangeRate = 1m;
			testApInvoice.AH_InvoiceAmount = 111m;
			testApInvoice.AH_OutstandingAmount = 111m;
			testApInvoice.AH_Desc = "Invoice Desc";
			testApInvoice.AH_ComplianceSubType = "TXA";
			testApInvoice.AH_ConsolidatedInvoiceRef = "AP1000";

			Factory.Save();

			testArInvoice.AH_ConsolidatedInvoiceRef = "AR1001";
			testApInvoice.AH_ConsolidatedInvoiceRef = "AP1000";

			Factory.Save();
		}

		#endregion
	}
}
