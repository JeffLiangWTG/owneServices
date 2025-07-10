using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionLinesCollection))]
	public class TransactionLinesCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		public void TestNew()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TransactionLinesCollection collection = new TransactionLinesCollection(factory, new ZQuery());
			AssertEquals("Collection should be empty", 0, collection.Count);

			TransactionLine line = (TransactionLine)factory.New(typeof(DummyTransactionLine));
			collection.Add(line);
			AssertEquals("Collection should have one element", 1, collection.Count);
			AssertEquals("Should return added element", line, collection[0]);
		}

		protected class DummyTransactionLine : TransactionLine
		{
			public DummyTransactionLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString LineType
			{
				get
				{
					return new ZString();
				}
			}

			protected override bool InvertSigns
			{
				get
				{
					return false;
				}
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransactionLinesCollection(Factory, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoiceLine));
		}
	}
}
