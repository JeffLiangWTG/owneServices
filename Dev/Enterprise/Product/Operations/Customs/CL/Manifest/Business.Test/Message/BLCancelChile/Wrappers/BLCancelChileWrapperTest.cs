using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class BLCancelChileWrapperTest : TestCaseWithFactory
	{
		AsycudaBill bill;

		public void TestBLCancelWrapper()
		{
			CreateAndPopulateHouseBill();

			IBLCancelRequest wrapper = new BLCancelChileWrapper(bill, "Reason");
			IParticipantDocuments participant = wrapper.ParticipantDocuments;
			IDocumentObservation document = wrapper.DocumentObservations.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("15716610", wrapper.DocumentID);
				AssertEquals("SMS-301", wrapper.ReferenceNumber);

				AssertEquals("92048000-4", participant.IDValue);

				AssertEquals("Reason", document.Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "92048000-4";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Chile);

			Factory.Save();
		}

		void CreateAndPopulateHouseBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "SMS-301";
			bill.CustomsEntryNumber = "15716610";

			Factory.Save();
		}
	}
}
