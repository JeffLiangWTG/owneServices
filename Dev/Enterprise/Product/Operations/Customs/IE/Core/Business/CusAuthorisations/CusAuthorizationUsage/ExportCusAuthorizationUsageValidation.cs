using CargoWise.Common;

namespace Enterprise.Customs.IE.Business
{
	public sealed class ExportCusAuthorizationUsageValidation : CusAuthorizationUsageValidation
	{
		public ExportCusAuthorizationUsageValidation(CusAuthorizationUsage parent, IExportCusAuthorizationUsageValidationStrategy validationStrategy)
			: base(parent)
		{
			ValidationStrategy = Argument.NotNull(validationStrategy, nameof(validationStrategy));
		}

		public static class MessageError
		{
			public static string CodeCclNotAllowedForSubtypeBOrE => Res.GetString("CC9899A0-7ABE-4802-9DF7-2C573C0F3CB3", "Code 'CCL' is not allowed when Subtype in B,E");
		}

		public IExportCusAuthorizationUsageValidationStrategy ValidationStrategy { get; }

		protected override void CheckAGC_Code()
		{
			base.CheckAGC_Code();
			var parent = Parent;
			var applicableRules = ValidationStrategy.GetRulesApplicableToCode(parent.AGC_Code);
			if (applicableRules.Length > 0)
			{
				applicableRules.ForEach(parent.AGC_CodeInfo.AddMessageError);
			}
		}
	}
}
