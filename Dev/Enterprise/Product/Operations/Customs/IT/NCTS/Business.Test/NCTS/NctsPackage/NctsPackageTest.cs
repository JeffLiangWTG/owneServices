using Enterprise.Customs.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsPackage))]
sealed class NctsPackageTest : CusInvPackTest<NctsDepartureCargoDesc>
{
	public void TestPhase4ValidationType()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsPackagePhase4Validation>(package.Validation);
	}

	public void TestPhase5ValidationType()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<EU.NCTS.Business.NctsPackagePhase5Validation>(package.Validation);
	}

	public void TestHumanReadableName()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(package.HumanReadableName), "Package", package.HumanReadableName);
			AssertEquals(nameof(package.HumanReadableShortcutName), "Package", package.HumanReadableShortcutName);
		});
	}

	[ExpectNoExceptions]
	public void TestSetMarksAndNumbersTriggerUnitCountValidation()
	{
		var mockPackage = Factory.NewMoq<NctsPackage>();
		var mockPackageValidation = new Mock<NctsPackagePhase4Validation>(mockPackage.Object);
		mockPackage.Protected()
			.Setup<Customs.Business.CusInvPackValidation>("GetNewPhase4Validation")
			.Returns(mockPackageValidation.Object);
		mockPackageValidation.Protected().Setup("CheckB5_UnitCount");
		mockPackage.Object.B5_MarksAndNumbers = "*";
		mockPackageValidation.VerifyAll();
		mockPackage.Verify();
	}

	protected override NctsDepartureCargoDesc GetNewParent()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		return nctsHeader.MovementHeader.GoodsItems.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		package = Factory.New<NctsPackage>();
		goodsItem.Packages.Add(package);
	}

	NctsHeader nctsHeader;
	NctsPackage package;
}
