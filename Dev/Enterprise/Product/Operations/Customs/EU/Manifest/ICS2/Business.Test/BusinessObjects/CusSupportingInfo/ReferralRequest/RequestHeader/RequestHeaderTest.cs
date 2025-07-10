using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RequestHeader))]
	sealed class RequestHeaderTest : EUMemberStateCommunicationTest
	{
		public void TestHumanReadableName()
		{
			var requestHeader = GetNewRequestHeaderWithManifestHeader(Factory);
			AssertEquals("Request Header", requestHeader.HumanReadableName);
		}

		public void TestStatus()
		{
			var requestHeader = GetNewRequestHeaderWithManifestHeader(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly", expected: true, requestHeader.EUS_StatusInfo.ReadOnly);
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(RequestHeader), nameof(RequestHeader.EUS_Status), includesInherit: false, x => x.Caption == "Status");
			});
		}

		public void TestRequestInformations()
		{
			var requestHeader = GetNewRequestHeaderWithManifestHeader(Factory);
			AssertEquals(0, requestHeader.RequestInformations.Count);
			requestHeader.RequestInformations.AddNew();
			AssertEquals(1, requestHeader.RequestInformations.Count);
			Factory.Save();
			var requestHeaderReloaded = new BusinessObjectFactory().Load<RequestHeader>(requestHeader.PK);
			AssertEquals(1, requestHeaderReloaded.RequestInformations.Count);
		}

		public void TestRequestResponses()
		{
			var requestHeader = GetNewRequestHeaderWithManifestHeader(Factory);
			AssertEquals(0, requestHeader.RequestResponses.Count);
			requestHeader.RequestResponses.AddNew();
			AssertEquals(1, requestHeader.RequestResponses.Count);
			Factory.Save();
			var requestHeaderReloaded = new BusinessObjectFactory().Load<RequestHeader>(requestHeader.PK);
			AssertEquals(1, requestHeaderReloaded.RequestResponses.Count);
		}

		public void TestSupportingDocuments()
		{
			var requestHeader = GetNewRequestHeaderWithManifestHeader(Factory);
			AssertEquals(0, requestHeader.SupportingDocuments.Count);
			requestHeader.SupportingDocuments.AddNew();
			AssertEquals(1, requestHeader.SupportingDocuments.Count);
			Factory.Save();
			var requestHeaderReloaded = new BusinessObjectFactory().Load<RequestHeader>(requestHeader.PK);
			AssertEquals(1, requestHeaderReloaded.SupportingDocuments.Count);
		}

		public void TestAttachments()
		{
			var requestHeader = GetNewRequestHeaderWithManifestHeader(Factory);
			AssertEquals(0, requestHeader.Attachments.Count);
			var edoc = requestHeader.Attachments.AddNew();
			edoc.CSD_DocType = "OTH";
			AssertEquals(1, requestHeader.Attachments.Count);
			Factory.Save();
			var requestHeaderReloaded = new BusinessObjectFactory().Load<RequestHeader>(requestHeader.PK);
			AssertEquals(1, requestHeaderReloaded.Attachments.Count);
		}

		public void TestRelatedHouseBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HouseBill1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HouseBill2";

			var requestHeader = header.RequestHeaders.AddNew();
			requestHeader.EUS_HouseBillNumber = bill1.ABL_BillNumber;

			AssertEquals("Load related house bill", bill1.PK, requestHeader.RelatedHouseBill.PK);
		}

		public void TestMemberState()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var requestHeader = header.RequestHeaders.AddNew();
			AssertEquals("Default to empty", string.Empty, requestHeader.EUS_MemberState);

			requestHeader.EUS_MemberState = "FR";

			CombineAssertions(() =>
			{
				Assert("Should be readonly", requestHeader.EUS_MemberStateInfo.ReadOnly);
				AssertEquals(3, requestHeader.EUS_MemberStateInfo.MaxLength);
				AssertEquals("Should set the value on EUS_MemberState", "FR", requestHeader.EUS_MemberState);
				AssertEquals("Should get the value from EUS_MemberState", "FR", requestHeader.EUS_MemberState);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewRequestHeaderWithManifestHeader(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewRequestHeaderWithManifestHeader(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewRequestHeaderWithManifestHeader(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			return header.RequestHeaders.AddNew();
		}

		RequestHeader GetNewRequestHeaderWithManifestHeader(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var requestHeader = header.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "XXX";
			requestHeader.EUS_Type = "YYY";
			return requestHeader;
		}
	}
}
