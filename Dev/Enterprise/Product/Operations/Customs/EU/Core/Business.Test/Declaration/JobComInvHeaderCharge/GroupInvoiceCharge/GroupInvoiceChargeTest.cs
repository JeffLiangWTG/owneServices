using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	public class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestDefaultCurrency()
		{
			var invoiceHeader = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";

			var charge1 = GroupHeader.Charges.AddNew();
			charge1.J7_Amount = 1m;
			AssertEquals("EUR", charge1.J7_RX_NKCurrency);

			charge1.J7_RX_NKCurrency = "";
			var charge2 = GroupHeader.Charges.AddNew();
			charge2.J7_Amount = 1m;
			AssertEquals("EUR", charge2.J7_RX_NKCurrency);

			charge2.J7_RX_NKCurrency = "CNY";
			var charge3 = GroupHeader.Charges.AddNew();
			charge3.J7_Amount = 1m;
			AssertEquals("CNY", charge3.J7_RX_NKCurrency);
		}

		public virtual void TestDutiableVATableSTATableFlags()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var declaration = GetNewDeclarationForTest();
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				var groupInv = declaration.TopGroupInvoice;
				var charge = groupInv.Charges.AddNew();
				charge.J7_ChargeType = "123";

				Assert(!charge.J7_IsStatisticalValueApplicable);
				Assert(!charge.J7_IsDutiable);
				Assert(!charge.J7_IsGSTApplicable);

				charge.J7_IsDutiable = true;
				Assert(charge.J7_IsStatisticalValueApplicable);
				Assert(charge.J7_IsGSTApplicable);

				charge.J7_IsDutiable = false;
				charge.J7_IsGSTApplicable = false;
				charge.J7_IsStatisticalValueApplicable = false;
				charge.J7_IsStatisticalValueApplicable = true;
				Assert(charge.J7_IsGSTApplicable);

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				groupInv = declaration.TopGroupInvoice;
				charge = groupInv.Charges.AddNew();
				charge.J7_ChargeType = "123";

				Assert(!charge.J7_IsStatisticalValueApplicable);
				Assert(!charge.J7_IsDutiable);
				Assert(!charge.J7_IsGSTApplicable);

				charge.J7_IsDutiable = true;
				Assert(!charge.J7_IsStatisticalValueApplicable);
				Assert(charge.J7_IsGSTApplicable);

				charge.J7_IsDutiable = false;
				charge.J7_IsGSTApplicable = false;
				charge.J7_IsStatisticalValueApplicable = false;
				charge.J7_IsStatisticalValueApplicable = true;
				Assert(!charge.J7_IsGSTApplicable);
			}
		}

		public void TestAmountCorrection()
		{
			Assert("Will be implemented by WI's WI00831078 WI00837660 WI00838181 WI00838254 WI00838273 WI00838919 WI00838938 WI00838971 WI00839025 WI00839039 WI00839057", true);
		}

		public void TestCaptions() => CombineAssertions(() =>
		{
			InvoiceChargesTestHelper.AssertCaptions(Factory.New<GroupInvoiceCharge>());
		});

		public void TestTypeDecider()
		{
			Assert("Update BaseGroupInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		protected override BaseJobDeclaration GetNewDeclarationForTest()
		{
			var dec = base.GetNewDeclarationForTest();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			return dec;
		}

		protected override string ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys => CustomsChargeTypeList.Codes.OverseasInsurance;
	}
}
