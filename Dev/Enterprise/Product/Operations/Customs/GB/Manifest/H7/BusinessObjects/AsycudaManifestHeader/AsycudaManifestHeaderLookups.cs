using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaManifestHeaderLookups : EU.H7.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public CodeDescriptionPairList ProfileList
		{
			get
			{
				var branchPK = Parent.Branch?.PK ?? ZGuid.Empty;
				var branchGuid = (branchPK.IsEmpty || !branchPK.IsValid ? GlbBranch.CurrentBranch.PK : branchPK).ToGuid();

				return Factory.GetCachedValue("Enterprise.Customs.GB.H7.Business.AsycudaManifestHeaderLookups|ProfileList|" + branchGuid.ToString() + "|" + Parent.TargetPort,  delegate
				{
					return GetProfileList(branchGuid, Parent.TargetPort);
				});
			}
		}

		CodeDescriptionPairList GetProfileList(Guid branchGuid, ZString targetPort)
		{
			var result = new CodeDescriptionPairList();
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branchGuid, Guid.Empty);

			var badgeCodes = badgeCodeSettings
				.OfType<BadgeCodeSetting>()
				.Where(x => (x.Direction == BadgeDirectionList.Codes.IMP || x.Direction == BadgeDirectionList.Codes.Both)
					&& (x.RL_PortCode.IsEmpty || targetPort.IsEmpty || x.RL_PortCode.Equals(targetPort)))
				.Select(x => x.BadgeCode)
				.Distinct()
				.OrderBy(x => x);

			foreach (var badgeCode in badgeCodes)
			{
				result.AddPair(badgeCode, string.Empty);
			}

			return result;
		}

		public CodeDescriptionPairList CSPList => Factory.GetCachedValue<GatewayList>();

		public new CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue("GBH7AsycudaManifestHeaderLookups|RegistrationStatusList", delegate
		{
			var billCustomsStatusList = AsycudaBillLookups.GetCustomsStatusList(Factory);
			var result = new CodeDescriptionPairList(billCustomsStatusList);
			result.Add(new CodeDescriptionPair("MLT", "Multiple statuses exist"));
			return result;
		});

		public OrganisationsFindBoxCollection SupervisingOfficeList => new OrganisationsFindBoxCollection(Factory);
	}
}
