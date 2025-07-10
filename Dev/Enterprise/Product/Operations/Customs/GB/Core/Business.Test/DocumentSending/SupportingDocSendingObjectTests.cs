using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.DocumentSending;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing.DocumentSending
{
	[TestedType(typeof(SupportingDocSendingObject))]
	public class SupportingDocSendingObjectTests : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			var sendingObject = SupportingDocSendingObject.New(dec);
			AssertType<SupportingDocSendingObjectValidation>(sendingObject.Validation);
		}

		public void TestUseLRNNotDUCRForFileUploadToCDSBeforeMRN()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(d => d.JE_ApplicationCode).Returns(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			var declaration = declarationMock.Object;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "1GB000000000000-B00000000";
			AssertEquals("Pre-requisite: Entry count", 1, declaration.Entries.Count);

			var message = "Entry (MRN or functional reference) value is {0}";
			var sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(string.Format(message, "empty"), sendingObject.LocalReferenceNumber, string.Empty);
			AssertNotContains("Entries", "1GB000000000000-B00000000", sendingObject.Entries.ElementsAsString);

			entry.LRN = "LRNAR1";
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(string.Format(message, "LRN"), sendingObject.LocalReferenceNumber, "LRNAR1");
			AssertContains("Entries", "LRNAR1 - LRN: LRNAR1", sendingObject.Entries.ElementsAsString);

			entry.MovementReferenceNumberSetter("MRNAR1", ZDateTime.BrettsBirthday);
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(string.Format(message, "MRN"), sendingObject.LocalReferenceNumber, "MRNAR1");
			AssertContains("Entries", "MRNAR1 - MRN: MRNAR1", sendingObject.Entries.ElementsAsString);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.LRN = "LRNAR2";
			AssertEquals("Entry count", 2, declaration.Entries.Count);
			sendingObject = SupportingDocSendingObject.New(declaration);
			AssertEquals(string.Format(message, "empty"), sendingObject.LocalReferenceNumber, "");
			AssertContainsExactElementsInAnyOrder("Entries", new string[] { "MRNAR1", "LRNAR2" }, sendingObject.Entries.GetAllCodes());
		}

		public void TestShouldCheckFileNameInEdocField()
		{
			var sendingObject = GetNewBusinessObject() as SupportingDocSendingObject;
			AssertEquals(true, sendingObject.ShouldCheckFileNameInEdocField);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return SupportingDocSendingObject.New(declaration);
		}
	}
}
