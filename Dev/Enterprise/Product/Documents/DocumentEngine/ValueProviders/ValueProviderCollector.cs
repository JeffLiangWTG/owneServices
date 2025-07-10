using System.Collections;
using CargoWise.Application;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ValueProviders.Macros;

namespace Enterprise.DocumentEngine
{
	// Please DO NOT add to this list without talking to someone in DocEngine - Zubin or Ben.
	internal sealed class ValueProviderCollector
	{
		public ProviderCache ValueProviders
		{
			get { return valueProviders ?? (valueProviders = GetValueProviders()); }
		}
		ProviderCache valueProviders;

		ProviderCache GetValueProviders()
		{
			ProviderCache result = new ProviderCache();

			if (ObjectFactory.IsConfigured)
			{
				foreach (ValueProvider valueProvider in (ArrayList)ObjectFactory.Get("DocumentEngineValueProviders"))
				{
					result.AddProvider(valueProvider);
				}
			}

			if (ObjectFactory.IsConfigured)
			{
				foreach (ValueProvider valueProvider in (ArrayList)ObjectFactory.Get("ExtraValueProviders"))
				{
					result.AddProvider(valueProvider);
				}
			}

			result.AddProvider(new AccumulativeTotal());
			result.AddProvider(new AutoHeight());
			result.AddProvider(new BalanceSheetStartingAccount());
			result.AddProvider(new ChinaBalanceSheetStartingAccount());
			result.AddProvider(new BranchCode());
			result.AddProvider(new BranchProxy());
			result.AddProvider(new CapitaliseFirstLetterOfWords());
			result.AddProvider(new CodePairValue());
			result.AddProvider(new SelectList());
			result.AddProvider(new KeyedCodePairValue());
			result.AddProvider(new CompanyAddress1());
			result.AddProvider(new CompanyAddress2());
			result.AddProvider(new CompanyCity());
			result.AddProvider(new CompanyCode());
			result.AddProvider(new CompanyCountry());
			result.AddProvider(new CompanyCountryCode());
			result.AddProvider(new CompanyCustomsCountryCode());
			result.AddProvider(new CompanyCurrencyCode());
			result.AddProvider(new CompanyCustomsCurrencyCode());
			result.AddProvider(new CompanyFax());
			result.AddProvider(new CompanyIsReciprocal());
			result.AddProvider(new CompanyName());
			result.AddProvider(new CompanyNameChina());
			result.AddProvider(new CompanyNameFromPK());
			result.AddProvider(new CompanyOfficeAddress1());
			result.AddProvider(new CompanyOfficeAddress2());
			result.AddProvider(new CompanyPhone());
			result.AddProvider(new CompanyPK());
			result.AddProvider(new CompanyPostalAddress1());
			result.AddProvider(new CompanyPostalAddress2());
			result.AddProvider(new CompanyPostCode());
			result.AddProvider(new CompanyReg1());
			result.AddProvider(new CompanyReg2());
			result.AddProvider(new CompanyState());
			result.AddProvider(new CompanyWebAddress());
			result.AddProvider(new ConvertDimension());
			result.AddProvider(new ConvertUTCDateTimeToLocal());
			result.AddProvider(new ConvertVolume());
			result.AddProvider(new ConvertWeight());
			result.AddProvider(new Count());
			result.AddProvider(new Counter());
			result.AddProvider(new Currency());
			result.AddProvider(new CurrencyLanguageName());
			result.AddProvider(new CurrencyToWords());
			result.AddProvider(new CurrentCompany());
			result.AddProvider(new CurrentBranch());
			result.AddProvider(new CurrentDepartment());
			result.AddProvider(new CurrencySubUnitFormat());
			result.AddProvider(new CurrencyMajorUnit());
			result.AddProvider(new CurrentPage());
			result.AddProvider(new CustomisedColumn());
			result.AddProvider(new DatabaseName());
			result.AddProvider(new DatabaseServer());
			result.AddProvider(new DataMatrix());
			result.AddProvider(new DateTimeAsString());
			result.AddProvider(new DateTimeStart());
			result.AddProvider(new DataType());
			result.AddProvider(new DocumentClosingText());
			result.AddProvider(new DocumentCustomLabel());
			result.AddProvider(new DocumentOpeningText());
			result.AddProvider(new ExcelFormula());
			result.AddProvider(new ExchangeRate());
			result.AddProvider(new FirstDayOfLastWeek());
			result.AddProvider(new FormatPhoneNumber());
			result.AddProvider(new FreightChargeCode());
			result.AddProvider(new GetBusinessObjectValue());
			result.AddProvider(new GetBusinessObjectNumericValueWithZeroAsEmpty());
			result.AddProvider(new GroupCount());
			result.AddProvider(new GroupIndex());
			result.AddProvider(new GroupTotalPages());
			result.AddProvider(new GroupCurrentPage());
			result.AddProvider(new HideRowIf());
			result.AddProvider(new HideRowIfCellIsEmpty());
			result.AddProvider(new HPageBreak());
			result.AddProvider(new Image());
			result.AddProvider(new IsDocumentCustomLabelHidden());
			result.AddProvider(new IsFunctionalityValid());
			result.AddProvider(new IsReportCustomLabelHidden());
			result.AddProvider(new LandedCostingPreference());
			result.AddProvider(new LanguageFromCode());
			result.AddProvider(new LastDayOfLastWeek());
			result.AddProvider(new LastDateTransformRunUtcForEdw());
			result.AddProvider(new LocalBusinessNumber());
			result.AddProvider(new LoginCode());
			result.AddProvider(new LoginEmail());
			result.AddProvider(new LoginFullName());
			result.AddProvider(new LoginName());
			result.AddProvider(new LoginPK());
			result.AddProvider(new MenuTitle());
			result.AddProvider(new ModuleDescriptionFromCode());
			result.AddProvider(new MultilinePayableAt());
			result.AddProvider(new MultilinePrePaidCharges());
			result.AddProvider(new MultilingualAccount());
			result.AddProvider(new Now());
			result.AddProvider(new ReportTitle());
			result.AddProvider(new NumberToWords());
			result.AddProvider(new PLAppropriationAccount());
			result.AddProvider(new PostalAddress());
			result.AddProvider(new ProfitShareChargeCode());
			result.AddProvider(new RecipientCompanyCode());
			result.AddProvider(new RecipientCompanyName());
			result.AddProvider(new RecipientEmailAddress());
			result.AddProvider(new RecipientFaxNumber());
			result.AddProvider(new RecipientName());
			result.AddProvider(new RecipientAddress());
			result.AddProvider(new RecipientNameAndAddress());
			result.AddProvider(new RecipientPhoneNumber());
			result.AddProvider(new RecipientNameAndIntendedRecipientAddress());
			result.AddProvider(new IntendedRecipientAddress());
			result.AddProvider(new RecipientSalutation());
			result.AddProvider(new RegistryItem());
			result.AddProvider(new ReportCustomLabel());
			result.AddProvider(new ReportName());
			result.AddProvider(new MacroUntranslatedValueProvider());
			result.AddProvider(new ScheduledTaskDescription());
			result.AddProvider(new SelectedSortOrder());
			result.AddProvider(new ShipmentPrePaidCharges());
			result.AddProvider(new SignOff());
			result.AddProvider(new SignOffText());
			result.AddProvider(new TaxCode());
			result.AddProvider(new TextLineAt());
			result.AddProvider(new Total());
			result.AddProvider(new TotalPages());
			result.AddProvider(new SelectedGroupbyOption());
			result.AddProvider(new DescriptionFromCode());
			result.AddProvider(new RegistryAPPaymentMethod());
			result.AddProvider(new GetShowEditFormUrl());
			result.AddProvider(new RecipientContactPK());
			result.AddProvider(new ServerVersion());
			result.AddProvider(new GetTrackingUrl());
			result.AddProvider(new GetUserEmailWhoRaisedEvent());
			result.AddProvider(new UrlHyperlink());
			result.AddProvider(new EnterpriseInformation());
			result.AddProvider(new CustomsCode());
			result.AddProvider(new LoginStaffCertificateNumber());
			result.AddProvider(new BranchPortName());
			result.AddProvider(new ShrinkToFit());
			result.AddProvider(new LoginPhoneNumber());
			result.AddProvider(new LoginPhoneExtensionNumber());
			result.AddProvider(new LoginFaxNumber());
			result.AddProvider(new CompanyLicenceInfo());
			result.AddProvider(new AbriBar128sBarCode());
			result.AddProvider(new QrCode());
			result.AddProvider(new Modifiable());
			result.AddProvider(new NonModifiable());
			result.AddProvider(new BranchProxyName());
			result.AddProvider(new BranchProxyCode());
			result.AddProvider(new CompanyProxyCode());
			result.AddProvider(new RecipientNameAndAddressFollowingByContactName());
			result.AddProvider(new DeliveryMode());
			result.AddProvider(new LoginTitle());
			result.AddProvider(new FallbackIfEmpty());
			result.AddProvider(new SubString());
			result.AddProvider(new DeliveryCount());
			result.AddProvider(new EvaluateInnerContent());
			result.AddProvider(new If());
			result.AddProvider(new Delimit());
			result.AddProvider(new ReportNameShort());
			result.AddProvider(new Truncate());
			result.AddProvider(new AddTitleIfNotEmpty());
			result.AddProvider(new Left());
			result.AddProvider(new Upper());
			result.AddProvider(new Substitute());
			result.AddProvider(new ReplaceCarriageReturnsWithSpaces());
			result.AddProvider(new USCustomsDisbursementChargeCodesValueProvider());
			result.AddProvider(new ConsumptionTaxRegistrationCodesList());
			result.AddProvider(new CalculateConsolChargeable());
			result.AddProvider(new FormatOrgAddress());
			result.AddProvider(new FormatJobDocAddress());
			result.AddProvider(new Html());
			result.AddProvider(new HtmlEx());
			result.AddProvider(new FormatNumber());
			result.AddProvider(new RegistryItemBusinessObject());
			result.AddProvider(new RegistryItemTVP());
			result.AddProvider(new SuppressField());
			result.AddProvider(new IsDraft());
			result.AddProvider(new ModifiableField());
			result.AddProvider(new ExpandToFit());
			result.AddProvider(new CurrentPeriod());
			result.AddProvider(new OverFlowToFollowPage());
			result.AddProvider(new GetCategoriesForReport());
			result.AddProvider(new GetCategoryDescForReport());
			result.AddProvider(new CurrentCountryTaxRegistrationOrgCusCode());
			result.AddProvider(new Add());
			result.AddProvider(new Subtract());
			result.AddProvider(new Multiply());
			result.AddProvider(new Divide());
			result.AddProvider(new Coalesce());
			result.AddProvider(new AddDurationToDate());
			result.AddProvider(new DurationAsDateTime());
			result.AddProvider(new CountryExtraTaxDescription());
			result.AddProvider(new CountryExtraTaxDescriptionRecoverable());
			result.AddProvider(new CountryExtraTaxDescriptionNotRecoverable());
			result.AddProvider(new FormatCanadianTaxRate());
			result.AddProvider(new SSCCCheckDigit());
			result.AddProvider(new ShrinkToFitForBillOfLading());
			result.AddProvider(new CreditOnHold());
			result.AddProvider(new HasEvent());
			result.AddProvider(new ComplianceSubTypeLocalDescription());
			result.AddProvider(new TranslateDBField());
			result.AddProvider(new TranslateRegistry());
			result.AddProvider(new Absolute());
			result.AddProvider(new BoolToYN());
			result.AddProvider(new UserRepository());
			result.AddProvider(new ChangeCase());
			result.AddProvider(new EmailSenderOverride());
			result.AddProvider(new GetAccountingControllerID());
			result.AddProvider(new GetTranslatedAddress());
			result.AddProvider(new TransportCoTrackingURL());
			result.AddProvider(new AttachmentType());
			result.AddProvider(new UPCA());
			result.AddProvider(new EAN13());
			result.AddProvider(new EAN8());
			result.AddProvider(new PDF417());
			result.AddProvider(new GetGeography());
			result.AddProvider(new IsPointInShape());
			result.AddProvider(new UTCTimeOffset());
			result.AddProvider(new WhsInventoryDutyAndTax());
			result.AddProvider(new CODE39());
			result.AddProvider(new UnselectedCollectionBatchTypesMacro());
			result.AddProvider(new ComplianceDocumentSupportingReasonDescription());
			result.AddProvider(new IsProductionSystem());
			result.AddProvider(new OverCreditLimit());
			result.AddProvider(new EquivalentComplianceSubTypeCode());
			result.AddProvider(new ApprovalTaskUrl());
			result.AddProvider(new GetGuidByOrgCode());
			result.AddProvider(new GetCurrentManagerCodeByType());
			result.AddProvider(new SetAndReturnStaffTempPassword());
			result.AddProvider(new GetStaffValueForPerson());
			result.AddProvider(new DateDiff());
			result.AddProvider(new Contains());
			result.AddProvider(new GetPropertyFromEntity());
			result.AddProvider(new IsRunReportFromEDW());
			result.AddProvider(new HasValidGeneralLedgerData());
			result.AddProvider(new GetEventReferenceValue());
			result.AddProvider(new StaffLoginNamesRelatedToAllTasks());
			result.AddProvider(new GSTVATConversionExchangeRate());
			result.AddProvider(new ProductName());
			result.AddProvider(new TextGroup());
			result.AddProvider(new FeatureControl());
			result.AddProvider(new ReportingBookPresentation());

			return result;
		}
	}
}
