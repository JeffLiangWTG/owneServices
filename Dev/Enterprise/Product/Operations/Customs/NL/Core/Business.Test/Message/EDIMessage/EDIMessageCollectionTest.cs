namespace Enterprise.Customs.NL.Business.Testing;

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

[TestedType(typeof(EDIMessageCollection))]
class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return Factory.New<NLEDIMessage>();
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return new EDIMessageCollection(EntryHeader);
	}

	public void TestEDIMessageCollection()
	{
		AssertEquals(typeof(EDIMessageCollection), EntryHeader.Messages.GetType());
	}

	JobDeclaration Declaration
	{
		get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
	}
	JobDeclaration declaration;

	CusEntryHeader EntryHeader
	{
		get { return entryHeader ?? (entryHeader = Declaration.CustomsEntryHeaders.AddNew()); }
	}
	CusEntryHeader entryHeader;
}

