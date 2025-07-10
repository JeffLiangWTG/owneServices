using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ScanForOutturnManager : IScanForOutturnManager, IDisposable
	{
		public ScanForOutturnManager(ScanMasterBill scanObj)
		{
			this.scanObj = scanObj;
		}
		readonly ScanMasterBill scanObj;

		protected ScanMasterBill ScanObj
		{
			get { return scanObj; }
		}

		public ScanWizardDataSource ScanWizardDataSource
		{
			get { return fScanWizardDataSource ?? (fScanWizardDataSource = GetScanWizardDataSource(scanObj)); }
		}
		ScanWizardDataSource fScanWizardDataSource;

		protected BusinessObjectFactory Factory
		{
			get { return scanObj.Factory; }
		}

		public string ExportFileName
		{
			get
			{
				return String.Format("eManifest_{0}{1}_{2}.csv", scanObj.MasterBillNumber
					, !string.IsNullOrEmpty(scanObj.MasterBill.MasterHouseBill) ? "_" + scanObj.MasterBill.MasterHouseBill : string.Empty
					, ZDateTime.Now.ToString("yyyyMMddHHmm"));
			}
		}

		protected abstract ScanWizardDataSource GetScanWizardDataSource(ScanMasterBill scanObj);

		public ZBool IsStandaloneShipment
		{
			get { return scanObj.IsStandAlone; }
		}

		public virtual string ValidateSelectedUnderbond()
		{
			return scanObj.ValidateSelectedUnderbond();
		}

		public void SetSelectedShipment()
		{
			if (ScanWizardDataSource.ShipmentSelectorLineCollection != null)
			{
				scanObj.SelectedLines = ScanWizardDataSource.ShipmentSelectorLineCollection.GetSelectedLines();
			}
		}

		public abstract string ValidateStandAloneUnderbond();

		public UnderbondSelectorLine SelectedUnderbond
		{
			get { return selectedUnderbond; }
			set
			{
				selectedUnderbond = value;
				scanObj.SelectedUnderbond = value.Underbond;
			}
		}
		UnderbondSelectorLine selectedUnderbond;

		#region ScanningForOutturnMutex

		public string TryLockScan()
		{
			if (accessRestrictionManager != null)
			{
				accessRestrictionManager.Dispose();
				accessRestrictionManager = null;
			}

			accessRestrictionManager = ScanAccessRestrictionManager.LockScan(scanObj);
			return accessRestrictionManager.LockResult;
		}
		ScanAccessRestrictionManager accessRestrictionManager;

		#endregion

		#region Outturn Collection

		public OutturnLineCollection OutturnCollection
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

		protected ZBool IsOutturnCollectionEmpty
		{
			get { return outturnCollection == null; }
		}

		public bool IsOutturnCollectionCreatedAndHasMembers
		{
			get { return outturnCollection != null && OutturnCollection.Count > 0; }
		}

		public void MergeAutomaticScanResults(OutturnLineCollection scanCollection)
		{
			if (scanCollection.Count > 0)
			{
				HasScanHappened = true;
			}

			foreach (OutturnLine line in scanCollection)
			{
				if (OutturnCollection.ContainsConsignmentRef(line.ConsignmentRef))
				{
					OutturnCollection.FindByConsignmentRef(line.ConsignmentRef).Count += line.Count;
				}
				else
				{
					var surplusConsignment = GetNewOutturnLine();
					surplusConsignment.ConsignmentRef = line.ConsignmentRef;
					surplusConsignment.Status = nameof(ManifestStatuses.Held);
					surplusConsignment.Count = line.Count;
					OutturnCollection.Add(surplusConsignment);

					var surplusOutturnLine = new SurplusOutturnLine(surplusConsignment, ScanWizardDataSource.ShipmentSelectorLineCollection, Factory);
					surplusOutturnLine.ConsignmentRef = line.ConsignmentRef;
					ScanWizardDataSource.SurplusOutturnCollection.Add(surplusOutturnLine);
				}
			}
		}
		public bool HasScanHappened { get; set; }

		public int CountTotalNumberOfManualScansByBarcode(string barcode)
		{
			var numberOfCurrentManualScans = ManualScanHistory.CountNumberOfManualScansByBarcode((barcode));

			var outturnLine = OutturnCollection.FindByConsignmentRef(barcode);
			var numberOfPreviousScans = outturnLine == null ? 0 : (int)outturnLine.Count;

			return numberOfCurrentManualScans + numberOfPreviousScans;
		}

		public ManualScanLineCollection ManualScanHistory
		{
			get { return manualScanHistory ?? (manualScanHistory = new ManualScanLineCollection(Factory)); }
		}
		ManualScanLineCollection manualScanHistory;

		public string MergeManualScanResults()
		{
			if (ManualScanHistory.Count > 0)
			{
				HasScanHappened = true;
			}

			foreach (ManualScanLine line in ManualScanHistory)
			{
				if (OutturnCollection.ContainsConsignmentRef(line.Barcode))
				{
					OutturnCollection.FindByConsignmentRef(line.Barcode).Count++;
				}
				else
				{
					var newLine = GetNewOutturnLine();
					newLine.ConsignmentRef = line.Barcode;
					newLine.Status = nameof(ManifestStatuses.Held);
					newLine.Count = 1;
					OutturnCollection.Add(newLine);

					var surplusOutturnLine = new SurplusOutturnLine(newLine, ScanWizardDataSource.ShipmentSelectorLineCollection, Factory);
					surplusOutturnLine.ConsignmentRef = line.Barcode;
					ScanWizardDataSource.SurplusOutturnCollection.Add(surplusOutturnLine);
				}
			}

			NumberOfManualParcelScanned += ManualScanHistory.Count;
			ManualScanHistory.RemoveAndDeleteAll();
			return string.Empty;
		}
		public int NumberOfManualParcelScanned { get; private set; }

		public abstract string SaveOutturnResult(Customs.Business.ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem sender);

		protected ZString GetOutturnResultType(ZInt manifestPieces, ZInt landedPieces)
		{
			if (manifestPieces > landedPieces)
			{
				return CMROutturnResultType.Codes.ShortLanded;
			}
			else if (manifestPieces < landedPieces)
			{
				return CMROutturnResultType.Codes.SurplusPackages;
			}
			else
			{
				return CMROutturnResultType.Codes.NilDiscrepancy;
			}
		}

		#endregion

		#region Manifest Collection

		public OutturnLineCollection ManifestCollection
		{
			get
			{
				if (manifestCollection == null)
				{
					manifestCollection = GetManifestCollection();
				}

				return manifestCollection;
			}
		}
		OutturnLineCollection manifestCollection;

		OutturnLineCollection GetManifestCollection()
		{
			var collection = GetNewOutturnLineCollection();

			if (IsStandaloneShipment)
			{
				AddManifestLines(collection, scanObj.GetChildBills());
			}
			else
			{
				foreach (ShipmentSelectorLine shipment in ScanWizardDataSource.ShipmentSelectorLineCollection)
				{
					if (shipment.IncludeInScan)
					{
						AddManifestLinesForConsol(collection, shipment);
					}
				}
			}

			return collection;
		}

		public abstract OutturnLineCollection GetNewOutturnLineCollection();

		void AddManifestLines(OutturnLineCollection collection, IEnumerable<IScanHouseBillProvider> houseBills)
		{
			foreach (var houseBill in houseBills)
			{
				if (houseBill.ShouldScan(scanObj.SelectedUnderbond))
				{
					collection.Add(NewLine(houseBill, scanObj.SelectedUnderbond));
				}
			}
		}

		protected virtual void AddManifestLinesForConsol(OutturnLineCollection collection, ShipmentSelectorLine shipmentSelectorLine)
		{
			AddManifestLines(collection, shipmentSelectorLine.HouseBills);
		}

		protected OutturnLine NewLine(IScanHouseBillProvider houseBill, CusUnderbond underbond)
		{
			var result = GetNewOutturnLine();
			result.ManifestInfo = houseBill.GetManifestInformation(underbond);
			result.ConsignmentRef = houseBill.HouseBill;
			result.Status = ConvertToManifestStatuses(result.ManifestInfo.CustomsStatus).ToString();
			result.Count = 0;
			result.HouseBill = houseBill;
			result.Underbond = underbond;
			return result;
		}

		protected abstract OutturnLine GetNewOutturnLine();

		static ManifestStatuses ConvertToManifestStatuses(string customsStatus)
		{
			if (customsStatus == CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased)
			{
				return ManifestStatuses.Clear;
			}
			else
			{
				return ManifestStatuses.Held;
			}
		}

		#endregion

		#region Enums

		public enum ManifestStatuses
		{
			Clear,
			Held,
			Unknown
		}
		public const int MaifestStatusesMaxLength = 7;

		public enum ManualScanStatuses
		{
			AwaitingScanInput,
			Release,
			Hold,
			Unknow,
			SurplusPackage,
			Invalid
		}

		#endregion

		#region Dispose

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (accessRestrictionManager != null)
				{
					accessRestrictionManager.Dispose();
					accessRestrictionManager = null;
				}
			}
		}

		#endregion

		#region IScanForOutturnManager

		public ManualScanTarget CreateManualScanTarget(string barcode)
		{
			return new ManualScanTarget(this, barcode);
		}

		public ManualScanTarget CreateEmptyManualScanTarget()
		{
			return new ManualScanTarget(this);
		}

		bool IScanForOutturnManager.IsMatchedConsignmentCleared(ManualScanTarget target)
		{
			return target.consignment != null && target.consignment.Status == nameof(ScanForOutturnManager.ManifestStatuses.Clear);
		}

		bool IScanForOutturnManager.IsPossibleSurplusPackage(OutturnLine outturn)
		{
			return outturn != null;
		}

		bool IScanForOutturnManager.IsPossibleSurplusConsignment(OutturnLine outturn)
		{
			return outturn == null;
		}

		bool IScanForOutturnManager.IsInvalidConsignment(ManualScanTarget target)
		{
			return false;
		}

		BusinessObject IScanForOutturnManager.GetRelatedBusinessObject(ManualScanTarget target)
		{
			return target.consignment != null ? (BusinessObject)target.consignment.HouseBill : null;
		}

		ManualScanStatuses IScanForOutturnManager.GetStatus(BusinessObject relatedBusinessObject)
		{
			return ManualScanStatuses.Hold;
		}

		#endregion
	}
}
