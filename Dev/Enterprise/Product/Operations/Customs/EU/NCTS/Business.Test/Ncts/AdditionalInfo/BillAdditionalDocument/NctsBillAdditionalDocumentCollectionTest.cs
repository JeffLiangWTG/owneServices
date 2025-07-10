
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>))]
	sealed class NctsBillAdditionalDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestShortSequenceNumberGenerator()
		{
			var collection = (NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)GetCollectionToTest();
			AssertType<ShortSequenceNumberGenerator>("Collection should have a ShortSequenceNumberGenerator for type REF", collection.RefSequenceNumberGenerator);
			AssertType<ShortSequenceNumberGenerator>("Collection should have a ShortSequenceNumberGenerator for type INF", collection.InfSequenceNumberGenerator);
			AssertType<ShortSequenceNumberGenerator>("Collection should have a ShortSequenceNumberGenerator for type TRA", collection.TraSequenceNumberGenerator);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			return new NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>(bill);
		}

		public void TestSetDefaultsForNewChild()
		{
			var document = (NctsBillAdditionalDocument)Collection.AddNew();
			AssertEquals("CSI_Status", NctsBillAdditionalDocumentStatusList.Codes.NEW, document.CSI_Status);
		}
	}
}
