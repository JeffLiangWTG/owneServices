using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class ImportIncoTermAndCustomsChargeFactoryTest : BaseIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestChargesCodes()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();

			void AssertChargeCode(string code, bool isDutiable, bool isDutiableDeemedForThisCharge, string distributedBy = "")
			{
				var chargeCode = charges.FirstOrDefault(x => x.Code == code);
				AssertEquals($"{code} - Import + IsDutiable", isDutiable, chargeCode.IsDutiable);
				AssertEquals($"{code} - Import + IsDutiableDeemedForThisCharge", isDutiableDeemedForThisCharge, chargeCode.IsDutiableDeemedForThisCharge);
				if (!distributedBy.IsNullOrEmpty())
				{
					AssertEquals($"{code} - Import + DistributedBy", distributedBy, chargeCode.DistributeBy);
				}
			}

			CombineAssertions(() =>
			{
				AssertChargeCode("EXW", true, true);
				AssertChargeCode("FIF", true, true);
				AssertChargeCode("OFT", true, true, ChargeDistributeByList.Codes.Weight);
				AssertChargeCode("ONS", true, true);
				AssertChargeCode("PAC", true, true);
				AssertChargeCode("LCH", false, true);
				AssertChargeCode("ADD", true, true);
				AssertChargeCode("COM", true, false);
				AssertChargeCode("DED", false, true);
				AssertChargeCode("DIS", false, false);
				AssertChargeCode("OTH", true, false);
			});
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Japan + Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
	}
}
