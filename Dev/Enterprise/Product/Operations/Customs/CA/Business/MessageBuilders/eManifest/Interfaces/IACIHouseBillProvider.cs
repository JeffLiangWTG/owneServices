namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Edifact.D11B.Elements;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Integration;

	public interface IACIHouseBillProvider : IACIForwarderMessageProvider
	{
		ZString HouseCCN { get; }
		ZString UCR { get; }
		IEnumerable<ISecondaryNotifyParty> SecondaryNotifyParties { get; }
		ZString MovementType { get; }
		ZString PrimaryCCN { get; }
		ZString B2BComments { get; }
		ZString TransportMode { get; }
		ZBool IsConsolidatedCargo { get; }
		ZDecimal Volume { get; }
		ZString VolumeUOM { get; }
		ZString SpecialHandlingInstructions { get; }
		IJobDocAddress Consignee { get; }
		IJobDocAddress Shipper { get; }
		IEnumerable<IJobDocAddress> DeliveryAddresses { get; }
		IEnumerable<IJobDocAddress> NotifyParties { get; }
		IJobDocAddress PlaceOfConsolidation { get; }
		IOrgContact UNDGContact { get; }
		ZString ReleasePortCode { get; }
		ZString ReleaseSubLocationCode { get; }
		ZString DischargePortCode { get; }
		ZString DischargeSubLocationCode { get; }
		IJobDocAddress Consolidator { get; }
		ZString DGSpecialInstructions { get; }
		IEnumerable<IHouseBillContainer> Containers { get; }
		IEnumerable<IHouseBillLine> Lines { get; }
		ZDecimal TotalWeight { get; }
		ZString TotalWeightUOM { get; }
	}

	public interface ISecondaryNotifyParty
	{
		PartyFunctionCodeQualifierList SecondaryNotifyType { get; }
		ZString Identifier { get; }
		ZString NoticeType { get; }
	}

	public interface IHouseBillContainer
	{
		ZString ContainerNumber { get; }
		IEnumerable<ZString> Seals { get; }
	}

	public interface IHouseBillLine
	{
		ZInt LineNumber { get; }
		ZDecimal Packs { get; }
		ZString PacksUOM { get; }
		IEnumerable<ZString> Marks { get; }
		ZString GoodsDescription { get; }
		ZString HSCode { get; }
		IEnumerable<ZString> DGCodes { get; }
	}
}
