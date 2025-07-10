using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ZZDatabaseValidationHelper))]
	sealed class ZZDatabaseValidationHelperTest : TestCaseWithFactory
	{
		public void TestMandatoryFields()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (bill.SuspendValidationTesting())
			{
				var propertyInfo = bill.ABL_OA_ConsigneeInfo;
				header.ZZValidationHelper.CheckIsMandatoryFor(propertyInfo, ManifestValidationRuleCodes.Consignee);
				AssertHasMessageError("Consignee", propertyInfo, "A Consignee is required");

				propertyInfo = bill.ABL_RL_NKFinalDestinationInfo;
				header.ZZValidationHelper.CheckIsMandatoryFor(propertyInfo, ManifestValidationRuleCodes.FinalDestination);
				AssertHasMessageError("FinalDestination", propertyInfo, "A Final Destination is required");
			}
		}
	}
}
