using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputDocumentDeliveryRequestDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CharteraOutputDocumentDeliveryRequestDataProvider(null));
	}

	public void TestProperties()
	{
		var documentId1 = ZGuid.NewZGuid().ToString();
		var documentId2 = ZGuid.NewZGuid().ToString();

		var transaction1 = Factory.New<CusPollingTransaction>();
		var transaction2 = Factory.New<CusPollingTransaction>();

		transaction1.CPT_TransactionID = documentId1;
		transaction2.CPT_TransactionID = documentId2;

		var dataProvider = new CharteraOutputDocumentDeliveryRequestDataProvider(new[] { transaction1, transaction2 });

		AssertNotNullOrEmpty("ProcessId", dataProvider.ProcessId);
		AssertEquals("ProcessId does not change", dataProvider.ProcessId, dataProvider.ProcessId);

		var documentIds = dataProvider.DocumentIds.ToArray();
		AssertEquals("DocumentIds.Count", 2, documentIds.Length);
		AssertEquals("DocumentIds[0]", documentId1, documentIds[0]);
		AssertEquals("DocumentIds[1]", documentId2, documentIds[1]);
		AssertSame("DocumentIds cached", dataProvider.DocumentIds, dataProvider.DocumentIds);
	}
}
