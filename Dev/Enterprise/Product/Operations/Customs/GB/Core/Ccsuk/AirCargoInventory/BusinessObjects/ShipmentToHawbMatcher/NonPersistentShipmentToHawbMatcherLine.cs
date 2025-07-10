using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher
{
	public class NonPersistentShipmentToHawbMatcherLine : AutoNonPersistentShipmentToHawbMatcherLine
	{
		public NonPersistentShipmentToHawbMatcherLine(NonPersistentShipmentToHawbMatcherHeader header, BusinessObjectFactory factory) : base(factory)
		{
			this.Header = header;
		}

		internal readonly NonPersistentShipmentToHawbMatcherHeader Header;

		[ReadOnly(true)]
		public override ZString ShipmentHouseBill
		{
			get { return base.ShipmentHouseBill; }
			set { base.ShipmentHouseBill = value; }
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override ZBool CreateNewHawb
		{
			get { return base.CreateNewHawb; }
			set
			{
				base.CreateNewHawb = value;
				if (value)
				{
					CS = ZGuid.Empty;
					HawbNumber = CcsukUtilities.LeftPadWithZeros(ShipmentHouseBill.KeepNumericCharacters().Right(8));
				}
				RecalculateVisualisation();
			}
		}

		[ReadOnly(true)]
		[List(nameof(ShipmentsList))]
		public override ZGuid JS
		{
			get { return base.JS; }
			set
			{
				base.JS = value;
				RecalculateVisualisation();
			}
		}

		public BusinessObjectCollection ShipmentsList
		{
			get { return Header != null ? Header.Consol.Shipments : null; }
		}

		public List<CusHAWB> HawbsList
		{
			get
			{
				var result = new List<CusHAWB>();
				if (Header != null)
				{
					var hawbs = Header.SelectedMAWB?.ChildBills.OfType<CusHAWB>().Where(h => h.CS_JS.IsEmpty);
					result.AddRange(hawbs ?? Enumerable.Empty<CusHAWB>());
				}
				return result;
			}
		}

		[List(nameof(HawbsList))]
		[ReadOnlyMember(nameof(CSReadOnly))]
		public override ZGuid CS
		{
			get { return base.CS; }
			set
			{
				base.CS = value;
				HAWB = Factory.Load<CusHAWB>(CS);
				HawbNumber = HAWB != null ? HAWB.CS_HAWB.SubstringSafe(0, AutoNonPersistentShipmentToHawbMatcherLine.Schema.HawbNumberMaxLength) : ZString.Empty;
				RecalculateVisualisation();
			}
		}

		protected bool CSReadOnly
		{
			get { return CreateNewHawb; }
		}

		[ReadOnlyMember(nameof(HawbNumberReadOnly))]
		public override ZString HawbNumber
		{
			get { return base.HawbNumber; }
			set
			{
				base.HawbNumber = value;
				RecalculateVisualisation();
			}
		}

		protected bool HawbNumberReadOnly
		{
			get { return !CreateNewHawb; }
		}

		public CusHAWB HAWB { get; private set; }

		void RecalculateVisualisation()
		{
			if (Header != null)
			{
				Header.RecalculateVisualisation();
			}
		}
	}
}
