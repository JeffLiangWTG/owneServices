using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoDepotLoadList : SeaCargoDepotBusinessObject
	{
		#region Schema

		public new abstract class Schema : SeaCargoDepotBusinessObject.Schema
		{
			public const string OceanBill = "OceanBill";
			public const string Vessel = "Vessel";
			public const string Voyage = "Voyage";
			public const string Lloyds = "Lloyds";
		}

		#endregion

		protected SeaCargoDepotLoadList(CFSLoadListConsol loadList)
			: base(loadList)
		{
			this.LoadList = loadList;
		}

		public static SeaCargoDepotLoadList Load(CFSLoadListConsol loadList)
		{
			return (SeaCargoDepotLoadList)Load(typeof(SeaCargoDepotLoadList), loadList);
		}

		#region Properties

		public readonly CFSLoadListConsol LoadList;

		public override CFSLoadListConsol ParentConsol
		{
			get { return LoadList; }
		}

		#region OceanBill

		public ZString OceanBill
		{
			get { return LoadList.JK_MasterBillNum; }
		}

		public ZPropertyInfo OceanBillInfo
		{
			get { return GetZPropertyInfo(Schema.OceanBill); }
		}

		#endregion

		#region Vessel

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString Vessel
		{
			get
			{
				ZString result = "";
				if (LoadList.Schedule != null && LoadList.Schedule.Voyage != null)
				{
					result = LoadList.Schedule.Voyage.JV_RV_NKVessel;
				}
				return result;
			}
		}

		public ZPropertyInfo VesselInfo
		{
			get { return GetZPropertyInfo(Schema.Vessel); }
		}

		#endregion

		#region Voyage

		[CargoWise.ComponentModel.MaxLength(10)]
		public ZString Voyage
		{
			get
			{
				ZString result = "";
				if (LoadList.Schedule != null && LoadList.Schedule.Voyage != null)
				{
					result = LoadList.Schedule.Voyage.JV_VoyageFlight;
				}
				return result;
			}
		}

		public ZPropertyInfo VoyageInfo
		{
			get { return GetZPropertyInfo(Schema.Voyage); }
		}

		#endregion

		#region Lloyds

		[CargoWise.ComponentModel.MaxLength(7)]
		public ZString Lloyds
		{
			get
			{
				ZString result = "";
				if (LoadList.Schedule != null && LoadList.Schedule.Voyage != null && LoadList.Schedule.Voyage.Vessel != null)
				{
					result = LoadList.Schedule.Voyage.Vessel.RV_LloydsNumber;
				}
				return result;
			}
		}

		public ZPropertyInfo LloydsInfo
		{
			get { return GetZPropertyInfo(Schema.Lloyds); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Containers

		public SeaCargoDepotContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new SeaCargoDepotContainerCollection(Factory);
					foreach (CFSContainer container in LoadList.Containers)
					{
						fContainers.Add(SeaCargoDepotContainer.Load(container));
					}
					LoadList.Containers.CountChanged += new CollectionCountChangedEventHandler(Containers_CountChanged);
				}
				return fContainers;
			}
		}
		SeaCargoDepotContainerCollection fContainers;

		#endregion

		#region Shipments

		public SeaCargoDepotShipmentCollection Shipments
		{
			get
			{
				if (fShipments == null)
				{
					fShipments = new SeaCargoDepotShipmentCollection(Factory);
					foreach (CFSShipment shipment in LoadList.Shipments)
					{
						fShipments.Add(SeaCargoDepotShipment.Load(shipment));
					}
					LoadList.Shipments.CountChanged += new CollectionCountChangedEventHandler(Shipments_CountChanged);
				}
				return fShipments;
			}
		}
		SeaCargoDepotShipmentCollection fShipments;

		#endregion

		#endregion

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CFSContainer container = e.BizObject as CFSContainer;
			if (container != null)
			{
				if (e.ItemAdded)
				{
					Containers.Add(SeaCargoDepotContainer.Load(container));
				}
				else if (e.ItemRemoved)
				{
					Containers.RemoveRelated(container);
				}
			}
		}

		void Shipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CFSShipment shipment = e.BizObject as CFSShipment;
			if (shipment != null)
			{
				if (e.ItemAdded)
				{
					Shipments.Add(SeaCargoDepotShipment.Load(shipment));
				}
				else if (e.ItemRemoved)
				{
					Shipments.RemoveRelated(shipment);
				}
			}
		}
	}
}
