using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocHeaderValidation : AutoQuarantineExDocHeaderValidation
	{
		public QuarantineExDocHeaderValidation(AutoQuarantineExDocHeader parent)
			: base(parent)
		{
		}

		protected new QuarantineExDocHeader Parent
		{
			get { return (QuarantineExDocHeader)base.Parent; }
		}

		bool IsValidationRequired
		{
			get
			{
				var declaration = Parent?.Declaration;
				return (declaration != null && declaration.IsQuarantine);
			}
		}

		static ImmutableHashSet<string> CDD03RequiredType { get; } = ImmutableHashSet.Create(EXDOCCommodityCodes.Codes.Dairy, EXDOCCommodityCodes.Codes.Fish, EXDOCCommodityCodes.Codes.Eggs);

		protected override void CheckQH_ProduceType()
		{
			base.CheckQH_ProduceType();
			if (IsValidationRequired)
			{
				var propertyInfo = Parent.QH_ProduceTypeInfo;
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(propertyInfo, Parent.Lookups.ProduceType);

				if (CDD03RequiredType.Contains(Parent.QH_ProduceType) && !Parent.SupportingInfos.Cast<QuarantineSupportingInfo>().Any(info => info.CSI_Description == "CDD03"))
				{
					propertyInfo.AddMessageError(Res.GetString("29ee933d-3f2b-4c48-a9c7-77adb3d4b5d9", "Declaration code CDD03 is required when produce type is Dairy, Fish or Eggs."));
				}

				ValidateQH_DecOfCompliance();
				Parent.AddInfo.Validation.ValidateZH_TrueAndCompleteIndicator();
			}
		}

		protected override void CheckQH_QuotaType()
		{
			base.CheckQH_QuotaType();
			if (IsValidationRequired && !Parent.IsNEXDOCSActive && !Parent.QH_QuotaType.IsEmpty && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
			{
				Parent.QH_QuotaTypeInfo.AddMessageError("Quota Type may only be present when the Produce Type is Meat");
			}
		}

		protected override void CheckQH_RL_NKBorderInspectionPort()
		{
			base.CheckQH_RL_NKBorderInspectionPort();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.QH_RL_NKBorderInspectionPortInfo, Parent.Lookups.BorderInspectionPorts);
				if (Parent.QH_RL_NKBorderInspectionPort.StartsWith(Core.Constants.CountryCodes.Australia, StringComparison.Ordinal))
				{
					Parent.QH_RL_NKBorderInspectionPortInfo.AddMessageError("Must not be an Australian Port Code.");
				}

				if (!Parent.QH_RL_NKBorderInspectionPort.IsEmpty)
				{
					EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(Parent.QH_ProduceType, Parent.QH_RL_NKBorderInspectionPortInfo, "Border Inspection Port");
				}
			}
		}

		protected override void CheckQH_AbsoluteTemperature()
		{
			base.CheckQH_AbsoluteTemperature();
			if (IsValidationRequired)
			{
				var parent = Parent;
				if (parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Fish && parent.IsNEXDOCSActive && !parent.QH_AbsoluteTemperature.IsEmpty)
				{
					parent.QH_AbsoluteTemperatureInfo.AddMessageError("Absolute temperature is not allowed for this produce type.");
				}
				else if (!parent.QH_AbsoluteTemperature.IsEmpty || !parent.QH_MinimumTemperature.IsEmpty || !parent.QH_MaximumTemperature.IsEmpty)
				{
					if (commoditiesWhichMayHaveTemperature.Contains(parent.QH_ProduceType))
					{
						if (!parent.QH_AbsoluteTemperature.IsEmpty && (!parent.QH_MinimumTemperature.IsEmpty || !parent.QH_MaximumTemperature.IsEmpty))
						{
							parent.QH_AbsoluteTemperatureInfo.AddMessageError("Absolute temperature must equal zero when temperature range is set.");
						}
					}
					else if (!parent.QH_AbsoluteTemperature.IsEmpty)
					{
						parent.QH_AbsoluteTemperatureInfo.AddMessageError("Absolute temperature may only be present if produce type is dairy, meat, fish, eggs or inedible meat.");
					}
				}

				ValidateQH_MinimumTemperature();
				ValidateQH_MaximumTemperature();
			}
		}
		readonly List<ZString> commoditiesWhichMayHaveTemperature = new List<ZString>()
		{
					EXDOCCommodityCodes.Codes.Fish,
					EXDOCCommodityCodes.Codes.Meat,
					EXDOCCommodityCodes.Codes.Dairy,
					EXDOCCommodityCodes.Codes.Eggs,
					EXDOCCommodityCodes.Codes.InedibleMeat
				};

		protected override void CheckQH_MinimumTemperature()
		{
			base.CheckQH_MinimumTemperature();
			if (IsValidationRequired)
			{
				if (!Parent.QH_MinimumTemperature.IsEmpty || !Parent.QH_MaximumTemperature.IsEmpty || !Parent.QH_AbsoluteTemperature.IsEmpty)
				{
					if (commoditiesWhichMayHaveTemperatureRange.Contains(Parent.QH_ProduceType))
					{
						if (!Parent.QH_MinimumTemperature.IsEmpty && !Parent.QH_AbsoluteTemperature.IsEmpty)
						{
							Parent.QH_MinimumTemperatureInfo.AddMessageError("Range temperature values must be zero when absolute temperature is set.");
						}

						if (Parent.QH_MinimumTemperature > Parent.QH_MaximumTemperature)
						{
							Parent.QH_MinimumTemperatureInfo.AddMessageError("Minimum temperature value must be less then maximum temperature value.");
						}
					}
					else if (!Parent.QH_MinimumTemperature.IsEmpty)
					{
						Parent.QH_MinimumTemperatureInfo.AddMessageError("Minimum temperature may only be present if produce type is fish, eggs or inedible meat.");
					}
				}

				ValidateQH_MaximumTemperature();
				ValidateQH_AbsoluteTemperature();
			}
		}
		readonly List<ZString> commoditiesWhichMayHaveTemperatureRange = new List<ZString>()
		{
					EXDOCCommodityCodes.Codes.Fish,
					EXDOCCommodityCodes.Codes.Eggs,
					EXDOCCommodityCodes.Codes.InedibleMeat
				};

		protected override void CheckQH_MaximumTemperature()
		{
			base.CheckQH_MaximumTemperature();
			if (IsValidationRequired)
			{
				if (!Parent.QH_MaximumTemperature.IsEmpty || !Parent.QH_AbsoluteTemperature.IsEmpty || !Parent.QH_MinimumTemperature.IsEmpty)
				{
					if (commoditiesWhichMayHaveTemperatureRange.Contains(Parent.QH_ProduceType))
					{
						if (!Parent.QH_MaximumTemperature.IsEmpty && !Parent.QH_AbsoluteTemperature.IsEmpty)
						{
							Parent.QH_MaximumTemperatureInfo.AddMessageError("Range temperature values must be zero when absolute temperature is set.");
						}

						if (Parent.QH_MaximumTemperature < Parent.QH_MinimumTemperature)
						{
							Parent.QH_MaximumTemperatureInfo.AddMessageError("Maximum temperature must be greater than minimum temperature.");
						}
					}
					else if (!Parent.QH_MaximumTemperature.IsEmpty)
					{
						Parent.QH_MaximumTemperatureInfo.AddMessageError("Maximum temperature may only be present if produce type is fish, eggs or inedible meat.");
					}
				}

				ValidateQH_MinimumTemperature();
				ValidateQH_AbsoluteTemperature();
			}
		}

		protected override void CheckQH_TemperatureUM()
		{
			base.CheckQH_TemperatureUM();
			if (IsValidationRequired)
			{
				if (commoditiesWhichMayHaveTemperature.Contains(Parent.QH_ProduceType))
				{
					if (!Parent.QH_AbsoluteTemperature.IsEmpty || !Parent.QH_MinimumTemperature.IsEmpty || !Parent.QH_MaximumTemperature.IsEmpty || !Parent.QH_TemperatureUM.IsEmpty)
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.QH_TemperatureUMInfo, Parent.Lookups.TemperatureUnit);
					}
				}
				else if (!Parent.QH_TemperatureUM.IsEmpty)
				{
					Parent.QH_TemperatureUMInfo.AddMessageError("Unit of temperature may only be present if produce type is dairy, meat, fish, eggs or inedible meat.");
				}
			}
		}

		protected override void CheckQH_AuthorisationEstablishment()
		{
			if (AuthorisationLocationIsCodeOrAQISPlace)
			{
				base.CheckQH_AuthorisationEstablishment();
				if (IsValidationRequired)
				{
					if (Parent.QH_AuthorisationEstablishment.IsEmpty && AuthorisationEstablishmentShouldBeEntered)
					{
						Parent.QH_AuthorisationEstablishmentInfo.AddMessageError(AuthorisationEstablishmentShouldBeEnteredMessage);
					}
					else if (Parent.QH_AuthorisationLocation == QuarantineExDocHeaderLookups.AqisPlaceCode)
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.QH_AuthorisationEstablishmentInfo);
					}
				}
			}
		}

		bool AuthorisationLocationIsCodeOrAQISPlace => Parent.QH_AuthorisationLocation == QuarantineExDocHeaderLookups.AqisPlaceCode || Parent.QH_AuthorisationLocation == EXDOCCodeOrganisation.Codes.Code;

		bool AuthorisationEstablishmentShouldBeEntered => !Parent.IsWoolOrSkinsProduceType;

		ZString AuthorisationEstablishmentShouldBeEnteredMessage => Res.GetString("9C7E49AD-B4B0-4E7C-A370-78A64AAC4F6C", "Authorization Establishment must be entered.");

		protected override void CheckQH_AuthorisingOfficerID()
		{
			base.CheckQH_AuthorisingOfficerID();
			if (IsValidationRequired)
			{
				if (!Parent.QH_AuthorisingOfficerID.IsEmpty)
				{
					if (Parent.IsWoolOrSkinsProduceType)
					{
						Parent.QH_AuthorisingOfficerIDInfo.AddMessageError("Authorising officer identifier can only be set if produce type is not wool or skins and hides. ");
					}
				}
			}
		}

		protected override void CheckQH_PackDate()
		{
			base.CheckQH_PackDate();
			if (IsValidationRequired)
			{
				if (!Parent.QH_PackDate.IsEmpty && !Parent.IsWoolOrSkinsProduceType)
				{
					Parent.QH_PackDateInfo.AddMessageError("Packing date may only be present when produce type is wool or skins and hides.");
				}
			}
		}

		protected override void CheckQH_OriginCatchZone()
		{
			base.CheckQH_OriginCatchZone();
			if (IsValidationRequired)
			{
				if (!Parent.QH_OriginCatchZone.IsEmpty && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish)
				{
					Parent.QH_OriginCatchZoneInfo.AddMessageError("Origin catch zone may only be present when produce type is fish.");
				}
			}
		}

		protected override void CheckQH_LotNumber()
		{
			base.CheckQH_LotNumber();
			if (IsValidationRequired)
			{
				if (!Parent.QH_LotNumber.IsEmpty && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Horticulture && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.GrainsAndPlants)
				{
					Parent.QH_LotNumberInfo.AddMessageError("Lot number may only be present when produce type is horticulture or grains and plants.");
				}
			}
		}

		protected override void CheckQH_CertificatePrintIndicator()
		{
			base.CheckQH_CertificatePrintIndicator();
			if (IsValidationRequired)
			{
				if (!Parent.QH_CertificatePrintIndicator.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.QH_CertificatePrintIndicatorInfo, Parent.Lookups.CertificatePrintCode);
					if (commoditiesWhichMayNotHaveCertificatePrintIndicator.Contains(Parent.QH_ProduceType) &&
						Parent.QH_CertificatePrintIndicator == EXDOCCertificatePrintCodes.Codes.CustomCertificate)
					{
						Parent.QH_CertificatePrintIndicatorInfo.AddMessageError("Customs certificate is invalid for produce types meat, inedible meat, eggs, dairy and fish.");
					}
				}
			}
		}
		readonly List<ZString> commoditiesWhichMayNotHaveCertificatePrintIndicator = new List<ZString>()
		{
					EXDOCCommodityCodes.Codes.Meat,
					EXDOCCommodityCodes.Codes.InedibleMeat,
					EXDOCCommodityCodes.Codes.Dairy,
					EXDOCCommodityCodes.Codes.Fish,
					EXDOCCommodityCodes.Codes.Eggs
				};

		protected override void CheckQH_ShipsStores()
		{
			base.CheckQH_ShipsStores();
			if (IsValidationRequired)
			{
				if (Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat && Parent.QH_ShipsStores)
				{
					Parent.QH_ShipsStoresInfo.AddMessageError("Ships stores can only be set when produce type is meat.");
				}
			}
		}

		protected override void CheckQH_AMLCQuota()
		{
			base.CheckQH_AMLCQuota();
			if (IsValidationRequired)
			{
				if (Parent.QH_AMLCQuota && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
				{
					Parent.QH_AMLCQuotaInfo.AddMessageError("AMLC Quota can only be set when produce type is Meat or Dairy.");
				}
				else if (!Parent.QH_AMLCQuota && !Parent.QH_AMLCQuotaYear.IsEmpty)
				{
					Parent.QH_AMLCQuotaInfo.AddMessageError("AMLC Quota indicator needs to be Set when AMCL Quota Year is entered.");
				}

				ValidateQH_AMLCQuotaYear();
			}
		}

		#region QH_AMLCQuotaYear

		protected override void CheckQH_AMLCQuotaYear()
		{
			base.CheckQH_AMLCQuotaYear();

			if (IsValidationRequired)
			{
				if (Parent.QH_AMLCQuota)
				{
					if (Parent.QH_AMLCQuotaYear.IsEmpty)
					{
						Parent.QH_AMLCQuotaYearInfo.AddMessageError("AMCL Quota Year needs to be entered when the AMLC Quota indicator is YES.");
					}
					else
					{
						var quotaYearValue = Parent.QH_AMLCQuotaYear;

						var rx = new System.Text.RegularExpressions.Regex(@"^\d\d\d\d(-\d\d)?$");
						var matches = rx.IsMatch(quotaYearValue);

						int startYear = 0;

						if (matches && int.TryParse(Parent.QH_AMLCQuotaYear.Substring(0, 4), out startYear))
						{
							var hasEndYear = Parent.QH_AMLCQuotaYear.IndexOf('-') > 0;
							var thisYear = ZDateTime.Today.Year;
							var lastYear = thisYear - 1;
							var nextYear = thisYear + 1;

							if (!hasEndYear && (startYear < thisYear || startYear > nextYear))
							{
								Parent.QH_AMLCQuotaYearInfo.AddMessageError("AMCL Quota Year must be this year or next year.");
							}
							else if (hasEndYear)
							{
								int endYear = 0;
								if (int.TryParse(Parent.QH_AMLCQuotaYear.Substring(5, 2), out endYear))
								{
									endYear += 2000;
								}

								if (startYear < lastYear || startYear > nextYear)
								{
									Parent.QH_AMLCQuotaYearInfo.AddMessageError("AMCL Quota Year range must begin last year, this year or next year.");
								}
								else if (startYear + 1 != endYear)
								{
									Parent.QH_AMLCQuotaYearInfo.AddMessageError("AMCL Quota Year range must be consecutive years.");
								}
							}
						}
						else
						{
							Parent.QH_AMLCQuotaYearInfo.AddMessageError("AMLC Quota Year must be in the format CCYY, or CCYY-YY (e.g. 2017-18)");
						}
					}
				}
				else if (!Parent.QH_AMLCQuotaYear.IsEmpty)
				{
					Parent.QH_AMLCQuotaYearInfo.AddMessageError("AMCL Quota Year should not be entered when the AMLC Quota indicator is NO or not entered.");
				}

				ValidateQH_AMLCQuota();
			}
		}

		#endregion

		protected override void CheckQH_InspectionRequestedDate()
		{
			base.CheckQH_InspectionRequestedDate();
			if (IsValidationRequired)
			{
				if (!Parent.QH_InspectionRequestedDate.IsEmpty)
				{
					if (UniversalReferenceHelper.Errata53Enabled() && EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(Parent.QH_ProduceType))
					{
						Parent.QH_InspectionRequestedDateInfo.AddMessageError("Inspection Requested Date must not be present when Produce Type is Horticulture, Grains and Seeds, Skins and Hides or Wool");
					}
					else if (Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Wool || Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants)
					{
						Parent.QH_InspectionRequestedDateInfo.AddMessageError("Inspection requested date can only be set when produce type is not wool or grains and plants.");
					}
					else
					{
						ZDateTime departureDate = !Parent.Declaration.JE_ExportDate.IsEmpty ? Parent.Declaration.JE_ExportDate : Parent.Declaration.JE_DateAtOrigin;
						if (Parent.QH_InspectionRequestedDate > departureDate)
						{
							Parent.QH_InspectionRequestedDateInfo.AddMessageError("Inspection requested date must be less than or equal to departure date.");
						}
					}
				}
			}
		}

		protected override void CheckQH_AuthorisedStartDate()
		{
			base.CheckQH_AuthorisedStartDate();
			if (IsValidationRequired)
			{
				if (!Parent.QH_AuthorisedStartDate.IsEmpty)
				{
					if (Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants || Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Horticulture)
					{
						ZDateTime departureDate = !Parent.Declaration.JE_ExportDate.IsEmpty ? Parent.Declaration.JE_ExportDate : Parent.Declaration.JE_DateAtOrigin;
						if (Parent.QH_AuthorisedStartDate > departureDate)
						{
							Parent.QH_AuthorisedStartDateInfo.AddMessageError("Authorised start date must be less than or equal to departure date.");
						}

						if (Parent.QH_AuthorisedStartDate > Parent.QH_AuthorisedEndDate)
						{
							Parent.QH_AuthorisedStartDateInfo.AddMessageError("Authorised start date must be less than or equal to authorised end date.");
						}
					}
					else
					{
						Parent.QH_AuthorisedStartDateInfo.AddMessageError("Authorised start date can only be set when produce type is horticulture or grains and plants.");
					}
				}

				if (Parent.QH_AuthorisedStartDate.IsEmpty && !Parent.QH_AuthorisedEndDate.IsEmpty)
				{
					Parent.QH_AuthorisedStartDateInfo.AddMessageError("Authorised start date must be present when authorised end date is entered.");
				}

				ValidateQH_AuthorisedEndDate();
			}
		}

		protected override void CheckQH_AuthorisedEndDate()
		{
			base.CheckQH_AuthorisedEndDate();
			if (IsValidationRequired)
			{
				if (!Parent.QH_AuthorisedEndDate.IsEmpty)
				{
					if (Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
					{
						ZDateTime departureDate = !Parent.Declaration.JE_ExportDate.IsEmpty ? Parent.Declaration.JE_ExportDate : Parent.Declaration.JE_DateAtOrigin;
						if (Parent.QH_AuthorisedEndDate > departureDate)
						{
							Parent.QH_AuthorisedEndDateInfo.AddMessageError("Authorised end date must be less than or equal to departure date.");
						}

						if (Parent.QH_AuthorisedEndDate < Parent.QH_AuthorisedStartDate)
						{
							Parent.QH_AuthorisedEndDateInfo.AddMessageError("Authorised end date must be greater than or equal to authorised start date.");
						}
					}
					else
					{
						Parent.QH_AuthorisedEndDateInfo.AddMessageError("Authorised end date is not allowed when produce type is dairy.");
					}
				}

				ValidateQH_AuthorisedStartDate();
			}
		}

		protected override void CheckQH_CertificateRequiredLocation()
		{
			base.CheckQH_CertificateRequiredLocation();
			if (IsValidationRequired)
			{
				if (Parent.QH_PrintLocation == QuarantineExDocHeaderLookups.AqisPlaceCode)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.QH_CertificateRequiredLocationInfo);
				}

				if (Parent.QH_CertificatePrintIndicator != EXDOCCertificatePrintCodes.Codes.NotRequired && Parent.QH_CertificateRequiredLocation.IsEmpty)
				{
					Parent.QH_CertificateRequiredLocationInfo.AddMessageError("Health certificate print location is required when certificate print indicator is Automatic, Customs Certificate or Manual.");
				}
			}
		}

		protected override void CheckQH_AQISRegion()
		{
			base.CheckQH_AQISRegion();
			if (IsValidationRequired && !Parent.IsNEXDOCSActive)
			{
				if (!Parent.QH_AQISRegion.IsEmpty || (Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat && Parent.QH_ProduceType != EXDOCCommodityCodes.Codes.InedibleMeat))
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.QH_AQISRegionInfo);
				}
			}
		}

		protected override void CheckQH_RN_NKOriginCountry()
		{
			base.CheckQH_RN_NKOriginCountry();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.QH_RN_NKOriginCountryInfo, Parent.Lookups.OriginCountries);
			}
		}

		protected override void CheckQH_InspectorComments()
		{
			base.CheckQH_InspectorComments();
			if (IsValidationRequired)
			{
				if (Parent.QH_AuthorisingOfficerID.IsEmpty && !Parent.QH_InspectorComments.IsEmpty)
				{
					Parent.QH_InspectorCommentsInfo.AddMessageError("Inspector comments are not required when officer ID is empty");
				}
			}
		}

		protected override void CheckQH_StartHoldSeal()
		{
			base.CheckQH_StartHoldSeal();
			if (IsValidationRequired && !Parent.QH_StartHoldSeal.IsEmpty)
			{
				if (Parent.QH_StartHoldSeal.Length != 7)
				{
					Parent.QH_StartHoldSealInfo.AddMessageError("Start hold seal number must be 7 characters.");
				}

				if (Parent.QH_AuthorisingOfficerID.IsEmpty)
				{
					Parent.QH_StartHoldSealInfo.AddMessageError("Start hold seal must not be present when officer ID is empty.");
				}

				EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(Parent.QH_ProduceType, Parent.QH_StartHoldSealInfo, "Start Hold Seal");
			}
		}

		protected override void CheckQH_EndHoldSeal()
		{
			base.CheckQH_EndHoldSeal();
			if (IsValidationRequired && !Parent.QH_EndHoldSeal.IsEmpty)
			{
				if (Parent.QH_EndHoldSeal.Length != 7)
				{
					Parent.QH_EndHoldSealInfo.AddMessageError("End hold seal number must be 7 characters.");
				}

				if (Parent.QH_AuthorisingOfficerID.IsEmpty)
				{
					Parent.QH_EndHoldSealInfo.AddMessageError("End hold seal must not be present when officer ID is empty.");
				}

				EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(Parent.QH_ProduceType, Parent.QH_EndHoldSealInfo, "End Hold Seal");
			}
		}

		protected override void CheckQH_SplitHealthCertByContainer()
		{
			base.CheckQH_SplitHealthCertByContainer();
			if (IsValidationRequired)
			{
				if (Parent.QH_SplitHealthCertByContainer &&
				(Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants ||
				Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Horticulture))
				{
					Parent.QH_SplitHealthCertByContainerInfo.AddMessageError("Split by container cannot be set when produce type is horticulture or grains and plants.");
				}
			}
		}

		protected override void CheckQH_SplitHealthCertByMarks()
		{
			base.CheckQH_SplitHealthCertByMarks();
			if (IsValidationRequired)
			{
				if (Parent.QH_SplitHealthCertByMarks &&
				(Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants ||
				Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Horticulture))
				{
					Parent.QH_SplitHealthCertByMarksInfo.AddMessageError("Split by marks cannot be set when produce type is horticulture or grains and plants.");
				}
			}
		}

		protected override void CheckQH_SplitHealthCertByPacker()
		{
			base.CheckQH_SplitHealthCertByPacker();
			if (IsValidationRequired)
			{
				if (Parent.QH_SplitHealthCertByPacker &&
				(Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants ||
				Parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Horticulture))
				{
					Parent.QH_SplitHealthCertByPackerInfo.AddMessageError("Split by packer cannot be set when produce type is horticulture or grains and plants.");
				}
			}
		}

		protected override void CheckQH_ForwardStatus()
		{
			base.CheckQH_ForwardStatus();
			if (IsValidationRequired)
			{
				if (!Parent.IsNEXDOCSActive && Parent.QH_ForwardStatus.IsEmpty && !Parent.QH_ForwardeeEDIUserIdentifier.IsEmpty)
				{
					Parent.QH_ForwardStatusInfo.AddMessageError("Forward status may not be empty when forward edi user identifier is not empty.");
				}
				else if (!Parent.QH_ForwardStatus.IsEmpty && Parent.QH_ForwardeeEDIUserIdentifier.IsEmpty)
				{
					Parent.QH_ForwardStatusInfo.AddMessageError("Forward status is not allowed when forward edi user identifier is not entered.");
				}
			}
		}

		protected override void CheckQH_ImportedProductFlag()
		{
			base.CheckQH_ImportedProductFlag();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.QH_ImportedProductFlagInfo);
			}
		}

		protected override void CheckQH_DecOfCompliance()
		{
			base.CheckQH_DecOfCompliance();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.QH_DecOfComplianceInfo);
			}
		}

		internal static string DeclarationOfComplianceMessage
		{
			get { return Res.GetString("{EA6766F8-B114-4E5B-A4B5-719E4293872B}", "The declaration of compliance question needs to be answered when produce type is dairy, eggs, fish, meat, grain or horticulture."); }
		}

		protected override void CheckQH_OA_AuthorisationEstablishment()
		{
			if (Parent.QH_AuthorisationLocation == EXDOCCodeOrganisation.Codes.Organisation)
			{
				var errorMsgRequired = false;
				base.CheckQH_OA_AuthorisationEstablishment();
				if (IsValidationRequired)
				{
					if (Parent.QH_AuthorisationEstablishment.IsEmpty && AuthorisationEstablishmentShouldBeEntered)
					{
						if (Parent.QH_OA_AuthorisationEstablishment.IsEmpty)
						{
							errorMsgRequired = true;
						}
						else
						{
							errorMsgRequired = true;
							var organisation = (OrgHeader)Parent.QH_OA_AuthorisationEstablishment_ZAddress.OrgHeader;
							if (organisation != null)
							{
								var orgEstablishmentNo = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, Parent.Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
								if (!orgEstablishmentNo.IsEmpty)
								{
									errorMsgRequired = false;
								}
							}
						}

						if (errorMsgRequired)
						{
							Parent.QH_OA_AuthorisationEstablishmentInfo.AddMessageError(ZString.Format(ESNIsRequired, "Authorisation"));
						}
					}
				}
			}
		}

		protected override void CheckQH_OA_StorageEstablishment()
		{
			base.CheckQH_OA_StorageEstablishment();
			if (IsValidationRequired)
			{
				if (!Parent.QH_OA_StorageEstablishment.IsEmpty && Parent.QH_StorageEstablishment.IsEmpty)
				{
					Parent.QH_OA_StorageEstablishmentInfo.AddMessageError(ZString.Format(ESNIsRequired, "Storage"));
				}
			}
		}
		internal const string ESNIsRequired = "An ESN code must be entered for the selected {0} Location Address. Please press F3, go to Details > Config > Registration Numbers/Codes and enter the ESN code for the selected address.";

		protected override void CheckQH_OH_PrintLocationOrganisationIsValidZGuid()
		{
			if (Parent.QH_OH_PrintLocationOrganisation != ZGuid.Invalid)
			{
				ListValidation.ErrorIfInvalidPK(Parent.QH_OH_PrintLocationOrganisationInfo, Parent.Lookups.EDIUser, (NoResString)NEXDOCRequired);
			}
			else
			{
				base.CheckQH_OH_PrintLocationOrganisationIsValidZGuid();
			}
		}

		protected override void CheckQH_OH_ForwardLocationOrganisationIsValidZGuid()
		{
			if (Parent.QH_OH_ForwardLocationOrganisation != ZGuid.Invalid)
			{
				ListValidation.ErrorIfInvalidPK(Parent.QH_OH_ForwardLocationOrganisationInfo, Parent.Lookups.EDIUser, (NoResString)NEXDOCRequired);
			}
			else
			{
				base.CheckQH_OH_ForwardLocationOrganisationIsValidZGuid();
			}
		}

		protected override void CheckQH_OH_TransferEDIUserLocationOrganisationIsValidZGuid()
		{
			if (Parent.QH_OH_TransferEDIUserLocationOrganisation != ZGuid.Invalid)
			{
				ListValidation.ErrorIfInvalidPK(Parent.QH_OH_TransferEDIUserLocationOrganisationInfo, Parent.Lookups.EDIUser, (NoResString)NEXDOCRequired);
			}
			else
			{
				base.CheckQH_OH_TransferEDIUserLocationOrganisationIsValidZGuid();
			}
		}

		internal const string NEXDOCRequired = "Organisation must have NEXDOC External ID entered against Config tab.";

		protected override void CheckQH_OH_TransferExporterLocationOrganisationIsValidZGuid()
		{
			if (Parent.QH_OH_TransferExporterLocationOrganisation != ZGuid.Invalid)
			{
				ListValidation.ErrorIfInvalidPK(Parent.QH_OH_TransferExporterLocationOrganisationInfo, Parent.Lookups.ExporterNumber, (NoResString)EXDOCESNRequired);
			}
			else
			{
				base.CheckQH_OH_TransferExporterLocationOrganisationIsValidZGuid();
			}
		}

		protected override void CheckQH_ConsigneeAgentName()
		{
			base.CheckQH_ConsigneeAgentName();
			if (IsValidationRequired && !Parent.QH_ConsigneeAgentName.IsEmpty)
			{
				EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(Parent.QH_ProduceType, Parent.QH_ConsigneeAgentNameInfo, "Consignee Representative");
			}
		}

		protected override void CheckQH_LoadingDate()
		{
			base.CheckQH_LoadingDate();
			if (IsValidationRequired)
			{
				var parent = Parent;
				if (parent.QH_ProduceType == EXDOCCommodityCodes.Codes.SkinsAndHides)
				{
					var propertyInfo = parent.QH_LoadingDateInfo;
					var establishmentNumberIsEmpty = parent.InvoiceHeader.AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber.IsEmpty;
					var loadingDateIsEmpty = parent.QH_LoadingDate.IsEmpty;
					if (loadingDateIsEmpty && !establishmentNumberIsEmpty)
					{
						propertyInfo.AddMessageError(Res.GetString("Enterprise.Customs.AU.Declaration.Business.QuarantineExDocHeaderValidation|QH_LoadingDate_MustBeEntered", "Loading date must be entered."));
					}
					else if (!loadingDateIsEmpty && establishmentNumberIsEmpty)
					{
						propertyInfo.AddMessageError(Res.GetString("Enterprise.Customs.AU.Declaration.Business.QuarantineExDocHeaderValidation|QH_LoadingDate_EmptyEstablishmentID", "The loading date may only be present if the loading establishment ID is also present."));
					}
				}
			}
		}

		internal const string EXDOCESNRequired = "Organisation must have EXDOC Establishment Number entered against Config tab.";
	}
}
