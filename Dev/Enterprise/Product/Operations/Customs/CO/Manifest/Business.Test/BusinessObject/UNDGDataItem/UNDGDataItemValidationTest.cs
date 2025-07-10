using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(UNDGDataItemValidation))]
	class UNDGDataItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateUNDGSubstanceManager()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertNoNotifications(pack.UNDGs.FirstItemForBinding[0].DI_DG_NKSubsInfo);

			undg.DI_DG_NKSubs = "xx04";
			AssertHasWarningContaining(pack.UNDGs.FirstItemForBinding[0].DI_DG_NKSubsInfo, "The value should be numeric");
		}

		public void TestValidateDI_IMOClass()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_IMOClass = ZString.Empty;
			AssertNoErrors(item.DI_IMOClassInfo);
		}
	}
}
