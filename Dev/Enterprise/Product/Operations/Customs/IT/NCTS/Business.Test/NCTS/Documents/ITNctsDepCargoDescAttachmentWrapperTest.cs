using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ITNctsDepCargoDescAttachmentWrapper))]
sealed class ITNctsDepCargoDescAttachmentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("when cargo desc line is null", () => ITNctsDepCargoDescAttachmentWrapper.New(null, Factory));
		AssertNoExceptionThrown("when cargo desc line is valid", () => ITNctsDepCargoDescAttachmentWrapper.New(Factory.New<ITNctsDepartureCargoDesc>(), Factory));
	}

	public void TestWrapperProperties()
	{
		CombineAssertions(() =>
		{
			(ITNctsHeader header, ITNctsDepartureCargoDesc item, ITNctsDepCargoDescAttachmentWrapper wrapper) = SetUpData();
			AssertEquals("Item number", 8, wrapper.ItemNumber);
			AssertEquals("HarmonisedTariff", "1000000099", wrapper.HarmonisedTariff);
			AssertEquals("Description", "Description 199", wrapper.Description);
			AssertEquals("Remarks", remarksString, wrapper.Remarks);
		});
	}

	(ITNctsHeader header, ITNctsDepartureCargoDesc item, ITNctsDepCargoDescAttachmentWrapper wrapper) SetUpData()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType("D");
		var item = header.Bills.AddNew().GoodsItems.AddNew();
		item.BY_LineNo = 8;
		item.Remarks = remarksString;
		item.BY_Description = "Description 199";
		item.BY_HarmonisedTariff = "1000000099";

		var wrapper = (ITNctsDepCargoDescAttachmentWrapper)ITNctsDepCargoDescAttachmentWrapper.New(item, Factory);
		return (header, item, wrapper);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return ITNctsDepCargoDescAttachmentWrapper.New(Factory.New<ITNctsDepartureCargoDesc>(), Factory);
	}

	const string remarksString = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit:
	1) Duis scelerisque rhoncus nibh, a convallis mi maximus et. 
	2) Suspendisse eget enim pulvinar, posuere lacus sed, aliquam magna.
	3) Praesent egestas elit nisi, quis ultrices justo consequat vel.";
}
