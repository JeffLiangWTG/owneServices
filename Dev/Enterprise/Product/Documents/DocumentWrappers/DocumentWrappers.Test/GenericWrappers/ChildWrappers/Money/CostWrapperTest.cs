using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CostWrapper))]
	sealed class CostWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CostWrapper wrapper = new CostWrapper(Factory.New<JobConsolCost>(), Factory);

			CombineAssertions(delegate
			{
				AssertEquals("Charge Code", ZString.Empty, wrapper.ChargeCode.Code);
				AssertEquals("Currency", ZString.Empty, wrapper.OSCost.Currency.Code);
				AssertEquals("OSCostAmount", ZDecimal.Zero, wrapper.OSCost.Amount);
				AssertEquals("LocalCostAmount", ZDecimal.Zero, wrapper.LocalCost.Amount);
				AssertEquals("Creditor", ZString.Empty, wrapper.Creditor.CompanyCode);
				AssertEquals("GSTAmount", 0m, wrapper.GSTAmount.Amount);
			});
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
ChargeCode : FRT - International Freight
Creditor : BOB'S FREIGHT COMPANY\nAUSTRALIA
GSTAmount : 10.00 AUD
LocalCost : 730.50 USD
OSCost : 500.00 AUD
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			JobConsolCost consolCost = Factory.New<JobConsolCost>();
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			OrgHeader creditor = Factory.New<OrgHeader>();

			chargeCode.AC_Code = "FRT";
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_OSCostAmount = 500m;
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 10m;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "USD";
			consolCost.E6_LocalCostAmount = 730.50m;
			creditor.OH_FullName = "BOB'S FREIGHT COMPANY";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";
			consolCost.E6_OH_Creditor = creditor.PK;

			return new CostWrapper(consolCost, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Cost
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChargeCode                              CodeAndDescription
GSTAmount                               Money
LocalCost                               Money
OSCost                                  Money
Creditor                                Organisation
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return GetSetupWrapperForDefaultFormatting();
		}
	}
}
