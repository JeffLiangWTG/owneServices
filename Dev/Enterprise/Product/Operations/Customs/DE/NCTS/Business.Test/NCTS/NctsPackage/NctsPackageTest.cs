using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackage))]
	sealed class NctsPackageTest : CusInvPackTest<NctsDepartureCargoDesc>
	{
		public void TestB5_MarksAndNumbers_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals(512, package.B5_MarksAndNumbersInfo.MaxLength);
			});
		}

		public void TestB5_MarksAndNumbers_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(42, package.B5_MarksAndNumbersInfo.MaxLength);
			});
		}

		public void TestEnsureThatCorrectTypeDeciderIsUsed()
		{
			Factory.Save();
			AssertType<NctsPackage>("Load using EU.NCTS.Business.NctsPackage", new BusinessObjectFactory().Load<EU.NCTS.Business.NctsPackage>(package.PK));
			AssertType<NctsPackage>("Load using Customs.Business.CusInvPack", new BusinessObjectFactory().Load<Customs.Business.CusInvPack>(package.PK));
			AssertType<NctsPackage>("Load using DE.Business.NctsPackage", new BusinessObjectFactory().Load<NctsPackage>(package.PK));
		}

		public void TestValidation()
		{
			AssertType<NctsPackageValidation>(package.Validation);
		}

		protected override NctsDepartureCargoDesc GetNewParent()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => package;

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = goodsItem.Packages.AddNew();
		}
		NctsPackage package;
		NctsDepartureCargoDesc goodsItem;
	}
}
