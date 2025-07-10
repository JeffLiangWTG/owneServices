using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Registry
{
	[DebuggerDisplay("BadgeCodeSetting. Badge={BadgeCode}; Port={RL_PortCode}; Dir={Direction}; CSP={CSPCode}")]
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class BadgeCodeSetting : RegistryBusinessObjectTemplate
	{
		protected abstract class Schema
		{
			public const string BadgeCode = "BadgeCode";
			public const string RL_PortCode = "RL_PortCode";
			internal const string BadgeDescription_DO_NOT_USE = "BadgeDescription";  // NOT USED!  Maintained to support old versions in StmData 
			public const string CSPCode = "CSPCode";
			internal const string MasterUCRPrefix_DO_NOT_USE = "MasterUCRPrefix";
			public const string Direction = "Direction";
			public const string MasterUcrCalculationMode = "MasterUcrCalculationMode";
			public const string IsPrimaryBadgeForBranch = "IsPrimaryBadgeForBranch";
			public const string ApplicationCode = "ApplicationCode";
			public const string IsNewBadge = "IsNewBadge";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BadgeCodeSetting(fallbackLevel, factory);
		}

		public BadgeCodeSetting()
		{
		}

		public BadgeCodeSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BadgeCodeSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Direction - import or export
		ZString fDirection;
		[MaxLength(3)]
		public ZString Direction
		{
			get { return fDirection; }
			set
			{
				SetNonPersistentPropertyValue(DirectionInfo, ref fDirection, value.ToUpper());
				SetMucrStyleForCcsuk();
				if (!IsValidationSuspended)
				{
					ValidateDirection();
				}
			}
		}

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(Schema.Direction); }
		}

		public BadgeDirectionList Direction_List
		{
			get { return new BadgeDirectionList(); }
		}

		public void ValidateDirection()
		{
			DirectionInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DirectionInfo, Direction_List, (NoResString)ErrorDirectionMustBeInListOrEmpty);
			CheckModeAndDirectionCompatible(DirectionInfo);
			CheckCspAndDirectionCompatible(DirectionInfo);
			ValidateMasterUcrCalculationMode();
		}

		void CheckCspAndDirectionCompatible(ZPropertyInfo prop)
		{
			if (!CSPCode.IsEmpty && prop.Value.ToString() != BadgeDirectionList.Codes.EXP && CSPCode == GatewayList.Codes.NES)
			{
				prop.AddError("NES is for exports only. It's the New EXPORTS System. Please select EXP for the direction or select a different CSP gateway.");
			}
		}

		void CheckModeAndDirectionCompatible(ZPropertyInfo prop)
		{
			if (!MasterUcrCalculationMode.IsEmpty && Direction != BadgeDirectionList.Codes.EXP)
			{
				if (MasterUcrCalculationMode == MucrGenerationStyles.Codes.Eori || MasterUcrCalculationMode == MucrGenerationStyles.Codes.Air || MasterUcrCalculationMode == MucrGenerationStyles.Codes.SeaConsol)
				{
					prop.AddError("This generation style is for exports only. Select a different mode or direction.");
				}
			}

			if (!MasterUcrCalculationMode.IsEmpty && Direction == BadgeDirectionList.Codes.Both && CSPCode == GatewayList.Codes.CCSUKviaNTMsgGW)
			{
				prop.AddError("You cannot select a MUCR generation style for CCSUK without specifying a direction.  If this badge is to be used for both import and exports, with different MUCR generation styles, create two rows in this grid (one for imports, one for exports)");
			}
		}
		// Will be shown as "Please enter a....." 
		public const string ErrorDirectionMustBeInListOrEmpty = "direction. Please specify either IMP for an import badge, EXP for an export badge, or leave blank for both";
		#endregion

		#region BadgeCode
		[MaxLength(3)]
		public ZString BadgeCode
		{
			get { return fBadgeCode; }
			set
			{
				SetNonPersistentPropertyValue(BadgeCodeInfo, ref fBadgeCode, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateBadgeCode();
				}
			}
		}
		ZString fBadgeCode;

		public ZPropertyInfo BadgeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BadgeCode); }
		}

		public void ValidateBadgeCode()
		{
			BadgeCodeInfo.ClearAllNotifications();
			if (BadgeCode.IsEmpty)
			{
				BadgeCodeInfo.AddError(ErrorMustHaveBadgeCode);
			}
		}
		public const string ErrorMustHaveBadgeCode = "You must have a valid Badge Code for each Badge Configuration.";
		#endregion

		#region RL_PortCode
		[MaxLength(5)]
		public ZString RL_PortCode
		{
			get { return fRL_PortCode; }
			set
			{
				SetNonPersistentPropertyValue(RL_PortCodeInfo, ref fRL_PortCode, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateRL_PortCode();
				}
			}
		}
		ZString fRL_PortCode;

		public ZPropertyInfo RL_PortCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RL_PortCode); }
		}

		public void ValidateRL_PortCode()
		{
			RL_PortCodeInfo.ClearAllNotifications();
			if (!RL_PortCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(RL_PortCodeInfo, PortCode_List, (NoResString)ErrorPortCodeMustBeAValidUNLOCO);
				if (!RL_PortCode.StartsWith("GB"))
				{
					RL_PortCodeInfo.AddError(ErrorPortCodeMustMustBeInGB);
				}
			}
		}
		public const string ErrorPortCodeMustBeAValidUNLOCO = "You must have a valid Port Code for each Badge Configuration, or leave it empty.";
		public const string ErrorPortCodeMustMustBeInGB = "Port Code specified must be in GB if you choose to specify one.";

		public IBusinessObjectCollection PortCode_List
		{
			get
			{
				return new RefUNLOCOCollection(CurrentFactory);
			}
		}

		#endregion

		#region CSPCode
		[MaxLength(5)]
		public ZString CSPCode
		{
			get { return fCSPCode; }
			set
			{
				SetNonPersistentPropertyValue(CSPCodeInfo, ref fCSPCode, value.ToUpper());
				if (CSPCode == GatewayList.Codes.NES)
				{
					Direction = BadgeDirectionList.Codes.EXP;
				}
				SetMucrStyleForCcsuk();
				if (!IsValidationSuspended)
				{
					ValidateCSPCode();
				}
			}
		}
		ZString fCSPCode;

		public ZPropertyInfo CSPCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CSPCode); }
		}

		public void ValidateCSPCode()
		{
			CSPCodeInfo.ClearAllNotifications();
			if (CSPCode.IsEmpty || !CSPCode_List.ContainsCode(CSPCode))
			{
				CSPCodeInfo.AddError(ErrorMustHaveCSP);
			}
		}
		public const string ErrorMustHaveCSP = "You must have a valid CSP/Communication Gateway for each Badge Configuration.";

		public GatewayList CSPCode_List
		{
			get { return new GatewayList(); }
		}
		#endregion

		#region ApplicationCode
		[ReadOnlyMember(nameof(IsApplicationCode_Readonly))]
		[MaxLength(3)]
		public ZString ApplicationCode
		{
			get { return fApplicationCode; }
			set
			{
				SetNonPersistentPropertyValue(ApplicationCodeInfo, ref fApplicationCode, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateApplicationCode();
				}
			}
		}
		ZString fApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		public ZBool ApplicationCodeIsChief => ApplicationCode == DeclarationApplicationCodeList.Codes.CHIEF || ApplicationCode.IsEmpty;

		public ZPropertyInfo ApplicationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ApplicationCode); }
		}

		public DeclarationApplicationCodeList ApplicationCode_List
		{
			get { return new DeclarationApplicationCodeList(); }
		}

		public void ValidateApplicationCode()
		{
			ApplicationCodeInfo.ClearAllNotifications();
			if (!ApplicationCode.IsEmpty && !ApplicationCode_List.ContainsCode(ApplicationCode))
			{
				ApplicationCodeInfo.AddError(ErrorMustHaveApplicationCodeOrEmpty);
			}

			if (ApplicationCodeIsChief && CSPCode != GatewayList.Codes.CCSUKviaNTMsgGW && IsNewBadge)
			{
				ApplicationCodeInfo.AddError(ErrorMustHaveApplicationCode);
			}
		}
		public const string ErrorMustHaveApplicationCodeOrEmpty = "You must have a valid Application Code, or leave it empty.";
		public const string ErrorMustHaveApplicationCode = "You must have a valid Application Code and CSP.";

		bool IsApplicationCode_Readonly =>
			(!GlbStaff.CurrentUser.GS_IsDeveloper && !GlbStaff.CurrentUser.IsSupportUser)
				&& (Direction == BadgeDirectionList.Codes.EXP
					&& IsApplicationCodeReadOnlyForExports
				|| (Direction == BadgeDirectionList.Codes.IMP
					&& IsApplicationCodeReadOnlyForImports)
				|| (Direction == BadgeDirectionList.Codes.Both
					&& (IsApplicationCodeReadOnlyForImports || IsApplicationCodeReadOnlyForExports))
			);

		bool IsApplicationCodeReadOnlyForExports => !IsEnabledForCdsExports
						&& !IsCdsFunctionalityValidForExports;

		bool IsApplicationCodeReadOnlyForImports => !IsEnabledForCdsImports
						&& !IsCdsFunctionalityValidForImports;

		bool IsEnabledForCdsExports => GBCustomsDataRegistry.Instance.CDSEnabledForExports.GetValueWithoutFallback(Guid.Empty, CurrentFallbackLevel.BranchPK, Guid.Empty);

		bool IsEnabledForCdsImports => GBCustomsDataRegistry.Instance.CDSEnabledForImports.GetValueWithoutFallback(Guid.Empty, CurrentFallbackLevel.BranchPK, Guid.Empty);

		bool IsCdsFunctionalityValidForExports => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now);

		bool IsCdsFunctionalityValidForImports => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now);

		#endregion

		#region Mucr calculation style
		[MaxLength(9)]
		public ZString MasterUcrCalculationMode
		{
			get { return masterUcrCalculationMode; }
			set
			{
				SetNonPersistentPropertyValue(MasterUcrCalculationModeInfo, ref masterUcrCalculationMode, value);
				if (!IsValidationSuspended)
				{
					ValidateMasterUcrCalculationMode();
				}
			}
		}
		ZString masterUcrCalculationMode;

		public ZPropertyInfo MasterUcrCalculationModeInfo
		{
			get { return GetZPropertyInfo(Schema.MasterUcrCalculationMode); }
		}

		public void ValidateMasterUcrCalculationMode()
		{
			MasterUcrCalculationModeInfo.ClearAllNotifications();
			if (!MasterUcrCalculationMode.IsEmpty && !MasterUcrCalculationMode_List.ContainsCode(MasterUcrCalculationMode))
			{
				MasterUcrCalculationModeInfo.AddError("Select a valid mode of calculation");
			}
			CheckModeAndDirectionCompatible(MasterUcrCalculationModeInfo);
		}

		public MucrGenerationStyles MasterUcrCalculationMode_List
		{
			get { return new MucrGenerationStyles(); }
		}
		#endregion

		void SetMucrStyleForCcsuk()
		{
			if (CSPCode == GatewayList.Codes.CCSUKviaNTMsgGW)
			{
				switch (Direction)
				{
					case BadgeDirectionList.Codes.EXP:
						MasterUcrCalculationMode = MucrGenerationStyles.Codes.Air;
						break;
					case BadgeDirectionList.Codes.IMP:
						MasterUcrCalculationMode = MucrGenerationStyles.Codes.GemsCcsuk;
						break;
					case BadgeDirectionList.Codes.Both:
						MasterUcrCalculationMode = "";
						break;
				}
			}
		}

		#region IsNewBadge

		ZBool fIsNewBadge;

		public ZBool IsNewBadge
		{
			get { return fIsNewBadge; }
			set
			{
				SetNonPersistentPropertyValue(IsNewBadgeInfo, ref fIsNewBadge, value);
				if (!IsValidationSuspended)
				{
					ValidateIsNewBadge();
				}
			}
		}

		void ValidateIsNewBadge()
		{
		}

		public ZPropertyInfo IsNewBadgeInfo
		{
			get { return GetZPropertyInfo(Schema.IsNewBadge); }
		}

		#endregion

		#region IsPrimaryBadgeForBranch
		ZBool fIsPrimaryBadgeForBranch;
		public ZBool IsPrimaryBadgeForBranch
		{
			get { return fIsPrimaryBadgeForBranch; }
			set
			{
				SetNonPersistentPropertyValue(IsPrimaryBadgeForBranchInfo, ref fIsPrimaryBadgeForBranch, value);
				if (!IsValidationSuspended)
				{
					ValidateIsPrimaryBadgeForBranch();
				}
			}
		}

		public void ValidateIsPrimaryBadgeForBranch()
		{
			IsPrimaryBadgeForBranchInfo.ClearAllNotifications();
			if (IsPrimaryBadgeForBranch)
			{
				// Look at the OTHER branches' badges
				var branches = new GlbBranch.Loader(CurrentFactory).LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.UnitedKingdom);
				foreach (var branch in branches)
				{
					if (CurrentFallbackLevel != null && branch.PK != CurrentFallbackLevel.BranchPK) // don't look at this branch
					{
						var badgesInOtherBranch = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
						if ((from BadgeCodeSetting b in badgesInOtherBranch where b.BadgeCode == BadgeCode && b.IsPrimaryBadgeForBranch select b).Any())
						{
							IsPrimaryBadgeForBranchInfo.AddError(string.Format("Badge {0} is already marked as primary for branch {1} in company {2}.", BadgeCode, branch.GB_Code, branch.Company.GC_Code));
							break;
						}
					}
				}
			}
		}

		public ZPropertyInfo IsPrimaryBadgeForBranchInfo
		{
			get { return GetZPropertyInfo(Schema.IsPrimaryBadgeForBranch); }
		}
		#endregion

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BadgeCode, BadgeCode);
			writer.WriteElementString(Schema.BadgeDescription_DO_NOT_USE, string.Empty); //Needed because clients may have old versions of the badgecodesetting in their StmData.  If we do not write, we barf when we read.
			writer.WriteElementString(Schema.RL_PortCode, RL_PortCode);
			writer.WriteElementString(Schema.CSPCode, CSPCode);
			writer.WriteElementString(Schema.MasterUCRPrefix_DO_NOT_USE, string.Empty);  //Needed because clients may have old versions of the badgecodesetting in their StmData.  If we do not write, we barf when we read.
			writer.WriteElementString(Schema.Direction, Direction);
			writer.WriteElementString(Schema.MasterUcrCalculationMode, MasterUcrCalculationMode);
			writer.WriteElementString(Schema.IsPrimaryBadgeForBranch, IsPrimaryBadgeForBranch.ToString());
			writer.WriteElementString(Schema.ApplicationCode, ApplicationCode);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			string throwAway = string.Empty; //Needed because clients may have old versions of the badgecodesetting in their StmData.  If we do not read, we barf.
			BadgeCode = reader.ReadElementString(Schema.BadgeCode);
			throwAway = reader.ReadElementString(Schema.BadgeDescription_DO_NOT_USE); //Needed because clients may have old versions of the badgecodesetting in their StmData.  If we do not write, we barf when we read.
			RL_PortCode = reader.ReadElementString(Schema.RL_PortCode);
			CSPCode = reader.ReadElementString(Schema.CSPCode);
			throwAway = reader.ReadElementString(Schema.MasterUCRPrefix_DO_NOT_USE);  //Needed because clients may have old versions of the badgecodesetting in their StmData.  If we do not write, we barf when we read.
			Direction = reader.ReadElementString(Schema.Direction);
			MasterUcrCalculationMode = reader.ReadElementString(Schema.MasterUcrCalculationMode);
			IsPrimaryBadgeForBranch = reader.ReadElementStringAsZBool(Schema.IsPrimaryBadgeForBranch);
			ApplicationCode = reader.ReadElementString(Schema.ApplicationCode);
		}
		#endregion

	}
}
