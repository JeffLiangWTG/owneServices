using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business;

public sealed class PreviousDocumentCombinationsProvider : IPreviousDocumentCombinationsProvider
{
	public PreviousDocumentCombinationsProvider(PreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}

	readonly PreviousDocument previousDocument;

	IReadOnlyCollection<PreviousDocumentCombinationItem> IPreviousDocumentCombinationsProvider.GetAllowedPreviousDocumentCombinations()
	{
		var isImport = IsImport;
		var procedure = previousDocument.CSI_Procedure;

		var factory = previousDocument.Factory;
		var cacheKey = FormattableString.Invariant($"IT.PreviousDocumentCombinationsProvider.{isImport}.{procedure}");
		return factory.GetCachedValue(cacheKey, () => GetNonCachedCombinations(isImport, procedure, factory));
	}

	ZBool IsImport => previousDocument.ImportExportParent?.IsImport ?? false;

	ZBool IsUcc6Export => (previousDocument.Parent as IUcc6ValueProvider)?.IsUCC6AndIsExport() ?? false;

	IReadOnlyCollection<PreviousDocumentCombinationItem> GetNonCachedCombinations(ZBool isImport, ZString procedure, BusinessObjectFactory factory)
	{
		var allCombinations = GetAllPreviousDocumentAllowedCombinations(factory, isImport);
		return procedure.IsEmpty
			? allCombinations
			: allCombinations.Where(x => x.Procedure == procedure).ToList().AsReadOnly();
	}

	IReadOnlyCollection<PreviousDocumentCombinationItem> GetAllPreviousDocumentAllowedCombinations(BusinessObjectFactory factory, ZBool isImport)
	{
		return isImport
			? new ImportPreviousDocumentAllowedCombinationsFactory().GetAllowedCombinations(factory)
			: new PreviousDocumentAllowedCombinationsFactory().GetAllowedCombinations(factory);
	}

	PreviousDocumentFieldsInfo IPreviousDocumentCombinationsProvider.GetNewSettings(PreviousDocumentCombinationTemplate? template)
	{
		if (IsImport)
		{
			return new ImportPreviousDocumentFieldsInfo(template);
		}
		if (IsUcc6Export)
		{
			return new Ucc6ExportPreviousDocumentFieldsInfo(template);
		}
		return new PreviousDocumentFieldsInfo(template);
	}
}
