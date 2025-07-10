using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CargoControlNumberCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public CargoControlNumberCollectionSynchroniser(ForwardingShipment source, JobDeclaration declaration)
			: base(source, declaration)
		{
		}

		public new JobDeclaration Destination => (JobDeclaration)base.Destination;

		public new ForwardingShipment Source => (ForwardingShipment)base.Source;

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.Numbers;

			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					yield return worker.Numbers;
				}
			}
		}

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);

			if (!isDeleted && !SyncChangesDetected && Destination.ShouldSynchroniseWithShipment())
			{
				DeleteOrAddCargoControlNumber();
			}
		}

		void DeleteOrAddCargoControlNumber()
		{
			var sourceCCNs = new List<CusEntryNumber>(GetSourceCargoControlNumberToSynchronise());
			if (sourceCCNs != null && sourceCCNs.Count > 0)
			{
				var cargoControlNumbers = new List<CargoControlNumber>(new TypedEnumerable<CargoControlNumber>(Destination.CargoControlNumbers));
				while (cargoControlNumbers.Count > 0)
				{
					var cargoControlNumber = cargoControlNumbers[0];
					cargoControlNumbers.Remove(cargoControlNumber);
					if (cargoControlNumber.IsDeleted)
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<CargoControlNumberSynchroniser>(cargoControlNumber);
						if (synchroniser != null)
						{
							synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<CargoControlNumberSynchroniser>(cargoControlNumber);
						if (synchroniser != null)
						{
							if (sourceCCNs.Contains(synchroniser.Source))
							{
								sourceCCNs.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							cargoControlNumber = FindMatchingCargoControlNumberAndAddSynchroniser(sourceCCNs, cargoControlNumbers, cargoControlNumber);
						}

						if (cargoControlNumber != null)
						{
							cargoControlNumber.Delete();
						}
					}
				}

				foreach (var sourceCCN in sourceCCNs)
				{
					var ccn = Destination.CargoControlNumbers.AddNew();
					var synchroniser = new CargoControlNumberSynchroniser(ccn, sourceCCN);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				Destination.CargoControlNumbers.DeleteAll();
			}
		}

		CargoControlNumber FindMatchingCargoControlNumberAndAddSynchroniser(List<CusEntryNumber> sourceCCNs, List<CargoControlNumber> cargoControlNumbers, CargoControlNumber ccn)
		{
			CusEntryNumber existingCCN = null;
			var alreadyProcessedCCNs = new List<CusEntryNumber>();
			while ((existingCCN = sourceCCNs.FirstOrDefault(sourceCCN => !alreadyProcessedCCNs.Contains(sourceCCN) && sourceCCN.CE_EntryNum == ccn.CY_CargoControlNumber)) != null)
			{
				alreadyProcessedCCNs.Add(existingCCN);
				var synchroniser = ElementSynchronisers.FindMatchingSource<CargoControlNumberSynchroniser>(existingCCN);
				if (synchroniser != null)
				{
					sourceCCNs.Remove(existingCCN);
					cargoControlNumbers.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new CargoControlNumberSynchroniser(ccn, existingCCN);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					sourceCCNs.Remove(existingCCN);
					ccn = null;
					break;
				}
			}

			return ccn;
		}

		protected override void HookElementSynchronisers()
		{
			if (Destination.ShouldSynchroniseWithShipment())
			{
				HookSynchronisationForExistingCargoControlNumbers();
			}
		}

		void HookSynchronisationForExistingCargoControlNumbers()
		{
			var cargoControlNumbers = new List<CargoControlNumber>(new TypedEnumerable<CargoControlNumber>(Destination.CargoControlNumbers));
			foreach (var sourceCCN in GetSourceCargoControlNumberToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<CargoControlNumberSynchroniser>(sourceCCN);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					cargoControlNumbers.Remove(synchroniser.Destination);
				}
				else
				{
					var ccnFromSource = sourceCCN.CE_EntryNum;
					CargoControlNumber ccn = null;
					while ((ccn = cargoControlNumbers.FirstOrDefault(x => !x.IsDeleted && x.CY_CargoControlNumber == ccnFromSource)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CargoControlNumberSynchroniser>(ccn);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CargoControlNumberSynchroniser(ccn, sourceCCN), IsEnabled, DetectEnabled);
							cargoControlNumbers.Remove(ccn);
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							cargoControlNumbers.Remove(ccn);
						}
					}
				}
			}
		}

		IEnumerable<CusEntryNumber> GetSourceCargoControlNumberToSynchronise()
		{
			var ccnList = new List<CusEntryNumber>();
			ccnList.AddRange(Source.Numbers.OfType<CusEntryNumber>().Where(x => x.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN));

			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments.ToArray<ForwardingShipment>())
				{
					ccnList.AddRange(worker.Numbers.OfType<CusEntryNumber>().Where(x => x.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN));
				}
			}

			return ccnList;
		}
	}
}
