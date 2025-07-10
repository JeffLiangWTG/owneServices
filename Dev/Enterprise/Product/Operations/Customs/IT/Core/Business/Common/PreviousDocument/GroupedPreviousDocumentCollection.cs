using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class GroupedPreviousDocumentCollection : NonPersistentBusinessObjectCollection<GroupedPreviousDocument>
{
	GroupedPreviousDocumentCollection(IMergedPreviousDocumentsProvider mergedPreviousDocumentsProvider, BusinessObjectFactory factory) : base(factory)
	{
		this.mergedPreviousDocumentsProvider = Argument.NotNull(mergedPreviousDocumentsProvider, nameof(mergedPreviousDocumentsProvider));
	}
	readonly IMergedPreviousDocumentsProvider mergedPreviousDocumentsProvider;

	public static GroupedPreviousDocumentCollection LoadNew(IMergedPreviousDocumentsProvider mergedPreviousDocumentsProvider, BusinessObjectFactory factory)
	{
		Argument.NotNull(factory, nameof(factory));
		var result = new GroupedPreviousDocumentCollection(mergedPreviousDocumentsProvider, factory);
		result.PerformGrouping();
		return result;
	}

	public ZBool IsSendableInNbMessage
	{
		get
		{
			var customsStatus = mergedPreviousDocumentsProvider.NBStatus;
			return Count > 0 && (customsStatus.IsEmpty || customsStatus == EntryLineCustomsStatusList.Codes.Rejected || customsStatus == EntryLineCustomsStatusList.Codes.Sent);
		}
	}

	public ZBool ContainsSummaryDeclarationDocument => this.Cast<GroupedPreviousDocument>().Any(x => !x.SummaryDeclarationDocumentRegister.IsEmpty);

	protected override bool AllowNewCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new InvalidOperationException("It is not possible to create a new grouped previous document object from this collection");
	}

	#region Implementation

	void PerformGrouping()
	{
		var mergedPreviousDocuments = mergedPreviousDocumentsProvider.MergedPreviousDocuments;
		var summaryDeclarationDocuments = mergedPreviousDocuments.Where(x => x.IsSummaryDeclarationDocument);
		var previousProcedureDocuments = mergedPreviousDocuments.Where(x => x.IsPreviousProcedureDocument);

		if (HasMoreThanOnePreviousProcedureDocument())
		{
			var summaryDeclarationDocument = summaryDeclarationDocuments.OrderBy(x => x.Register).FirstOrDefault();
			previousProcedureDocuments.ForEach(previousProcedureDocument => Add(summaryDeclarationDocument, previousProcedureDocument));
		}
		else if (HasMoreThanOneSummaryDeclarationDocument())
		{
			var previousProcedureDocument = previousProcedureDocuments.SingleOrDefault();
			summaryDeclarationDocuments.ForEach(summaryDeclarationDocument => Add(summaryDeclarationDocument, previousProcedureDocument));
		}
		else if (mergedPreviousDocumentsProvider.IsExport && previousProcedureDocuments.Any() && summaryDeclarationDocuments.Any())
		{
			var summaryDeclarationDocument = summaryDeclarationDocuments.Single();
			var previousProcedureDocument = previousProcedureDocuments.Single();
			Add(summaryDeclarationDocument, previousProcedureDocument);
		}

		bool HasMoreThanOnePreviousProcedureDocument() => previousProcedureDocuments.Skip(1).Any();

		bool HasMoreThanOneSummaryDeclarationDocument() => summaryDeclarationDocuments.Skip(1).Any();
	}

	void Add(IMergedPreviousDocument summaryDeclarationDocument, IMergedPreviousDocument previousProcedureDocument)
	{
		Add(new GroupedPreviousDocument(mergedPreviousDocumentsProvider, summaryDeclarationDocument, previousProcedureDocument, Factory));
	}

	#endregion
}
