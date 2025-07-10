using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryInstructionBasicDetailsLayout))]
sealed class EntryInstructionBasicDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestSubStyleDropEditVisibility()
	{
		var layout = GetLayout();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals("IsUCC6AndIsExport is true", true, layout.IsVisible(commonBag.SubStyleDropEdit, entryInstruction));
			AssertContainsExactElementsInAnyOrder("Visibility dependencies", new[] { declaration.JE_MessageTypeInfo, declaration.MessageVersionInfo }, layout.GetVisibilityDependencies(commonBag.SubStyleDropEdit, entryInstruction));
		}

		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertEquals("IsUCC6AndIsExport is false", false, layout.IsVisible(commonBag.SubStyleDropEdit, entryInstruction));
	}

	public void TestUseDeclarationOfIntentCheckBoxVisibility()
	{
		var layout = GetLayout();
		var isUseDeclarationOfIntentCheckBoxVisible = layout.IsVisible(itBag.UseDeclarationOfIntentCheckBox, entryInstruction);
		AssertEquals("Visibility", false, isUseDeclarationOfIntentCheckBoxVisible);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		isUseDeclarationOfIntentCheckBoxVisible = layout.IsVisible(itBag.UseDeclarationOfIntentCheckBox, entryInstruction);
		AssertEquals("Visibility", true, isUseDeclarationOfIntentCheckBoxVisible);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		isUseDeclarationOfIntentCheckBoxVisible = layout.IsVisible(itBag.UseDeclarationOfIntentCheckBox, entryInstruction);
		AssertEquals("Visibility", false, isUseDeclarationOfIntentCheckBoxVisible);
	}

	public void TestParticipantTypeDropEditVisibility()
	{
		var layout = GetLayout();
		declaration.JE_MessageType = ZString.Empty;
		var isParticipantTypeDropEditVisible = layout.IsVisible(itBag.ParticipantTypeDropEdit, entryInstruction);
		AssertEquals("Visibility for EMPTY Message Type", false, isParticipantTypeDropEditVisible);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		isParticipantTypeDropEditVisible = layout.IsVisible(itBag.ParticipantTypeDropEdit, entryInstruction);
		AssertEquals("Visibility for EXP", true, isParticipantTypeDropEditVisible);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		isParticipantTypeDropEditVisible = layout.IsVisible(itBag.ParticipantTypeDropEdit, entryInstruction);
		AssertEquals("Visibility for IMP", false, isParticipantTypeDropEditVisible);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			isParticipantTypeDropEditVisible = layout.IsVisible(itBag.ParticipantTypeDropEdit, entryInstruction);
			AssertEquals("Visibility for UCC6 EXP", false, isParticipantTypeDropEditVisible);
		}
	}

	public void TestSimplifiedDecAcceptanceDateEditVisibility()
	{
		var layout = GetLayout();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("CEI_SubStyle is Empty", false, IsSimplifiedDecAcceptanceDateEditVisible());

			entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationX;
			AssertEquals($"CEI_SubStyle = {entryInstruction.CEI_SubStyle}", true, IsSimplifiedDecAcceptanceDateEditVisible());

			entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.StandardDeclarationA;
			AssertEquals($"CEI_SubStyle = {entryInstruction.CEI_SubStyle}", false, IsSimplifiedDecAcceptanceDateEditVisible());

			entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationY;
			AssertEquals($"CEI_SubStyle = {entryInstruction.CEI_SubStyle}", true, IsSimplifiedDecAcceptanceDateEditVisible());

			entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationZ;
			AssertEquals($"CEI_SubStyle = {entryInstruction.CEI_SubStyle}", true, IsSimplifiedDecAcceptanceDateEditVisible());

			entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD;
			AssertEquals($"CEI_SubStyle = {entryInstruction.CEI_SubStyle}", false, IsSimplifiedDecAcceptanceDateEditVisible());
		});

		bool IsSimplifiedDecAcceptanceDateEditVisible() => layout.IsVisible(itBag.SimplifiedDecAcceptanceDateEdit, entryInstruction);
	}

	public void TestSimplifiedDecAcceptanceDateEditVisibility_WhenDeclarationIsUCC6AndExport()
	{
		var layout = GetLayout();
		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Dec is EXP UCC6", () =>
			{
				entryInstruction.CEI_Style = "B1";
				entryInstruction.CEI_SubStyle = "X";
				AssertEquals("Style: not C1 and SubStyle: X. Is SimplifiedDecAcceptanceDateEdit visible?", true, IsSimplifiedDecAcceptanceDateEditVisible());

				entryInstruction.CEI_Style = "C1";
				AssertEquals("Style: C1 and SubStyle: X. Is SimplifiedDecAcceptanceDateEdit visible?", false, IsSimplifiedDecAcceptanceDateEditVisible());
			});
		}

		bool IsSimplifiedDecAcceptanceDateEditVisible() => layout.IsVisible(itBag.SimplifiedDecAcceptanceDateEdit, entryInstruction);
	}

	public void TestPreviousInvoiceControlsVisibility()
	{
		entryInstruction.ZG_ParticipantType = ZString.Empty;
		AssertPreviousInvoiceControlsVisibility("Empty ZG_ParticipantType", false);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		AssertPreviousInvoiceControlsVisibility("ZG_ParticipantType = BUY", false);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		AssertPreviousInvoiceControlsVisibility("ZG_ParticipantType = TRG", true);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		AssertPreviousInvoiceControlsVisibility("ZG_ParticipantType = JTD", true);
	}

	public void TestPresentationOfGoodsDateEditVisibility()
	{
		var layout = GetLayout();
		var isPresentationOfGoodsDateEditVisible = layout.IsVisible(itBag.PresentationOfGoodsDateEdit, entryInstruction);
		AssertEquals("[PRE-CONDITION] Visibility", false, isPresentationOfGoodsDateEditVisible);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			isPresentationOfGoodsDateEditVisible = layout.IsVisible(itBag.PresentationOfGoodsDateEdit, entryInstruction);
			AssertEquals("When declaration is export - UCC6", true, isPresentationOfGoodsDateEditVisible);

			declaration.JE_MessageType = "IMP";
			isPresentationOfGoodsDateEditVisible = layout.IsVisible(itBag.PresentationOfGoodsDateEdit, entryInstruction);
			AssertEquals("When declaration is import - UCC6", false, isPresentationOfGoodsDateEditVisible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = "EXP";
			isPresentationOfGoodsDateEditVisible = layout.IsVisible(itBag.PresentationOfGoodsDateEdit, entryInstruction);
			AssertEquals("When declaration is export - Non-UCC6", false, isPresentationOfGoodsDateEditVisible);

			declaration.JE_MessageType = "IMP";
			isPresentationOfGoodsDateEditVisible = layout.IsVisible(itBag.PresentationOfGoodsDateEdit, entryInstruction);
			AssertEquals("When declaration is import - Non-UCC6", false, isPresentationOfGoodsDateEditVisible);
		}
	}

	public void TestWarehouseControlsVisibility()
	{
		var layout = GetLayout();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions("IMP Controls Visibility", () =>
		{
			AssertEquals("IT-ToWarehouseUserControl", true, layout.IsVisible(itBag.ToWarehouseUserControl, entryInstruction));
			AssertEquals("IT-FromWarehouseUserControl", true, layout.IsVisible(itBag.FromWarehouseUserControl, entryInstruction));

			AssertEquals("EU-ToWarehouseUserControl", false, layout.IsVisible(euBag.ToWarehouseUserControl, entryInstruction));
			AssertEquals("EU-FromWarehouseUserControl", false, layout.IsVisible(euBag.FromWarehouseUserControl, entryInstruction));
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		CombineAssertions("EXP Non Ucc6 Controls Visibility", () =>
		{
			AssertEquals("IT-ToWarehouseUserControl", false, layout.IsVisible(itBag.ToWarehouseUserControl, entryInstruction));
			AssertEquals("IT-FromWarehouseUserControl", false, layout.IsVisible(itBag.FromWarehouseUserControl, entryInstruction));

			AssertEquals("EU-ToWarehouseUserControl", true, layout.IsVisible(euBag.ToWarehouseUserControl, entryInstruction));
			AssertEquals("EU-FromWarehouseUserControl", true, layout.IsVisible(euBag.FromWarehouseUserControl, entryInstruction));
		});

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("EXP Ucc6 Controls Visibility", () =>
			{
				AssertEquals("IT-ToWarehouseUserControl", true, layout.IsVisible(itBag.ToWarehouseUserControl, entryInstruction));
				AssertEquals("IT-FromWarehouseUserControl", true, layout.IsVisible(itBag.FromWarehouseUserControl, entryInstruction));

				AssertEquals("EU-ToWarehouseUserControl", false, layout.IsVisible(euBag.ToWarehouseUserControl, entryInstruction));
				AssertEquals("EU-FromWarehouseUserControl", false, layout.IsVisible(euBag.FromWarehouseUserControl, entryInstruction));
			});
		}
	}

	public void TestElectronicDocumentsUploadRequiredControlVisibility()
	{
		var layout = GetLayout();

		SetMessageTypeAndAssertElectronicsDocumentUploadRquiredCheckBoxIsVisible(EUJobMessageTypeList.Codes.Export, true);
		SetMessageTypeAndAssertElectronicsDocumentUploadRquiredCheckBoxIsVisible(EUJobMessageTypeList.Codes.MiscellaneousCustoms, true);
		SetMessageTypeAndAssertElectronicsDocumentUploadRquiredCheckBoxIsVisible(EUJobMessageTypeList.Codes.Import, false);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			SetMessageTypeAndAssertElectronicsDocumentUploadRquiredCheckBoxIsVisible(EUJobMessageTypeList.Codes.Export, false);
		}

		void SetMessageTypeAndAssertElectronicsDocumentUploadRquiredCheckBoxIsVisible(string declarationType, bool expectedIsVisible)
		{
			declaration.JE_MessageType = declarationType;
			var isVisible = layout.IsVisible(itBag.BoxElectronicDocumentsCheckBox, entryInstruction);
			AssertEquals($"When Declaration.JE_MessageType={declarationType}", expectedIsVisible, isVisible);
		}
	}

	public void TestFinancialAndBankingDataControlsVisibility()
	{
		var layout = GetLayout();

		SetMessageTypeAndAssertFinancialAndBaningControlVisibility(EUJobMessageTypeList.Codes.Export, true);
		SetMessageTypeAndAssertFinancialAndBaningControlVisibility(EUJobMessageTypeList.Codes.MiscellaneousCustoms, true);
		SetMessageTypeAndAssertFinancialAndBaningControlVisibility(EUJobMessageTypeList.Codes.Import, false);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			SetMessageTypeAndAssertFinancialAndBaningControlVisibility(EUJobMessageTypeList.Codes.Export, false);
		}

		void SetMessageTypeAndAssertFinancialAndBaningControlVisibility(string declarationType, bool expectedIsVisible)
		{
			declaration.JE_MessageType = declarationType;

			CombineAssertions($"When Declaration.JE_MessageType={declarationType}", () =>
			{
				AssertEquals("FinancialAndBankingDataLabel", expectedIsVisible, layout.IsVisible(itBag.FinancialAndBankingDataLabel, entryInstruction));
				AssertEquals("FinancialAndBankingDataLine1TextBox", expectedIsVisible, layout.IsVisible(itBag.FinancialAndBankingDataLine1TextBox, entryInstruction));
				AssertEquals("FinancialAndBankingDataLine2TextBox", expectedIsVisible, layout.IsVisible(itBag.FinancialAndBankingDataLine2TextBox, entryInstruction));
			});
		}
	}

	public void TestOtherCustomsInformationLabelVisibility()
	{
		var layout = GetLayout();

		SetMessageTypeAndAssertOtherCustomsInformationLabelVisibility(EUJobMessageTypeList.Codes.Export, true);
		SetMessageTypeAndAssertOtherCustomsInformationLabelVisibility(EUJobMessageTypeList.Codes.MiscellaneousCustoms, true);
		SetMessageTypeAndAssertOtherCustomsInformationLabelVisibility(EUJobMessageTypeList.Codes.Import, true);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			SetMessageTypeAndAssertOtherCustomsInformationLabelVisibility(EUJobMessageTypeList.Codes.Export, false);
		}

		void SetMessageTypeAndAssertOtherCustomsInformationLabelVisibility(string declarationType, bool expectedIsVisible)
		{
			declaration.JE_MessageType = declarationType;
			var isVisible = layout.IsVisible(itBag.OtherCustomsInformationLabel, entryInstruction);
			AssertEquals($"When Declaration.JE_MessageType={declarationType}", expectedIsVisible, isVisible);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
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

	void AssertPreviousInvoiceControlsVisibility(string criteriaMessage, bool isVisible)
	{
		var layout = GetLayout();
		var isPrevInvoiceLabelVisible = layout.IsVisible(itBag.PreviousInvoiceLabel, entryInstruction);
		var isPrevInvoiceCurryExRateCalcVisible = layout.IsVisible(itBag.PreviousInvoiceCurrencyExRateCalcEdit, entryInstruction);
		var isPrevInvoiceCurrencyControlVisible = layout.IsVisible(itBag.PreviousInvoiceAmountBoundCurrencyUserControl, entryInstruction);
		CombineAssertions(criteriaMessage, () =>
		{
			AssertEquals("Label", isVisible, isPrevInvoiceLabelVisible);
			AssertEquals("RateCalEdit", isVisible, isPrevInvoiceCurryExRateCalcVisible);
			AssertEquals("AmountBoundCurrencyUserControl", isVisible, isPrevInvoiceCurrencyControlVisible);
		});
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (commonBag.DetailsLabel, ControlWidthClass.LongNoCaption);
			yield return (commonBag.StyleDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.SubStyleDropEdit, ControlWidthClass.Auto);
			yield return (itBag.ProcedureCodeDropEdit, ControlWidthClass.Auto);
			yield return (itBag.TempProcLimitDateEdit, ControlWidthClass.Auto);
			yield return (itBag.SimplifiedDecAcceptanceDateEdit, ControlWidthClass.Auto);
			yield return (itBag.PresentationOfGoodsDateEdit, ControlWidthClass.Auto);
			yield return (itBag.ParticipantTypeDropEdit, ControlWidthClass.Auto);
			yield return (itBag.OtherCustomsInformationLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.BoxElectronicDocumentsCheckBox, ControlWidthClass.LongNoCaption);
			yield return (itBag.UseDeclarationOfIntentCheckBox, ControlWidthClass.LongNoCaption);
			yield return (commonBag.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (itBag.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (euBag.ToWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (itBag.ToWarehouseUserControl, ControlWidthClass.LongControl);
			yield return (itBag.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (euBag.FromWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (itBag.FromWarehouseUserControl, ControlWidthClass.LongControl);
			yield return (itBag.FinancialAndBankingDataLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.FinancialAndBankingDataLine1TextBox, ControlWidthClass.LongNoCaption);
			yield return (itBag.FinancialAndBankingDataLine2TextBox, ControlWidthClass.LongNoCaption);
			yield return (itBag.PreviousInvoiceLabel, ControlWidthClass.LongNoCaption);
			yield return (itBag.PreviousInvoiceCurrencyExRateCalcEdit, ControlWidthClass.Auto);
			yield return (itBag.PreviousInvoiceAmountBoundCurrencyUserControl, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (itBag.AssessmentDateEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (itBag.IncotermTextBox, ControlWidthClass.Auto);
			yield return (itBag.ValuationCodeTextBox, ControlWidthClass.Auto);
			yield return (itBag.CurrencyTextBox, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 3;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

	EU.GUI.EntryInstructionBasicDetailsControlBag euBag => EU.GUI.EntryInstructionBasicDetailsControlBag.Instance;

	Customs.GUI.EntryInstructionBasicDetailsControlBag commonBag => Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance;

	EntryInstructionBasicDetailsControlBag itBag => EntryInstructionBasicDetailsControlBag.Instance;

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	PanelLayout GetLayout() => GetPanelLayoutProvider().Layout;

	IPanelLayoutProvider GetPanelLayoutProvider() => new EntryInstructionBasicDetailsLayout();
}
