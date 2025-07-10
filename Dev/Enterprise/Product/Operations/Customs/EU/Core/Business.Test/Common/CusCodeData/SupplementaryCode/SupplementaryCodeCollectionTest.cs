using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(SupplementaryCodeCollection))]
	sealed class SupplementaryCodeCollectionTest : BaseSupplementaryCodeCollectionAbstractTest<SupplementaryCode>
	{
		[ExpectNoExceptions]
		public void TestAsString()
		{
			var classification = Factory.New<CusClassification>();
			var collection = new SupplementaryCodeCollectionForTest(classification.CC_EcAdditionalSupplementsInfo, 8);
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(0));
			collection.AsString = "QWER,TYUI,ASDF";
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(3));
			collection.AddNew("GHJK");
			NUnit.Framework.Assert.That(collection.AsString, NUnit.Framework.Is.EqualTo("QWER,TYUI,ASDF,GHJK").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultOfCY_Order()
		{
			var classification = Factory.New<CusClassification>();
			var collection = new SupplementaryCodeCollectionForTest(classification.CC_EcAdditionalSupplementsInfo, 8);
			NUnit.Framework.Assert.That(collection.AddNew().CY_Order, NUnit.Framework.Is.EqualTo((short)1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection.AddNew().CY_Order, NUnit.Framework.Is.EqualTo((short)2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection.AddNew().CY_Order, NUnit.Framework.Is.EqualTo((short)3).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNumberOfCodesAllowed()
		{
			var classification = Factory.New<CusClassification>();
			var collection = new SupplementaryCodeCollectionForTest(classification.CC_EcAdditionalSupplementsInfo, 8);
			AssertNumberOfCodes(collection, 8);
			collection = new SupplementaryCodeCollectionForTest(classification.CC_EcAdditionalSupplementsInfo, 10);
			AssertNumberOfCodes(collection, 10);
		}

		[ExpectNoExceptions]
		void AssertNumberOfCodes(SupplementaryCodeCollection collection, int size)
		{
			for (int i = 0; i < size; i++)
			{
				NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True, "Can Add " + i);
				collection.AddNew();
			}
			NUnit.Framework.Assert.That(!collection.AllowNew, NUnit.Framework.Is.True, "Cannot add more than " + size + " codes");
		}

		protected override CusCodeDataCollection<SupplementaryCode> GetCusCodeDataCollection()
		{
			var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return invoiceLine.AdditionalSupplementaryCodes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SupplementaryCode>();
			result.CY_Order = 3;
			return result;
		}
	}

	public class SupplementaryCodeCollectionForTest : SupplementaryCodeCollection
	{
		public SupplementaryCodeCollectionForTest(ZPropertyInfo info, short size, short startOrder = 1)
			: base(info, size, startOrder)
		{
		}
	}
}
