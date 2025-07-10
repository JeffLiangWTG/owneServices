using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBCancelChileWrapperTest : TestCaseWithFactory
	{
		AsycudaBill bill;
		AsycudaManifestHeader header;

		public void TestAWBCancelWrapper()
		{
			CreateAndPopulateBO();
			CreateAndPopulateArrivalInfo(bill.PK);

			IAWBCancelRequest wrapper = new AWBCancelChileWrapper(bill, "Motive");

			CombineAssertions(() =>
			{
				AssertEquals("15716610", wrapper.DocumentID);
				AssertEquals("SMS-301", wrapper.ReferenceNumber);
				AssertEquals("101018", wrapper.ReferenceDocument);
				AssertEquals("1", wrapper.PartialCorrelative);

				AssertEquals("92048000-4", wrapper.ParticipationIDValue);

				AssertEquals("Motive", wrapper.ObservationDescription);

				AssertEquals("06-04-2021", wrapper.DateValue);
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

		void CreateAndPopulateBO()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBillIssueDate = new ZDate(2021, 04, 06);
			header.RegistrationNumber = "101018";

			_ = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Chile, false);

			bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "SMS-301";
			bill.CustomsEntryNumber = "15716610";

			Factory.Save();
		}

		void CreateAndPopulateArrivalInfo(ZGuid billPk)
		{
			AsycudaArrivalHeader arrivalHeader = header.ArrivalHeaders.AddNew();
			AsycudaArrivalLine arrivalLine = (AsycudaArrivalLine)arrivalHeader.ArrivalDetails.AddNew();
			arrivalLine.ATL_ABL_AsycudaBill = billPk;
			arrivalLine.ATL_Quantity = 20;
		}
	}
}
