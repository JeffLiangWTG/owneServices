using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class ExportIncoTermAndCustomsChargeFactoryTest : BaseIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestChargesCodes()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();

			void AssertChargeCode(string code, bool isDutiable, bool isDutiableDeemedForThisCharge, string distributedBy = "")
			{
				var chargeCode = charges.FirstOrDefault(x => x.Code == code);
				AssertEquals($"{code} - Export + IsDutiable", isDutiable, chargeCode.IsDutiable);
				AssertEquals($"{code} - Export + IsDutiableDeemedForThisCharge", isDutiableDeemedForThisCharge, chargeCode.IsDutiableDeemedForThisCharge);
				if (!distributedBy.IsNullOrEmpty())
				{
					AssertEquals($"{code} - Export + DistributedBy", distributedBy, chargeCode.DistributeBy);
				}
			}

			CombineAssertions(() =>
			{
				AssertChargeCode("EXW", true, true);
				AssertChargeCode("FIF", true, true);
				AssertChargeCode("OFT", false, true, ChargeDistributeByList.Codes.Weight);
				AssertChargeCode("ONS", false, true);
				AssertChargeCode("PAC", true, true);
				AssertChargeCode("LCH", false, true);
				AssertChargeCode("ADD", true, true);
				AssertChargeCode("COM", true, false);
				AssertChargeCode("DED", false, true);
				AssertChargeCode("DIS", false, false);
				AssertChargeCode("OTH", true, false);
			});
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Japan + Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
	}
}
