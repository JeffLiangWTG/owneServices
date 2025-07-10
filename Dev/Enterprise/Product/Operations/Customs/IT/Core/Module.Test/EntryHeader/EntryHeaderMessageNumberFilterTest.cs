using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Test;

[TestedType(typeof(EntryHeaderFilterBusinessObject))]
sealed class EntryHeaderMessageNumberFilterTest : FilterStripBusinessObjectTestCase
{
	JobDeclaration declarationOne, declarationTwo;
	CusEntryHeader entryHeaderOne, entryHeaderTwo;
	FilterStripBusinessObject filterStripBusinessObject;
	Customs.Business.CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollectionOne, entryHeaderCollectionTwo;
	ITEDIMessage messageOne, messageTwo;

	public void TestMessageNumberFilter()
	{
		entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
		entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
		CombineAssertions(() =>
		{
			AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
			AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
		});

		var messageNumberFilter =
			(ModuleTextFilter)filterStripBusinessObject[
				EntryHeaderFilterBusinessObject.ITFilterConstants.MessageNumber];
		AssertNotNull("Message Number Filter", messageNumberFilter);

		messageNumberFilter.Property = messageOne.EM_MessageNum;
		messageNumberFilter.IsActive = true;
		entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
		entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
			AssertEquals("Single Entry Header Found", entryHeaderOne.PK, entryHeaderCollectionOne[0].PK);
			AssertEquals("EntryHeaderCollectionTwo Count", 0, entryHeaderCollectionTwo.Count);
		});

		messageNumberFilter.Property = messageTwo.EM_MessageNum;
		messageNumberFilter.IsActive = true;
		entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
		entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("EntryHeaderCollectionOne Count", 0, entryHeaderCollectionOne.Count);
			AssertEquals("EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
			AssertEquals("Single Entry Header Found", entryHeaderTwo.PK, entryHeaderCollectionTwo[0].PK);
		});

		messageNumberFilter.Property = ZString.Empty;
		messageNumberFilter.IsActive = true;
		entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
		entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
			AssertEquals("EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
		});
	}

	public void TestEntryHeaderMessageNumberFilterGenerateReturnTextFilter()
	{
		var messageNumberFilter = new EntryHeaderMessageNumberFilterGenerator()
			.Generate("My Description", FilterCategories.Other,
				ResString.GetMultilingualString("E4782E7B-7E55-4F59-A2F9-0F69B226DE4B", "Some Description"));

		AssertNotNull("Message Number filter", messageNumberFilter);
		AssertEquals("Message Number Filter Type", typeof(ModuleTextFilter), messageNumberFilter.GetType());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declarationOne = Factory.New<JobDeclaration>();
		entryHeaderOne = declarationOne.CustomsEntryHeaders.AddNew();
		messageOne = entryHeaderOne.Messages.AddNew();
		messageOne.MessageNumberStrategy = new FixedMessageNumberStrategy("023445323");
		entryHeaderCollectionOne =
			new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declarationOne, Factory);

		declarationTwo = Factory.New<JobDeclaration>();
		entryHeaderTwo = declarationTwo.CustomsEntryHeaders.AddNew();
		messageTwo = entryHeaderTwo.Messages.AddNew();
		messageTwo.MessageNumberStrategy = new FixedMessageNumberStrategy("19348233");
		entryHeaderCollectionTwo =
			new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declarationTwo, Factory);

		Factory.Save();
		filterStripBusinessObject = GetNewFilterStripBusinessObject();
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		=> new EntryHeaderFilterBusinessObject();
}
