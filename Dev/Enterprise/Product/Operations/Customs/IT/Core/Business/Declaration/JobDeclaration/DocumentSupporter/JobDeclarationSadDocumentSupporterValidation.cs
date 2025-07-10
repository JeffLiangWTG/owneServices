using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationSadDocumentSupporterValidation : ZValidation
{
	public JobDeclarationSadDocumentSupporterValidation(JobDeclarationSadDocumentSupporter parent) : base(parent)
	{
		Parent = Argument.NotNull(parent, nameof(parent));
	}
	JobDeclarationSadDocumentSupporter Parent { get; }

	public override Type AutoValidationType => typeof(JobDeclarationSadDocumentSupporterValidation);

	public override void ValidateAll()
	{
		ValidateBGMReferenceToPrint();
		ValidateLayoutStyle();
	}

	public void ValidateBGMReferenceToPrint()
	{
		ValidateCalculatedProperty(Parent.BGMReferenceToPrintInfo);
	}

	protected virtual void CheckBGMReferenceToPrint()
	{
		var bgmReferenceToPrintInfo = Parent.BGMReferenceToPrintInfo;

		ListValidation.ErrorIfInvalidCode(bgmReferenceToPrintInfo);
		MandatoryValidation.CheckEntered(bgmReferenceToPrintInfo);
	}

	public void ValidateLayoutStyle()
	{
		ValidateCalculatedProperty(Parent.LayoutStyleInfo);
	}

	protected virtual void CheckLayoutStyle()
	{
		var layoutStyleInfo = Parent.LayoutStyleInfo;

		ListValidation.ErrorIfInvalidCode(layoutStyleInfo);
		MandatoryValidation.CheckEntered(layoutStyleInfo);
	}
}
