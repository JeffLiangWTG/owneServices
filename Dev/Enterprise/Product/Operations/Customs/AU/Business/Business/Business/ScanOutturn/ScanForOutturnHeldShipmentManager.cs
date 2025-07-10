using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ScanForOutturnHeldShipmentManager : NonPersistentBusinessObject, IScanForOutturnManager, IObsoleteValidation
	{
		protected ScanForOutturnHeldShipmentManager(BusinessObjectFactory factory)
			: base(factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
			SetDefaultPremisID();
		}

		readonly BusinessObjectFactory factory;

		[CargoWise.ComponentModel.MaxLength(5)]
		public ZString OutturningPremiseID
		{
			get { return outturningPremiseID; }
			set
			{
				if (outturningPremiseID != value)
				{
					CheckMaximumLength(OutturningPremiseIDInfo, value);
					SetNonPersistentPropertyValue(OutturningPremiseIDInfo, ref outturningPremiseID, value);
					outturningPremiseID = value;
				}
			}
		}
		ZString outturningPremiseID;

		public ZPropertyInfo OutturningPremiseIDInfo
		{
			get { return GetZPropertyInfo(nameof(OutturningPremiseID)); }
		}

		void SetDefaultPremisID()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			OutturningPremiseID = currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID;
			if (OutturningPremiseID.IsEmpty)
			{
				OutturningPremiseID = GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID;
			}
		}

		#region IScanForOutturnManager Members

		bool IScanForOutturnManager.IsMatchedConsignmentCleared(ManualScanTarget target)
		{
			if (target.consignment != null)
			{
				return target.consignment.Status == nameof(ScanForOutturnManager.ManifestStatuses.Clear);
			}

			return target.Status == ScanForOutturnManager.ManualScanStatuses.Release;
		}

		bool IScanForOutturnManager.IsPossibleSurplusPackage(OutturnLine outturn)
		{
			return false;
		}

		bool IScanForOutturnManager.IsPossibleSurplusConsignment(OutturnLine outturn)
		{
			return false;
		}

		public ManualScanLineCollection ManualScanHistory
		{
			get { return manualScanHistory ?? (manualScanHistory = new ManualScanLineCollection(factory)); }
		}
		ManualScanLineCollection manualScanHistory;

		public OutturnLineCollection ManifestCollection
		{
			get { return manifestCollection ?? (manifestCollection = GetManifestCollection()); }
		}
		OutturnLineCollection manifestCollection;

		OutturnLineCollection GetManifestCollection()
		{
			var collection = GetNewOutturnLineCollection();
			foreach (var house in GetHouseBills())
			{
				collection.Add(NewLine(house));
			}
			return collection;
		}

		OutturnLineCollection OutturnCollection
		{
			get
			{
				if (outturnCollection == null)
				{
					outturnCollection = ManifestCollection.DeepCopy();
					outturnCollection.ResetOutturnLineCountField();
				}
				return outturnCollection;
			}
		}
		OutturnLineCollection outturnCollection;

		protected abstract IEnumerable<IScanHouseBillProvider> GetHouseBills();

		public void UpdateManifestCollectionIfNeeded()
		{
			if (manifestCollection != null)
			{
				var existingLines = new List<OutturnLine>(manifestCollection.Cast<OutturnLine>());
				manifestCollection.RemoveAll();
				foreach (var hawb in GetHouseBills())
				{
					var existingLine = existingLines.FirstOrDefault(x => x.HouseBill == hawb);
					if (existingLine == null)
					{
						manifestCollection.Add(NewLine(hawb));
					}
					else
					{
						manifestCollection.Add(existingLine);
						existingLines.Remove(existingLine);
					}
				}
				existingLines.ForEach(x => x.Delete());
			}
		}

		OutturnLine NewLine(IScanHouseBillProvider house)
		{
			var manifestInfo = house.GetManifestInformation(null);
			var result = GetNewOutturnLine();
			result.ConsignmentRef = house.HouseBill;
			result.Status = manifestInfo != null && IsCargoStatusClear(manifestInfo.CustomsStatus) ?
				nameof(ScanForOutturnManager.ManifestStatuses.Clear) : nameof(ScanForOutturnManager.ManifestStatuses.Held);
			result.HouseBill = house;
			return result;
		}

		public int CountTotalNumberOfManualScansByBarcode(string barcode)
		{
			return ManualScanHistory.CountNumberOfManualScansByBarcode((barcode));
		}

		public ManualScanTarget CreateManualScanTarget(string barcode)
		{
			return new ManualScanTarget(this, barcode);
		}

		public bool HasScanHappened { get; set; }

		public string MergeManualScanResults()
		{
			if (ManualScanHistory.Count > 0)
			{
				HasScanHappened = true;
			}

			var unknownConsignments = new ZStringBuilder();
			foreach (var line in ManualScanHistory.Cast<ManualScanLine>())
			{
				IScanHouseBillProvider house;
				var outturnLine = OutturnCollection.FindByConsignmentRef(line.Barcode);
				if (outturnLine != null)
				{
					house = outturnLine.HouseBill;
				}
				else
				{
					house = GetMatchingOutturnedHouseBill(line.Barcode);
				}
				if (house != null)
				{
					if (line.Instruction.ToUpper() == "RELEASE")
					{
						house.LogReadyForLocalDeliveryIfIsCargoStatusClear();
					}
				}
				else
				{
					unknownConsignments.Append(line.Barcode);
				}
			}

			NumberOfManualParcelScanned += ManualScanHistory.Count;
			ManualScanHistory.RemoveAndDeleteAll();

			return ReportUnmatchedIfNeeded(unknownConsignments);
		}

		string ReportUnmatchedIfNeeded(ZStringBuilder unknownConsignments)
		{
			var result = "";
			if (!unknownConsignments.IsEmpty)
			{
				unknownConsignments.Prepend("The following consignments could not be matched.\r\nEither they were not outturned in the selectecd establishment,\r\nor they do not have a 'CAD' event (not outturned),\r\nor might already have an 'RLD' event (ready for delivery).\r\n");
				result = unknownConsignments.ToStringWithNewLineBetweenAppends();
			}
			return result;
		}

		public ManualScanTarget CreateEmptyManualScanTarget()
		{
			return new ManualScanTarget(this);
		}

		public int NumberOfManualParcelScanned { get; private set; }

		public string ExportFileName
		{
			get { return string.Format(CultureInfo.InvariantCulture, "eManifest_HeldShipments_{0}.csv", ZDateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)); }
		}

		public abstract OutturnLineCollection GetNewOutturnLineCollection();
		protected abstract OutturnLine GetNewOutturnLine();

		public string MergeAutomaticScanResults(OutturnLineCollection scanCollection)
		{
			var result = "";

			if (scanCollection != null)
			{
				if (scanCollection.Count > 0)
				{
					HasScanHappened = true;
				}

				var unknownConsignments = new ZStringBuilder();
				foreach (var line in scanCollection.Cast<OutturnLine>())
				{
					if (OutturnCollection.ContainsConsignmentRef(line.ConsignmentRef))
					{
						if (line.Count > 0 && line.Status.ToUpper() == "CLEAR")
						{
							var outturnLine = OutturnCollection.FindByConsignmentRef(line.ConsignmentRef);
							if (outturnLine != null)
							{
								var house = outturnLine.HouseBill;
								if (house != null)
								{
									house.LogReadyForLocalDeliveryIfIsCargoStatusClear();
								}
							}
						}
					}
					else
					{
						var isUnknown = true;
						if (line.Count > 0)
						{
							var house = GetMatchingOutturnedHouseBill(line.ConsignmentRef);
							if (house != null)
							{
								house.ResetCargoReceivedAtDepotLogs("Reset by HELD scan");
								isUnknown = false;
							}
						}
						if (isUnknown)
						{
							unknownConsignments.Append(line.ConsignmentRef);
						}
					}
				}
				result = ReportUnmatchedIfNeeded(unknownConsignments);
			}
			return result;
		}

		protected abstract IScanHouseBillProvider GetMatchingOutturnedHouseBill(ZString houseBillNo);

		bool IScanForOutturnManager.IsInvalidConsignment(ManualScanTarget target)
		{
			return target.RelatedBusinessObject == null;
		}

		BusinessObject IScanForOutturnManager.GetRelatedBusinessObject(ManualScanTarget target)
		{
			return (BusinessObject)GetMatchingOutturnedHouseBill(target.Barcode);
		}

		#pragma warning disable IDE0001 // Prevent simplification to base class
		AirScanForOutturnManager.ManualScanStatuses IScanForOutturnManager.GetStatus(BusinessObject relatedBusinessObject)
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			var house = relatedBusinessObject as IScanHouseBillProvider;
			var manifest = house != null ? house.GetManifestInformation(null) : null;
			return manifest != null && IsCargoStatusClear(manifest.CustomsStatus) ?
				AirScanForOutturnManager.ManualScanStatuses.Release :
				AirScanForOutturnManager.ManualScanStatuses.Hold;
		}

		bool IsCargoStatusClear(ZString status)
		{
			var isConditionalClear = status == CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;

			if (isConditionalClear)
			{
				return IsConditionalClearToBeConsideredAsClear;
			}
			else
			{
				return CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(status);
			}
		}

		protected abstract bool IsConditionalClearToBeConsideredAsClear { get; }

		#endregion
	}
}
