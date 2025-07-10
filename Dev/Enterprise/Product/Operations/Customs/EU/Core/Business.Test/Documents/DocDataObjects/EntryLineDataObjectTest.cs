using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
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
			(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, false),
			(UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, false),
			(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, false),
			(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, false),
		};

		[ExpectNoExceptions]
		public void TestCharges()
		{
			foreach (var charge in chargesToTest)
			{
				TestCharge(charge.chargeType, charge.addition);
			}
		}

		[ExpectNoExceptions]
		public void TestPrice()
		{
			var entryLineDataObject = new EntryLineDataObject(entryLine);
			NUnit.Framework.Assert.That(entryLineDataObject.Price, NUnit.Framework.Is.EqualTo((decimal)50.0).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalA()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalA(invoiceLine1.Charges);

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			NUnit.Framework.Assert.That(entryLineDataObject.TotalA, NUnit.Framework.Is.EqualTo(60.0m).Using(CustomComparers.TypeComparison));

			invoiceLine1.InvoiceHeader.JZ_InvoiceCurrExRate = 0.9m;
			entryLineDataObject = new EntryLineDataObject(entryLine);
			NUnit.Framework.Assert.That(entryLineDataObject.TotalA, NUnit.Framework.Is.EqualTo(66.67m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalB()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalB(invoiceLine1.Charges);

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			NUnit.Framework.Assert.That(entryLineDataObject.TotalB, NUnit.Framework.Is.EqualTo((decimal)78.00).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalC()
		{
			invoiceLine1.Charges.RemoveAndDeleteAll();
			CreateChargesForTotalC(invoiceLine1.Charges);

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			NUnit.Framework.Assert.That(entryLineDataObject.TotalC, NUnit.Framework.Is.EqualTo((decimal)10.0).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
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

		[ExpectNoExceptions]
		void TestAdditionCharge(ZString chargeType)
		{
			var entryLineDataObject = new EntryLineDataObject(entryLine);

			NUnit.Framework.Assert.That(entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)0).Using(CustomComparers.TypeComparison));

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = false;
			NUnit.Framework.Assert.That(entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)30).Using(CustomComparers.TypeComparison));

			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = false;
			NUnit.Framework.Assert.That(entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)0).Using(CustomComparers.TypeComparison));

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(entryLineDataObject.GetAdditionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)0).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		void TestDeductionCharge(ZString chargeType)
		{
			var entryLineDataObject = new EntryLineDataObject(entryLine);

			NUnit.Framework.Assert.That(entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)0).Using(CustomComparers.TypeComparison));

			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = false;
			NUnit.Framework.Assert.That(entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)30).Using(CustomComparers.TypeComparison));

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = false;
			NUnit.Framework.Assert.That(entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)0).Using(CustomComparers.TypeComparison));

			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = true;
			NUnit.Framework.Assert.That(entryLineDataObject.GetDeductionCharges(new ZString[] { chargeType }), NUnit.Framework.Is.EqualTo((ZDecimal)0).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLineNumber()
		{
			var entryLineDataObject = new EntryLineDataObject(entryLine);
			NUnit.Framework.Assert.That(entryLineDataObject.LineNumber, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison));

			entryLine.CL_LineNumber = 99;
			NUnit.Framework.Assert.That(entryLineDataObject.LineNumber, NUnit.Framework.Is.EqualTo(99).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCommissionsCharges()
		{
			const string commissionsChargesChargeType = UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge;

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			lineCharge1.J7_ChargeType = apportionedCharge1.J7_ChargeType = commissionsChargesChargeType;
			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = true;
			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(entryLineDataObject.CommissionsCharges, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), $"{nameof(lineCharge1)} and {nameof(apportionedCharge1)} are dutiable and included in ITOT ");

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = false;
			NUnit.Framework.Assert.That(entryLineDataObject.CommissionsCharges, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison), $"{nameof(lineCharge1)} and {nameof(apportionedCharge1)} are dutiable and NOT included in ITOT ");

			lineCharge1.J7_ChargeType = "XXX";
			NUnit.Framework.Assert.That(entryLineDataObject.CommissionsCharges, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison), $"{nameof(lineCharge1)} is not {commissionsChargesChargeType}");

			apportionedCharge1.J7_ChargeType = "XXX";
			NUnit.Framework.Assert.That(entryLineDataObject.CommissionsCharges, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), $"{nameof(apportionedCharge1)} is not {commissionsChargesChargeType}");
		}

		[ExpectNoExceptions]
		public void TestBrokerageCharges()
		{
			const string brokerageChargesChargeType = UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge;

			var entryLineDataObject = new EntryLineDataObject(entryLine);
			lineCharge1.J7_ChargeType = apportionedCharge1.J7_ChargeType = brokerageChargesChargeType;
			lineCharge1.J7_IsDutiable = apportionedCharge1.J7_IsDutiable = true;
			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(entryLineDataObject.BrokerageCharges, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), $"{nameof(lineCharge1)} and {nameof(apportionedCharge1)} are dutiable and included in ITOT ");

			lineCharge1.J7_IsIncludedInITOT = apportionedCharge1.J7_IsIncludedInITOT = false;
			NUnit.Framework.Assert.That(entryLineDataObject.BrokerageCharges, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison), $"{nameof(lineCharge1)} and {nameof(apportionedCharge1)} are dutiable and NOT included in ITOT ");

			lineCharge1.J7_ChargeType = "XXX";
			NUnit.Framework.Assert.That(entryLineDataObject.BrokerageCharges, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison), $"{nameof(lineCharge1)} is not {brokerageChargesChargeType}");

			apportionedCharge1.J7_ChargeType = "XXX";
			NUnit.Framework.Assert.That(entryLineDataObject.BrokerageCharges, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), $"{nameof(apportionedCharge1)} is not {brokerageChargesChargeType}");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 50;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
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

		void CreateChargesForTotalA(IJobComInvChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, 10.0, true, false);

			//This charge won't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 5, true, true);
		}

		InvoiceLineCharge AddNewCharge(IJobComInvChargeCollection<InvoiceLineCharge> chargeCollection, ZString chargeType, ZDecimal amount, bool isDutiable, bool included)
		{
			var charge = chargeCollection.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_RX_NKCurrency = "EUR";
			charge.J7_Amount = amount;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = included;
			return charge;
		}

		void CreateChargesForTotalB(IJobComInvChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 1.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 1.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 2.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 3.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 4.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 5.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 6.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, 7.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 8.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, 9.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 10.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 11.0, true, false);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 12.0, true, false);

			//These ones shouldn't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, true, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 5, false, true);
		}

		void CreateChargesForTotalC(IJobComInvChargeCollection<InvoiceLineCharge> charges)
		{
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 1.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, 2.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 3.0, false, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 4.0, false, true);

			//These ones shouldn't be counted
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 5, true, true);
			AddNewCharge(charges, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 5, true, false);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine1;
		CusEntryLine entryLine;
		InvoiceLineCharge lineCharge1;
		InvoiceLineApportionCharge apportionedCharge1;
	}
}
