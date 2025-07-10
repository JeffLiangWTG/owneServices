using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationLoadingCompletionMessageSendingObject))]
	sealed class JobDeclarationLoadingCompletionMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
		}

		public void TestData()
		{
			var entry = new TestDataSetupHelper(Factory).GetDF3Entry();

			var sendingObj = new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
			AssertEquals("030-33-15-123456-2", sendingObj.CustomsReceiptNumber);
			AssertEquals("030", sendingObj.CustomsOffice);
			AssertEquals("33", sendingObj.Department);
			AssertEquals("군장유업", sendingObj.StevedoresCompany);
			AssertEquals(new ZDate(2021, 04, 06), sendingObj.LoadingDate);
		}

		public void TestStevedores()
		{
			var entry = new TestDataSetupHelper(Factory).GetDF3Entry();

			var sendingObj = new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
			AssertEquals("2 stevedores exists", 2, sendingObj.Stevedores.Count);

			var stevedoreItems = sendingObj.Stevedores;
			AssertEquals("김태경", stevedoreItems[0].PersonFullName);
			AssertEquals("0514621354", stevedoreItems[0].PersonHomePhoneNumber);
			AssertEquals("01057855226", stevedoreItems[0].PersonMobilePhoneNumber);

			AssertEquals("신태영", stevedoreItems[1].PersonFullName);
			AssertEquals("0514621354", stevedoreItems[1].PersonHomePhoneNumber);
			AssertEquals("01046587745", stevedoreItems[1].PersonMobilePhoneNumber);
		}
	}
}
