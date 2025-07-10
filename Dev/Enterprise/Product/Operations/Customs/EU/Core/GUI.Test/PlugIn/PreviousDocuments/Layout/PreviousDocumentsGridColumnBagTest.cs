using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PreviousDocumentsGridColumnBagTest : TestCase
	{
		public void TestCodeDropEditColumn()
		{
			var columnInfo = ColumnsBag.CodeDropEditColumn?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.CodeDropEditColumn);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_Code, columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestCodeDescriptionColumn()
		{
			var columnInfo = ColumnsBag.CodeDescriptionColumn?.CreateGridColumnInfo();

			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.CodeDescriptionColumn);
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_CodeDescription, columnInfo.ColumnName);
				Assert("IsReadOnly", columnInfo.IsReadOnly);
			});
		}

		public void TestSubTypeColumn()
		{
			var columnInfo = ColumnsBag.SubTypeColumn?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.SubTypeColumn);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_SubType, columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestReferenceNumberColumn()
		{
			var columnInfo = ColumnsBag.ReferenceNumberColumn?.CreateGridColumnInfo();

			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.ReferenceNumberColumn);
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_ReferenceNumber, columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestDateOfIssueColumn()
		{
			var columnInfo = ColumnsBag.DateOfIssueColumn?.CreateGridColumnInfo();

			CombineAssertions("ZDateEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.DateOfIssueColumn);
				AssertType<ZDateEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_DateOfIssue, columnInfo.ColumnName);
			});
		}

		public void TestLineNoColumn()
		{
			var columnInfo = ColumnsBag.LineNoColumn?.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;

			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.LineNoColumn);
				AssertNull(columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_LineNo, columnInfo.ColumnName);
				AssertEquals("CaptionResourceString", NoResourceStringData.GetData("Line No."), columnInfo.CaptionResourceString);
				AssertEquals("MaxValue", 99999m, columnInfo.MaxValue);
			});
		}

		public void TestProcedureColumn()
		{
			var columnInfo = ColumnsBag.ProcedureColumn?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.ProcedureColumn);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_Procedure, columnInfo.ColumnName);
			});
		}

		public void TestReferenceNumber2Column()
		{
			var columnInfo = ColumnsBag.ReferenceNumber2Column?.CreateGridColumnInfo();

			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.ReferenceNumber2Column);
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_ReferenceNumber2, columnInfo.ColumnName);
			});
		}

		public void TestCustomsOfficeColumn()
		{
			var columnInfo = ColumnsBag.CustomsOfficeColumn?.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;

			CombineAssertions("ZCodeFindBoxColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.CustomsOfficeColumn);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_CustomsOffice, columnInfo.ColumnName);
				AssertEquals("ColumnName", ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, columnInfo.ModuleID);
			});
		}

		public void TestQuantityColumn()
		{
			var columnInfo = ColumnsBag.QuantityColumn?.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;

			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.QuantityColumn);
				AssertNull(columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_Quantity, columnInfo.ColumnName);
			});
		}

		public void TestUnitOfQuantityColumn()
		{
			var columnInfo = ColumnsBag.UnitOfQuantityColumn?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.UnitOfQuantityColumn);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_UnitOfQuantity, columnInfo.ColumnName);
			});
		}

		public void TestQuantity3Column()
		{
			var columnInfo = ColumnsBag.Quantity3Column?.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;

			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.Quantity3Column);
				AssertNull(columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_Quantity3, columnInfo.ColumnName);
			});
		}

		public void TestUnitOfQuantity3Column()
		{
			var columnInfo = ColumnsBag.UnitOfQuantity3Column?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.UnitOfQuantity3Column);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_UnitOfQuantity3, columnInfo.ColumnName);
			});
		}

		public void TestPackQtyColumn()
		{
			var columnInfo = ColumnsBag.PackQtyColumn?.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;

			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.PackQtyColumn);
				AssertNull(columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_PackQty, columnInfo.ColumnName);
				AssertEquals("MaxValue", 99999999m, columnInfo.MaxValue);
			});
		}

		public void TestPackTypeColumn()
		{
			var columnInfo = ColumnsBag.PackTypeColumn?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.PackTypeColumn);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_PackType, columnInfo.ColumnName);
			});
		}

		public void TestItemNumberColumn()
		{
			var columnInfo = ColumnsBag.ItemNumberColumn?.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;

			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.ItemNumberColumn);
				AssertNull(columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_ItemNumber, columnInfo.ColumnName);
				AssertEquals("CaptionResourceString", NoResourceStringData.GetData("Item No."), columnInfo.CaptionResourceString);
			});
		}

		public void TestQuantity2Column()
		{
			var columnInfo = ColumnsBag.Quantity2Column?.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;

			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.Quantity2Column);
				AssertNull(columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_Quantity2, columnInfo.ColumnName);
			});
		}

		public void TestUnitOfQuantity2Column()
		{
			var columnInfo = ColumnsBag.UnitOfQuantity2Column?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.UnitOfQuantity2Column);
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_UnitOfQuantity2, columnInfo.ColumnName);
			});
		}

		public void TestCountryCodeColumn()
		{
			var columnInfo = ColumnsBag.CountryCodeColumn?.CreateGridColumnInfo();
			CombineAssertions("ZCodeFindBoxColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.CountryCodeColumn);
				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_RN_NKCountryCode, columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		PreviousDocumentsGridColumnBag ColumnsBag => PreviousDocumentsGridColumnBag.Instance;
	}
}
