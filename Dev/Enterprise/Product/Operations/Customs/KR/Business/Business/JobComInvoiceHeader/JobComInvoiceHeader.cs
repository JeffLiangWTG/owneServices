using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.KR.Business
{
	public partial class JobComInvoiceHeader : AutoKRJobComInvoiceHeader,
		ICusCodeDataTypeSupporter,
		ICusSupportingInfoTypeSupporter,
		Integration.Customs.KR.IJobComInvoiceHeader,
		ISupportingDocumentParent,
		ICurrencyConverterDataProvider
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoKRJobComInvoiceHeader.Schema
		{
			public const int JZ_NoOfPacksDecimalPlaces = 0;
			public const int JZ_InvoiceCurrLandedCostExRateDecimalPlaces = 4;
			public const int JZ_PaymentExRateDecimalPlaces = 4;
			public const int JZ_InvoiceCurrExRateDecimalPlaces = 4;
			public const int CustomsValueKRWDecimalPlaces = 0;
			public const int CustomsValueUSDDecimalPlaces = 0;

			public new const int JZ_PaymentTermsMaxLength = 2;
			public const int CertificateOfOriginIssueStatusMaxLength = 1;
			public const int CriteriaForDeterminingCountryOfOriginMaxLength = 1;
			public const int CertificateOfOriginNoMaxLength = 29;
			public const int CertificateOfOriginCriteriaCodeMaxLength = 2;
			public const int CertificateOfOriginIssuingCountryMaxLength = 2;
			public const int CertificateOfOriginAgencyNameMaxLength = 60;
			public const int CertificateOfOriginAreaNameMaxLength = 30;
			public const int CertificateOfOriginPersonNameMaxLength = 60;
			public const int CertificateOfOriginStatusMaxLength = 1;

			public const int JZRemarksMaxLength = 600;
			public const int JZ_OnlineTradeTypeMaxLength = 1;
			public const int JZ_IsProvPricingMaxLength = 1;
			public const int YNMaxLength = 1;
			public const int ValuationQuestion5BMaxLength = 2;
			public const int ValuationQuestion5EAMaxLength = 2;
			public const int ValuationQuestion5EBMaxLength = 50;
			public const int JZ_DeductionTypeMaxLength = 1;
			public const int CustomsReferenceNumberMaxLength = 30;
			public const int ValuationQuestion7EAMaxLength = 2;
			public const int ValuationQuestion7BMaxLength = 2;
		}
		protected override ZString LocalCurrencyCodeCore
		{
			get { return Core.Constants.CurrencyCodes.KoreaRepublicOf; }
		}

		#region Certificate of Origin
		[ChildEditable(true)]
		public CertificateOfOriginCollection CertificateOfOriginCollection
		{
			get
			{
				if (certificateOfOriginCollection == null)
				{
					certificateOfOriginCollection = new CertificateOfOriginCollection(this);
					certificateOfOriginCollection.Load();
					RegisterEditableChildObject(certificateOfOriginCollection);
				}
				return certificateOfOriginCollection;
			}
		}
		CertificateOfOriginCollection certificateOfOriginCollection;

		CertificateOfOrigin CertificateOfOriginData
		{
			get
			{
				if (certificateOfOriginData?.IsDeleted ?? true)
				{
					certificateOfOriginData = CertificateOfOriginCollection.FirstOrDefault();
				}
				return certificateOfOriginData;
			}
		}
		CertificateOfOrigin certificateOfOriginData;

		void CreateCertificateOfOriginDataIfRequired()
		{
			if (CertificateOfOriginData == null)
			{
				CertificateOfOriginCollection.AddNew();
			}
		}
		[ResourceStringData("DB8506C4-5CD3-4979-ABCD-5DACAE5B4193", Caption = "C/O Issued")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CertificateOfOriginIssuedCodeList))]
		[MaxLength(Schema.CertificateOfOriginIssueStatusMaxLength)]
		public ZString CertificateOfOriginIssueStatus
		{
			get
			{
				return CertificateOfOriginData?.CSI_Code ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Code = value;

				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginIssueStatus.IsEmpty)
					{
						line.CertificateOfOriginIssueStatus = value;
					}
				}
				CertificateOfOriginIssueStatusInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginIssueStatusInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginIssueStatus)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginIssueStatus), x => CertificateOfOriginData.CSI_CodeInfo); }
		}

		[ResourceStringData("1D9B67D3-1289-4697-A0F2-D94A14759B49", Caption = "C/O Determination Rule")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryOfOriginDeterminationRuleCodeList))]
		[MaxLength(Schema.CriteriaForDeterminingCountryOfOriginMaxLength)]
		public ZString CriteriaForDeterminingCountryOfOrigin
		{
			get
			{
				return CertificateOfOriginData?.CSI_SubType ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_SubType = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CriteriaForDeterminingCountryOfOrigin.IsEmpty)
					{
						line.CriteriaForDeterminingCountryOfOrigin = value;
					}
				}
				CriteriaForDeterminingCountryOfOriginInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CriteriaForDeterminingCountryOfOriginInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CriteriaForDeterminingCountryOfOrigin)) : GetWrappedZPropertyInfo(nameof(CriteriaForDeterminingCountryOfOrigin), x => CertificateOfOriginData.CSI_SubTypeInfo); }
		}
		[ResourceStringData("433A526C-EF97-4736-8623-F23A7C5FC289", Caption = "C/O Issuing Country")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryCollection))]
		[MaxLength(Schema.CertificateOfOriginIssuingCountryMaxLength)]
		public ZString CertificateOfOriginIssuingCountry
		{
			get
			{
				return CertificateOfOriginData?.CSI_RN_NKCountryCode ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_RN_NKCountryCode = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginIssuingCountry.IsEmpty)
					{
						line.CertificateOfOriginIssuingCountry = value;
					}
				}
				CertificateOfOriginIssuingCountryInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginIssuingCountryInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginIssuingCountry)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginIssuingCountry), x => CertificateOfOriginData.CSI_RN_NKCountryCodeInfo); }
		}
		[ResourceStringData("67A6219C-8313-43BE-A98C-0B73F9719D50", Caption = "C/O Issue Date")]
		public ZDateTime CertificateOfOriginIssueDate
		{
			get
			{
				return CertificateOfOriginData?.CSI_DateOfIssue ?? ZDateTime.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_DateOfIssue = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginIssueDate.IsEmpty)
					{
						line.CertificateOfOriginIssueDate = value;
					}
				}
				CertificateOfOriginIssueDateInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginIssueDateInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginIssueDate)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginIssueDate), x => CertificateOfOriginData.CSI_DateOfIssueInfo); }
		}

		[ResourceStringData("D8463CBE-8326-4A0A-B2DE-126A6FD429A6", Caption = "C/O Reference Number")]
		[MaxLength(Schema.CertificateOfOriginNoMaxLength)]
		public ZString CertificateOfOriginNo
		{
			get
			{
				return CertificateOfOriginData?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_ReferenceNumber = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginNo.IsEmpty)
					{
						line.CertificateOfOriginNo = value;
					}
				}
				CertificateOfOriginNoInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginNoInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginNo)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginNo), x => CertificateOfOriginData.CSI_ReferenceNumberInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryOfOriginDeterminationRuleCodeList))]
		[MaxLength(Schema.CertificateOfOriginCriteriaCodeMaxLength)]
		[ResourceStringData("B3DE0BEC-797E-49C0-B149-A767B2AC4C31", Caption = "C/O Criteria Code")]
		public ZString CertificateOfOriginCriteriaCode
		{
			get
			{
				return CertificateOfOriginData?.CSI_Procedure ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Procedure = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginCriteriaCode.IsEmpty)
					{
						line.CertificateOfOriginCriteriaCode = value;
					}
				}
				CertificateOfOriginCriteriaCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginCriteriaCodeInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginCriteriaCode)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginCriteriaCode), x => CertificateOfOriginData.CSI_ProcedureInfo); }
		}

		[MaxLength(Schema.CertificateOfOriginAgencyNameMaxLength)]
		[ResourceStringData("8A413D2F-56A8-4500-B917-7CA7DB804EEB", Caption = "C/O Issuing Agency Name")]
		public ZString CertificateOfOriginAgencyName
		{
			get
			{
				return CertificateOfOriginData?.CSI_Description ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Description = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginAgencyName.IsEmpty)
					{
						line.CertificateOfOriginAgencyName = value;
					}
				}
				CertificateOfOriginAgencyNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginAgencyNameInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginAgencyName)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginAgencyName), x => CertificateOfOriginData.CSI_DescriptionInfo); }
		}

		[ResourceStringData("8402F27D-9B06-4C56-A334-65E832C46952", Caption = "C/O Issuing Area Name")]
		[MaxLength(Schema.CertificateOfOriginAreaNameMaxLength)]
		public ZString CertificateOfOriginAreaName
		{
			get
			{
				return CertificateOfOriginData?.CSI_AdditionalDescription ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_AdditionalDescription = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginAreaName.IsEmpty)
					{
						line.CertificateOfOriginAreaName = value;
					}
				}
				CertificateOfOriginAreaNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginAreaNameInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginAreaName)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginAreaName), x => CertificateOfOriginData.CSI_AdditionalDescriptionInfo); }
		}

		[ResourceStringData("FE4C7408-3B49-4789-97B3-E98ED240760A", Caption = "C/O Issuing Person Name")]
		[MaxLength(Schema.CertificateOfOriginPersonNameMaxLength)]
		public ZString CertificateOfOriginPersonName
		{
			get
			{
				return CertificateOfOriginData?.CSI_ReferenceNumber2 ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_ReferenceNumber2 = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginPersonName.IsEmpty)
					{
						line.CertificateOfOriginPersonName = value;
					}
				}
				CertificateOfOriginPersonNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginPersonNameInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginPersonName)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginPersonName), x => CertificateOfOriginData.CSI_ReferenceNumber2Info); }
		}

		[ResourceStringData("91C048CF-DDE7-46DF-AD00-F07BAEF3077A", Caption = "C/O Split Y/N")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CertificateOfOriginSplitCodeList))]
		[MaxLength(Schema.CertificateOfOriginStatusMaxLength)]
		public ZString CertificateOfOriginStatus
		{
			get
			{
				return CertificateOfOriginData?.CSI_Status ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Status = value;
				foreach (JobComInvoiceLine line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (line.CertificateOfOriginStatus.IsEmpty)
					{
						line.CertificateOfOriginStatus = value;
					}
				}
				CertificateOfOriginStatusInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginStatusInfo
		{
			get { return CertificateOfOriginData == null ? GetZPropertyInfo(nameof(CertificateOfOriginStatus)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginStatus), x => CertificateOfOriginData.CSI_StatusInfo); }
		}
		#endregion

		[ResourceStringData("4632773A-FDE3-4C23-ABF9-9E836B82FBC8", Caption = "Supporting Document Type", ShortCaption = "Support Doc. Type")]
		[MaxLength(SupportingDocument.Schema.SUP_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.LocalExportDocumentTypeList))]
		public ZString SupportingDocumentCode
		{
			get
			{
				return SupportingDocument?.CSI_Code ?? ZString.Empty;
			}
			set
			{
				if (SupportingDocument == null)
				{
					SupportingDocumentCollection.AddNew();
				}
				SupportingDocument.CSI_Code = value;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					if (line.SupportingDocumentCode.IsEmpty)
					{
						line.SupportingDocumentCode = value;
					}
				}
				SupportingDocumentCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SupportingDocumentCodeInfo
		{
			get { return SupportingDocument == null ? GetZPropertyInfo(nameof(SupportingDocumentCode)) : GetWrappedZPropertyInfo(nameof(SupportingDocumentCode), x => SupportingDocument.CSI_CodeInfo); }
		}

		[MaxLength(SupportingDocument.Schema.SUP_ReferenceNumberMaxLength)]
		[ResourceStringData("E2ACC5BC-4FB0-4209-8EFC-BC270C75CB17", Caption = "Supporting Document No.", ShortCaption = "Support Doc. No.")]
		public ZString SupportingDocumentReferenceNumber
		{
			get
			{
				return SupportingDocument?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				if (SupportingDocument == null)
				{
					SupportingDocumentCollection.AddNew();
				}
				SupportingDocument.CSI_ReferenceNumber = value;
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					if (line.SupportingDocumentReferenceNumber.IsEmpty)
					{
						line.SupportingDocumentReferenceNumber = value;
					}
				}
				SupportingDocumentReferenceNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SupportingDocumentReferenceNumberInfo
		{
			get { return SupportingDocument == null ? GetZPropertyInfo(nameof(SupportingDocumentReferenceNumber)) : GetWrappedZPropertyInfo(nameof(SupportingDocumentReferenceNumber), x => SupportingDocument.CSI_ReferenceNumberInfo); }
		}

		[MaxLength(SupportingDocument.Schema.ValuationDocumentMaxLength)]
		[ResourceStringData("FD592F49-0EA9-4B33-8B00-1CF98B0C049D", Caption = "Supporting Document 1")]
		public ZString ValuationSupportingDocument1
		{
			get => ValuationSupportingDoc1?.CSI_Description ?? ZString.Empty;
			set
			{
				if (ValuationSupportingDoc1 == null)
				{
					var doc = SupportingDocumentCollection.AddNew();
					doc.CSI_LineNo = 1;
				}
				ValuationSupportingDoc1.CSI_Description = value;
			}
		}

		[MaxLength(SupportingDocument.Schema.ValuationDocumentMaxLength)]
		[ResourceStringData("9B1A3A16-4674-4095-BD75-F3B6D5F5E458", Caption = "Supporting Document 2")]
		public ZString ValuationSupportingDocument2
		{
			get => ValuationSupportingDoc2?.CSI_Description ?? ZString.Empty;
			set
			{
				if (ValuationSupportingDoc2 == null)
				{
					var doc = SupportingDocumentCollection.AddNew();
					doc.CSI_LineNo = 2;
				}
				ValuationSupportingDoc2.CSI_Description = value;
			}
		}
		SupportingDocument ValuationSupportingDoc1
		{
			get
			{
				if (valuationSupportingDoc1 == null)
				{
					valuationSupportingDoc1 = SupportingDocumentCollection.Where(x => x.CSI_LineNo == 1).FirstOrDefault();
				}
				return valuationSupportingDoc1;
			}
		}
		SupportingDocument valuationSupportingDoc1;
		SupportingDocument ValuationSupportingDoc2
		{
			get
			{
				if (valuationSupportingDoc2 == null)
				{
					valuationSupportingDoc2 = SupportingDocumentCollection.Where(x => x.CSI_LineNo == 2).FirstOrDefault();
				}
				return valuationSupportingDoc2;
			}
		}
		SupportingDocument valuationSupportingDoc2;

		[ChildEditable(true)]
		SupportingDocumentCollection SupportingDocumentCollection
		{
			get
			{
				if (supportingDocumentCollection == null)
				{
					supportingDocumentCollection = new SupportingDocumentCollection(this);
					supportingDocumentCollection.Load();
					RegisterEditableChildObject(supportingDocumentCollection);
				}
				return supportingDocumentCollection;
			}
		}
		SupportingDocumentCollection supportingDocumentCollection;
		SupportingDocument SupportingDocument => SupportingDocumentCollection.FirstOrDefault();

		[ChildEditable(true)]
		ContractCollection Contracts
		{
			get
			{
				if (contracts == null)
				{
					contracts = new ContractCollection(this);
					contracts.Load();
					RegisterEditableChildObject(contracts);
				}
				return contracts;
			}
		}
		ContractCollection contracts;

		Contract ContractData
		{
			get
			{
				if (contractData?.IsDeleted ?? true)
				{
					contractData = Contracts.FirstOrDefault();
				}
				return contractData;
			}
		}
		Contract contractData;

		[MaxLength(Contract.Schema.CY_Data_MaxLength)]
		[ResourceStringData("27A79976-8526-4E05-94C2-86028E9DED0D", Caption = "Contract No.")]
		public ZString ContractNumber
		{
			get => ContractData?.CY_Data ?? ZString.Empty;
			set
			{
				ContractDataIfRequired();
				ContractData.CY_Data = value;
				ContractNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ContractNumberInfo => GetZPropertyInfo(nameof(ContractNumber));

		[ResourceStringData("AD3FC413-EDB3-4480-9C80-88A1FF46305B", Caption = "Contract Date")]
		public ZDateTime ContractDate
		{
			get => ContractData?.CY_Date ?? ZDateTime.Empty;
			set
			{
				ContractDataIfRequired();
				ContractData.CY_Date = value;
				ContractDateInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ContractDateInfo => GetZPropertyInfo(nameof(ContractDate));

		void ContractDataIfRequired()
		{
			if (ContractData == null)
			{
				Contracts.AddNew();
			}
		}

		[ChildEditable(true)]
		PurchaseOrderCollection PurchaseOrders
		{
			get
			{
				if (purchaseOrders == null)
				{
					purchaseOrders = new PurchaseOrderCollection(this);
					purchaseOrders.Load();
					RegisterEditableChildObject(purchaseOrders);
				}
				return purchaseOrders;
			}
		}
		PurchaseOrderCollection purchaseOrders;

		PurchaseOrder PurchaseOrderData
		{
			get
			{
				if (purchaseOrderData?.IsDeleted ?? true)
				{
					purchaseOrderData = PurchaseOrders.FirstOrDefault();
				}
				return purchaseOrderData;
			}
		}
		PurchaseOrder purchaseOrderData;

		[MaxLength(PurchaseOrder.Schema.CY_Data_MaxLength)]
		[ResourceStringData("D513FF91-70EE-4B4D-B7B4-4C828D47606A", ShortCaption = "P/O No", Caption = "Purchase Order No.")]
		public ZString PurchaseOrderNumber
		{
			get => PurchaseOrderData?.CY_Data ?? ZString.Empty;
			set
			{
				PurchaseOrderDataIfRequired();
				PurchaseOrderData.CY_Data = value;
				PurchaseOrderNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PurchaseOrderNumberInfo => GetZPropertyInfo(nameof(PurchaseOrderNumber));

		[ResourceStringData("30813DCD-AC5D-4A59-A670-DFB40F7E9263", ShortCaption = "P/O Date", Caption = "Purchase Order Date")]
		public ZDateTime PurchaseOrderDate
		{
			get => PurchaseOrderData?.CY_Date ?? ZDateTime.Empty;
			set
			{
				PurchaseOrderDataIfRequired();
				PurchaseOrderData.CY_Date = value;
				PurchaseOrderDateInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PurchaseOrderDateInfo => GetZPropertyInfo(nameof(PurchaseOrderDate));

		void PurchaseOrderDataIfRequired()
		{
			if (PurchaseOrderData == null)
			{
				PurchaseOrders.AddNew();
			}
		}

		[ChildEditable(true)]
		public ValuationDeclarationCodeCollection ValuationDeclarationCodes
		{
			get
			{
				if (valuationDeclarationCodes == null)
				{
					valuationDeclarationCodes = new ValuationDeclarationCodeCollection(this);
					valuationDeclarationCodes.Load();
					RegisterEditableChildObject(valuationDeclarationCodes);
				}
				return valuationDeclarationCodes;
			}
		}
		ValuationDeclarationCodeCollection valuationDeclarationCodes;

		[ChildEditable(true)]
		public ValuationQuestionCollection ValuationQuestions
		{
			get
			{
				if (valuationQuestions == null)
				{
					valuationQuestions = new ValuationQuestionCollection(this);
					valuationQuestions.Load();
					RegisterEditableChildObject(valuationQuestions);
				}
				return valuationQuestions;
			}
		}
		ValuationQuestionCollection valuationQuestions;

		[ChildEditable(true)]
		public ParcelCollection Parcels
		{
			get
			{
				if (parcels == null)
				{
					parcels = new ParcelCollection(this);
					parcels.Load();
					RegisterEditableChildObject(parcels);
				}
				return parcels;
			}
		}
		ParcelCollection parcels;

		[MaxLength(Schema.JZRemarksMaxLength)]
		[ResourceStringData("043150CC-3571-4DC9-AB0E-19092F51B6DF", Caption = "Declarant Desc.")]
		[ResourceStringData("589AD9B9-6199-4A74-9E2D-705900EEEDFC", Caption = "Customs Broker Comment", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JZ_Remarks
		{
			get => base.JZ_Remarks;
			set => base.JZ_Remarks = value;
		}

		[ResourceStringData("387C1D24-21DA-47FE-BDFD-601B98D61B7E", Caption = "Valuation Code")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationCodeList))]
		public override ZString JZ_ValuationCode
		{
			get => base.JZ_ValuationCode;
			set
			{
				base.JZ_ValuationCode = value;

				if (Is5SM)
				{
					switch (JZ_ValuationCode)
					{
						case ValuationCodeList.Codes.MethodOne:
							ValuationDeclarationCodes.RemoveAndDeleteAll();
							if (!ValuationQuestions.Any())
							{
								PopulateValuationQuestionsForMethodOne();
								ValuationQuestion5BInfo.RefreshBinding();
								ValuationQuestion5CInfo.RefreshBinding();
								ValuationQuestion5DInfo.RefreshBinding();
								ValuationQuestion5EAInfo.RefreshBinding();
								ValuationQuestion5EBInfo.RefreshBinding();
							}
							break;
						case ValuationCodeList.Codes.MethodTwo:
						case ValuationCodeList.Codes.MethodThree:
						case ValuationCodeList.Codes.MethodFourA:
						case ValuationCodeList.Codes.MethodFourB:
						case ValuationCodeList.Codes.MethodFive:
						case ValuationCodeList.Codes.MethodSix:
							ValuationQuestions.RemoveAndDeleteAll();
							if (!ValuationDeclarationCodes.Any())
							{
								PopulateValuationDeclarationCodes();
							}
							break;
					}
				}
				else if (IsImport)
				{
					if (JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
					{
						var provisionalPricingReasonCodeList = ValuationDeclarationCodes.Where(x => x.CY_Code.StartsWith(PriceDeclarationItemCodeList.StartingDigits.ProvisionalPricingReasonCode));
						foreach (var valuationDeclarationCode in ValuationDeclarationCodes.Cast<ValuationDeclarationCode>().ToList())
						{
							if (!provisionalPricingReasonCodeList.Contains(valuationDeclarationCode))
							{
								ValuationDeclarationCodes.RemoveAndDelete(valuationDeclarationCode);
							}
						}

						PopulateValuationQuestionsForMethodOne();
						ValuationQuestion7B_IMPInfo.RefreshBinding();
						ValuationQuestion7CInfo.RefreshBinding();
						ValuationQuestion7DInfo.RefreshBinding();
						ValuationQuestion7EAInfo.RefreshBinding();
						ValuationQuestion7EBInfo.RefreshBinding();
					}
				}
			}
		}

		[ResourceStringData("79BEBE43-6D5C-4383-9307-1AD3A0704993", Caption = "Address")]
		public override ZGuid JZ_OA_DistributorAddress
		{
			get => base.JZ_OA_DistributorAddress;
			set => base.JZ_OA_DistributorAddress = value;
		}
		[ResourceStringData("BFA19345-BFB6-4098-9450-4D4BA56C0A47", Caption = "Online Trade Distributor", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZGuid DistributorOrgPK
		{
			get => base.DistributorOrgPK;
			set => base.DistributorOrgPK = value;
		}
		[ResourceStringData("8C882AA4-899D-4E2B-8480-8470C09F1E63", Caption = "Address")]
		public override ZGuid JZ_OA_SellerAddress
		{
			get => base.JZ_OA_SellerAddress;
			set => base.JZ_OA_SellerAddress = value;
		}
		[ResourceStringData("5F84A8A5-8DA6-443F-99B8-68C30C9DB802", Caption = "Online Trade Seller", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZGuid SellerOrgPK
		{
			get => base.SellerOrgPK;
			set => base.SellerOrgPK = value;
		}
		[ResourceStringData("8F0C2548-4D95-4E2B-B745-99B66192744D", Caption = "Online Trade Selling Agent")]
		public override ZGuid JZ_OH_SellingAgent
		{
			get => base.JZ_OH_SellingAgent;
			set => base.JZ_OH_SellingAgent = value;
		}

		[DecimalPlaces(Schema.JZ_InvoiceCurrLandedCostExRateDecimalPlaces)]
		public override ZDecimal JZ_InvoiceCurrLandedCostExRate
		{
			get => base.JZ_InvoiceCurrLandedCostExRate;
			set => base.JZ_InvoiceCurrLandedCostExRate = value;
		}

		[DecimalPlaces(Schema.JZ_PaymentExRateDecimalPlaces)]
		public override ZDecimal JZ_PaymentExRate
		{
			get => base.JZ_PaymentExRate;
			set => base.JZ_PaymentExRate = value;
		}

		[DecimalPlaces(Schema.JZ_NoOfPacksDecimalPlaces)]
		public override ZDecimal JZ_NoOfPacks
		{
			get => base.JZ_NoOfPacks;
			set => base.JZ_NoOfPacks = value;
		}

		[ResourceStringData("704E76DC-1B29-456A-A461-1C9D9DC578AA", Caption = "Payment Method")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.InvoicePaymentTermCodeList))]
		[MaxLength(Schema.JZ_PaymentTermsMaxLength)]
		public override ZString JZ_PaymentTerms
		{
			get => base.JZ_PaymentTerms;
			set => base.JZ_PaymentTerms = value;
		}

		[ResourceStringData("39708E24-4C03-475D-9708-CAF1EB12410A", Caption = "L/C No.")]
		public override ZString JZ_LetterOfCreditNumber
		{
			get => base.JZ_LetterOfCreditNumber;
			set => base.JZ_LetterOfCreditNumber = value;
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				var oldValue = JZ_RX_NKInvoice_Currency;
				base.JZ_RX_NKInvoice_Currency = value;
				if (!IsCopying && oldValue != JZ_RX_NKInvoice_Currency && !IsValidationSuspended)
				{
					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						line.MarkAsNeedingValidation();
					}
				}
			}
		}
		int ICurrencyConverterDataProvider.MaximumDaysToFallback => ((ICurrencyConverterDataProvider)JobDeclaration)?.MaximumDaysToFallback ?? 0;

		[ResourceStringData("68E0E6AC-F4BA-4C30-B41C-19946E9253C8", Caption = "Goods Origin")]
		public override ZString JZ_RN_NKDefaultOrigin
		{
			get => base.JZ_RN_NKDefaultOrigin;
			set
			{
				var oldValue = JZ_RN_NKDefaultOrigin;
				base.JZ_RN_NKDefaultOrigin = value;

				if (oldValue != JZ_RN_NKDefaultOrigin)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.JI_CountryOfOrigin.IsEmpty)
						{
							invoiceLine.JI_CountryOfOrigin = JZ_RN_NKDefaultOrigin;
						}
					}
				}
			}
		}

		[ResourceStringData("CB194F48-E32A-41F2-A704-22E0F179787E", Caption = "Invoice Date")]
		public override ZDateTime JZ_InvoiceDate { get => base.JZ_InvoiceDate; set => base.JZ_InvoiceDate = value; }

		[ResourceStringData("444C07A7-74D8-4B98-964F-7B3C6A27FD56", Caption = "Invoice No.")]
		public override ZString JZ_InvoiceNumber { get => base.JZ_InvoiceNumber; set => base.JZ_InvoiceNumber = value; }

		[ResourceStringData("1E0BB3FB-5881-4156-8332-065723B0F169", Caption = "C/O Label Location")]
		[ResourceStringData("23487FC3-7F1D-4A4E-978F-4177901F8C6C", Caption = "C/O Label Location", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryOfOriginLabelLocationCodeList))]
		public override ZString JZ_COOLabelLocation
		{
			get => base.JZ_COOLabelLocation;
			set
			{
				var oldValue = JZ_COOLabelLocation;
				base.JZ_COOLabelLocation = value;

				if (oldValue != JZ_COOLabelLocation)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.JI_COOLabelLocation.IsEmpty)
						{
							invoiceLine.JI_COOLabelLocation = JZ_COOLabelLocation;
						}
					}
				}
			}
		}

		[ResourceStringData("2CE933A8-E84A-4B31-9083-C2138BFBDA88", Caption = "C/O Label Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryOfOriginLabelTypeCodeList))]
		public override ZString JZ_COOLabelType
		{
			get => base.JZ_COOLabelType;
			set
			{
				var oldValue = JZ_COOLabelType;
				base.JZ_COOLabelType = value;

				if (oldValue != JZ_COOLabelType)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.JI_COOLabelType.IsEmpty)
						{
							invoiceLine.JI_COOLabelType = JZ_COOLabelType;
						}
					}
				}
			}
		}

		[ResourceStringData("D37E76FF-CF34-4199-BB4D-BDD04481DAC2", Caption = "C/O Label Exemption Reason")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryOfOriginExemptionReasonCodeList))]
		public override ZString JZ_COOExemptionReason
		{
			get => base.JZ_COOExemptionReason;
			set
			{
				var oldValue = JZ_COOExemptionReason;
				base.JZ_COOExemptionReason = value;

				if (oldValue != JZ_COOExemptionReason)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.JI_COOExemptionReason.IsEmpty)
						{
							invoiceLine.JI_COOExemptionReason = JZ_COOExemptionReason;
						}
					}
				}
			}
		}

		[ResourceStringData("576765B6-D279-4491-B952-D6A6706F85D3", Caption = "Cargo Management No.")]
		[ResourceStringData("EBC90D7E-B8E1-41EA-BDC6-C03BECF640D2", Caption = "Cargo Management No.", MultipleKey = ElectronicDocumentTypeList.Codes._D87)]
		[ResourceStringData("2B86FCD1-CA87-45AB-9AB2-CF5D5145DAD8", Caption = "Cargo Management No.", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JZ_ImportCargoManagementNumber
		{
			get { return base.JZ_ImportCargoManagementNumber; }
			set { base.JZ_ImportCargoManagementNumber = value; }
		}

		[ResourceStringData("C6A36B86-5FF1-4771-9DE5-539708F2863E", Caption = "C/O Status")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.COOIssuedCodeList))]
		public override ZString JZ_COOStatus { get => base.JZ_COOStatus; set => base.JZ_COOStatus = value; }

		[ResourceStringData("424CF12A-2D7D-4CE1-822F-22DBB4DBB1A6", Caption = "Valuation Declaration Status", ShortCaption = "Valuation Dec. Status")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValueDeclarationAttachedCodeList))]
		public override ZString JZ_ValuationDecAttachCode { get => base.JZ_ValuationDecAttachCode; set => base.JZ_ValuationDecAttachCode = value; }

		[ResourceStringData("72551B7A-BD67-453E-930B-2C35AA735483", Caption = "5SM No.", FullDescription = "Blanket Valuation Declaration No.")]
		public override ZString JZ_BlanketValuationDeclarationNumber { get => base.JZ_BlanketValuationDeclarationNumber; set => base.JZ_BlanketValuationDeclarationNumber = value; }

		[ResourceStringData("09C09405-7277-4911-97EF-26EAA5D7D544", Caption = "Provisional Pricing Y/N")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.YesNoCodeList))]
		[MaxLength(Schema.JZ_IsProvPricingMaxLength)]
		public override ZString JZ_ProvPricingYN { get => base.JZ_ProvPricingYN; set => base.JZ_ProvPricingYN = value; }
		public ZBool IsProvPricing => JZ_ProvPricingYN == YesNoList.Codes.Yes;

		[ResourceStringData("C7EE87CC-2B77-4EC4-B248-B9E12D873CF3", Caption = "Provisional Additional Rate")]
		[DecimalPlaces(DecimalPlacesConstants.ProvAdditionalRate)]
		public override ZDecimal JZ_ProvAdditionalRate { get => base.JZ_ProvAdditionalRate; set => base.JZ_ProvAdditionalRate = value; }

		[ResourceStringData("962C5BED-4859-46D4-9D83-E7A9DAF60014", Caption = "Provisional Additional Amount")]
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public override ZDecimal JZ_ProvAdditionalAmount { get => base.JZ_ProvAdditionalAmount; set => base.JZ_ProvAdditionalAmount = value; }

		[ResourceStringData("45D3522D-7E71-4453-92DE-1DC282A083D2", Caption = "Contract Expiration Date")]
		public override ZDateTime JZ_ImpContractExpiryDate { get => base.JZ_ImpContractExpiryDate; set => base.JZ_ImpContractExpiryDate = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CustomsOfficeList))]
		public override ZString JZ_ScheduledReExportCustomsOffice { get => base.JZ_ScheduledReExportCustomsOffice; set => base.JZ_ScheduledReExportCustomsOffice = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CustomsOfficeList))]
		public override ZString JZ_JurisdictionalCusOffice { get => base.JZ_JurisdictionalCusOffice; set => base.JZ_JurisdictionalCusOffice = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.GoodsDestination))]
		public override ZString JZ_RN_NKReExportDestinationCountry { get => base.JZ_RN_NKReExportDestinationCountry; set => base.JZ_RN_NKReExportDestinationCountry = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SpecificUseProductTypeList))]
		public override ZString JZ_SpecificUseProductType { get => base.JZ_SpecificUseProductType; set => base.JZ_SpecificUseProductType = value; }

		[ResourceStringData("11ABEC7F-3677-4260-814B-326B77DD8508", Caption = "Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("36DEB049-3403-44A8-A04C-4C29F865C9C7", Caption = "Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("43D4E88B-FB80-4DCE-8ABD-D8BC13845BE3", Caption = "Total Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[DecimalPlaces(Schema.CustomsValueKRWDecimalPlaces)]
		public ZDecimal CustomsValueKRW
		{
			get
			{
				if (customsValueKRW == null)
				{
					customsValueKRW = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var krwCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
						return CurrencyConverter.ConvertExact(new Money(JZ_Calc_FOBAmount, Invoice_Currency), krwCurrency, false).Amount.Truncate();
					});
				}
				return customsValueKRW.Value;
			}
		}
		CachedProperty<ZDecimal> customsValueKRW;

		[ResourceStringData("11EF79A7-34C5-4209-B64A-765402D73917", Caption = "Customs Value (USD)")]
		[DecimalPlaces(Schema.CustomsValueUSDDecimalPlaces)]
		public ZDecimal CustomsValueUSD
		{
			get
			{
				if (customsValueUSD == null)
				{
					customsValueUSD = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var krwCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
						var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);

						return CurrencyConverter.ConvertExact(new Money(CustomsValueKRW, krwCurrency), usdCurrency).Amount.Round(0);
					});
				}
				return customsValueUSD.Value;
			}
		}
		CachedProperty<ZDecimal> customsValueUSD;

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.SupportingDoc, typeof(SupportingDocument) },
				{ CusSupportingInfoTypeList.Codes.CertificateOfOrigin, typeof(CertificateOfOrigin) },
				{ CusSupportingInfoTypeList.Codes.Parcel, typeof(Parcel) }
			};
			return result;
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.Contract, typeof(Contract) },
				{ CusCodeDataTypeList.Codes.PurchaseOrder, typeof(PurchaseOrder) },
				{ CusCodeDataTypeList.Codes.ValuationDeclarationCode, typeof(ValuationDeclarationCode) },
				{ CusCodeDataTypeList.Codes.ValuationQuestion, typeof(ValuationQuestion) }
			};
		}
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);
		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		[ResourceStringData("B60723BB-1698-4E52-991E-221C61BB7256", Caption = "Supplier")]
		public override ZGuid JZ_OH_Supplier
		{
			get => base.JZ_OH_Supplier;
			set => base.JZ_OH_Supplier = value;
		}

		[ResourceStringData("A73AF78C-B1C2-41B4-9EA5-55908DC61451", Caption = "Supplier")]
		public override ZGuid SupplierOrgPK
		{
			get => base.SupplierOrgPK;
			set => base.SupplierOrgPK = value;
		}

		public override ZGuid JZ_OA_BuyerAddress
		{
			get => base.JZ_OA_BuyerAddress;
			set
			{
				ZGuid jZ_OA_BuyerAddress = JZ_OA_BuyerAddress;
				base.JZ_OA_BuyerAddress = value;
				if (!IsCopying && jZ_OA_BuyerAddress != JZ_OA_BuyerAddress)
				{
					JZ_OH_Buyer = ((!value.IsValid) ? ZGuid.Empty : base.JZ_OA_BuyerAddress_ZAddress.OrgPK);
				}
			}
		}

		public override ZGuid JZ_OH_Buyer
		{
			get => base.JZ_OH_Buyer;
			set
			{
				base.JZ_OH_Buyer = value;
				if (base.JZ_OH_Buyer != base.JZ_OA_BuyerAddress_ZAddress.OrgPK)
				{
					JZ_OA_BuyerAddress = ZGuid.Empty;
				}
			}
		}

		[ResourceStringData("FD9D4290-CBF0-49ED-A50A-1F10D34AB48E", Caption = "Address")]
		public override ZGuid JZ_OA_SupplierAddress
		{
			get => base.JZ_OA_SupplierAddress;
			set => base.JZ_OA_SupplierAddress = value;
		}

		[ResourceStringData("B5257FFC-B1F0-4074-AA28-355FF1F2C180", Caption = "Address")]
		public override ZGuid JZ_OA_ShipperAddress
		{
			get => base.JZ_OA_ShipperAddress;
			set => base.JZ_OA_ShipperAddress = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		[ResourceStringData("1E68A7DC-7F9E-4CD0-B1F0-511294D64BB9", Caption = "Manufacturer")]
		public override ZGuid JZ_OH_Manufacturer
		{
			get => base.JZ_OH_Manufacturer;
			set => base.JZ_OH_Manufacturer = value;
		}

		[ResourceStringData("3C1938C7-1ACE-47B7-8FAD-1798F61133C8", Caption = "Address")]
		[ResourceStringData("FA224872-5FCB-4365-BB88-8BF9C06134DF", Caption = "Address", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZGuid JZ_OA_ManufacturerAddress
		{
			get => base.JZ_OA_ManufacturerAddress;
			set => base.JZ_OA_ManufacturerAddress = value;
		}

		[ResourceStringData("F00CC76C-3C8D-4479-8AD0-7DB8958E4F68", Caption = "Manufacturer")]
		[ResourceStringData("F06CF823-B63B-4611-BAE9-799B80C43CAA", Caption = "Manufacturer", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZGuid ManufacturerOrgPK
		{
			get => base.ManufacturerOrgPK;
			set => base.ManufacturerOrgPK = value;
		}

		[ResourceStringData("B47D2316-8150-477F-AD44-BAC12A1B0074", Caption = "Drawback Applicant Type", ShortCaption = "Drawback App.")]
		[ReadOnlyMember(nameof(JZ_DRWApplicantTypeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.DRWApplicantTypeList))]
		public override ZString JZ_DRWApplicantType
		{
			get { return base.JZ_DRWApplicantType; }
			set
			{
				base.JZ_DRWApplicantType = value;
				MarkAsNeedingValidation();
			}
		}

		internal bool JZ_DRWApplicantTypeReadOnly
		{
			get
			{
				var je_SimpleDRWApp = JobDeclaration?.JE_SimpleDRWApp ?? ZString.Empty;
				return je_SimpleDRWApp == ApplicationForSimpleDrawbackCodeList.Codes.AD;
			}
		}

		[ResourceStringData("EE133E59-B432-42CF-86D8-037C35EBF647", Caption = "Inbound Date")]
		public override ZDateTime JZ_InboundDate
		{
			get { return base.JZ_InboundDate; }
			set { base.JZ_InboundDate = value; }
		}

		[ResourceStringData("4FFB90B8-2F21-4704-B3D7-6E27BD48EB82", Caption = "Estimated Date of Final Price")]
		public override ZDateTime JZ_EstimatedDateOfFinalPrice
		{
			get { return base.JZ_EstimatedDateOfFinalPrice; }
			set { base.JZ_EstimatedDateOfFinalPrice = value; }
		}

		[DecimalPlaces(Schema.JZ_InvoiceCurrExRateDecimalPlaces)]
		[ResourceStringData("73150A97-5924-45B1-B43E-CDBB6702DF33", Caption = "Exchange Rate")]
		public override ZDecimal JZ_InvoiceCurrExRate
		{
			get { return base.JZ_InvoiceCurrExRate; }
			set { base.JZ_InvoiceCurrExRate = value; }
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;
				if (!IsCopying && oldValue != JZ_JE && !IsValidationSuspended)
				{
					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						line.VehicleNumbers.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZDecimal TotalInvoiceLinesNetWeightInKG
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (Core.Constants.Weight.Codes.Contains((string)invoiceLine.JI_NetWeightUQ))
					{
						result += Core.Constants.Weight.Convert(invoiceLine.JI_NetWeight, invoiceLine.JI_NetWeightUQ, Core.Constants.Weight.Kilograms);
					}
				}
				return result;
			}
		}

		[ResourceStringData("CFF326F6-946E-4721-8EA5-1ADF05D7C455", Caption = "Total Gross Weight")]
		public ZDecimal TotalInvoiceLinesWeightInKG
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (Core.Constants.Weight.Codes.Contains((string)invoiceLine.JI_WeightUQ))
					{
						result += Core.Constants.Weight.Convert(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ, Core.Constants.Weight.Kilograms);
					}
				}
				return result;
			}
		}
		public ZPropertyInfo TotalInvoiceLinesWeightInKGInfo => GetZPropertyInfo(nameof(TotalInvoiceLinesWeightInKG));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.IPCCodeList))]
		[ResourceStringData("80DBF40C-220B-45C6-B49D-8E262D982092", Caption = "Industrial Park Code")]
		public ZString ManufacturerIPCCode
		{
			get
			{
				var result = ManufacturerAddress?.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.IndustrialParkCode)?.OK_CustomsRegNo ?? ZString.Empty;
				if (JobDeclaration.JE_ProcedureType != DeclarationProcedureTypeList.Codes.E && result.IsEmpty)
				{
					result = ManufacturerDefaultCode.IndustrialParkCode;
				}
				return result;
			}
		}
		public ZPropertyInfo ManufacturerIPCCodeInfo => GetZPropertyInfo(nameof(ManufacturerIPCCode));

		[ResourceStringData("B1005605-31C3-4AA9-B452-9D20EED00976", Caption = "UNIPASS ID")]
		public ZString ManufacturerUnipassID
		{
			get
			{
				var result = ManufacturerAddress?.Header.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.UnipassIDForOrganization)?.OK_CustomsRegNo ?? ZString.Empty;
				if (JobDeclaration.JE_ProcedureType != DeclarationProcedureTypeList.Codes.E && result.IsEmpty)
				{
					result = ManufacturerDefaultCode.UnipassID;
				}
				return result;
			}
		}
		public ZPropertyInfo ManufacturerUnipassIDInfo => GetZPropertyInfo(nameof(ManufacturerUnipassID));

		[ResourceStringData("8BC3986F-5D30-49A3-80CB-82F11CD99AE0", Caption = "UNIPASS ID")]
		public ZString SupplierUnipassID
		{
			get
			{
				var result = ZString.Empty;
				if (Supplier != null)
				{
					result = Supplier.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.UnipassIDForOrganization)?.OK_CustomsRegNo ?? SupplierDefaultCode.UnipassID;
				}
				return result;
			}
		}

		[ResourceStringData("FA46B6D3-3B52-40FB-98A7-F5EA43678D2E", Caption = "Buyer ID")]
		public ZString BuyerID
		{
			get
			{
				var result = ZString.Empty;
				if (Buyer != null)
				{
					result = Buyer.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.ForeignCompanyID)?.OK_CustomsRegNo ?? ZString.Empty;
					if (JobDeclaration.JE_ProcedureType != DeclarationProcedureTypeList.Codes.E && result.IsEmpty)
					{
						result = BuyerDefaultCode.BuyerID;
					}
				}
				return result;
			}
		}
		public ZPropertyInfo BuyerIDInfo => GetZPropertyInfo(nameof(BuyerID));

		public ZString ImportSupplierID
		{
			get
			{
				var result = ZString.Empty;
				if (Supplier != null)
				{
					result = Supplier.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.ForeignCompanyID)?.OK_CustomsRegNo ?? ZString.Empty;
					if (result.IsEmpty &&
							(ImportDealingTypeCodeList.IsApplicableForDefaultSupplierID(JobDeclaration.JE_TradeType)
							|| ImportDeclarationTypeCodeList.IsSimpleDeclarationType(JobDeclaration.JE_MessageSubType)
							|| DeclarationProcedureTypeCodeList.IsApplicableForDefaultSupplierID(JobDeclaration.JE_ProcedureType)))
					{
						result = SupplierDefaultCode.SupplierID;
					}
				}
				return result;
			}
		}
		public ZPropertyInfo ImportSupplierIDInfo => GetZPropertyInfo(nameof(ImportSupplierID));

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var declaration = JobDeclaration;
				var result = ZDateTime.Today;
				if (declaration != null && InvoiceLines.Count > 0)
				{
					var firstEntryLine = InvoiceLines[0].CusEntryLine;
					if (firstEntryLine != null)
					{
						result = declaration.GetEntryIssueDate(firstEntryLine.CL_CH);
					}
				}
				return result;
			}
		}

		public override ZString JZ_MessageType
		{
			get => base.JZ_MessageType;
			set
			{
				var oldValue = JZ_MessageType;
				base.JZ_MessageType = value;
				if (JobDeclaration == null && JZ_MessageType != oldValue)
				{
					var currencyProvidersToRefreshExRatesFor = GetCurrencyProvidersToRefreshExRatesFor();
					foreach (var item in currencyProvidersToRefreshExRatesFor)
					{
						item.SetExchangeRateIfNotUserOverridden();
					}
				}
			}
		}

		[ResourceStringData("46738405-9A87-4417-8591-F774F1F14F2A", Caption = "Bill")]
		public override ZGuid JZ_CU_RelatedHouseBill
		{
			get => base.JZ_CU_RelatedHouseBill;
			set
			{
				var oldValue = JZ_CU_RelatedHouseBill;
				base.JZ_CU_RelatedHouseBill = value;
				if (oldValue != JZ_CU_RelatedHouseBill)
				{
					if (JobDeclaration != null && Bill != null && Bill.CargoManagementNumbers.Count == 1)
					{
						JZ_ImportCargoManagementNumber = Bill.CargoManagementNumber;
					}
				}
			}
		}

		[ResourceStringData("DD512F43-4163-4B38-A474-4B55DA70181A", Caption = "Online Trade Type")]
		[MaxLength(Schema.JZ_OnlineTradeTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.OnlineTradeTypeList))]
		public override ZString JZ_OnlineTradeType
		{
			get => base.JZ_OnlineTradeType;
			set => base.JZ_OnlineTradeType = value;
		}

		protected override ExchangeRateType RateTypeCore => ExchangeRateTypeDecider.GetExchangeRateType(JZ_MessageType);

		protected override JobComInvoiceHeaderDeepCopyStrategy GetTemplateCopyStrategy(CloneType cloneType)
		{
			return new JobComInvoiceHeaderDeepCloneStrategy(this, cloneType, JobDeclaration, null);
		}

		JobDeclaration ISupportingDocumentParent.Declaration => JobDeclaration;
		public bool IsLocalExport => JobDeclaration?.IsLocalExport ?? false;

		public bool IsD87 => JobDeclaration?.IsD87 ?? false;
		public bool IsPersonalItemDeclaration => JobDeclaration?.IsPersonalItemDeclaration ?? false;
		public bool Is5SM => JobDeclaration?.Is5SM ?? false;
		internal void SetDefaultSupplierFromDeclaration()
		{
			var declarationSupplier = JobDeclaration?.Supplier;
			if (declarationSupplier != null)
			{
				if (JobDeclaration.IsImport)
				{
					JZ_OH_Supplier = declarationSupplier.PK;
				}
			}
		}

		[ResourceStringData("D33D59FC-DB99-4229-91BA-FAA4AB10EEEB", Caption = "Additional Amount (KRW)")]
		public ZDecimal ImportTotalAdditionalAmount => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.GetImportAdditionalAmountInLocalCurrency(CurrencyConverter, x.InvoiceHeader.JZ_ValuationCode));

		[ResourceStringData("9A3040B3-326D-4CE9-9A7D-2FECFE483FC5", Caption = "Deducted Amount (KRW)")]
		public ZDecimal ImportTotalDeductedAmount => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.GetImportDeductionAmountInLocalCurrency(CurrencyConverter, x.InvoiceHeader.JZ_ValuationCode));

		public ZBool IsFreeTrade => JZ_PaymentTerms == InvoicePaymentTermCodeList.Codes.GN || ImportDealingTypeCodeList.IsTradeFree(JobDeclaration?.JE_TradeType);

		public bool Is008 => JobDeclaration?.IsPersonalItemDeclaration ?? false;

		protected override void SetSupplierFromDeclaration()
		{
			SetDefaultSupplierFromDeclaration();
		}
	}
}
