using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class BillConfigurationAbstractTest<B> : TestCaseWithFactory
		where B : BillConfiguration
	{
		public void TestValidationDecider_DeparturePhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			AssertType(ExpectedDeparturePhase5ValidationDeciderType, configuration.GetValidationDecider(header));
		}

		public void TestValidationDecider_ArrivalPhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			AssertType(ExpectedArrivalPhase5ValidationDeciderType, configuration.GetValidationDecider(header));
		}

		public void TestValidationDecider_WhenHeaderIsNull()
		{
			AssertType(null, configuration.GetValidationDecider(null));
		}

		public void TestBillAdditionalDocumentValidationDecider()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions("When MovementType: Departure", () =>
			{
				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertNull("NCTS4", configuration.GetBillAdditionalDocumentValidationDecider(departureHeader));

				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertType("NCTS5", GetBillAdditionalDocumentValidationPhase5DeciderForTest(), configuration.GetBillAdditionalDocumentValidationDecider(departureHeader));
			});

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions("When MovementType: Arrival", () =>
			{
				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertNull("NCTS4", configuration.GetBillAdditionalDocumentValidationDecider(arrivalHeader));

				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertNull("NCTS5", configuration.GetBillAdditionalDocumentValidationDecider(arrivalHeader));
			});
		}

		protected abstract Type ExpectedDeparturePhase5ValidationDeciderType { get; }

		protected abstract Type ExpectedArrivalPhase5ValidationDeciderType { get; }

		protected virtual Type GetBillAdditionalDocumentValidationPhase5DeciderForTest() => typeof(NctsBillAdditionalDocumentPhase5ValidationDecider);

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (B)Activator.CreateInstance(typeof(B));
		}

		protected B configuration;
	}

	sealed class NctsBillConfigurationBaseOnlyTest : BillConfigurationAbstractTest<BillConfiguration>
	{
		protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsBillDeparturePhase5ValidationDecider);

		protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsBillArrivalPhase5ValidationDecider);
	}
}
