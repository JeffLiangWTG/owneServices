using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NctsHeaderCollection = Enterprise.Customs.EU.NCTS.Business.NctsHeaderCollection;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageWrapperFromParentHelper : AutoTemporaryStorageWrapperFromParentHelper
	{
		public TemporaryStorageWrapperFromParentHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		[ReadOnlyMember(nameof(Shipment_Readonly))]
		[List(nameof(Shipments))]
		public override ZGuid ShipmentPK
		{
			get { return base.ShipmentPK; }
			set { base.ShipmentPK = value; }
		}

		[ReadOnlyMember(nameof(Shipment_Readonly))]
		public ForwardingShipment Shipment => Factory.Load<ForwardingShipment>(ShipmentPK);

		[ReadOnlyMember(nameof(Declaration_Readonly))]
		[List(nameof(Declarations))]
		public override ZGuid DeclarationPK
		{
			get { return base.DeclarationPK; }
			set { base.DeclarationPK = value; }
		}

		[ReadOnlyMember(nameof(Declaration_Readonly))]
		public JobDeclaration Declaration => Factory.Load<JobDeclaration>(DeclarationPK);

		[ReadOnlyMember(nameof(DeltaT_Readonly))]
		[List(nameof(DeltaTs))]
		public override ZGuid DeltaTPK
		{
			get { return base.DeltaTPK; }
			set { base.DeltaTPK = value; }
		}

		[ReadOnlyMember(nameof(DeltaT_Readonly))]
		public NctsHeader DeltaT => Factory.Load<NctsHeader>(DeltaTPK);

		public bool DeltaT_Readonly => Declaration != null || Shipment != null;
		public bool Declaration_Readonly => DeltaT != null || Shipment != null;
		public bool Shipment_Readonly => DeltaT != null || Declaration != null;

		public ForwardingShipmentCollection Shipments => shipments ?? (shipments = new ForwardingShipmentCollection(Factory));
		ForwardingShipmentCollection shipments;

		public JobDeclarationCollection Declarations => declarations ?? (declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK));
		JobDeclarationCollection declarations;

		public NctsHeaderCollection DeltaTs => deltaTs ?? (deltaTs = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany, new ZQuery(CusInBondHeaderSchema.BH_HeaderType, new ZString[] { EU.NCTS.Business.NctsMovementType.Codes.Arrival, EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival })));
		NctsHeaderCollection deltaTs;
	}
}
