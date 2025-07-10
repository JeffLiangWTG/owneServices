using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescAttachmentPrintingSupporterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When goodsItem is null", () => new NctsDepartureCargoDescAttachmentPrintingSupporter(goodsItem: null));
		AssertNoExceptionThrown("When goodsItem is not null", () => new NctsDepartureCargoDescAttachmentPrintingSupporter(Factory.New<NctsDepartureCargoDesc>()));
	}

	public void TestRequiresAttachment()
	{
		var goodsItem = Factory.New<NctsDepartureCargoDesc>();
		var attachmentSupporter = (IAttachmentPrintingSupporter)new NctsDepartureCargoDescAttachmentPrintingSupporter(goodsItem);
		goodsItem.Remarks = "";
		AssertEquals("RequiresAttachment", false, attachmentSupporter.RequiresAttachment);

		goodsItem.Remarks = "ABC";
		AssertEquals("RequiresAttachment is cached", false, attachmentSupporter.RequiresAttachment);

		attachmentSupporter = new NctsDepartureCargoDescAttachmentPrintingSupporter(goodsItem);
		AssertEquals("RequiresAttachment after re-instantiation", true, attachmentSupporter.RequiresAttachment);
	}
}
