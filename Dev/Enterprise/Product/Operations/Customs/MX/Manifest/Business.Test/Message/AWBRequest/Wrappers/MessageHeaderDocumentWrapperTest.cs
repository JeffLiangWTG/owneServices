using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class MessageHeaderDocumentWrapperTest : TestCaseWithFactory
	{
		[TestDate(2002, 07, 01, 05, 10, 10)]
		public void TestMessageHeaderDocumentWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "23456ABCD";

			IHouseWaybill wrapper = new HouseWaybillWrapper(bill, MessageSubTypeCodes.Codes.Original);
			IMessageHeaderDocument messageHeader = wrapper.MessageHeader;

			CombineAssertions(() =>
			{
				AssertEquals("00001", messageHeader.ID);
				AssertEquals("House Waybill", messageHeader.Name);
				AssertEquals("Item703", messageHeader.TypeCode);
				AssertEquals("2002-07-01T05:10:10", messageHeader.IssueDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("Creation", messageHeader.PurposeCode);
				AssertEquals("3.00", messageHeader.VersionID);
				AssertEquals("7845", messageHeader.SenderPartyPrimaryID);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "7845";
		}
	}
}
