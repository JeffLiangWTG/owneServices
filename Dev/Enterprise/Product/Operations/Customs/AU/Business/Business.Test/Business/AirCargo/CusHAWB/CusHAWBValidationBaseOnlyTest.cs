using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBValidationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestValidateCS_FreightPrepaidCollectCore_Mandatory()
		{
			const string mandatoryMessage = "Prepaid or Collect is required for air cargo messaging.";
			CombineAssertions(() =>
			{
				cusHawbValidation.ValidateCS_FreightPrepaidCollect();
				AssertNoMessageError("There should be no mandatory error as IsFreightPrepaidCollectRequired = false", cusHawb.CS_FreightPrepaidCollectInfo, "Prepaid or Collect is required for air cargo messaging.");

				cusHawbValidation.IsFreightPrepaidCollectRequiredExposed = true;
				cusHawbValidation.ValidateCS_FreightPrepaidCollect();
				AssertHasMessageError("Mandatory", cusHawb.CS_FreightPrepaidCollectInfo, mandatoryMessage);

				cusHawb.CS_FreightPrepaidCollect = "B";
				AssertNoMessageError("Entered", cusHawb.CS_FreightPrepaidCollectInfo, mandatoryMessage);
			});
		}

		public void TestValidateCS_FreightPrepaidCollectCore_List()
		{
			CombineAssertions(() =>
			{
				cusHawb.CS_FreightPrepaidCollect = "B";
				AssertHasMessageError("Invalid", cusHawb.CS_FreightPrepaidCollectInfo, ListValidation.InvalidCodeMessageError);

				cusHawb.CS_FreightPrepaidCollect = "A";
				AssertNoMessageError("Valid", cusHawb.CS_FreightPrepaidCollectInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusHawb = Factory.New<CusHAWB>();
			cusHawbValidation = new CusHAWBValidationForTest(cusHawb);
		}
		CusHAWB cusHawb;
		CusHAWBValidationForTest cusHawbValidation;

		sealed class CusHAWBValidationForTest : CusHAWBValidation
		{
			public CusHAWBValidationForTest(CusHAWBBase parent)
				: base(parent)
			{
			}

			public bool IsFreightPrepaidCollectRequiredExposed { get; set; }

			protected override bool IsFreightPrepaidCollectRequired => IsFreightPrepaidCollectRequiredExposed;
		}
	}
}
