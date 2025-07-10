using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PermitValidation : CusSupportingInfoValidation
{
	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent?.Parent?.JobDeclaration));
	PlausiValidation plausiValidation;

	public PermitValidation(Permit parent) : base(parent)
	{
	}

	protected override void CheckCSI_IssuerType()
	{
		base.CheckCSI_IssuerType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_IssuerTypeInfo);
		PlausiValidation.CheckR314(Parent.CSI_IssuerTypeInfo, Parent);
		PlausiValidation.CheckR315(Parent.CSI_IssuerTypeInfo, Parent);
		PlausiValidation.CheckR249c(Parent.CSI_IssuerTypeInfo, Parent);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		if (IsImport)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}

		PlausiValidation.CheckR313(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckR316(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckR249c(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckCH0005(Parent.CSI_CodeInfo, Parent);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}

	protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
	{
		if (IsImport)
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CSI_DateOfIssueInfo, new TypeValidationLimits()
			{
				PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
				PastYearsBeforeWarning = TypeValidationLimits.Default.PastYearsBeforeError,
			});
		}
		else
		{
			base.CheckCSI_DateOfIssueIsValidZDateTimeRange();
		}
	}

	protected new Permit Parent => (Permit)base.Parent;

	bool IsImport => Parent.Parent?.IsImport ?? false;
}
