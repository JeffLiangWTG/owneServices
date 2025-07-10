using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DepartureMovementDetailsGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<DepartureMovementDetailsGridColumnsLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(NctsDepartureMovementHeader.Schema.BM_InlandTransportMode, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsDepartureMovementHeader.Schema.BM_TransportAtDeparture, typeof(ZTextBoxColumnStyleInfo), 100),
			(NctsDepartureMovementHeader.Schema.FromWarehouseOrgPK, typeof(ZOrganisationFindBoxColumnStyleInfo), 100),
			(NctsDepartureMovementHeader.Schema.BM_OA_WarehouseAddress, typeof(ZAddressDropEditColumnStyleInfo), 125),
			(NctsDepartureMovementHeader.Schema.BM_CustomsStatus, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsDepartureMovementHeader.Schema.CustomsStatusDescription, typeof(ZTextBoxColumnStyleInfo), 175),
			(NctsDepartureMovementHeader.Schema.BM_MessageStatus, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsDepartureMovementHeader.Schema.MessageStatusDescription, typeof(ZTextBoxColumnStyleInfo), 225),
			(NctsDepartureMovementHeader.Schema.BM_Phase, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsDepartureMovementHeader.Schema.PhaseStatusDescription, typeof(ZTextBoxColumnStyleInfo), 150),
		};

		protected override Type GridBoundEntityType => typeof(NctsDepartureMovementHeader);
	}
}
