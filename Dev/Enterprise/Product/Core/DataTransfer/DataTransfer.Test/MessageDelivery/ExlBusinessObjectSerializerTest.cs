using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class ExlBusinessObjectSerializerTest : TestCaseWithFactory
	{
		[TestDate(2000, 1, 1)]
		public void TestSerializeToStream()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_MessagePurpose = "APP";
			mode.EK_LocalPartyVanID = "senderID";
			mode.EK_RelatedPartyVanID = "receiverID";

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var storageMain = documentFactory.New<IStorageMain>();

			IeDoc testBizObj = storageMain.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "DoesNotMatter", "MSC", true);

			StorageDocsBaseValueObjectDataAdatper dataAdapter = new StorageDocsBaseValueObjectDataAdatper();
			byte[] xmlBytesFromBizObj = GetExpectedBizObjXmlBytes(testBizObj as BusinessObject, dataAdapter, mode);

			ExlBusinessObjectSerializer testExlSerializer = new ExlBusinessObjectSerializer(dataAdapter, mode.EK_MessagePurpose) { Context = new ValueObjectExportContext(notifications), Mode = mode };
			using (var xmlStream = testExlSerializer.SerializeToStream(new BusinessObject[] { testBizObj as BusinessObject }))
			{
				byte[] xmlBytesFromStream = xmlStream.ConvertToByteArrayAndCloseStream();

				AssertEquals("Stream content is not correct", xmlBytesFromBizObj, xmlBytesFromStream);
			}
		}

		byte[] GetExpectedBizObjXmlBytes(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, EDICommunicationsMode mode)
		{
			MemoryStream result = new MemoryStream();
			Xsd.XmlInterchange interchange = dataAdapter.ToXmlInterchange(new BusinessObject[] { bizObj }, new ValueObjectExportContext(notifications)) as Xsd.XmlInterchange;
			interchange.InterchangeInfo.Source.SenderCode = mode.EK_LocalPartyVanID; //SenderID;
			interchange.InterchangeInfo.Target.ReceiverCode = mode.EK_RelatedPartyVanID; //ReceiverID;
			interchange.InterchangeInfo.Source.Purpose = mode.EK_MessagePurpose;
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			serialiser.Serialize(result, interchange);
			result.Position = 0;

			return result.ToArray();
		}

		readonly NotificationBuffer notifications = new NotificationBuffer();
	}
}
