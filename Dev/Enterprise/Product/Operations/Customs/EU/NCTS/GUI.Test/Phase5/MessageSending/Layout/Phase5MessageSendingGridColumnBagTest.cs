using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5MessageSendingGridColumnBagTest : TestCase
	{
		public void TestShouldSendCheckBoxColumn()
		{
			AssertNotNull(ColumnBag.ShouldSendCheckBoxColumn);
			var columnInfo = ColumnBag.ShouldSendCheckBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZCheckBozColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCheckBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", false, columnInfo.IsMandatory);
				AssertEquals("ColumnName", NctsHeaderMessageSendingObject.Schema.ShouldSend, columnInfo.ColumnName);
			});
		}

		public void TestLrnTextBoxColumn()
		{
			AssertNotNull(ColumnBag.LrnTextBoxColumn);
			var columnInfo = ColumnBag.LrnTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", true, columnInfo.IsMandatory);
				AssertEquals("ColumnName", "LRN", columnInfo.ColumnName);
			});
		}

		public void TestMrnTextBoxColumn()
		{
			AssertNotNull(ColumnBag.MrnTextBoxColumn);
			var columnInfo = ColumnBag.MrnTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", true, columnInfo.IsMandatory);
				AssertEquals("ColumnName", "MRN", columnInfo.ColumnName);
			});
		}

		public void TestMessageTypeDropEditColumn()
		{
			AssertNotNull(ColumnBag.MessageTypeDropEditColumn);
			var columnInfo = ColumnBag.MessageTypeDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", true, columnInfo.IsMandatory);
				AssertEquals("ColumnName", "MessageType", columnInfo.ColumnName);
			});
		}

		public void TestReleaseRequestDropEditColumn()
		{
			AssertNotNull(ColumnBag.ReleaseRequestDropEditColumn);
			var columnInfo = ColumnBag.ReleaseRequestDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", true, columnInfo.IsMandatory);
				AssertEquals("ColumnName", "ReleaseRequest", columnInfo.ColumnName);
			});
		}

		public void TestJustificationTextBoxColumn()
		{
			AssertNotNull(ColumnBag.JustificationTextBoxColumn);
			var columnInfo = ColumnBag.JustificationTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("Mandatory", true, columnInfo.IsMandatory);
				AssertEquals("ColumnName", "Justification", columnInfo.ColumnName);
			});
		}

		Phase5MessageSendingGridColumnBag ColumnBag => Phase5MessageSendingGridColumnBag.Instance;
	}
}
