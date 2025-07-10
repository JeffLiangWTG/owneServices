using System.Collections.Generic;
using Enterprise.Customs.Common.BE;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControlLayout))]
sealed class EntryInstructionDetailsBasicUserControlLayoutTest : LayoutsAbstractTest
{
	public void TestSubStyleDropEditVisibility() => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new EntryInstructionDetailsBasicUserControlLayout()).Layout;
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = BEJobMessageTypeList.Codes.Export;
		AssertEquals("Visible when JE_MessageType = 'EXP'", true, layout.IsVisible(Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));

		declaration.JE_MessageType = BEJobMessageTypeList.Codes.Import;
		AssertEquals("Visible when JE_ApplicationCode = 'IMP'", true, layout.IsVisible(Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));

		declaration.JE_MessageType = BEJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Visible when JE_ApplicationCode = 'MSC'", true, layout.IsVisible(Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));

		declaration.JE_MessageType = BEJobMessageTypeList.Codes.ReExport;
		AssertEquals("Visible when JE_ApplicationCode = 'REX'", false, layout.IsVisible(Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));

		declaration.JE_MessageType = BEJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("Visible when JE_ApplicationCode = 'EXS'", false, layout.IsVisible(Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));
	});

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.BondHolderOrganisationControl, ControlWidthClass.LongNoCaption);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.RemoverOrganisationControl, ControlWidthClass.LongNoCaption);
		}
	}
}
