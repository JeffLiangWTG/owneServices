using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class CommodityTypeProviderBaseOnlyTest : CommodityTypeProviderTest<CommodityTypeProvider>
	{
		#region Public test methods

		public void TestGoodsDescription()
		{
			var invoiceLine = invoiceLines[0];
			var transitionPeriodMaxLengthString = new ZString('A', 280);
			var postTransitionPeriodMaxLengthString = new ZString('A', 512);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				invoiceLine.JI_Description = postTransitionPeriodMaxLengthString;
				AssertEquals("TransitionPeriodAES30", transitionPeriodMaxLengthString, Provider.GoodsDescription);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				invoiceLine.JI_Description = postTransitionPeriodMaxLengthString;
				AssertEquals("Not TransitionPeriodAES30", postTransitionPeriodMaxLengthString, Provider.GoodsDescription);

				invoiceLine.JI_Description += "B";
				AssertEquals("Not TransitionPeriodAES30", postTransitionPeriodMaxLengthString, Provider.GoodsDescription);
			}
		}

		public void TestCUSCode()
		{
			invoiceLines[0].ZG_CusNumber = "CUS001";
			AssertEquals("CUSCode", Provider.CUSCode, "CUS001");
		}

		public void TestCommodityCode()
		{
			AssertType<CommodityCodeProvider>("Type of CommodityCode", Provider.CommodityCode);
		}

		public void TestDangerousGoods()
		{
			var invoiceLine = invoiceLines[0];
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "!!5%";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "!!6%";
			subs2.DG_Variant = "B";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = invoiceLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg = invoiceLine.UNDGs.AddNew();
			undg.DI_DG = subs2.PK;
			_ = invoiceLine.UNDGs.AddNew(); // ensure we ignore null
			var subs3 = Factory.New<UNDGSubstance>();
			undg = invoiceLine.UNDGs.AddNew();
			undg.DI_DG = subs3.PK; // ensure we ignore empty
								   // We don't include DG_Variant for IE
			AssertContainsExactElementsInAnyOrder("DangerousGoods", new[] { "!!5%", "!!6%" }, Provider.DangerousGoods);
		}

		#endregion

		protected override CommodityTypeProvider GetProvider() => new CommodityTypeProvider(entryLineWrapper);
	}

	abstract class CommodityTypeProviderTest<T> : DataProviderTestCase<T>
		where T : CommodityTypeProvider
	{
		#region Overridings & inherits

		protected override void SetUp()
		{
			base.SetUp();
			(entryHeaderWrapper, entryLineWrapper, invoiceLines) = MessageProviderTestHelper.SetupBasicTestBizObjsMultipleInvoiceLinesSingleEntryLine(Factory);
		}
		protected EntryHeaderWrapper entryHeaderWrapper;
		protected EntryLineWrapper entryLineWrapper;
		protected JobComInvoiceLine[] invoiceLines;

		#endregion
	}
}
