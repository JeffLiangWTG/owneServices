using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EdiOrgMembershipFlattenedDataTransferProcessor : SimpleModuleDataTransferProcessor<EdiOrgMembership, EdiOrgMembershipFlattened>
	{
		public EdiOrgMembershipFlattenedDataTransferProcessor(EdiOrgMembershipCollection membershipCollection, IImportCollectionInfo collectionInfo)
			: base(membershipCollection, collectionInfo)
		{
		}

		Dictionary<string, EDIOrgHeader> orgCodeMap;

		public CodeDescriptionBoolCollection MembershipTypes
		{
			get => membershipTypes ?? (membershipTypes = EdiOrgMembershipLookups.GetMembershipTypes());
		}
		CodeDescriptionBoolCollection membershipTypes;

		public override void Import()
		{
			if (flattenedCollection.Count == 0)
			{
				return;
			}

			LoadExisting();
			base.Import();
		}

		void LoadExisting()
		{
			var orgCodes = flattenedCollection.Cast<EdiOrgMembershipFlattened>().Select(x => x.OrgCode);
			orgCodeMap = Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCodes))
				.ToDictionary(x => (string)x.OH_Code);
		}

		void AddErrorMessage(EdiOrgMembershipFlattened flat, string reason)
		{
			Log += Res.GetString("48E896DF-C3F8-4EC1-9217-2127FD3BE2AE", "Record [Org. Code: {0}] excluded: {1}"
				, flat.OrgCode, reason) + System.Environment.NewLine;
		}

		void AddInfoMessage(EdiOrgMembershipFlattened flat, string msg)
		{
			Log += Res.GetString("EF212747-55B8-4802-AEF5-D2C864FF2ABE", "Record [Org. Code: {0}]: {1}"
				, flat.OrgCode, msg) + System.Environment.NewLine;
		}

		protected override EdiOrgMembership CreateHeader(IBusinessObjectCollection headerCollection, EdiOrgMembershipFlattened flat)
		{
			EDIOrgHeader org;
			if (!orgCodeMap.TryGetValue(flat.OrgCode, out org))
			{
				AddErrorMessage(flat, Res.GetString("FE051281-3D87-4A50-A320-58728224AC37", "Org. Code not found"));
				return null;
			}

			if (!ValidateMembership(org.Memberships, flat))
			{
				return null;
			}

			EdiOrgMembership membership = null;
			var validFrom = flat.ValidFrom.Date;
			var validTo = flat.ValidTo.Date;

			foreach (var existing in org.Memberships.ToList())
			{
				if (existing.EOR_MembershipType == flat.MembershipType)
				{
					if (DateRange.HasOverlap(existing.EOR_ValidFrom, existing.EOR_ValidTo, validFrom, validTo)
						|| DateRange.IsConsecutive(existing.EOR_ValidFrom, existing.EOR_ValidTo, validFrom, validTo))
					{
						if (membership == null)
						{
							membership = existing;
							membership.EOR_ValidFrom = DateRange.Min(membership.EOR_ValidFrom, validFrom);
							membership.EOR_ValidTo = DateRange.Max(membership.EOR_ValidTo, validTo);
						}
						else
						{
							membership.EOR_ValidFrom = DateRange.Min(membership.EOR_ValidFrom, existing.EOR_ValidFrom);
							membership.EOR_ValidTo = DateRange.Max(membership.EOR_ValidTo, existing.EOR_ValidTo);
							existing.Delete();
						}
					}
				}
			}

			if (membership == null)
			{
				membership = org.Memberships.AddNew();
				membership.EOR_MembershipType = flat.MembershipType;
				membership.EOR_ValidFrom = validFrom;
				if (!validTo.IsEmpty)
				{
					membership.EOR_ValidTo = validTo;
				}
			}

			headerCollection.Add(membership);
			return membership;
		}

		bool ValidateMembership(EdiOrgMembershipCollection memberships, EdiOrgMembershipFlattened flat)
		{
			if (flat.MembershipType.IsEmpty || !MembershipTypes.ContainsCode(flat.MembershipType))
			{
				AddErrorMessage(flat, Res.GetString("4C584D8C-D228-4930-95C0-B600FAD4D75E", "Invalid Membership Type: {0}", flat.MembershipType));
				return false;
			}

			var validFrom = flat.ValidFrom.Date;
			var validTo = flat.ValidTo.Date;

			if (validFrom.IsEmpty || !validFrom.IsValid)
			{
				AddErrorMessage(flat, Res.GetString("08532046-E9B1-42D9-B231-B5D574B6F219", "Invalid Valid From"));
				return false;
			}

			if (!validTo.IsEmpty)
			{
				if (!validTo.IsValid || validTo < validFrom)
				{
					AddErrorMessage(flat, Res.GetString("63993E8C-50FE-44AB-B36F-93AFEEF2CB4A", "Invalid Valid To"));
					return false;
				}
			}

			foreach (var existing in memberships)
			{
				if (existing.EOR_MembershipType == flat.MembershipType)
				{
					if (existing.EOR_ValidTo == validTo && existing.EOR_ValidFrom == validFrom)
					{
						AddInfoMessage(flat, Res.GetString("376D598A-F9B3-4E43-83E3-51DAE1C063DC", "Already exists"));
						return false;
					}
					else if (existing.EOR_ValidTo.IsEmpty && validTo.IsEmpty)
					{
						if (existing.EOR_ValidFrom <= validFrom)
						{
							AddInfoMessage(flat, Res.GetString("E9A375C4-09E3-486B-AFE8-10D7587FBF75", "No change"));
							return false;
						}
					}
				}
			}

			return true;
		}
	}
}
