using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public sealed class ExportUcc6AdditionalProcedureCodeValidation : AdditionalProcedureCodeValidation
{
	public ExportUcc6AdditionalProcedureCodeValidation(AdditionalProcedureCode parent) : base(parent)
	{
	}

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		var parent = Parent;
		var code = parent.CY_Code;

		if (!code.IsEmpty)
		{
			var additionalProcedureParent = parent.Parent;
			var codeInfo = parent.CY_CodeInfo;

			ListValidation.MessageErrorIfInvalidCode(codeInfo);

			CheckTransitionPeriod(codeInfo, additionalProcedureParent);
			additionalProcedureParent.AdditionalProcedureCodesAsStringInfo.AddAllNotificationsFrom(Parent.CY_CodeInfo);
		}
	}

	#region Implementation

	void CheckTransitionPeriod(ZPropertyInfo codeInfo, IAdditionalProcedureParent additionalProcedureParent)
	{
		if (additionalProcedureParent is JobComInvoiceLine invoiceLine && IsTransitionPeriod())
		{
			codeInfo.AddMessageError(ValidationCaptions.AdditionalProcedureCode.MustBeEmptyInTransitionPeriod);
		}

		bool IsTransitionPeriod() => invoiceLine.Declaration?.IsTransitionPeriodAES30 ?? false;
	}

	#endregion
}
