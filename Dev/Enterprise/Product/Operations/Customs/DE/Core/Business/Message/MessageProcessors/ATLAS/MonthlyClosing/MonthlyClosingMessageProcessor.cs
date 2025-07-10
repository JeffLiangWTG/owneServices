using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public abstract class MonthlyClosingMessageProcessor<TEDIMessage, TDataProvider> : ImportMessageProcessor<TEDIMessage, TDataProvider>
		where TDataProvider : IDataProvider
		where TEDIMessage : AtlasInboundEDIMessage<TDataProvider>
	{
		protected MonthlyClosingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is CusReconDeclaration dec)
			{
				result = dec.CRD_GB_Branch;
			}
			return result;
		}
	}
}
