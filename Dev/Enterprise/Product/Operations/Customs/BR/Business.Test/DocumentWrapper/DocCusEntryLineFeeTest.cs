using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocCusEntryLineFee))]
	class DocCusEntryLineFeeTest : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			var feeWrapper = GetDocumentWrappers()[0] as DocCusEntryLineFee;
			CombineAssertions(() =>
			{
				AssertEquals("ChargeType", "IPI", feeWrapper.ChargeType);
				AssertEquals("ChargeAmount", 100m, feeWrapper.ChargeAmount);
				AssertEquals("ChargeAmount", 200m, feeWrapper.BaseValue);
				AssertEquals("ChargeAmount", 0.5m, feeWrapper.Rate);
				AssertEquals("MethodOfCalculation", "QPU", feeWrapper.MethodOfCalculation);
			});
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCusEntryLineFee.New(fee, Factory) };
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Brazil; }
		}

		protected override void SetUp()
		{
			if (fee == null)
			{
				fee = Factory.New<CusEntryLineFee>();
				fee.CF_ChargeType = "IPI";
				fee.CF_ChargeAmount = 100m;
				fee.CF_BaseValue = 200m;
				fee.CF_Rate = 0.5m;
				fee.CF_MethodOfCalculation = "QPU";
			}
			base.SetUp();
		}

		CusEntryLineFee fee;

		#endregion
	}
}
