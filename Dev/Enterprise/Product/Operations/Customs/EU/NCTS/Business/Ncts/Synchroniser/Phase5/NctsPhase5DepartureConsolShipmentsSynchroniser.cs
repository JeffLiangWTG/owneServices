using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsPhase5DepartureConsolShipmentsSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public NctsPhase5DepartureConsolShipmentsSynchroniser(NctsHeader header, ForwardingConsol source)
			: base(source, header)
		{
		}

		protected new ForwardingConsol Source => (ForwardingConsol)base.Source;

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted)
			{
				DeleteOrAddBills();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted)
			{
				HookSynchronisationForExistingBills();
			}
		}

		IEnumerable<ForwardingShipment> GetSourceBillsToSynchronise() => Source.Shipments.ToArray<ForwardingShipment>();

		void HookSynchronisationForExistingBills()
		{
			var destBillsList = Destination.Bills.ToList();
			foreach (var sourceBill in GetSourceBillsToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<NctsPhase5DepartureShipmentToBillSynchroniser>(sourceBill);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
					destBillsList.Remove(synchroniser.Destination);
				}
				else
				{
					var sourceReference = sourceBill.JS_UniqueConsignRef;
					NctsBill destBill;
					while ((destBill = destBillsList.FirstOrDefault(x => !x.IsDeleted && x.B0_ReferenceID == sourceReference)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<NctsPhase5DepartureShipmentToBillSynchroniser>(destBill);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new NctsPhase5DepartureConsolShipmentToBillSynchroniser(destBill, sourceBill, Source), IsEnabled, DetectEnabled);
							destBillsList.Remove(destBill);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
							destBillsList.Remove(destBill);
						}
					}
				}
			}
		}

		void DeleteOrAddBills()
		{
			var sourceBills = GetSourceBillsToSynchronise().ToList();
			var existingDestBills = Destination.Bills.ToList();
			if (sourceBills.Count > 0)
			{
				while (existingDestBills.Count > 0)
				{
					var existingDestBill = existingDestBills[0];
					existingDestBills.Remove(existingDestBill);
					var synchroniser = ElementSynchronisers.FindMatchingDestination<NctsPhase5DepartureShipmentToBillSynchroniser>(existingDestBill);
					if (existingDestBill.IsDeleted || existingDestBill.IsDeleting)
					{
						if (synchroniser != null)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						if (synchroniser != null)
						{
							if (sourceBills.Contains(synchroniser.Source))
							{
								sourceBills.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							existingDestBill = FindMatchingBillAndAddSynchroniser(sourceBills, existingDestBills, existingDestBill);
						}

						if (existingDestBill != null)
						{
							existingDestBill.Delete();
						}
					}
				}

				foreach (var sourceBill in sourceBills)
				{
					var destinationBill = Destination.Bills.AddNew();
					var synchroniser = new NctsPhase5DepartureConsolShipmentToBillSynchroniser(destinationBill, sourceBill, Source);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				existingDestBills.ForEach(nctsBill =>
				{
					nctsBill.Delete();
				});
			}
		}

		NctsBill FindMatchingBillAndAddSynchroniser(List<ForwardingShipment> sourceBills, List<NctsBill> destBills, NctsBill destBill)
		{
			ForwardingShipment existingSourceBill;
			var alreadyProcessedSourceBills = new List<ForwardingShipment>();
			while ((existingSourceBill = sourceBills.FirstOrDefault(bill => !alreadyProcessedSourceBills.Contains(bill) && destBill.B0_ReferenceID == bill.JS_UniqueConsignRef)) != null)
			{
				alreadyProcessedSourceBills.Add(existingSourceBill);
				var synchroniser = ElementSynchronisers.FindMatchingSource<NctsPhase5DepartureShipmentToBillSynchroniser>(existingSourceBill);
				if (synchroniser != null)
				{
					sourceBills.Remove(existingSourceBill);
					destBills.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new NctsPhase5DepartureConsolShipmentToBillSynchroniser(destBill, existingSourceBill, Source);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					sourceBills.Remove(existingSourceBill);
					destBill = null;
					break;
				}
			}
			return destBill;
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent() => new[] { Source.Shipments };
	}
}
