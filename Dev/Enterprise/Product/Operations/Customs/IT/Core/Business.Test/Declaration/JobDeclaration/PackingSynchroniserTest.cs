using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PackingSynchroniserTest : TestCaseWithFactory
{
	public void TestSynchronisePackageMarksAndNumbersWithMaxLength()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_HouseBill = "HOUSE123";
		var packLineExceedsMaxLegth = shipment.OuterPackLines.AddNew();
		packLineExceedsMaxLegth.JL_F3_NKPackType = "BX";
		packLineExceedsMaxLegth.JL_MarksAndNumbers = "STRING THAT EXCEEDS THE MAXIMUM LENGTH FOR MARKS";

		var packLineDoesNotExceed = shipment.OuterPackLines.AddNew();
		packLineDoesNotExceed.JL_F3_NKPackType = "CN";
		packLineDoesNotExceed.JL_MarksAndNumbers = "MARKS AND NO";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;
		declaration.ShipmentSynchroniser.Synchronise(force: true);

		var packages = declaration.Packages.Cast<BasePackage>();
		AssertEquals("[PRE-CONDITION] Declaration Packages count", 2, declaration.Packages.Count);

		var exceedingPack = packages.Single(x => x.CW_PackType == "BX");
		AssertEquals("Marks and No.", "STRING THAT EXCEEDS THE MAXIMUM LENGTH FOR", exceedingPack.CW_MarksAndNos);
		AssertEquals("Marks and No. Actual Length", CustomsFieldMaxLength.EntryLine.MarksAndNumbers, exceedingPack.CW_MarksAndNos.Length);

		var notExceedingPack = packages.Single(x => x.CW_PackType == "CN");
		AssertEquals("Marks and No.", "MARKS AND NO", notExceedingPack.CW_MarksAndNos);
	}
}
