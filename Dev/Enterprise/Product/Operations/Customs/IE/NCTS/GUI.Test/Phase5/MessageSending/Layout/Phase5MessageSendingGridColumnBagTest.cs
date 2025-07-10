using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	sealed class Phase5MessageSendingGridColumnBagTest : TestCase
	{
		public void TestDestinationCustomsOfficeCodeFindBoxColumn()
		{
			AssertNotNull(ColumnBag.DestinationCustomsOfficeCodeFindBoxColumn);
			var columnInfo = ColumnBag.DestinationCustomsOfficeCodeFindBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZCodeFindBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", false, columnInfo.IsMandatory);
				AssertEquals("ColumnName", NctsHeaderMessageSendingObject.Schema.DestinationCustomsOfficeCode, columnInfo.ColumnName);
			});
		}

		public void TestConsigneeFindBoxColumn()
		{
			AssertNotNull(ColumnBag.ConsigneeFindBoxColumn);
			var columnInfo = ColumnBag.ConsigneeFindBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZCodeFindBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", false, columnInfo.IsMandatory);
				AssertEquals("ColumnName", NctsHeaderMessageSendingObject.Schema.Consignee, columnInfo.ColumnName);
			});
		}

		public void TestTC11DeliveryDateColumn()
		{
			AssertNotNull(ColumnBag.TC11DeliveryDateColumn);
			var columnInfo = ColumnBag.TC11DeliveryDateColumn.CreateGridColumnInfo();
			CombineAssertions("ZDateEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDateEditColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", false, columnInfo.IsMandatory);
				AssertEquals("ColumnName", NctsHeaderMessageSendingObject.Schema.TC11DeliveryDate, columnInfo.ColumnName);
			});
		}

		public void TestEnquiryTextBoxColumn()
		{
			AssertNotNull(ColumnBag.EnquiryTextBoxColumn);
			var columnInfo = ColumnBag.EnquiryTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", false, columnInfo.IsMandatory);
				AssertEquals("ColumnName", NctsHeaderMessageSendingObject.Schema.EnquiryText, columnInfo.ColumnName);
			});
		}

		public void TestMessageStatusTextBoxColumn()
		{
			AssertNotNull(ColumnBag.MessageStatusTextBoxColumn);
			var columnInfo = ColumnBag.MessageStatusTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", false, columnInfo.IsMandatory);
				AssertEquals("ColumnName", NctsHeaderMessageSendingObject.Schema.MessageStatus, columnInfo.ColumnName);
			});
		}

		Phase5MessageSendingGridColumnBag ColumnBag => Phase5MessageSendingGridColumnBag.Instance;
	}
}
