using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BranchFinderResult
	{
		public BranchFinderResult(GlbBranch branch, bool isCreationAllowed)
		{
			Branch = branch;
			IsCreationAllowed = isCreationAllowed;
		}

		public GlbBranch Branch { get; private set; }
		public bool IsCreationAllowed { get; private set; }

		public override string ToString()
		{
			return string.Format("{0}|{1}", Branch != null ? Branch.GB_Code.ToString() : string.Empty, IsCreationAllowed);
		}
	}

	public class BranchLocator
	{
		BranchLocator(Xsd.XmlInterchange interchange, BranchLocatorObjectWrapper shipmentOrConsolValueObject)
		{
			Argument.NotNull(interchange, "interchange");
			Argument.NotNull(shipmentOrConsolValueObject, "ShipmentOrConsolValueObject");

			this.interchange = interchange;
			this.valueObject = shipmentOrConsolValueObject;

			lazyBranches = new Lazy<IEnumerable<GlbBranch>>(() =>
				GlbCompany.CurrentCompany.Branches.Where(branch => branch.GB_IsActive));
		}

		readonly BranchLocatorObjectWrapper valueObject;
		readonly Xsd.XmlInterchange interchange;

		IEnumerable<GlbBranch> Branches
		{
			get { return lazyBranches.Value; }
		}
		readonly Lazy<IEnumerable<GlbBranch>> lazyBranches;

		public BranchFinderResult Find(ImportBranchRule importBranchRule)
		{
			BranchFinderResult result = null;

			foreach (Func<BranchFinderResult> method in GetFallbackMethods(importBranchRule))
			{
				result = method();

				if (result != null && (result.Branch != null || !result.IsCreationAllowed))
				{
					break;
				}
			}

			return result ?? new BranchFinderResult(GlbBranch.CurrentBranch, true);
		}

		public static BranchFinderResult Find(Xsd.XmlInterchange interchange, BranchLocatorObjectWrapper shipmentOrConsolValue)
		{
			BranchLocator locator = new BranchLocator(interchange, shipmentOrConsolValue);
			return locator.Find(shipmentOrConsolValue.ImportRule);
		}

		IEnumerable<Func<BranchFinderResult>> GetFallbackMethods(ImportBranchRule rule)
		{
			foreach (ImportBranchRule.BranchSearchRules searchRule in rule.GetBranchSearchRules())
			{
				switch (searchRule)
				{
					case ImportBranchRule.BranchSearchRules.FromInterchange:
						yield return GetBranchFromInterchange;
						break;
					case ImportBranchRule.BranchSearchRules.FromOriginLoadPort:
						yield return GetBranchRelatedToOriginLoadPort(IsBranchInUnloco);
						break;
					case ImportBranchRule.BranchSearchRules.FromOriginLoadPortCountry:
						yield return GetBranchRelatedToOriginLoadPort(IsBranchInCountry);
						break;
					case ImportBranchRule.BranchSearchRules.FromDestinationDischargePort:
						yield return GetBranchRelatedToDestinationDischargePort(IsBranchInUnloco);
						break;
					case ImportBranchRule.BranchSearchRules.FromDestinationDischargePortCountry:
						yield return GetBranchRelatedToDestinationDischargePort(IsBranchInCountry);
						break;
					case ImportBranchRule.BranchSearchRules.Any:
						yield return GetAnyBranch;
						break;
					case ImportBranchRule.BranchSearchRules.Cancel:
						yield return DoNotCreate;
						break;
				}
			}
		}

		BranchFinderResult GetBranchFromInterchange()
		{
			GlbBranch result = null;

			if (interchange.InterchangeInfoSpecified && interchange.InterchangeInfo.TargetSpecified && interchange.InterchangeInfo.Target.BranchCodeSpecified)
			{
				ZString branchCode = interchange.InterchangeInfo.Target.BranchCode;
				result = Branches.FirstOrDefault(companyBranch => companyBranch.GB_Code == branchCode);
			}

			return new BranchFinderResult(result, true);
		}

		Func<BranchFinderResult> GetBranchRelatedToOriginLoadPort(Func<GlbBranch, ZString, bool> isBranchMatch)
		{
			return () =>
			{
				GlbBranch result = null;

				if (!valueObject.OriginOrLoadPort.IsEmpty)
				{
					ZString unloco = valueObject.OriginOrLoadPort;

					result = Branches.FirstOrDefault(branch => isBranchMatch(branch, unloco));
				}

				return new BranchFinderResult(result, true);
			};
		}

		Func<BranchFinderResult> GetBranchRelatedToDestinationDischargePort(Func<GlbBranch, ZString, bool> isBranchMatch)
		{
			return () =>
			{
				GlbBranch result = null;

				if (!valueObject.DestinationOrDischargePort.IsEmpty)
				{
					ZString unloco = valueObject.DestinationOrDischargePort;

					result = Branches.FirstOrDefault(branch => isBranchMatch(branch, unloco));
				}

				return new BranchFinderResult(result, true);
			};
		}

		BranchFinderResult GetAnyBranch()
		{
			return new BranchFinderResult(GlbBranch.CurrentBranch, true);
		}

		BranchFinderResult DoNotCreate()
		{
			return new BranchFinderResult(null, false);
		}

		bool IsBranchInUnloco(GlbBranch branch, ZString unloco)
		{
			return branch.GB_RL_NKHomePort == unloco ||
				branch.ExtraPorts.Cast<GlbBranchExtraPorts>()
					.Any(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort == unloco);
		}

		bool IsBranchInCountry(GlbBranch branch, ZString unloco)
		{
			return branch.GB_RL_NKHomePort.SubstringSafe(0, 2) == unloco.SubstringSafe(0, 2) ||
				branch.ExtraPorts.Cast<GlbBranchExtraPorts>()
					.Any(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort.SubstringSafe(0, 2) == unloco.SubstringSafe(0, 2));
		}
	}
}
