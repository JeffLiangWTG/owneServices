using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	public class CusEntryHeaderChargesTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestIDocSADHLineTaxBoxSupporter()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeType = "A00";
			charge.C1_MethodOfPayment = "1";
			charge.C1_ChargeAmount = 10m;
			charge.EntryHeader.Declaration.JE_PaymentMethod = "5";

			var docSADHLineTaxBoxSupporter = (IDocSADHLineTaxBoxSupporter)charge;
			CombineAssertions(() =>
			{
				AssertEquals("Type", "A00", docSADHLineTaxBoxSupporter.Type);
				AssertEquals("AmountInDeclarationCurrency", "10.00", docSADHLineTaxBoxSupporter.AmountInDeclarationCurrency);
				AssertEquals("MethodOfPayment", "1", docSADHLineTaxBoxSupporter.MethodOfPayment);
				AssertEquals("DeclarationMethodOfPayment", "5", docSADHLineTaxBoxSupporter.DeclarationMethodOfPayment);
				AssertEquals("TaxBase", ZString.Empty, docSADHLineTaxBoxSupporter.TaxBase);
				AssertEquals("Rate", ZString.Empty, docSADHLineTaxBoxSupporter.Rate);
				AssertEquals("RateDuty", ZString.Empty, docSADHLineTaxBoxSupporter.RateDuty);
				AssertEquals("RateOverride", ZString.Empty, docSADHLineTaxBoxSupporter.RateOverride);
				AssertEquals("NationalFeeTypeCode", ZString.Empty, docSADHLineTaxBoxSupporter.NationalFeeTypeCode);
			});
		}
		public virtual void TestRoundC1_ChargeAmount()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				charge.C1_ChargeAmount = 10.5m;
				AssertEquals("10.5m", 10.5m, charge.C1_ChargeAmount);
				charge.C1_ChargeAmount = 10.51m;
				AssertEquals("10.51m", 10.51m, charge.C1_ChargeAmount);
			});
		}

		public void TestShouldResetDataOnMerging()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			AssertEquals("Should reset to zero on merging", false, charge.ShouldResetDataOnMerging);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			return entryHeader.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}
	}
}
