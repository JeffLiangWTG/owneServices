using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class NctsPackageConfigurationAbstractTest : TestCaseWithFactory
	{
		public void TestGetValidationDeciderForDeparture()
		{
			CombineAssertions("When MovementType: Departure", () =>
			{
				var headerPhase4 = Factory.New<NctsHeader>();
				headerPhase4.SetMovementType(NctsMovementType.Codes.Departure);
				headerPhase4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				var goodsItemPhase4 = headerPhase4.MovementHeader.GoodsItems.AddNew();
				goodsItemPhase4.Packages.AddNew();
				AssertNull("NCTS4", configuration.GetValidationDecider(goodsItemPhase4));

				var headerPhase5 = Factory.New<NctsHeader>();
				headerPhase5.SetMovementType(NctsMovementType.Codes.Departure);
				headerPhase5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var goodsItemPhase5 = headerPhase5.Bills.AddNew().GoodsItems.AddNew();
				goodsItemPhase5.Packages.AddNew();
				AssertType("NCTS5", NctsPackageDeparturePhase5ValidationDeciderForTest, configuration.GetValidationDecider(goodsItemPhase5));
			});
		}

		public void TestGetValidationDeciderForArrival()
		{
			CombineAssertions("When MovementType: Arrival", () =>
			{
				var headerPhase4 = Factory.New<NctsHeader>();
				headerPhase4.SetMovementType(NctsMovementType.Codes.Arrival);
				headerPhase4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				var goodsItemPhase4 = headerPhase4.ArrivalMovementHeader.GoodsItems.AddNew();
				goodsItemPhase4.Packages.AddNew();
				AssertNull("NCTS4", configuration.GetValidationDecider(goodsItemPhase4));

				var headerPhase5 = Factory.New<NctsHeader>();
				headerPhase5.SetMovementType(NctsMovementType.Codes.Arrival);
				headerPhase5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var goodsItemPhase5 = headerPhase5.Bills.AddNew().ArrivalGoodsItems.AddNew();
				goodsItemPhase5.Packages.AddNew();
				AssertType("NCTS5", NctsPackageArrivalPhase5ValidationDeciderForTest, configuration.GetValidationDecider(goodsItemPhase5));
			});
		}

		protected abstract Type NctsPackageDeparturePhase5ValidationDeciderForTest { get; }

		protected abstract Type NctsPackageArrivalPhase5ValidationDeciderForTest { get; }

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (NctsPackageConfiguration)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
		}
		protected NctsPackageConfiguration configuration;
	}

	[TestedType(typeof(NctsPackageConfiguration))]
	sealed class NctsPackageConfigurationBaseOnlyTest : NctsPackageConfigurationAbstractTest
	{
		protected override Type NctsPackageDeparturePhase5ValidationDeciderForTest => typeof(NctsPackageDeparturePhase5ValidationDecider);

		protected override Type NctsPackageArrivalPhase5ValidationDeciderForTest => typeof(NctsPackageArrivalPhase5ValidationDecider);
	}
}
