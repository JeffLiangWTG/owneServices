using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAJobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestClonePGADetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = "IID";

			var sourceInvoice = declaration.Invoices.AddNew();
			var sourceLine = (JobComInvoiceLine)sourceInvoice.InvoiceLines.AddNew();
			sourceLine.CA_CNSCInd = "Y";
			var cnscPGA = sourceLine.CNSCPGAHeader;
			cnscPGA.CA_AllProgramInd = "Y";
			cnscPGA.CA_Category = "C01";
			var lpco1 = cnscPGA.LPCOViews.AddNew();
			lpco1.CLP_RefNo = "111";
			var compnent1 = cnscPGA.Components.AddNew();
			compnent1.CA_Name = "222";

			Factory.Save();

			var clonedLine = (JobComInvoiceLine)new CAJobComInvoiceLineDeepCloneStrategy(sourceLine, CloneType.TemplateCopy, sourceInvoice, null).Clone();

			var clonePGA = clonedLine.CNSCPGAHeader;
			AssertNotNull(clonePGA);
			AssertEquals("Y", clonePGA.CA_AllProgramInd);
			AssertEquals("C01", clonePGA.CA_Category);
			AssertEquals(1, clonePGA.LPCOViews.Count);
			AssertEquals("111", clonePGA.LPCOViews[0].CLP_RefNo);
			AssertEquals(1, clonePGA.Components.Count);
			AssertEquals("222", clonePGA.Components[0].CA_Name);
		}

		public void TestCloneDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var sourceInvoice = declaration.Invoices.AddNew();
			var sourceLine = (JobComInvoiceLine)sourceInvoice.InvoiceLines.AddNew();
			var dutyAndTax = sourceLine.DutiesAndTaxes.AddNew();
			dutyAndTax.C1_Code = "1";
			Factory.Save();

			var clonedLine =
				(JobComInvoiceLine)new CAJobComInvoiceLineDeepCloneStrategy(sourceLine, CloneType.TemplateCopy, sourceInvoice, null).Clone();
			AssertEquals(sourceLine.DutiesAndTaxes.Count, clonedLine.DutiesAndTaxes.Count);

			for (var index = 0; index < clonedLine.DutiesAndTaxes.Count; index++)
			{
				var source = sourceLine.DutiesAndTaxes[index];
				var target = clonedLine.DutiesAndTaxes[index];
				AssertEquals("C1_Code", source.C1_Code, target.C1_Code);
			}
		}

		public void TestCloneCFIARegistrationNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_OGDCFIA = true;

			var sourceInvoice = declaration.Invoices.AddNew();
			var sourceLine = (JobComInvoiceLine)sourceInvoice.InvoiceLines.AddNew();
			var cfiaRegistrationNumber = sourceLine.CFIARegistrationNumbers.AddNew();
			cfiaRegistrationNumber.CY_Code = "599";
			cfiaRegistrationNumber.CY_Data = "123456";

			Factory.Save();

			var clonedLine =
				(JobComInvoiceLine)new CAJobComInvoiceLineDeepCloneStrategy(sourceLine, CloneType.TemplateCopy, sourceInvoice, null).Clone();
			AssertEquals(sourceLine.CFIARegistrationNumbers.Count, clonedLine.CFIARegistrationNumbers.Count);

			for (var index = 0; index < clonedLine.CFIARegistrationNumbers.Count; index++)
			{
				var source = sourceLine.CFIARegistrationNumbers[index];
				var target = clonedLine.CFIARegistrationNumbers[index];
				AssertEquals("CY_Code", source.CY_Code, target.CY_Code);
				AssertEquals("CY_Data", source.CY_Data, target.CY_Data);
			}
		}

		public void TestCloneSITTCertificationNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_OGDIC = true;
			var sourceInvoice = declaration.Invoices.AddNew();
			var sourceLine = (JobComInvoiceLine)sourceInvoice.InvoiceLines.AddNew();
			var sittCertificationNumber = sourceLine.SITTCertificationNumbers.AddNew();
			sittCertificationNumber.CY_Data = "222222";
			Factory.Save();

			var clonedLine =
				(JobComInvoiceLine)new CAJobComInvoiceLineDeepCloneStrategy(sourceLine, CloneType.TemplateCopy, sourceInvoice, null).Clone();
			AssertEquals(sourceLine.SITTCertificationNumbers.Count, clonedLine.SITTCertificationNumbers.Count);

			for (var index = 0; index < clonedLine.SITTCertificationNumbers.Count; index++)
			{
				var source = sourceLine.SITTCertificationNumbers[index];
				var target = clonedLine.SITTCertificationNumbers[index];
				AssertEquals("CY_Data", source.CY_Data, target.CY_Data);
			}
		}
	}
}
