using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUStateCodeCollection))]
	public class AUStateCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AUStateCodeCollection>
	{
		public void TestLoadingAUStateCodeElements()
		{
			var collection = GetCollectionToTest();
			string[] splitValues = AddInfoProperty.Value.ToString().Split(',');
			AssertEquals("Collection count", splitValues.Length, collection.Count);
			for (int i = 0; i < splitValues.Length; i++)
			{
				AssertEquals("Values should be the same", true, splitValues[i] == collection[i].Code);
			}
		}

		public void TestGetNewAUStateElementsString()
		{
			var collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			AUStateCode code1 = collection.AddNew();
			code1.Code = "ACT";
			collection.ReBuildAUState();
			AssertEquals("With one element", "ACT", AddInfoProperty.Value);
			AUStateCode code2 = collection.AddNew();
			code2.Code = "FO";
			collection.ReBuildAUState();
			AssertEquals("With two element", "ACT,FO", AddInfoProperty.Value);
		}

		protected override AUStateCodeCollection GetCollectionToTest()
		{
			return new AUStateCodeCollection(invoiceLine, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AUStateCode(invoiceLine, Factory);
		}

		public ZPropertyInfo AddInfoProperty
		{
			get { return invoiceLine.AddInfo.ZA_AUState_HiddenInfo; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AUState_Hidden = "ACT,FO";
		}
		JobComInvoiceLine invoiceLine;
	}
}
