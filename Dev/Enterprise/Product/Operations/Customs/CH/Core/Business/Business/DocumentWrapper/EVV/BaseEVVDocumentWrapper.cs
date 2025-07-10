using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseEVVDocumentWrapper : DocumentWrapper, IBODocDataProvider, IDocumentWrapper
{
	protected BaseEVVDocumentWrapper(ICHEDIMessage message, BusinessObjectFactory factory) : base(message.EM_LinkedObject, factory)
	{
	}

	public ZString DocumentFilename => DocumentFilenameCore;

	protected virtual ZString DocumentFilenameCore { get; }
}
