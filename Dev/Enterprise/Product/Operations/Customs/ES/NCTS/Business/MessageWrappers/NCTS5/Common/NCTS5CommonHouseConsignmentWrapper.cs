using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonHouseConsignmentWrapper : NCTS5CommonHouseConsignmentSeqNumWrapper, INCTSCommonHouseConsignment
	{
		public NCTS5CommonHouseConsignmentWrapper(NctsBill houseConsignment) : base(GetSeqNum(houseConsignment))
		{
			this.houseConsignment = Argument.NotNull(houseConsignment, nameof(houseConsignment));
		}
		protected readonly NctsBill houseConsignment;

		protected const int WeightMaxDecimalsTransitionalPeriod = 3;
		protected const int WeightMaxDecimalsFinalPeriod = 6;
		protected int WeightMaxDecimals => houseConsignment.IsInPhase5TransitionPeriod ? WeightMaxDecimalsTransitionalPeriod : WeightMaxDecimalsFinalPeriod;

		public ZDecimal GrossMass => GrossMassCore;
		protected virtual ZDecimal GrossMassCore => houseConsignment.GrossWeightInKilograms.Round(WeightMaxDecimals);

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument
		{
			get
			{
				if (transportDocuments == null)
				{
					transportDocuments = GetAdditionalDocumentsWrapperListForSubType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				}
				return transportDocuments;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> transportDocuments;

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReference
		{
			get
			{
				if (additionalReference == null)
				{
					additionalReference = GetAdditionalDocumentsWrapperListForSubType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				}
				return additionalReference;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalReference;

		static ZShort GetSeqNum(NctsBill houseConsignment) => houseConsignment?.IsPhase5Arrival ?? false
																	? (ZShort.TryParse(houseConsignment.MovementDetail.B9_SeqNo, out var consignmentSeqNum)
																					? consignmentSeqNum
																					: ZShort.Zero)
																	: (houseConsignment?.SequenceNumber ?? ZShort.Zero);

		protected virtual IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetAdditionalDocumentsWrapperListForSubType(ZString subType)
		{
			var addDocs = !houseConsignment.IsInPhase5TransitionPeriod ? houseConsignment.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList() : new List<CusSupportingInfo>();
			return CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs);
		}
	}
}
