#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Import;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class CusProcedureWrapper : ICusProcedure
	{
		protected CusProcedureWrapper(CusEntryHeader cusEntryHeader, ZString actionCode, ZDateTime messageSentDate, ErrorCollector errorCollector)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			itemErrorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			invoiceHeaders = declaration.Invoices;
			calculator = new TransportCostCalculator(cusEntryHeader, declaration.JE_ShipmentIncoTerm, declaration.ZG_AgreedPlaceCode, declaration.JE_TransportMode, declaration.JE_AirRouteType);
			Argument.GreaterThanZero(declaration.Invoices.Count, Res.GetString("53caf7ba-731a-4ff1-9ad8-7a4dac4f5576", "One invoice must exist in customs procedure"));

			this.actionCode = actionCode;
			messageDate = messageSentDate;
		}

		#region Members

		public IAlternateCalcValue AlternateCalcValue => new AlternateCalcValueWrapper(entryHeader, itemErrorCollector);

		public ZString ProcedureType => GetProcedureType();

		public ZString EntryStyle => declaration.JE_EntryStyle;

		public ZString EntryStyleCode => GetEntryStyleCode();

		public ZShort ArticleCount => (ZShort)entryHeader.MergedLines.Count;

		public ZString EstimatedAssessmentDate => GetEstimatedAssessmentDate();

		public ZString EstimatedAssessmentHour => GetEstimatedAssessmentHour();

		public ZString DeclEmergencyProcDate => GetDeclEmergencyProcDate();

		public ZInt PackageCount => (int)entryHeader.CustomsPackageCount;

		public ZString AgreedGoodsLocation => declaration.JE_LocationOfGoods;

		public ZString ClearanceLocation => declaration.JE_SubLocationOfGoods;

		public ZString TransactionNature => GetTransactionNature();

		public ZString ArrivalState => declaration.FinalDestination?.RL_RN_NKCountryCode ?? ZString.Empty;

		public ZString DepartureState => declaration?.Origin?.RL_RN_NKCountryCode ?? ZString.Empty;

		public ICusOffice Office => new CusOfficeWrapper(entryHeader);

		public IEnumerable<IOrganisation> Suppliers => GetSuppliers();

		public IEnumerable<IOrganisation> Importers => GetImporters();

		public IOrganisation RepTaxOrganisation => RepTaxOrganisationWrapper.New(entryHeader, itemErrorCollector);

		public IOrganisation ImporterDeliveryAddress => new ImporterDeliveryAddressWrapper(entryHeader);

		public ZString RepresentationModeCode => declaration.RepresentationTypeNo;

		public ZString AgreementOwnerEORI => GetAgreementOwnerEORI();

		public ZString DeltaGAuthorisationNumber => GetCustomsAgreementNumber();

		public ZString BranchCusBrokerageCode => GetBranchCusBrokerageCode();

		public ZString DeferalApprovalCreditNumber => GetDeferalApprovalCreditNumber();

		public ZString VariousOperationCreditNumber => declaration.GetVariousOperationCreditNumber();

		public ZDecimal EntryGoodsPriceSum => GetEntryGoodsPriceSum();

		public ZString EntryGoodsPriceCurrency => GetEntryGoodsPriceCurrency();

		public ZDecimal EntryGoodsPriceCurrencyRate => GetEntryGoodsPriceCurrencyRate();

		public ZString PaymentMode => declaration.JE_PaymentMethod;

		public ZString GuaranteeMode => GetGuaranteeMode();

		public IDeliveryTerms DeliveryTerms => new DeliveryTermsWrapper(GetFirstMergedLine(entryHeader));

		public ITransport Transport => new TransportWrapper(entryHeader);

		public ICostsAndInsurance ThirdCountryTransportCosts => calculator.CumulTiers;

		public ICostsAndInsurance EUTransportCostsInInvoice => calculator.CumulCEHorsFRInclus;

		public ICostsAndInsurance EUTransportCostsNotInInvoice => calculator.CumulCEHorsFRExclus;

		public ICostsAndInsurance FRTransportCostsInInvoice => calculator.CumulFRInclus;

		public ICostsAndInsurance FRTransportCostsNotInInvoice => calculator.CumulFRExclus;

		public ICostsAndInsurance ThirdCountryAirCosts => calculator.CumulAerienTiers;

		public ICostsAndInsurance FRAirCosts => calculator.CumulAerienFR;

		public IAmountAndCurrency OthAddedCosts => AmountAndCurrencyWrapper.New(entryHeader, new List<ZString>()
		{ UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge,
		UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge,
		UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge }, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldBeDutiable + AmountAndCurrencyWrapper.ShouldBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency Interest => AmountAndCurrencyWrapper.New(entryHeader, new List<ZString>() { UCCCustomsChargeTypeList.Codes.InterestCharge }, AmountAndCurrencyWrapper.ShouldBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeDutiable + AmountAndCurrencyWrapper.ShouldNotBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency Commission => AmountAndCurrencyWrapper.New(entryHeader, new List<ZString>() { UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge }, AmountAndCurrencyWrapper.ShouldNotBeDutiable + AmountAndCurrencyWrapper.ShouldNotBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency VATBaseCosts => AmountAndCurrencyWrapper.New(entryHeader, new List<ZString>() { UCCCustomsChargeTypeList.Codes.AdjustmentCharge }, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeDutiable + AmountAndCurrencyWrapper.ShouldNotBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public ICostsAndInsurance DOMCostsAndInsurance => new CostsAndInsuranceWrapper(entryHeader, "", "", "");  //TODO : Addressed in new WI00302119 Delta C - Handling DOM fees in mapping

		public ZString ValuationBypassCode => entryHeader.EntryInstruction?.ZG_BypassCode ?? ZString.Empty;

		public ZString ValuationBypassReason => entryHeader.EntryInstruction?.ZG_BypassReason ?? ZString.Empty;

		public ZString VATOrganization => ZString.Empty;

		public ZString ImporterEORINumber => declaration.Importer?.GetEORI() ?? ZString.Empty;

		#region Export
		public IEnumerable<ZString> Itinerary => entryHeader.CountriesOfRouting.Where(x => x != DepartureState && x != DestinationState);

		public ZString CommercialReference => new ZString(null);

		public ZString SpecificCircumstanceIndicator => declaration.ZG_SpecificCircumstanceIndicator;

		public ZString OrigineState => declaration.JE_GoodsOrigin;

		public ZString DestinationState => declaration.JE_GoodsDestination;

		#endregion

		#endregion

		#region Methods
		IEnumerable<IOrganisation> GetImporters()
		{
			return GetOrganisations(declaration.HasNonStandardCountryOfDestination, declaration.JE_GoodsDestination, declaration.ImporterDocumentaryAddress, declaration.Importer, true);
		}

		IEnumerable<IOrganisation> GetSuppliers()
		{
			return GetOrganisations(declaration.HasNonStandardCountryOfOrigin, declaration.JE_GoodsOrigin, declaration.SupplierDocumentaryAddress, declaration.Supplier, false);
		}

		IEnumerable<IOrganisation> GetOrganisations(bool hasNonStandardCountry, string alternateCountry, FRJobDocAddress documentaryAddress, OrgHeader orgHeader, bool isImporter)
		{
			var result = new List<IOrganisation>();
			var alternateCountryCode = hasNonStandardCountry ? alternateCountry : String.Empty;

			if (documentaryAddress?.E2_AddressOverride ?? false)
			{
				result.Add(new OrganisationDocumentaryWrapper(entryHeader, documentaryAddress, orgHeader, alternateCountryCode));
			}
			else
			{
				var addressOverride = GetAddress(documentaryAddress?.Address, orgHeader);
				if (addressOverride != null)
				{
					result.Add(new OrganisationWrapper(entryHeader, addressOverride, addressOverride.Header, alternateCountryCode));
				}
			}

			var orgList = GetOrgList(isImporter);
			foreach (var orgAddress in orgList)
			{
				result.Add(new OrganisationWrapper(entryHeader, orgAddress, orgAddress.Header));
			}

			return result.DistinctBy(x => new
			{
				x.OrganisationNumber,
				x.OrganisationNumberEoriOnly,
				x.FullName,
				x.Address,
				x.CountryCode,
				x.PostCode,
				x.City
			});
		}

		List<OrgAddress> GetOrgList(bool lookForImporters)
		{
			var result = new List<OrgAddress>();

			if (declaration.IsDeltaC && ((lookForImporters && declaration.IsExport) || (!lookForImporters && declaration.IsImport)))
			{
				foreach (var invoice in declaration.Invoices)
				{
					var invoiceOrgOfInterest = lookForImporters ? GetAddress(invoice.BuyerAddress, invoice.Buyer) : GetAddress(invoice.SupplierAddress, invoice.Supplier);
					AddIfNotNull(result, invoiceOrgOfInterest);
				}
			}

			return result;
		}

		protected OrgAddress GetAddress(OrgAddress addressApplied, OrgHeader header)
			=> addressApplied
				?? header?.MainAddress;

		void AddIfNotNull(List<OrgAddress> list, OrgAddress element)
		{
			if (element != null)
			{
				list.Add(element);
			}
		}
		protected ZString GetEntryStyleCode()
		{
			return entryHeader?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
		}

		protected virtual ZString GetEstimatedAssessmentDate()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetEstimatedAssessmentHour()
		{
			return ZString.Empty;
		}

		protected ZString GetDeclEmergencyProcDate()
		{
			if (ShouldGetDeclEmergencyProcDateFromMessageDate)
			{
				return GetDeclEmergencyProcDateFromMessageDate();
			}
			else if (ShouldGetDeclEmergencyProcDateFromSpecialMentions)
			{
				return GetDeclEmergencyProcDateFromSpecialMentions();
			}
			return ZString.Empty;
		}

		protected virtual ZBool ShouldGetDeclEmergencyProcDateFromMessageDate => false;

		ZString GetDeclEmergencyProcDateFromMessageDate()
		{
			return messageDate.ToString("dd/MM/yyyy");
		}

		protected virtual ZBool ShouldGetDeclEmergencyProcDateFromSpecialMentions => false;

		ZString GetDeclEmergencyProcDateFromSpecialMentions()
		{
			ZString[] targetCodes = { "51000", "52000", "53000" };

			var targetInfo = entryHeader.Declaration.AdditionalInfos.Cast<AdditionalInfo>()
				.Where(info => targetCodes.Contains(info.CSI_Code))
				.OrderBy(info => info.CSI_Code)
				.ThenBy(info => info.CSI_DateOfIssue)
				.FirstOrDefault();

			if (targetInfo == null)
			{
				return ZString.Empty;
			}

			return targetInfo.CSI_DateOfIssue.ToString("dd/MM/yyyy");
		}

		CusEntryLine GetFirstMergedLine(CusEntryHeader cusEntryHeader)
		{
			return ((cusEntryHeader?.MergedLines?.Count ?? 0) > 0) ? cusEntryHeader.MergedLines[0] : null;
		}
		ZString GetTransactionNature()
		{
			return entryHeader.EntryInstruction?.ZG_TransNature ?? ZString.Empty;
		}

		ZString GetAgreementOwnerEORI()
		{
			var registrationNumber = entryHeader.Declaration.DeltaAccountOrgHeader?.GetEuIdentificationNumber() ?? ZString.Empty;

			return registrationNumber;
		}

		ZString GetCustomsAgreementNumber()
		{
			var agreementNumber = declaration.JE_CustomsProfile;

			return agreementNumber;
		}

		ZString GetProcedureType()
		{
			var result = ZString.Empty;
			var deltaMode = declaration.JE_DeltaMode;
			if (deltaMode == OrgCusAccountDeltaGTypeList.Codes.G1)
			{
				var orgImpAddInfo = FROrgImpAddInfo.Get(declaration.DeltaAccountOrgHeader);
				result = orgImpAddInfo.ZO_DeltaG1SubProcedure;
				switch (result)
				{
					case DeltaG1SubProcedureList.Codes.C:
						result = nameof(RefTypeProc.C);
						break;
					case DeltaG1SubProcedureList.Codes.D:
						result = nameof(RefTypeProc.D);
						break;
				}
			}
			return result;
		}

		ZString GetDeferalApprovalCreditNumber()
		{
			var creditNumber = ZString.Empty;

			if (declaration.JE_PaymentMethod == MethodOfPaymentList.Codes.R
				|| declaration.JE_PaymentMethod == MethodOfPaymentList.Codes.M)
			{
				creditNumber = declaration.JE_DefermentAccountNumber;
			}

			return creditNumber;
		}

		ZString GetGuaranteeMode()
		{
			return declaration.CustomsGuarantee?.GuaranteeModeCode ?? ZString.Empty;
		}

		ZString GetBranchCusBrokerageCode()
		{
			var brokerageNumber = ZString.Empty;
			if ((brokerageNumber = entryHeader?.DeclarantOrganisation.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.BrokerageRegistration) ?? ZString.Empty).IsEmpty)
			{
				brokerageNumber = entryHeader?.RepresentativeOrganisation.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.BrokerageRegistration) ?? ZString.Empty;
			}

			return brokerageNumber;
		}

		ZDecimal GetEntryGoodsPriceSum()
		{
			ZDecimal result = ZDecimal.Zero;

			bool isMultiCurrency = entryHeader.IsMultiInvoiceCurrency;

			foreach (CusEntryLine mergeLine in entryHeader.MergedLines)
			{
				if (isMultiCurrency)
				{
					result += mergeLine.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency;
				}
				else
				{
					result += mergeLine.CL_InvoiceAmount;
				}
			}

			return result;
		}
		ZString GetEntryGoodsPriceCurrency()
		{
			var result = FRConstants.FrenchCurrency.CurrencyCode;

			if (!entryHeader.IsMultiInvoiceCurrency)
			{
				if (entryHeader?.InvoiceHeaders.Length > 0)
				{
					result = entryHeader?.InvoiceHeaders?.FirstOrDefault().JZ_RX_NKInvoice_Currency ?? ZString.Empty;
				}
			}

			return result;
		}
		ZDecimal GetEntryGoodsPriceCurrencyRate() => entryHeader?.InvoiceHeaders.Length > 0 ? (entryHeader?.InvoiceHeaders?.FirstOrDefault().JZ_InvoiceCurrExRate ?? ZDecimal.Zero) : ZDecimal.Zero;

		#endregion

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly InvoiceHeaderActiveCollection invoiceHeaders;
		protected readonly ZString actionCode;
		protected readonly ZDateTime messageDate;
		readonly ErrorCollector itemErrorCollector;
		readonly TransportCostCalculator calculator;
	}
}
