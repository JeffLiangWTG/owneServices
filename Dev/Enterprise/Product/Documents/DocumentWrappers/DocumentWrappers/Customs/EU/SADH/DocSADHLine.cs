using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Biz = Enterprise.Customs.Business;
using EUDocumentWrapperConstants = Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class DocSADHLine : DocBaseWrapper
	{
		public static DocSADHLine New(CusEntryLine entryLine, BusinessObjectFactory factory)
		{
			return entryLine == null ? null : new DocSADHLine(entryLine, factory);
		}

		protected DocSADHLine(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			EntryHeader = Argument.NotNull(entryLine.Header, nameof(entryLine.Header));
			Declaration = Argument.NotNull(EntryHeader.Declaration, nameof(EntryHeader.Declaration));
			EntryInstruction = EntryHeader.EntryInstruction;
		}

		protected CusEntryLine EntryLine { get; }
		protected CusEntryHeader EntryHeader { get; }
		protected JobDeclaration Declaration { get; }
		protected CusEntryInstruction EntryInstruction { get; }

		public ZString Box1bSubStyle => EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

		public ZString Box15aExportCountry => EntryLine.CountryOfExport.IfEmptyUse(() => Declaration.JE_GoodsOrigin);

		public ZString Box17aDestinationCountry => EntryLine.CountryOfDestination.IfEmptyUse(() => Declaration.JE_GoodsDestination);

		public ZString Box18IdentityOfTransportAtDepartureLine => Box18IdentityOfTransportAtDepartureLineCore;

		protected virtual ZString Box18IdentityOfTransportAtDepartureLineCore => Declaration.ZG_Box18TransportID;

		public ZString Box2Consignor
		{
			get
			{
				return EntryLine.Consignor?.Header?.OH_FullName ?? ZString.Empty;
			}
		}

		public ZString Box8Consignee
		{
			get
			{
				return EntryLine.Consignee?.Header?.OH_FullName ?? ZString.Empty;
			}
		}

		public ZString Box2ConsignorForEAD
		{
			get
			{
				var sb = new ZStringBuilder();
				var consignor = EntryLine.Consignor?.Header;
				if (consignor != null)
				{
					sb.AppendLine(consignor.OH_FullName);
					sb.AppendLine(consignor.MainAddress.AddressDescription);
					sb.Append(consignor.GetEuIdentificationNumber());
				}

				return sb.ToString();
			}
		}

		public ZString Box8ConsigneeForEAD
		{
			get
			{
				var sb = new ZStringBuilder();
				var consignee = EntryLine.Consignee?.Header;
				if (consignee != null)
				{
					sb.AppendLine(consignee.OH_FullName);
					sb.AppendLine(consignee.MainAddress.AddressDescription);
					sb.Append(consignee.GetEuIdentificationNumber());
				}

				return sb.ToString();
			}
		}

		public ZString BoxS28SealsLine
		{
			get
			{
				var seals = EntryLine
					.ContainersForInvoiceLines
					.Cast<Biz.NonPersistentCusContainer>()
					.Where(c => c.IsForInvoiceLine)
					.SelectMany(c => GetSeals(c))
					.Where(x => !x.IsEmpty)
					.Distinct()
					.OrderBy(x => x)
					.ToArray();

				return ZString.Join(EUDocumentWrapperConstants.Delimiters.CommaAndSpace, seals);
			}
		}

		IEnumerable<ZString> GetSeals(Biz.NonPersistentCusContainer container)
		{
			yield return container.Seal;
			yield return container?.Container.CO_SecondSeal ?? ZString.Empty;
		}

		public ZString BoxS29TransportChargesMoP
		{
			get
			{
				ZString mop = EntryHeader.TransportChargesMoP;
				return mop.IsEmpty ? (ZString)DocSADH.EadBlankBoxDashes : mop;
				// Gems supports only header-level MOPs at the moment. Thus so do we. 
			}
		}

		//PM PC PN
		public ZString Box31_1NumberOfPackagesPiecesMarksAndNumbers => Box31_1NumberOfPackagesPiecesMarksAndNumbersCore;

		public ZString Box31_2DescriptionOfGoods
		{
			get
			{
				return EntryLine.CL_Description.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Left(280);
			}
		}

		public ZString Box31_3ContainerNumbers => Box31_3ContainerNumbersCore;

		protected virtual ZString Box31_3ContainerNumbersCore => string.Join(EUDocumentWrapperConstants.Delimiters.CommaAndSpace, ContainerList);

		protected virtual ZString Box31_1NumberOfPackagesPiecesMarksAndNumbersCore => new SADHEntryLinePacksMarksAndNumbersBuilder(EntryLine).Build();

		protected IEnumerable<ZString> ContainerList => containerList ?? (containerList = EntryLine.Containers.ToArray());
		ZString[] containerList;

		// Overall MIsh mash for SAD and ESS - for EAD we use discrete fields
		public virtual ZString Box31PackagesAndDescriptionOfGoods
		{
			get
			{
				ZStringBuilder box31MisMash = new ZStringBuilder();

				box31MisMash.AppendIfNotEmpty(EntryLine.SadBox31PackagesPremable);

				box31MisMash.AppendIfNotEmpty(Box31_2DescriptionOfGoods);

				ZString conts = Box31_3ContainerNumbers;
				if (!conts.IsEmpty)
				{
					box31MisMash.AppendIfNotEmpty(Box31_1NumberOfPackagesPiecesMarksAndNumbers + ContainerNumberText + conts);
				}
				else
				{
					box31MisMash.AppendIfNotEmpty(Box31_1NumberOfPackagesPiecesMarksAndNumbers);
				}

				AppendBox31CountrySpecificInformation(box31MisMash);
				return box31MisMash.ToStringWithNewLineBetweenAppends();
			}
		}

		protected virtual ZString ContainerNumberText
		{
			get { return (NoResString)"; CN="; }
		}

		protected virtual void AppendBox31CountrySpecificInformation(ZStringBuilder stringBuilder) { }

		public ZInt Box32ItemNumber
		{
			get { return EntryLine.CL_LineNumber.ToZInt(); }
		}

		public ZString Box33CommodityCode => Box33CommodityCodeCore;
		protected virtual ZString Box33CommodityCodeCore => EntryLine.Tariff;

		public ZString Box33ECSupplement => Box33ECSupplementCore;

		protected virtual ZString Box33ECSupplementCore
		{
			get { return EntryLine.SupplementaryCode1; }
		}

		public ZString Box33ECSupplement2 => Box33ECSupplement2Core;

		protected virtual ZString Box33ECSupplement2Core
		{
			get { return EntryLine.SupplementaryCode2; }
		}

		public ZString Box34CountryOfOrigin => Box34CountryOfOriginCore;

		protected virtual ZString Box34CountryOfOriginCore => EntryLine.CountryOfOriginCode;

		public ZString Box34StateOfOrigin => Box34StateOfOriginCore;

		protected virtual ZString Box34StateOfOriginCore => EntryLine.RandomLine.JI_StateOrRegionOfOrigin;

		public ZString Box35GrossWeightInKG => Box35GrossWeightInKGCore;

		protected virtual ZString Box35GrossWeightInKGCore
		{
			get
			{
				var effectiveGrossWeight = EntryLine.EffectiveGrossWeight.InKilogramsSafe.Round(3);
				return effectiveGrossWeight == 0 ? ZString.Empty : FormatDecimalDefault(effectiveGrossWeight);
			}
		}

		public virtual ZString Box35GrossWeightInKGForCommericalPurposesOnly
		{
			get
			{
				var effectiveGrossWeight = EntryLine.GrossWeightForCommericalPurpose.InKilogramsSafe.Round(3);
				return effectiveGrossWeight == 0 ? ZString.Empty : FormatDecimalDefault(effectiveGrossWeight);
			}
		}

		public bool GrossWeightIsApplicable
		{
			get { return EntryLine.EffectiveGrossWeightIsApplicable; }
		}

		public ZString Box36Preference => Box36PreferenceCore;

		protected virtual ZString Box36PreferenceCore
		{
			get { return EntryLine.PreferenceCode; }
		}

		public virtual ZString Box37Procedure
		{
			get { return EntryLine.ProcedureCode; }
		}

		public ZString Box37_2Procedure => Box37_2ProcedureCore;

		protected virtual ZString Box37_2ProcedureCore => ZString.Empty;

		public ZString Box38NetWeightInKG => Box38NetWeightInKGCore;

		protected virtual ZString Box38NetWeightInKGCore
		{
			get { return EntryLine.CustomsQuantity.Round(3).ToStringTrimZeros(); } // Obtained from JI_CustomsQuantity always in KGs
		}

		public ZString Box39Quota => Box39QuotaCore;

		protected virtual ZString Box39QuotaCore
		{
			get { return EntryLine.QuotaOrderNumber; }
		}

		public ZString Box40PreviousDocuments
		{
			get { return GetBox40PreviousDocumentsCore(EntryLine.PreviousDocuments); }
		}

		protected virtual ZString GetBox40PreviousDocumentsCore(IEnumerable<PreviousDocument> previousDocuments)
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (var prevDoc in previousDocuments)
			{
				result.Append(FormatPreviousDocument(prevDoc));
			}

			return result.ToStringWithDelimiterBetweenAppends("; ");
		}

		protected virtual ZString FormatPreviousDocument(PreviousDocument prevDoc)
		{
			return prevDoc.CSI_SubType + "-" + prevDoc.CSI_Code + "-" + prevDoc.CSI_ReferenceNumber + (prevDoc.CSI_DateOfIssue.IsValid ? "-" + prevDoc.DateOfIssueInFormat : string.Empty);
		}

		public ZString Box41SupplementaryUnits => Box41SupplementaryUnitsCore;
		protected virtual ZString Box41SupplementaryUnitsCore => EntryLine.SupplementaryQuantity == 0 ? "" : Box41Format(EntryLine.SupplementaryQuantity) + "[" + EntryLine.SupplementaryUQ + "]";

		public ZBool ShowBox41SupplementaryUnits => ShowBox41SupplementaryUnitsCore;
		protected virtual ZBool ShowBox41SupplementaryUnitsCore => false;

		public virtual ZString Box41SupplementaryQty
		{
			get { return EntryLine.SupplementaryQuantity.IsEmpty ? "" : Box41Format(EntryLine.SupplementaryQuantity); }
		}

		protected virtual string Box41Format(ZDecimal supplementaryQuantity) => supplementaryQuantity.ToString(3);

		public ZString Box41SupplementaryUQDescription => Box41SupplementaryUQDescriptionCore;
		protected virtual ZString Box41SupplementaryUQDescriptionCore => EntryLine.SupplementaryQuantity.IsEmpty ? "" : EntryLine.RandomLine.Lookups.CustomsUQList.GetDescriptionFromCode(EntryLine.SupplementaryUQ);

		public virtual ZString Box42ItemPrice
		{
			get { return EntryLine.TotalLinePrice.ToString(); }
		}

		public ZString Box43ValuationMethod => Box43ValuationMethodCore;

		protected virtual ZString Box43ValuationMethodCore
		{
			get { return EntryLine.ValuationMethod; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No Smell, DES 238 p85")]
		public virtual ZString Box44AddInfoAndDocuments
		{
			get
			{
				var result = new ZStringBuilder();
				var isFirstPage = EntryLine.CL_LineNumber == 1;

				result.AppendIfNotEmpty(GetFirstLineOfBox44());
				AddAdditionalInfos(result, isFirstPage);

				OrgAddress supervisingOffice = EntryLine.SupervisingOffice;
				if (supervisingOffice == null && isFirstPage)
				{
					supervisingOffice = Declaration.SupervisingOfficeDocAddress.Address;
				}
				if (supervisingOffice != null)
				{
					ZStringBuilder spoffDetails = new ZStringBuilder();
					spoffDetails.AppendIfNotEmpty(supervisingOffice.Header.OH_FullName);
					spoffDetails.AppendIfNotEmpty(supervisingOffice.OA_Address1);
					spoffDetails.AppendIfNotEmpty(supervisingOffice.OA_City);
					spoffDetails.AppendIfNotEmpty(supervisingOffice.OA_PostCode);
					if (supervisingOffice.RelatedCountry != null)
					{
						spoffDetails.AppendIfNotEmpty(supervisingOffice.RelatedCountry.RN_DescMultilingual);
					}
					result.Append((NoResString)"SPOFF - " + spoffDetails.ToStringWithDelimiterBetweenAppends(EUDocumentWrapperConstants.Delimiters.CommaAndSpace));
				}

				if (isFirstPage)
				{
					ZString eoriBranchList = EuEoriProviderAndValidator.GetBranchSuffixesForBox44(EntryLine.Header);
					if (!eoriBranchList.IsEmpty)
					{
						result.Append(eoriBranchList);
					}

					if (ShouldAppendBox44SupportingDocument9DCR)
					{
						AppendSupportingDocument9DCR(result);
					}

					AddSupportingDocumentsToFirstPage(result);
					AddEntryHeaderSupportingDocumentsToFirstPage(result);
				}

				AddEntryLineSupportingDocuments(result);

				if (!EntryLine.ThirdQuantity.IsEmpty)
				{
					result.Append(EntryLine.ThirdQuantity.ToString() + "- THRDQ");
				}

				return result.ToStringWithDelimiterBetweenAppends(EUDocumentWrapperConstants.Delimiters.SemiColonAndspace);
			}
		}

		protected virtual void AddSupportingDocumentsToFirstPage(ZStringBuilder result) { }

		protected virtual void AddEntryHeaderSupportingDocumentsToFirstPage(ZStringBuilder result) => ProducedDocumentsCertificatesBuilder.AppendSupportingDocumentInfo(result, EntryHeader.SupportingDocuments);

		protected virtual void AddEntryLineSupportingDocuments(ZStringBuilder result) => ProducedDocumentsCertificatesBuilder.AppendSupportingDocumentInfo(result, EntryLine.SupportingDocuments);

		protected virtual ZString GetFirstLineOfBox44()
		{
			return ZString.Empty;
		}

		protected virtual ZBool ShouldAppendBox44SupportingDocument9DCR => ZBool.True;

		protected void AppendSupportingDocument9DCR(ZStringBuilder result)
		{
			ZString ducr = EntryHeader.DeclarationUCR;
			if (!ducr.IsEmpty)
			{
				ZStringBuilder ducrResult = new ZStringBuilder();
				ducrResult.Append("9DCR-" + ducr);
				ZString ducrPart = EntryHeader.DeclarationUCRPartSuffix;
				if (!ducrPart.IsEmpty)
				{
					ducrResult.Append((NoResString)" Part:" + ducrPart);
				}
				result.Append(ducrResult.ToString());
			}
		}

		void AddEoriSuffixes(ZStringBuilder result)
		{
			ZString eoriBranchList = EuEoriProviderAndValidator.GetBranchSuffixesForBox44(EntryLine.Header);
			if (!eoriBranchList.IsEmpty)
			{
				result.Append(eoriBranchList);
			}
		}

		protected virtual void AddAdditionalInfos(ZStringBuilder result, bool isFirstPage)
		{
			var additionalInfos =
				(isFirstPage ? EntryHeader.AdditionalInfos.Union(EntryLine.AdditionalInfos) : EntryLine.AdditionalInfos.Except(EntryHeader.AdditionalInfos))
				.Distinct();

			foreach (AdditionalInfo info in additionalInfos)
			{
				var formatResult = AdditionalInfoFormatter.Format(info);
				result.Append(formatResult);
			}
		}

		IAdditionalInformationFormatter AdditionalInfoFormatter => additionalInfoFormatter ?? (additionalInfoFormatter = GetAdditionalInformationFormatter());
		IAdditionalInformationFormatter additionalInfoFormatter;

		protected virtual IAdditionalInformationFormatter GetAdditionalInformationFormatter() => new AdditionalInformationFormatter();

		public ZString Box44_1ProducedDocumentsCertificates => Box44_1ProducedDocumentsCertificatesCore;

		protected virtual ZString Box44_1ProducedDocumentsCertificatesCore => ProducedDocumentsCertificatesBuilder.GetProducedDocumentsCertificatesFormatted(((EntryLine.CL_LineNumber == 1 ? EntryHeader.SupportingDocuments : null) ?? Enumerable.Empty<SupportingDocument>()).Concat(EntryLine.SupportingDocuments));

		protected virtual ProducedDocumentsCertificatesBuilder GetProducedDocumentsCertificatesBuilder() => new ProducedDocumentsCertificatesBuilder();

		ProducedDocumentsCertificatesBuilder ProducedDocumentsCertificatesBuilder => producedDocumentsCertificatesBuilder ?? (producedDocumentsCertificatesBuilder = GetProducedDocumentsCertificatesBuilder());
		ProducedDocumentsCertificatesBuilder producedDocumentsCertificatesBuilder;

		public ZString Box44_2SpecialMentions => Box44_2SpecialMentionsCore;
		protected virtual ZString Box44_2SpecialMentionsCore
		{
			get
			{
				var aIs = new ZStringBuilder();
				var isFirstPage = Box32ItemNumber == 1;
				AddAdditionalInfos(aIs, isFirstPage);
				if (isFirstPage)
				{
					AddEoriSuffixes(aIs); // Claire Whitebread to confirm where these should go in the EAD
				}
				return aIs.ToStringWithDelimiterBetweenAppends(EUDocumentWrapperConstants.Delimiters.SemiColonAndspace);
			}
		}

		// There is no 44/3

		public ZString Box44_4UnDangerousGoods
		{
			get
			{
				List<ZString> undgCodesToShowInBox44 = new List<ZString>();
				foreach (JobComInvoiceLine underlyingInvoiceLine in EntryLine.InvoiceLines)
				{
					foreach (var substance in underlyingInvoiceLine.UNDGs.Cast<UNDGDataItem>().Select(x => x.UNDGSubstance).Where(x => x != null))
					{
						ZString fourCharCode = substance.DG_UNNO;
						if (!undgCodesToShowInBox44.Contains(fourCharCode))
						{
							undgCodesToShowInBox44.Add(fourCharCode);
						}
					}
				}
				if (undgCodesToShowInBox44.Count > 0)
				{
					return ZString.Join(EUDocumentWrapperConstants.Delimiters.CommaAndSpace, undgCodesToShowInBox44.ToArray());
				}
				return ZString.Empty;
			}
		}

		public GenericWrappers.OrganisationWrapper Box44FiscalReference => GetBox44FiscalReferenceCore();

		protected virtual GenericWrappers.OrganisationWrapper GetBox44FiscalReferenceCore()
		{
			OrgAddress orgAddress = null;
			return new GenericWrappers.OrganisationWrapper(GenericWrappers.OrganisationUsageType.Consignee, orgAddress, ContactType.NoContactType, Factory);
		}

		public ZString Box44FiscalReferenceNumber => GetBox44FiscalReferenceNumberCore();

		protected virtual ZString GetBox44FiscalReferenceNumberCore() => ZString.Empty;

		public ZString Box44FiscalReferenceNumberLabel => GetBox44FiscalReferenceNumberLabelCore();

		protected virtual ZString GetBox44FiscalReferenceNumberLabelCore() => ZString.Empty;

		public ZString Box44FiscalReferenceNumberSummary
		{
			get
			{
				var result = ZString.Empty;

				if (ShowBox44FiscalReference)
				{
					result = Box44FiscalReferenceNumberLabel + " " + Box44FiscalReferenceNumber + " " + Box44FiscalReference + " " + Box44FiscalReference.Country.Code;
				}
				return result;
			}
		}

		public ZBool ShowBox44FiscalReference => ShowBox44FiscalReferenceCore;
		protected virtual ZBool ShowBox44FiscalReferenceCore => false;

		public ZString Box45Adjustment => GetBox45AdjustmentCore();

		protected virtual ZString GetBox45AdjustmentCore()
		{
			ZString adjCode = EntryLine.ValueAdjustmentCode;
			return (adjCode.IsEmpty ? "" : "[" + adjCode + "] ") + EntryLine.ValueAdjustmentAmount.ToString() + "%";
		}

		public ZDecimal Box46StatisticalValue => Box46StatisticalValueCore;

		protected virtual ZDecimal Box46StatisticalValueCore
		{
			get { return EntryLine.StatisticalValue.Round(2); }
		}

		public ZBool ShowBox46StatisticalValue => ShowBox46StatisticalValueCore;
		protected virtual ZBool ShowBox46StatisticalValueCore => true;

		public ZString Box46CurrencyCode
		{
			get { return EntryLine.Declaration.Country.RN_RX_NKLocalCurrency; }
		}

		public DocSADHLineTaxCollection Box47Taxes => box47Taxes ?? (box47Taxes = GetBox47TaxesCore());
		DocSADHLineTaxCollection box47Taxes;

		protected virtual DocSADHLineTaxCollection GetBox47TaxesCore()
		{
			var lineTaxCollection = new DocSADHLineTaxCollection(GetTaxBoxSupporterListCore(), Factory);
			SortBox47TaxesCore(lineTaxCollection);
			return lineTaxCollection;
		}

		protected virtual void SortBox47TaxesCore(DocSADHLineTaxCollection taxCollection)
		{
			taxCollection.Sort(new TaxSorter());
		}

		#region TaxSorter

		/// <summary>
		/// Sorts tax rows to print in order as per Appendix C7A of the UK Tariff (Page 79 v1.0)
		/// </summary>
		protected class TaxSorter : IComparer<DocSADHLineTax>
		{
			protected const string ChargeTypeD00 = "D00";
			protected const string ChargeTypeB20 = "B20";
			protected const string ChargeTypeD10 = "D10";
			public int Compare(DocSADHLineTax x, DocSADHLineTax y)
			{
				return taxCodes.IndexOf(x.G4_Type).CompareTo(taxCodes.IndexOf(y.G4_Type));
			}

			readonly IList<string> codes = new string[]
			{
				UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts,
				UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
				UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty,
				UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty,
				UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty,
				UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty,
				UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts,
				UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
				UniversalReferenceConstants.RefCusRateCodes.Vat,
				ChargeTypeD00, ChargeTypeB20, ChargeTypeD10,
				UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat
			};
			protected virtual IList<string> taxCodes => codes;
		}

		#endregion

		protected virtual IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => EntryLine.GetTaxBoxSupporterList();

		public virtual ZString Box48DeferredPayment
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				if (!Declaration.JE_PaymentMethod.IsEmpty)
				{
					var org = (Declaration.JE_PaymentMethod == DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14)
						? Declaration.Declarant?.Header
						: Declaration.Importer;

					if (org != null)
					{
						var paymentMethod = ShouldAppendBox48PaymentMethod ? (ZString)(Declaration.JE_PaymentMethod + " ") : ZString.Empty;
						result.Append(paymentMethod + org.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber));
					}
				}
				return result.ToStringWithDelimiterBetweenAppends("; ");
			}
		}

		protected virtual ZBool ShouldAppendBox48PaymentMethod => ZBool.True;

		public ZString Box49Warehouse => Box49WarehouseCore; // Premise ID

		protected virtual ZString Box49WarehouseCore
		{
			get
			{
				if (Declaration.WarehouseAddress is OrgAddress warehouseAddress && warehouseAddress.Header is OrgHeader warehouse)
				{
					return warehouse.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Declaration.CountryCode, warehouseAddress.PK);
				}

				if (EntryInstruction is CusEntryInstruction entryInstruction)
				{
					if (EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasIntoRegimeProcedure))
					{
						return entryInstruction.ToWarehouseCode;
					}

					if (EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasOutOfRegimeProcedure))
					{
						return entryInstruction.FromWarehouseCode;
					}
				}

				return ZString.Empty;
			}
		}

		/// <summary>
		/// Optionally to be completed if the goods are being exported via another EU country. Insert the name and
		/// city of the representative who is responsible for dealing with Customs Clearance at the Office of Exit.
		/// </summary>
		public virtual ZString Box50PrincipalsRepresentativeName
		{
			get { return EntryLine.PrincipalsRepresentativeName; }
		}

		/// <summary>
		/// Optionally to be completed if the goods are being exported via another EU country. Insert the name and
		/// city of the representative who is responsible for dealing with Customs Clearance at the Office of Exit.
		/// </summary>
		public virtual ZString Box50PrincipalsRepresentativeCity
		{
			get { return EntryLine.PrincipalsRepresentativeCity; }
		}

		public ZString Box44VatInfoCalculation => Box44VatInfoCalculationCore;
		protected virtual ZString Box44VatInfoCalculationCore => ZString.Empty;

		protected virtual ZString FormatDecimalDefault(ZDecimal number) => number.ToString();
	}
}
