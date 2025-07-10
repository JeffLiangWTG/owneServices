using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM451;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM451ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestDecisionReason()
		{
			AssertEquals("Invalid data", provider.DecisionReason);
		}

		public void TestPreferredPaymentMethod()
		{
			AssertEquals("J", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestControlResult()
		{
			var controlResult = provider.ControlResult;
			AssertNotNull(controlResult);
			AssertEquals("CI001", controlResult.ControlResultCode);
			var expectedDate = DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime("20230101");
			AssertEquals(expectedDate, controlResult.ControlDate);
			AssertEquals("rr001", controlResult.Remarks);
		}

		IM451Provider provider;

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM451Provider(new Im451
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345ABCDE678R9",
					Lrn = "LRN001",
					AdditionalDeclarationType = "A",
					RejectionReason = "Invalid data",
					PreferredPaymentMethod = "J",
					Remarks = "Remarks001",
					ControlResult = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX.ControlsType
					{
						ControlResultCode = "CI001",
						ControlDate = "20230101",
						Remarks = "rr001"
					}
				}
			});
		}
	}
}

