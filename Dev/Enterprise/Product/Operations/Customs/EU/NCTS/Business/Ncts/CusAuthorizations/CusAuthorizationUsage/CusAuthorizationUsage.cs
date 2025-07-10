using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage, Integration.Customs.EU.NCTS.ICusAuthorizationUsage
	{
		public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly CusAuthorizationUsageTypeDecider TypeDecider = new CusAuthorizationUsageTypeDecider();

		public NctsHeader Header => AGC_ParentTableCode.ToUpperInvariant().ToString() switch
		{
			CusInBondHeaderSchema.Constants.Prefix => (NctsHeader)Parent,
			CusInBondMoveHeaderSchema.Constants.Prefix => MovementHeader?.Header,
			_ => null
		};

		public NctsDepartureMovementHeader MovementHeader => AGC_ParentTableCode.ToUpperInvariant().ToString() switch
		{
			CusInBondHeaderSchema.Constants.Prefix => Header?.MovementHeader,
			CusInBondMoveHeaderSchema.Constants.Prefix => (NctsDepartureMovementHeader)Parent,
			_ => null
		};

		[ResourceStringData("52C6C45C-1310-4A15-9B33-50FAEA5878EF", Caption = "Code")]
		public override ZString AGC_Code
		{
			get => base.AGC_Code;
			set
			{
				var oldValue = AGC_Code;
				base.AGC_Code = value;
				if (!IsCopying && oldValue != AGC_Code)
				{
					UpdateNumberAndOwner();
					DefaultAGC_Location();
					MovementHeader?.Validation.ValidateBM_ReducedDatasetIndicator();
				}
			}
		}

		public override ZGuid AGC_ParentID
		{
			get => base.AGC_ParentID;
			set
			{
				var oldValue = AGC_ParentID;
				base.AGC_ParentID = value;
				if (!IsCopying && oldValue != AGC_ParentID)
				{
					if (AGC_ParentTableCode.EqualsIgnoringCase(CusInBondHeaderSchema.Constants.Prefix) && (Header?.IsPhase5Departure ?? false))
					{
						throw new DeveloperNotificationException("Trying to add CusAuthorizationUsage on NctsHeader for Phase 5 Departure. For Phase 5 Departure CusAuthorizationUsage should be added to MovementHeader");
					}
				}
			}
		}

		public override ZString AGC_ParentTableCode
		{
			get => base.AGC_ParentTableCode;
			set
			{
				var oldValue = AGC_ParentTableCode;
				base.AGC_ParentTableCode = value;
				if (!IsCopying && oldValue != AGC_ParentTableCode)
				{
					if (AGC_ParentTableCode.EqualsIgnoringCase(CusInBondHeaderSchema.Constants.Prefix) && (Header?.IsPhase5Departure ?? false))
					{
						throw new DeveloperNotificationException("Trying to add CusAuthorizationUsage on NctsHeader for Phase 5 Departure. For Phase 5 Departure CusAuthorizationUsage should be added to MovementHeader");
					}
				}
			}
		}

		[ResourceStringData("B0B5F024-94A3-45CE-AC66-948FD27ADBB9", Caption = "Authorization Number", MediumCaption = "Auth. Number", ShortCaption = "Auth. No.")]
		public override ZString AGC_Number
		{
			get => base.AGC_Number;
			set => base.AGC_Number = value;
		}

		[ResourceStringData("639F970E-2B82-4F71-B4CD-D6598FA8005F", Caption = "Authorization Owner", MediumCaption = "Auth. Owner", ShortCaption = "Owner")]
		public override ZGuid AGC_OH_Owner
		{
			get => base.AGC_OH_Owner;
			set => base.AGC_OH_Owner = value;
		}

		protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);

		public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;

		protected sealed override CusAuthorizationUsageValidation GetNewValidation() => Header is { IsPhase5: true } ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusAuthorizationUsagePhase5Validation GetNewPhase5Validation() => new CusAuthorizationUsagePhase5Validation(this);

		protected virtual CusAuthorizationUsageValidation GetNewPhase4Validation() => new CusAuthorizationUsageValidation(this);

		void UpdateNumberAndOwner()
		{
			var header = Header;
			if (header.IsPhase5Departure && !AGC_Code.IsEmpty && header.Principal.Organisation is OrgHeader principalOrg)
			{
				var principalOrgPK = principalOrg.PK;
				AGC_OH_Owner = principalOrgPK;

				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, header.CountryCode, new[] { AGC_Code }, ZDate.Today, principalOrgPK);
				if (authorizations.Length == 1)
				{
					var authorisationHeader = authorizations[0];
					AGC_Number = authorisationHeader.CPH_Number;
					SetAGC_Location(authorisationHeader);
				}
			}
		}
	}
}
