using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IComplXDVDMessageDataProvider : IDVDCommonDataProvider
	{
		ZInt TotalPackages { get; }
		ZDecimal TotalGrossMass { get; }
		IComplXDVDDeclarantAndRepresentative DeclarantAndRepresentative { get; }
		IReadOnlyCollection<IComplXDVDLine> Lines { get; }
	}

	public interface IComplXDVDLine
	{
		ZInt LineNumber { get; }
		IReadOnlyCollection<IDVDCommonPackage> Packages { get; }
		IReadOnlyCollection<IVehicleCommon> Vehicles { get; }
		ZString DepositUnitOfMeasureCodeEU { get; }
		ZDecimal DepositUnitOfMeasureQuantity { get; }
		ZDecimal GrossMassKg { get; }
		ZDecimal NetMassKg { get; }
		ZDecimal SupplementaryQuantity { get; }
	}

	public interface IComplXDVDDeclarantAndRepresentative
	{
		IPartyNameProvider Declarant { get; }
		IPartyNameProvider Representative { get; }
		ZString RepresentativeTypeAuthorization { get; }
	}
}
