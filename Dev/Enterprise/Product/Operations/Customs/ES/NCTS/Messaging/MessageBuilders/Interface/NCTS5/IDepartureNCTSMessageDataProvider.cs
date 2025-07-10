using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IDepartureNCTSCommonMessageDataProvider : IDepartureAndNotificationNCTSCommonMessageDataProvider
	{
		IReadOnlyCollection<INCTSCommonAuthorisation> Authorisations { get; }
		ZString CustomsOfficeOfDestinationDeclared { get; }
		IReadOnlyCollection<INCTSCommonCustomsOffice> CustomsOfficeOfTransitDeclared { get; }
		IReadOnlyCollection<INCTSCommonCustomsOffice> CustomsOfficeOfExitForTransitDeclared { get; }
		INCTSCompleteHolderOfTheTransitProcedure HolderOfTheTransitProcedure { get; }
		IReadOnlyCollection<INCTSCommonGuarantee> Guarantee { get; }
		INCTSCommonConsignmentDepartureAndAmendment Consignment { get; }
	}

	public interface IDepartureNCTSMessageDataProvider : IDepartureNCTSCommonMessageDataProvider
	{
		IDepartureNCTSTransitOperation TransitOperation { get; }
	}

	public interface IDepartureNCTSTransitOperation : INCTSCommonTransitOperationLRN
	{
		INCTSCommonCompleteTransitOperation CommonTransitOperation { get; }
	}
}
