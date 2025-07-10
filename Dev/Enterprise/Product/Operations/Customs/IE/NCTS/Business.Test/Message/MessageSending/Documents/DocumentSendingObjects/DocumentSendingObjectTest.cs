using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentSendingObject))]
	class DocumentSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			var add1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			sendingObject = new DocumentSendingObject(nctsHeader);
			sendingObject.EDoc = add1.UniqueKey;
		}

		protected override BusinessObject GetNewBusinessObject() => sendingObject;

		NctsHeader nctsHeader;
		DocumentSendingObject sendingObject;
	}
}
