using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(SendACDAOperationalActionMethodApplicator))]
	class SendACDAOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestApply()
		{
			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory, Env.CurrentCompanyPK, Guid.Empty))
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				var targets = new BusinessObject[] { testEntryHeader };
				var applicator = new SendACDAOperationalActionMethodApplicatorForTest(Factory);
				applicator.NotSendIfACDANumberExist = true;
				var testEntryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
				testEntryHeader.CH_CEI_Instruction = testEntryInstruction.PK;
				var testAttachment = testEntryInstruction.Attachments.AddNew();
				testAttachment.AttachmentType = CSDDocTypeList.Codes._10000001;
				testAttachment.AttachmentNumber = "12345678910111213";
				var testOperationalLog2 = new DummyOperationalActionSectionLog();
				applicator.PerformFunctionOperationalAction(testOperationalLog2, targets);
				LogControllerLink logControllerLink = new LogControllerLink(testEntryHeader.HumanReadableName,
					ControllerIDs.Customs.EntryHeader, testEntryHeader.PK);
				var expectMessage = $"WARNING: ACDA Message for [HL {logControllerLink.Text}] was not sent due to ACDA number already exists.";
				AssertEquals("log should contain one message", 1, testOperationalLog2.messages.Count);
				AssertEquals("log should contain ACDA number already exists message", expectMessage,
					testOperationalLog2.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
				applicator.NotSendIfACDANumberExist = false;
				applicator.NotSendWithMessageErrors = true;
				applicator.SendWithMessageErrors = false;
				var startDate = ZDateTime.Today.AddDays(-1);
				var endDate = ZDateTime.Today.AddDays(1);
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", true);
				helper.CreateCusMap("CNTRY", "US", "502", startDate, endDate, Core.Constants.CountryCodes.China);
				Factory.Save();
				var testOperationalLog3 = new DummyOperationalActionSectionLog();
				applicator.PerformFunctionOperationalAction(testOperationalLog3, targets);
				logControllerLink = new LogControllerLink(testEntryHeader.HumanReadableName,
					ControllerIDs.Customs.EntryHeader, testEntryHeader.PK);
				expectMessage = $"WARNING: ACDA Message for [HL {logControllerLink.Text}] was not sent due to some message errors.";
				AssertEquals("log should contain one message", 1, testOperationalLog3.messages.Count);
				AssertEquals("log should contain message not sent message", expectMessage,
					testOperationalLog3.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
				applicator.NotSendWithMessageErrors = false;
				applicator.SendWithMessageErrors = true;
				var testOperationalLog4 = new DummyOperationalActionSectionLog();
				applicator.PerformFunctionOperationalAction(testOperationalLog4, targets);
				logControllerLink = new LogControllerLink(testEntryHeader.HumanReadableName,
					ControllerIDs.Customs.EntryHeader, testEntryHeader.PK);
				expectMessage = $"INFO: ACDA Message for [HL {logControllerLink.Text}] was sent with some message errors.";
				AssertEquals("log should contain one message", 1, testOperationalLog4.messages.Count);
				AssertEquals("log should contain message was sent with errors message", expectMessage,
					testOperationalLog4.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
				testEntryHeader = SetUpValidMandatoryFields();
				targets = new BusinessObject[] { testEntryHeader };
				var testOperationalLog5 = new DummyOperationalActionSectionLog();
				applicator.PerformFunctionOperationalAction(testOperationalLog5, targets);
				logControllerLink = new LogControllerLink(testEntryHeader.HumanReadableName,
					ControllerIDs.Customs.EntryHeader, testEntryHeader.PK);
				expectMessage = $"INFO: ACDA Message for [HL {logControllerLink.Text}] was sent successfully.";
				AssertEquals("log should contain one message", 1, testOperationalLog5.messages.Count);
				AssertEquals("log should contain message was sent successfully message", expectMessage,
					testOperationalLog5.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			}
		}

		CusEntryHeader SetUpValidMandatoryFields()
		{
			var testData = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", true);
			helper.CreateCusMap("CNTRY", "US", "502", startDate, endDate, Core.Constants.CountryCodes.China);
			Factory.Save();
			var supplier = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company", "CcdCode", "UscCode", "CiqCode").Header;
			testData.JobDeclaration.JE_OH_Supplier = supplier.PK;
			var proxy = Factory.New<OrgHeader>();
			var ccdCode = proxy.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = "CCD123";
			testData.JobDeclaration.Branch.GB_OH_OrgProxy = proxy.PK;
			testData.EntryInstruction.CEI_Style = "AB";
			testData.InvoiceLine.JI_Tariff = "3005109000";
			var invoice1 = testData.InvoiceHeader;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine1 = testData.InvoiceLine;
			invoiceLine1.JI_LinePrice = 50m;
			var entryLine2 = testData.EntryHeader.MergedLines.AddNew();
			var invoice2 = testData.JobDeclaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_LinePrice = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = testData.JobDeclaration.LocalCurrencyCode;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			testData.InvoiceLine.JI_NameOfGoods = "Car";
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testData.EntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			testData.InvoiceLine.JI_CountryOfOrigin = "US";
			return testData.EntryHeader;
		}

		public void TestProperties()
		{
			var method = new SendACDAOperationalActionMethod();
			SendACDAOperationalActionMethodApplicator applicator = (SendACDAOperationalActionMethodApplicator)method.NewApplicator(Factory, null);
			AssertEquals("SendWithMessageErrors default value should be false", false, applicator.SendWithMessageErrors);
			AssertEquals("NotSendWithMessageErrors default value should be true", true, applicator.NotSendWithMessageErrors);
			AssertEquals("NotSendIfACDANumberExist default value should be false", false, applicator.NotSendIfACDANumberExist);
		}

		protected override BusinessObject GetNewBusinessObject() => new SendACDAOperationalActionMethodApplicator(Factory);

		class SendACDAOperationalActionMethodApplicatorForTest : SendACDAOperationalActionMethodApplicator
		{
			public SendACDAOperationalActionMethodApplicatorForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public void PerformFunctionOperationalAction(IOperationalActionSectionLog log, BusinessObject[] targets)
			{
				base.ApplyCore(log, targets);
			}
		}
	}
}
