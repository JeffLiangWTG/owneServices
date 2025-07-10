using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderDocumentSupporter : EU.NCTS.Business.NctsHeaderDocumentSupporter
{
	public NctsHeaderDocumentSupporter(NctsHeader parent) : base(parent)
	{
	}

	#region Implementation

	protected override DataContext[] GetSupportedDataContexts()
	{
		var result = new List<DataContext>(base.GetSupportedDataContexts())
		{
			DataContext.ITTADAttachment
		};

		return result.ToArray();
	}

	NctsHeader NctsHeader => (NctsHeader)BusinessObject;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		BusinessObject businessObjectToWrap = null;
		switch (dataContext)
		{
			case DataContext.EuNcts:
			case DataContext.EuNcts5TAD:
				businessObjectToWrap = NctsHeader;
				break;
			case DataContext.ITTADAttachment:
				businessObjectToWrap = GetAttachmentWrapper();
				break;
		}

		if (businessObjectToWrap == null)
		{
			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		return CreateDocumentWrappers(dataContext, businessObjectToWrap);
	}

	NctsHeader GetAttachmentWrapper()
	{
		var requiresAttachment = NctsHeader.MovementHeader
			?.GoodsItems
			.Cast<NctsDepartureCargoDesc>()
			.Any(x => x.AttachmentPrintingSupporter.RequiresAttachment) ?? false;
		return requiresAttachment ? NctsHeader : null;
	}

	DocumentWrapper[] CreateDocumentWrappers(DataContext dataContext, BusinessObject businessObjectToWrap)
	{
		var wrapperStrongName = supportedWrappers[dataContext];
		return new[] { DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, businessObjectToWrap) }
				.WhereNotNull()
				.ToArray();
	}

	readonly ImmutableDictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
	{
		{ DataContext.EuNcts,  DocumentWrapperConstants.FullNames.NctsHeaderAccompanyingDocument },
		{ DataContext.EuNcts5TAD,  DocumentWrapperConstants.FullNames.NctsHeaderPhase5TransitAccompanyingDocument },
		{ DataContext.ITTADAttachment,  DocumentWrapperConstants.FullNames.ITTADAttachment },
	}.ToImmutableDictionary();

	#endregion
}
