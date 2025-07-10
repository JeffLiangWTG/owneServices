using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration;

public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
{
	public JobDeclarationDocumentSupporter(JobDeclaration declaration)
		: base(declaration)
	{
	}

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

	internal IEnumerable<CusEntryHeader> WrapperDataSources => wrapperDataSources ?? (wrapperDataSources = Declaration.CustomsEntryHeaders.Select(entry => CusEntryHeaderDocumentSupporter.GetRevertedEntryHeaderIfSnapshotExisting(entry.PK) ?? entry));
	IEnumerable<CusEntryHeader> wrapperDataSources;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => (dataContext is DataContext.FRSADH or DataContext.SADH or DataContext.LiquidationDetails) ? WrapperDataSources.Select(entry => DocumentWrapperHelper.GetFRSpecificDocumentWrapper(dataContext, entry)).WhereNotNull().ToArray() : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);

	public override bool SupportDocBuilderInvoiceAsChildCommand => false;

	protected override DataContext[] GetSupportedDataContexts()
	{
		var result = new List<DataContext>(base.GetSupportedDataContexts())
		{
			DataContext.SADH,
			DataContext.FRSADH,
			DataContext.LiquidationDetails
		};
		return result.ToArray();
	}
}
