using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.Business;

public class CHEDIMessageDocumentSupporter : Customs.Business.EDIMessageDocumentSupporter
{
	internal const string EVVTaxationDocument = ".EVVTaxationDocument";

	internal const string EVVRefundDocument = ".EVVRefundDocument";

	internal const string EVVValidationDocument = ".EVVValidationDocument";

	protected new CHEDIMessage EdiMessage => (CHEDIMessage)base.EdiMessage;

	public CHEDIMessageDocumentSupporter(CHEDIMessage message) : base(message)
	{
	}

	protected override List<DataContextValue> GetSupportedBODataSources()
	{
		var result = base.GetSupportedBODataSources();
		result.AddRange(new[] { new DataContextValue(EVVTaxationDocument), new DataContextValue(EVVRefundDocument), new DataContextValue(EVVValidationDocument) });
		return result;
	}

	protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
	{
		var fullDataContext = dataContextValue.FullDataContext;
		switch (fullDataContext)
		{
			case EVVTaxationDocument:
				return new[] { BODocDataProvider.Get(EVVTaxationDocumentWrapper.New(EdiMessage, Factory)) };
			case EVVRefundDocument:
				return new[] { BODocDataProvider.Get(EVVRefundDocumentWrapper.New(EdiMessage, Factory)) };
			case EVVValidationDocument:
				return new[] { BODocDataProvider.Get(EvvSignatureDocumentWrapper.New(EdiMessage, Factory)) };
			default:
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}
	}
}

