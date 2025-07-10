using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalMessageDataProvider : INctsHeaderMessageProvider
	{
		#region Type Felds

		ZBool IsOnlyArrivalNotification { get; }
		ZBool IsOnlyUnloadingRemarks { get; }
		ZBool IsArrivalWithAVI { get; }
		ZBool IsArrivalWithOBS { get; }
		ZBool IsArrivalWithTNN { get; }

		#endregion

		#region Fields For UNH and BGM

		ZString DocumentMessageName { get; }

		#endregion

		#region Fields For CST

		ZString CustomsProcedureCategory1 { get; }
		ZString CustomsProcedureCategory2 { get; }

		#endregion

		#region Fields For LOC

		ZString CustomsTransitDestinationOffice { get; }
		IArrivalCustomsEffectiveDestinationOffice CustomsOfficesOfDestination { get; }

		#endregion

		#region Fields For DTM

		ZDateTime DateOfArrival { get; }
		ZDateTime DateOfUnloading { get; }

		#endregion

		#region Fields For GIS

		ZString GoodsInContainerIndicator { get; } //Boolean with "0" or "1" but not mandatory so ZString because ZBool is not null if not declared
		ZString UnloadingComplianceIndicator { get; } //Boolean with "0" or "1" but not mandatory so ZString because ZBool is not null if not declared
		ZString SealsInGoodState { get; } //Boolean with "B" or "M" but not mandatory so ZString because ZBool is not null if not declared
		ZString AttachedDocumentTypeA { get; } //Boolean with "A" or "4" but not mandatory so ZString because ZBool is not null if not declared
		ZBool ReceiverComplianceForAutoDischarge { get; }
		ZString DirectlyLoadedOnCompletion { get; } //Boolean with "0" or "1" but not mandatory so ZString because ZBool is not null if not declared
		IReadOnlyCollection<string> SealsStateDiscrepancies { get; }
		ZString TIRCompletionNumber { get; }
		ZString TIRIsCompleteUnloading { get; } //Boolean with "T" or "P" but not mandatory so ZString because ZBool is not null if not declared

		#endregion

		#region Fields For SEL

		IReadOnlyCollection<ZString> SealCodes { get; }
		IReadOnlyCollection<ZString> SealCodesWithDiscrepancies { get; }

		#endregion

		#region Fields For RFF and PAC and PCI and FTX

		IArrivalReferencesGroup ReferenceGroup { get; }

		#endregion

		#region Fields For TDT

		ZString TransitTransportMedium { get; }

		#region Fields For TPL

		ZString TransportId { get; }
		ZString TransportNationality { get; }

		#endregion

		#endregion

		#region Fields For NAD

		IPartyNameProvider Declarant { get; }

		#endregion

		#region Fields For TOD and FTX

		ZString UnloadingObservations { get; }

		#endregion

		#region Fields For MOA

		ZBool IsDeclarationInEuros { get; }

		#endregion

		#region Fields For Goods

		IReadOnlyCollection<IArrivalLine> Lines { get; }

		#endregion

		#region Fields For CNT

		ZInt TotalNumberOfGoods { get; }
		ZLong TotalNumberOfPackageElements { get; }

		#endregion
	}
}
