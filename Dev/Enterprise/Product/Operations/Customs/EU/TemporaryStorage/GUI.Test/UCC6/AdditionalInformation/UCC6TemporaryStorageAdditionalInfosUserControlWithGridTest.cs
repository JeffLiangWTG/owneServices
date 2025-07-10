using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStorageAdditionalInfosUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
			{
				Assert("EU.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid", control is Integration.Customs.EU.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid);
				AssertNotNull("AdditionalInfosGrid", control.FindSingle<ZGrid>("AdditionalInfosGrid"));
			}
		}

		public void TestSupportingInfoUserControls_GridBindingMember()
		{
			using (var control = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
			{
				AssertEquals("Bills", control.GridBindingMember);
			}
		}

		[RequiresSTA]
		public void TestGridColumnNames_UCC6TemporaryStorageMasterBill_Master()
		{
			AssertGridColumnNames(new UCC6TemporaryStorageMasterBillTestData());
		}

		[RequiresSTA]
		public void TestGridColumnNames_UCC6TemporaryStorageHouseBill_Master()
		{
			AssertGridColumnNames(new UCC6TemporaryStorageHouseBillTestData());
		}

		public void TestGridColumnNames_UCC6TemporaryStorageBillPackedItem()
		{
			AssertGridColumnNames(new UCC6TemporaryStorageBillPackedItemTestData());
		}

		public void TestGridDefaultColumnOrder_UCC6TemporaryStorageMasterBill()
		{
			AssertGridColumnOrder(new UCC6TemporaryStorageMasterBillTestData());
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder_UCC6TemporaryStorageHouseBill()
		{
			AssertGridColumnOrder(new UCC6TemporaryStorageHouseBillTestData());
		}

		public void TestGridDefaultColumnOrder_UCC6TemporaryStorageBillPackedItem()
		{
			AssertGridColumnOrder(new UCC6TemporaryStorageBillPackedItemTestData());
		}

		public void TestGridColumnWidths_UCC6TemporaryStorageMasterBill()
		{
			AssertGridColumnWidths(new UCC6TemporaryStorageMasterBillTestData());
		}

		public void TestGridColumnWidths_UCC6TemporaryStorageHouseBill()
		{
			AssertGridColumnWidths(new UCC6TemporaryStorageHouseBillTestData());
		}

		public void TestGridColumnWidths_UCC6TemporaryStorageBillPackedItem()
		{
			AssertGridColumnWidths(new UCC6TemporaryStorageBillPackedItemTestData());
		}

		public void TestCaptionCustomized()
		{
			var testData = new UCC6TemporaryStorageMasterBillTestData();
			using (var form = new ZForm(testData.GetBindingParent(Factory)))
			using (var control = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "Bills", testData.BindingString);
				form.Controls.Add(control);
				form.Show();

				var groupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
				AssertEquals("GroupBox Caption should be customized for UCC6TemporaryStorageBill AdditionalInformation control", "Additional Information", groupBox.CaptionResourceString.Caption);
			}
		}

		void AssertGridColumnNames(TestData testData)
		{
			using (var frm = new ZForm(testData.GetBindingParent(Factory)))
			using (var control = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "Bills", testData.BindingString);
				frm.Controls.Add(control);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in testData.OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		void AssertGridColumnOrder(TestData testData)
		{
			using (var frm = new ZForm(testData.GetBindingParent(Factory)))
			using (var control = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "Bills", testData.BindingString);
				frm.Controls.Add(control);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				AssertSequencesEqual(testData.OrderedColumnDetails.Select(x => x.ColumnName), grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		void AssertGridColumnWidths(TestData testData)
		{
			using (var control = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "Bills", testData.BindingString);
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in testData.OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		abstract class TestData
		{
			public abstract string BindingString { get; }

			public abstract BusinessObject GetBindingParent(BusinessObjectFactory factory);

			public abstract (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails { get; }
		}

		class UCC6TemporaryStorageMasterBillTestData : TestData
		{
			public override string BindingString => "Bills";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<TemporaryStorageHeader>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Kind", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Type", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530),
			};
		}

		class UCC6TemporaryStorageHouseBillTestData : TestData
		{
			public override string BindingString => "Bills";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<TemporaryStorageHeader>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Kind", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Type", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530),
			};
		}

		class UCC6TemporaryStorageBillPackedItemTestData : TestData
		{
			public override string BindingString => "Bills.PackedItems";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<TemporaryStorageHeader>();

			public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
			{
				(CusSupportingInfo.Schema.CSI_SubType, "Kind", 80),
				(CusSupportingInfo.Schema.CSI_Code, "Type", 80),
				(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
				(CusSupportingInfo.Schema.CSI_Description, "Description", 530),
			};
		}
	}
}
