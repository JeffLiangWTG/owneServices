using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[TestedType(typeof(NonPersistentFee))]
	sealed class NonPersistentFeeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var newFee = GetFee("A00", 50m, "A");
			entryLine.Fees.Add(newFee);
			var nonPersistentFee = new NonPersistentFee(newFee, Factory);

			CombineAssertions("NonPersistentFee", () =>
			{
				AssertEquals("Type", newFee.CF_ChargeType, nonPersistentFee.Type);
				AssertEquals("DestinationStateIsCanaryIsland", ((JobDeclaration)newFee.EntryLine?.Declaration)?.DestinationStateIsCanaryIsland, nonPersistentFee.DestinationStateIsCanaryIsland);
				AssertEquals("EntryLineMethodOfPayment", newFee.EntryLine.RandomLine.ZG_MethodOfPayment, nonPersistentFee.EntryLineMethodOfPayment);
				AssertEquals("EntryLineMethodOfPayment2", ((CusEntryLine)newFee.EntryLine).RandomLine.ZG_MethodOfPayment2, nonPersistentFee.EntryLineMethodOfPayment2);
				AssertEquals("TaxBase", ZString.Empty, nonPersistentFee.TaxBase);
				AssertEquals("Rate", ZString.Empty, nonPersistentFee.Rate);
				AssertEquals("RateDuty", ZString.Empty, nonPersistentFee.RateDuty);
				AssertEquals("RateOverride", ZString.Empty, nonPersistentFee.RateOverride);
				AssertEquals("AmountInDeclarationCurrency", newFee.CF_ChargeAmount.ToString("N2"), nonPersistentFee.AmountInDeclarationCurrency);
				AssertEquals("MethodOfPayment", newFee.CF_MethodOfPayment, nonPersistentFee.MethodOfPayment);
				AssertEquals("NationalFeeTypeCode", ZString.Empty, nonPersistentFee.NationalFeeTypeCode);
				AssertEquals("DeclarationMethodOfPayment", ZString.Empty, nonPersistentFee.DeclarationMethodOfPayment);
			});
		}

		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null Fee", () => new NonPersistentFee(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("Null Factory", () => new NonPersistentFee(Factory.New<CusEntryLineFee>(), null));
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fee = GetFee("A00", 50m, "A");
			entryLine.Fees.Add(fee);
			return new NonPersistentFee(fee, Factory);
		}

		CusEntryLineFee GetFee(ZString type, ZDecimal amount, ZString methodOfPay)
		{
			var fee = Factory.New<CusEntryLineFee>();

			fee.CF_ChargeType = type;
			fee.CF_ChargeAmount = amount;
			fee.CF_MethodOfPayment = methodOfPay;
			return fee;
		}

		void CanaryIslandSetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain + "C", parent: grouping);
			helper.CreateCusCodeList("ESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "64", "Test 61", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		protected override void SetUp()
		{
			CanaryIslandSetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.ZG_DestinationState = Enterprise.Customs.ES.Business.CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.ZG_MethodOfPayment = "A";
			invoiceLine.ZG_MethodOfPayment2 = "R";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
		}

		JobDeclaration declaration;
		CusEntryLine entryLine;
	}
}
