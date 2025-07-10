using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using static Enterprise.Customs.AU.Declaration.Business.ScanForOutturnManager;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ManualScanTarget
	{
		internal ManualScanTarget(IScanForOutturnManager manager, string barcode)
		{
			Barcode = BarcodeMatchMask(Argument.NotNull(barcode, "barcode"));
			this.manager = Argument.NotNull(manager, "manager");
			consignment = manager.ManifestCollection.FindByConsignmentRef(Barcode);
			Status = manager.GetStatus(RelatedBusinessObject);
		}

		internal ManualScanTarget(IScanForOutturnManager manager)
			: this(manager, string.Empty)
		{
			Status = ManualScanStatuses.AwaitingScanInput;
		}

		public string Barcode { get; private set; }
		public ManualScanStatuses Status { get; set; }
		public OutturnLine consignment { get; private set; }

		string BarcodeMatchMask(string input)
		{
			var sameOrgMasks = new List<OrgBarcodeMask>();

			foreach (OrgBarcodeMask maskItem in BarcodeMasks)
			{
				if (maskItem.Org == Env.CurrentCompany.OrganisationPK)
				{
					sameOrgMasks.Add(maskItem);
				}
			}

			return GetMatcher().GetMatch(sameOrgMasks.OrderBy(o => o.Priority).Select(x => x.Mask.ToString()).ToList(), input);
		}

		IMaskMatcher GetMatcher()
		{
			return new RegexMatcher();
		}

		OrgBarcodeMaskCollection BarcodeMasks
		{
			get
			{
				return registryCollection ?? (registryCollection = OrganisationsDataRegistry.Instance.OrgBarcodeMask.Value);
			}
		}

		OrgBarcodeMaskCollection registryCollection;

		public int NumberOfExpectedPackage
		{
			get
			{
				if (!HasMatchedConsignment)
				{
					return int.MaxValue;
				}
				else
				{
					return consignment.ManifestInfo.Quantity;
				}
			}
		}

		public int NumberOfScannedPackage
		{
			get
			{
				return manager.CountTotalNumberOfManualScansByBarcode(Barcode);
			}
		}

		public bool IsMatchedConsignmentCleared
		{
			get
			{
				return manager.IsMatchedConsignmentCleared(this);
			}
		}

		public bool IsSurplusPackage
		{
			get { return manager.IsPossibleSurplusPackage(consignment) && HasExpectedCountBeenReached; }
		}

		public bool IsSurplusConsignment
		{
			get { return manager.IsPossibleSurplusConsignment(consignment) && IsBarcodeScannedFirstTime; }
		}

		public bool IsInvalidConsignment
		{
			get { return !HasMatchedConsignment && manager.IsInvalidConsignment(this); }
		}

		bool HasMatchedConsignment
		{
			get { return consignment != null; }
		}

		bool IsBarcodeScannedFirstTime
		{
			get { return !manager.ManualScanHistory.ContainsBarcode(Barcode); }
		}

		bool HasExpectedCountBeenReached
		{
			get { return NumberOfScannedPackage >= NumberOfExpectedPackage; }
		}

		public BusinessObject RelatedBusinessObject
		{
			get { return relatedBusinessObject ?? (relatedBusinessObject = manager.GetRelatedBusinessObject(this)); }
		}
		BusinessObject relatedBusinessObject;

		readonly IScanForOutturnManager manager;
	}
}
