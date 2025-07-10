using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ECCNCodeCollection))]
sealed class ECCNCodeCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestAsString()
	{
		var dec = Factory.New<JobDeclaration>();
		var line = dec.InvoiceLines.AddNew();
		var collection = line.ECCNCodes;
		AssertEquals(collection.Count, 0);
		collection.AsString = "ASDF,QWER,TYUI";
		AssertEquals(collection.Count, 3);
		collection.AddNew("GHJK");
		AssertEquals(collection.AsString, "ASDF,GHJK,QWER,TYUI");
	}

	public void TestMaxCount()
	{
		var dec = Factory.New<JobDeclaration>();
		var line = dec.InvoiceLines.AddNew();
		var collection = line.ECCNCodes;
		AssertEquals(collection.MaxCount, 9);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var dec = Factory.New<JobDeclaration>();
		var line = dec.InvoiceLines.AddNew();
		return line.ECCNCodes;
	}
}
