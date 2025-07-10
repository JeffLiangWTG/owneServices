using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE871HeaderProvider))]
	public class IE871HeaderProviderTest : HeaderProviderAbstractTest<IE871HeaderProvider>
	{
		public void TestSequenceNumber()
		{
			var cusEntryNumber = CusEntryNumber.New(emcsDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			cusEntryNumber.CE_EntryLineReference = "1";

			AssertEquals(1, HeaderProvider.SequenceNumber);
		}

		public void TestSubmitterType()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("Correct value 1 from the Declaration", emcsDeclaration.JE_DeclarantType, HeaderProvider.SubmitterType);
		}

		[TestDate(2022, 6, 24, 14, 58, 14)]
		public void TestDateOfAnalysis()
		{
			AssertEquals("Returns Correct value", new DateTime(2022, 6, 24, 14, 58, 14), HeaderProvider.DateOfAnalysis);
		}

		public void TestDateAndTimeOfValidationOfExplanationOnShortage()
		{
			AssertNull(HeaderProvider.DateAndTimeOfValidationOfExplanationOnShortage);
		}

		public void TestGlobalExplanation()
		{
			AssertEquals("Correct Value from NonPersistent BO", "Reason for Shortage Explanation", HeaderProvider.GlobalExplanation.Text);
		}

		public void TestConsigneeTrader_Null()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertNull("No Consignee Trader entered", HeaderProvider.ConsigneeTrader);
		}

		public void TestConsigneeTrader_ConsigneeDeclarantType()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TEN121212").PK;
			var consigneeTrader = HeaderProvider.ConsigneeTrader;
			CombineAssertions(() =>
			{
				AssertEquals("Trader ID", "TEN121212", consigneeTrader.TraderId);
				//DeclarantType setup means we have to test caching seperately
				AssertSame("Cached", consigneeTrader, HeaderProvider.ConsigneeTrader);
			});
		}

		public void TestConsigneeTrader_ConsignorDeclarantType()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TEN121212").PK;
			AssertEquals("DeclarantType Consignor -> Consignee null", null, HeaderProvider.ConsigneeTrader);
		}

		public void TestConsignorTrader_Null()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertNull("No Consignor Trader entered", HeaderProvider.ConsignorTrader);
		}

		public void TestConsignorTrader_ConsignorDeclarantType()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TRD539").PK;
			var consignorTrader = HeaderProvider.ConsignorTrader;
			CombineAssertions(() =>
			{
				AssertEquals("TraderExciseNumber", "TRD539", consignorTrader.TraderExciseNumber);
				//DeclarantType setup means we have to test caching seperately
				AssertSame("Cached", consignorTrader, HeaderProvider.ConsignorTrader);
			});
		}

		public void TestConsignorTrader_ConsigneeDeclarantType()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TRD539").PK;
			AssertEquals("DeclarantType Consignee -> Consignor null", null, HeaderProvider.ConsignorTrader);
		}

		public void TestLine_Null()
		{
			AssertEquals("Collections doesn't return null but is empty", false, HeaderProvider.Lines.Any());
		}

		public void TestLines()
		{
			for (var i = 1; i < 6; i++)
			{
				var invoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 2m;
				invoiceLine.ZG_DeclaredValue = 1m;
				invoiceLine.Outturn.C5_OutturnResultReason = "Reason";
			}
			var lines = HeaderProvider.Lines;
			AssertEquals("5 records, contents is tested in the provider", 5, lines.Count);
		}

		public void TestLines_ObservedDifferenceZero()
		{
			var invoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.ZG_DeclaredValue = 1m;
			invoiceLine.Outturn.C5_OutturnResultReason = "NOT EMPTY";
			AssertEquals("line is not mapped when ObservedDifference is 0", 0, HeaderProvider.Lines.Count);
		}

		public void TestLines_ObservedDifferenceNegative()
		{
			var invoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.ZG_DeclaredValue = 2m;
			invoiceLine.Outturn.C5_OutturnResultReason = "NOT EMPTY";
			AssertEquals("line is mapped when ObservedDifference is negative", 1, HeaderProvider.Lines.Count);
		}

		public void TestLines_ExplanationIsEmpty()
		{
			var invoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.ZG_DeclaredValue = 1m;
			invoiceLine.Outturn.C5_OutturnResultReason = ZString.Empty;
			AssertEquals("line is not mapped when Explanation is empty", 0, HeaderProvider.Lines.Count);
		}

		protected new IIE871Header HeaderProvider => base.HeaderProvider;

		protected override IE871HeaderProvider GetHeaderProvider() => new IE871HeaderProvider(emcsDeclaration, "Reason for Shortage Explanation");

		protected override IEnumerable<Expression<Func<IE871HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.GlobalExplanation;
		}
	}
}
