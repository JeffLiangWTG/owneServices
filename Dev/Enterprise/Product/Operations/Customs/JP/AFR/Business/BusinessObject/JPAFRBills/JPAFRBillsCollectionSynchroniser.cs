using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRBillsCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public JPAFRBillsCollectionSynchroniser(JPAFRHeader header)
			: base(header.Consol, header)
		{
		}

		protected new JPAFRHeader Destination
		{
			get { return (JPAFRHeader)base.Destination; }
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Synchronise

		protected override void HookEvents()
		{
			base.HookEvents();
			HookInfoValueChanged(Destination.JPH_GB_BranchInfo);
			foreach (ForwardingShipment shipment in Source.Shipments)
			{
				HookShipmentChangeEvent(shipment);
			}
		}

		void HookInfoValueChanged(ZPropertyInfo info)
		{
			info.ValueChanged -= Info_ValueChanged;
			info.ValueChanged += Info_ValueChanged;
		}

		protected override void UnHookEvents()
		{
			Destination.JPH_GB_BranchInfo.ValueChanged -= Info_ValueChanged;
			foreach (ForwardingShipment shipment in Source.Shipments)
			{
				UnHookShipmentChangeEvent(shipment);
			}
			base.UnHookEvents();
		}

		void Info_ValueChanged(object sender, System.EventArgs e)
		{
			Synchronise();
		}

		void UnHookShipmentChangeEvent(ForwardingShipment shipment)
		{
			shipment.JS_ShipmentTypeInfo.ValueChanged -= Info_ValueChanged;
			shipment.JS_JS_ColoadMasterShipmentInfo.ValueChanged -= Info_ValueChanged;
			shipment.ConsignorPKInfo.ValueChanged -= Info_ValueChanged;
		}

		void HookShipmentChangeEvent(ForwardingShipment shipment)
		{
			HookInfoValueChanged(shipment.JS_ShipmentTypeInfo);
			HookInfoValueChanged(shipment.JS_JS_ColoadMasterShipmentInfo);
			HookInfoValueChanged(shipment.ConsignorPKInfo);
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = e.BizObject as ForwardingShipment;
			if (shipment != null)
			{
				if (e.ItemAdded)
				{
					HookShipmentChangeEvent(shipment);
				}
				else
				{
					UnHookShipmentChangeEvent(shipment);
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !Destination.JPH_OverrideFreightDefaults)
			{
				DeleteOrAddJPAFRBills();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.JPH_OverrideFreightDefaults)
			{
				HookSynchronisationForExistingBills();
			}
		}

		void HookSynchronisationForExistingBills()
		{
			var bills = new List<JPAFRBills>(Destination.Bills);
			var shipments = GetApplicableShipments(new TypedEnumerable<ForwardingShipment>(Source.Shipments));

			foreach (var shipment in shipments.ToArray())
			{
				var billNumber = shipment.GetTargetBillNumber(Destination.RegistryCompanyPK);
				var synchroniser = ElementSynchronisers.FindMatchingSource<JPAFRBillsynchroniser>(shipment);
				if (synchroniser != null)
				{
					var bill = synchroniser.Destination;
					bills.Remove(synchroniser.Destination);
					if (bill.IsBillAlreadyRegistered)
					{
						if (bill.JPB_BillNumber != billNumber)
						{
							synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
							ElementSynchronisers.Remove(synchroniser);
							synchroniser = null;
						}
					}
					else
					{
						var billAlreadyRegistered = bills.FirstOrDefault(x => !x.IsDeleted && x.IsBillAlreadyRegistered && x.JPB_BillNumber == billNumber);
						if (billAlreadyRegistered != null)
						{
							var synchroniserForBillAlreadyRegistered = ElementSynchronisers.FindMatchingDestination<JPAFRBillsynchroniser>(billAlreadyRegistered);
							if (synchroniserForBillAlreadyRegistered != null && !shipments.Contains(synchroniserForBillAlreadyRegistered.Source))
							{
								synchroniserForBillAlreadyRegistered.SetEnabled(false, synchroniserForBillAlreadyRegistered.DetectEnabled);
								ElementSynchronisers.Remove(synchroniserForBillAlreadyRegistered);
								synchroniserForBillAlreadyRegistered = null;
							}
							if (synchroniserForBillAlreadyRegistered == null)
							{
								synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
								ElementSynchronisers.Remove(synchroniser);
								synchroniser = null;
								ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new JPAFRBillsynchroniser(billAlreadyRegistered, shipment), IsEnabled, DetectEnabled);
								bills.Remove(billAlreadyRegistered);
							}
						}
					}
				}

				if (synchroniser == null)
				{
					JPAFRBills bill = null;
					while ((bill = bills.FirstOrDefault(x => !x.IsDeleted && x.JPB_BillNumber == billNumber)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<JPAFRBillsynchroniser>(bill);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new JPAFRBillsynchroniser(bill, shipment), IsEnabled, DetectEnabled);
							bills.Remove(bill);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							bills.Remove(bill);
						}
					}
				}
				else
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
				}
			}
		}

		static IEnumerable<ForwardingShipment> GetApplicableShipments(IEnumerable<ForwardingShipment> shipments)
		{
			var includeAsmCldClbSubShipmentsInManifest = JPAFRRegistry.Instance.IncludeAsmCldClbSubShipmentsInManifest.Value;
			var includeBCNSubShipmentsInManifest = JPAFRRegistry.Instance.IncludeBCNSubShipmentsInManifest.Value;

			bool IsShipmentIncluded(ForwardingShipment shipment)
			{
				var masterShipment = shipment.CoLoadMasterShipment;
				return masterShipment == null
					|| (includeAsmCldClbSubShipmentsInManifest && (masterShipment.IsCoLoadMaster || masterShipment.IsBlindCoLoadMaster || masterShipment.IsAssemblyMaster))
					|| (includeBCNSubShipmentsInManifest && masterShipment.IsBuyersConsolLead);
			}

			return shipments.Where(IsShipmentIncluded);
		}

		void DeleteOrAddJPAFRBills()
		{
			var shipments = GetApplicableShipments(Source.Shipments.ToArray<ForwardingShipment>()).ToList();
			var bills = new List<JPAFRBills>(Destination.Bills);

			while (bills.Count > 0)
			{
				var bill = bills[0];
				bills.Remove(bill);
				if (bill.IsDeleted)
				{
					var synchroniser = ElementSynchronisers.FindMatchingDestination<JPAFRBillsynchroniser>(bill);
					if (synchroniser != null)
					{
						synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
						ElementSynchronisers.Remove(synchroniser);
					}
				}
				else
				{
					var synchroniser = ElementSynchronisers.FindMatchingDestination<JPAFRBillsynchroniser>(bill);
					if (synchroniser != null)
					{
						if (shipments.Contains(synchroniser.Source))
						{
							shipments.Remove(synchroniser.Source);
							synchroniser.Synchronise();
							continue;
						}
					}
					else
					{
						bill = FindMatchingShipmentAndAddSynchroniser(shipments, bills, bill);
					}

					if (bill != null && !bill.IsBillAlreadyRegistered && !bill.IsMessagingInProgress)
					{
						bill.Delete();
					}
				}
			}

			foreach (var shipment in shipments)
			{
				var bill = Destination.Bills.AddNew();
				var synchroniser = new JPAFRBillsynchroniser(bill, shipment);
				ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
				synchroniser.Synchronise();
			}
		}

		JPAFRBills FindMatchingShipmentAndAddSynchroniser(List<ForwardingShipment> shipments, List<JPAFRBills> bills, JPAFRBills bill)
		{
			ForwardingShipment existingShipment = null;
			var alreadyProcessedShipments = new List<ForwardingShipment>();
			while ((existingShipment = shipments.FirstOrDefault(shipment => !alreadyProcessedShipments.Contains(shipment) && shipment.JS_HouseBill == bill.JPB_BillNumber)) != null)
			{
				alreadyProcessedShipments.Add(existingShipment);
				var synchroniser = ElementSynchronisers.FindMatchingSource<JPAFRBillsynchroniser>(existingShipment);
				if (synchroniser != null)
				{
					shipments.Remove(existingShipment);
					bills.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new JPAFRBillsynchroniser(bill, existingShipment);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					shipments.Remove(existingShipment);
					bill = null;
					break;
				}
			}
			return bill;
		}

		#endregion

		#region Hook/UnHook Events

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.Shipments;
		}

		#endregion
	}
}
