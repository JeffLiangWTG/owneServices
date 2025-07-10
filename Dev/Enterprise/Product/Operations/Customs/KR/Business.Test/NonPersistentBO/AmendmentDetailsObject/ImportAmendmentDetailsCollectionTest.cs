using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportAmendmentDetailsCollection))]
	sealed class ImportAmendmentDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportAmendmentDetailsCollection>
	{
		protected override ImportAmendmentDetailsCollection GetCollectionToTest() => new ImportAmendmentDetailsCollection(Factory.New<CusEntryHeader>());
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			return new ImportAmendmentDetails(new Import5FECreator().Create(entry, sendingObject), Factory);
		}

		public void TestNullCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var fileReader = new TestFileReader(typeof(ImportAmendmentDetailsCollectionTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing", "GOVCBR5FE_Null.xml");
			var message5FE1 = entry.Messages.AddNew();
			message5FE1.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE1.EM_MessageNum = "10";
			message5FE1.EM_MessageText = messageText;
			message5FE1.EM_SystemCreateTimeUtc = new ZDateTime(2021, 12, 01);
			message5FE1.EM_ApplicationReference = ZString.Empty;

			AssertNoExceptionThrown(() => new ImportAmendmentDetailsCollection(entry));
		}
	}
}
