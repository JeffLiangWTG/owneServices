using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CH.Business;

public class DocForwardingConsol : DocumentWrappers.DocForwardingConsol
{
	public new static DocForwardingConsol New(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
	{
		return consol == null ? null : new DocForwardingConsol(consol, factoryToWrap);
	}

	protected DocForwardingConsol(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
		: base(consol, factoryToWrap)
	{
		this.consol = consol;
	}
	readonly ForwardingConsol consol;

	public ZString TransportRegNumber => consol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_VoyageFlightForBinding ?? ZString.Empty;

	public CodeAndDescriptionWrapper CustomsOffice => customsOffice ??= RandomDeclaration != null ? new CodeAndDescriptionWrapper(RandomDeclaration.JE_CustomsOffice, RandomDeclaration.Lookups.CustomsOffices, Factory) : null;
	CodeAndDescriptionWrapper customsOffice;

	internal IEnumerable<JobDeclaration> Declarations => from s in consol.Shipments.Cast<ForwardingShipment>()
														 from d in s.Declarations.OfType<JobDeclaration>()
														 select d;
	JobDeclaration RandomDeclaration => Declarations.FirstOrDefault();

	public ZString HeaderSelectionResult => headerSelectionResult ??= InitHeaderSelectionResult();
	string headerSelectionResult;

	public ZString HeaderSelectionResultDescription => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult, ZDateTime.Today, languageCode: DocumentWrapperHelper.SelectedPrinterLanguage).GetDescriptionFromCode(HeaderSelectionResult);

	public DocCusEntryLineCollection EntryLines => entryLines ??= InitEntryLineCollection();
	DocCusEntryLineCollection entryLines;

	string InitHeaderSelectionResult()
	{
		(entryLines, headerSelectionResult) = CreateNewEntryLineCollectionAndEvaluateHeaderSelectionResult();
		return headerSelectionResult;
	}

	DocCusEntryLineCollection InitEntryLineCollection()
	{
		(entryLines, headerSelectionResult) = CreateNewEntryLineCollectionAndEvaluateHeaderSelectionResult();
		return entryLines;
	}

	(DocCusEntryLineCollection, ZString) CreateNewEntryLineCollectionAndEvaluateHeaderSelectionResult()
	{
		var entryHeaderMaxResult = ZString.Empty;
		var entryLines = new DocCusEntryLineCollection(Factory);

		foreach (var declaration in Declarations.OrderBy(x => x.JE_DeclarationReference).Select(d => DocDeclaration.New(d, Factory)))
		{
			var declarationEntryLines = new List<DocCusEntryLine>();

			foreach (var header in declaration.EntryHeaders.Cast<DocBaseCusEntryHeader>())
			{
				entryHeaderMaxResult = entryHeaderMaxResult.GetMaxNumericValue(header.SelectionResult);

				foreach (var entryLine in header.EntryLines.Cast<DocCusEntryLine>())
				{
					entryLine.Declaration = declaration;
					declarationEntryLines.Add(entryLine);
				}
			}
			entryLines.AddRange(declarationEntryLines);
		}
		return (entryLines, entryHeaderMaxResult);
	}
}
