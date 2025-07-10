using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class NctsEuOfficeCodeConfigurationAbstractTest : TestCaseWithFactory
	{
		public abstract void TestAutomaticSequenceNumberEnabled();

		protected abstract Type NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest { get; }

		protected abstract Type NctsEuOfficeCodeArrivalPhase5ValidationDeciderForTest { get; }

		public void TestGetValidationDecider()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions("When MovementType: Departure", () =>
			{
				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertNull("NCTS4", configuration.GetValidationDecider(departureHeader));

				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertType("NCTS5", NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest, configuration.GetValidationDecider(departureHeader));
			});

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions("When MovementType: Arrival", () =>
			{
				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertNull("NCTS4", configuration.GetValidationDecider(arrivalHeader));

				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertType("NCTS5", NctsEuOfficeCodeArrivalPhase5ValidationDeciderForTest, configuration.GetValidationDecider(arrivalHeader));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (NctsEuOfficeCodeConfiguration)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
		}
		protected NctsEuOfficeCodeConfiguration configuration;
	}

	[TestedType(typeof(NctsEuOfficeCodeConfiguration))]
	sealed class NctsEuOfficeCodeConfigurationBaseOnlyTest : NctsEuOfficeCodeConfigurationAbstractTest
	{
		protected override Type NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest => typeof(NctsEuOfficeCodeDeparturePhase5ValidationDecider);

		protected override Type NctsEuOfficeCodeArrivalPhase5ValidationDeciderForTest => typeof(NctsEuOfficeCodeArrivalPhase5ValidationDecider);

		public override void TestAutomaticSequenceNumberEnabled()
		{
			AssertEquals(true, configuration.AutomaticSequenceNumberEnabled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
