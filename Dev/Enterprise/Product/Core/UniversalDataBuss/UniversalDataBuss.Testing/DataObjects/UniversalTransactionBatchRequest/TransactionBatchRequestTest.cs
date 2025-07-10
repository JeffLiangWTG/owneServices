using System.IO;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(TransactionBatchRequest))]
	class TransactionBatchRequestTest : TopLevelDataObjectTestCase<TransactionBatchRequest>
	{
		public void TestEmptyObjectWritesFineThroughXmlWriter()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var transactionBatchRequest = new TransactionBatchRequest();

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(transactionBatchRequest, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized TransactionBatchRequest", EmptyTransactionBatchRequestXML.Trim(), result);
					}
				}
			}
		}

		#region EmptyTransactionBXML

		const string EmptyTransactionBatchRequestXML = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransactionBatchRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionBatchRequest>
  </TransactionBatchRequest>
</UniversalTransactionBatchRequest>
";
		#endregion
	}
}

