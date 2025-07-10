using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryHeader : ICustomsEntryHeader
	{
		public new partial class Schema : Customs.Business.CusEntryHeader.Schema
		{
			#region SuppressResourceStringsCheckRegion

			public const string ManualNo = "ManualNo";
			public const string VesselName = "VesselName";
			public const string Voyage = "Voyage";
			public const string BillOfLading = "BillOfLading";
			public const string CustomsProcedureCode = "CustomsProcedureCode";
			public const string CustomsProcedureDesc = "CustomsProcedureDesc";
			public const string IncoTermDesc = "IncoTermDesc";
			public const string ContractNo = "ContractNo";
			public const string FreightFeeMarkDesc = "FreightFee+MarkDesc";
			public const string FreightFeeAmount = "FreightFeeAmount";
			public const string FreightFeeCurrencyCode = "FreightFeeCurrencyCode";
			public const string GrossWeightInKG = "GrossWeightInKG";
			public const string NetWeightInKG = "NetWeightInKG";

			#endregion
		}

		public ZDateTime DateOfValuation
		{
			get
			{
				var result = MovementReferenceNumberIssueDate;
				if (result.IsEmpty)
				{
					result = Declaration?.DateOfValuation ?? ZDateTime.Today;
				}
				return result;
			}
		}

		#region Implementation of ICustomsEntryLine

		#region Parties

		#region CNJobDocAddresses

		public CNJobDocAddress TradeParty => IsEntering ? Declaration?.ImporterDocumentaryAddress : Declaration?.SupplierDocumentaryAddress;

		public CNJobDocAddress CargoOwner
		{
			get
			{
				CNJobDocAddress address = null;
				var declaration = Declaration;
				if (declaration != null)
				{
					if (IsEntering)
					{
						address = declaration.BuyerDocAddress.HasAddress() ? declaration.BuyerDocAddress : declaration.ImporterDocumentaryAddress;
					}
					else
					{
						address = declaration.ManufacturerDocumentaryAddress.HasAddress() ? declaration.ManufacturerDocumentaryAddress : declaration.SupplierDocumentaryAddress;
					}
				}
				return address;
			}
		}

		public CNJobDocAddress OverseasParty
		{
			get
			{
				var address = IsEntering ? Declaration?.SupplierDocumentaryAddress : Declaration?.ImporterDocumentaryAddress;
				if (address != null && address.Country != null && address.Country.Code == Core.Constants.CountryCodes.China)
				{
					address = null;
				}
				return address;
			}
		}

		public OrgHeader Declarant => Declaration?.Declarant;

		#endregion

		ZString EmptyCode => "NO";

		ZString ICusCommonHeader.TradePartyUSCI => TradeParty?.SocialCreditCode ?? ZString.Empty;
		ZString ICusCommonHeader.TradePartyCCD => TradeParty?.CustomsCode ?? ZString.Empty;
		ZString ICustomsEntryHeader.TradePartyCIQ => TradeParty?.CIQCode ?? ZString.Empty;
		ZString ICusCommonHeader.TradePartyName => TradeParty?.ChineseCompanyName ?? ZString.Empty;

		ZString ICustomsEntryHeader.CargoOwnerUSCI => (CargoOwner?.SocialCreditCode ?? ZString.Empty).FallbackIfEmpty(EmptyCode);
		ZString ICustomsEntryHeader.CargoOwnerCCD => CargoOwner?.CustomsCode ?? ZString.Empty;
		ZString ICustomsEntryHeader.CargoOwnerCIQ => CargoOwner?.CIQCode ?? ZString.Empty;
		ZString ICustomsEntryHeader.CargoOwnerName => CargoOwner?.ChineseCompanyName ?? ZString.Empty;

		ZString ICustomsEntryHeader.OverseasPartyCode => (OverseasParty?.OverseasPartyCode ?? ZString.Empty).FallbackIfEmpty(EmptyCode);
		ZString ICustomsEntryHeader.OverseasPartyName => (OverseasParty?.EnglishCompanyName ?? ZString.Empty).FallbackIfEmpty(EmptyCode);

		ZString ICustomsEntryHeader.DeclarantUSCI => Declarant.GetUSCI();
		ZString ICustomsEntryHeader.DeclarantCCD => Declarant.GetCCD();
		ZString ICustomsEntryHeader.DeclarantCIQ => Declarant.GetCIQ();
		ZString ICustomsEntryHeader.DeclarantName => Declarant?.MainAddress?.GetChineseCompanyName() ?? ZString.Empty;

		#endregion

		#region Customs Offices

		public ZString OfficeOfEntryOrExitCode => Declaration.JE_OfficeOfEntryExit;
		public ZString CIQOfficeOfEntryOrExitCode => Declaration.JE_CIQOfficeOfEntryExit;
		public ZString CustomsOfficeCode => Declaration.JE_CustomsOffice;

		#endregion

		#region Dates

		public ZDateTime ImportOrExportDate => IsEntering ? Declaration.JE_DateOfArrival : Declaration.JE_ExportDate;
		public ZDateTime DeclarantDate => MovementReferenceNumberIssueDate;

		#endregion

		#region Bounded Area Code

		public ZString BondedAreaCode => Declaration.WarehouseDocAddress?.Address?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Core.Constants.CountryCodes.China) ?? ZString.Empty;

		#endregion

		#region Feight Yard Code

		public ZString FreightYardCode => Declaration.DepotDocAddress?.Address?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DepotControlledPremisesID, Core.Constants.CountryCodes.China) ?? ZString.Empty;

		#endregion

		#region Transport Details

		public ZString TransportModeCode
		{
			get
			{
				ZString result;

				if (Declaration.TransportDataHelper.IsCrossBorder)
				{
					result = CNTransportModeList.GetCorrespondingTransportCode(Declaration.JE_TransportMode);
				}
				else if (IsCustomsEntry)
				{
					result = Declaration.JE_CNTransportMode;
				}
				else
				{
					result = CNTransportModeList.Codes.Others;
				}

				return result;
			}
		}

		public ZString BillOfLading => Declaration.TransportDataHelper.PopulateBillNumber(this);
		public ZString VesselName => Declaration.TransportDataHelper.PopulateVesselName();
		public ZString Voyage => Declaration.TransportDataHelper.PopulateVoyage(this);
		public ZString LocationOfGoods => Declaration.JE_LocationOfGoods;

		#endregion

		#region Procedure Code

		public ZString CustomsProcedureCode => EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZString CustomsProcedureDesc => CNRefCusProcedure.GetRefCusProcedure(Factory, CustomsProcedureCode, DateOfValuation)?.ZZ6_Description ?? ZString.Empty;

		#endregion

		#region LevyType

		public ZString LevyTypeCode => EntryInstruction?.CEI_LevyType ?? ZString.Empty;

		#endregion

		#region Countries

		public RefCountry CountryOfTrade => Declaration.CountryOfTrade;
		public ZString CountryOfTradeCode => CountryOfTrade.GetCNCountryCode();

		public RefCountry CountryOfLoadOrDischarge => RefCountry.LoadFromCountryCode(Factory, (IsEntering ? Declaration.JE_RL_NKPortOfLoading : Declaration.JE_RL_NKPortOfArrival).Left(2));
		public ZString CountryOfLoadOrDischargeCode => CountryOfLoadOrDischarge.GetCNCountryCode();

		#endregion

		#region Ports

		public ZString PortOfOriginOrDestCode => IsEntering ? Declaration.JE_CNPortOfOrigin : Declaration.JE_CNPortOfDestination;

		public ZString PortOfStopoverCode => IsEntering ? Declaration.JE_CNLastPortBeforeEntry : ZString.Empty;

		#endregion

		#region IncoTerm

		public ZString IncoTermCode
		{
			get
			{
				var code = RandomHeader.JZ_IncoTerm;
				if (IsRecordListing && !Declaration.TransportDataHelper.IsCrossBorder)
				{
					if (IsEntering)
					{
						code = Core.Constants.IncoTerms.CostInsuranceAndFreight;
					}
					else if (IsExiting)
					{
						code = Core.Constants.IncoTerms.FreeOnBoard;
					}
				}

				return ShipmentIncoTerm.MapIncoTermCode(code);
			}
		}

		public ZString IncoTermDesc => Factory.GetCachedValue<ShipmentIncoTerm>().GetDescriptionFromCode(IncoTermCode);

		#endregion

		#region Fees

		public CustomsFee FreightFee => cachedFreightFee?.Value ?? (cachedFreightFee = new CachedProperty<CustomsFee>(Factory, () => this.CalculateCustomsFee(Common.CustomsChargeTypeList.Codes.OverseasFreight))).Value;
		CachedProperty<CustomsFee> cachedFreightFee;

		public ZDecimal FreightFeeAmount => FreightFee.Amount;
		public ZString FreightFeeCurrencyCode => FreightFee.CurrencyCode;
		public ZString FreightFeeMarkCode => FreightFee.MarkCode;

		public CustomsFee InsuranceFee => cachedInsuranceFee?.Value ?? (cachedInsuranceFee = new CachedProperty<CustomsFee>(Factory, () => this.CalculateCustomsFee(Common.CustomsChargeTypeList.Codes.OverseasInsurance))).Value;
		CachedProperty<CustomsFee> cachedInsuranceFee;

		public ZDecimal InsuranceFeeAmount => InsuranceFee.Amount;
		public ZString InsuranceFeeCurrencyCode => InsuranceFee.CurrencyCode;
		public ZString InsuranceFeeMarkCode => InsuranceFee.MarkCode;

		public CustomsFee OtherFee => cachedOtherFee?.Value ?? (cachedOtherFee = new CachedProperty<CustomsFee>(Factory, () => this.CalculateCustomsFee(CustomsChargeTypeList.Codes.Royalty))).Value;
		CachedProperty<CustomsFee> cachedOtherFee;

		public ZDecimal OtherFeeAmount => OtherFee.Amount;
		public ZString OtherFeeCurrencyCode => OtherFee.CurrencyCode;
		public ZString OtherFeeMarkCode => OtherFee.MarkCode;

		#endregion

		#region Package

		public ZInt NoOfPacks => CEI_Packages;

		public ZString PackTypeCode => CEI_PackageUQ;

		public ZString PackTypeDesc => XC_PackageUQDescription;

		#endregion

		#region Weight

		public ZDecimal GrossWeightInKG => Math.Max(InvoiceLines.Sum(invoiceline => invoiceline.GrossWeightInKG), 0.01m).Round(2);

		public ZDecimal NetWeightInKG => Math.Max(InvoiceLines.Sum(invoiceline => invoiceline.NetWeightInKG), 0.01m).Round(2);

		#endregion

		#region Confirm

		public ZString SpecialRelationshipConfirmCode => RandomHeader?.JZ_SpecialRelationshipConfirm ?? ZString.Empty;
		public ZString PriceAffectConfirmCode => RandomHeader?.JZ_PriceAffectConfirm ?? ZString.Empty;
		public ZString PaymentOfRoyaltyConfirmCode => RandomHeader?.JZ_PaymentOfRoyaltyConfirm ?? ZString.Empty;

		#endregion

		#region Pricing Confirm

		public ZString FormulaPricingConfirmCode => RandomHeader?.JZ_Calc_FormulaPricingConfirm ?? ZString.Empty;
		public ZString TemporaryPricingConfirmCode => RandomHeader?.JZ_Calc_TemporaryPricingConfirm ?? ZString.Empty;

		#endregion

		#region MarksAndNotes

		public ZString Remarks
		{
			get
			{
				string remarks = EntryInstruction?.CustomsMessageRemarks;

				var mergedLinesWithNumber = MergedLines.Cast<CusEntryLine>().OrderBy(v => v.EntryLineNo).Where(v => !v.RandomLine.FormulaPricingRecordNumber.IsEmpty).ToList();

				var onlyOneLine = mergedLinesWithNumber.Count == 1;

				string GetFormulaPricingRecordRemark(CusEntryLine entryLine)
				{
					var entryLineNo = onlyOneLine ? string.Empty : $"#{entryLine.EntryLineNo}";
					return $"公式定价{entryLine.RandomLine.FormulaPricingRecordNumber}{entryLineNo}@";
				}

				return string.Join(" ", mergedLinesWithNumber.Select(v => GetFormulaPricingRecordRemark(v)).Append(remarks).Where(x => !string.IsNullOrEmpty(x)));
			}
		}

		public ZString MarksAndNumbers
		{
			get
			{
				var marksAndNumbers = ZString.Join(";", InvoiceHeaders.Cast<JobComInvoiceHeader>().Select(x => x.EffectiveMarksAndNumbers).Where(x => !x.IsEmpty && x != Core.Constants.ContainerMarking.NoMarks).Distinct().ToArray());
				return marksAndNumbers.IsEmpty ? (ZString)Core.Constants.ContainerMarking.NoMarks : marksAndNumbers;
			}
		}

		#endregion

		#region Numbers

		public ZString LocalReferenceNumber => CH_BGMReference;

		public ZString ManualNo => EntryInstruction?.CEI_ManualNo ?? ZString.Empty;

		public ZString LicenseNo => CusSupportingDocuments.Cast<CusSupportingDocument>().FirstOrDefault(document => document.IsLicense)?.CSI_ReferenceNumber ?? ZString.Empty;

		public ZString ContractNo
		{
			get
			{
				return (fContractNo ?? (fContractNo = new CachedProperty<ZString>(Factory, () =>
				{
					var contractNos = InvoiceHeaders.SelectMany(x => x.ContractNumbers.Select(ctr => ctr.J2_ReferenceNumber)).Distinct();
					if (contractNos.IsCountLessThan(2))
					{
						return contractNos.FirstOrDefault();
					}
					else
					{
						var length = 0;
						var contractNosToShow = new List<ZString>();
						foreach (var elem in contractNos)
						{
							length += elem.Length;
							if (length <= 32)
							{
								contractNosToShow.Add(elem);
								length++;
							}
							else
							{
								break;
							}
						}
						ZString result = new ZStringBuilder(contractNosToShow).ToStringWithDelimiterBetweenAppends(",");
						if (contractNos.IsCountMoreThan(contractNosToShow.Count))
						{
							if (result.Length < 32)
							{
								result += (NoResString)"等";
							}
							else if (result.Length == 32 && contractNosToShow.Count > 1)
							{
								result = result.SubstringSafe(0, result.LastIndexOf(',')) + (NoResString)"等";
							}
						}
						return result;
					}
				}))).Value;
			}
		}
		CachedProperty<ZString> fContractNo;

		public ZString RelatedEntryNumber => EntryInstruction?.CEI_RelatedMRN ?? ZString.Empty;

		public ZString RelatedManualNumber => EntryInstruction?.CEI_RelatedManualNo ?? ZString.Empty;

		#endregion

		#region DocumentSubmissionType

		public ZString DocumentSubmissionTypeCode => EntryInstruction?.CEI_DocumentSubmissionType ?? ZString.Empty;

		#endregion

		#region OperationMatters

		ZBool IsOperationMatterMatch(ZString code)
		{
			return EntryInstruction?.OperationMatters.ContainsCode(code) ?? ZBool.False;
		}

		public ZBool IsPaperlessTaxForm => IsOperationMatterMatch(OperationMatterList.Codes.PaperlessTaxForm);
		public ZBool IsAutonomousTaxFiling => IsOperationMatterMatch(OperationMatterList.Codes.AutonomousTaxFiling);
		public ZBool IsAssuredInspectClearance => IsOperationMatterMatch(OperationMatterList.Codes.AssuredInspectClearance);

		#endregion

		IEnumerable<ICusSupportingDocument> ICustomsEntryHeader.SupportingDocuments => SupportingDocuments.Cast<ICusSupportingDocument>();

		IEnumerable<ICusContainer> ICustomsEntryHeader.Containers => EntryHeaderContainers.Cast<ICusContainer>();

		IEnumerable<ICustomsEntryLine> ICustomsEntryHeader.EntryLines => MergedLines.Cast<ICustomsEntryLine>();

		#endregion

		#region Implementation of ICIQDataHeader

		public ZString OfficeOfDestination => Declaration.OfficeOfDestination;

		public ZDateTime DepartureDate => Declaration.JE_ExportDate;

		public ZDateTime DateOfUnloadComplete => Declaration.JE_DateOfUnloadComplete;

		public ZString BillNumber => Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea ? Declaration.JE_MasterBill : ZString.Empty;

		public ZString IsOriginalContainerLoading
		{
			get
			{
				var result = ZString.Empty;
				if (InvoiceLines != null)
				{
					var lines = InvoiceLines.Cast<JobComInvoiceLine>();
					if (lines.Any(line => line.JI_OrigContainerFlag == ConfirmationTypeList.Codes.Yes))
					{
						result = ConfirmationTypeList.Codes.Yes;
					}
					else if (lines.Any(line => line.JI_OrigContainerFlag == ConfirmationTypeList.Codes.No))
					{
						result = ConfirmationTypeList.Codes.No;
					}
				}
				return result;
			}
		}

		public ZString CIQRelatedNumber => EntryInstruction?.CEI_CIQRelatedNum ?? ZString.Empty;

		public ZString CIQRelatedReason => EntryInstruction?.CEI_CIQRelatedReason ?? ZString.Empty;

		public ZString ConsumerContactName
		{
			get
			{
				var result = ZString.Empty;
				if (IsEntering && Declaration != null)
				{
					result = Declaration.BuyerDocAddress.E2_Contact;
					if (result.IsEmpty)
					{
						result = Declaration.ImporterDocumentaryAddress.E2_Contact;
					}
				}
				return result;
			}
		}

		public ZString ConsumerContactPhone
		{
			get
			{
				PhoneNumber phone = null;
				if (IsEntering && Declaration != null)
				{
					if (!Declaration.BuyerDocAddress.E2_Contact.IsEmpty)
					{
						phone = Declaration.BuyerDocAddress.PhoneNumber;
					}
					else if (!Declaration.ImporterDocumentaryAddress.E2_Contact.IsEmpty)
					{
						phone = Declaration.ImporterDocumentaryAddress.PhoneNumber;
					}
				}
				return phone?.FormattedLocalNumberIfLoggedInSameCountryForBinding ?? ZString.Empty;
			}
		}

		public IEnumerable<ZString> OtherPackageCodes => EntryInstruction?.OtherPackages?.AllCodes ?? Enumerable.Empty<ZString>();

		public IEnumerable<ZString> SpecialBusinessIdentifiers => EntryInstruction?.SpecialBusinessIdentifiers?.AllCodes ?? Enumerable.Empty<ZString>();

		IEnumerable<ICIQEnterpriseQualification> ICIQDataHeader.EnterpriseQualifications => EntryInstruction?.EnterpriseQualifications.Cast<ICIQEnterpriseQualification>() ?? Enumerable.Empty<ICIQEnterpriseQualification>();

		IEnumerable<ICIQRequiredDocument> ICIQDataHeader.RequiredDocuments => RequiredDocuments.Cast<ICIQRequiredDocument>();

		#endregion
	}
}
