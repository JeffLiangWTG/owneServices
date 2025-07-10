using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(NumberSequenceCustomisationRegistryDataType))]
	class NumberSequenceCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NumberSequenceCustomisationRegistryDataType>
	{
		protected override NumberSequenceCustomisationRegistryDataType GetNewDataType()
		{
			return new NumberSequenceCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = TransactionsNumberSequenceCustomisationRegistryItem.AccountingTransactionsNumberSequenceCustomisationRegistryItemImpl.CreateDefaultCollection();
			var collection2 = TransactionsNumberSequenceCustomisationRegistryItem.AccountingTransactionsNumberSequenceCustomisationRegistryItemImpl.CreateDefaultCollection();
			collection2[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Order = 3;
			string xmlFull = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfTransactionNumberSequenceCustomisation xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TransactionNumberSequenceCustomisation><ElementName>Transaction Header Branch Code</ElementName><Include>N</Include><Order>0</Order><Code /><Length>3</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Transaction Header Department Code</ElementName><Include>N</Include><Order>0</Order><Code /><Length>3</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Custom Element 1</ElementName><Include>N</Include><Order>0</Order><Code /><Length>0</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Custom Element 2</ElementName><Include>N</Include><Order>0</Order><Code /><Length>0</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Year as Digit(s)</ElementName><Include>N</Include><Order>0</Order><Code>4</Code><Length>4</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Year as Letter</ElementName><Include>N</Include><Order>0</Order><Code /><Length>1</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Month as 2 Digits</ElementName><Include>N</Include><Order>0</Order><Code /><Length>2</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Month as Letter</ElementName><Include>N</Include><Order>0</Order><Code /><Length>1</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Accounting Year as Digit(s)</ElementName><Include>N</Include><Order>0</Order><Code>4</Code><Length>4</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Accounting Year as Letter</ElementName><Include>N</Include><Order>0</Order><Code /><Length>1</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Accounting Period as 2 Digits</ElementName><Include>N</Include><Order>0</Order><Code /><Length>2</Length><Fountain>N</Fountain></TransactionNumberSequenceCustomisation><TransactionNumberSequenceCustomisation><ElementName>Sequence Number</ElementName><Include>Y</Include><Order>50</Order><Code /><Length>8</Length><Fountain>Y</Fountain></TransactionNumberSequenceCustomisation></ArrayOfTransactionNumberSequenceCustomisation>";
			string xmlOldOverriden = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfTransactionNumberSequenceCustomisation xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TransactionNumberSequenceCustomisation><ElementName>Sequence Number</ElementName><Include>Y</Include><Order>3</Order><Length>8</Length><Fountain>Y</Fountain></TransactionNumberSequenceCustomisation></ArrayOfTransactionNumberSequenceCustomisation>";

			return new[] {
				new ValidSampleAndBinaryValueInDB(collection1, System.Text.Encoding.Unicode.GetBytes(xmlFull)),
				new ValidSampleAndBinaryValueInDB(collection2, System.Text.Encoding.Unicode.GetBytes(xmlOldOverriden)) };
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
