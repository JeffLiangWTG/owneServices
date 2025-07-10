using System.Collections.Generic;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ImportEntryInstructionDetailsLayout))]
	sealed class ImportEntryInstructionDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestStyleDropEditCaption()
		{
			Layout.TryGetCaption(EntryInstructionDetailsControlBag.Instance.StyleDropEdit, Instruction, out var resourceStringData);
			AssertEquals("Declaration Type", resourceStringData.Caption);
		}

		public void TestSubStyleDropEditCaption()
		{
			Layout.TryGetCaption(EntryInstructionDetailsControlBag.Instance.SubStyleDropEdit, Instruction, out var resourceStringData);
			AssertEquals("Sub Style", resourceStringData.Caption);
		}

		public void TestVisibility()
		{
			CombineAssertions(() =>
			{
				Instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Visible AAV", true, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.AuthorisationNumberDropEdit, Instruction));

				Instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				AssertEquals("Not Visible", false, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.AuthorisationNumberDropEdit, Instruction));

				Instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				AssertEquals("Visible VAV", true, Layout.IsVisible(EntryInstructionDetailsControlBag.Instance.AuthorisationNumberDropEdit, Instruction));
			});
		}
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.AdditionalInfoTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.CPCDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.AuthorisationNumberDropEdit, ControlWidthClass.Long);
			}
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ImportEntryInstructionDetailsLayout()).Layout);
		PanelLayout layout;

		CusEntryInstruction Instruction
		{
			get
			{
				if (instruction == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					instruction = declaration.CustomsEntryInstructions.AddNew();
				}
				return instruction;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionDetailsLayoutBuilder();

		CusEntryInstruction instruction;
	}
}
