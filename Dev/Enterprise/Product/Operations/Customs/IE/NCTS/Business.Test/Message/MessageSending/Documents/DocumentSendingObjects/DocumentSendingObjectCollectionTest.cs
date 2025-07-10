using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentSendingObjectCollection))]
	sealed class DocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSendingObjectCollection>
	{
		protected override DocumentSendingObjectCollection GetCollectionToTest() => new DocumentSendingObjectCollection(nctsHeader);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentSendingObject(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = GetNewNctsHeader();
		}

		NctsHeader nctsHeader;

		NctsHeader GetNewNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			_ = header.MovementHeader.Guarantees.AddNew();
			_ = header.MovementHeader.Guarantees.AddNew();
			return header;
		}
	}
}
