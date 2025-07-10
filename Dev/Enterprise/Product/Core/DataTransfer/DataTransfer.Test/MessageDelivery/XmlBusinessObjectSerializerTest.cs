using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class XmlBusinessObjectSerializerTest : TestCaseWithFactory
	{
		[TestDate(2000, 1, 1)]
		public void TestSerializeToStream()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_MessagePurpose = "APP";
			mode.EK_LocalPartyVanID = "senderID";
			mode.EK_RelatedPartyVanID = "receiverID";
			OrgHeader testBizObj = Factory.NewWithValidTestData<OrgHeader>();
			testBizObj.OH_Code = "JobNumber";
			OrganisationValueObjectDataAdapter dataAdapter = new OrganisationValueObjectDataAdapter();
			byte[] xmlBytesFromBizObj = GetExpectedBizObjXmlBytes(testBizObj, dataAdapter, mode.EK_LocalPartyVanID, mode.EK_RelatedPartyVanID, mode.EK_MessagePurpose);

			XmlBusinessObjectSerializer testXmlSerializer = new XmlBusinessObjectSerializer(dataAdapter, mode.EK_MessagePurpose) { Context = new ValueObjectExportContext(notifications), Mode = mode };
			using (var xmlStream = testXmlSerializer.SerializeToStream(testBizObj))
			{
				byte[] xmlBytesFromStream = xmlStream.ConvertToByteArrayAndCloseStream();

				AssertEquals("Stream content is not correct", xmlBytesFromBizObj, xmlBytesFromStream);
			}
		}

		byte[] GetExpectedBizObjXmlBytes(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, string senderID, string receiverID, string purpose)
		{
			using (MemoryStream result = new MemoryStream())
			{
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				serializer.ExportXmlData(result, dataAdapter, new BusinessObject[] { bizObj }, new ValueObjectExportContext(notifications), senderID, receiverID, purpose);
				return result.ToArray();
			}
		}

		readonly NotificationBuffer notifications = new NotificationBuffer();
	}
}
