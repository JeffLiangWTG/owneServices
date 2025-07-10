using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class FRNctsArrivalMovementMessagingMenuProviderTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1820:Test for empty strings using string length", Justification = "Need to retain reference to constant member for test")]
		public void TestCanSendArrivalMessage()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var arrivalMovementMessagingMenuProvider = new FRNctsArrivalMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);

				arrivalMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
				Assert(arrivalMovementMessagingMenuProvider.CanSendArrivalMessage);

				arrivalMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.ArrivalRejected;
				Assert(arrivalMovementMessagingMenuProvider.CanSendArrivalMessage);

				var list = new NctsTransitStatusList();

				foreach (var item in list.GetAllCodes().Where(x => x != NctsTransitStatusList.Codes.ArrivalRejected && x != NctsTransitStatusList.Codes.Unknown))
				{
					arrivalMovement.BM_CustomsStatus = item;
					Assert(!arrivalMovementMessagingMenuProvider.CanSendArrivalMessage);
				}
			}
		}

		public void TestCreateNewProvider()
		{
			var header = Factory.New<NctsHeader>();
			var helper = new NctsHeaderUniversalMessagingHelper();

			using (var nctsMovementForm = new NctsMovementForm(header))
			{
				header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
				var provider = NctsMessagingMenuProvider.New(header, helper, nctsMovementForm);
				AssertType<FRNctsArrivalMovementMessagingMenuProvider>(provider);
			}
		}

		class FRNctsArrivalMovementMessagingMenuProviderForTest : FRNctsArrivalMovementMessagingMenuProvider
		{
			public FRNctsArrivalMovementMessagingMenuProviderForTest(NctsHeader header, NctsMovementForm form) : base(header, form)
			{
			}
			public new bool CanSendArrivalMessage => base.CanSendArrivalMessage;
		}
	}
}
