using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing.EntryLineDataObjectTest;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using InvoiceLineApportionCharge = Enterprise.Customs.ES.Business.Declaration.InvoiceLineApportionCharge;
using InvoiceLineCharge = Enterprise.Customs.ES.Business.Declaration.InvoiceLineCharge;
using JobComInvoiceLine = Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(GroupedEntryLineDataObject))]
	class GroupedEntryLineDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var entryLineCollection = new List<CusEntryLine>() { entryLine };
			return new GroupedEntryLineDataObject(entryLine, entryLineCollection);
		}

		public void TestExchangeRate()
		{
			invoiceLine1.InvoiceHeader.JZ_InvoiceCurrExRate = 50.2;
			var groupedEntryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
			AssertEquals((decimal)50.2, groupedEntryLineDataObject.ExchangeRate);
		}

		public void TestTotalA()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalA(invoiceLine1.Charges);
			CreateChargesForTotalA(invoiceLine2.Charges);
			var groupedEntryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
			AssertEquals((decimal)120.0, groupedEntryLineDataObject.TotalA);
		}

		public void TestCustomsValueDeclared()
		{
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 10.0, true, false);
			AddNewCharge(invoiceLine1.Charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20.0, false, false);
			AddNewCharge(invoiceLine1.Charges, ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 25.0, false, true);
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 30.0, false, true);

			AddNewCharge(invoiceLine2.Charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 40.0, true, false);
			AddNewCharge(invoiceLine2.Charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 50.0, false, false);
			AddNewCharge(invoiceLine2.Charges, ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 35.0, false, true);
			AddNewCharge(invoiceLine2.Charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 60.0, false, true);

			declaration.ResumeApportionment();
			Factory.Save();

			var groupedEntryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
			AssertEquals((decimal)130.0, groupedEntryLineDataObject.CustomsValueDeclared);
		}

		public void TestPrice()
		{
			var groupedEntryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
			AssertEquals((decimal)100.0, groupedEntryLineDataObject.Price);
		}

		public void TestOtherNotElsewhereDeclaredCharges()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTOtherNotElsewhereDeclaredCharges(invoiceLine1.Charges);
			CreateChargesForTOtherNotElsewhereDeclaredCharges(invoiceLine2.Charges);
			declaration.ResumeApportionment();
			Factory.Save();

			var groupedEntryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
			AssertEquals((decimal)300.00, groupedEntryLineDataObject.OtherNotElsewhereDeclaredCharges);
		}

		public void TestMaterialsComponentsPartsCharges()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 10.0, true, false);
			AddNewCharge(invoiceLine1.Charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20.0, false, false);
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 30.0, false, true);

			AddNewCharge(invoiceLine2.Charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 40.0, true, false);
			AddNewCharge(invoiceLine2.Charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 50.0, false, false);
			AddNewCharge(invoiceLine2.Charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 60.0, false, true);

			declaration.ResumeApportionment();
			Factory.Save();

			var entryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
			AssertEquals((decimal)120.00, entryLineDataObject.MaterialsComponentsPartsCharges);
		}

		public void TestConversionForRoyaltiesAndLicenseCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, true, false, nameof(EntryLineDataObject.RoyaltiesAndLicenseCharges));
		}

		public void TestConversionForContainersAndPackingCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, true, false, nameof(EntryLineDataObject.ContainersAndPackingCharges));
		}

		public void TestConversionForCommissionsCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, true, false, nameof(EntryLineDataObject.CommissionsCharges));
		}

		public void TestConversionForBrokerageCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, true, false, nameof(EntryLineDataObject.BrokerageCharges));
		}

		public void TestConversionForProceedsOfAnySubsequentResaleCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, true, false, nameof(EntryLineDataObject.ProceedsOfAnySubsequentResaleCharges));
		}

		public void TestConversionForEngineeringDevelopmentArtworkCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, true, false, nameof(EntryLineDataObject.EngineeringDevelopmentArtworkCharges));
		}

		public void TestConversionForMaterialsConsumedCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, true, false, nameof(EntryLineDataObject.MaterialsConsumedCharges));
		}

		public void TestConversionForToolsDiesMouldsCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, true, false, nameof(EntryLineDataObject.ToolsDiesMouldsCharges));
		}

		public void TestConversionForMaterialsComponentsPartsCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, true, false, nameof(EntryLineDataObject.MaterialsComponentsPartsCharges));
		}

		public void TestConversionForIndirectAndOtherPaymentsCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, true, false, nameof(EntryLineDataObject.IndirectAndOtherPaymentsCharges));
		}

		public void TestConversionForTransportCostsCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, true, false, nameof(EntryLineDataObject.TransportCostsCharges));
		}

		public void TestConversionForInsuranceCostsCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, true, false, nameof(EntryLineDataObject.InsuranceCostsCharges));
		}

		public void TestConversionForInternationalTransportCharges()
		{
			AssertConversionForCharges(ESCustomsChargeTypeList.Codes.InternationalFreight, true, false, nameof(EntryLineDataObject.InternationalTransportCharges));
		}

		public void TestConversionForConstructionErectionAssemblyCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, false, true, nameof(EntryLineDataObject.ConstructionErectionAssemblyCharges));
		}

		public void TestConversionForOtherNotElsewhereDeclaredCharges()
		{
			CombineAssertions(() =>
			{
				invoiceLine1.Charges.RemoveAndDeleteAll();
				CreateChargesForTOtherNotElsewhereDeclaredCharges(invoiceLine1.Charges);

				var entryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
				AssertEquals("When charge is in current currency no conversion is done", 150.00m, entryLineDataObject.OtherNotElsewhereDeclaredCharges);

				var (declaration2, invoiceLine2, entryHeader2) = GetEntryLineDataObjectForUSDCurrentCurrency();

				declaration2.SetLocalCurrencyCodeCore(Core.Constants.CurrencyCodes.UnitedStates);
				invoiceLine2.Charges.RemoveAndDeleteAll();
				CreateChargesForTOtherNotElsewhereDeclaredCharges(invoiceLine2.Charges);

				var entryLineDataObject2 = new GroupedEntryLineDataObject(null, entryHeader2.AllEntryLines.Cast<CusEntryLine>());
				AssertEquals("When charge is not in current currency a conversion is done using the database exchange rate when JZ_InvoiceCurrExRate is not declared", 142.67m, entryLineDataObject2.OtherNotElsewhereDeclaredCharges);

				invoiceLine2.InvoiceHeader.JZ_InvoiceCurrExRate = 3.1222m;

				entryLineDataObject2 = new GroupedEntryLineDataObject(null, entryHeader2.AllEntryLines.Cast<CusEntryLine>());
				AssertEquals("When charge is not in current currency a conversion is done using JZ_InvoiceCurrExRate, not the database exchange rate", 22.67m, entryLineDataObject2.OtherNotElsewhereDeclaredCharges);
			});
		}

		public void TestConversionForImportDutiesOrOtherCharges()
		{
			AssertConversionForCharges(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, false, true, nameof(EntryLineDataObject.ImportDutiesOrOtherCharges));
		}

		public void TestConversionForCostOfTransportOTEU()
		{
			AssertConversionForCharges(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, false, true, nameof(EntryLineDataObject.CostOfTransportOTEU));
		}

		void AssertConversionForCharges(ZString chargeType, bool isDutiable, bool included, ZString chargeToCheck)
		{
			CombineAssertions(() =>
			{
				invoiceLine1.Charges.RemoveAndDeleteAll();
				AddNewCharge(invoiceLine1.Charges, chargeType, 10.0, isDutiable, included);

				var entryLineDataObject = new GroupedEntryLineDataObject(null, entryHeader.AllEntryLines.Cast<CusEntryLine>());
				var charge = entryLineDataObject.GetType().GetProperty(chargeToCheck)?.GetValue(entryLineDataObject);
				AssertEquals("When charge is in current currency no conversion is done", 10.00m, charge);

				var (declaration2, invoiceLine2, entryHeader2) = GetEntryLineDataObjectForUSDCurrentCurrency();

				declaration2.SetLocalCurrencyCodeCore(Core.Constants.CurrencyCodes.UnitedStates);
				invoiceLine2.Charges.RemoveAndDeleteAll();
				AddNewCharge(invoiceLine2.Charges, chargeType, 10.0, isDutiable, included);

				var entryLineDataObject2 = new GroupedEntryLineDataObject(null, entryHeader2.AllEntryLines.Cast<CusEntryLine>());
				var charge2 = entryLineDataObject2.GetType().GetProperty(chargeToCheck)?.GetValue(entryLineDataObject2);
				AssertEquals("When charge is not in current currency a conversion is done using the database exchange rate when JZ_InvoiceCurrExRate is not declared", 11.22m, charge2);

				invoiceLine2.InvoiceHeader.JZ_InvoiceCurrExRate = 3.1222m;

				entryLineDataObject2 = new GroupedEntryLineDataObject(null, entryHeader2.AllEntryLines.Cast<CusEntryLine>());
				charge2 = entryLineDataObject2.GetType().GetProperty(chargeToCheck)?.GetValue(entryLineDataObject2);
				AssertEquals("When charge is not in current currency a conversion is done using JZ_InvoiceCurrExRate, not the database exchange rate", 31.22m, charge2);
			});
		}

		(JobDeclarationForTesting declaration, JobComInvoiceLine invoiceLine, CusEntryHeader entryHeader) GetEntryLineDataObjectForUSDCurrentCurrency()
		{
			SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 1.1222m, ZDateTime.Today);
			var declaration2 = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration2.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 50;
			var entryHeader2 = (CusEntryHeader)declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_JE = declaration2.PK;
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			return (declaration2, invoiceLine2, entryHeader2);
		}

		void SetExchangeRate(string currency, ZDecimal rate, ZDateTime effectiveDate)
		{
			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, refCurrency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, "CUS");

			var exchangeRate = Factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = Factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = refCurrency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = "CUS";
			}
			exchangeRate.RE_SellRate = rate;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Spain;
			invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 50;
			invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 50;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.CL_CustomsValue = 40;
			entryLine2 = entryHeader.AllEntryLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_CustomsValue = 40;

			invoiceLine1.Charges.RemoveAndDeleteAll();
			lineCharge1 = invoiceLine1.Charges.AddNew();
			lineCharge1.J7_Amount = 10;

			invoiceLine1.ApportionedCharges.RemoveAndDeleteAll();
			apportionedCharge1 = invoiceLine1.ApportionedCharges.AddNew();
			apportionedCharge1.J7_Amount = 10;

			invoiceLine2.Charges.RemoveAndDeleteAll();
			lineCharge2 = invoiceLine2.Charges.AddNew();
			lineCharge2.J7_Amount = 10;
		}

		protected void CreateChargesForTotalA(InvoiceLineChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, 10.0, true, false);

			//This charge won't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 5, true, true);
		}
		void CreateChargesForTOtherNotElsewhereDeclaredCharges(InvoiceLineChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, 10.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 20.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 30.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, 40.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 50.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 60.0, false, true);

			//These ones shouldn't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, true, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, false, false);
		}

		protected InvoiceLineCharge AddNewCharge(InvoiceLineChargeCollection<InvoiceLineCharge> chargeCollection, ZString chargeType, ZDecimal amount, bool isDutiable, bool included)
		{
			var charge = chargeCollection.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
			charge.J7_Amount = amount;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = included;
			return charge;
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine1;
		CusEntryLine entryLine2;
		InvoiceLineCharge lineCharge1;
		InvoiceLineCharge lineCharge2;
		InvoiceLineApportionCharge apportionedCharge1;
	}
}
