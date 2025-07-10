using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using SDFields = Enterprise.DocumentWrappers.Customs.Base.DocBaseJobDeclaration.SDFields;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("InvoiceNumber")]
	public class CommercialInvoiceWrapper : GenericWrapper
	{
		public static CommercialInvoiceWrapper[] New(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			BaseJobDeclaration declarationToWrap = null;
			ForwardingShipment shipmentToWrap = businessObjectToWrap as ForwardingShipment;
			if (shipmentToWrap != null)
			{
				declarationToWrap = (BaseJobDeclaration)shipmentToWrap.DeclarationForDocuments;
			}
			else
			{
				declarationToWrap = businessObjectToWrap as BaseJobDeclaration;
			}
			if (declarationToWrap != null)
			{
				List<CommercialInvoiceWrapper> result = new List<CommercialInvoiceWrapper>();
				foreach (BaseJobComInvoiceHeader invoiceHeader in declarationToWrap.Invoices)
				{
					result.Add(new CommercialInvoiceWrapper(invoiceHeader, factory));
				}
				return result.ToArray();
			}

			BaseJobComInvoiceHeader invoiceHeaderToWrap = businessObjectToWrap as BaseJobComInvoiceHeader;
			if (invoiceHeaderToWrap != null)
			{
				return new CommercialInvoiceWrapper[] { new CommercialInvoiceWrapper(invoiceHeaderToWrap, factory) };
			}
			return null;
		}

		public CommercialInvoiceWrapper(BaseJobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory)
			: base(invoiceHeader, factory)
		{
			InvoiceHeaderBO = invoiceHeader ?? factory.GetNull<BaseJobComInvoiceHeader>();
			DeclarationBO = InvoiceHeaderBO.JobDeclaration;
		}
		readonly BaseJobDeclaration DeclarationBO;

		public FreightWrapper FreightJob
		{
			get
			{
				if (freightJob == null && DeclarationBO != null)
				{
					freightJob = FreightWrapperFromDeclaration.New(DeclarationBO, Factory);
				}
				return freightJob;
			}
		}
		FreightWrapper freightJob;

		public CodeAndDescriptionWrapper IncoTerm
		{
			get
			{
				ZString incoTerm = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.GetInternationalCode(InvoiceHeaderBO.JZ_IncoTerm);
				if (incoTerm.IsEmpty)
				{
					return new CodeAndDescriptionWrapper(InvoiceHeaderBO.JZ_IncoTerm, InvoiceHeaderBO.Lookups.JZ_IncoTerm_List, Factory);
				}
				else
				{
					return new CodeAndDescriptionWrapper(incoTerm, Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>(), Factory);
				}
			}
		}

		public CountryWrapper CountryOfOrigin
		{
			get
			{
				ZString originCode = ZString.Empty;
				if (FreightJob != null)
				{
					originCode = FreightJob.Origin.Location.Country.Code;
				}
				if (!InvoiceHeaderBO.JZ_RN_NKDefaultOrigin.IsEmpty)
				{
					originCode = InvoiceHeaderBO.JZ_RN_NKDefaultOrigin;
				}
				return new CountryWrapper(originCode, Factory);
			}
		}

		public MoneyWrapper InvoiceAmount
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.InvoiceAmount, Factory); }
		}

		public bool IsExport
		{
			get { return InvoiceHeaderBO.IsExport; }
		}
		public ZString MarksAndNumbers => InvoiceHeaderBO.JZ_MarksAndNumbers;

		public ValueAndUnitWrapper NoOfPacks => new ValueAndUnitWrapper(InvoiceHeaderBO.JZ_NoOfPacks, InvoiceHeaderBO.NoOfPacksPackType, InvoiceHeaderBO.Lookups.NoOfPacksPackType_List, Factory);

		public OrganisationWrapper Buyer
		{
			get
			{
				OrganisationWrapper result = null;
				if (InvoiceHeaderBO is Integration.Customs.US.IJobComInvoiceHeader)
				{
					var usInvoiceHeader = InvoiceHeaderBO as Integration.Customs.US.IJobComInvoiceHeader;
					var usBuyer = Factory.Load<OrgHeader>(usInvoiceHeader.BuyerOrgPK);
					if (usBuyer != null)
					{
						result = new OrganisationWrapper(OrganisationUsageType.Buyer, usBuyer.MainAddress, ContactType.All, Factory);
					}
					else
					{
						result = FreightJobBuyer;
					}
				}
				else
				{
					result = FreightJobBuyer;
				}
				return result;
			}
		}

		OrganisationWrapper FreightJobBuyer
		{
			get
			{
				OrganisationWrapper result = null;
				if (FreightJob != null)
				{
					result = FreightJob.Buyer;
				}
				return result;
			}
		}

		public ZString LCDateNumberOrInvoiceDate
		{
			get { return IsExport ? LCDateNumber : (ZString)InvoiceDate.ToShortDateString(); }
		}

		public ZString LCDateNumber
		{
			get { return ZString.Format("{0} {1}", LetterOfCreditNumber, LetterOfCreditDate); }
		}

		public OrganisationWrapper Importer
		{
			get { return fImporter ?? (fImporter = GetImporter()); }
		}
		OrganisationWrapper fImporter;

		protected virtual OrganisationWrapper GetImporter()
		{
			return new OrganisationWrapper(OrganisationUsageType.Importer, InvoiceHeaderBO.EffectiveConsigeeAddress, ContactType.Consignee, Factory);
		}

		public ZString ImporterRequiredVATNumber
		{
			get
			{
				if (DeclarationBO != null && DeclarationBO.Importer != null && InvoiceHeaderBO.Importer_Effective == DeclarationBO.Importer && ParentFreightJob.Consignee != null)
				{
					return ParentFreightJob.ConsigneeRequiredTaxNumber;
				}
				else
				{
					if (DeclarationBO != null && !DeclarationBO.JE_RL_NKFinalDestination.IsEmpty && DeclarationBO.FinalDestination != null)
					{
						return RequiredTaxNumbers.GetRequiredTaxNumberWithType(DeclarationBO.FinalDestination.RL_RN_NKCountryCode, string.Empty, InvoiceHeaderBO.Importer_Effective, RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
					}

					return ZString.Empty;
				}
			}
		}

		public OrganisationWrapper Supplier
		{
			get { return fSupplier ?? (fSupplier = GetSupplier()); }
		}
		OrganisationWrapper fSupplier;

		public ZString SupplierRequiredVATNumber
		{
			get
			{
				if (DeclarationBO != null && DeclarationBO.Supplier != null && InvoiceHeaderBO.Supplier_Effective == DeclarationBO.Supplier && ParentFreightJob.Consignor != null)
				{
					return ParentFreightJob.ConsignorRequiredTaxNumber;
				}
				else
				{
					if (DeclarationBO != null && DeclarationBO.Origin != null && !DeclarationBO.Origin.RL_RN_NKCountryCode.IsEmpty)
					{
						if (DeclarationBO.FinalDestination == null || DeclarationBO.FinalDestination.RL_RN_NKCountryCode.IsEmpty)
						{
							return RequiredTaxNumbers.GetRequiredTaxNumberWithType(ZString.Empty, DeclarationBO.Origin.RL_RN_NKCountryCode, InvoiceHeaderBO.Supplier_Effective, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.General);
						}

						return RequiredTaxNumbers.GetRequiredTaxNumberWithType(DeclarationBO.FinalDestination.RL_RN_NKCountryCode, DeclarationBO.Origin.RL_RN_NKCountryCode, InvoiceHeaderBO.Supplier_Effective, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.General);
					}

					return ZString.Empty;
				}
			}
		}

		protected virtual OrganisationWrapper GetSupplier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Supplier, InvoiceHeaderBO.EffectiveSupplierAddress, ContactType.Consignor, Factory);
		}

		public ZString ApprovalNumber
		{
			get { return CurrentCompany.Country.Code == Constants.CountryCodes.HongKong ? Supplier.MainAddress.AviationSecurity.ApprovalNumber : ZString.Empty; }
		}

		public ZString JobNumber
		{
			get
			{
				if (FreightJob != null && FreightJob.FreightShipment != null)
				{
					return FreightJob.FreightShipment.JS_UniqueConsignRef;
				}
				if (DeclarationBO != null)
				{
					return DeclarationBO.JE_DeclarationReference;
				}
				return ZString.Empty;
			}
		}

		public ValueAndUnitWrapper Volume
		{
			get
			{
				var decimalPlaces = GetDecimalPlaces(InvoiceHeaderBO.JZ_Volume, JobComInvoiceHeaderSchema.JZ_Volume.Name);
				if (decimalPlaces < 0)
				{
					decimalPlaces = 1;
				}

				return new ValueAndUnitWrapper(InvoiceHeaderBO.JZ_Volume, InvoiceHeaderBO.JZ_VolumeUQ, decimalPlaces, InvoiceHeaderBO.Lookups.JZ_VolumeUQ_List, Factory);
			}
		}

		public ValueAndUnitWrapper Weight
		{
			get
			{
				var decimalPlaces = GetDecimalPlaces(InvoiceHeaderBO.JZ_Weight, JobComInvoiceHeaderSchema.JZ_Weight.Name);
				if (decimalPlaces < 0)
				{
					decimalPlaces = 1;
				}

				return new ValueAndUnitWrapper(InvoiceHeaderBO.JZ_Weight, InvoiceHeaderBO.JZ_WeightUQ, decimalPlaces, InvoiceHeaderBO.Lookups.JZ_WeightUQ_List, Factory);
			}
		}

		int GetDecimalPlaces(ZDecimal value, string fieldName)
		{
			int metaData = InvoiceHeaderBO.GetDecimalPlacesMetaData(fieldName);

			return Math.Min(value.DecimalPlaces, metaData);
		}

		public ZDecimal ExchangeRate
		{
			get { return InvoiceHeaderBO.JZ_InvoiceCurrExRate; }
		}

		public ZDateTime InvoiceDate
		{
			get { return InvoiceHeaderBO.JZ_InvoiceDate; }
		}

		public ZString InvoiceNumber
		{
			get { return InvoiceHeaderBO.JZ_InvoiceNumber; }
		}

		public ZDateTime DateOfIssue
		{
			get { return !NameOfSignatory.IsEmpty ? InvoiceDate : Now; }
		}

		public ZString SignatoryCompany
		{
			get { return !NameOfSignatory.IsEmpty ? Supplier.CompanyName : CurrentBranch.BranchName; }
		}

		public ZString NameOfSignatory
		{
			get { return InvoiceHeaderBO.InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP)?.J2_ReferenceNumber ?? ZString.Empty; }
		}

		#region Collections

		public CommercialInvoiceLineWrapperCollection InvoiceLines
		{
			get
			{
				if (fInvoiceLines == null)
				{
					fInvoiceLines = new CommercialInvoiceLineWrapperCollection(InvoiceHeaderBO, Factory);
				}
				return fInvoiceLines;
			}
		}

		public CommercialInvoiceLineWrapperCollection UnclassifiedInvoiceLines
		{
			get
			{
				if (fUnclassifiedInvoiceLines == null)
				{
					fUnclassifiedInvoiceLines = new CommercialInvoiceLineWrapperCollection(InvoiceHeaderBO, Factory);
					fUnclassifiedInvoiceLines.RemoveClassifiedInoviceLines();
				}
				return fUnclassifiedInvoiceLines;
			}
		}

		#endregion

		#region Included and Excluded Charges
		#region Overseas Freight
		public MoneyWrapper IncludedOverseasFreight
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedOverseasFreight, Factory); }
		}

		public MoneyWrapper ExcludedOverseasFreight
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedOverseasFreight, Factory); }
		}
		#endregion

		#region Overseas Insurance
		public MoneyWrapper IncludedOverseasInsurance
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedOverseasInsurance, Factory); }
		}

		public MoneyWrapper ExcludedOverseasInsurance
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedOverseasInsurance, Factory); }
		}
		#endregion

		#region ExWorks Charges
		public MoneyWrapper IncludedExWorksCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedExWorks, Factory); }
		}

		public MoneyWrapper ExcludedExWorksCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedExWorks, Factory); }
		}
		#endregion

		#region Inland Freight
		public MoneyWrapper IncludedInlandFreight
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedForeignInlandFreight, Factory); }
		}

		public MoneyWrapper ExcludedInlandFreight
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedForeignInlandFreight, Factory); }
		}
		#endregion

		#region Packing Charges
		public MoneyWrapper IncludedPackingCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedPackingCosts, Factory); }
		}

		public MoneyWrapper ExcludedPackingCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedPackingCosts, Factory); }
		}
		#endregion

		#region Landing Charges
		public MoneyWrapper IncludedLandingCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedLandingCharges, Factory); }
		}

		public MoneyWrapper ExcludedLandingCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedLandingCharges, Factory); }
		}
		#endregion

		#region Dutiable Other Charges
		public MoneyWrapper IncludedDutiableOtherCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedOtherCharges1, Factory); }
		}

		public MoneyWrapper ExcludedDutiableOtherCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedOtherCharges1, Factory); }
		}
		#endregion

		#region Non-Dutiable Other Charges
		public MoneyWrapper IncludedNonDutiableOtherCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedOtherCharges2, Factory); }
		}

		public MoneyWrapper ExcludedNonDutiableOtherCharges
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedOtherCharges2, Factory); }
		}
		#endregion

		#region Commission
		public MoneyWrapper IncludedCommission
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedCommission, Factory); }
		}

		public MoneyWrapper ExcludedCommission
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedCommission, Factory); }
		}
		#endregion

		#region Discount
		public MoneyWrapper IncludedDiscount
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedDiscount, Factory); }
		}

		public MoneyWrapper ExcludedDiscount
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedDiscount, Factory); }
		}
		#endregion

		#region Charges Total
		public MoneyWrapper IncludedChargesTotal
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.IncludedTotalInInvoiceCurr, Factory); }
		}

		public MoneyWrapper ExcludedChargesTotal
		{
			get { return new MoneyWrapper(InvoiceHeaderBO.ExcludedTotalInInvoiceCurr, Factory); }
		}
		#endregion
		#endregion

		#region SDF Fields from Declaration

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			if (DeclarationBO != null)
			{
				return (BusinessObject)DeclarationBO.Shipment ?? DeclarationBO;
			}
			return base.GetParentBOForNoteStorageEDocsAndDocData();
		}

		public ZString AdditionalInformation
		{
			get
			{
				var result = InvoiceHeaderBO.JZ_Remarks;
				if (!InvoiceHeaderBO.AdditionalInformation.IsEmpty)
				{
					result = result + System.Environment.NewLine + InvoiceHeaderBO.AdditionalInformation;
				}
				return result;
			}
		}

		public ZString AdditionalPaymentTerms
		{
			get { return GetDocDataValueOnly(SDFields.AdditionalPaymentTerms); }
		}

		public ZString ExportersBankAccountNo => InvoiceHeaderBO.JZ_ExporterBankAccountNumber;

		public ZString ExportersBankName => InvoiceHeaderBO.JZ_ExporterBankName;

		public ZString ExportersBankSWIFTCode => InvoiceHeaderBO.JZ_ExporterBankSWIFTCode;

		public ZString InsurancePolicyNumber
		{
			get { return GetDocDataValueOnly(SDFields.InsurancePolicyNumber); }
		}

		public ZString InsuredValue
		{
			get
			{
				ZString result = GetDocDataValueOnly(SDFields.InsuredValue);
				return result.IsEmpty ? InvoiceAmount.AmountAndCurrencyCode : result;
			}
		}

		public ZString LetterOfCreditNumber
		{
			get
			{
				//StmSystemDefinedField LetterOfCreditNumber can be removed after '1 Jan 2024'.
				var result = GetDocDataValueOnly(SDFields.LetterOfCreditNumber);
				return result.IsEmpty ? InvoiceHeaderBO.JZ_LetterOfCreditNumber : result;
			}
		}

		public ZString LetterOfCreditDate
		{
			get
			{
				//StmSystemDefinedField LetterOfCreditDate can be removed after '1 Jan 2024'.
				var result = GetDocDataValueOnly(SDFields.LetterOfCreditDate);
				if (result.IsEmpty)
				{
					result = InvoiceHeaderBO.JZ_LetterOfCreditDate.ToShortDateString();
				}
				return result;
			}
		}
		#endregion

		#region US Certificate of Origin custom fields

		public ZString LocalChamberOfCommerceInfo
		{
			get { return DocumentsDataRegistry.Instance.LocalChamberOfCommerceInformation.Value; }
		}

		public ZString NotaryPublicInfo
		{
			get { return DocumentsDataRegistry.Instance.NotaryPublicInformation.Value; }
		}

		#endregion

		#region Implementation
		readonly BaseJobComInvoiceHeader InvoiceHeaderBO;
		CommercialInvoiceLineWrapperCollection fInvoiceLines;
		CommercialInvoiceLineWrapperCollection fUnclassifiedInvoiceLines;

		FreightWrapper ParentFreightJob
		{
			get
			{
				if (fParentFreightJob == null && DeclarationBO != null)
				{
					FreightWrapper[] wrappers = FreightWrapper.New(DeclarationBO, Factory);
					if (wrappers.Length == 1)
					{
						fParentFreightJob = wrappers[0];
					}
				}
				return fParentFreightJob;
			}
		}
		FreightWrapper fParentFreightJob;
		#endregion
	}
}
