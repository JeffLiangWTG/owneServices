//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoECCCPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoECCCPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ECCCPGAHeaderAddInfoValidation : AutoECCCPGAHeaderAddInfoValidation
	{
		public ECCCPGAHeaderAddInfoValidation(AutoECCCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		ECCCPGAHeader ECCCHeader
		{
			get { return ((ECCCPGAHeaderAddInfo)Parent).Parent as ECCCPGAHeader; }
		}

		protected override void CheckCA_IntendedUseCode()
		{
			base.CheckCA_IntendedUseCode();

			var header = ECCCHeader;
			if (header != null && (Parent.CA_WENProgramInd == YesNoList.Codes.Yes || Parent.CA_WRMProgramInd == YesNoList.Codes.Yes))
			{
				if (Parent.CA_WENProgramInd == YesNoList.Codes.Yes)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_IntendedUseCodeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodeList);
			}
		}

		protected override void CheckCA_ScientificName()
		{
			base.CheckCA_ScientificName();
			if (Parent.CA_WENProgramInd == YesNoList.Codes.Yes && Parent.CA_ComplianceDeclaration)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ScientificNameInfo);
			}
		}

		protected override void CheckCA_Sex()
		{
			base.CheckCA_Sex();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_SexInfo, Parent.Lookups.SexList);
		}

		protected override void CheckCA_LifeStage()
		{
			base.CheckCA_LifeStage();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_LifeStageInfo, Parent.Lookups.LifeStages);
		}

		protected override void CheckCA_SourceOfSpecimen()
		{
			base.CheckCA_SourceOfSpecimen();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_SourceOfSpecimenInfo, Parent.Lookups.SourceOfSpecimenList);
			if (Parent.CA_WENProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_SourceOfSpecimenInfo);
			}
		}

		protected override void CheckCA_ProcessCode()
		{
			base.CheckCA_ProcessCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_ProcessCodeInfo, Parent.Lookups.ProcessCodeList);

			if (Parent.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ProcessCodeInfo);

				var complianceStatementsProvided = Parent.CA_NationalMark
					|| Parent.CA_EPACertified
					|| Parent.CA_CanadaUnique
					|| Parent.CA_Incomplete
					|| (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && Parent.CA_Transition)
					|| Parent.CA_BulkReporting;

				if (Parent.Lookups.ProcessCodeList.ContainsCode(Parent.CA_ProcessCode) && !complianceStatementsProvided)
				{
					Parent.CA_ProcessCodeInfo.AddMessageError(Res.GetString("2250de85-a233-45be-920f-d27723437a41", "At least one of compliance statements must be provided."));
				}
			}
		}

		protected override void CheckCA_VehicleClass()
		{
			base.CheckCA_VehicleClass();
			var processCode = Parent.CA_ProcessCode;
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_VehicleClassInfo, Parent.Lookups.VehicleClassList);
			if (processCode == ProcessCodes.Codes.XE01 || processCode == ProcessCodes.Codes.XE04)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_VehicleClassInfo);
			}
		}

		protected override void CheckCA_EngineClass()
		{
			base.CheckCA_EngineClass();
			if (Parent.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				var processCode = Parent.CA_ProcessCode;
				if (processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_EngineClassInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_EngineClassInfo, Parent.Lookups.EngineClassList);
			}
		}

		protected override void CheckCA_MachineManufacturer()
		{
			base.CheckCA_MachineManufacturer();

			var parent = Parent;
			var processCode = parent.CA_ProcessCode;
			var engineClass = parent.CA_EngineClass;
			if (processCode == ProcessCodes.Codes.XE02 && (engineClass == ECCCProductCategories.Codes.EC16 || engineClass == ECCCProductCategories.Codes.EC20 || engineClass == ECCCProductCategories.Codes.EC0B || engineClass == ECCCProductCategories.Codes.EC0D)
				|| processCode == ProcessCodes.Codes.XE03)
			{
				var ecccHeader = ECCCHeader;
				if (ecccHeader.InvoiceLine is JobComInvoiceLine invoiceLine)
				{
					if (invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice && invoice.JZ_OA_ManufacturerAddress.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(parent.CA_MachineManufacturerInfo);
					}
					if (parent.CA_MachineManufacturer.IsValid &&
						invoiceLine.Declaration is JobDeclaration declaration &&
						declaration.IsIID &&
						ecccHeader.MachineManufacturer is OrgAddress orgAddress)
					{
						CAAddressValidator.ValidateMandatory(orgAddress, parent.CA_MachineManufacturerInfo, Res.GetString("E4780C71-68DE-491D-AEF3-6CA20DB4002A", "Manufacturer"));
					}
				}
			}
		}

		protected override void CheckCA_EngineManufacturer()
		{
			base.CheckCA_EngineManufacturer();
			var processCode = Parent.CA_ProcessCode;
			if (processCode == ProcessCodes.Codes.XE02
				|| processCode == ProcessCodes.Codes.XE03
				|| ECCCHeader.IsEngineDetailsRequiredForXE01)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_EngineManufacturerInfo);
			}
			else if (processCode == ProcessCodes.Codes.XE04)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.CA_EngineManufacturerInfo);
			}
		}

		protected override void CheckCA_MakeOfEngine()
		{
			base.CheckCA_MakeOfEngine();
			var processCode = Parent.CA_ProcessCode;
			if (processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_MakeOfEngineInfo);
			}
			else if (processCode == ProcessCodes.Codes.XE04)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.CA_MakeOfEngineInfo);
			}
		}

		protected override void CheckCA_EngineIDNumber()
		{
			base.CheckCA_EngineIDNumber();
			if (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_EngineIDNumberInfo);
			}
		}

		protected override void CheckCA_MakeOfMachine()
		{
			base.CheckCA_MakeOfMachine();
			var processCode = Parent.CA_ProcessCode;
			var engineClass = Parent.CA_EngineClass;
			if (processCode == ProcessCodes.Codes.XE02 && (engineClass == ECCCProductCategories.Codes.EC16 || engineClass == ECCCProductCategories.Codes.EC20 || engineClass == ECCCProductCategories.Codes.EC0B || engineClass == ECCCProductCategories.Codes.EC0D)
				|| (processCode == ProcessCodes.Codes.XE03 && engineClass == ECCCProductCategories.Codes.EC23))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_MakeOfMachineInfo);
			}
		}

		protected override void CheckCA_ModelOfEngine()
		{
			base.CheckCA_ModelOfEngine();
			var processCode = Parent.CA_ProcessCode;
			if (processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ModelOfEngineInfo);
			}
			else if (processCode == ProcessCodes.Codes.XE04)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.CA_ModelOfEngineInfo);
			}
		}

		protected override void CheckCA_ModelOfMachine()
		{
			base.CheckCA_ModelOfMachine();
			var processCode = Parent.CA_ProcessCode;
			var engineClass = Parent.CA_EngineClass;
			if (processCode == ProcessCodes.Codes.XE02 && (engineClass == ECCCProductCategories.Codes.EC16 || engineClass == ECCCProductCategories.Codes.EC20 || engineClass == ECCCProductCategories.Codes.EC0B || engineClass == ECCCProductCategories.Codes.EC0D)
				|| (processCode == ProcessCodes.Codes.XE03 && engineClass == ECCCProductCategories.Codes.EC23))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ModelOfMachineInfo);
			}
		}

		protected override void CheckCA_EngineModelYear()
		{
			base.CheckCA_EngineModelYear();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_EngineModelYearInfo, Parent.Lookups.EngineModelYearList);
			var processCode = Parent.CA_ProcessCode;
			if (processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_EngineModelYearInfo);
			}
			else if (processCode == ProcessCodes.Codes.XE04)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.CA_EngineModelYearInfo);
			}
		}

		protected override void CheckCA_MachineModelYear()
		{
			base.CheckCA_MachineModelYear();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_MachineModelYearInfo, Parent.Lookups.MachineModelYearList);
		}

		protected override void CheckCA_Incomplete()
		{
			base.CheckCA_Incomplete();
			if (!Parent.CA_Incomplete && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				var isVehicleIncompleted = Parent.CA_VehicleClass == ECCCProductCategories.Codes.EC13 || Parent.CA_VehicleClass == ECCCProductCategories.Codes.EC32;
				var isEnginIncompleted = Parent.CA_EngineClass == ECCCProductCategories.Codes.EC21 || Parent.CA_EngineClass == ECCCProductCategories.Codes.EC33 || Parent.CA_EngineClass == ECCCProductCategories.Codes.EC34;
				var processCode = Parent.CA_ProcessCode;
				if ((processCode == ProcessCodes.Codes.XE01 || processCode == ProcessCodes.Codes.XE04) && (isVehicleIncompleted || isEnginIncompleted)
					|| (processCode == ProcessCodes.Codes.XE02 || processCode == ProcessCodes.Codes.XE03) && isEnginIncompleted)
				{
					Parent.CA_IncompleteInfo.AddMessageError(Res.GetString("20cdb60c-5e2a-4280-9ad8-a2f8eb0e6146", "Incomplete Vehicles or Engines should be provided."));
				}
			}
		}

		protected override void CheckCA_PowerRatingUQ()
		{
			base.CheckCA_PowerRatingUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PowerRatingUQInfo, Parent.Lookups.PowerRatingUQList);

			if (Parent.CA_EnginePowerRating > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_PowerRatingUQInfo);
			}
		}

		protected override void CheckCA_BulkReporting()
		{
			base.CheckCA_BulkReporting();

			if (Parent.CA_BulkReporting && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				OneOfBulkReportingNationalMarkNonCommercialValidation(Parent.CA_BulkReportingInfo);
			}
		}

		protected override void CheckCA_EngineFamilyName()
		{
			base.CheckCA_EngineFamilyName();
			if (((Parent.CA_ProcessCode == ProcessCodes.Codes.XE02) || Parent.CA_ProcessCode == ProcessCodes.Codes.XE03) && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_EngineFamilyNameInfo);
			}
			if (!Parent.CA_EngineFamilyName.IsEmpty && Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && !Regex.IsMatch(Parent.CA_EngineFamilyName, @"^[A-z0-9]{12}$"))
			{
				Parent.CA_EngineFamilyNameInfo.AddMessageError(Res.GetString("1719F120-EBD0-412D-8990-78F6243292CF", "Engine Family Name should be 12 alpha numeric characters."));
			}
		}

		protected override void CheckCA_NationalMark()
		{
			base.CheckCA_NationalMark();
			if (ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				if (Parent.CA_NationalMark)
				{
					OneOfBulkReportingNationalMarkNonCommercialValidation(Parent.CA_NationalMarkInfo);
				}
			}
		}

		protected override void CheckCA_NonCommercialImport()
		{
			base.CheckCA_NonCommercialImport();
			if (ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				if (Parent.CA_NonCommercialImport)
				{
					OneOfBulkReportingNationalMarkNonCommercialValidation(Parent.CA_NonCommercialImportInfo);
				}
			}
		}

		void OneOfBulkReportingNationalMarkNonCommercialValidation(ZPropertyInfo info)
		{
			if (ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				var isBulkReportingIsChecked = Parent.CA_BulkReporting ? 1 : 0;
				var isNationalMarkIsChecked = Parent.CA_NationalMark ? 1 : 0;
				var isNonCommercialIsChecked = Parent.CA_NonCommercialImport ? 1 : 0;
				if ((isBulkReportingIsChecked + isNationalMarkIsChecked + isNonCommercialIsChecked) > 1)
				{
					info.AddMessageError(Res.GetString("{2B2D7EFF-371C-40B2-B33D-9340EA611A65}", "Only ONE of National Emissions Mark, Bulk Reporting Approval or Non-Commercial Import can be ticked"));
				}
			}
		}

		protected override void CheckCA_AOSConformity()
		{
			base.CheckCA_AOSConformity();

			var parent = Parent;
			var ecccHeader = ECCCHeader;
			if (parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ecccHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CA_AOSConformityInfo, parent.Lookups.AOSConformityCodeList);

				if (parent.CA_AOSConformity == AffirmationOfStatementCodes.Codes.ME04)
				{
					if (parent.Parent is ECCCPGAHeader eccc && eccc.InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration)
					{
						if (declaration.ImporterOfRecord is OrgHeader importerOfRecord)
						{
							var eccNumber = importerOfRecord.CustomsCodes.GetCustomsRegNo(MasterFiles.Business.OrgCusCode.CACodeTypes.ECCCAuthorizationNumber, Core.Constants.CountryCodes.Canada);
							if (eccNumber.IsEmpty)
							{
								parent.CA_AOSConformityInfo.AddMessageError(Res.GetString("06AC97A8-486B-4C71-ADE1-F9F410889AA8", "Declaration's Importer of Record should contain a ECC registration number."));
							}
						}
						else if (declaration.Importer is OrgHeader importer)
						{
							var eccNumber = importer.CustomsCodes.GetCustomsRegNo(MasterFiles.Business.OrgCusCode.CACodeTypes.ECCCAuthorizationNumber, Core.Constants.CountryCodes.Canada);
							if (eccNumber.IsEmpty)
							{
								parent.CA_AOSConformityInfo.AddMessageError(Res.GetString("4E05443F-2009-4235-8EC7-871CBC7211B6", "Declaration's Importer should contain a ECC registration number."));
							}
						}
					}
				}
				else if (parent.CA_AOSConformity == AffirmationOfStatementCodes.Codes.ME03)
				{
					if (ecccHeader.LPCOViews is LPCOViewCollection lpcoViewColl && !(lpcoViewColl.OfType<LPCOView>().Any(x => x.CLP_Type == LPCODocumentTypeQualifier.Codes._8030) && lpcoViewColl.OfType<LPCOView>().Any(x => x.CLP_Type == LPCODocumentTypeQualifier.Codes._8031)))
					{
						parent.CA_AOSConformityInfo.AddMessageError(Res.GetString("4A80D53E-E1BC-4A7D-A230-37CE924F8D51", "LPCO Document Types 8030 and 8031 are required when Affirmation of Statement Conformity = ME03"));
					}
				}
			}
		}

		protected override void CheckCA_AOSReplacement()
		{
			base.CheckCA_AOSReplacement();

			if (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_AOSReplacementInfo, Parent.Lookups.AOSReplacementCodeList);

				if (Parent.CA_ReplacementEngines)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_AOSReplacementInfo);
				}

				if (Parent.CA_AOSConformity != AffirmationOfStatementCodes.Codes.ME01 &&
					(Parent.CA_AOSReplacement == AffirmationOfStatementCodes.Codes.ME05 || Parent.CA_AOSReplacement == AffirmationOfStatementCodes.Codes.ME06))
				{
					Parent.CA_AOSReplacementInfo.AddMessageError(Res.GetString("37802640-1FB6-48D8-8F36-304BD878E56C", "Affirmation of Statement Replacement code ME05 or ME06 can only be provided if Affirmation of Statement Conformity selection includes ME01"));
				}
			}
		}

		protected override void CheckCA_AOSEvidence()
		{
			base.CheckCA_AOSEvidence();
			if (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_AOSEvidenceInfo, Parent.Lookups.AOSEvidenceCodeList);

				if (Parent.CA_AOSConformity == AffirmationOfStatementCodes.Codes.ME02 && !(Parent.CA_ReplacementEngines && Parent.CA_AOSReplacement == AffirmationOfStatementCodes.Codes.ME06))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_AOSEvidenceInfo);
				}
			}
		}

		protected override void CheckCA_AOSRetention()
		{
			base.CheckCA_AOSRetention();
			if (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_AOSRetentionInfo, Parent.Lookups.AOSRetentionCodeList);

				if (Parent.CA_AOSConformity == AffirmationOfStatementCodes.Codes.ME02 && !(Parent.CA_ReplacementEngines && Parent.CA_AOSReplacement == AffirmationOfStatementCodes.Codes.ME06))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_AOSRetentionInfo);
				}
			}
		}

		protected override void CheckCA_AlternativeStandardOfEngineClass()
		{
			base.CheckCA_AlternativeStandardOfEngineClass();
			if (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_AlternativeStandardOfEngineClassInfo, Parent.Lookups.AlternativeStandardOfEngineClassCodeList);

				var engineClass = Parent.CA_EngineClass;
				if (engineClass == ECCCProductCategories.Codes.EC15 || engineClass == ECCCProductCategories.Codes.EC16
					|| engineClass == ECCCProductCategories.Codes.EC0A || engineClass == ECCCProductCategories.Codes.EC0B)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_AlternativeStandardOfEngineClassInfo);
				}
			}
		}

		protected override void CheckCA_EvaporativeFamily()
		{
			base.CheckCA_EvaporativeFamily();
			if (Parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ECCCHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				var engineClass = Parent.CA_EngineClass;
				if (engineClass == ECCCProductCategories.Codes.EC0C || engineClass == ECCCProductCategories.Codes.EC0D)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_EvaporativeFamilyInfo);
				}
			}
		}

		protected override void CheckCA_OA_EngineLocation()
		{
			base.CheckCA_OA_EngineLocation();

			var parent = Parent;
			var ecccHeader = ECCCHeader;
			if (parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ecccHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				if (parent.CA_AOSConformity == AffirmationOfStatementCodes.Codes.ME04 || parent.CA_AOSReplacement == AffirmationOfStatementCodes.Codes.ME06)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CA_OA_EngineLocationInfo);

					if (ecccHeader.CA_OA_EngineLocation.IsValid &&
						ecccHeader.InvoiceLine is JobComInvoiceLine invoiceLine &&
						invoiceLine.Declaration is JobDeclaration declaration &&
						declaration.IsIID &&
						ecccHeader.EngineLocation is OrgAddress orgAddress)
					{
						CAAddressValidator.ValidateMandatory(orgAddress, parent.CA_OA_EngineLocationInfo, Res.GetString("6370B848-741C-4F8C-AFD8-B89B576C830A", "Engine Location"));
					}
				}
			}
		}

		protected override void CheckCA_OA_EvidenceOfConformityLocation()
		{
			base.CheckCA_OA_EvidenceOfConformityLocation();

			var parent = Parent;
			var ecccHeader = ECCCHeader;
			if (parent.CA_ProcessCode == ProcessCodes.Codes.XE02 && ecccHeader.CA_VEEProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CA_OA_EvidenceOfConformityLocationInfo);

				if (ecccHeader.CA_OA_EvidenceOfConformityLocation.IsValid &&
					ecccHeader.InvoiceLine is JobComInvoiceLine invoiceLine &&
					invoiceLine.Declaration is JobDeclaration declaration &&
					declaration.IsIID &&
					ecccHeader.EvidenceOfConformityLocation is OrgAddress orgAddress)
				{
					CAAddressValidator.ValidateMandatory(orgAddress, parent.CA_OA_EvidenceOfConformityLocationInfo, Res.GetString("E9E0C8E1-9DF6-4315-AA2C-1323C82E189C", "Evidence of Conformity Location"));
				}
			}
		}
	}
}
