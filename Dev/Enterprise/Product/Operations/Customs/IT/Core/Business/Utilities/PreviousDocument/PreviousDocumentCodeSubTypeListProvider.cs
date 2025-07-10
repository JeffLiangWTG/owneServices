#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public sealed class PreviousDocumentCodeSubTypeListProvider
{
	public PreviousDocumentCodeSubTypeListProvider(IPreviousDocumentCombinationsProvider combinationsProvider)
	{
		this.combinationsProvider = Argument.NotNull(combinationsProvider, nameof(combinationsProvider));
	}

	readonly IPreviousDocumentCombinationsProvider combinationsProvider;

	public CodeDescriptionPairList GetPreviousDocumentCodeList()
		=> GetCodeDescriptionPairListFor(x => x.Code);

	public CodeDescriptionPairList GetPreviousDocumentSubTypeList()
		=> GetCodeDescriptionPairListFor(x => x.SubType);

	CodeDescriptionPairList GetCodeDescriptionPairListFor(Func<PreviousDocumentCombinationItem, ICodeDescription> selectPredicate)
	{
		var allowedCombinations = combinationsProvider.GetAllowedPreviousDocumentCombinations();
		var selectedCombinations = allowedCombinations.Select(selectPredicate).DistinctBy(x => x.Code);
		return BuildCodeDescriptionPairList(selectedCombinations);
	}

	CodeDescriptionPairList BuildCodeDescriptionPairList(IEnumerable<ICodeDescription> selectedCombinations)
	{
		var result = new CodeDescriptionPairList();
		foreach (var item in selectedCombinations)
		{
			result.Add(item);
		}
		result.Sort();
		return result;
	}
}
