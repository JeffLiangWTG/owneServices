using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper : INCTSCommonConsignmentDepartureAndAmendmentAndTNN
	{
		public NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper(NctsHeader header, ZBool isTIRDeclaration, ZBool shouldDeclareCountryOfDispatchInConsignment, ZBool shouldDeclareCountryOfDestinationInConsignment, ZBool shouldDeclareUCRInConsignment, ZBool shouldDeclareConsigneeInConsignment, ZBool any30600AdditionalInfoInItems, ZBool isEXISecurityType)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			departureMovement = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));

			this.isTIRDeclaration = isTIRDeclaration;
			this.shouldDeclareCountryOfDispatchInConsignment = shouldDeclareCountryOfDispatchInConsignment;
			this.shouldDeclareCountryOfDestinationInConsignment = shouldDeclareCountryOfDestinationInConsignment;
			this.shouldDeclareUCRInConsignment = shouldDeclareUCRInConsignment;
			this.shouldDeclareConsigneeInConsignment = shouldDeclareConsigneeInConsignment;
			this.any30600AdditionalInfoInItems = any30600AdditionalInfoInItems;
			this.isEXISecurityType = isEXISecurityType;
			this.isEXISecurityType = isEXISecurityType;
		}
		readonly NctsHeader nctsHeader;
		readonly NctsDepartureMovementHeader departureMovement;
		readonly ZBool isTIRDeclaration;
		readonly ZBool shouldDeclareCountryOfDispatchInConsignment;
		readonly ZBool shouldDeclareCountryOfDestinationInConsignment;
		readonly ZBool shouldDeclareUCRInConsignment;
		readonly ZBool shouldDeclareConsigneeInConsignment;
		readonly ZBool any30600AdditionalInfoInItems;
		readonly ZBool isEXISecurityType;

		const int WeightMaxDecimalsTransitionalPeriod = 3;
		const int WeightMaxDecimalsFinalPeriod = 6;
		protected int WeightMaxDecimals => nctsHeader.IsInPhase5TransitionPeriod ? WeightMaxDecimalsTransitionalPeriod : WeightMaxDecimalsFinalPeriod;

		public ZString CountryOfDispatch => (!nctsHeader.IsInPhase5TransitionPeriod || isTIRDeclaration) && shouldDeclareCountryOfDispatchInConsignment ? departureMovement.BM_RN_NKCountryOfDispatch : ZString.Empty;

		public ZString CountryOfDestination => shouldDeclareCountryOfDestinationInConsignment ? departureMovement.BM_RL_NKDestinationPort : ZString.Empty;

		public ZString ReferenceNumberUCR => shouldDeclareUCRInConsignment ? departureMovement.BM_UniqueConsignmentReference : ZString.Empty;

		public INCTSPartyNameProviderWithAddress Consignee => consignee ?? (consignee = shouldDeclareConsigneeInConsignment && !any30600AdditionalInfoInItems ? NCTS5PartyNameProviderWithAddressWrapper.New(nctsHeader.Consignee, true, nctsHeader.IsInPhase5TransitionPeriod) : null);
		NCTS5PartyNameProviderWithAddressWrapper consignee;

		public IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment> CountryOfRoutingOfConsignment
		{
			get
			{
				if (countryOfRoutingOfConsignments == null)
				{
					var countryOfRoutingOfConsignmentsList = new List<CommonCountryOfRoutingOfConsignmentWrapper>();

					ZShort seqNum = 1;
					foreach (var country in nctsHeader.CountriesOfRouting)
					{
						countryOfRoutingOfConsignmentsList.Add(new CommonCountryOfRoutingOfConsignmentWrapper(seqNum, country.CY_Data));
						seqNum++;
					}

					countryOfRoutingOfConsignments = countryOfRoutingOfConsignmentsList.AsReadOnly();
				}
				return countryOfRoutingOfConsignments;
			}
		}
		IReadOnlyCollection<CommonCountryOfRoutingOfConsignmentWrapper> countryOfRoutingOfConsignments;

		public ZString MethodOfPayment => isEXISecurityType ? departureMovement.BM_MethodOfPayment : ZString.Empty;

		public ZDecimal GrossMass => departureMovement.BM_GrossWeight.Round(WeightMaxDecimals);

		public IReadOnlyCollection<INCTSCommonDocumentWithItem> SupportingDocument
		{
			get
			{
				if (supportingDocument == null)
				{
					var addDocs = !nctsHeader.IsInPhase5TransitionPeriod ? nctsHeader.MovementHeader.SupportingDocuments.Cast<CusSupportingInfo>().ToList() : new List<CusSupportingInfo>();
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

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument
		{
			get
			{
				if (transportDocuments == null)
				{
					var addDocs = GetAdditionalInfos(AdditionalInfoSubTypeList.Codes.TransportDocument);

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

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInformation
		{
			get
			{
				if (additionalInformation == null)
				{
					var addDocs = GetAdditionalInfos(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
					additionalInformation = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs, shouldSendReferenceNumber: false, shouldSendDescription: true);
				}
				return additionalInformation;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalInformation;

		List<CusSupportingInfo> GetAdditionalInfos(ZString subType)
		{
			var addDocs = !nctsHeader.IsInPhase5TransitionPeriod ? nctsHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList() : new List<CusSupportingInfo>();
			return addDocs;
		}

		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(List<CusSupportingInfo> documents)
								=> CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(documents);
	}
}
