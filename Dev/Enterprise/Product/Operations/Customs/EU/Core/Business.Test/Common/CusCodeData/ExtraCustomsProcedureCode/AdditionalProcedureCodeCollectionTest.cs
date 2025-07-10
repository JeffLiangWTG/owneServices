using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(AdditionalProcedureCodeCollection))]
	class AdditionalProcedureCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestAsString()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			var collection = line.AdditionalProcedureCodes;
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(0));
			collection.AsString = "ASDF,QWER,TYUI";
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(3));
			collection.AddNew("GHJK");
			NUnit.Framework.Assert.That(collection.AsString, NUnit.Framework.Is.EqualTo("ASDF,GHJK,QWER,TYUI").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			var collection = line.AdditionalProcedureCodes;
			NUnit.Framework.Assert.That(collection.MaxCount, NUnit.Framework.Is.EqualTo(line.MaxNumberOfAdditionalProcedureCode).Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			return line.AdditionalProcedureCodes;
		}
	}
}
