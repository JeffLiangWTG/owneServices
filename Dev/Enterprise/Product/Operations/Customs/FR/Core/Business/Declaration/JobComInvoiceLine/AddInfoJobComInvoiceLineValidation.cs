using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

		protected new AddInfoJobComInvoiceLineLookups Lookups => Parent.Lookups;
		JobComInvoiceLine JobComInvoiceLine => (JobComInvoiceLine)Parent.Parent;

		protected override void CheckZG_BypassCode()
		{
			base.CheckZG_BypassCode();
			ListValidation.MessageErrorIfInvalidCode(JobComInvoiceLine.JI_TariffBypassCodeInfo, Lookups.TariffBypassCodeList, ListValidation.InvalidCodeMessageError);

			if (JobComInvoiceLine.JI_TariffBypassCode != ZString.Empty && JobComInvoiceLine.EntryInstruction != null && JobComInvoiceLine.EntryInstruction.ZG_BypassCode == ZString.Empty)
			{
				JobComInvoiceLine.JI_TariffBypassCodeInfo.AddMessageError(missingValuationBypassCode);
			}
		}

		protected override void CheckZG_BypassReason()
		{
			base.CheckZG_BypassReason();
			if (JobComInvoiceLine.JI_TariffBypassCode == TariffBypassCodeList.Codes.TariffBypass_E)
			{
				MandatoryValidation.MessageErrorIfNotEntered(JobComInvoiceLine.JI_TariffBypassReasonInfo);
			}
		}

		protected override void CheckZG_CountryOfSupply()
		{
			base.CheckZG_CountryOfSupply();

			var invoiceLine = JobComInvoiceLine;
			if (invoiceLine.ZG_CountryOfSupply.IsEmpty && (invoiceLine.Declaration?.IsDeltaC ?? false) && (invoiceLine.Declaration?.IsImport ?? false))
			{
				MandatoryValidation.MessageErrorIfNotEntered(invoiceLine.ZG_CountryOfSupplyInfo);
			}
			CheckRuleC0699_N01();
			CheckRuleC0699_N02();
		}

		void CheckRuleC0699_N01()
		{
			var invoiceLine = Parent.Parent;
			if (invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleC0699_N01Active: true })
			{
				var primaryPreference = invoiceLine.JI_PrimaryPreference;
				if ((primaryPreference.StartsWith(FRConstants.Preferences.Prefixes._2) || primaryPreference.StartsWith(FRConstants.Preferences.Prefixes._3)) && invoiceLine.ZG_CountryOfSupply.IsEmpty)
				{
					invoiceLine.ZG_CountryOfSupplyInfo.AddMessageError(Res.GetString("BBD5340E-9EDA-4A6B-AF46-197880B667AB", "[C0699_N01] If Pref. Code starts with 2 or 3 then the field Pref.Origin is mandatory."));
				}
			}
		}

		void CheckRuleC0699_N02()
		{
			var invoiceLine = Parent.Parent;
			if (invoiceLine.Validation.ValidationDecider is IImportInvoiceLineValidationDecider { IsRuleC0699_N02Active: true })
			{
				var primaryPreference = invoiceLine.JI_PrimaryPreference;
				if (!primaryPreference.StartsWith(FRConstants.Preferences.Prefixes._2) && !primaryPreference.StartsWith(FRConstants.Preferences.Prefixes._3) && !invoiceLine.ZG_CountryOfSupply.IsEmpty)
				{
					invoiceLine.ZG_CountryOfSupplyInfo.AddWarning(Res.GetString("EC313A80-141D-4BC3-97B2-95BAD669ED1E", "[C0699_N02] Because Pref. Code does not start with 2 or 3, the Pref. Origin country will not be sent in message to Customs."));
				}
			}
		}

		static string missingValuationBypassCode => Res.GetString("84D495F5-671B-463A-8D33-AF00EF08AB1C", "Please set a value for declaration valuation bypass first.");
	}
}
