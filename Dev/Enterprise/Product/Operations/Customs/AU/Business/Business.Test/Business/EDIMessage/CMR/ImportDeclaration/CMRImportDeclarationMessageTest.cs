using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRImportDeclarationMessageTest : CMRCUSRESMessageTest
	{
		public virtual void TestGetReportForFormattedMessage()
		{
			EDIMessage bizObj = GetEDIMessage("B12345");
			AssertEquals("Report Body Not empty", true, !bizObj.EM_FormattedMessageText.IsEmpty);
			AssertEquals("Message Included", true, bizObj.EM_FormattedMessageText.StartsWith(bizObj.EM_MessageText.Replace("'", "\r\n")));
		}

		public virtual void TestEM_MessageInterpretation()
		{
			EDIMessage bizObj = GetEDIMessage("B12345");
			AssertEquals("Report Body Not empty", true, !bizObj.EM_MessageInterpretation.IsEmpty);
		}

		public void TestGetWrappedObjectWhenDoesNotStartWithSOrB()
		{
			TestWrappedObjectWithCorrectReference("12345");
		}

		public void TestGetWrappedObjectWhenStartsWithS()
		{
			TestWrappedObjectWithCorrectReference("S12345");
		}

		public void TestGetWrappedObjectWhenStartsWithB()
		{
			TestWrappedObjectWithCorrectReference("B12345");
		}

		protected virtual void TestWrappedObjectWithCorrectReference(string reference)
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = reference;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = reference + "/1";

			Factory.Save();

			BusinessObject bizObj = GetWrappedObject(reference);
			AssertNotNull("Found Entry Header", bizObj);
			AssertEquals("Type of entry header", typeof(CusEntryHeader), bizObj.GetType());
		}

		protected abstract BusinessObject GetWrappedObject(string reference);
		protected abstract EDIMessage GetEDIMessage(string reference);
	}
}
