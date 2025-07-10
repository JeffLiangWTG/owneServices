using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.Business
{
	public class DocJobComInvoiceHeader : DocBaseJobComInvoiceHeader
	{
		DocJobComInvoiceHeader(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceHeader New(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			return jobComInvoiceHeader == null ? null : new DocJobComInvoiceHeader(jobComInvoiceHeader, factoryToWrap);
		}

		public DocDeclaration Declaration
		{
			get { return (DocDeclaration)DeclarationInternal; }
		}

		#region Overrides

		protected override DocBaseJobDeclaration CreateJobDeclaration(BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		public override DocOrganisation Supplier
		{
			get { return JobDeclaration.IsImport ? GetNewOrg(JobComInvoiceHeader.SupplierDocumentaryAddress) : base.Supplier; }
		}

		#endregion

		#region Collections

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection UnclassifiedInvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)UnclassifiedInvoiceLinesInternal; }
		}

		#region CCIInvoiceLines

		public DocJobComInvoiceLineCollection CCIInvoiceLines
		{
			get
			{
				var invoiceLines = InvoiceLines;
				invoiceLines.Load(GetDummyInvoiceLines(GetCountOfDummyLinesToAddToFillCCILastPage(invoiceLines.Count)));
				return invoiceLines;
			}
		}

		static int GetCountOfDummyLinesToAddToFillCCILastPage(int count)
		{
			const int countOnFirstPage = 10;
			const int countOnSecondPage = 27;
			return count == countOnFirstPage ? 0
					   : count < countOnFirstPage ? countOnFirstPage - count
							 : countOnSecondPage - (count - countOnFirstPage - (countOnSecondPage * ((count - countOnFirstPage) / countOnSecondPage)));
		}

		#endregion

		#endregion

		#region Wrapper Fields

		public ZInt InvoiceCount
		{
			get { return 1; }
		}

		public ZDateTime EffectiveValuationDate
		{
			get { return JobComInvoiceHeader.EffectiveValuationDate; }
		}

		public ZString CommercialInvoiceOriginatorAddress
		{
			get
			{
				ZString originator = new AddressFormatter(Factory, JobComInvoiceHeader.CommercialInvoiceOriginator, GlbCompany.CurrentCompany, false).PostalAddress();
				if (originator.IsEmpty && Declaration != null)
				{
					originator = Declaration.CommercialInvoiceOriginatorAddress;
				}
				return originator;
			}
		}

		#region Organisations

		public DocOrganisation Vendor
		{
			get
			{
				var declaration = JobComInvoiceHeader.JobDeclaration;
				if (declaration != null && declaration.IsImport)
				{
					var supplierDocumentaryAddress = JobComInvoiceHeader.SupplierDocumentaryAddress;
					return supplierDocumentaryAddress.E2_AddressOverride || supplierDocumentaryAddress.HasRealAddress ? GetNewOrg(supplierDocumentaryAddress) : GetNewOrg(declaration.SupplierDocumentaryAddress);
				}
				else
				{
					return GetNewOrg(JobComInvoiceHeader.JZ_OH_Supplier_Effective);
				}
			}
		}

		public DocOrganisation Consignee
		{
			get
			{
				DocOrganisation result = null;
				var declaration = JobComInvoiceHeader.JobDeclaration;
				if (declaration != null && declaration.IsImport)
				{
					result = JobComInvoiceHeader.FinalConsigneeAddress.HasRealAddress ? GetNewOrg(JobComInvoiceHeader.FinalConsigneeAddress)
						: declaration.ImporterOfRecordAddress.HasRealAddress ? GetNewOrg(declaration.ImporterOfRecordAddress) : null;
				}
				if (result == null)
				{
					result = GetNewOrg(declaration.JE_OH_Importer);
				}
				return result;
			}
		}

		public new DocOrganisation Buyer
		{
			get
			{
				DocOrganisation result = null;
				var declaration = JobComInvoiceHeader.JobDeclaration;
				if (declaration != null && declaration.IsImport && JobComInvoiceHeader.BuyerDocumentaryAddress.HasRealAddress)
				{
					result = GetNewOrg(JobComInvoiceHeader.BuyerDocumentaryAddress);
				}
				return result ?? GetNewOrg(JobComInvoiceHeader.JZ_OH_Buyer_Effective);
			}
		}

		public DocOrganisation Exporter
		{
			get
			{
				DocOrganisation result = null;
				var declaration = JobComInvoiceHeader.JobDeclaration;
				if (declaration != null && declaration.IsImport && JobComInvoiceHeader.ExporterDocumentaryAddress.HasRealAddress)
				{
					result = GetNewOrg(JobComInvoiceHeader.ExporterDocumentaryAddress);
				}
				return result;
			}
		}

		#endregion

		#region Countries & Places

		public DocCountry TranshipmentCountry
		{
			get { return DocCountry.New(Factory, InvoiceOGDData.TranshipmentCountry); }
		}

		public ZString CountryAndStateOfOrigin
		{
			get { return InvoiceLines.Any(l => !((DocJobComInvoiceLine)l).CountryAndStateOfOrigin.IsEmpty) ? SeeBelowString : InvoiceOGDData.CommonCountryOfOrigin; }
		}

		public ZString CountryAndStateOfExport
		{
			get { return InvoiceLines.Any(l => !((DocJobComInvoiceLine)l).CountryAndStateOfExport.IsEmpty) ? SeeBelowString : InvoiceOGDData.CommonCountryOfExport; }
		}

		public DocUNLOCO PlaceOfDirectShipment
		{
			get { return DocUNLOCO.New(Factory, JobComInvoiceHeader.CA_RL_NKLastPort); }
		}

		public ZString PortOfClearance
		{
			get
			{
				return JobComInvoiceHeader.CA_PortOfClearance.IsEmpty && JobComInvoiceHeader.JobDeclaration != null
					? JobComInvoiceHeader.JobDeclaration.JE_CustomsOffice : JobComInvoiceHeader.CA_PortOfClearance;
			}
		}

		public ZString PortOfClearanceCodeDescription
		{
			get { return DocDeclaration.GetCodeDescriptionFormatted(PortOfClearance, JobComInvoiceHeader.AddInfoLookups.CBSAOffices); }
		}

		public ZString ProvinceOfClearance
		{
			get
			{
				return Factory.GetCachedValue("DocJobComInvoiceHeader|ProvinceOfClearance" + "|" + Core.Constants.CountryCodes.Canada + "|" + PortOfClearance, () =>
				{
					var cbsaOfficeCode = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, PortOfClearance, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					return cbsaOfficeCode?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province) ?? ZString.Empty;
				});
			}
		}

		#endregion

		#region Measurement

		public ZDecimal GrossWeightInKilograms
		{
			get { return new ZWeight(JobComInvoiceHeader.JZ_Weight, JobComInvoiceHeader.JZ_WeightUQ).InKilogramsSafe; }
		}

		public ZDecimal NetWeightInKilograms
		{
			get { return new ZWeight(JobComInvoiceHeader.JZ_NetWeight, JobComInvoiceHeader.JZ_NetWeightUQ).InKilogramsSafe; }
		}

		public ZInt TimeLimit
		{
			get { return JobComInvoiceHeader.CA_TimeLimit; }
		}

		public ZString TimeLimitUnit
		{
			get { return JobComInvoiceHeader.CA_TimeLimitCode; }
		}

		public ZString NumberOfPacksFormatted
		{
			get
			{
				return JobComInvoiceHeader.JZ_NoOfPacks > 0 ? new ZString((JobComInvoiceHeader.JZ_NoOfPacks.ToString("#,###.###") + " " + JobComInvoiceHeader.NoOfPacksPackType).TrimEnd()) : ZString.Empty;
			}
		}

		#endregion

		#region Amounts & Charges

		public ZDecimal IncludedTransportChargesAndInsuranceInCAD
		{
			get { return InvoiceOGDData.IncludedOFTAndONS.Round(AmountDecimalPlaces); }
		}

		public ZDecimal IncludedConstructionCostsInCAD
		{
			get { return InvoiceOGDData.IncludedConstruction.Round(AmountDecimalPlaces); }
		}

		public ZDecimal IncludedExportPackingCostsInCAD
		{
			get { return InvoiceOGDData.IncludedPacking.Round(AmountDecimalPlaces); }
		}

		public ZDecimal ExcludedTransportChargesAndInsuranceInCAD
		{
			get { return InvoiceOGDData.ExcludedOFTAndONS.Round(AmountDecimalPlaces); }
		}

		public ZDecimal ExcludedCommissionInCAD
		{
			get { return InvoiceOGDData.ExcludedCommission.Round(AmountDecimalPlaces); }
		}

		public ZDecimal ExcludedExportPackingCostsInCAD
		{
			get { return InvoiceOGDData.ExcludedPacking.Round(AmountDecimalPlaces); }
		}

		public ZDecimal LVSTotalBilledAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (JobDeclaration != null)
				{
					if (JobDeclaration.IsLVX)
					{
						result = JobDeclaration.TotalBilledAmount;
					}
					else
					{
						result = JobComInvoiceHeader.TotalBilledAmount;
					}
				}
				return result;
			}
		}

		public ZDecimal LVSTotalInvoicedAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (JobDeclaration != null)
				{
					if (JobDeclaration.IsLVX)
					{
						result = JobDeclaration.TotalInvoicedAmount;
					}
					else
					{
						result = JobComInvoiceHeader.TotalInvoicedAmount;
					}
				}
				return result;
			}
		}

		JobDeclaration JobDeclaration
		{
			get { return JobComInvoiceHeader.JobDeclaration; }
		}

		#endregion

		#region References & Other text fields

		public ZString TreatmentCode
		{
			get { return InvoiceLines.Any(l => !((DocJobComInvoiceLine)l).TreatmentCode.IsEmpty) ? SeeBelowString : JobComInvoiceHeader.CA_TreatmentCode; }
		}

		public ZString ValueForDutyCode
		{
			get { return JobComInvoiceHeader.CA_ValueForDutyCode; }
		}

		public ZString ConsigneeBusinessNumber
		{
			get { return GetCustomsRegNo(Consignee, OrgCusCode.CACodeTypes.BusinessNumberForImportExport, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial); }
		}

		public ZString ImporterBusinessNumber
		{
			get { return GetCustomsRegNo(Buyer, OrgCusCode.CACodeTypes.BusinessNumberForImportExport, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial); }
		}

		public ZString OtherReference
		{
			get { return InvoiceOGDData.OtherReference; }
		}

		public ZString DepartmentRuling
		{
			get { return InvoiceOGDData.DepartmentRuling; }
		}

		public ZString ConditionsOfSale
		{
			get { return InvoiceOGDData.ConditionsOfSale; }
		}

		public ZString TermsOfPayment
		{
			get { return InvoiceOGDData.TermsOfPayment; }
		}

		#endregion

		#region Indicators

		public ZBool IsAdjustmentsToPricePaidOrPayableApplicable
		{
			get
			{
				return ServicesInd
					   || RoyaltyInd
					   || IncludedTransportChargesAndInsuranceInCAD > 0
					   || ExcludedTransportChargesAndInsuranceInCAD > 0
					   || IncludedConstructionCostsInCAD > 0
					   || ExcludedCommissionInCAD > 0
					   || IncludedExportPackingCostsInCAD > 0
					   || ExcludedExportPackingCostsInCAD > 0;
			}
		}

		public ZBool ServicesInd
		{
			get { return InvoiceOGDData.ServicesInd; }
		}

		public ZBool RoyaltyInd
		{
			get { return InvoiceOGDData.RoyaltyInd; }
		}

		#endregion

		#endregion

		#region Implementation

		DocOrganisation GetNewOrg(ZGuid pk)
		{
			return pk.IsValid ? DocOrganisation.New(JobComInvoiceHeader.Factory, pk) : null;
		}

		DocOrganisation GetNewOrg(JobDocAddress jobDocAddress)
		{
			return jobDocAddress != null ? DocOrganisation.New(jobDocAddress, JobComInvoiceHeader.Factory) : null;
		}

		ZString GetCustomsRegNo(DocOrganisation orgHeader, params ZString[] codeTypes)
		{
			if (orgHeader != null)
			{
				var refCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada);
				foreach (var codeType in codeTypes)
				{
					var orgCusCode = orgHeader.CustomCodes.GetCustomsRegNoForCodeAndCountry(codeType, refCountry);
					if (!orgCusCode.IsEmpty)
					{
						return orgCusCode;
					}
				}
			}
			return ZString.Empty;
		}

		static IEnumerable GetDummyInvoiceLines(int dummyLinesToAdd)
		{
			if (dummyLinesToAdd > 0)
			{
				var factory = new BusinessObjectFactory();
				for (var i = 0; i < dummyLinesToAdd; i++)
				{
					yield return factory.New<JobComInvoiceLine>();
				}
			}
		}

		static ZString SeeBelowString
		{
			get { return Res.GetString("6bc469cd-0719-4573-9dca-f085317ada4a", "See Below"); }
		}

		JobComInvoiceHeader JobComInvoiceHeader
		{
			get { return (JobComInvoiceHeader)WrappedObject; }
		}

		IEDIInvoiceOGD InvoiceOGDData
		{
			get { return JobComInvoiceHeader; }
		}

		const int AmountDecimalPlaces = 2;

		#endregion
	}
}
