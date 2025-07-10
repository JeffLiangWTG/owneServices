using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper : NCTS5CommonHouseConsignmentWrapper, INCTSCommonHouseConsignmentDepartureAndAmendmentAndTNN
	{
		public NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper(NctsBill houseConsignment, bool shouldDeclareReferenceNumberUCRInHouseOrItem = false) : base(houseConsignment)
		{
			this.shouldDeclareReferenceNumberUCRInHouseOrItem = shouldDeclareReferenceNumberUCRInHouseOrItem;
		}
		protected readonly ZBool shouldDeclareReferenceNumberUCRInHouseOrItem;

		public ZString ReferenceNumberUCR => ReferenceNumberUCRCore;

		protected virtual ZString ReferenceNumberUCRCore => !houseConsignment.IsInPhase5TransitionPeriod && shouldDeclareReferenceNumberUCRInHouseOrItem && !HasDifferentReferenceNumberUCRInItem() ? GetReferenceNumberUCRFromHouseOrParent() : ZString.Empty;
		ZString GetReferenceNumberUCRFromHouseOrParent() => !houseConsignment.B0_ReferenceID.IsEmpty
																? houseConsignment.B0_ReferenceID
																: houseConsignment.Header.MovementHeader.BM_UniqueConsignmentReference;

		public IReadOnlyCollection<INCTSCommonDocumentWithItem> SupportingDocument
		{
			get
			{
				if (supportingDocument == null)
				{
					var addDocs = !houseConsignment.IsInPhase5TransitionPeriod ? houseConsignment.SupportingDocuments.Cast<CusSupportingInfo>().ToList() : new List<CusSupportingInfo>();
					var orderedDocs = addDocs.OrderBy(doc => doc.CSI_LineNo);

					var docList = new List<NCTS5CommonDocumentWithItemWrapper>();
					foreach (var document in orderedDocs)
					{
						docList.Add(new NCTS5CommonDocumentWithItemWrapper(document, document.CSI_LineNo));
					}

					supportingDocument = docList.AsReadOnly();
				}
				return supportingDocument;
			}
		}
		IReadOnlyCollection<NCTS5CommonDocumentWithItemWrapper> supportingDocument;

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInformation
		{
			get
			{
				if (additionalInformation == null)
				{
					var addDocs = !houseConsignment.IsInPhase5TransitionPeriod ? houseConsignment.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList() : new List<CusSupportingInfo>();
					additionalInformation = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs, shouldSendReferenceNumber: false, shouldSendDescription: true);
				}
				return additionalInformation;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalInformation;

		protected ZBool HasDifferentReferenceNumberUCRInItem() => houseConsignment.GoodsItems.Any(y => !y.BY_CommercialReferenceNumber.IsEmpty && y.BY_CommercialReferenceNumber != houseConsignment.B0_ReferenceID);
	}
}
