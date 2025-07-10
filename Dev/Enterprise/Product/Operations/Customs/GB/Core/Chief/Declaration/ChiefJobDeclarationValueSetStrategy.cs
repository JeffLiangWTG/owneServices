using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	public class ChiefJobDeclarationValueSetStrategy : Eu.JobDeclarationValueSetStrategy
	{
		public ChiefJobDeclarationValueSetStrategy(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		protected readonly JobDeclaration declaration;

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_MessageType:
					MessageTypeChanged();
					break;
				case JobDeclaration.Schema.JE_VoyageFlightNo:
					FlightNumberChanged();
					break;
				case JobDeclaration.Schema.JE_RL_NKPortOfLoading:
					PortOfLoadingChanged();
					break;
				case JobDeclaration.Schema.JE_RL_NKPortOfArrival:
					PortOfArrivalChanged();
					break;
				case JobDeclaration.Schema.JE_OH_Importer:
					DefaultImporterChanged();
					break;
				case JobDeclaration.Schema.JE_OH_Supplier:
					SupplierChanged();
					break;
				case JobDeclaration.Schema.JE_GB:
					BranchChangedHandler();
					break;
				case JobDeclaration.Schema.JE_UsePostponedVatAccounting:
					SetDefaultFiscalReferences(declaration.ZG_UsePostponedVatAccounting);
					break;
				case JobDeclaration.Schema.JE_HouseSplitReference:
					HouseSplitReferenceChanged();
					break;
			}
			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_RN_NKTransportNationality:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JE_FLG);
					break;
				case JobDeclaration.Schema.JE_RL_NKOrigin:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JE_DSP);
					break;
				case JobDeclaration.Schema.JE_RL_NKFinalDestination:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JE_DST);
					break;
			}

			if (valueThatHasChanged.Name == JobDeclarationSchema.JE_DateOfArrival.Name && declaration.IsImport && valueThatHasChanged.Value != valueThatHasChanged.OriginalValue)
			{
				SetImportEntrySubStyle();
			}
			ValueSetCoreForCHIEF(valueThatHasChanged, oldValue);
		}

		void HouseSplitReferenceChanged()
		{
			CalculateMasterUcr();
		}

		void CalculateMasterUcr()
		{
			declaration.CalculateMasterUCR();
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

		protected override ZString GetNewCtStatusId() => CTStatusIdHelper.GetNewCtStatusId(declaration);

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
				}
			}
		}

		#endregion

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValueSetCoreForCHIEF(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_MasterBill:
				case JobDeclaration.Schema.JE_HouseBill:
					CalculateMasterUCR();
					break;

				case JobDeclaration.Schema.JE_OH_Importer:
				case JobDeclaration.Schema.JE_OH_Supplier:
				case JobDeclaration.Schema.JE_OH_ShippingLine:
				case JobDocAddress.Schema.E2_OA_Address:  // for warehouse.  For declarant see AddInfoJobDeclarationValueSetStrategy
					ValueSetStrategyHelperForAuthorisedEconomicOperator.PerformAeoSupportingDocumentDefaulting(valueThatHasChanged, declaration);
					break;
			}

			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_LocationOfGoods:
				case JobDeclaration.Schema.JE_SubLocationOfGoods:
				case JobDeclaration.Schema.JE_OH_ShippingLine:
					LocationOfGoodsChanged();
					CalculateMasterUCR();
					break;
				case JobDeclaration.Schema.JE_OA_DeclarantAddress:
					ValueSetStrategyHelperForAuthorisedEconomicOperator.PerformAeoSupportingDocumentDefaulting(valueThatHasChanged, declaration);
					declaration.UpdateDucrIfNotLocked();
					break;
				case JobDeclaration.Schema.JE_IsGvmsPort:
					HandleGvmsPortChanged();
					break;
				case JobDeclaration.Schema.JE_MasterUCR:
					HandleMasterUCRChanged(valueThatHasChanged.Value, oldValue);
					break;
				case JobDeclaration.Schema.JE_MessageSubType:  // entry substyle
				case JobDeclaration.Schema.JE_RL_NKPortOfArrival:
				case JobDeclaration.Schema.JE_RL_NKPortOfLoading:
				case JobDeclaration.Schema.JE_TransportMode:
					new Box30LocationOfGoodsValueSetter(declaration).SetBox30();
					break;
			}
		}

		void CalculateMasterUCR()
		{
			declaration.CalculateMasterUCR();
		}

		protected void MessageTypeChanged()
		{
			DefaultDeclarationTypeAndMessageStylesFromMessageType();
			DefaultBadgeCodeFromBarrierPort();
		}

		protected void PortOfArrivalChanged()
		{
			DefaultBadgeCodeFromBarrierPort();
		}

		protected void PortOfLoadingChanged()
		{
			DefaultBadgeCodeFromBarrierPort();
		}

		protected override void BranchChangedHandler()
		{
			base.BranchChangedHandler();
			DefaultBadgeCodeFromBarrierPort();
		}

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

		#region IValueSetStrategy Members

		void ValidateRelatedFECChallenge(ZString fecFieldCode)
		{
			foreach (CusEntryHeader header in declaration.CustomsEntryHeaders)
			{
				header.FECChallenges.Find(x => x.CY_Code == fecFieldCode).ForEach(x => x.Validation.ValidateCY_IsOverridden());
			}
		}

		protected void SupplierChanged()
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
				}
			}
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

		#endregion

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
			declaration.JE_IsGvmsPort = IsGvmsPort(declaration.Factory, declaration.JE_LocationOfGoods, Constants.CountryCodes.UnitedKingdom, GBCommonConstants.RefCusCodeListAttributeCodes.GvmsPortId);
		}

		public static bool IsGvmsPort(BusinessObjectFactory factory, String port, ZString countryOrDataGrouping, ZString refCusCodeListAttributeCode)
		{
			var query = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, countryOrDataGrouping);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, port);
			var portCodeList = factory.Load<ZZRefCusCodeListCombined>(query)?.FirstOrDefault();
			return portCodeList?.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(refCusCodeListAttributeCode)) ?? ZBool.False;
		}

		protected void HandleGvmsPortChanged()
		{
			if (ShouldEnableRRS01Automation)
			{
				if (declaration.ShouldDefaultRRS01)
				{
					SetAdditionalInfoRRS01(declaration);
				}
				else
				{
					RemoveRRS01Statement(declaration);
				}
			}
		}

		void HandleMasterUCRChanged(IZType valueThatHasChanged, IZType oldValue)
		{
			if (ShouldEnableRRS01Automation && oldValue.IsEmpty && !valueThatHasChanged.IsEmpty)
			{
				RemoveRRS01Statement(declaration);
			}
		}

		bool ShouldEnableRRS01Automation => GBCustomsDataRegistry.Instance.ChiefEnableRRS01Automation.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);

		public static void SetAdditionalInfoRRS01(JobDeclaration declaration, bool shippingLineRequired = false)
		{
			var shippingLineEori = declaration.ShippingLine?.GetEuIdentificationNumber() ?? ZString.Empty;
			var shippingLineName = declaration.ShippingLine?.OH_FullName ?? ZString.Empty;
			var isGvmsPortAndShippingLineSet = declaration.JE_IsGvmsPort && declaration.JE_OH_ShippingLine != ZGuid.Empty;

			if (shippingLineRequired && isGvmsPortAndShippingLineSet)
			{
				var addInfo = declaration.AdditionalInfos.AddNew();
				addInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.RRS01;
				addInfo.CSI_Description = shippingLineEori != ZString.Empty ? shippingLineEori : shippingLineName;
			}
			else if (!shippingLineRequired && declaration.JE_IsGvmsPort)
			{
				var addInfo = declaration.AdditionalInfos.AddNew();
				addInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.RRS01;
			}
			else
			{
				RemoveRRS01Statement(declaration);
			}
		}

		static void RemoveRRS01Statement(JobDeclaration declaration)
		{
			var addInfosToRemove = declaration.AdditionalInfos.Cast<Business.Declaration.MultiLineAddInfos.AdditionalInfo>().Where(y => y.CSI_Code == GBCommonConstants.AdditonalInfoCodes.RRS01).ToList();
			foreach (var ai in addInfosToRemove)
			{
				declaration.AdditionalInfos.RemoveAndDelete(ai);
			}
		}

		protected void DefaultDeclarationTypeAndMessageStylesFromMessageType()
		{
			DefaultValueForEntryStyle();
			DefaultValueForCEIStyle();
		}

		protected void DefaultValueForEntryStyle()
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

		protected virtual void DefaultValueForCEIStyle()
		{
			string result = string.Empty;
			switch (declaration.JE_MessageType)
			{
				case MessageTypeList.Codes.Import:
					result = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
					break;
				case MessageTypeList.Codes.Export:
					result = ExportSADDeclarationTypeList.Codes.ExportFullDeclaration;
					break;
			}
			declaration.JE_DeclarationType = result;

			DefaultEntrySubStyle(declaration.CusEntryInstruction);
		}

		protected override void DefaultEntrySubStyleCore(Eu.CusEntryInstruction entryInstruction)
		{
			var allowedSubstylesList = entryInstruction.Lookups.EntrySubStyleList;
			ZString defaultSubstyleForThisDepartment = GBCustomsDataRegistry.Instance.ChiefBox1bEntrySubstyleDefault.GetFallBackValueAtAllLevels(Guid.Empty, declaration.RegistryBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid());
			entryInstruction.CEI_SubStyle = (!defaultSubstyleForThisDepartment.IsEmpty && allowedSubstylesList.ContainsCode(defaultSubstyleForThisDepartment))
							? defaultSubstyleForThisDepartment.ToString()
							: (allowedSubstylesList.Count > 0 ? allowedSubstylesList[0].Code : "");
			if (!allowedSubstylesList.ContainsCode(entryInstruction.CEI_SubStyle))
			{
				entryInstruction.CEI_SubStyle = "";
			}
		}

		protected override void SetImportEntrySubStyleCore()
		{
			if (declaration != null)
			{
				var pairs = GetImportEntrySubStylePairs(declaration);
				if (pairs != null && pairs.Length > 1 && ShouldUpdateSubstyleFromArrivalDate(declaration) && CanMakeAccurateComparisonOfFutureVsPastDueToResolutionOfDateOfArrival(declaration))
				{
					declaration.JE_EntrySubStyle = declaration.JE_DateOfArrival.IsEmpty || declaration.JE_DateOfArrival > ZDateTime.UtcNow
								? pairs[0]
								: pairs[1];
				}
			}
		}

		static bool CanMakeAccurateComparisonOfFutureVsPastDueToResolutionOfDateOfArrival(Eu.JobDeclaration declaration)
		{
			// Can only update box 1b if we have a shipment (JE_DateOfArrival has a time aspect too) or date part is not today (allowing accurate comparison)
			return declaration.Shipment != null || declaration.JE_DateOfArrival.Date != ZDateTime.UtcNow.Date;
		}

		static bool ShouldUpdateSubstyleFromArrivalDate(Eu.JobDeclaration declaration)
		{
			return !declaration.CustomsEntryHeaders.HasAnEntryWithEntryNumber;
		}

		static ZString[] GetImportEntrySubStylePairs(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				if (declaration.IsICR)
				{
					return new ZString[] { GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.C21GoodsNotArrived, GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.C21GoodsArrived };
				}
				else if (declaration.IsIFD)
				{
					var normalFullDeclaration = new ZString[] { GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived, GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived };
					var sdpSfd = new ZString[] { GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived, GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsArrived };
					var transitSfd = new ZString[] { GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived, GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.TransitSfdGoodsArrived };
					if (normalFullDeclaration.Contains(declaration.JE_EntrySubStyle))
					{
						return normalFullDeclaration;
					}
					else if (sdpSfd.Contains(declaration.JE_EntrySubStyle))
					{
						return sdpSfd;
					}
					else if (transitSfd.Contains(declaration.JE_EntrySubStyle))
					{
						return transitSfd;
					}
				}
				else if (declaration.IsIFW)
				{
					return new ZString[] { "", GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived };
				}
			}
			return null;
		}

		protected override void DefaultFiscalRepresentativeDefaulterCore() => fiscalRepresentativeDefaulter = new Eu.FiscalRepresentativeDefaulter();
	}
}
