
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusHAWBValidation : AirCargoCusHAWBValidation
	{
		public CMRCusHAWBValidation(CusHAWB hAWB)
			: base(hAWB)
		{
		}

		new CusHAWB Parent
		{
			get { return base.Parent; }
		}

		protected override void CheckCS_HAWB()
		{
			base.CheckCS_HAWB();
			if (HAWB.CS_HAWB.IsEmpty && !HAWB.IsDirect)
			{
				Parent.CS_HAWBInfo.AddError("Housebill number is required for air cargo messaging.");
			}
			else
			{
				new CustomsValidation(HAWB.CS_HAWBInfo).ErrorOnKeyDataWithNoChildren(HAWB.CMRMessageStatus.Code);
			}
		}

		protected override void CheckCS_ConsigneeStreet()
		{
			base.CheckCS_ConsigneeStreet();
			CheckConsigneeAddress(HAWB.CS_ConsigneeStreetInfo);
		}

		protected override void CheckCS_ConsigneeStreet2()
		{
			base.CheckCS_ConsigneeStreet2();
			CheckConsigneeAddress(HAWB.CS_ConsigneeStreet2Info);
		}

		protected override void CheckCS_ConsigneeCity()
		{
			base.CheckCS_ConsigneeCity();
			CheckConsigneeAddress(HAWB.CS_ConsigneeCityInfo);
		}

		protected override void CheckCS_ConsigneePostcode()
		{
			base.CheckCS_ConsigneePostcode();
			CheckConsigneeAddress(HAWB.CS_ConsigneePostcodeInfo);
		}

		protected override void CheckCS_ConsigneeState()
		{
			base.CheckCS_ConsigneeState();
			CheckConsigneeAddress(HAWB.CS_ConsigneeStateInfo);
		}

		protected override void CheckCS_RN_NKConsigneeCountry()
		{
			base.CheckCS_RN_NKConsigneeCountry();
			CheckConsigneeAddress(HAWB.CS_RN_NKConsigneeCountryInfo);
		}

		void CheckConsigneeAddress(ZPropertyInfo info)
		{
			if (!ZString.Format("{0}{1}{2}{3}{4}{5}", HAWB.CS_ConsigneeStreet, HAWB.CS_ConsigneeStreet2, HAWB.CS_ConsigneeCity, HAWB.CS_ConsigneePostcode, HAWB.CS_ConsigneeState, HAWB.CS_RN_NKConsigneeCountry).ContainsAnyLetters)
			{
				info.AddMessageError("Some part of the Consignee address must be entered, and there must be at least 1 alpha character in some part of the address.");
			}
			HAWB.Validation.ValidateCS_ConsigneeStreet();
			HAWB.Validation.ValidateCS_ConsigneeStreet2();
			HAWB.Validation.ValidateCS_ConsigneeCity();
			HAWB.Validation.ValidateCS_ConsigneePostcode();
			HAWB.Validation.ValidateCS_ConsigneeState();
			HAWB.Validation.ValidateCS_RN_NKConsigneeCountry();
		}

		protected override void CheckCS_ConsignorStreet()
		{
			base.CheckCS_ConsignorStreet();
			CheckConsignorAddress(HAWB.CS_ConsignorStreetInfo);
		}

		protected override void CheckCS_ConsignorStreet2()
		{
			base.CheckCS_ConsignorStreet2();
			CheckConsignorAddress(HAWB.CS_ConsignorStreet2Info);
		}

		protected override void CheckCS_ConsignorCity()
		{
			base.CheckCS_ConsignorCity();
			CheckConsignorAddress(HAWB.CS_ConsignorCityInfo);
		}

		protected override void CheckCS_ConsignorPostcode()
		{
			base.CheckCS_ConsignorPostcode();
			CheckConsignorAddress(HAWB.CS_ConsignorPostcodeInfo);
		}

		protected override void CheckCS_ConsignorState()
		{
			base.CheckCS_ConsignorState();
			CheckConsignorAddress(HAWB.CS_ConsignorStateInfo);
		}

		protected override void CheckCS_RN_NKConsignorCountry()
		{
			base.CheckCS_RN_NKConsignorCountry();
			CheckConsignorAddress(HAWB.CS_RN_NKConsignorCountryInfo);
		}

		void CheckConsignorAddress(ZPropertyInfo info)
		{
			if (!ZString.Format("{0}{1}{2}{3}{4}{5}", HAWB.CS_ConsignorStreet, HAWB.CS_ConsignorStreet2, HAWB.CS_ConsignorCity, HAWB.CS_ConsignorPostcode, HAWB.CS_ConsignorState, HAWB.CS_RN_NKConsignorCountry).ContainsAnyLetters)
			{
				info.AddMessageError("Some part of the Consignor address must be entered, and there must be at least 1 alpha character in some part of the address");
			}
			HAWB.Validation.ValidateCS_ConsignorStreet();
			HAWB.Validation.ValidateCS_ConsignorStreet2();
			HAWB.Validation.ValidateCS_ConsignorCity();
			HAWB.Validation.ValidateCS_ConsignorPostcode();
			HAWB.Validation.ValidateCS_ConsignorState();
			HAWB.Validation.ValidateCS_RN_NKConsignorCountry();
		}

		protected override void CheckCS_IsSpecialReporter()
		{
			base.CheckCS_IsSpecialReporter();
			if (Parent.CS_IsSpecialReporter && string.IsNullOrEmpty(Env.Registry.AUCustoms.HVLVSpecialReporterNumber))
			{
				Parent.CS_IsSpecialReporterInfo.AddMessageError(GetSpecialReporterNumberEmptyMessage("HVLV"));
			}
			CheckRemailReporterAndHVLVReporter(Parent.CS_IsSpecialReporterInfo);
		}

		internal static string GetSpecialReporterNumberEmptyMessage(string name)
		{
			return string.Format("{0} Special Reporter Number is empty. Please set it at System -> Registry -> Customs -> Australia -> CMR -> {0} Sepecial Reporter Number", name);
		}

		protected override void CheckCS_IsRemailReporter()
		{
			base.CheckCS_IsRemailReporter();
			if (Parent.CS_IsRemailReporter)
			{
				if (!Parent.CS_IsMasterHouse)
				{
					Parent.CS_IsRemailReporterInfo.AddMessageError(RemailReporterMustBeAtSubMawbLevel);
				}
				if (string.IsNullOrEmpty(Env.Registry.AUCustoms.RemailSpecialReporterNumber))
				{
					Parent.CS_IsRemailReporterInfo.AddMessageError(GetSpecialReporterNumberEmptyMessage("Remail"));
				}
			}
			CheckRemailReporterAndHVLVReporter(Parent.CS_IsRemailReporterInfo);
		}
		internal const string RemailReporterMustBeAtSubMawbLevel = "Remail consignments must be reported at sub mawb level.";

		void CheckRemailReporterAndHVLVReporter(ZPropertyInfo info)
		{
			if (Parent.CS_IsRemailReporter && Parent.CS_IsSpecialReporter)
			{
				info.AddMessageError(RemailAndHVLVCannotBeBothReported);
			}
		}
		internal const string RemailAndHVLVCannotBeBothReported = "Remail and HVLV cannot be both reported.";

		#region CS_RX_NKGoodsCurrency

		protected override void CheckCS_RX_NKGoodsCurrency()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CS_RX_NKGoodsCurrencyInfo, HAWB.Lookups.CMRCurrencyList);

			if (Parent.CS_RX_NKGoodsCurrency.IsEmpty && IsGoodsCurrencyRequired)
			{
				Parent.CS_RX_NKGoodsCurrencyInfo.AddMessageError("A goods currency must be specified for air cargo messaging.");
			}
			else if (Shipment != null && Shipment.GoodsValueCurr != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_RX_NKGoodsCurrencyInfo, Shipment.GoodsValueCurr.PK);
			}

			var mawb = Parent.MAWB;
			if (IsGoodsCurrencyRequired && !Parent.CS_RX_NKGoodsCurrency.IsEmpty && Parent.CS_RX_NKGoodsCurrency != JobDeclaration.LocalCurrencyConstantCode && mawb != null && mawb.CM_DepartureDate.IsEmpty)
			{
				Parent.CS_RX_NKGoodsCurrencyInfo.AddWarning(MissingDepatureDate);
			}
		}
		internal const string MissingDepatureDate = "Unable to determine goods value in AUD since depature date used for exchange rate lookups is missing.";

		protected override bool IsFreightPrepaidCollectRequired
		{
			get { return HAWB.CS_GoodsValue > 0; }
		}

		protected override bool IsGoodsCurrencyRequired
		{
			get { return HAWB.CS_GoodsValue > 0; }
		}

		#endregion

		#region CS_IsSelfAssessedClearance

		protected override void CheckCS_IsSelfAssessedClearance()
		{
			base.CheckCS_IsSelfAssessedClearance();
			if (HAWB.CS_IsSelfAssessedClearance)
			{
				if (HAWB.CS_IsMasterHouse)
				{
					HAWB.CS_IsSelfAssessedClearanceInfo.AddMessageError("For co-load master shipment, self-clearance is not allowed.");
				}

				var factory = HAWB.Factory;
				SACDecider decider = new SACDecider(factory, HAWB.GoodsValueInLocalCurrency, HAWB.GoodsDescription);
				if (!decider.IsValidForSAC)
				{
					if (decider.IsValueOverTheScreenFreeValue)
					{
						var deminimus = UniversalReferenceHelper.GetDeminimus(factory);

						HAWB.CS_IsSelfAssessedClearanceInfo.AddWarning("Self-assessed clearance should apply only for consignment valued less than or equal to " + deminimus + " " + JobDeclaration.LocalCurrencyConstantCode);
					}

					if (decider.StopPhrasesFoundInGoodsDescription.Any())
					{
						HAWB.CS_IsSelfAssessedClearanceInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, "Self-assessed clearance is not allowed as the goods description contains the following words ({0}) which are found in the Thesaurus provided by Customs.", decider.ThesaurusWordsFoundAsSingleString()));
					}
				}
				else if (AUCustomsDataRegistry.Instance.ManifestSACOverride.Value)
				{
					if (decider.StopPhrasesFoundInGoodsDescription.Any())
					{
						HAWB.CS_IsSelfAssessedClearanceInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, "Self-assessed clearance has been indicated here as the registry to ignore the Thesaurus check is activated.\r\nHowever, the following words in the goods description, ({0}), are found in the Thesaurus provided by Customs.", decider.ThesaurusWordsFoundAsSingleString()));
					}
				}
			}
		}

		#endregion
	}
}
