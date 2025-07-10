using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class CancelNCTS5SendMessageWrapper : NCTS5CommonSendMessageWrapper, ICancelNCTSMessageDataProvider
	{
		public CancelNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData, ZString reasonForCancellation) : base(header, certificateData)
		{
			this.reasonForCancellation = Argument.NotNullOrEmpty(reasonForCancellation, nameof(reasonForCancellation));
		}
		readonly ZString reasonForCancellation;

		public INCTSCommonTransitOperationMRN TransitOperation => transitOperation ?? (transitOperation = new NCTS5CommonTransitOperationMRNWrapper(nctsHeader));
		NCTS5CommonTransitOperationMRNWrapper transitOperation;

		public ICancelNCTSInvalidation Invalidation => invalidation ?? (invalidation = new CancelNCTS5InvalidationWrapper(reasonForCancellation));
		CancelNCTS5InvalidationWrapper invalidation;

		public ZString CustomsOfficeOfDeparture => nctsHeader.CommonMovementHeader.CustomsOffices.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).FirstOrDefault()?.CY_Data ?? ZString.Empty;

		public INCTSCommonHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader));
		NCTS5CommonHolderOfTheTransitProcedureWrapper holderOfTheTransitProcedure;
	}
}
