using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBDependentCollection : Customs.Business.CusHAWBDependentCollection
	{
		public CusHAWBDependentCollection(CusMAWB parent)
			: base(parent, GetFilter())
		{
			this.parentMawb = parent;
			this.CountChanged += new CollectionCountChangedEventHandler(CusHAWBDependentCollection_CountChanged);
		}

		static ZQuery GetFilter()
		{
			var q = new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false);
			q.AddToFilter(CusHAWBSchema.CS_IsActive, true);
			return q;
		}

		void CusHAWBDependentCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!parentMawb.IsDeleted)
			{
				if (Count > 0 && parentMawb.DescriptionOfGoods.IsEmpty)
				{
					parentMawb.DescriptionOfGoods = "Consolidation";
				}
				if (Count == 0 && parentMawb.DescriptionOfGoods.ToUpper() == "CONSOLIDATION")
				{
					parentMawb.DescriptionOfGoods = "";
				}
				parentMawb.DescriptionOfGoodsInfo.RefreshBinding();
			}
		}

		public new CusHAWB this[int index]
		{
			get { return (CusHAWB)Elements[index]; }
		}

		protected override bool AllowNewCore
		{
			get
			{
				if (!base.AllowNewCore)
				{
					return false;
				}

				if (parentMawb.Consol != null && parentMawb.Consol.IsDirect)
				{
					return false;
				}

				if (parentMawb.IsBasic && parentMawb.MasterLevelHouseHelper.HasDeclaration)
				{
					return false;
				}

				if (UserHasNoHawbSecurityRight)
				{
					return false;
				}

				return parentMawb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_AddChildAwb);
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				if (!base.AllowRemoveCore)
				{
					return false;
				}

				if (UserHasNoHawbSecurityRight)
				{
					return false;
				}

				return parentMawb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.Delete);
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || UserHasNoHawbSecurityRight; }
		}

		public new CusHAWB AddNew()
		{
			return (CusHAWB)base.AddNew();
		}

		protected override BusinessObject AddNewCore(System.Type bizOType)
		{
			return base.AddNewCore(typeof(CusHAWB));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			// Hawb inherits these properties from parent
			var hawb = (CusHAWB)child;
			hawb.CargoTerminalOperator = parentMawb.CargoTerminalOperator;
			hawb.CargoTerminalOperatorAirport = parentMawb.CargoTerminalOperatorAirport;
			hawb.AgentBadge = parentMawb.AgentBadge;
			hawb.ShipmentDescriptionCode = parentMawb.ShipmentDescriptionCode;
			hawb.CS_WeightUQ = parentMawb.WeightCode;
			hawb.Profile = parentMawb.Profile;
			var hawbAsAwb = (ICcsukCusAwb)hawb;
			hawbAsAwb.AirportOfArrival = parentMawb.AirportOfArrival;
			hawbAsAwb.AirportOfDestination = parentMawb.AirportOfDestination;
			if (((ICcsukCusAwb)parentMawb).IsCompleteOnCcsuk)
			{
				((ICcsukCusAwb)parentMawb).UncompleteOnCcsuk();
			}
		}

		bool UserHasNoHawbSecurityRight
		{
			get { return !Env.Security.AirCcsukHouse.IsAllowed; }
		}

		readonly CusMAWB parentMawb;
	}
}
