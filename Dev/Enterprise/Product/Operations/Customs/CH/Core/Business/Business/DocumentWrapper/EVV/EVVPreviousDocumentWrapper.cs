using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public class EVVPreviousDocumentWrapper : DocumentWrapper
{
	public static EVVPreviousDocumentWrapper New(IEvvPreviousDocument previousDocument, BusinessObjectFactory factory) => new EVVPreviousDocumentWrapper(Argument.NotNull(previousDocument, nameof(previousDocument)), factory);

	EVVPreviousDocumentWrapper(IEvvPreviousDocument previousDocument, BusinessObjectFactory factory) : base(previousDocument, factory)
	{
	}
	IEvvPreviousDocument PreviousDocument => (IEvvPreviousDocument)WrappedObject;

	public ZString Type => PreviousDocument.Type;

	public ZString Reference => PreviousDocument.Reference;

	public ZString AdditionalInformation => PreviousDocument.AdditionalInformation;
}
