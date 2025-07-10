using System.Data;
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
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using InvoiceLineApportionCharge = Enterprise.Customs.ES.Business.Declaration.InvoiceLineApportionCharge;
using InvoiceLineCharge = Enterprise.Customs.ES.Business.Declaration.InvoiceLineCharge;
using JobComInvoiceLine = Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(EntryLineDataObject))]
	class EntryLineDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = Factory.New<CusEntryLine>();
			return new EntryLineDataObject(entryLine);
		}

		readonly (ZString chargeType, bool addition)[] chargesToTest = new (ZString, bool)[]
		{
			(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, true),
			(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, true),
			(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, true),
			(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, true),
			(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, true),
			(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, true),
			(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, true),
			(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, true),
			(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, true),
			(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, true),
			(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, true),
			(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, true),
			(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, true),
			(ESCustomsChargeTypeList.Codes.InternationalFreight, true),
			(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, false),
			(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, false),
			(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, false),
		};

		public void TestCharges()
		{
			foreach (var charge in chargesToTest)
			{
				TestCharge(charge.chargeType, charge.addition);
			}
		}

		void TestCharge(ZString chargeType, bool addition = true)
		{
			lineCharge1.J7_ChargeType = apportionedCharge1.J7_ChargeType = chargeType;
			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = true;
			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = true;

			if (addition)
			{
				TestAdditionCharge(chargeType);
			}
			else
			{
				TestDeductionCharge(chargeType);
			}
		}

		void TestAdditionCharge(ZString chargeType)
		{
			var entryLineDataObject = new EntryLineDataObject(entryLine);

			AssertEquals((ZDecimal)0, entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }));

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = false;
			AssertEquals((ZDecimal)30, entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }));

			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = false;
			AssertEquals((ZDecimal)0, entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }));

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = true;
			AssertEquals((ZDecimal)0, entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }));
		}

		void TestDeductionCharge(ZString chargeType)
		{
			var entryLineDataObject = new EntryLineDataObject(entryLine);

			AssertEquals((ZDecimal)0, entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }));

			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = false;
			AssertEquals((ZDecimal)30, entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }));

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = false;
			AssertEquals((ZDecimal)0, entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }));

			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = true;
			AssertEquals((ZDecimal)0, entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }));
		}

		public void TestOtherNotElsewhereDeclaredCharges()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTOtherNotElsewhereDeclaredCharges(invoiceLine1.Charges);
			declaration.ResumeApportionment();
			Factory.Save();

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			AssertEquals((decimal)150.00, entryLineDataObject.OtherNotElsewhereDeclaredCharges);
		}

		public void TestMaterialsComponentsPartsCharges()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 10.0, true, false);
			AddNewCharge(invoiceLine1.Charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20.0, false, false);
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 30.0, false, true);

			declaration.ResumeApportionment();
			Factory.Save();

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			AssertEquals((decimal)30.00, entryLineDataObject.MaterialsComponentsPartsCharges);
		}

		public void TestTotalB()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalB(invoiceLine1.Charges);
			invoiceLine1.InvoiceHeader.JZ_InvoiceCurrExRate = 1.1222m;

			CombineAssertions(() =>
			{
				AssertEquals("COM charge is 1.13 EUR, amount is rounded to 2 decimals", 1.13m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge).J7_Amount);
				AssertEquals("CBR charge is 2.77 EUR, amount is rounded to 2 decimals", 2.77m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge).J7_Amount);
				AssertEquals("CPA charge is 3.77 EUR, amount is rounded to 2 decimals", 3.77m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge).J7_Amount);

				var entryLineDataObject = new EntryLineDataObject(entryLine);
				AssertEquals("CommissionsCharges are the sum of all COM charges, in this case only one, converted to the current currency, EUR", 1.13m, entryLineDataObject.CommissionsCharges);
				AssertEquals("TotalB should not be divided by the currency exchange rate since charges are already in the current currency", 115.19m, entryLineDataObject.TotalB);
			});
		}

		public void TestTotalB_LocalCurrencyUSD()
		{
			SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 1.1222m, ZDateTime.Today);
			declaration.SetLocalCurrencyCodeCore(Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalB(invoiceLine1.Charges);
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			invoiceLine1.InvoiceHeader.JZ_InvoiceCurrExRate = 3.1222m;

			CombineAssertions(() =>
			{
				AssertEquals("COM charge is 1.13 EUR, amount is rounded to 2 decimals", 1.13m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge).J7_Amount);
				AssertEquals("CBR charge is 2.77 EUR, amount is rounded to 2 decimals", 2.77m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge).J7_Amount);
				AssertEquals("CPA charge is 3.77 EUR, amount is rounded to 2 decimals", 3.77m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge).J7_Amount);

				var entryLineDataObject = new EntryLineDataObject(entryLine);
				AssertEquals("CommissionsCharges are the sum of all COM charges, in this case only one, converted to the current currency using JZ_InvoiceCurrExRate, USD", 3.53m, entryLineDataObject.CommissionsCharges);
				AssertEquals("TotalB should not be divided by the currency exchange rate since charges are already in the current currency", 316.72m, entryLineDataObject.TotalB);
			});
		}

		public void TestTotalC()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalC(invoiceLine1.Charges);
			invoiceLine1.InvoiceHeader.JZ_InvoiceCurrExRate = 1.1222m;

			CombineAssertions(() =>
			{
				AssertEquals("CEA charge is rounded to 1 EUR", 1m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge).J7_Amount);
				AssertEquals("ADD charge is rounded to 2.01 EUR", 2.01m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge).J7_Amount);
				AssertEquals("IDO charge is rounded to 2.01 EUR", 3.01m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge).J7_Amount);

				var entryLineDataObject = new EntryLineDataObject(entryLine);
				AssertEquals("ConstructionErectionAssemblyCharges are the sum of all CEA charges, in this case only one, converted to the current currency, EUR", 1m, entryLineDataObject.ConstructionErectionAssemblyCharges);
				AssertEquals("TotalC should not be divided by the currency exchange rate since charges are already in the current currency", 15.02m, entryLineDataObject.TotalC);
			});
		}

		public void TestTotalC_LocalCurrencyUSD()
		{
			SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 1.1222m, ZDateTime.Today);
			declaration.SetLocalCurrencyCodeCore(Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalC(invoiceLine1.Charges);
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceLine1.InvoiceHeader.JZ_InvoiceCurrExRate = 3.1222m;

			CombineAssertions(() =>
			{
				AssertEquals("CEA charge is rounded to 1 EUR", 1m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge).J7_Amount);
				AssertEquals("ADD charge is rounded to 2.01 EUR", 2.01m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge).J7_Amount);
				AssertEquals("IDO charge is rounded to 2.01 EUR", 3.01m, invoiceLine1.Charges.First<InvoiceLineCharge>(x => x.J7_ChargeType == UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge).J7_Amount);

				var entryLineDataObject = new EntryLineDataObject(entryLine);
				AssertEquals("ConstructionErectionAssemblyCharges are the sum of all CEA charges, in this case only one, converted to the current currency using JZ_InvoiceCurrExRate, USD", 3.12m, entryLineDataObject.ConstructionErectionAssemblyCharges);
				AssertEquals("TotalC should not be divided by the currency exchange rate since charges are already in the current currency", 15.02m, entryLineDataObject.TotalC);
			});
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

				var entryLineDataObject = new EntryLineDataObject(entryLine);
				AssertEquals("When charge is in current currency no conversion is done", 150.00m, entryLineDataObject.OtherNotElsewhereDeclaredCharges);

				var (declaration2, invoiceLine2, entryLine2) = GetEntryLineDataObjectForUSDCurrentCurrency();

				declaration2.SetLocalCurrencyCodeCore(Core.Constants.CurrencyCodes.UnitedStates);
				invoiceLine2.Charges.RemoveAndDeleteAll();
				CreateChargesForTOtherNotElsewhereDeclaredCharges(invoiceLine2.Charges);

				var entryLineDataObject2 = new EntryLineDataObject(entryLine2);
				AssertEquals("When charge is not in current currency a conversion is done using the database exchange rate when JZ_InvoiceCurrExRate is not declared", 142.67m, entryLineDataObject2.OtherNotElsewhereDeclaredCharges);

				invoiceLine2.InvoiceHeader.JZ_InvoiceCurrExRate = 3.1222m;

				entryLineDataObject2 = new EntryLineDataObject(entryLine2);
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

				var entryLineDataObject = new EntryLineDataObject(entryLine);
				var charge = entryLineDataObject.GetType().GetProperty(chargeToCheck)?.GetValue(entryLineDataObject);
				AssertEquals("When charge is in current currency no conversion is done", 10.00m, charge);

				var (declaration2, invoiceLine2, entryLine2) = GetEntryLineDataObjectForUSDCurrentCurrency();

				declaration2.SetLocalCurrencyCodeCore(Core.Constants.CurrencyCodes.UnitedStates);
				invoiceLine2.Charges.RemoveAndDeleteAll();
				AddNewCharge(invoiceLine2.Charges, chargeType, 10.0, isDutiable, included);

				var entryLineDataObject2 = new EntryLineDataObject(entryLine2);
				var charge2 = entryLineDataObject2.GetType().GetProperty(chargeToCheck)?.GetValue(entryLineDataObject2);
				AssertEquals("When charge is not in current currency a conversion is done using the database exchange rate when JZ_InvoiceCurrExRate is not declared", 11.22m, charge2);

				invoiceLine2.InvoiceHeader.JZ_InvoiceCurrExRate = 3.1222m;

				entryLineDataObject2 = new EntryLineDataObject(entryLine2);
				charge2 = entryLineDataObject2.GetType().GetProperty(chargeToCheck)?.GetValue(entryLineDataObject2);
				AssertEquals("When charge is not in current currency a conversion is done using JZ_InvoiceCurrExRate, not the database exchange rate", 31.22m, charge2);
			});
		}

		(JobDeclarationForTesting declaration, JobComInvoiceLine invoiceLine, CusEntryLine entryLine) GetEntryLineDataObjectForUSDCurrentCurrency()
		{
			SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 1.1222m, ZDateTime.Today);
			var declaration2 = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration2.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 50;
			var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_JE = declaration2.PK;
			var entryLine2 = entryHeader2.AllEntryLines.AddNew() as CusEntryLine;
			invoiceLine2.JI_CL = entryLine2.PK;

			return (declaration2, invoiceLine2, entryLine2);
		}

		public void TestCustomsValueDeclared()
		{
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 10.0, true, false);
			AddNewCharge(invoiceLine1.Charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20.0, false, false);
			AddNewCharge(invoiceLine1.Charges, ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 25.0, false, true);
			AddNewCharge(invoiceLine1.Charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 30.0, false, true);

			declaration.ResumeApportionment();
			Factory.Save();

			var groupedEntryLineDataObject = new EntryLineDataObject(entryLine);
			AssertEquals((decimal)50.0, groupedEntryLineDataObject.CustomsValueDeclared);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Spain;
			invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 50;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as Declaration.CusEntryHeader;
			entryHeader.CH_JE = declaration.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			invoiceLine1.Charges.RemoveAndDeleteAll();
			lineCharge1 = invoiceLine1.Charges.AddNew();
			lineCharge1.J7_Amount = 10;

			invoiceLine1.ApportionedCharges.RemoveAndDeleteAll();
			apportionedCharge1 = invoiceLine1.ApportionedCharges.AddNew();
			apportionedCharge1.J7_Amount = 20;
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

		void CreateChargesForTotalB(InvoiceLineChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 1.1267, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 2.765, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 3.774, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 4.44, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 5.44, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 6.11, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, 7.11, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 8.22, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, 9.22, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 10.33, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 11.33, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 12.55, true, false);
			AddNewCharge(charges, ESCustomsChargeTypeList.Codes.InternationalFreight, 12.55, true, false);
			AddNewCharge(charges, ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20.22, false, false);

			//These ones shouldn't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, true, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, false, true);
		}

		void CreateChargesForTotalC(InvoiceLineChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 1.001, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, 2.009, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 3.005, false, true);
			AddNewCharge(charges, ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, 4.0, false, true);

			//These ones shouldn't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, true, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, false, true);
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

		JobDeclarationForTesting declaration;
		JobComInvoiceLine invoiceLine1;
		CusEntryLine entryLine;
		InvoiceLineCharge lineCharge1;
		InvoiceLineApportionCharge apportionedCharge1;

		public class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void SetLocalCurrencyCodeCore(ZString currency) => currencyCode = currency;
			ZString currencyCode;

			protected override ZString LocalCurrencyCodeCore => currencyCode.IsEmpty ? base.LocalCurrencyCodeCore : currencyCode;
		}
	}
}
