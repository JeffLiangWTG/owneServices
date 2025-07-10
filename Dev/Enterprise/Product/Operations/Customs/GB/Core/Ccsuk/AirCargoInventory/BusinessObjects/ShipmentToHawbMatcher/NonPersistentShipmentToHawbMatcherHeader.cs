using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher
{
	public class NonPersistentShipmentToHawbMatcherHeader : AutoNonPersistentShipmentToHawbMatcherHeader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public NonPersistentShipmentToHawbMatcherHeader(ForwardingConsol consol, CusMAWB[] cusMawbs, IEnumerable<ForwardingShipment> shipments)
		{
			Consol = consol;
			Pivots = new NonPersistentShipmentToHawbMatcherLineCollection(Consol.Factory, this);
			this.cusMawbs = cusMawbs;
			DeleteAnyExistingLocalSplitOnBasicIfStatusISR = false;
			this.Shipments = shipments;
			DefaultMawb();
		}

		void DefaultMawb()
		{
			var count = cusMawbs.Length;

			if (count == 1)
			{
				SelectedMawbWrapperGUID = AvailableMawbs.OfType<CusMawbWrapper>().FirstOrDefault().PK;
			}
			else
			{
				var possibleMawb = AvailableMawbs.OfType<CusMawbWrapper>().Where(x => x.MAWB.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc || x.MAWB.CustomsActionCode.IsEmpty || x.MAWB.CustomsActionCode == CustomsStatusCodes.Codes.EntryOrRequestCancelled);

				SelectedMawbWrapperGUID = possibleMawb.Count() == 1 ? possibleMawb.FirstOrDefault().PK : ZGuid.Empty;
			}
		}

		void RefreshPivots()
		{
			Pivots.RemoveAndDeleteAll();

			foreach (ForwardingShipment shipment in Shipments)
			{
				if (CcsukUtilities.IsShipmentValidForCcsuk(shipment, Consol) && !ShipmentAlreadyHasAStronglyLinkedHawb(shipment))
				{
					var item = Pivots.AddNew();
					item.JS = shipment.PK;
					item.ShipmentHouseBill = shipment.JS_HouseBill;
					if (SelectedMAWB != null)
					{
						MatchToExistingCcsuk(SelectedMAWB, item);
					}
					item.HasChanges = false;  // Just to make NonPersistentShipmentToHawbMatcherLineCollectionTest.TestAddAndCancelOfElementAsThoughBinding() be quiet
				}
			}
		}

		public NonPersistentShipmentToHawbMatcherLineCollection Pivots { get; }

		public ForwardingConsol Consol { get; private set; }

		readonly CusMAWB[] cusMawbs;
		CusMawbWrapperCollection mawbWrappers;
		public CusMawbWrapperCollection AvailableMawbs
		{
			get
			{
				if (mawbWrappers == null)
				{
					mawbWrappers = new CusMawbWrapperCollection(new BusinessObjectFactory());
					foreach (var cm in this.cusMawbs)
					{
						var mw = new CusMawbWrapper(cm);
						mawbWrappers.Add(mw);
					}
				}
				return mawbWrappers;
			}
		}

		[List(nameof(AvailableMawbs))]
		public override ZGuid SelectedMawbWrapperGUID
		{
			get { return base.SelectedMawbWrapperGUID; }
			set
			{
				base.SelectedMawbWrapperGUID = value;  // value is the PK of the non-persistent CusMAwbWrapper
				RefreshPivots();
			}
		}

		public CusMAWB SelectedMAWB
		{
			get { return mawbWrappers.OfType<CusMawbWrapper>().FirstOrDefault(x => x.PK == SelectedMawbWrapperGUID)?.MAWB; }
		}

		IEnumerable<ForwardingShipment> Shipments { get; set; }

		bool ShipmentAlreadyHasAStronglyLinkedHawb(ForwardingShipment shipment)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			query.AddToFilter(CusHAWBSchema.CS_JS, shipment.PK);

			var subQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			var mawbQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mawbQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			subQuery.AddSubQuery(CusHAWBSchema.CS_CM, mawbQuery, JoinCondition.Or);

			var hawbQuery = new ZQuery(CusHAWBSchema.CS_CM, null);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			subQuery.AddToFilter(hawbQuery, JoinCondition.Or);
			query.AddToFilter(subQuery);

			var existingHawbOnThisShipment = shipment.Factory.LoadTop1<CusHAWB>(query);
			return existingHawbOnThisShipment != null;
		}

		void MatchToExistingCcsuk(CusMAWB mawb, NonPersistentShipmentToHawbMatcherLine item)
		{
			var hawb = (from CusHAWB cs in mawb?.ChildBills where cs.CS_JS == item.JS select cs).FirstOrDefault() ?? (from CusHAWB cs in mawb?.ChildBills where cs.CS_HAWB == item.ShipmentHouseBill.KeepNumericCharacters().PadLeft(8, '0') select cs).FirstOrDefault();  // Probably don't need this now that ShipmentAlreadyHasAStronglyLinkedHawb() precludes them, but no hard in keeping...

			if (hawb != null)
			{
				item.CS = hawb.PK;
				item.HawbNumber = hawb.CS_HAWB.Left(8);
			}
		}

		public CanProceedCheckResult CheckIfCanProceedToMakeOrMatchAll()
		{
			var checkResult = new CanProceedCheckResult();
			this.Validation.ValidateSelectedMawbWrapperGUID();
			checkResult.CanProceed = SelectedMawbWrapperGUIDInfo.Notifications.GetErrors().Count() == 0;
			if (checkResult.CanProceed)
			{
				foreach (NonPersistentShipmentToHawbMatcherLine line in Pivots)
				{
					if (line.CreateNewHawb)
					{
						line.Validation.ValidateCreateNewHawb();
						if (line.CreateNewHawbInfo.Notifications.GetErrors().Count() > 0)
						{
							checkResult.CanProceed = false;
							break;
						}
					}
				}
			}
			if (!checkResult.CanProceed)
			{
				checkResult.ErrorMessage = "Please fix the validation errors.";
			}
			return checkResult;
		}

		public bool MakeOrMatchAll()
		{
			// TODO - need to have the shipment's MUTEX passed over here so we can lock it

			if (!CheckIfCanProceedToMakeOrMatchAll().CanProceed)
			{
				return false;
			}

			RemoveBasicSplitsIfNecessary();

			var result = false;
			foreach (NonPersistentShipmentToHawbMatcherLine item in Pivots)
			{
				CusHAWB hawb = null;
				if (item.CreateNewHawb)
				{
					hawb = SelectedMAWB?.ChildBills.AddNew();
				}
				else
				{
					if (item.HAWB != null)
					{
						hawb = item.HAWB;
					}
				}

				if (hawb != null)  // It could be null if a HAWB existed on the MAWB but the user did not select it for any shipment
				{
					hawb.CS_JS = item.JS;
					hawb.CS_HAWB = item.HawbNumber;
					hawb.IsNonStandardHawbNumberSoDoNotSynch = hawb.CS_HAWB != item.ShipmentHouseBill;
					hawb.SynchroniseFromShipment(hawb.Shipment);
					result = true;
				}
			}
			return result;
		}

		void RemoveBasicSplitsIfNecessary()
		{
			var basic = SelectedMAWB;
			if ((DeleteAnyExistingLocalSplitOnBasicIfStatusISR)
			&& (basic.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc)
			&& (basic.Splits.Count > 0))
			{
				basic.Splits.RemoveAndDeleteAll();
			}
		}

		internal void RecalculateVisualisation()
		{
			var sb = new ZStringBuilder();
			foreach (ForwardingShipment shipment in Shipments)
			{
				var pivot = (from NonPersistentShipmentToHawbMatcherLine p in Pivots where p.JS == shipment.PK select p).FirstOrDefault();
				if (pivot != null)
				{
					sb.AppendFormat("{0} ({1})	--> {2}{3}", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill, pivot.HawbNumber, pivot.CreateNewHawb ? " *" : "");
				}
			}

			if (SelectedMAWB != null)
			{
				foreach (CusHAWB hawb in SelectedMAWB?.ChildBills)
				{
					var pivot = (from NonPersistentShipmentToHawbMatcherLine p in Pivots where p.CS == hawb.PK select p).FirstOrDefault();
					if (pivot == null)
					{
						if (hawb.CS_JS.IsEmpty)
						{
							sb.AppendFormat("(no shipment)		--> {0}", hawb.CS_HAWB);
						}
						else
						{
							sb.AppendFormat("{0} ({1})	--> {2} (already linked)", hawb.Shipment.JS_UniqueConsignRef, hawb.Shipment.JS_HouseBill, hawb.CS_HAWB);
						}
					}
				}
			}
			Status = sb.ToStringWithNewLineBetweenAppends();
		}

		protected override NonPersistentShipmentToHawbMatcherHeaderValidation GetNewValidation()
		{
			return new NonPersistentShipmentToHawbMatcherHeaderValidation(this);
		}
	}

	public class CanProceedCheckResult
	{
		public ZBool CanProceed { get; set; }
		public ZString ErrorMessage { get; set; }
	}
}
