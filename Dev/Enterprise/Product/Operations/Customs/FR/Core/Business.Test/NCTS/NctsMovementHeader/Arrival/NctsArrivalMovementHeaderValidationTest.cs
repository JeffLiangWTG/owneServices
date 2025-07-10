using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsArrivalMovementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_LocationOfGoodsCode()
		{
			AssertNoMessageErrorContaining(arrivalMovement.BM_LocationOfGoodsCodeInfo, "In a Simplified Procedure, this field can’t be empty.");

			arrivalMovement.IsSimplifiedNctsProcedure = ZBool.True;
			AssertHasMessageErrorContaining(arrivalMovement.BM_LocationOfGoodsCodeInfo, "In a Simplified Procedure, this field can’t be empty.");

			arrivalMovement.BM_LocationOfGoodsCode = "FR7758258";
			AssertNoMessageErrorContaining(arrivalMovement.BM_LocationOfGoodsCodeInfo, "In a Simplified Procedure, this field can’t be empty.");
		}

		public void TestCheckBM_MessageStatus()
		{
			var log = nctsHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms;
				log.SL_SE_NKEvent = Events.FrenchCustomsMessageStatus.Code;
				log.SL_EventTime = ZDateTime.Now;
				var errorContextItem = log.SourceInfoItems.AddNew();
				errorContextItem.Key = "Error";
				errorContextItem.Data = "Error Description";
			}

			var log2 = nctsHeader.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_Reference = CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms;
				log2.SL_SE_NKEvent = Events.FrenchCustomsMessageStatus.Code;
				log2.SL_EventTime = ZDateTime.Now.AddDays(-1);
				var errorContextItem = log2.SourceInfoItems.AddNew();
				errorContextItem.Key = "Error";
				errorContextItem.Data = "Error Description 2";
			}

			AssertNoWarning("No warning to expect when BM_MessageStatus is not REJ.", arrivalMovement.BM_MessageStatusInfo, "Error Description");

			arrivalMovement.BM_MessageStatus = EDIMessageStatusList.Codes.Rejected;
			AssertHasWarning("Warning should match most recent event.", arrivalMovement.BM_MessageStatusInfo, "Error Description");
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			arrivalMovement = nctsHeader.ArrivalMovementHeader;
		}
		NctsHeader nctsHeader;
		NctsArrivalMovementHeader arrivalMovement;
	}
}
