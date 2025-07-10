using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonConsignmentItemWrapper : INCTSCommonConsignmentItem
	{
		public NCTS5CommonConsignmentItemWrapper(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}
		protected readonly NctsCommonCargoDesc item;

		public ZString GoodsItemNumber => item.BY_LineNo.ToString();

		public ZString DeclarationGoodsItemNumber => item.BY_DeclarationGoodsItemNumber.ToString();

		public IReadOnlyCollection<INCTSCommonPackaging> Packaging => packages ?? (packages = GetPackagingCore());
		IReadOnlyCollection<NCTS5CommonPackagingWrapper> packages;
		protected virtual IReadOnlyCollection<NCTS5CommonPackagingWrapper> GetPackagingCore() => new List<NCTS5CommonPackagingWrapper>().AsReadOnly();

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument
		{
			get
			{
				if (transportDocuments == null)
				{
					var addDocs = ShouldSendTransportDocuments ? GetAdditionalInfos(AdditionalInfoSubTypeList.Codes.TransportDocument) : new List<CusSupportingInfo>();

					transportDocuments = GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs);
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
					var addDocs = GetAdditionalInfos(AdditionalInfoSubTypeList.Codes.AdditionalReference);

					additionalReference = GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs);
				}
				return additionalReference;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalReference;

		protected virtual List<CusSupportingInfo> GetAdditionalInfos(ZString subType)
		{
			var addDocs = item.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList();
			if (item.IsInPhase5TransitionPeriod)
			{
				addDocs.AddRange(item.Bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList());
				addDocs.AddRange(item.Header.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList());
			}

			return addDocs;
		}

		protected virtual IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(List<CusSupportingInfo> documents)
								=> CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(documents);

		protected virtual bool ShouldSendTransportDocuments => item.IsInPhase5TransitionPeriod;
	}
}
