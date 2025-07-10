using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(MailItemIDsMessageSendingObject))]
	sealed class MailItemIDsMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new MailItemIDsMessageSendingObject(entry);
		}

		public void Test5SIData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N00221000004M";

			var messageSendingObject = new MailItemIDsMessageSendingObject(entry);
			AssertEquals("6N002-21-000004M", messageSendingObject.FormattedEntryNumber);
			AssertEquals("010", messageSendingObject.DeclarationCustomsOffice);
			AssertEquals("10", messageSendingObject.DeclarationCustomsDivision);
		}
	}
}
