using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryInstructionBasicDetailsLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
		var commonBag = builder.CommonBag;
		var itBag = EntryInstructionBasicDetailsControlBag.Instance;
		var euBag = EU.GUI.EntryInstructionBasicDetailsControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(commonBag.DetailsLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(commonBag.SubStyleDropEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.ProcedureCodeDropEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.TempProcLimitDateEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.SimplifiedDecAcceptanceDateEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.PresentationOfGoodsDateEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.ParticipantTypeDropEdit, widthClass: ControlWidthClass.Auto);

		builder.Add(itBag.OtherCustomsInformationLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.BoxElectronicDocumentsCheckBox, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.UseDeclarationOfIntentCheckBox, widthClass: ControlWidthClass.LongNoCaption);

		builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongControl);
		builder.Add(itBag.FromWarehouseLabel, alignToControl: itBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongControl);

		builder.Add(itBag.FinancialAndBankingDataLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.FinancialAndBankingDataLine1TextBox, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.FinancialAndBankingDataLine2TextBox, widthClass: ControlWidthClass.LongNoCaption);

		builder.Add(itBag.PreviousInvoiceLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(itBag.PreviousInvoiceCurrencyExRateCalcEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.PreviousInvoiceAmountBoundCurrencyUserControl, widthClass: ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(itBag.AssessmentDateEdit, alignToControl: commonBag.StyleDropEdit, widthClass: ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(itBag.IncotermTextBox, alignToControl: itBag.AssessmentDateEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.ValuationCodeTextBox, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.CurrencyTextBox, widthClass: ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.SubStyleDropEdit, isVisible: i => i.JobDeclaration?.IsUCC6AndIsExport ?? false, dependencies: GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(itBag.UseDeclarationOfIntentCheckBox, isVisible: i => i?.JobDeclaration?.IsImport ?? false, dependencies: i => i.JobDeclaration?.JE_MessageTypeInfo);
		builder.SetVisibility(itBag.ParticipantTypeDropEdit, isVisible: IsParticipantTypeVisible, i => i.JobDeclaration?.JE_MessageTypeInfo, i => i.JobDeclaration?.MessageVersionInfo);
		builder.SetVisibility(itBag.SimplifiedDecAcceptanceDateEdit, isVisible: i => i.CanSetSimplifiedDecAcceptanceDate, GetSimplifiedDecAcceptanceDateEditDependencies().ToArray());
		builder.SetVisibility(itBag.PreviousInvoiceLabel, isVisible: i => i.IsTriangulationOrJointDeclaration, dependencies: i => i.ZG_ParticipantTypeInfo);
		builder.SetVisibility(itBag.PreviousInvoiceCurrencyExRateCalcEdit, isVisible: i => i.IsTriangulationOrJointDeclaration, dependencies: i => i.ZG_ParticipantTypeInfo);
		builder.SetVisibility(itBag.PreviousInvoiceAmountBoundCurrencyUserControl, isVisible: i => i.IsTriangulationOrJointDeclaration, dependencies: i => i.ZG_ParticipantTypeInfo);
		builder.SetVisibility(itBag.PresentationOfGoodsDateEdit, isVisible: i => i.JobDeclaration?.IsUCC6AndIsExport ?? false, dependencies: i => i.JobDeclaration?.JE_MessageTypeInfo);

		builder.SetVisibility(itBag.FromWarehouseUserControl, isVisible: IsUcc6Declaration, GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(itBag.ToWarehouseUserControl, isVisible: IsUcc6Declaration, GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(euBag.FromWarehouseUserControl, isVisible: i => !IsUcc6Declaration(i), GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(euBag.ToWarehouseUserControl, isVisible: i => !IsUcc6Declaration(i), GetDependenciesForUcc6Property().ToArray());

		builder.SetVisibility(itBag.OtherCustomsInformationLabel, isVisible: i => !(i?.JobDeclaration?.IsUCC6AndIsExport ?? false), GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(itBag.BoxElectronicDocumentsCheckBox, isVisible: i => !IsUcc6Declaration(i), GetDependenciesForUcc6Property().ToArray());

		builder.SetVisibility(itBag.FinancialAndBankingDataLabel, isVisible: i => !IsUcc6Declaration(i), GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(itBag.FinancialAndBankingDataLine1TextBox, isVisible: i => !IsUcc6Declaration(i), GetDependenciesForUcc6Property().ToArray());
		builder.SetVisibility(itBag.FinancialAndBankingDataLine2TextBox, isVisible: i => !IsUcc6Declaration(i), GetDependenciesForUcc6Property().ToArray());

		return builder.Build();

		bool IsUcc6Declaration(CusEntryInstruction entryInstruction) => entryInstruction.JobDeclaration?.IsUCC6 ?? false;

		IEnumerable<Func<CusEntryInstruction, ZPropertyInfo>> GetDependenciesForUcc6Property()
		{
			yield return i => i.JobDeclaration?.JE_MessageTypeInfo;
			yield return i => i.JobDeclaration?.MessageVersionInfo;
		}

		IEnumerable<Func<CusEntryInstruction, ZPropertyInfo>> GetSimplifiedDecAcceptanceDateEditDependencies()
		{
			yield return i => i.CEI_StyleInfo;
			yield return i => i.CEI_SubStyleInfo;
			yield return i => i.JobDeclaration?.JE_MessageTypeInfo;
			yield return i => i.JobDeclaration?.MessageVersionInfo;
		}
	}

	bool IsParticipantTypeVisible(CusEntryInstruction i)
	{
		var jobDeclaration = i?.JobDeclaration;
		if (jobDeclaration == null)
		{
			return false;
		}

		return jobDeclaration.IsExport && !jobDeclaration.IsUCC6AndIsExport;
	}
}
