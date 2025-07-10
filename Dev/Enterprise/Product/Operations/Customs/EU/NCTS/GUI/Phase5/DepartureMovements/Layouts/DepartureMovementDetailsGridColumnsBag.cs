using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class DepartureMovementDetailsGridColumnsBag
	{
		public DepartureMovementDetailsGridColumnsBag()
		{
			TransportAtDepartureDropEdit = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureMovementHeader.Schema.BM_TransportAtDeparture, 100);
			InlandTransportModeDropEdit = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureMovementHeader.Schema.BM_InlandTransportMode, 100);
			FromWarehouseAddressDropEdit = new GridColumnReference<ZAddressDropEditColumnStyleInfo>(NctsDepartureMovementHeader.Schema.BM_OA_WarehouseAddress, 125,
				c =>
				{
					c.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("88492e52-bc66-4901-b17b-4f8532699219", "From Warehouse");
				});
			FromWarehouseOrganisationFindBox = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>(NctsDepartureMovementHeader.Schema.FromWarehouseOrgPK, 100,
				c =>
				{
					c.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("88492e52-bc66-4901-b17b-4f8532699219", "From Warehouse");
				});
			DepartureStatusDropEdit = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureMovementHeader.Schema.BM_CustomsStatus, 100);
			MessageStatusDropEdit = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureMovementHeader.Schema.BM_MessageStatus, 100);
			PhaseStatusDropEdit = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureMovementHeader.Schema.BM_Phase, 100);
			DepartureStatusDescriptionTextEdit = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureMovementHeader.Schema.CustomsStatusDescription, 175);
			MessageStatusDescriptionTextEdit = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureMovementHeader.Schema.MessageStatusDescription, 225);
			PhaseStatusDescriptionTextEdit = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureMovementHeader.Schema.PhaseStatusDescription, 150);
		}

		public static DepartureMovementDetailsGridColumnsBag Instance => instance ??= new DepartureMovementDetailsGridColumnsBag();

		[ThreadStatic]
		static DepartureMovementDetailsGridColumnsBag instance;

		public IGridColumnReference PhaseStatusDropEdit { get; }
		public IGridColumnReference PhaseStatusDescriptionTextEdit { get; }
		public IGridColumnReference DepartureStatusDescriptionTextEdit { get; }
		public IGridColumnReference MessageStatusDescriptionTextEdit { get; }
		public IGridColumnReference InlandTransportModeDropEdit { get; }
		public IGridColumnReference FromWarehouseAddressDropEdit { get; }
		public IGridColumnReference FromWarehouseOrganisationFindBox { get; }
		public IGridColumnReference DepartureStatusDropEdit { get; }
		public IGridColumnReference MessageStatusDropEdit { get; }
		public IGridColumnReference TransportAtDepartureDropEdit { get; set; }
	}
}
