using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
{
	public AdditionalInfoValidation(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		ValidateAdditionalInfoLinesMaxCountInEntryLineIfNecessary();
		CheckCodeAndDescriptionAreEmptyForImport();
	}

	void ValidateAdditionalInfoLinesMaxCountInEntryLineIfNecessary()
	{
		Parent.RemoveRowMessageError(ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowedPerEntryLine);
		Parent.RemoveRowMessageError(ValidationCaptions.AdditionalInfo.Only99LinesOfAdditionalInfoAreAllowedForAnEntryLine);

		var entryLine = InvoiceLine?.CusEntryLine;
		if (InvoiceLine != null && entryLine != null)
		{
			ValidateAdditionalInfoLinesMaxCountWithEntryLine(entryLine);
		}
	}

	void ValidateAdditionalInfoLinesMaxCountWithEntryLine(CusEntryLine entryLine)
	{
		if (DoesDeclarationImport && entryLine.AdditionalInfos.Skip(99).Any())
		{
			Parent.AddRowMessageError(ValidationCaptions.AdditionalInfo.Only99LinesOfAdditionalInfoAreAllowedForAnEntryLine);
		}
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		if (DoesDeclarationExport && Parent.CSI_Description.Length > SADConstants.CustomsFieldMaxLength.AdditionalInfo.DescriptionForExport)
		{
			Parent.CSI_DescriptionInfo.AddWarning(ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(Parent.CSI_DescriptionInfo, SADConstants.CustomsFieldMaxLength.AdditionalInfo.DescriptionForExport));
		}
	}

	protected override bool ShouldCodeBeInTheList => !DoesDeclarationExport;

	protected override bool IsCodeMandatory => !DoesDeclarationImport;

	void CheckCodeAndDescriptionAreEmptyForImport()
	{
		Parent.RemoveRowMessageError(ValidationCaptions.AdditionalInfo.CodeOrDescriptionMustBeFilled);
		if (DoesDeclarationImport && Parent.CSI_Code.IsEmpty && Parent.CSI_Description.IsEmpty)
		{
			Parent.AddRowMessageError(ValidationCaptions.AdditionalInfo.CodeOrDescriptionMustBeFilled);
		}
	}

	ZBool DoesDeclarationExport => InvoiceLine?.Declaration?.IsExport ?? ZBool.False;
	ZBool DoesDeclarationImport => InvoiceLine?.Declaration?.IsImport ?? ZBool.False;
	JobComInvoiceLine InvoiceLine => Parent.Parent as JobComInvoiceLine;
}
