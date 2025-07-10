using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeaderDocumentSupporter : EU.Business.CusTempStorage.TemporaryStorageHeaderDocumentSupporter
{
	public TemporaryStorageHeaderDocumentSupporter(TemporaryStorageHeader header) : base(header)
	{
		temporaryStorageHeader = header;
	}

	public override string GetFilterValue(DocumentFilters filterName)
	{
		if (filterName == DocumentFilters.CTY)
		{
			return Core.Constants.CountryCodes.Spain;
		}
		return base.GetFilterValue(filterName);
	}

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		if (dataContext == Constants.DataContext.EuPnts)
		{
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapperWithoutException(temporaryStorageHeader.Configuration.TemporaryStorageHeaderDocumentWrapperClass, BusinessObject, DocumentWrapperConstants.DocumentWrappersAssembly) };
		}

		return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}

	readonly TemporaryStorageHeader temporaryStorageHeader;
}
