using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
	{
		protected override string GetOverseasFreightChargeCodeForTest()
		{
			return ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB;
		}

		protected override ICustomsChargeCode GetOverseasFreightCharge()
		{
			return ChargesProvider.InternationalFreight;
		}

		public void TestDistributeBy()
		{
			var declaration = GroupInvoiceCharge.GroupInvoice.JobDeclaration;
			declaration.JE_MessageType = "IMP";

			Assert("ZG_ApportionByWeight is what will be sent to Customs and J7_DistributeBy is editable for imports", !GroupInvoiceCharge.J7_DistributeByInfo.ReadOnly);

			declaration.ZG_ApportionByWeight = true;

			GroupInvoiceCharge.J7_ChargeType = ChargesProvider.AirFreightCode;
			AssertEquals(ChargeDistributeByList.Codes.Weight, GroupInvoiceCharge.J7_DistributeBy);

			var ons = declaration.TopGroupInvoice.Charges.AddNew();
			ons.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals(ChargeDistributeByList.Codes.Value, ons.J7_DistributeBy);

			var oft = declaration.TopGroupInvoice.Charges.AddNew();
			oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals(ChargeDistributeByList.Codes.Weight, oft.J7_DistributeBy);

			var dis = declaration.TopGroupInvoice.Charges.AddNew();
			dis.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			AssertEquals(ChargeDistributeByList.Codes.Value, dis.J7_DistributeBy);

			var vat = declaration.TopGroupInvoice.Charges.AddNew();
			vat.J7_ChargeType = ChargesProvider.VATAdjustmentCode;
			AssertEquals(ChargeDistributeByList.Codes.Weight, vat.J7_DistributeBy);

			var add = declaration.TopGroupInvoice.Charges.AddNew();
			add.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals(ChargeDistributeByList.Codes.Value, add.J7_DistributeBy);

			declaration.ZG_ApportionByWeight = false;
			foreach (GroupInvoiceCharge charge in declaration.TopGroupInvoice.Charges)
			{
				AssertEquals("Not apportion by weight any more", ChargeDistributeByList.Codes.Value, add.J7_DistributeBy);
			}

			declaration.JE_MessageType = "EXP";
			Assert("J7_DistributeBy editable for exports", !GroupInvoiceCharge.J7_DistributeByInfo.ReadOnly);
		}

		public void TestChargeDistributeByList()
		{
			AssertEquals("only two options", 2, GroupInvoiceCharge.Lookups.ChargeDistributionBy.Count);
			Assert(GroupInvoiceCharge.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Value));
			Assert(GroupInvoiceCharge.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Weight));
		}

		public void TestChargeTypeList()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("only 7 options", 7, GroupInvoiceCharge.Lookups.ChargeTypeList.Count);
			Assert(GroupInvoiceCharge.Lookups.ChargeTypeList.ContainsCode(ChargesProvider.AirFreightCode));
			Assert(GroupInvoiceCharge.Lookups.ChargeTypeList.ContainsCode(ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB));

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("only 7 options", 7, GroupInvoiceCharge.Lookups.ChargeTypeList.Count);
			Assert(GroupInvoiceCharge.Lookups.ChargeTypeList.ContainsCode(ChargesProvider.AirFreightCode));
			Assert(GroupInvoiceCharge.Lookups.ChargeTypeList.ContainsCode(ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB));
		}

		public override void TestApportionChargeWithSameChargeTypeWithDifferntKeys()
		{
			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;

			GroupInvoiceCharge.J7_ChargeType = ChargesProvider.AirFreightCode;
			GroupInvoiceCharge.J7_Amount = 100;
			GroupInvoiceCharge.J7_RX_NKCurrency = Declaration.LocalCurrencyCode;
			GroupInvoiceCharge.J7_IsDutiable = true;
			Declaration.ResumeApportionment();
			AssertEquals("PreCondition: apportioned", 1, invoice.GroupCharges.Count);

			var otherCharge = Declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200, Declaration.LocalCurrencyCode);
			otherCharge.J7_IsDutiable = false;
			Declaration.ResumeApportionment();
			AssertEquals("Two apportioned Charges", 2, invoice.GroupCharges.Count);
		}

		public override void TestChargePrepaidCollectCommittedToApportionedCharge()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var oFT = GroupHeader.Charges.AddNew();
			oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			oFT.J7_Amount = 100;
			oFT.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:OFT is Prepaid", Core.Constants.PaymentType.Prepaid, oFT.J7_PrepaidCollect);
				AssertEquals("1 apportioned Charge", 1, invoice1.GroupCharges.Count);
				AssertEquals("1 apportioned Charge", 1, invoice2.GroupCharges.Count);

				oFT.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
				TestDec.ResumeApportionment();
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice1.GroupCharges[0].J7_PrepaidCollect);
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice2.GroupCharges[0].J7_PrepaidCollect);
			});
		}

		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = GetJobDeclaration());
			}
		}
		JobDeclaration declaration;

		JobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclarationForTest()
		{
			var declaration = base.GetNewDeclarationForTest();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		GroupInvoiceCharge GroupInvoiceCharge
		{
			get
			{
				if (groupInvoiceCharge == null)
				{
					groupInvoiceCharge = Declaration.TopGroupInvoice.Charges.AddNew();
				}
				return groupInvoiceCharge;
			}
		}
		GroupInvoiceCharge groupInvoiceCharge;
	}
}
