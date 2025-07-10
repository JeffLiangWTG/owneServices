using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IT2LPOUSCommonDataProvider : IESEDIMessageCollectionProvider
{
	IT2LPOUSCommonPersonReqPresWithAddress PersonReqPres { get; }
	ZString CustomsOffice { get; }
	ZString SendEmailL { get; }
}

public interface IT2LPOUSCommonPersonReqPres : IPartyIdProvider
{
	IPartyContactProvider ContactPerson { get; }
}

public interface IT2LPOUSCommonPersonReqPresWithAddress : IT2LPOUSCommonPersonReqPres
{
	IPartyAddressProvider Address { get; }
}

public interface IT2LPOUSCommonContainerIndicator
{
	ZBool IsContainerised { get; }
}

public interface IT2LPOUSTransportEquipment
{
	ZString ContainerIdentificationNumber { get; }
	IReadOnlyCollection<ZInt> GoodsReference { get; }
}

public interface IT2LPOUSCommonGoodsItem
{
	ZInt GoodsItemNumber { get; }
}

public interface IT2LPOUSCommonPackaging
{
	ZString TypeOfPackages { get; }
	ZInt NumberOfPackages { get; }
	ZBool NumberOfPackagesValueSpecified { get; }
}

public interface IT2LPOUSCommonProofOperationInformationForT2LT2LF
{
	ZString DeclarationType { get; }
	IT2LPOUSRequestedValidityOfTheProof RequestedValidityOfTheProof { get; }
	ZString RequestType { get; }
	ZBool NationalOnlyRequest { get; }
}

public interface IT2LPOUSRequestedValidityOfTheProof
{
	ZInt NumberOfDays { get; }
	ZString Justification { get; }
}

public interface IT2LPOUSRequestAndReceptionMessageDataProvider : IT2LPOUSCommonDataProvider
{
	IT2LPOUSAuthorisation Authorisation { get; }
	IT2LPOUSCommonPersonReqPres Representative { get; }
	IT2LPOUSGoodsShipment GoodsShipment { get; }
	ZString SendEmailU { get; }
	ZString SendEmailExp { get; }
}

public interface IT2LPOUSAuthorisation
{
	ZString TypeOfAuthorisation { get; }
	ZString DecisionReferenceNumber { get; }
	ZString HolderOfTheAuthorisation { get; }
}

public interface IT2LPOUSGoodsShipment
{
	IT2LPOUSCommonContainerIndicator ContainerIndication { get; }
	IReadOnlyCollection<IT2LPOUSTransportEquipment> TransportEquipment { get; }
	IReadOnlyCollection<IDocumentsCommon> AdditionalInformation { get; }
	IReadOnlyCollection<IDocumentsCommon> PreviousDocument { get; }
	IReadOnlyCollection<IDocumentsCommon> SupportingDocument { get; }
	IReadOnlyCollection<IDocumentsCommon> TransportDocument { get; }
	IReadOnlyCollection<IDocumentsCommon> AdditionalReference { get; }
	IReadOnlyCollection<IT2LPOUSRequestAndReceptionGoodItem> GoodItems { get; }
}

public interface IT2LPOUSRequestAndReceptionGoodItem : IT2LPOUSCommonGoodsItem
{
	ICommodityCodeCommon CommodityCode { get; }
	ZString Description { get; }
	ZString CusCode { get; }
	IGoodsMeasureCommon GoodsMeasure { get; }
	IReadOnlyCollection<IT2LPOUSRequestAndReceptionPackaging> Package { get; }
	IReadOnlyCollection<IDocumentsCommon> AdditionalInformation { get; }
	IReadOnlyCollection<IDocumentsCommon> PreviousDocument { get; }
	IReadOnlyCollection<IDocumentsCommon> SupportingDocument { get; }
	IReadOnlyCollection<IDocumentsCommon> AdditionalReference { get; }
}

public interface IT2LPOUSRequestAndReceptionPackaging : IT2LPOUSCommonPackaging
{
	ZString Marks { get; }
}
