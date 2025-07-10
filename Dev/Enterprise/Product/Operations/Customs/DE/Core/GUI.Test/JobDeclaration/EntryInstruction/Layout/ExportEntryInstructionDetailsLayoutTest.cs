using System.Collections.Generic;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExportEntryInstructionDetailsLayout))]
	sealed class ExportEntryInstructionDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestStyleDropEditCaption()
		{
			Layout.TryGetCaption(EntryInstructionDetailsControlBag.Instance.StyleDropEdit, Instruction, out var resourceStringData);
			AssertEquals("Type (Procedure)", resourceStringData.Caption);
		}

		public void TestSubStyleDropEditCaption()
		{
			Layout.TryGetCaption(EntryInstructionDetailsControlBag.Instance.SubStyleDropEdit, Instruction, out var resourceStringData);
			AssertEquals("Type (Time)", resourceStringData.Caption);
		}

		protected override int ControlBagCount => 2;

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
				yield return (EntryInstructionDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.PartyConstellationDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.DateForDutyDateEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionDetailsControlBag.Instance.ExitDateDateEdit, ControlWidthClass.Auto);
			}
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ExportEntryInstructionDetailsLayout()).Layout);
		PanelLayout layout;

		CusEntryInstruction Instruction
		{
			get
			{
				if (instruction == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					instruction = declaration.CustomsEntryInstructions.AddNew();
				}
				return instruction;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionDetailsLayoutBuilder();

		CusEntryInstruction instruction;
	}
}
