using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IGoodsMeasureCommon
{
	ZDecimal GrossWeight { get; }
	ZDecimal NetWeight { get; }
}

public interface ICommonGoodsMeasureWithSpecified : IGoodsMeasureCommon
{
	ZBool GrossWeightSpecified { get; }
	ZBool NetWeightSpecified { get; }
}

public interface ICommonGoodsMeasureWithSupUnitsAndSpecified : ICommonGoodsMeasureWithSpecified
{
	ZDecimal SupplementaryUnits { get; }
	ZBool SupplementaryUnitsSpecified { get; }
}
