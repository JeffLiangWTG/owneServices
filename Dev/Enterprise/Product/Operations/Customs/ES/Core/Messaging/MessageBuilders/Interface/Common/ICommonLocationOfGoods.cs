using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IGenericLocation
{
	ZString Type { get; }
	ZString Qualifier { get; }
	ICodedGenericLocation Coded { get; }
	IPartyAddressProvider Address { get; }
}

public interface ICodedGenericLocation
{
	ZString UNLOCOCode { get; }
	ZString CustomsOffice { get; }
	ICommonGNSS GPS { get; }
	ZString EconomicOperator { get; }
	ZString AuthorisationNumber { get; }
	ZString AdditionalId { get; }
}

public interface ICommonLocationOfGoods
{
	ZString LocationType { get; }
	ZString LocationQualifier { get; }
	ZString LocationId { get; }
	ZString LocationAdditionalId { get; }
	ZString LocationUNloCode { get; }
	ZString LocationCustomOffice { get; }
	ICommonGNSS LocationGNSS { get; }
	ZString LocationEconomicOperatorId { get; }
	IPartyAddressProvider LocationAddress { get; }
	ICommonPostcodeAddress LocationPostcodeAddress { get; }
}

public interface ICommonPostcodeAddress
{
	ZString HouseNumber { get; }
	ZString PostCode { get; }
	ZString Country { get; }
}
