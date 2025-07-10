using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DeclarationOtherDetailsLayout))]
sealed class DeclarationOtherDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	public void TestControlsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var layout = ((IPanelLayoutProvider)new DeclarationOtherDetailsLayout()).Layout;
		CombineAssertions(() =>
		{
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.OriginStateDropEdit, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.IECCodeTextBox, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.ExporterClassTextBox, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.EPZCodeDropEdit, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.BranchSerialNumberTextBox, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.AuthorizedDealerCodeTextBox, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.TypeOfExporterDropEdit, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.SealByDropEdit, declaration));
			AssertEquals("IsExport", false, layout.IsVisible(CustomsBag.RotationNumberTextBox, declaration));
			AssertEquals("IsExport", false, layout.IsVisible(CustomsBag.RotationDateDateEdit, declaration));
			AssertEquals("IsExport", false, layout.IsVisible(CustomsBag.StuffingAtDropEdit, declaration));
			AssertEquals("IsExport", false, layout.IsVisible(CustomsBag.SampleAccompaniedDropEdit, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.GoodsRegistrationSeparatorUserControl, declaration));
			AssertEquals("IsExport", true, layout.IsVisible(CustomsBag.TranshipperGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.OriginStateDropEdit, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.ExporterClassTextBox, declaration));
			AssertEquals("IsImport", true, layout.IsVisible(CustomsBag.IECCodeTextBox, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.EPZCodeDropEdit, declaration));
			AssertEquals("IsImport", true, layout.IsVisible(CustomsBag.BranchSerialNumberTextBox, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.AuthorizedDealerCodeTextBox, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.TypeOfExporterDropEdit, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.SealByDropEdit, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.RotationNumberTextBox, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.RotationDateDateEdit, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.StuffingAtDropEdit, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.SampleAccompaniedDropEdit, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.GoodsRegistrationSeparatorUserControl, declaration));
			AssertEquals("IsImport", false, layout.IsVisible(CustomsBag.TranshipperGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.OriginStateDropEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.IECCodeTextBox, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.ExporterClassTextBox, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.EPZCodeDropEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.BranchSerialNumberTextBox, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.AuthorizedDealerCodeTextBox, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.TypeOfExporterDropEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.SealByDropEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.RotationNumberTextBox, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.RotationDateDateEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.StuffingAtDropEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.SampleAccompaniedDropEdit, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.GoodsRegistrationSeparatorUserControl, declaration));
			AssertEquals("Not Export and Import", false, layout.IsVisible(CustomsBag.TranshipperGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsSea", true, layout.IsVisible(CustomsBag.SealByDropEdit, declaration));
			AssertEquals("IsSea", true, layout.IsVisible(CustomsBag.RotationNumberTextBox, declaration));
			AssertEquals("IsSea", true, layout.IsVisible(CustomsBag.RotationDateDateEdit, declaration));
			AssertEquals("IsSea", false, layout.IsVisible(CustomsBag.StuffingAtDropEdit, declaration));
			AssertEquals("IsSea", false, layout.IsVisible(CustomsBag.SampleAccompaniedDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsAir", false, layout.IsVisible(CustomsBag.SealByDropEdit, declaration));
			AssertEquals("IsAir", false, layout.IsVisible(CustomsBag.StuffingAtDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			AssertEquals("IsExport, IsSea, IsContainerised", true, layout.IsVisible(CustomsBag.StuffingAtDropEdit, declaration));
			AssertEquals("IsExport, IsSea, IsContainerised", false, layout.IsVisible(CustomsBag.SampleAccompaniedDropEdit, declaration));

			declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertEquals("IsFactory", true, layout.IsVisible(CustomsBag.SampleAccompaniedDropEdit, declaration));
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CustomsBag.IECCodeTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.OriginStateDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.ExporterClassTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.EPZCodeDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.BranchSerialNumberTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.AuthorizedDealerCodeTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.TypeOfExporterDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.SealByDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.RotationNumberTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.RotationDateDateEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.StuffingAtDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.SampleAccompaniedDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.GoodsRegistrationSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (CustomsBag.TranshipperGuidFindBox, ControlWidthClass.Auto);
		}
	}

	DeclarationOtherDetailsControlBag CustomsBag => DeclarationOtherDetailsControlBag.Instance;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DeclarationOtherDetailsLayoutBuilder();
}
