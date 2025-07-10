using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class AWBCancelMessageSenderTest : TestCaseWithFactory
	{
		public void TestCancelManifest()
		{
			CreateAndPopulateBO();
			CreateAndPopulateArrivalInfo(bill.PK);

			var messageSender = new AWBCancelMessageSender(bill, new AWBCancelChileWrapper(bill, "REASON OF CANCELATION"));
			var messageResult = messageSender.SendMessage();

			AssertEquals(messageResult, "Message sent successfully");

			AssertEquals(MessageTypes.Codes.CHF, bill.Messages[0].EM_MessageType);
			AssertEquals("Message Sub Type should be Empty", "", bill.Messages[0].EM_MessageSubType);
		}

		AsycudaBill bill;
		AsycudaManifestHeader header;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "96915330-4";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Chile);
		}

		void CreateAndPopulateBO()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBillIssueDate = new ZDate(2021, 06, 21);
			header.RegistrationNumber = "123456";

			_ = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Chile, false);

			bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "SMS-1996";
			bill.CustomsEntryNumber = "15716610";
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
