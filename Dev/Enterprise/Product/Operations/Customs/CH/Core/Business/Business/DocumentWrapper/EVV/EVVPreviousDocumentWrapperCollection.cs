using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public class EVVPreviousDocumentWrapperCollection : DocumentWrapperCollection
{
	public static EVVPreviousDocumentWrapperCollection New(IEnumerable<IEvvPreviousDocument> previousDocuments, BusinessObjectFactory factory) => new EVVPreviousDocumentWrapperCollection(previousDocuments.EmptyIfNull(), factory);

	EVVPreviousDocumentWrapperCollection(IEnumerable<IEvvPreviousDocument> previousDocuments, BusinessObjectFactory factory) : base(previousDocuments, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVPreviousDocumentWrapper.New((IEvvPreviousDocument)objectToWrap, Factory);
	}
}
