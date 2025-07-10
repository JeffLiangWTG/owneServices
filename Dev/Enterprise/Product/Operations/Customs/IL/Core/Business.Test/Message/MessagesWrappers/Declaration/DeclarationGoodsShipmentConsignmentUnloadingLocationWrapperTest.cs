using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business.Message.MessagesWrappers.Declaration;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentUnloadingLocationWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper>
	{
		public void TestArrivalDateTime()
		{
			AssertEquals("ArrivalDateTime should be equal to the expected value", "2024-06-30T10:12:00", Provider.ArrivalDateTime);
			jobDeclaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNull("ArrivalDateTime", Provider.ArrivalDateTime);
		}

		public void TestID()
		{
			AssertNotNull("ID", Provider.ID);
			AssertEquals("ID should be equal to JE_CustomsDischargePort", "ILASH", Provider.ID.Value);

			jobDeclaration.JE_CustomsDischargePort = ZString.Empty;
			jobDeclaration.JE_RL_NKPortOfArrival = "ILTLV";
			AssertNotNull("ID", Provider.ID);
			AssertEquals("ID should be equal to JE_RL_NKPortOfArrival", "ILTLV", Provider.ID.Value);
		}

		protected override DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper GetProvider()
		{
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_DateOfArrival = new DateTime(2024, 06, 30, 10, 12, 0, DateTimeKind.Utc);
			jobDeclaration.JE_CustomsDischargePort = "ILASH";

			return DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper.NewOrNull(jobDeclaration);
		}

		JobDeclaration jobDeclaration;
	}
}
