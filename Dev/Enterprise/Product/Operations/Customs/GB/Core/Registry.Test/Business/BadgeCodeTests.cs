namespace Enterprise.Customs.GB.Registry.Testing
{
	using System;
	using CargoWise.Types;
	using Enterprise.Customs.GB.Registry.Business;
	using Enterprise.Customs.Universal;
	using Enterprise.Environment;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business.Testing;
	using Enterprise.ZArchitecture.Environment;
	using NUnit.Framework;
	using static Enterprise.Core.Constants;

	[TestedType(typeof(BadgeCodeSetting))]
	public class BadgeCodeSettingTest : RegistryBusinessObjectTemplateTestCase<BadgeCodeSetting>
	{
		public void TestReadElements()
		{
			BadgeCode.BadgeCode = "ABC";
			BadgeCode.RL_PortCode = "GBFXT";
			BadgeCode.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			BadgeCode.Direction = "EXP";
			BadgeCode.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Eori;
			BadgeCode.IsPrimaryBadgeForBranch = true;
			BadgeCode.ApplicationCode = "CDS";
			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(BadgeCodeSetting));
			ZBlob serialisedValue = dummyDataType.Serialise(BadgeCode);
			BadgeCodeSetting deserialisedBusinessObject = (BadgeCodeSetting)dummyDataType.Deserialise(serialisedValue);

			AssertEquals("ABC", deserialisedBusinessObject.BadgeCode);
			AssertEquals("GBFXT", deserialisedBusinessObject.RL_PortCode);
			AssertEquals("CCSUK", deserialisedBusinessObject.CSPCode);
			AssertEquals("EORI", deserialisedBusinessObject.MasterUcrCalculationMode.ToUpper());
			AssertEquals("EXP", deserialisedBusinessObject.Direction);
			AssertEquals(true, deserialisedBusinessObject.IsPrimaryBadgeForBranch);
			AssertEquals("CDS", deserialisedBusinessObject.ApplicationCode);
		}

		public void TestCcsukMucrGenerationDefaults()
		{
			BadgeCode.BadgeCode = "ABC";
			BadgeCode.CSPCode = GatewayList.Codes.CNS_CUSDECOnly;
			BadgeCode.Direction = BadgeDirectionList.Codes.EXP;
			AssertEquals("", BadgeCode.MasterUcrCalculationMode);
			BadgeCode.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			BadgeCode.Direction = BadgeDirectionList.Codes.Both;
			AssertEquals("", BadgeCode.MasterUcrCalculationMode);
			BadgeCode.Direction = BadgeDirectionList.Codes.EXP;
			AssertEquals(MucrGenerationStyles.Codes.Air, BadgeCode.MasterUcrCalculationMode);
			BadgeCode.Direction = BadgeDirectionList.Codes.IMP;
			AssertEquals(MucrGenerationStyles.Codes.GemsCcsuk, BadgeCode.MasterUcrCalculationMode);
		}

		public void TestBadgeCodeValidation()
		{
			BadgeCode.BadgeCode = "ABC";
			AssertNoNotifications(BadgeCode.BadgeCodeInfo);
			BadgeCode.BadgeCode = "";
			AssertHasError(BadgeCode.BadgeCodeInfo, BadgeCodeSetting.ErrorMustHaveBadgeCode);
			BadgeCode.BadgeCode = "DEF";
			AssertNoNotifications(BadgeCode.BadgeCodeInfo);
		}

		public void TestMucrGenerationStyleValidation()
		{
			BadgeCode.MasterUcrCalculationMode = "FART";
			AssertHasErrorContaining(BadgeCode.MasterUcrCalculationModeInfo, "valid");
			BadgeCode.MasterUcrCalculationMode = MucrGenerationStyles.Codes.GemsCcsuk;
			AssertNoErrorContaining(BadgeCode.MasterUcrCalculationModeInfo, "valid");

			BadgeCode.Direction = BadgeDirectionList.Codes.IMP;
			BadgeCode.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Air;
			AssertHasErrorContaining(BadgeCode.MasterUcrCalculationModeInfo, "exports only");
			BadgeCode.Direction = BadgeDirectionList.Codes.EXP;
			AssertNoErrorContaining(BadgeCode.DirectionInfo, "exports only");
			BadgeCode.Direction = BadgeDirectionList.Codes.IMP;
			BadgeCode.MasterUcrCalculationMode = MucrGenerationStyles.Codes.GemsCcsuk;
			AssertNoErrorContaining(BadgeCode.MasterUcrCalculationModeInfo, "exports only");

			BadgeCode.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			AssertNoErrorContaining(BadgeCode.MasterUcrCalculationModeInfo, "You cannot select a MUCR generation style for CCSUK without specifying a direction");
			BadgeCode.Direction = BadgeDirectionList.Codes.Both;
			BadgeCode.MasterUcrCalculationMode = MucrGenerationStyles.Codes.GemsCcsuk;
			AssertHasErrorContaining(BadgeCode.MasterUcrCalculationModeInfo, "You cannot select a MUCR generation style for CCSUK without specifying a direction");
		}

		public void TestMustHaveAPortCodeInGB()
		{
			BadgeCode.RL_PortCode = "ABC";
			AssertHasError(BadgeCode.RL_PortCodeInfo, BadgeCodeSetting.ErrorPortCodeMustBeAValidUNLOCO);
			BadgeCode.RL_PortCode = "AUSYD";
			AssertHasError(BadgeCode.RL_PortCodeInfo, BadgeCodeSetting.ErrorPortCodeMustMustBeInGB);
			BadgeCode.RL_PortCode = "GBFXT";
			AssertNoNotifications(BadgeCode.RL_PortCodeInfo);
			BadgeCode.RL_PortCode = "";
			AssertNoNotifications(BadgeCode.RL_PortCodeInfo);
		}

		public void TestCSPValidation()
		{
			BadgeCode.CSPCode = "XZXZ";
			AssertHasError(BadgeCode.CSPCodeInfo, BadgeCodeSetting.ErrorMustHaveCSP);
			BadgeCode.CSPCode = GatewayList.Codes.CNS_CUSDECOnly;
			AssertNoNotifications(BadgeCode.CSPCodeInfo);
			BadgeCode.CSPCode = "";
			AssertHasError(BadgeCode.CSPCodeInfo, BadgeCodeSetting.ErrorMustHaveCSP);
		}

		public void TestDirectionValidation()
		{
			BadgeCode.Direction = "DAN";
			AssertHasError(BadgeCode.DirectionInfo, "Enter a valid " + BadgeCodeSetting.ErrorDirectionMustBeInListOrEmpty + ".");
			BadgeCode.Direction = "";
			AssertNoNotifications(BadgeCode.DirectionInfo);
			BadgeCode.Direction = "EXP";
			AssertNoNotifications(BadgeCode.DirectionInfo);
			BadgeCode.Direction = "IMP";
			AssertNoNotifications(BadgeCode.DirectionInfo);
		}

		public void TestNesAndDirectionValidationAndDefaulting()
		{
			string errMsg = "NES is for exports only";
			BadgeCodeSetting badgeCode = new BadgeCodeSetting();
			badgeCode.CSPCode = GatewayList.Codes.NES;
			AssertEquals(BadgeDirectionList.Codes.EXP, badgeCode.Direction);

			badgeCode.Direction = "";
			AssertHasErrorContaining(badgeCode.DirectionInfo, errMsg);

			badgeCode.Direction = BadgeDirectionList.Codes.IMP;
			AssertHasErrorContaining(badgeCode.DirectionInfo, errMsg);

			badgeCode.Direction = BadgeDirectionList.Codes.EXP;
			AssertNoErrorContaining(badgeCode.DirectionInfo, errMsg);
		}

		public void TestIsPrimaryBadgeForBranchValidation()
		{
			// This validation will be added after discussion with Alex/Brett of the problem of only being able to read in-database values. 
			Assert(true);
		}

		public void TestApplicationCodeValidation()
		{
			BadgeCode.ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertNoMessageErrors(BadgeCode.ApplicationCodeInfo);
			BadgeCode.ApplicationCode = "ABC";
			AssertHasError(BadgeCode.ApplicationCodeInfo, "You must have a valid Application Code, or leave it empty.");
			BadgeCode.ApplicationCode = string.Empty;
			AssertNoMessageErrors(BadgeCode.ApplicationCodeInfo);
			BadgeCode.ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertNoMessageErrors(BadgeCode.ApplicationCodeInfo);
		}

		public void TestApplicationCodeIsReadOnlyWhenCDSNotAvailableAndNotDeveloperForExports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			GlbStaff.CurrentUser.GS_LoginName = "Non-Support-User";
			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be readonly when CDS not active and not a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should be readonly when CDS not active and not a developer and registry setting is set to false", badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsReadOnlyWhenCDSNotAvailableAndNotDeveloperForImports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			GlbStaff.CurrentUser.GS_LoginName = "Non-Support-User";
			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be readonly when CDS not active and not a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should be readonly when CDS not active and not a developer and registry setting is set to false", badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsNotReadOnlyWhenCDSNotAvailableAndDeveloperForExports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be readonly when CDS not active and a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be readonly when CDS not active and a developer and registry setting is set to false", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsNotReadOnlyWhenCDSNotAvailableAndDeveloperForImports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be readonly when CDS not active and a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be readonly when CDS not active and a developer and registry setting is set to false", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsNotReadOnlyWhenCDSAvailableAndNotDeveloperForExports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be read only when CDS is active and not a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should be read only when CDS is active and not a developer and registry setting is set to false", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsNotReadOnlyWhenCDSAvailableAndNotDeveloperForImports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be read only when CDS is active and not a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should be read only when CDS is active and not a developer and registry setting is set to false", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsNotReadOnlyWhenCDSAvailableAndDeveloperForExports()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be read only when CDS is active and also a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.EXP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be read only when CDS is active and also a developer and registry setting is set to false", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsNotReadOnlyWhenCDSAvailableAndDeveloperForImport()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be read only when CDS is active and also a developer and registry setting is set to true", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}

			GBCustomsDataRegistry.Instance.CDSEnabledForImports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var badgeCode = new BadgeCodeSetting();
				badgeCode.Direction = BadgeDirectionList.Codes.IMP;
				badgeCode.CurrentFallbackLevel = new FallbackLevel(null, GlbBranch.CurrentBranch, null);
				Assert("BadgeCodeSetting Application Code should not be read only when CDS is active and also a developer and registry setting is set to false", !badgeCode.ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestApplicationCodeIsChief()
		{
			BadgeCode.ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(false, BadgeCode.ApplicationCodeIsChief);
			BadgeCode.ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals(true, BadgeCode.ApplicationCodeIsChief);
			BadgeCode.ApplicationCode = "";
			AssertEquals(true, BadgeCode.ApplicationCodeIsChief);
		}

		#region Implementation
		BadgeCodeSetting BadgeCode
		{
			get
			{
				if (fBadgeCode == null)
				{
					fBadgeCode = new BadgeCodeSetting(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
				}
				return fBadgeCode;
			}
		}
		BadgeCodeSetting fBadgeCode;

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BadgeCodeSetting GetBusinessObjectToClone()
		{
			return new BadgeCodeSetting(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BadgeCodeSetting GetBusinessObjectToSerialise()
		{
			BadgeCode.BadgeCode = "ABC";
			BadgeCode.RL_PortCode = "GBFXT";
			BadgeCode.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			return BadgeCode;
		}
		#endregion
	}
}
