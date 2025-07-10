using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

public sealed class PreviousDocumentSettingsProvider
{
	public PreviousDocumentSettingsProvider(IPreviousDocumentCombinationsProvider combinationsProvider)
	{
		this.combinationsProvider = Argument.NotNull(combinationsProvider, nameof(combinationsProvider));
	}

	readonly IPreviousDocumentCombinationsProvider combinationsProvider;

	public PreviousDocumentFieldsInfo GetSettings()
	{
		var allowedCombinations = combinationsProvider.GetAllowedPreviousDocumentCombinations();
		var templatesSelected = allowedCombinations.Select(x => x.Template).Distinct().ToArray();

		return templatesSelected.Length == 1
			? combinationsProvider.GetNewSettings(templatesSelected[0])
			: combinationsProvider.GetNewSettings();
	}
}
