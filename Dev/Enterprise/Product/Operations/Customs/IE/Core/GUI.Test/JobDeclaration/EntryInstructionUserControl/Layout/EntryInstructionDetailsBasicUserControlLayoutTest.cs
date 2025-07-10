using System.Collections.Generic;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using EntryInstructionBasicDetailsControlBag = Enterprise.Customs.EU.GUI.EntryInstructionBasicDetailsControlBag;
using JobDeclaration = Enterprise.Customs.IE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsBasicUserControlLayout))]
	sealed class EntryInstructionDetailsBasicUserControlLayoutTest : LayoutsAbstractTest
	{
		public void TestControlVisibility() => CombineAssertions(() =>
		{
			var layout = ((IPanelLayoutProvider)new EntryInstructionDetailsBasicUserControlLayout()).Layout;
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("LocationOfGoodsUserControl Visible when JE_MessageType = 'EXP'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction));
			AssertEquals("NewOwnerOrganisationControl Visible when JE_MessageType = 'EXP'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, entryInstruction));
			AssertEquals("AcceptanceDateEdit Visible when JE_MessageType = 'EXP'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, entryInstruction));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertEquals("LocationOfGoodsUserControl Visible when JE_MessageType = 'IMP'", true, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction));
			AssertEquals("NewOwnerOrganisationControl Visible when JE_MessageType = 'IMP'", true, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, entryInstruction));
			AssertEquals("AcceptanceDateEdit Visible when JE_MessageType = 'IMP'", true, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, entryInstruction));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("LocationOfGoodsUserControl Visible when JE_MessageType = 'MSC'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction));
			AssertEquals("NewOwnerOrganisationControl Visible when JE_MessageType = 'MSC'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, entryInstruction));
			AssertEquals("AcceptanceDateEdit Visible when JE_MessageType = 'MSC'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, entryInstruction));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("LocationOfGoodsUserControl Visible when JE_MessageType = 'REX'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction));
			AssertEquals("NewOwnerOrganisationControl Visible when JE_MessageType = 'REX'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, entryInstruction));
			AssertEquals("AcceptanceDateEdit Visible when JE_MessageType = 'REX'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, entryInstruction));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("LocationOfGoodsUserControl Visible when JE_MessageType = 'EXS'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction));
			AssertEquals("NewOwnerOrganisationControl Visible when JE_MessageType = 'EXS'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, entryInstruction));
			AssertEquals("AcceptanceDateEdit Visible when JE_MessageType = 'EXS'", false, layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, entryInstruction));
		});

		public void TestToWarehouseLabelCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var layout = ((IPanelLayoutProvider)new EntryInstructionDetailsBasicUserControlLayout()).Layout;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);

			CombineAssertions("ToWarehouseLabel Caption", () =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				layout.TryGetCaption(EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, instruction, out var resourceStringData);
				AssertEquals("Caption IMP", "[2/7] To Warehouse", resourceStringData.Caption);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				layout.TryGetCaption(EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, instruction, out var resourceStringDataExport);
				AssertEquals("Caption EXP", "To Warehouse", resourceStringDataExport.Caption);
			});
		}

		public void TestFromWarehouseLabelCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var layout = ((IPanelLayoutProvider)new EntryInstructionDetailsBasicUserControlLayout()).Layout;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);

			CombineAssertions("FromWarehouseLabel Caption", () =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				layout.TryGetCaption(EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, instruction, out var resourceStringData);
				AssertEquals("Caption IMP", "[2/7] From Warehouse", resourceStringData.Caption);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				layout.TryGetCaption(EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, instruction, out var resourceStringDataExport);
				AssertEquals("Caption EXP", "From Warehouse", resourceStringDataExport.Caption);
			});
		}

		public void TestAcceptanceDateControlAlignment()
		{
			var columns = ((IPanelLayoutProvider)new EntryInstructionDetailsBasicUserControlLayout()).Layout.Columns;
			AssertEquals("Columns", 3, columns.Count);
			var column2 = columns[1];
			AssertEquals("Rows in 2nd column", 4, column2.Rows.Count);
			var column2Row4 = column2.Rows[3];
			AssertCollectionContains(EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, column2Row4.Parts);
			AssertEquals("AcceptanceDateEdit aligned to control", EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, column2Row4.AlignToControl);
			AssertEquals("AcceptanceDateEdit vertical alignment", System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom, column2Row4.VerticalAlignment);
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseAddressControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseAddressControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.RequestedDocumentsGroupBox, ControlWidthClass.CustomWidth);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseTypeTextBox, ControlWidthClass.Medium);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseTypeTextBox, ControlWidthClass.Medium);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.AcceptanceDateEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseCodeTextBox, ControlWidthClass.Medium);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseCodeTextBox, ControlWidthClass.Medium);
			}
		}
	}
}
