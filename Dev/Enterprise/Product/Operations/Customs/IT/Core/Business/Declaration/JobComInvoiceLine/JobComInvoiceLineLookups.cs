using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{ }

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public new RefCusProcedureCollection CPCList
	{
		get
		{
			var declaration = Parent?.Declaration;
			var messageType = declaration?.JE_MessageType ?? ZString.Empty;
			var procedureCode = Parent?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
			if (!procedureCode.IsEmpty)
			{
				return RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, GlbCompany.CurrentCompany.Country.Code, messageType, procedureCode, ZDateTime.Today);
			}
			else
			{
				return base.CPCList;
			}
		}
	}

	public IEnumerable<ZString> SupplementaryQuantityUOMs
	{
		get
		{
			var universalTariff = Parent.UniversalTariff;
			if (universalTariff == null)
			{
				return Enumerable.Empty<ZString>();
			}
			else
			{
				return Factory.GetCachedValue(FormattableString.Invariant($"UniversalTariff_SupplementaryQuantityUOMs_{Parent.JI_Tariff}"), () =>
				{
					return UniversalReferenceHelper.GetSupplementaryQuantityUOMs(universalTariff);
				});
			}
		}
	}

	public CodeDescriptionPairList ItalyStateList => Factory.GetStateList(Core.Constants.CountryCodes.Italy, hasErrors: false);

	public ICollection CountryOfExportList => Parent.Declaration?.Lookups.GoodsOrigin ?? new CodeDescriptionPairList();

	public override ICodeDescriptionPairList PrimaryPreferenceList => GetPrimaryPreferenceList();

	ICodeDescriptionPairList GetPrimaryPreferenceList()
	{
		var primaryPreferenceList = base.PrimaryPreferenceList;

		if (Parent.IsNonTurkishImportWithTurkishDispatch
			&& GetTurkeyNonImpositionOfCustomsDutyPreference() is ICodeDescription turkeyNonImpositionOfCustomsDutyPreference)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(primaryPreferenceList);
			result.AddPair(turkeyNonImpositionOfCustomsDutyPreference.Code, turkeyNonImpositionOfCustomsDutyPreference.Description);
			result.Sort();

			return result;
		}

		return primaryPreferenceList;
	}

	ICodeDescription GetTurkeyNonImpositionOfCustomsDutyPreference()
	{
		var invoiceLine = Parent;

		return UniversalReferenceDataHelper.GetPreferenceList(
			Factory,
			invoiceLine.UseUniversalTariff,
			invoiceLine.JI_Tariff,
			Core.Constants.CountryCodes.Turkey,
			invoiceLine.UniversalTariff,
			new ItalianRateSelectionCriteriaForTurkey(invoiceLine),
			GetDefaultTariffDataGroupingCode())[UniversalReferenceConstants.RefCusPreferences.NonImpositionOfCustomsDuties];
	}

	sealed class ItalianRateSelectionCriteriaForTurkey : EU.Business.Declaration.JobComInvoiceLine.RateSelectionCriteria<JobComInvoiceLine>
	{
		public ItalianRateSelectionCriteriaForTurkey(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, ZString.Empty, ZString.Empty)
		{
			TradeGroupCountry = Core.Constants.CountryCodes.Turkey;
		}
	}
}
