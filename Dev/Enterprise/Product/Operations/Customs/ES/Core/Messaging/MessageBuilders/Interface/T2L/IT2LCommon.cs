using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IT2LCommunicationsCommon
	{
		ZString DeclarationEmail { get; }
		ZString OtherEmail { get; }
		ZBool GreenCircuitIndicator { get; }
	}

	public interface IT2LHeaderCommon
	{
		ZString ExpeditionCountry { get; }
		ZInt TotalLinesNum { get; }
		ZInt TotalPackagesQty { get; }
		ZBool ContainersIndicator { get; }
		IPartyNameProvider Declarant { get; }
	}

	public interface IT2LLineCommon
	{
		ZInt LineNumber { get; }
		ZString GoodsCode { get; }
		ZString GoodsDescription { get; }
		ZDecimal GrossWeightInKG { get; }
		ZDecimal NetWeightInKG { get; }
		IReadOnlyCollection<IPackageCommonNumbers> Packages { get; }
		IReadOnlyCollection<ZString> Containers { get; }
		IReadOnlyCollection<IVehicleCommon> Vehicles { get; }
	}
}
