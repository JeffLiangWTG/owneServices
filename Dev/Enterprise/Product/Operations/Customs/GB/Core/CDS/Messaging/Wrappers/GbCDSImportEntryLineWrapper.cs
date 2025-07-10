using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using static System.FormattableString;
using static Enterprise.Customs.GB.Business.GBCommonConstants;
using RefCusProcedure = Enterprise.Customs.Universal.RefCusProcedure;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSImportEntryLineWrapper : IGovernmentAgencyGoodsItem, ICommodity
	{
		readonly CusEntryLine entryLine;
		readonly JobDeclaration declaration;
		readonly GbCDSImportEntryHeaderWrapper cdsImportEntryHeaderWrapper;
		readonly GbCDSImportLine cdsImportLine;
		readonly JobComInvoiceLine randomLine;

		public GbCDSImportEntryLineWrapper(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
			var header = entryLine.Header;
			cdsImportEntryHeaderWrapper = new GbCDSImportEntryHeaderWrapper(header);
			cdsImportLine = new GbCDSImportLine(cdsImportEntryHeaderWrapper.ImportHeaderWrapper, entryLine);
			declaration = entryLine.Declaration;
			randomLine = entryLine.RandomLine;
		}
		const string gbp = "GBP";

		protected GbCDSImportLine ImportLine => cdsImportLine;

		#region IGovernmentAgencyGoodsItem

		IEnumerable<IPreviousDocument> IGovernmentAgencyGoodsItem.PreviousDocuments
		{
			get => entryLine.PreviousDocuments.Select(PreviousDocumentWrapper.New);
		}

		IEnumerable<IStatement> IGovernmentAgencyGoodsItem.AdditionalInformations => ((ILine)cdsImportLine).Statements;

		IEnumerable<IAdditionalDocument> IGovernmentAgencyGoodsItem.AdditionalDocuments
		{
			get
			{
				var headerSupportingDocuments = entryLine.Header?.SupportingDocuments.Cast<SupportingDocument>() ?? Enumerable.Empty<SupportingDocument>();
				var lineSupportingDocuments = entryLine.SupportingDocuments.Cast<SupportingDocument>();
				var supportingDocuments = headerSupportingDocuments.Union(lineSupportingDocuments);

				return supportingDocuments.Select(AdditionalDocumentWrapper.New).Distinct();
			}
		}

		IEnumerable<IGovernmentProcedure> IGovernmentAgencyGoodsItem.GovernmentProcedures
		{
			get
			{
				var zzz = declaration.GetDefaultDataGroupingCode();
				var when = declaration.DateOfValuation;
				var loader = new RefCusProcedure.Loader(entryLine.Factory);
				var refCusProcedure = loader.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(entryLine.ProcedureCode, zzz, when);
				if (refCusProcedure != null)
				{
					yield return GovernmentProcedureWrapper.New(refCusProcedure.ZZ6_ProcedureCode, refCusProcedure.ZZ6_PreviousProcedureCode);
					yield return GovernmentProcedureWrapper.New(refCusProcedure.ZZ6_Concession, string.Empty);
				}

				foreach (AdditionalProcedureCode additionalCpc in randomLine.AdditionalProcedureCodes)
				{
					var additionalRefCusProcedure = loader.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(additionalCpc.CY_Code, zzz, when);
					if (additionalRefCusProcedure != null)
					{
						yield return GovernmentProcedureWrapper.New(additionalRefCusProcedure.ZZ6_Concession, null);
					}
				}
			}
		}

		//3/1
		//3/2
		IOrganisation IGovernmentAgencyGoodsItem.Consignor
		{
			get
			{
				var consignor = cdsImportLine.actualEntryLine.Consignor;
				var consignorId = consignor?.Header.GetEuIdentificationNumber() ?? ZString.Empty;
				var hasMultipleExportersViaLines = entryLine.Header?.HasMultipleExportersViaLines() ?? ZBool.False;
				IOrganisation organisation;
				if (hasMultipleExportersViaLines)
				{
					organisation = OrganisationWrapper.New(ZString.Empty, consignorId);
					if (organisation != null && organisation.IsForeignEori && !declaration.IsSendForeignEoriToCds)
					{
						organisation = !CDSExtensions.IsUnmatchedOrgAddress(cdsImportLine.actualEntryLine.Consignor) ? OrganisationWrapper.New(cdsImportLine.actualEntryLine.Consignor, ZString.Empty) : null;
					}
					else
					{
						organisation = !CDSExtensions.IsUnmatchedOrgAddress(cdsImportLine.actualEntryLine.Consignor) ? OrganisationWrapper.New(cdsImportLine.actualEntryLine.Consignor, consignorId) : null;
					}
				}
				else
				{
					organisation = null;
				}

				return organisation;
			}
		}

		//3/24
		//3/25
		IOrganisation IGovernmentAgencyGoodsItem.Seller
		{
			get
			{
				IOrganisation organisation = null;
				var countSellersViaInvoiceHeaders = entryLine.Header?.CountSellersViaInvoiceHeaders() ?? ZInt.Zero;

				if (countSellersViaInvoiceHeaders > 1)
				{
					organisation = GetOrganisationWrapperFromOrgAddress(randomLine.InvoiceHeader.SellerAddress);
				}

				return organisation;
			}
		}

		//3/26
		//3/27
		IOrganisation IGovernmentAgencyGoodsItem.Buyer
		{
			get
			{
				IOrganisation organisation = null;
				var countBuyersViaInvoiceHeaders = entryLine.Header?.CountBuyersViaInvoiceHeaders() ?? ZInt.Zero;

				if (countBuyersViaInvoiceHeaders > 1)
				{
					organisation = GetOrganisationWrapperFromOrgAddress(randomLine.InvoiceHeader.BuyerAddress);
				}
				return organisation;
			}
		}

		IOrganisation GetOrganisationWrapperFromOrgAddress(OrgAddress address)
		{
			IOrganisation organisation;
			var org = address?.Header;
			var id = org?.GetEuIdentificationNumber() ?? ZString.Empty;

			organisation = !CDSExtensions.IsUnmatchedOrgAddress(address) ? OrganisationWrapper.New(address, id, EoriNameAndAddressOrBoth.EORI) : null;

			if (organisation != null && organisation.IsForeignEori && !declaration.IsSendForeignEoriToCds)
			{
				organisation = OrganisationWrapper.New(address, ZString.Empty);
			}

			return organisation;
		}

		IEnumerable<IParty> IGovernmentAgencyGoodsItem.AEOMutualRecognitionParties => null;

		IEnumerable<IParty> IGovernmentAgencyGoodsItem.DomesticDutyTaxParties
		{
			get
			{
				foreach (var fr in entryLine.FiscalReferences)
				{
					yield return PartyWrapper.New(fr.CFR_Reference, fr.CFR_Code);
				}
			}
		}

		ICommodity IGovernmentAgencyGoodsItem.Commodity => this;

		ZString IGovernmentAgencyGoodsItem.ValuationAdjustmentAdditionCode
		{
			get
			{
				if (randomLine.JI_ValuationCode == "1")
				{
					var invoiceHeader = randomLine.InvoiceHeader;
					var flag1 = (invoiceHeader?.RelatedIndicator ?? ZBool.False) || randomLine.RelatedIndicator;
					var flag2 = (invoiceHeader?.RelatedIndicator2 ?? ZBool.False) || randomLine.RelatedIndicator2;
					var flag3 = (invoiceHeader?.RelatedIndicator3 ?? ZBool.False) || randomLine.RelatedIndicator3;
					var flag4 = (invoiceHeader?.RelatedIndicator4 ?? ZBool.False) || randomLine.RelatedIndicator4;
					return BoolToBit(flag1) + BoolToBit(flag2) + BoolToBit(flag3) + BoolToBit(flag4);
				}

				return ZString.Empty;
			}
		}

		static ZString BoolToBit(ZBool flag) => flag ? "1" : "0";

		ZString IGovernmentAgencyGoodsItem.DestinationCountryCode => randomLine.ZG_CountryOfDestination;

		IEnumerable<IPackaging> IGovernmentAgencyGoodsItem.Packagings => ((ILine)cdsImportLine).Packages.Select(PackagingWrapper.New);

		ZString IGovernmentAgencyGoodsItem.TransactionNatureCode => ((IGoodsShipment)cdsImportEntryHeaderWrapper).TransactionNatureCode;

		IAmountAndCurrency IGovernmentAgencyGoodsItem.StatisticalValue => AmountAndCurrencyWrapper.New(((ILine)cdsImportLine).StatisticalValue, gbp);

		ICustomsValuation IGovernmentAgencyGoodsItem.CustomsValuation
		{
			get
			{
				var chargeDeductions = entryLine.CDSChargeDeductions.Select(x =>
					ChargeDeductionWrapper.New(
						AmountAndCurrencyWrapper.New(x.Value.Amount, x.Value.Currency?.Code ?? ZString.Empty),
						x.Key.Left(2)));

				return CustomsValuationWrapper.New(
					chargeDeductions
					, entryLine.ValuationMethod);
			}
		}

		IEnumerable<ICountry> IGovernmentAgencyGoodsItem.Origins => GetOrigins();

		protected virtual IEnumerable<ICountry> GetOrigins()
		{
			var countryOfOrigin = cdsImportLine.CountryOfOriginCode;

			if (cdsImportLine.PreferenceCode.StartsWith("1", System.StringComparison.OrdinalIgnoreCase))
			{
				if (!countryOfOrigin.IsEmpty)
				{
					yield return CountryWrapper.New(countryOfOrigin, Constants.OriginTypeCodes.NonPreferential);
				}
			}
			else
			{
				if (!countryOfOrigin.IsEmpty)
				{
					yield return CountryWrapper.New(countryOfOrigin, Constants.OriginTypeCodes.Preferential);
				}

				var countryOfSupply = cdsImportLine.CountryOfSupply;
				if (!countryOfSupply.IsEmpty)
				{
					yield return CountryWrapper.New(countryOfSupply, Constants.OriginTypeCodes.NonPreferential);
				}
			}
		}

		ZString IGovernmentAgencyGoodsItem.TransportChargesMethodOfPayment => entryLine.TransportChargesMethodOfPayment;

		ZString IGovernmentAgencyGoodsItem.ExportCountryCode => randomLine.JI_RN_NKCountryOfExport;

		#endregion

		#region ICommodity

		ZString ICommodity.Description => ((ILine)cdsImportLine).DescriptionOfGoods.StripNewlineCharacters(DescriptionMaxLength);

		protected virtual ZShort DescriptionMaxLength => 512;

		IEnumerable<IClassification> ICommodity.Classifications => GetClassifications();

		protected virtual IEnumerable<IClassification> GetClassifications()
		{
			foreach (var tax in ((ILine)cdsImportLine).Taxes.Where(x => x.TaxType.SubstringSafe(0, 1).IsNumbersOnlyOrEmpty))
			{
				yield return ClassificationWrapper.New(Invariant($"X{tax.TaxType}"),
					Constants.Classification.IdentificationTypeCodes.GN);
			}

			var commodityAndTaric = randomLine.JI_FormattedTariff.KeepAlphanumericCharacters();
			if (!commodityAndTaric.IsEmpty)
			{
				var tsp = commodityAndTaric.SubstringSafe(0, 8);
				if (!tsp.IsEmpty)
				{
					yield return ClassificationWrapper.New(tsp, Constants.Classification.IdentificationTypeCodes.TSP);
				}

				var trc = commodityAndTaric.SubstringSafe(8, 2);
				if (!trc.IsEmpty)
				{
					yield return ClassificationWrapper.New(trc, Constants.Classification.IdentificationTypeCodes.TRC);
				}
			}

			var vat = randomLine.JI_ZZF_NKTaxType;
			switch (vat)
			{
				case GBUniversalReferenceConstants.TaxOrFeeTypeCode.ReducedRate:
					yield return ClassificationWrapper.New(Constants.Classification.IDs.ReducedRate,
						Constants.Classification.IdentificationTypeCodes.GN);
					break;
				case GBUniversalReferenceConstants.TaxOrFeeTypeCode.VATExempt:
					yield return ClassificationWrapper.New(Constants.Classification.IDs.VATExempt,
					Constants.Classification.IdentificationTypeCodes.GN);
					break;
				case GBUniversalReferenceConstants.TaxOrFeeTypeCode.ZeroRated:
					yield return ClassificationWrapper.New(Constants.Classification.IDs.ZeroRated,
						Constants.Classification.IdentificationTypeCodes.GN);
					break;
			}

			var supplementaryCodes = GetSupplementaryCodes();
			foreach (var supplementaryCode in supplementaryCodes)
			{
				yield return supplementaryCode;
			}
		}

		public IEnumerable<IClassification> GetSupplementaryCodes()
		{
			var supplementaryCodes = new List<IClassification>();
			var supplementaryCode1 = ((ILine)cdsImportLine).SupplementaryCode1.Left(4).PadRight(4);
			if (!supplementaryCode1.IsEmpty)
			{
				supplementaryCodes.Add(ClassificationWrapper.New(supplementaryCode1,
					Constants.Classification.IdentificationTypeCodes.TRA));
			}

			var supplementaryCode2 = ((ILine)cdsImportLine).SupplementaryCode2.Left(4).PadRight(4);
			if (!supplementaryCode2.IsEmpty)
			{
				supplementaryCodes.Add(ClassificationWrapper.New(supplementaryCode2,
					Constants.Classification.IdentificationTypeCodes.TRA));
			}
			return supplementaryCodes;
		}

		ZString ICommodity.UNDGID => ((ILine)cdsImportLine).UNDGCode;

		IEnumerable<IDutyTaxFee> ICommodity.DutyTaxFees
		{
			get
			{
				var allTaxes = ((ILine)cdsImportLine).Taxes.Cast<Tax>();
				if (GBCustomsDataRegistry.Instance.CDSSuppressSendingASVXFor3XXTaxTypes.Value)
				{
					allTaxes = allTaxes.Where(tax => !tax.TaxType.StartsWith("3") || tax.MethodOfCalculation != "ASVX");
				}

				var entryLinePreference = cdsImportLine.actualEntryLine.RandomLine.JI_PrimaryPreference;

				if (allTaxes.Any(t => t.TaxOverrideCode == TaxOverrideReasonCodes.Override))
				{
					var totalDues = AmountAndCurrencyWrapper.New(allTaxes.Sum(t => t.TaxAmountValue), gbp);
					int i = 0;
					foreach (var tax in allTaxes)
					{
						var payment = PaymentWrapper.New(tax.MethodOfPayment,
															AmountAndCurrencyWrapper.New(tax.TaxAmountValue, gbp),
															totalDues);

						var dutyTaxFee = DutyTaxFeeWrapper.New(tax, payment, currency: gbp, dutyRegimeCode: i == 0 ? entryLinePreference : ZString.Empty, outputTypeCode: true);

						i++;
						if (i == 1)
						{
							totalDues = null;
						}
						yield return dutyTaxFee;
					}
				}
				else
				{
					var filteredTaxes = allTaxes.Where(x => x.TaxType != UniversalReferenceConstants.RefCusRateCodes.Vat);

					var preferences = filteredTaxes.Select(x => x.MethodOfCalculation).Where(x => x != Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage).ToList();
					var orderedTaxes = filteredTaxes.OrderBy(item => preferences.IndexOf(item.MethodOfCalculation));

					var entryLineQuota = cdsImportLine.QuotaOrderNumber.Left(6).PadRight(6);
					var methodOfPayment = cdsImportLine.actualEntryLine.RandomLine.ZG_MethodOfPayment;

					if (!orderedTaxes.Any())
					{
						yield return DutyTaxFeeWrapper.New(new TaxWrapper(methodOfPayment: methodOfPayment), quotaOrderID: entryLineQuota, dutyRegimeCode: entryLinePreference);
					}

					int i = 0;
					foreach (var tax in orderedTaxes)
					{
						if (tax.MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage)
						{
							var payment = PaymentWrapper.New(tax.MethodOfPayment
												, AmountAndCurrencyWrapper.New(ZDecimal.Zero, gbp)
												, AmountAndCurrencyWrapper.New(ZDecimal.Zero, gbp));
							yield return DutyTaxFeeWrapper.New(new TaxWrapper(methodOfPayment: methodOfPayment), payment: payment, quotaOrderID: entryLineQuota, dutyRegimeCode: i == 0 ? entryLinePreference : ZString.Empty);
						}
						else
						{
							var payment = PaymentWrapper.New(i == 0 ? tax.MethodOfPayment : ZString.Empty
												, AmountAndCurrencyWrapper.New(ZDecimal.Zero, gbp)
												, AmountAndCurrencyWrapper.New(ZDecimal.Zero, gbp));

							if (i == 0)
							{
								yield return DutyTaxFeeWrapper.New(tax, payment, dutyRegimeCode: entryLinePreference, quotaOrderID: entryLineQuota);
							}
							else
							{
								yield return DutyTaxFeeWrapper.New(tax, payment);
							}
						}

						i++;
					}
				}
			}
		}

		IAmountAndCurrency ICommodity.InvoiceLineItemCharge
		{
			get => AmountAndCurrencyWrapper.New(cdsImportLine.ItemPrice, cdsImportLine.ItemPriceCurrency);
		}

		ZDecimal ICommodity.NetWeight => ((ILine)cdsImportLine).NetMassInKilograms.Normalize();

		ZDecimal ICommodity.GrossWeight => ((ILine)cdsImportLine).GrossMassInKilograms.Normalize();

		ZDecimal ICommodity.TariffQuantity => ((ILine)cdsImportLine).SupplementaryUnits.Normalize();

		IEnumerable<ZString> ICommodity.TransportEquipmentIDs
		{
			get => ((ILine)cdsImportLine).Containers.Select(container => container.ContainerNumber.StripIllegalCharactersT1().Left(17));
		}

		bool ICommodity.NoAdditionalProcedureCodesAreE01orE02 => JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(randomLine);
		#endregion
	}
}
