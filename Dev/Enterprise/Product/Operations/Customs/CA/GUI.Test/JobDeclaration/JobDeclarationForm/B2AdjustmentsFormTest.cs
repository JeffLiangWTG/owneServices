using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class B2AdjustmentsFormTest : JobDeclarationFormTest
	{
		public new void TestPlugins()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Business.JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.B2Adjustments;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var result = base.GetPopulatedDeclarationForFormBashingCore();
			result.CA_B2Explanation = "ABC";
			return result;
		}

		protected override BaseJobComInvoiceHeader CreateInvoiceHeaderForPerformanceTest(BaseJobDeclaration testDec)
		{
			var invoiceHeader = (JobComInvoiceHeader)base.CreateInvoiceHeaderForPerformanceTest(testDec);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoiceHeader.CA_TreatmentCode = "03";
			return invoiceHeader;
		}

		protected override BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader baseInvoiceHeader, int index, int invoiceIndex)
		{
			var classNumber = ClassificationNumbers[index % ClassificationNumbers.Length];
			var invoiceHeader = (JobComInvoiceHeader)baseInvoiceHeader;
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoiceHeader.AsAccountForFilteredInvoiceLines.AddNew();
			invoiceLine.CA_OriginalLineNo = "1";
			invoiceLine.JI_Tariff = classNumber;
			invoiceLine.CA_CVforCurrConv = 35;
			var claimLine = invoiceLine.CorrespondingAsClaimedForInvoiceLine;
			claimLine.JI_Tariff = classNumber;
			claimLine.CA_99TariffCode = TariffCodes[index % TariffCodes.Length];
			claimLine.CA_CVforCurrConv = 30;
			return invoiceLine;
		}

		protected override int NumberOfInvoiceLinesForPerformanceTest => base.NumberOfInvoiceLinesForPerformanceTest / 2;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
			Factory.Save();
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		IDisposable asecSetup;

		string[] ClassificationNumbers => classificationNumbers ?? (classificationNumbers = new[] { "8464200090", "5210410000", "6204330020", "0303630010", "5212259000", "3215190020" });
		string[] classificationNumbers;

		string[] TariffCodes => tariffCodes ?? (tariffCodes = new[] { "9915", "9938", "9932", "9952", "9990", "9988" });
		string[] tariffCodes;
	}
}
