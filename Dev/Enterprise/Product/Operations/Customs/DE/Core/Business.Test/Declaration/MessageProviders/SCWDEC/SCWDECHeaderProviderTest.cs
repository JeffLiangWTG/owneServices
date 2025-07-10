using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWDECHeaderProvider))]
	sealed class SCWDECHeaderProviderTest : ImportHeaderProviderAbstractTest<SCWDECHeaderProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusEntryHeader null", () => new SCWDECHeaderProvider(null));

				AssertExceptionThrown<ArgumentException>("CusEntryHeader.Declaration null", () => new SCWDECHeaderProvider(Factory.New<CusEntryHeader>()));

				entryHeader.CH_CEI_Instruction = ZGuid.Empty;
				AssertExceptionThrown<ArgumentException>("CusEntryHeader.EntryInstruction null", () => new SCWDECHeaderProvider(entryHeader));
			});
		}

		public void TestForeignTradeImportEarlyClearanceFlag()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_EarlyClearanceFlag = "J";
				AssertEquals("CEI_EarlyClearanceFlag = 'J'", true, Provider.ForeignTradeImportEarlyClearanceFlag);

				entryInstruction.CEI_EarlyClearanceFlag = "N";
				AssertEquals("CEI_EarlyClearanceFlag = 'N'", false, Provider.ForeignTradeImportEarlyClearanceFlag);

				entryInstruction.CEI_EarlyClearanceFlag = "X";
				AssertEquals("CEI_EarlyClearanceFlag = 'X'", false, Provider.ForeignTradeImportEarlyClearanceFlag);
			});
		}

		public void TestCustomsValue_false()
		{
			declaration.ZG_IsHighValueOvrd = false;
			AssertNull(Provider.CustomsValue);
		}

		public void TestCustomsValue_true()
		{
			declaration.ZG_IsHighValueOvrd = true;
			AssertNotNull(Provider.CustomsValue);
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryHeader.MergedLines.AddNew().PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
			AssertEquals(2, Provider.Lines.Count);
		}

		public void TestProcedureAuthorisation()
		{
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			authorizationUsage.AGC_Number = "111";

			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code is CWP", "111", Provider.ProcedureAuthorisation);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				authorizationUsage.AGC_Number = "222";
				var newProvider = new SCWDECHeaderProvider(entryHeader) as ISCWDECHeader;
				AssertEquals("AGC_Code is CW1", "222", newProvider.ProcedureAuthorisation);

				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				newProvider = new SCWDECHeaderProvider(entryHeader);
				AssertEquals("AGC_Code is invalid", ZString.Empty, newProvider.ProcedureAuthorisation);
			});
		}

		protected override SCWDECHeaderProvider GetProvider() => new SCWDECHeaderProvider(entryHeader);

		new ISCWDECHeader Provider => base.Provider;
	}
}
