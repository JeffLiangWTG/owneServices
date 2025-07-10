#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.Snapshot;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.Universal;
using SharedBusiness = Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ArticleWrapper : IArticle
	{
		public ArticleWrapper(CusEntryHeader cusEntryHeader, CusEntryLine cusEntryLine)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
		}

		public const int EntrylineDescriptionMaxLength = 260;

		#region Members
		public ZShort EntryNumber => entryLine.CL_LineNumber;

		public ZString ShippingIdReference => GetShippingIdReference();

		public IAlternateCalcValue AlternateCalcValue => new AlternateCalcValueWrapper(entryLine);

		public ZString TariffCode => entryLine.CL_AdValoremTariff;

		public ZString Observation => GetObservations();

		public IEnumerable<ITariffAdditionalCode> CETariffAdditionalCodes => GetCETariffAdditionalCodes();

		public IEnumerable<ITariffAdditionalCode> SecondMessageCETariffAdditionalCodes => ExcludeAlreadySentInFirstMessage(CETariffAdditionalCodes, (caco, c) => c.Code == caco.Code && c.Type == ChildType.CAC);

		public IEnumerable<ITariffAdditionalCode> FRTariffAdditionalCodes => GetFRTariffAdditionalCodes();

		public IEnumerable<ITariffAdditionalCode> SecondMessageFRTariffAdditionalCodes => ExcludeAlreadySentInFirstMessage(FRTariffAdditionalCodes, (cana, c) => c.Code == cana.Code && c.Type == ChildType.CAN);

		public IEnumerable<ITariffAdditionalCode> PartDispos => GetDispoPart();

		public IEnumerable<ITariffAdditionalCode> SecondMessagePartDispos => ExcludeAlreadySentInFirstMessage(PartDispos, (dtp, c) => c.Code == dtp.Code && c.Type == ChildType.DOC);

		public ZString EntryLineDescription => GetEntryLineDescription();

		public ZDecimal GrossWeight => GetGrossWeight();

		public ZDecimal CustomsQuantity => GetCustomsQuantity();

		public ZString CountryGoodsOrigineCode => entryLine.RandomLine?.JI_CountryOfOrigin ?? ZString.Empty;

		public ZString CountryGoodsSupplyCode => entryLine.RandomLine?.ZG_CountryOfSupply ?? ZString.Empty;

		public IEnumerable<ZString> QuotaRefNumber => GetQuotaRefNumber();

		public ZString ValuationMethod => entryLine.RandomLine?.JI_ValuationCode ?? ZString.Empty;

		public ISupplementaryUnit SuppUnit => new SupplementaryUnitWrapper(entryLine);

		public ISupplementaryUnit ThirdUnit => new ThirdUnitWrapper(entryLine);

		public ZString ProcedureCode => entryLine.RandomLine.CusProcedure?.ZZ6_ProcedureCode ?? ZString.Empty;

		public ZString PreviousCode => entryLine.RandomLine?.CusProcedure?.ZZ6_PreviousProcedureCode ?? ZString.Empty;

		public ZString Concession => entryLine.RandomLine.CusProcedure?.ZZ6_Concession ?? ZString.Empty;

		public ZBool IsPlacingGoodsUnderBW => entryHeader.EntryInstruction?.CEI_Style.Equals(DeltaGImportDeclarationTypeList.Codes.PlacingGoodsUnderBW) ?? false;

		public ZString WarehouseType => IsPlacingGoodsUnderBW ? entryLine.GetWarehouseType() : ZString.Empty;

		public ZString WarehouseReference => GetWarehouseCode();

		public ZString WarehouseCountryCode => WarehouseReference.IsEmpty ? ZString.Empty : (ZString)Core.Constants.CountryCodes.France; //TODO : Wait for SPJ to see if an MQ warehouse country should be MQ or FR

		public ZBool HasSpecificRegimeAuthorisation => entryHeader.EntryInstruction?.SpecificRegimeAuthorisation != null;

		public IEcoRegimeAuthorization EcoRegimeAuthorization => GetEcoRegimeAuthorization();

		public IEcoRegimeDatas EcoRegimeDatas
		{
			get
			{
				if (ecoRegimeDatas == null)
				{
					ecoRegimeDatas = HasSpecificRegimeAuthorisation ? new EcoRegimeDatasWrapper(entryLine) : null;
				}
				return ecoRegimeDatas;
			}
		}
		IEcoRegimeDatas ecoRegimeDatas;

		public IPreference Preference => new PreferenceWrapper(entryLine);

		public IEnumerable<ZString> Containers => GetContainers();

		public IPacking Packing => new PackingWrapper(entryLine);

		public ISupportingDocument PreviousDocument => GetPreviousDocument();

		public IEnumerable<ITariffAdditionalCode> SpecMens => GetSpecMens();

		public IEnumerable<ISupportingDocumentOnly> SupportingDocuments => GetSupportingDocumentList();

		public IEnumerable<ISupportingDocumentOnly> SecondMessageSupportingDocuments => ExcludeAlreadySentInFirstMessage(SupportingDocuments, (document, c) => c.Code == document.Code && c.Type == ChildType.DOC && c.Reference == document.RefNumber);

		public ZDecimal InvoiceLinePrice => entryLine.CL_InvoiceAmount;

		public ZString CurrencyCode => entryLine.CL_RX_NKInvoiceAmountCurrency;

		public ZDecimal ExchangeRate => entryLine.RandomLine?.InvoiceHeader?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;

		public ZDecimal CIFPrice => entryLine.CIF?.Amount ?? ZDecimal.Zero;

		public ZBool CIFApplicationFlag => false;

		public ZDecimal StatisticalAmount => entryLine.CL_StatisticalValue;

		public ZDecimal CustomsValue => entryLine.CL_CustomsValue;

		public ZDecimal TVAAssessedAmount => entryLine.CL_ValueForVAT;

		public ZBool ShouldSendCustomsStatisticAndVatValues => entryLine.Declaration.IsExport || !(entryLine.Header.EntryInstruction?.ZG_BypassCode.IsEmpty ?? true);

		public ZString DeliveryDepartment => GetDeliveryDepartment();

		public ZString ExpeditionDepartment => GetExpeditionDepartment();

		public ZDecimal ValuationAdjustPercent => entryLine.RandomLine?.JI_ValuationMarkup ?? ZDecimal.Zero;

		public IAmountAndCurrency PackingCosts => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge }, true, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldBeDutiable + AmountAndCurrencyWrapper.ShouldBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency Commission => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge }, true, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldBeDutiable + AmountAndCurrencyWrapper.ShouldBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency Fee => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge }, true, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldBeDutiable + AmountAndCurrencyWrapper.ShouldBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency Resale => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge }, true, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldBeDutiable + AmountAndCurrencyWrapper.ShouldBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency OthCosts => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.AdjustmentCharge }, false, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldNotBeDutiable + AmountAndCurrencyWrapper.ShouldNotBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency AssemblyCosts => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge }, false, AmountAndCurrencyWrapper.ShouldBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldNotBeDutiable + AmountAndCurrencyWrapper.ShouldNotBeStatable + AmountAndCurrencyWrapper.ShouldBeVatable);

		public IAmountAndCurrency CustomsCosts => AmountAndCurrencyWrapper.New(entryLine, new ZString[] { UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge }, true, AmountAndCurrencyWrapper.ShouldBeIncludedInInvoiceLine + AmountAndCurrencyWrapper.ShouldBeIncludedInInvoice + AmountAndCurrencyWrapper.ShouldNotBeDutiable + AmountAndCurrencyWrapper.ShouldNotBeStatable + AmountAndCurrencyWrapper.ShouldNotBeVatable);

		public IEnumerable<IPreCalcEntryLine> PreCalcEntryLines => GetPreCalcEntryLines();

		public IEnumerable<ITax> EntryHeaderCharges => GetEntryHeaderCharges();

		public ZString CusImportCertification => new ZString(null);

		public ZString CEQuotaCertification => new ZString(null);

		public ZDecimal SugarRate1 => 0;

		public ZDecimal SugarRate2 => 0;

		public ZDecimal SugarRate3 => 0;

		public ZDecimal SugarPolarisation => 0;

		public ZString PACInformations => new ZString(null);

		public IOrganisation Applicant => entryHeader.Declaration.Supplier != null ? new OrganisationWrapper(entryHeader, entryHeader.Declaration.SupplierDocumentaryAddress.Address, entryHeader.Declaration.Supplier) : null; //todo:skipped for now, check for another object

		public ZString ApplicantInwardNature => entryHeader.EntryInstruction?.SpecificRegimeNature ?? ZString.Empty;

		public ZString ApplicantDescription => entryHeader.EntryInstruction?.SpecificRegimeDescription ?? ZString.Empty;

		public ZString ApplicantConditions => entryHeader.EntryInstruction?.SpecificRegimeCondition ?? ZString.Empty;

		public ZString ApplicantPurOffice => entryHeader.EntryInstruction?.SpecificRegimeOffice ?? ZString.Empty;

		public ZString ApplicantInwardLocation => entryHeader.EntryInstruction?.SpecificRegimeLocation ?? ZString.Empty;

		public ZString ApplicantTransFormality => entryHeader.EntryInstruction?.SpecificRegimeProcedure ?? ZString.Empty;

		public ZString SpecificInfos => entryHeader.EntryInstruction?.SpecificRegimeInformation ?? ZString.Empty;

		#region Export

		public sbyte Seals => (sbyte)GetSealIds().Count();

		public IEnumerable<ZString> SealIds => GetSealIds();

		public ZString TransportMethodPayment => entryLine.RandomLine?.InvoiceHeader?.ZG_TransportChargesMethodOfPayment ?? new ZString(null);

		public IEnumerable<ZString> DangerousGoodsDeltas => GetDangerousGoodsDeltas();

		public ZString PACCode => ZString.Empty;

		public ZDecimal Restitution => ZDecimal.Zero;

		public ZString ExportCertification1 => ZString.Empty;

		public ZString ExportCertification2 => ZString.Empty;

		public ZString RestitutionMention => ZString.Empty;

		public ZString Recipient => ZString.Empty;

		public ZString InvariantMention => ZString.Empty;

		public ZString VariantMention => ZString.Empty;

		public ZDecimal VariantRate => ZDecimal.Zero;

		public ZDecimal LoadingStartDate => ZDecimal.Zero;

		public ZDecimal LoadingStartHour => ZDecimal.Zero;

		public ZDecimal LoadingEndDate => ZDecimal.Zero;

		public ZDecimal LoadingEndHour => ZDecimal.Zero;

		#endregion

		#endregion

		#region Methods

		ZString GetShippingIdReference()
		{
			var query = new CargoWise.EntityFramework.ZQuery(ZArchitecture.Schema.CusEntryNumSchema.CE_ParentID, entryHeader.Declaration.PK);
			query.AddToFilter(ZArchitecture.Schema.CusEntryNumSchema.CE_EntryType, FranceAdditionalReferenceNumberTypes.Codes.CommonAccessReference);
			query.AddToFilter(ZArchitecture.Schema.CusEntryNumSchema.CE_RN_NKCountryCode, entryHeader.CountryCode);
			return entryLine.Factory.LoadTop1<CusEntryNumber>(query)?.CE_EntryNum ?? ZString.Empty;
		}

		IEcoRegimeAuthorization GetEcoRegimeAuthorization()
		{
			return HasSpecificRegimeAuthorisation ? new EcoRegimeAuthorizationWrapper(entryHeader) : null;
		}

		IEnumerable<ZString> GetDangerousGoodsDeltas()
		{
			List<ZString> result = new List<ZString>();

			var dangerousGoods = entryLine.RandomLine.UNDGs;

			if (dangerousGoods != null)
			{
				foreach (var cusContainer in dangerousGoods)
				{
					result.Add(cusContainer.UNDGSubstance?.DG_UNNO ?? ZString.Empty);
				}
			}

			return result;
		}

		IEnumerable<ZString> GetSealIds()
		{
			List<ZString> result = new List<ZString>();

			foreach (SharedBusiness.BaseCusContainer cusContainer in entryHeader.Declaration.CusContainers.Where(c => c.PK == entryLine.RandomLine?.JI_CO))
			{
				result.Add(cusContainer.CO_Seal);
			}

			return result;
		}

		ZString GetWarehouseCode()
		{
			var result = ZString.Empty;

			if (entryLine.HasIntoRegimeProcedure || entryLine.HasOutOfRegimeProcedure)
			{
				result = entryHeader.EntryInstruction?.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault()?.AGC_AuthorizationShortCode ?? ZString.Empty;
			}

			return result;
		}

		ZString GetDeliveryDepartment()
		{
			var department = ZString.Empty;
			var address = entryHeader.Declaration?.ImporterDocumentaryAddress?.Address;
			ErrorCollectorHelper.DeliveryDepartmentCantBeDeterminate(address, ref department);

			return department;
		}

		ZString GetExpeditionDepartment()
		{
			var department = ZString.Empty;
			var declaration = entryHeader.Declaration;
			var origin = declaration.Origin;

			if (origin != null && origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.France)
			{
				department = GetCustomsFriendlyDepartment(origin.CountryStates?.RW_Code ?? ZString.Empty);
			}
			else
			{
				var postCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(declaration.Factory, declaration.JE_CustomsOffice, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PostCode) ?? ZString.Empty;
				if (postCode.Length > 2)
				{
					department = postCode.Left(2);
				}
			}
			return department;
		}

		ZString GetCustomsFriendlyDepartment(ZString department)
		{
			var departmentMapper = new Dictionary<string, string>()
			{
				{ "75C", "75" },
				{ "69M", "69" },
				{ "6AE", "67" },
			};
			return departmentMapper.ContainsKey(department) ? departmentMapper[department] : department;
		}

		ZString GetObservations()
		{
			ZString observations = ZString.Empty;//todo:skipped for now

			return observations;
		}
		IEnumerable<ITariffAdditionalCode> GetCETariffAdditionalCodes()
		{
			List<ITariffAdditionalCode> lstEuropeanAdditionalCode = new List<ITariffAdditionalCode>();

			var ceAdditionalCodes = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CEAdditionalCodes);
			foreach (var frAdditionalCode in ceAdditionalCodes)
			{
				var wrapper = new TariffAdditionalCodeWrapper(frAdditionalCode);
				lstEuropeanAdditionalCode.Add(wrapper);
			}

			return lstEuropeanAdditionalCode.DistinctBy(x => x.Code);
		}

		IEnumerable<ITariffAdditionalCode> GetFRTariffAdditionalCodes()
		{
			List<ITariffAdditionalCode> lstFrenchAdditionalCode = new List<ITariffAdditionalCode>();

			var frAdditionalCodes = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.FRAdditionalCodes);
			foreach (var frAdditionalCode in frAdditionalCodes)
			{
				var wrapper = new TariffAdditionalCodeWrapper(frAdditionalCode);
				lstFrenchAdditionalCode.Add(wrapper);
			}

			return lstFrenchAdditionalCode.DistinctBy(x => x.Code);
		}

		IEnumerable<ITariffAdditionalCode> GetDispoPart()
		{
			List<ITariffAdditionalCode> lstDTP = new List<ITariffAdditionalCode>();

			foreach (var dtp in this.entryHeader.SupportingDocumentsDTP)
			{
				lstDTP.Add(new TariffAdditionalCodeWrapper(dtp));
			}

			foreach (var dtp in this.entryLine.SupportingDocumentsDTP)
			{
				lstDTP.Add(new TariffAdditionalCodeWrapper(dtp));
			}

			return lstDTP;
		}

		ZString GetEntryLineDescription()
		{
			ZString entryLineDescription = entryLine.CL_Description;

			if (entryLineDescription.Length > EntrylineDescriptionMaxLength)
			{
				entryLineDescription = entryLineDescription.Left(EntrylineDescriptionMaxLength);
			}
			return entryLineDescription;
		}

		IEnumerable<ITariffAdditionalCode> GetSpecMens()
		{
			List<ITariffAdditionalCode> additionalInfoDocList = new List<ITariffAdditionalCode>();

			foreach (AdditionalInfo additionalInfoDoc in entryHeader.AdditionalInfos)
			{
				additionalInfoDocList.Add(new TariffAdditionalCodeWrapper(additionalInfoDoc));
			}

			foreach (AdditionalInfo additionalInfoDoc in entryLine.AdditionalInfos)
			{
				additionalInfoDocList.Add(new TariffAdditionalCodeWrapper(additionalInfoDoc));
			}

			return additionalInfoDocList;
		}

		ZDecimal GetGrossWeight()
		{
			return entryLine.EffectiveGrossWeight.InKilograms;
		}
		ZDecimal GetCustomsQuantity()
		{
			return entryLine.CustomsQuantity;
		}
		IEnumerable<ZString> GetQuotaRefNumber()
		{
			var quotaRefNumber = new List<ZString>();

			var quota = entryLine.RandomLine?.JI_ConcessionOrder ?? ZString.Empty;
			if (quota != ZString.Empty)
			{
				quotaRefNumber.Add(quota);
			}

			return quotaRefNumber;
		}

		IEnumerable<ZString> GetContainers()
		{
			List<ZString> containerList = new List<ZString>();

			if (entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly.Count > 0) //todo: To check in the future
			{
				foreach (SharedBusiness.NonPersistentCusContainer lineContainer in entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly)
				{
					if (lineContainer.IsForInvoiceLine)
					{
						containerList.Add(new ZString(lineContainer.ContainerNumber));
					}
				}
			}

			return containerList;
		}
		ISupportingDocument GetPreviousDocument()
		{
			var previousDoc = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>())?.FirstOrDefault() ??
								 entryLine.Header?.InvoiceHeaders.Cast<JobComInvoiceHeader>().SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()).FirstOrDefault() ??
									entryLine.Declaration?.PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault();
			return previousDoc == null ? null : new DocumentWrapper(previousDoc);
		}

		IEnumerable<ISupportingDocumentOnly> GetSupportingDocumentList()
		{
			var supportingDocList = new List<ISupportingDocumentOnly>();

			foreach (SupportingDocument supDoc in entryLine.SupportingDocumentsToCustoms)
			{
				supportingDocList.Add(new SupportingDocumentWrapper(supDoc, entryLine));
			}

			var authorisationSupportingDocument = GetAuthorisationSupportingDocument();
			if (authorisationSupportingDocument != null)
			{
				if (!supportingDocList.Any(x => x.Code == authorisationSupportingDocument.Code && x.RefNumber == authorisationSupportingDocument.RefNumber && x.DateIssue == authorisationSupportingDocument.DateIssue))
				{
					supportingDocList.Add(authorisationSupportingDocument);
				}
			}

			return supportingDocList;
		}

		ISupportingDocumentOnly GetAuthorisationSupportingDocument()
		{
			ISupportingDocumentOnly result = null;
			var instruction = entryHeader.EntryInstruction;
			var authorisation = instruction?.SpecificRegimeAuthorisation;

			if (instruction != null && authorisation != null && !authorisation.CPH_IsAdHoc)
			{
				var code = GetDocumentCodeFromUsageCode(authorisation.CPH_Type);
				if (!string.IsNullOrEmpty(code))
				{
					result = new SupportingDocumentWrapperWithoutDocument(code, authorisation.CPH_Number, authorisation.CPH_StartDate);
				}
			}
			return result;
		}

		ZString GetDocumentCodeFromUsageCode(ZString usageCode) => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(entryHeader.Factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.AUTDC, usageCode, ZDateTime.Today);

		IEnumerable<T> ExcludeAlreadySentInFirstMessage<T>(IEnumerable<T> allRecords, Func<T, FrenchEntryLineChildSnapshotCusEntryLineChildData, bool> firstPredicate)
		{
			var filteredRecords = new List<T>();
			var snapshottedEntryLines = EntrySnapshot;
			var snapshottedEntryLine = snapshottedEntryLines?.CusEntryLine.FirstOrDefault(x => x.LineNumber == entryLine.CL_LineNumber);
			foreach (var record in allRecords)
			{
				var isAlreadySentInFirstMessage = snapshottedEntryLine?.ChildData.Any(x => firstPredicate(record, x)) ?? false;
				if (!isAlreadySentInFirstMessage)
				{
					filteredRecords.Add(record);
				}
			}
			return filteredRecords;
		}

		internal FrenchEntryLineChildSnapshot EntrySnapshot
		{
			get
			{
				if (entrySnapshot == null)
				{
					var lodgedSnapshot = entryHeader.Snapshots.Cast<SharedBusiness.CusEntrySnapshot>().FirstOrDefault(x => x.CES_Status == Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged && DeltaGMessageSender.ApplicableEntryActionCodeForSnapshot.Contains(x.CES_MessageType));
					entrySnapshot = lodgedSnapshot != null ? Extensions.Deserialize<FrenchEntryLineChildSnapshot>(lodgedSnapshot.CES_SnapshotXml) : null;
				}

				return entrySnapshot;
			}
		}
		FrenchEntryLineChildSnapshot entrySnapshot;

		IEnumerable<IPreCalcEntryLine> GetPreCalcEntryLines()
		{
			var preCalcEntryLineList = new List<IPreCalcEntryLine>();

			if (entryLine.RandomLine != null && !entryLine.RandomLine.JI_TariffBypassCode.IsEmpty)
			{
				foreach (var fee in entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Additional || x.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule || x.CF_RateOverrideReasonCode.IsEmpty))
				{
					var wrapper = new PreCalcEntryLineWrapper(fee);
					preCalcEntryLineList.Add(wrapper);
				}
			}
			else
			{
				foreach (var fee in entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Additional || x.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule))
				{
					var wrapper = new PreCalcEntryLineWrapper(fee);
					preCalcEntryLineList.Add(wrapper);
				}
			}

			return preCalcEntryLineList;
		}

		IEnumerable<ITax> GetEntryHeaderCharges()
		{
			var entryHeaderChargeList = new List<ITax>();

			foreach (CusEntryHeaderCharges entryHeaderCharge in entryHeader.Charges)
			{
				var wrapper = new EntryHeaderChargeWrapper(entryHeaderCharge);
				entryHeaderChargeList.Add(wrapper);
			}

			return entryHeaderChargeList;
		}

		#endregion

		readonly CusEntryLine entryLine;
		readonly CusEntryHeader entryHeader;
	}
}
