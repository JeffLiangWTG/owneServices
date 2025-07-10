using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(IM2AdjustmentsDocLine))]
	sealed class IM2AdjustmentsDocLineTest : NonPersistentBusinessObjectTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestFirstLineMember()
		{
			var docLine = GetNewBusinessObject() as IM2AdjustmentsDocLine;
			AssertEquals("1", docLine.OriginalLineNo);
		}

		public void TestDescription()
		{
			var docLine = GetNewBusinessObject() as IM2AdjustmentsDocLine;
			AssertEquals(@"A LONG GOODS DESCRIPTION WHICH
 EXCEEDS 30 CHARACTERSBBBBBBBB", docLine.Description);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 30 CHARACTERSEEEEEEEESSSSSSSSSSMMMMMMMMMMEEEEEEEEEE4444";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0301100000";
			invoiceLine2.JI_CustomsQuantity = 20;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine2.JI_InvoiceQuantity = 10;
			invoiceLine2.JI_InvoiceUQ = "NMB";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine2.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 30 CHARACTERSBBBBBBBBAAAAAAAAOOOOOOOOOOTTTTTTTTTT55555";
			invoiceLine2.JI_LineNo = 1;
			invoiceLine2.CA_PageNumber = 1;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.InvoiceLines[0].JI_Tariff = "0301100001";
			im2.DoMerge();
			var newInvoice = im2.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(line => line.JI_B3LineNumber.EndsWith("/SL"));
			var cusEntryLine = (IClassificationLine1)newInvoice.B3EntryLine;
			var classLine = new ClassificationLine(cusEntryLine, null, null);
			return new IM2AdjustmentsDocLine().GetFirstLine(newInvoice.B3EntryLine, classLine, false);
		}

		public void TestGSTRate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var gst = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Rate = 5m;
			gst.C1_RateType = RateTypes.Codes.AdValorem;
			gst.C1_ExemptCode = "66";
			declaration.ResumeApportionment();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var cusEntryLine = (IClassificationLine1)invoiceLine.B3EntryLine;
			var classLine = new ClassificationLine(cusEntryLine, null, null);
			var emptyLine = new IM2AdjustmentsDocLine();
			var line = emptyLine.GetFirstLine(invoiceLine.B3EntryLine, classLine, false);
			AssertEquals("66", line.GSTRate);

			gst.C1_Override = true;
			declaration.DoMerge();
			line = emptyLine.GetFirstLine(invoiceLine.B3EntryLine, classLine, false);
			AssertEquals("66", line.GSTRate);

			gst.C1_ExemptCode = "";
			declaration.DoMerge();
			line = emptyLine.GetFirstLine(invoiceLine.B3EntryLine, classLine, false);
			AssertEquals("5.0", line.GSTRate);
		}
	}
}
