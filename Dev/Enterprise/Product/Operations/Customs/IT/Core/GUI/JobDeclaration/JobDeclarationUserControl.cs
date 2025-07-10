using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class JobDeclarationUserControl : EU.GUI.EUJobDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		StyleOfEntrySOEDropDown.Visible = false;
		BadgeCodeDropEdit.Visible = false;
		MessageVersionDropEdit.Visible = JobDeclaration?.IsMessageVersionApplicable ?? ZBool.False;
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		if (JobDeclaration != null)
		{
			JobDeclaration.MergeManager.BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments += BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments;
			JobDeclaration.OnGetEntryToPrintSadHC88 += OnGetEntryToPrintSadHC88;
		}
	}

	protected override Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);

	public new JobDeclaration JobDeclaration
	{
		get { return (JobDeclaration)base.JobDeclaration; }
		set { base.JobDeclaration = value; }
	}

	protected override void Dispose(bool disposing)
	{
		if (JobDeclaration != null)
		{
			JobDeclaration.MergeManager.BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments -= BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments;
			JobDeclaration.OnGetEntryToPrintSadHC88 -= OnGetEntryToPrintSadHC88;
		}

		base.Dispose(disposing);
	}

	#region Implementation

	void AskShouldOverrideC100SupportingDocumentRexCodeIfNeeded(IEnumerable<CusEntryLine> entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable)
	{
		if (JobDeclaration.IsMergeDone && entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable.Any(x => x.HasSupportingDocumentC100WithRexCodeDifferentFromSupplier))
		{
			JobDeclaration.ShouldOverrideC100SupportingDocumentRexCode = shouldOverrideC100SupportingDocumentRexCodeAnswer ?? (shouldOverrideC100SupportingDocumentRexCodeAnswer = Globals.Message.Show(Res.GetString("39BC6D40-819E-42D3-86A0-FEB1EBAE4F9E", "Do you want to update the existing Supporting Documents of type C100 with the REX code of the Supplier?")
				, Res.GetString("4727393C-4B6D-4A33-AA53-CEC546390BD3", "Supporting Document C100 Rex Code")
				, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes).Value;
		}
	}

	bool? shouldOverrideC100SupportingDocumentRexCodeAnswer;

	void BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments(object sender, EntryLineDefaultLogicForC100SupportingDocumentsEventArgs ev)
	{
		AskShouldOverrideC100SupportingDocumentRexCodeIfNeeded(ev.EntryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable);
	}

	void OnGetEntryToPrintSadHC88(object sender, System.ComponentModel.CancelEventArgs e)
	{
		using (var sadDocumentSupporterConfiguratorForm = new SadDocumentSupporterConfiguratorForm(JobDeclaration.DocumentSupporter.SadDocumentSupporter))
		{
			e.Cancel = ZFormModaliser.ShowDialogWithoutDispose(sadDocumentSupporterConfiguratorForm) != DialogResult.OK;
		}
	}

	#endregion
}
