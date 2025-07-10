using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionsNumberSequenceCustomisationRegistryDataType))]
	class TransactionsNumberSequenceCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TransactionsNumberSequenceCustomisationRegistryDataType>
	{
		protected override TransactionsNumberSequenceCustomisationRegistryDataType GetNewDataType()
		{
			return new TransactionsNumberSequenceCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation element = collection.AddNew();
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Order = 1;
			element.Include = true;
			element.Length = 8;

			string xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfTransactionNumberSequenceCustomisation xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TransactionNumberSequenceCustomisation><ElementName>Sequence Number</ElementName><Include>Y</Include><Order>1</Order><Length>8</Length></TransactionNumberSequenceCustomisation></ArrayOfTransactionNumberSequenceCustomisation>";
			byte[] byteArray = System.Text.Encoding.Unicode.GetBytes(xml);

			return new[] { new ValidSampleAndBinaryValueInDB(collection, byteArray) };
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "AccountingTransactionsNumberSequenceCustomisationRegistryItemEditor";
			}
		}
	}
}
