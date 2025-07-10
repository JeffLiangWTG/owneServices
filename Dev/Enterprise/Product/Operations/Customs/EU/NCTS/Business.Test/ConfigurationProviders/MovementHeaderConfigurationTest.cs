using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class MovementHeaderConfigurationAbstractTest<T> : TestCaseWithFactory
		where T : MovementHeaderConfiguration, new()
	{
		public void TestValidationDecider_DeparturePhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType(ExpectedDeparturePhase4ValidationDeciderType, configuration.GetValidationDecider(header.MovementHeader));
		}

		protected abstract Type ExpectedDeparturePhase4ValidationDeciderType { get; }

		public void TestValidationDecider_DeparturePhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType(ExpectedDeparturePhase5ValidationDeciderType, configuration.GetValidationDecider(header.MovementHeader));
		}
		protected abstract Type ExpectedArrivalPhase5ValidationDeciderType { get; }

		public void TestValidationDecider_ArrivalPhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType(ExpectedArrivalPhase5ValidationDeciderType, configuration.GetValidationDecider(header.ArrivalMovementHeader));
		}

		protected abstract Type ExpectedDeparturePhase5ValidationDeciderType { get; }

		public void TestCusGoodsLocationValidationDecider_DeparturePhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType(ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType, configuration.GetGoodsLocationValidationDecider(header.MovementHeader));
		}

		protected abstract Type ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType { get; }

		public void TestCusGoodsLocationValidationDecider_ArrivalPhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType(ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType, configuration.GetGoodsLocationValidationDecider(header.ArrivalMovementHeader));
		}

		protected abstract Type ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType { get; }

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new T();
		}
		protected T configuration;
	}

	sealed class MovementHeaderConfigurationBaseOnlyTest : MovementHeaderConfigurationAbstractTest<MovementHeaderConfiguration>
	{
		public void TestValidationDecider_ArrivalPhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(configuration.GetValidationDecider(header.ArrivalMovementHeader));
		}

		public void TestCusGoodsLocationValidationDecider_DeparturePhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(configuration.GetGoodsLocationValidationDecider(header.MovementHeader));
		}

		public void TestCusGoodsLocationValidationDecider_ArrivalPhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(configuration.GetGoodsLocationValidationDecider(header.ArrivalMovementHeader));
		}

		protected override Type ExpectedDeparturePhase4ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase4ValidationDecider);

		protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureMovementHeaderPhase5ValidationDecider);

		protected override Type ExpectedArrivalPhase5CusGoodsLocationValidationDeciderType => typeof(ArrivalPhase5CusGoodsLocationValidationDecider);

		protected override Type ExpectedDeparturePhase5CusGoodsLocatonValidationDeciderType => typeof(DeparturePhase5CusGoodsLocationValidationDecider);

		protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsArrivalMovementHeaderPhase5ValidationDecider);
	}
}
