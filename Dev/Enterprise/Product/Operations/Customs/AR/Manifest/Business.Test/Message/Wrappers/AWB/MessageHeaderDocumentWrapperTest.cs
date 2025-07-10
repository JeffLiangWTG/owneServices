using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class MessageHeaderDocumentWrapperTest : TestCaseWithFactory
	{
		[TestDate(2022, 09, 09, 05, 10, 10)]
		public void TestMessageHeaderDocumentWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "23456ABCD";

			IHouseWaybill wrapper = new HouseWaybillWrapper(bill, MessageSubTypeCodes.Codes.Original);
			IMessageHeaderDocument messageHeader = wrapper.MessageHeader;

			CombineAssertions(() =>
			{
				AssertEquals("House Waybill", messageHeader.Name);
				AssertEquals("Item703", messageHeader.TypeCode);
				AssertEquals("2022-09-09T05:10:10", messageHeader.IssueDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("Creation", messageHeader.PurposeCode);
				AssertEquals("3.00", messageHeader.VersionID);
				AssertEquals("7845", messageHeader.SenderPartyID);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "7845";
		}
	}
}
