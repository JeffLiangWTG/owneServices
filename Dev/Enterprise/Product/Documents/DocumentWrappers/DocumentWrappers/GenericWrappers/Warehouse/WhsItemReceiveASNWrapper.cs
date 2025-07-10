using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemReceiveASNWrapper : WarehouseJobGenericWrapper
	{
		public WhsItemReceiveASNWrapper(WhsItemReceiveASN receiveASN, BusinessObjectFactory factory)
			: base(receiveASN, factory)
		{
		}

		#region Headers

		protected override ZString JobNumberHeadingCore
		{
			get { return ZString.Empty; }
		}

		protected override ZString JobNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = ReceiveASN.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();
			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = Factory.Load<WhsWarehouse>(ReceiveASN.WRP_WW_IntendedWarehouse);
				return warehouse != null
					? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(WarehouseTitle, warehouse, Factory))
					: null;
			}
		}

		#endregion

		#region WarehouseName

		public override LabelValuePairWrapper WarehouseName
		{
			get
			{
				var warehouse = Factory.Load<WhsWarehouse>(ReceiveASN.WRP_WW_IntendedWarehouse);
				return new LabelValuePairWrapper(WarehouseTitle,
					warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty, Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("9f448d39-f071-429f-8b98-3c3794d64e53", "Warehouse");

		#endregion

		#region TransportReference

		public override LabelValuePairWrapper TransportReference
		{
			get
			{
				return new LabelValuePairWrapper(Res.GetString("0c1fd57c-99f4-40ba-9d6b-032d54c67624", "Vehicle Reference"),
					ReceiveASN.WRP_VehicleReference, Factory);
			}
		}

		#endregion

		#region TransportCompany

		public override OrganisationWrapper TransportCompany
		{
			get
			{
				var transportCoAddress = ((IDocAddresses)ReceiveASN).DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
				return new OrganisationWrapper(OrganisationUsageType.TransportCompany, transportCoAddress, Factory);
			}
		}

		#endregion

		#region JobType

		protected override ZString JobTypeCore => ReceiveASN.ASNType;

		#endregion

		#region TransportMode

		protected override ZString TransportModeCore => ReceiveASN.WRP_TransportMode;

		#endregion

		#region CompleteTime

		protected override ZDateTime CompleteTimeCore => ReceiveASN.WRP_CompleteTime.ToLocalZDateTime();

		#endregion

		#region WarehouseExpectedArrivalTime

		protected override ZDateTime GetWarehouseExpectedArrivalTime => ReceiveASN.WRP_ETA;

		#endregion

		#region Implementation

		WhsItemReceiveASN ReceiveASN => receiveASN ?? (receiveASN = (WhsItemReceiveASN)WrappedBO);
		WhsItemReceiveASN receiveASN;

		#endregion
	}
}
