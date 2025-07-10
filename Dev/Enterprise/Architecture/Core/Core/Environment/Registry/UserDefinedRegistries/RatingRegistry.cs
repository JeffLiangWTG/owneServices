using System;
using System.Drawing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class RatingRegistry
	{
		public RatingRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public DateTime GRINotificationLastRunDate
		{
			get { return (DateTime)RawRegistry.GRINotificationLastRunDate.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.GRINotificationLastRunDate.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		#region Validity/Notification Periods

		public int RateValidityPeriod
		{
			get { return (int)RawRegistry.RateValidityPeriod.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public int CostRateValidityPeriod
		{
			get { return (int)RawRegistry.CostRateValidityPeriod.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public int GlobalTariffValidityPeriod
		{
			get { return (int)RawRegistry.GlobalTariffValidityPeriod.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public int ExpiredRateNotificationPeriod
		{
			get { return (int)RawRegistry.ExpiredRateNotificationPeriod.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public int ExpiringRateNotificationPeriod
		{
			get { return (int)RawRegistry.ExpiringRateNotificationPeriod.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public DeleteExpiredRates PermanentlyDeleteRatesExpiredPeriod
		{
			get { return (DeleteExpiredRates)RawRegistry.PermanentlyDeleteRatesExpiredPeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PermanentlyDeleteRatesExpiredPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool QuoteEndDateMandatory
		{
			get { return (bool)RawRegistry.QuoteEndDateMandatory.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.QuoteEndDateMandatory.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int WebRateValidityPeriod
		{
			get { return (int)RawRegistry.WebRateValidityPeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.WebRateValidityPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public QuoteValidityRegistryItem QuoteValidityPeriod
		{
			get { return RawRegistry.QuoteValidityPeriod; }
#if DEBUG
			set { RawRegistry.QuoteValidityPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.Value); }
#endif
		}

		#endregion

		#region Documents

		#region Cover Page

		public string QuoteTitleText
		{
			get { return (MultilingualString)RawRegistry.QuoteDocumentHeading.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.QuoteDocumentHeading.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string OneOffQuoteTitleText
		{
			get { return (MultilingualString)RawRegistry.OneOffQuoteDocumentHeading.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.OneOffQuoteDocumentHeading.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string QuoteCoverPageTextExisting
		{
			get { return (MultilingualString)RawRegistry.CoverPageText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.CoverPageText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string QuoteCoverPageTextOneOffExisting
		{
			get { return (MultilingualString)RawRegistry.CoverPageTextOneOff.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.CoverPageTextOneOff.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string QuoteCoverPageTextNew
		{
			get { return (MultilingualString)RawRegistry.CoverPageTextNew.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.CoverPageTextNew.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string QuoteCoverPageTextOneOffNew
		{
			get { return (MultilingualString)RawRegistry.CoverPageTextOneOffNew.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.CoverPageTextOneOffNew.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string QuoteCoverPageFooterText
		{
			get { return (MultilingualString)RawRegistry.QuoteCoverPageFooterText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.QuoteCoverPageFooterText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string GRIUpdateCoverPageText
		{
			get { return (MultilingualString)RawRegistry.GRIUpdateCoverPageText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.GRIUpdateCoverPageText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string GRIUpdateFooterPageText
		{
			get { return (MultilingualString)RawRegistry.GRIUpdateFooterPageText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.GRIUpdateFooterPageText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Opening/Closing

		public string QuoteHeaderText
		{
			get { return (MultilingualString)RawRegistry.QuoteHeaderText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.QuoteHeaderText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif

		}

		#endregion

		#region Other Pages

		public string PublishedAgentsHeadingText
		{
			get { return (MultilingualString)RawRegistry.PublishedAgentsHeadingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool IsQuoteTermsAndConditionsPagesValueSet(int index)
		{
			var page = (IRegistryItemInternals)GetQuoteTermsAndConditionsPageItem(index);
			return page != null && page.HasValueForAnyLevel();
		}

		public Image GetQuoteTermsAndConditionsPage(int index)
		{
			var page = GetQuoteTermsAndConditionsPageItem(index);
			return page != null ? GetImage(page) : null;
		}

		IRegistryItem GetQuoteTermsAndConditionsPageItem(int index)
		{
			switch (index)
			{
				case 0:
					return RawRegistry.QuoteTCPage1;
				case 1:
					return RawRegistry.QuoteTCPage2;
				case 2:
					return RawRegistry.QuoteTCPage3;
				case 3:
					return RawRegistry.QuoteTCPage4;
				case 4:
					return RawRegistry.QuoteTCPage5;
				case 5:
					return RawRegistry.QuoteOtherPage1;
				case 6:
					return RawRegistry.QuoteOtherPage2;
				case 7:
					return RawRegistry.QuoteOtherPage3;
				case 8:
					return RawRegistry.QuoteOtherPage4;
				case 9:
					return RawRegistry.QuoteOtherPage5;
				default:
					return null;
			}
		}

#if DEBUG
		public void SetQuoteTermsAndConditionsPages(Image[] value)
		{
			SetImage(RawRegistry.QuoteTCPage1, value[0]);
			SetImage(RawRegistry.QuoteTCPage2, value[1]);
			SetImage(RawRegistry.QuoteTCPage3, value[2]);
			SetImage(RawRegistry.QuoteTCPage4, value[3]);
			SetImage(RawRegistry.QuoteTCPage5, value[4]);
			SetImage(RawRegistry.QuoteOtherPage1, value[5]);
			SetImage(RawRegistry.QuoteOtherPage2, value[6]);
			SetImage(RawRegistry.QuoteOtherPage3, value[7]);
			SetImage(RawRegistry.QuoteOtherPage4, value[8]);
			SetImage(RawRegistry.QuoteOtherPage5, value[9]);
		}
#endif

		#endregion

		public int RateLineSpacing
		{
			get { return (int)RawRegistry.RateLineSpacing.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool IncludeCFXOnQuote
		{
			get { return (bool)RawRegistry.IncludeCFXOnQuotation.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.IncludeCFXOnQuotation.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string FlatFeeText
		{
			get { return (MultilingualString)RawRegistry.FlatFeeText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public MultilingualString FlatFeeMultilingualText
		{
			get { return (MultilingualString)RawRegistry.FlatFeeText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool PrintScannedUserSignatureOnQuotationDocuments
		{
			get { return (bool)RawRegistry.PrintScannedUserSignatureOnQuotationDocs.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		#endregion

		#region Charge Code Groups

		public string[] FreightRatedCodes
		{
			get
			{
				string codes = (string)RawRegistry.FreightRatedCodes.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				return codes == null ? null : codes.Split(',');
			}
		}

#if DEBUG
		public void SetFreightRatedCodes(string value)
		{
			RawRegistry.FreightRatedCodes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
		}
#endif

		public string[] BrokerageRatedCodes
		{
			get
			{
				string codes = (string)RawRegistry.BrokerageRatedCodes.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				return codes == null ? null : codes.Split(',');
			}
		}

#if DEBUG
		public void SetBrokerageRatedCodes(string value)
		{
			RawRegistry.BrokerageRatedCodes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
		}
#endif

		public string[] BuyersConsolApportionedCodes
		{
			get
			{
				string codes = (string)RawRegistry.BuyersConsolApportionedCodes.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				return codes == null ? null : codes.Split(',');
			}
		}

#if DEBUG
		public void SetBuyersConsolApportionedCodes(string value)
		{
			RawRegistry.BuyersConsolApportionedCodes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
		}
#endif

		public string[] ShippersConsolApportionedCodes
		{
			get
			{
				string codes = (string)RawRegistry.ShippersConsolApportionedCodes.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				return codes == null ? null : codes.Split(',');
			}
		}

#if DEBUG
		public void SetShippersConsolApportionedCodes(string value)
		{
			RawRegistry.ShippersConsolApportionedCodes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
		}
#endif

		public string[] OriginBrokerageRatedCodes
		{
			get
			{
				string codes = (string)RawRegistry.OriginBrokerageRatedCodes.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				return codes == null ? null : codes.Split(',');
			}
		}

#if DEBUG
		public void SetOriginBrokerageRatedCodes(string value)
		{
			RawRegistry.OriginBrokerageRatedCodes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
		}
#endif

		#endregion

		public string FreightSearchPriorities
		{
			get { return (string)RawRegistry.FreightSearchPriorities.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.FreightSearchPriorities.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		#region  Container Classes

		public ReadOnlyCodeDescriptionPairList ContainerFreightRateClassList
		{
			get { return RawRegistry.ContainerFreightRateClassList.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public ReadOnlyCodeDescriptionPairList ContainerHandlingRateClassList
		{
			get { return RawRegistry.ContainerHandlingRateClassList.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		#endregion

		#region Mark-ups

		public string MarkUpPercentages
		{
			get { return (string)RawRegistry.MarkUpPercentages.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MarkUpPercentages.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MinimumMarkUpPercentages
		{
			get { return (string)RawRegistry.MinimumMarkUpPercentages.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MinimumMarkUpPercentages.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Storage Autorating

		public string StorageCalculationPeriod
		{
			get { return (string)RawRegistry.StorageCalculationPeriod.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.StorageCalculationPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ExcludeHolidaysInTimeRating
		{
			get { return (bool)RawRegistry.ExcludeHolidaysInTimeRating.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ExcludeHolidaysInTimeRating.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool IncludeCFSFreeStorageDaysInCalculation
		{
			get { return (bool)RawRegistry.IncludeCFSFreeStorageDaysInCalculation.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.IncludeCFSFreeStorageDaysInCalculation.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region CTO Addresses

		public string DefaultCTOAddressAir
		{
			get { return (string)RawRegistry.DefaultCTOPostCodeAir.GetFallBackValueAtAllLevels(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.DefaultCTOPostCodeAir.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string DefaultCTOAddressSea
		{
			get { return (string)RawRegistry.DefaultCTOPostCodeSea.GetFallBackValueAtAllLevels(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.DefaultCTOPostCodeSea.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string DefaultCTOAddressRail
		{
			get { return (string)RawRegistry.DefaultCTOPostCodeRail.GetFallBackValueAtAllLevels(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.DefaultCTOPostCodeRail.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string DefaultCFSAddressRoad
		{
			get { return (string)RawRegistry.DefaultCFSPostCodeRoad.GetFallBackValueAtAllLevels(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.DefaultCFSPostCodeRoad.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public bool UseDistanceCalculationService
		{
			get { return (bool)RawRegistry.UseDistanceCalculcationService.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.UseDistanceCalculcationService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Agency Calculator Entry Lines per Page

		public int AgencyCalcLinesFirstPageImport
		{
			get { return (int)RawRegistry.AgencyCalcLinesFirstPageImport.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AgencyCalcLinesFirstPageImport.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int AgencyCalcLinesFirstPageExport
		{
			get { return (int)RawRegistry.AgencyCalcLinesFirstPageExport.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AgencyCalcLinesFirstPageExport.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int AgencyCalcLinesAdditionalPageImport
		{
			get { return (int)RawRegistry.AgencyCalcLinesAdditionalPageImport.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AgencyCalcLinesAdditionalPageImport.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int AgencyCalcLinesAdditionalPageExport
		{
			get { return (int)RawRegistry.AgencyCalcLinesAdditionalPageExport.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AgencyCalcLinesAdditionalPageExport.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		public int TACTRateImportPartitionSize
		{
			get { return (int)RawRegistry.TACTRateImportPartitionSize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.TACTRateImportPartitionSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#region Implementation

		Image GetImage(IRegistryItem regItem)
		{
			return (Image)regItem.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

#if DEBUG
		void SetImage(IRegistryItem regItem, Image value)
		{
			if (value != null)
			{
				regItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
			}
		}
#endif

		readonly RawDataRegistry RawRegistry;

		#endregion
	}
}
