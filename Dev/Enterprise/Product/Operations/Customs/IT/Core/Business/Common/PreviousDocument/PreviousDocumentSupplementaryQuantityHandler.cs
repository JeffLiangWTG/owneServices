using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business;

public class PreviousDocumentSupplementaryQuantityHandler
{
	public PreviousDocumentSupplementaryQuantityHandler(IPreviousDocumentUniversalTariffProvider previousDocumentUniversalTariffProvider, BusinessObjectFactory factory)
	{
		this.previousDocumentUniversalTariffProvider = Argument.NotNull(previousDocumentUniversalTariffProvider, nameof(previousDocumentUniversalTariffProvider));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly IPreviousDocumentUniversalTariffProvider previousDocumentUniversalTariffProvider;
	readonly BusinessObjectFactory factory;

	protected TariffView UniversalTariff
	{
		get
		{
			TariffView universalTariff = null;
			var tariffCode = previousDocumentUniversalTariffProvider.TariffCode;
			var universalTariffType = previousDocumentUniversalTariffProvider.UniversalTariffType;
			if (!tariffCode.IsEmpty && !universalTariffType.IsEmpty)
			{
				universalTariff = new TariffView.Loader(factory).LoadLatestCachedTariff(Core.Constants.CountryCodes.Italy, universalTariffType, tariffCode);
			}
			return universalTariff;
		}
	}

	public void ValidateQuantity2()
	{
		if (CustomsRulesProvider.IsPreviousProcedureDocument(previousDocumentUniversalTariffProvider.Procedure) && SupplementaryQuantityUOMs.Any())
		{
			MandatoryValidation.MessageErrorIfNotEntered(previousDocumentUniversalTariffProvider.Quantity2Info);
		}
	}

	public IEnumerable<ZString> SupplementaryQuantityUOMs => factory.GetCachedValue(GetSupplementaryQuantityUOMsCacheKey(), () => GetSupplementaryQuantityUOMs());

	#region Implementation

	string GetSupplementaryQuantityUOMsCacheKey() => FormattableString.Invariant($"UniversalTariff_SupplementaryQuantityUOMs_{previousDocumentUniversalTariffProvider.FormattedTariff}");

	IEnumerable<ZString> GetSupplementaryQuantityUOMs()
	{
		var universalTariff = GetUniversalTariffView();
		if (universalTariff != null)
		{
			return UniversalReferenceHelper.GetSupplementaryQuantityUOMs(universalTariff);
		}
		return Enumerable.Empty<ZString>();
	}

	TariffView GetUniversalTariffView()
	{
		var tariffCode = previousDocumentUniversalTariffProvider.TariffCode;
		var universalTariffType = previousDocumentUniversalTariffProvider.UniversalTariffType;

		if (!tariffCode.IsEmpty && !universalTariffType.IsEmpty)
		{
			return new TariffView.Loader(factory).LoadLatestCachedTariff(Core.Constants.CountryCodes.Italy, universalTariffType, tariffCode);
		}
		return null;
	}

	#endregion
}
