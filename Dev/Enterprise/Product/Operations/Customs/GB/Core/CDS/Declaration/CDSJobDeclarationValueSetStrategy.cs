using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSJobDeclarationValueSetStrategy : Eu.JobDeclarationValueSetStrategy
	{
		public CDSJobDeclarationValueSetStrategy(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		protected readonly JobDeclaration declaration;

		internal const string AddInfoCodeForImportPerson = "00500";
		internal const string AddInfoDescriptionForImportPerson = "IMPORTER";

		#region IValueSetStrategy Members

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_ApplicationCode:
				case JobDeclaration.Schema.JE_MessageType:
					MessageTypeChanged();
					break;
				case JobDeclaration.Schema.JE_VoyageFlightNo:
					FlightNumberChanged();
					break;
				case JobDeclaration.Schema.JE_OH_Importer:
					DefaultImporterChanged();
					break;
				case JobDeclaration.Schema.JE_OH_Supplier:
					SupplierChanged();
					break;
				case JobDeclaration.Schema.JE_LocationOtherInformation:
					LocationOfGoodsChanged();
					break;
				case JobDeclaration.Schema.JE_GoodsLocation:
					LocationOfGoodsChanged();
					break;

				case JobDeclaration.Schema.JE_SubLocationOfGoods:
					SubLocationOfGoodsChanged();
					break;
				case JobDeclaration.Schema.JE_ClaimEuSubsidy:
					ClaimEUSubsidyChanged();
					break;
				case JobDeclaration.Schema.JE_NiGoodsAtRiskOfMovingToROI:
					NiGoodsAtRiskOfMovingToROIChanged();
					break;
				case JobDeclaration.Schema.JE_NorthernIrelandMode:
					NorthernIrelandModeChanged();
					break;
				case JobDeclaration.Schema.JE_OH_ShippingLine:
					ShippingLineOrIsGvmsChanges();
					break;
				case JobDeclaration.Schema.JE_IsGvmsPort:
					ShippingLineOrIsGvmsChanges();
					break;
				case JobDeclaration.Schema.JE_UsePostponedVatAccounting:
					SetDefaultFiscalReferences(declaration.JE_UsePostponedVatAccounting);
					break;
			}
			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_ApplicationCode:
					declaration.JE_Calc_LocationOtherInformationCountry = declaration.CountryCode;
					break;
				case JobDeclaration.Schema.JE_RN_NKTransportNationality:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JE_FLG);
					break;
				case JobDeclaration.Schema.JE_RL_NKOrigin:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JE_DSP);
					OriginOrDestinationChanged();
					break;
				case JobDeclaration.Schema.JE_RL_NKFinalDestination:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JE_DST);
					OriginOrDestinationChanged();
					break;
				case JobDeclaration.Schema.JE_DateOfArrival:
					if (declaration.IsImport && valueThatHasChanged.Value != valueThatHasChanged.OriginalValue)
					{
						SetImportEntrySubStyle();
					}
					break;
			}
			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_OA_DeclarantAddress:
					DefaultDeclarantChanged();
					break;
				case JobDeclaration.Schema.JE_GB:
					BranchChangedHandler();
					break;
				case JobDeclaration.Schema.JE_RL_NKPortOfLoading:
					DefaultBadgeCodeFromBarrierPort();
					break;
				case JobDeclaration.Schema.JE_RL_NKPortOfArrival:
					DefaultBadgeCodeFromBarrierPort();
					break;
			}
			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_MasterBill:
				case JobDeclaration.Schema.JE_HouseBill:
				case JobDeclaration.Schema.JE_LocationOfGoods:
				case JobDeclaration.Schema.JE_SubLocationOfGoods:
				case JobDeclaration.Schema.JE_OH_ShippingLine:
				case JobDeclaration.Schema.JE_HouseSplitReference:
					declaration.CalculateMasterUCR();
					break;
			}
		}
		#region override Default Values

		protected override void DefaultCT_Status(ZPropertyInfo valueThatHasChanged)
		{
			if (valueThatHasChanged.Name == JobDeclarationSchema.JE_MessageType.Name)
			{
				if (declaration.IsExport)
				{
					declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.X;
				}
				else if (declaration.IsImport)
				{
					declaration.ZG_CTStatusID = ZString.Empty;
				}
			}
		}

		protected override void DefaultSupplierChanged()
		{
			base.DefaultSupplierChanged();
			var supplier = declaration.Supplier;
			if (supplier != null)
			{
				declaration.UCCHelper.EnsureAdditionalInfoExistsForImportsIfDeclarantSameAsLocalClientBasedOnEori(declaration);
			}
		}

		protected override void DefaultImporterChanged()
		{
			base.DefaultImporterChanged();
			var importer = declaration.Importer;
			if (importer != null)
			{
				var gbAddInfo = GBOrgImpAddInfo.Get(importer);
				gbAddInfo.Deserialise();

				declaration.ZG_VATDeferType = gbAddInfo.ZO_VATDeferType;

				SetBox44DucrOptions(gbAddInfo);
				SetBox7DeclarantsRef(gbAddInfo);

				var euAddInfo = EUOrgImpAddInfo.Get(importer, declaration.CountryCode);
				if (euAddInfo != null)
				{
					euAddInfo.Deserialise();

					declaration.ZG_UsePostponedVatAccounting = euAddInfo.ZO_UseFr3FiscalRepresentation;

					declaration.UCCHelper.EnsureAdditionalInfoExistsForImportsIfDeclarantSameAsLocalClientBasedOnEori(declaration);

					if (importer.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
					{
						var ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == AddInfoCodeForImportPerson).FirstOrDefault() ?? declaration.AdditionalInfos.AddNew();
						ai.CSI_Code = AddInfoCodeForImportPerson;
						ai.CSI_Description = AddInfoDescriptionForImportPerson;
					}
					else
					{
						var ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == AddInfoCodeForImportPerson).FirstOrDefault();
						if (ai != null)
						{
							declaration.AdditionalInfos.RemoveAndDelete(ai);
						}
					}
				}
			}
		}

		protected override void DefaultDeclarantChanged()
		{
			base.DefaultDeclarantChanged();

			var declarant = declaration.Declarant;
			if (null != declarant)
			{
				declaration.UCCHelper.EnsureAdditionalInfoExistsForImportsIfDeclarantSameAsLocalClientBasedOnEori(declaration);
			}
		}

		protected override ZString GetNewCtStatusId() => CTStatusIdHelper.GetNewCtStatusId(declaration);

		#endregion

		void DefaultBadgeCodeFromBarrierPort()
		{
			BadgeCodeSetting badgeCodeSetting = BadgeCodeGetter.InstanceCachedFor(declaration).GetFromPortCode(declaration.BarrierPort, declaration.JE_MessageType);
			if (!declaration.JE_CustomsProfileInfo.ReadOnly) // will be readonly if it's being (or has been) synched from HAWB
			{
				if (badgeCodeSetting != null)
				{
					declaration.JE_CustomsProfile = badgeCodeSetting.BadgeCode;
					declaration.ZG_Gateway = badgeCodeSetting.CSPCode;
				}
				else
				{
					declaration.JE_CustomsProfile = string.Empty;
					declaration.ZG_Gateway = string.Empty;
				}
			}
		}

		void ValidateRelatedFECChallenge(ZString fecFieldCode)
		{
			foreach (CusEntryHeader header in declaration.CustomsEntryHeaders)
			{
				header.FECChallenges.Find(x => x.CY_Code == fecFieldCode).ForEach(x => x.Validation.ValidateCY_IsOverridden());
			}
		}

		protected virtual void SupplierChanged()
		{
			if (declaration.IsExport)
			{
				var supplier = declaration.Supplier;
				if (supplier != null)
				{
					var gbAddInfo = GBOrgImpAddInfo.Get(supplier);
					gbAddInfo.Deserialise();

					SetBox44DucrOptions(gbAddInfo);
					SetBox7DeclarantsRef(gbAddInfo);
					SetBox14Representation(supplier);
					SetOfficeOfExit(supplier);
				}
			}
		}

		void SetOfficeOfExit(OrgHeader supplier)
		{
			if (declaration.JE_CustomsOffice.IsEmpty)
			{
				declaration.JE_CustomsOffice = GetSuppliersRelatedPartyOfTypeCustomsOfficeCode(supplier);
			}
		}

		ZString GetSuppliersRelatedPartyOfTypeCustomsOfficeCode(OrgHeader supplier)
		{
			ZString officeCode = ZString.Empty;

			var suppliersRelatedPartyOfTypeCustomsOffice = supplier?.AllRelatedParties.Cast<OrgRelatedParty>()
				.FirstOrDefault(orp => orp.PR_PartyType == RelatedPartyTypeList.Codes.CustomsOffice);

			if (suppliersRelatedPartyOfTypeCustomsOffice != null)
			{
				var relatedParty = suppliersRelatedPartyOfTypeCustomsOffice.RelatedParty;
				if (relatedParty != null)
				{
					officeCode = relatedParty.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit, Core.Constants.CountryCodes.UnitedKingdom);
				}
			}
			return officeCode;
		}

		void SetBox44DucrOptions(GBOrgImpAddInfo addInfo)
		{
			if (addInfo.ZO_Box44UseClientsEoriForDucrs)
			{
				declaration.UseClientEoriForDucr = true;
			}
			var ducrGenerationAttributeName = addInfo.ZO_Box44ClientsDucrSourceAttributeField;
			ZPropertyInfo sourceInfo = null;
			if (declaration.DocsAndCartage != null)
			{
				if (ducrGenerationAttributeName == GBOrgImpAddInfoLookups.ClientsDucrSourceAttributeFieldList_CA1)
				{
					sourceInfo = declaration.DocsAndCartage.JP_CustomAttrib1Info;
				}
				else if (ducrGenerationAttributeName == GBOrgImpAddInfoLookups.ClientsDucrSourceAttributeFieldList_CA2)
				{
					sourceInfo = declaration.DocsAndCartage.JP_CustomAttrib2Info;
				}
			}
			if (sourceInfo != null && !sourceInfo.Value.IsEmpty && declaration.ClientReferenceForDucr.IsEmpty)
			{
				declaration.ClientReferenceForDucr = (ZString)sourceInfo.Value;
			}
		}

		void SetBox7DeclarantsRef(GBOrgImpAddInfo addInfo)
		{
			var declarantsRefName = addInfo.ZO_Box7DeclarantsReferenceSourceAttributeField;
			ZPropertyInfo sourceInfo = null;
			if (declaration.DocsAndCartage != null)
			{
				if (declarantsRefName == GBOrgImpAddInfoLookups.ClientsDucrSourceAttributeFieldList_CA1)
				{
					sourceInfo = declaration.DocsAndCartage.JP_CustomAttrib1Info;
				}
				else if (declarantsRefName == GBOrgImpAddInfoLookups.ClientsDucrSourceAttributeFieldList_CA2)
				{
					sourceInfo = declaration.DocsAndCartage.JP_CustomAttrib2Info;
				}
				if (sourceInfo != null && !sourceInfo.Value.IsEmpty && declaration.JE_OwnerRef.IsEmpty)
				{
					declaration.JE_OwnerRef = (ZString)sourceInfo.Value;
				}
			}
		}

		protected void MessageTypeChanged()
		{
			DefaultValueForEntryStyle();
			DefaultValueForCEIStyle();
		}
		#endregion

		public void DefaultValueForEntryStyle()
		{
			if (!declaration.Lookups.EntryStyleList.ContainsCode(declaration.JE_EntryStyle))
			{
				switch (declaration.JE_MessageType)
				{
					case MessageTypeList.Codes.Import:
						declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
						break;
					case MessageTypeList.Codes.Export:
						declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
						break;
					default:
						declaration.JE_EntryStyle = string.Empty;
						break;
				}
			}
		}

		protected void FlightNumberChanged()
		{
			if (declaration.IsAir)
			{
				var airline = declaration.Airline;
				if (airline != null)
				{
					var org = airline.GetCorrespondingCarrierOrganisation();
					if (org != null)
					{
						declaration.JE_RN_NKTransportNationality = org.OH_RL_NKClosestPort.Left(2);
					}
					else
					{
						var nameOfCountry = airline.RM_AirlineCountry;
						if (nameOfCountry == "USA") // Hack - the RefAirline data talks of 'USA' but RefCountry talks of 'United States'
						{
							declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.UnitedStates;
						}
						else
						{
							var refCountry = declaration.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Desc, nameOfCountry));
							if (refCountry != null)
							{
								declaration.JE_RN_NKTransportNationality = refCountry.Code;
							}
						}
					}
				}
			}
		}

		protected void LocationOfGoodsChanged()
		{
			declaration.JE_IsGvmsPort = ChiefJobDeclarationValueSetStrategy.IsGvmsPort(declaration.Factory, declaration.JE_GoodsLocation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Constants.RefCusCodeListAttributeCodes.GoodsVehicleMovementSystem);
			declaration.JE_SubLocationOfGoods = declaration.JE_GoodsLocation.SubstringSafe(3, 6);
		}

		protected void SubLocationOfGoodsChanged()
		{
		}

		void OriginOrDestinationChanged()
		{
			if (declaration.IsDestinationNorthernIreland)
			{
				if (!declaration.IsOriginNorthernIreland && declaration.JE_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
				{
					declaration.JE_NorthernIrelandMode = Business.CodeDescriptionPairLists.NIModeList.Codes.MovementFromGreatBritainToNi;
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
				}
				else if (!declaration.JE_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
				{
					declaration.JE_NorthernIrelandMode = Business.CodeDescriptionPairLists.NIModeList.Codes.ImportIntoNiFromRestOfWorld;
				}
			}

			if (declaration.IsOriginNorthernIreland)
			{
				if (!declaration.IsDestinationNorthernIreland && declaration.JE_RL_NKFinalDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
				{
					declaration.JE_NorthernIrelandMode = Business.CodeDescriptionPairLists.NIModeList.Codes.MovementFromNiToGreatBritain;
				}
				else if (!declaration.JE_RL_NKFinalDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
				{
					declaration.JE_NorthernIrelandMode = Business.CodeDescriptionPairLists.NIModeList.Codes.ExportFromNiToRestOfWorld;
				}
			}
		}

		public void ClaimEUSubsidyChanged()
		{
			var isSubsidy = declaration.JE_ClaimEuSubsidy;
			var isIntoNi = declaration.JE_NorthernIrelandMode == Constants.NorthernIrelandModeCodes.NII || declaration.JE_NorthernIrelandMode == Constants.NorthernIrelandModeCodes.G2N;
			foreach (var invLine in declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList())
			{
				if (isSubsidy && isIntoNi)
				{
					invLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIAID);
				}
				else
				{
					invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIAID);
				}
			}
		}

		public void NiGoodsAtRiskOfMovingToROIChanged()
		{
			var isNiGoodsAtRiskOfMovingToROI = declaration.JE_NiGoodsAtRiskOfMovingToROI;
			var isIntoNi = declaration.JE_NorthernIrelandMode == Constants.NorthernIrelandModeCodes.NII || declaration.JE_NorthernIrelandMode == Constants.NorthernIrelandModeCodes.G2N;
			foreach (var invLine in declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList())
			{
				if (!isNiGoodsAtRiskOfMovingToROI && isIntoNi)
				{
					invLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIREM);
				}
				else
				{
					invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIREM);
				}
			}
		}

		public void NorthernIrelandModeChanged()
		{
			var northernIrelandMode = declaration.JE_NorthernIrelandMode;
			var goodsAtRisk = declaration.JE_NiGoodsAtRiskOfMovingToROI;
			foreach (var invLine in declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList())
			{
				switch (northernIrelandMode)
				{
					case Constants.NorthernIrelandModeCodes.NII:
						AddNiRemForGoodsNotAtRisk(goodsAtRisk, invLine);
						invLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIIMP);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIDOM);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIEXP);
						break;
					case Constants.NorthernIrelandModeCodes.NIE:
						invLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIEXP);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIDOM);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIIMP);
						break;
					case Constants.NorthernIrelandModeCodes.G2N:
						AddNiRemForGoodsNotAtRisk(goodsAtRisk, invLine);
						invLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIDOM);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIEXP);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIIMP);
						break;
					case Constants.NorthernIrelandModeCodes.N2G:
						// Do nothing for now
						break;
					case "":
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIDOM);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIEXP);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIIMP);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIREM);
						invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIAID);
						break;
				}
			}
		}

		void AddNiRemForGoodsNotAtRisk(ZBool goodsAtRisk, JobComInvoiceLine invLine)
		{
			if (goodsAtRisk)
			{
				invLine.RemoveAdditionalInfoByCode(GBCommonConstants.AdditonalInfoCodes.NIREM);
			}
			else
			{
				invLine.AddAdditionalInfoByCodeIfNotExists(GBCommonConstants.AdditonalInfoCodes.NIREM);
			}
		}

		public void ShippingLineOrIsGvmsChanges()
		{
			ChiefJobDeclarationValueSetStrategy.SetAdditionalInfoRRS01(declaration, true);
		}

		protected void DefaultValueForCEIStyle()
		{
			string result = string.Empty;

			switch (declaration.JE_MessageType)
			{
				case MessageTypeList.Codes.Import:
					result = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
					break;
				case MessageTypeList.Codes.Export:
					result = ExportDeclarationTypeList.Codes.DeclarationForExport;
					break;
			}
			if (declaration.CustomsEntryInstructions.Count <= 1)
			{
				declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().ForEach(x => x.CEI_Style = result);
			}
			DefaultValueForCEISubStyle();
		}

		protected void DefaultValueForCEISubStyle()
		{
			if (declaration != null)
			{
				if (declaration.CustomsEntryInstructions.Count <= 1)
				{
					DefaultEntrySubStyleCore(declaration.CustomsEntryInstructions.Any() ? declaration.CustomsEntryInstructions[0] : null);
				}
			}
		}

		protected override void DefaultEntrySubStyleCore(Eu.CusEntryInstruction ceiParam)
		{
			var cei = ceiParam as CusEntryInstruction;
			if (cei != null)
			{
				var allowedSubstylesList = cei.Lookups.EntrySubStyleList.ToArray().Select(x => x.Code);
				var countOfAllowedSubstyles = allowedSubstylesList.Count();
				var result = allowedSubstylesList != null && countOfAllowedSubstyles > 0 ? allowedSubstylesList.FirstOrDefault() : string.Empty;

				if (countOfAllowedSubstyles > 1)
				{
					var dec = cei.JobDeclaration;
					if (dec != null)
					{
						var arrived = dec.JE_DateOfArrival.IsInThePast();

						result = arrived ? allowedSubstylesList.FirstOrDefault(x => ArrivedCodes.Contains(x)) : allowedSubstylesList.FirstOrDefault(x => !ArrivedCodes.Contains(x));
					}
				}
				cei.CEI_SubStyle = result;
			}
		}

		protected override void DefaultEntryStyleCore(Eu.CusEntryInstruction cei)
		{
			if (cei != null)
			{
				cei.CEI_Style = cei.CEI_SubStyle == EntrySubStyleCodeList.Codes.Q ? ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration : cei.CEI_Style.ToString();
			}
		}

		IEnumerable<string> arrivedCodes;
		public IEnumerable<string> ArrivedCodes
		{
			get
			{
				if (arrivedCodes == null)
				{
					arrivedCodes = new string[]
					{
						EntrySubStyleCodeList.Codes.A,
						EntrySubStyleCodeList.Codes.B,
						EntrySubStyleCodeList.Codes.C,
						EntrySubStyleCodeList.Codes.J,
						EntrySubStyleCodeList.Codes.X,
						EntrySubStyleCodeList.Codes.Y,
						EntrySubStyleCodeList.Codes.Z
					};
				}
				return arrivedCodes;
			}
		}

		public static IEnumerable<ZString> ArrivedFroniterSubstyleCodes
		{
			get
			{
				return new ZString[]
				{
					EntrySubStyleCodeList.Codes.A,
					EntrySubStyleCodeList.Codes.B,
					EntrySubStyleCodeList.Codes.C,
					EntrySubStyleCodeList.Codes.J,
				};
			}
		}

		protected override void SetImportEntrySubStyleCore()
		{
			if (ShouldUpdateSubstyleFromArrivalDate(declaration) && CanMakeAccurateComparisonOfFutureVsPastDueToResolutionOfDateOfArrival(declaration))
			{
				foreach (CusEntryInstruction cei in declaration.CustomsEntryInstructions)
				{
					DefaultEntrySubStyleCore(cei);
				}
			}
		}

		static bool CanMakeAccurateComparisonOfFutureVsPastDueToResolutionOfDateOfArrival(Eu.JobDeclaration declaration)
		{
			// Can only update box 1b if we have a shipment (JE_DateOfArrival has a time aspect too) or date part is not today (allowing accurate comparison)
			return declaration.Shipment != null || declaration.JE_DateOfArrival.Date != ZDateTime.UtcNow.Date;
		}

		static bool ShouldUpdateSubstyleFromArrivalDate(JobDeclaration declaration)
		{
			return !declaration.CustomsEntryHeaders.HasAnEntryWithEntryStatus;
		}

		protected override void DefaultFiscalRepresentativeDefaulterCore() => fiscalRepresentativeDefaulter = new CDSFiscalRepresentativeDefaulter();
	}
}
