using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IE.Business.Constants;
using EUInvoiceHeader = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusEntryInstruction : EU.Business.Declaration.CusEntryInstruction
		, Integration.Customs.IE.ICusEntryInstruction
		, IAdditionalInfoCollectionProvider
		, ICusGoodsLocationProviderWithUCCVersion
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusEntryInstruction.Schema
		{
		}

		protected override ZBool RequestedDocumentsReadOnly => true;

		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		public bool IsImport => JobDeclaration?.IsImport ?? false;

		public bool IsExport => JobDeclaration?.IsExport ?? false;

		public bool IsExitSummary => JobDeclaration is JobDeclaration declaration && declaration.IsExitSummary;

		public bool IsCoJob => JobDeclaration is JobDeclaration declaration && declaration.IsCoJob;

		public bool IsAir => JobDeclaration is JobDeclaration declaration && declaration.IsAir;

		public new IEnumerable<JobComInvoiceHeader> Invoices => base.Invoices.Cast<JobComInvoiceHeader>();

		protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(CEI_AddInfoInfo);

		public new AddInfoCusEntryInstructionLookups AddInfoLookups => (AddInfoCusEntryInstructionLookups)base.AddInfoLookups;

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				var oldValue = CEI_JE;
				base.CEI_JE = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					JobDeclaration?.CustomsOffices.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData(
			key: "IE.CusEntryInstruction.CEI_Style|UCC5",
			ShortCaption = "Decl. Type",
			MediumCaption = "[1/1] Decl.Type",
			Caption = "[1/1] Declaration Type",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC5
		)]
		[ResourceStringData("IE.CusEntryInstruction.CEI_Style|UCC6", Caption = "Declaration Type", FullDescription = "[11 01 001 000] Declaration Type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				var oldValue = CEI_Style;
				base.CEI_Style = value;
				if (!IsCopying && oldValue != CEI_Style)
				{
					PopulateCH_MessageType(value);
					CreateAndPopulateAdditionalRefType1D23();
					CreateAndPopulateAdditionalRefType1D24();
				}
			}
		}

		#region PreviousDocuments

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		#endregion

		#region SupportingDocuments

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;
		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#endregion

		#region AdditionalInfos

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		IEnumerable<AdditionalInfo> IAdditionalInfoCollectionProvider.AdditionalInfos => AdditionalInfos.Cast<AdditionalInfo>();

		#endregion

		#region ICusGoodsLocationProvider

		public bool IsUCC5 => JobDeclaration is JobDeclaration declaration && declaration.IsUCC5;

		public bool IsUCC5AndIsImport => JobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport;

		public bool IsUCC6 => JobDeclaration is JobDeclaration declaration && declaration.IsUCC6;

		public ZPropertyInfo IsUCC5Info => JobDeclaration is JobDeclaration declaration
			? GetWrappedZPropertyInfo(nameof(IsUCC5), x => declaration.JE_ApplicationCodeInfo)
			: GetZPropertyInfo(nameof(IsUCC5));

		[ResourceStringData("IE.CusEntryInstruction.GoodsLocationDescription|UCC5", ShortCaption = "[5/23] Goods Loc.", MediumCaption = "[5/23] Goods Location", Caption = "[5/23] Location of goods", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString GoodsLocationDescription => base.GoodsLocationDescription;

		#endregion

		public IReadOnlyList<JobComInvoiceLine> MainPackInvoiceLines => Factory.GetValue(ref mainPackInvoiceLinesCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.ZG_IsMainPack).ToArray());
		CachedProperty<JobComInvoiceLine[]> mainPackInvoiceLinesCached;

		[ResourceStringData(
			key: "IE.CusEntryInstruction.CEI_SubStyle|UCC5",
			ShortCaption = "[1/2]Add. Type",
			MediumCaption = "[1/2] Add. Decl. Type",
			Caption = "[1/2] Add. Declaration Type",
			FullDescription = "[1/2] Additional Declaration Type",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC5
		)]
		[ResourceStringData("IE.CusEntryInstruction.CEI_SubStyle|UCC6", Caption = "Sub Style", FullDescription = "[11 02 001 000] Additional Declaration Type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString CEI_SubStyle
		{
			get { return base.CEI_SubStyle; }
			set
			{
				base.CEI_SubStyle = value;
				HandleAuthorizations(value);
			}
		}

		[ResourceStringData("17F25757-C468-47D9-97E6-9928E270D2EB", ShortCaption = "[Art. 163 7/5] Planned Act.", MediumCaption = "[Art. 163 7/5] Planned Activity", Caption = "[Article 163 7/5] Details of Planned Activities", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString DetailsOfPlannedActivities
		{
			get => base.DetailsOfPlannedActivities;
			set => base.DetailsOfPlannedActivities = value;
		}

		[ResourceStringData("45D36AA0-78AF-4AB2-AA9B-FCA4A938AB45",
			ShortCaption = "[Art. 163 8/13] Import Duty Amount",
			MediumCaption = "[Art. 163 8/13] Import Duty Amount (per article 86(3))",
			Caption = "[Art. 163 8/13] Import Duty Amount in accordance with Article 86(3) of the Code",
			FullDescription = "[Article 163 8/13] Calculation of the amount of the import duty in accordance with Article 86(3) of the Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_Article86_3_UCC
		{
			get => base.ZG_Article86_3_UCC;
			set => base.ZG_Article86_3_UCC = value;
		}

		[ResourceStringData("CF9ED24E-53E4-4C87-835E-F7E82CC495AC", ShortCaption = "[Art. 163 8/5] Additional Info.", Caption = "[Article 163 8/5] Additional Information", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString AdditionalInformation
		{
			get => base.AdditionalInformation;
			set => base.AdditionalInformation = value;
		}

		[ResourceStringData("EAA21411-1688-425F-B3EE-28565A54C94F", ShortCaption = "[Art. 163 5/5] Rate of Yield", Caption = "[Article 163 5/5] Rate of Yield", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_RateOfYield
		{
			get => base.ZG_RateOfYield;
			set => base.ZG_RateOfYield = value;
		}

		[ResourceStringData("E563FB36-5704-4290-B5D1-C6060BA549CA", ShortCaption = "[Art. 163 4/17] Details", Caption = "[Article 163 4/17] Details", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString PeriodForDischargeDetails
		{
			get => base.PeriodForDischargeDetails;
			set => base.PeriodForDischargeDetails = value;
		}

		[ResourceStringData("46E37294-F4D6-42E2-A011-B24F7286289D", ShortCaption = "[Art. 163 4/17] Period (Monthly)", Caption = "[Article 163 4/17] Period (Monthly)", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZInt ZG_PeriodForDischarge { get => base.ZG_PeriodForDischarge; set => base.ZG_PeriodForDischarge = value; }

		[ResourceStringData("D3CB5427-49BC-4436-84EE-1A0109A9B89D", ShortCaption = "[Art. 163 4/17] Automatic Extension?", Caption = "[Article 163 4/17] Automatic Extension?", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZBool ZG_PeriodForDischargeAutoExtension { get => base.ZG_PeriodForDischargeAutoExtension; set => base.ZG_PeriodForDischargeAutoExtension = value; }

		[ResourceStringData("E53E95E2-741D-45F3-938F-3FCFB7A2AB62", ShortCaption = "[Art. 163 4/18] Deadline", Caption = "[Article 163 4/18] Deadline", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZInt ZG_BillOfDischargeDeadline { get => base.ZG_BillOfDischargeDeadline; set => base.ZG_BillOfDischargeDeadline = value; }

		[ResourceStringData("F95303EA-40F2-4DCD-958E-F24261EF9F2C", ShortCaption = "[Art. 163 4/18] Necessary?", Caption = "[Article 163 4/18] Necessary?", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZBool ZG_BillOfDischargeIsNecessary { get => base.ZG_BillOfDischargeIsNecessary; set => base.ZG_BillOfDischargeIsNecessary = value; }

		[ResourceStringData("E352758E-C7E1-4620-9237-754D4634DAC9", ShortCaption = "[Art. 163 4/18] Details", Caption = "[Article 163 4/18] Details", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString BillOfDischargeDetails
		{
			get => base.BillOfDischargeDetails;
			set => base.BillOfDischargeDetails = value;
		}

		[ResourceStringData("7DBEC0EA-9E13-4FB1-AF2D-E6B2E7D1DE16", ShortCaption = "[Art. 163 5/7] Commodity Code", Caption = "[Article 163 5/7] Commodity Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_ProcessedProductsCommodityCode { get => base.ZG_ProcessedProductsCommodityCode; set => base.ZG_ProcessedProductsCommodityCode = value; }

		[ResourceStringData("B189E0FD-92D7-4857-ACCA-956C0B2F7A7D", ShortCaption = "[Art. 163 5/7] Goods Desc.", Caption = "[Article 163 5/7] Goods Description", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ProcessedProductDescription
		{
			get => base.ProcessedProductDescription;
			set => base.ProcessedProductDescription = value;
		}

		[ResourceStringData("91776012-779A-495B-BE5B-98E94F67A041", ShortCaption = "[Art. 163 5/8] Code", Caption = "[Article 163 5/8] Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_IdOfGoodCode { get => base.ZG_IdOfGoodCode; set => base.ZG_IdOfGoodCode = value; }

		[ResourceStringData("83D4D3E5-E6D3-4571-B8C8-A2E6AF697A68", ShortCaption = "[Art. 163 5/8] Details", Caption = "[Article 163 5/8] Details", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString IdentificationofGoodsDetails
		{
			get => base.IdentificationofGoodsDetails;
			set => base.IdentificationofGoodsDetails = value;
		}

		[ResourceStringData("B90350BD-6A68-4D50-9469-377770A17CDC", ShortCaption = "[Art. 163 6/2] Processing Procedure", Caption = "[Article 163 6/2] Processing Procedure", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_ProcessingProcedureCode { get => base.ZG_ProcessingProcedureCode; set => base.ZG_ProcessingProcedureCode = value; }

		[ResourceStringData("20880B2C-DF64-4AF9-9FEF-D9F64C05ECB7", ShortCaption = "[Art. 163 6/2] Details", Caption = "[Article 163 6/2] Details", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ProcessingProcedureDetails
		{
			get => base.ProcessingProcedureDetails;
			set => base.ProcessingProcedureDetails = value;
		}

		[MaxLength(Schema.CEI_DescriptionMaxLength)]
		public override ZString CEI_Description
		{
			get => base.CEI_Description;
			set
			{
				CheckMaximumLength(CEI_DescriptionInfo, value);
				var oldValue = CEI_Description;
				base.CEI_Description = value;
				if(!IsCopying && oldValue != CEI_Description)
				{
					JobDeclaration?.MarkInvoicesAsNeedingValidation();
				}
			}
		}

		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set
			{
				var oldValue = CEI_DateForDuty;
				base.CEI_DateForDuty = value;
				if (!IsCopying && oldValue != CEI_DateForDuty)
				{
					JobDeclaration?.MarkInvoicesAsNeedingValidation();
				}
			}
		}

		public override ZGuid CEI_OA_Warehouse2
		{
			get => base.CEI_OA_Warehouse2;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_OA_Warehouse2))
				{
					var oldValue = CEI_OA_Warehouse2;
					base.CEI_OA_Warehouse2 = value;
					if (!IsCopying)
					{
						DefaultAuthorization(oldValue, CEI_OA_Warehouse2);
						if (oldValue != CEI_OA_Warehouse2 && !IsValidationSuspended)
						{
							InvoiceLines.FirstOrDefault(x => x.JI_CEI == PK)?.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZGuid CEI_OA_Warehouse
		{
			get => base.CEI_OA_Warehouse;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_OA_Warehouse))
				{
					var oldValue = CEI_OA_Warehouse;
					base.CEI_OA_Warehouse = value;
					if (!IsCopying)
					{
						DefaultAuthorization(oldValue, CEI_OA_Warehouse);
						if (oldValue != CEI_OA_Warehouse && !IsValidationSuspended)
						{
							InvoiceLines.FirstOrDefault(x => x.JI_CEI == PK)?.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public bool HasMutuallyExclusiveSupportingDocument => Factory.GetValue(ref hasMutuallyExclusiveSupportingDocument, () => SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasMutuallyExclusiveSupportingDocument;

		void DefaultAuthorization(ZGuid oldValue, ZGuid newValue)
		{
			if (newValue != oldValue && newValue.IsValid)
			{
				var authorisationHeaders = GetAuthorisationHeaders(newValue, WarehouseAuthorisationTypes);
				if (authorisationHeaders.Length == 1)
				{
					var header = authorisationHeaders[0];
					if (!CusAuthorizationUsages.Any(usage => usage.AGC_Code == header.CPH_Type && usage.AGC_OH_Owner == header.CPH_OH_PermitHolder && usage.AGC_Number == header.CPH_Number))
					{
						var usage = CusAuthorizationUsages.AddNew();
						using (usage.SuspendWarehouseDataRefresh())
						{
							usage.AGC_Code = header.CPH_Type.Left(usage.AGC_CodeInfo.MaxLength);
							usage.AGC_OH_Owner = header.CPH_OH_PermitHolder;
							usage.AGC_Number = header.CPH_Number;
						}
					}
				}
				RefreshWarehouseData();
			}
		}

		internal void RefreshWarehouseData()
		{
			FromWarehouseTypeInfo.RefreshBinding();
			ToWarehouseTypeInfo.RefreshBinding();
		}

		protected override ZString GetToWarehouseType()
		{
			return ToWarehouseAuthorizationUsage is CusAuthorizationUsage toWarehouseAuthorizationUsage ? AuthorisationTypeToWarehouseType[toWarehouseAuthorizationUsage.AGC_Code] : ZString.Empty;
		}

		protected override ZString GetFromWarehouseType()
		{
			return FromWarehouseAuthorizationUsage is CusAuthorizationUsage fromWarehouseAuthorizationUsage ? AuthorisationTypeToWarehouseType[fromWarehouseAuthorizationUsage.AGC_Code] : ZString.Empty;
		}

		protected override IReadOnlyList<ZString> WarehouseAuthorisationTypes => AuthorisationTypeToWarehouseType.Keys.ToArray();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly ReadOnlyDictionary<ZString, ZString> AuthorisationTypeToWarehouseType = new ReadOnlyDictionary<ZString, ZString>(new Dictionary<ZString, ZString>()
		{
			{ CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, WarehouseTypeList.Codes.CustomsWarehousingCW1 },
			{ CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, WarehouseTypeList.Codes.CustomsWarehousingCW2 },
			{ CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, WarehouseTypeList.Codes.CustomsWarehousingCWP },
			{ CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, WarehouseTypeList.Codes.TemporaryStorage },
		});

		public bool IsSimplifiedDeclarationAuthorizationRequired
		{
			get
			{
				var subStyle = CEI_SubStyle;
				return subStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.SimplifiedDeclaration) || subStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC);
			}
		}

		public bool IsSupplementaryDeclarationForCode
		{
			get
			{
				var subStyle = CEI_SubStyle.ToUpperInvariant();
				return subStyle.Equals(EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE) || subStyle.Equals(EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF);
			}
		}

		public bool IsSubStyle_B_C_E_F
		{
			get
			{
				var substyle = CEI_SubStyle;
				return substyle.EqualsIgnoringCase(EntrySubStyleList.Codes.IncompleteDeclaration) ||
					substyle.EqualsIgnoringCase(EntrySubStyleList.Codes.SimplifiedDeclaration) ||
					substyle.EqualsIgnoringCase(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB) ||
					substyle.EqualsIgnoringCase(EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC);
			}
		}

		public bool StatisticalValueRequired => JobDeclaration?.IsTransitionPeriodAES30 ?? false ? CEI_Style != ExportDeclarationTypeList.Codes.B3 : CEI_Style == ExportDeclarationTypeList.Codes.B1 || CEI_Style == ExportDeclarationTypeList.Codes.B2;

		public bool IsB1Declaration => CEI_Style == ExportDeclarationTypeList.Codes.B1;

		public bool IsCustomsWarehousingOfUnionGoods => CEI_Style == ExportDeclarationTypeList.Codes.B3;

		public bool IsH1 => CEI_Style == ImportDeclarationTypeList.Codes.H1;

		public bool IsH2 => CEI_Style == ImportDeclarationTypeList.Codes.H2;

		public bool IsH3 => CEI_Style == ImportDeclarationTypeList.Codes.H3;

		public bool IsH4 => CEI_Style == ImportDeclarationTypeList.Codes.H4;

		public bool IsH5 => CEI_Style == ImportDeclarationTypeList.Codes.H5;

		public bool IsH6 => CEI_Style == ImportDeclarationTypeList.Codes.H6;

		public bool IsI1 => CEI_Style == ImportDeclarationTypeList.Codes.I1;

		public bool IsCustomsWarehousingProcedure76Or77 => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsCustomsWarehousingProcedure76Or77);

		public bool IsProcedureCode44 => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsProcedureCode44);

		public bool IsProcedureCode53 => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsProcedureCode53);

		public bool IsInwardProcessingProcedure51 => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsInwardProcessingProcedure51);

		public bool IsSecuritiesForEndUse => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsSecuritiesForEndUse);

		public bool IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);

		public bool HasMultipleDeliveryTerms => Factory.GetValue(ref hasMultipleDeliveryTermsCached, () =>
		{
			var incoTerm = ZString.Empty;
			var incoTermPlace = ZString.Empty;
			var agreedPlaceCode = ZString.Empty;
			var additionalTerms = ZString.Empty;
			var isFirst = true;
			foreach (var invoice in Invoices)
			{
				if (isFirst)
				{
					isFirst = false;
					incoTerm = invoice.JZ_IncoTerm;
					incoTermPlace = invoice.JZ_IncoTermPlace;
					agreedPlaceCode = invoice.ZG_AgreedPlaceCode;
					additionalTerms = invoice.JZ_AdditionalTerms;
				}
				else if (!(incoTerm.EqualsIgnoringCase(invoice.JZ_IncoTerm) && incoTermPlace.EqualsIgnoringCase(invoice.JZ_IncoTermPlace) && agreedPlaceCode.EqualsIgnoringCase(invoice.ZG_AgreedPlaceCode) && additionalTerms.EqualsIgnoringCase(invoice.JZ_AdditionalTerms)))
				{
					return true;
				}
			}
			return false;
		});
		CachedProperty<bool> hasMultipleDeliveryTermsCached;

		public bool HasAuthorisationForSpecialProcedure => Factory.GetValue(ref hasAuthorisationForSpecialProcedure, () => AdditionalInfos.HasAuthorisationForSpecialProcedure());
		CachedProperty<bool> hasAuthorisationForSpecialProcedure;

		public bool HasSupportingDocumentForExportWithInvoiceNumber => Factory.GetValue(ref hasSupportingDocumentForExportWithInvoiceNumberCached, () => SupportingDocuments.Cast<SupportingDocument>().Any(supportingDoc => !supportingDoc.CSI_ReferenceNumber.IsEmpty && Constants.SupportingDocumentCodes.InvoiceDocumentTypesForExport.Contains(supportingDoc.CSI_Code)));
		CachedProperty<bool> hasSupportingDocumentForExportWithInvoiceNumberCached;

		public bool HasSupportingDocumentWithCode(ZString csi_code)
		{
			hasSupportingDocumentWithCode = hasSupportingDocumentWithCode ?? new Dictionary<ZString, CachedProperty<bool>>();
			var cachedProperty = hasSupportingDocumentWithCode.GetOrAdd(csi_code, () => default);
			return Factory.GetValue(ref cachedProperty, () => SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == csi_code));
		}
		Dictionary<ZString, CachedProperty<bool>> hasSupportingDocumentWithCode;

		public bool HasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumber => Factory.GetValue(ref hasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumberCached, () => Invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.HasSupportingDocumentForExportWithInvoiceNumber));
		CachedProperty<bool> hasExportInvoiceHeaderWithSupportingDocumentHavingInvoiceNumberCached;

		public bool HasEstimatedTimeOfDepartureAdditionalInfo => Factory.GetValue(ref hasEstimatedTimeOfDepartureAdditionalInfoCached, () => AdditionalInfos.Cast<AdditionalInfo>().Any(addInfo => addInfo.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture)));
		CachedProperty<bool> hasEstimatedTimeOfDepartureAdditionalInfoCached;

		public bool HasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfo => Factory.GetValue(ref hasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfoCached, () => Invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.HasEstimatedTimeOfDepartureAdditionalInfo));
		CachedProperty<bool> hasInvoiceHeaderWithEstimatedTimeOfDepartureAdditionalInfoCached;

		public bool HasTransportDocument => Factory.GetValue(ref hasTransportDocumentCached, () => AdditionalInfos.Cast<AdditionalInfo>().Any(addInfo => addInfo.IsATransportDocument));
		CachedProperty<bool> hasTransportDocumentCached;

		public bool HasInvoiceHeaderWithTransportDocument => Factory.GetValue(ref hasInvoiceHeaderWithTransportDocumentCached, () => Invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.HasTransportDocument));
		CachedProperty<bool> hasInvoiceHeaderWithTransportDocumentCached;

		public bool DoAllInvoiceLinesHaveFR5FiscalReference => Factory.GetValue(ref doAllInvoiceLinesHaveFR5FiscalReferenceCached,
			() => InvoiceLines.Cast<JobComInvoiceLine>().Any()
					&& InvoiceLines.Cast<JobComInvoiceLine>().All(invoiceLine => invoiceLine.FiscalReferences.Any(x => x.CFR_Code.EqualsIgnoringCase(EU.Business.FiscalReferenceCodeList.Codes.FR5_Vendor))));
		CachedProperty<bool> doAllInvoiceLinesHaveFR5FiscalReferenceCached;

		public bool HasInvoiceLineWithPreviousProcedure0700 => Factory.GetValue(ref hasInvoiceLineWithPreviousProcedure0700Cached, () => InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.RequestedPreviousProcedure).Any(x => x == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._0700));
		CachedProperty<bool> hasInvoiceLineWithPreviousProcedure0700Cached;

		public bool HasMultipleCurrencies => Factory.GetValue(ref hasMultipleCurrencies, () => HasMultipleInvoiceProperties((EUInvoiceHeader invoice) => invoice.JZ_RX_NKInvoice_Currency));
		CachedProperty<bool> hasMultipleCurrencies;

		public bool HasMultipleNatureOfTransactions => Factory.GetValue(ref hasMultipleNatureOfTransactions, () => HasMultipleInvoiceProperties((EUInvoiceHeader invoice) => invoice.JZ_ValuationCode));
		CachedProperty<bool> hasMultipleNatureOfTransactions;

		public bool HasU164OrU166OrN865SupportingDocument => Factory.GetValue(ref hasU164OrU166OrN865SupportingDocumentCached, () => SupportingDocumentCollectionExtensions.HasU164OrU166OrN865SupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasU164OrU166OrN865SupportingDocumentCached;

		public bool HasU165OrU167SupportingDocument => Factory.GetValue(ref hasU165OrU167SupportingDocumentCache, () => SupportingDocumentCollectionExtensions.HasU165OrU167SupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasU165OrU167SupportingDocumentCache;

		public bool HasN018SupportingDocument => Factory.GetValue(ref hasN018SupportingDocumentCached, () => SupportingDocumentCollectionExtensions.HasN018SupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasN018SupportingDocumentCached;

		public IEnumerable<SupportingDocument> SupportingDocumentsAtAnyLevel => Factory.GetValue(ref supportingDocumentsAtAnyLevelCached, () =>
			SupportingDocuments.Cast<SupportingDocument>()
			.Union(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.SupportingDocuments.Cast<SupportingDocument>()))
			.Union(Invoices.Cast<JobComInvoiceHeader>().SelectMany(invoice => invoice.SupportingDocuments.Cast<SupportingDocument>())));
		CachedProperty<IEnumerable<SupportingDocument>> supportingDocumentsAtAnyLevelCached;

		public IEnumerable<AdditionalInfo> AdditionalInfosAtAnyLevel => Factory.GetValue(ref additionalInfoAtAnyLevelCached, () =>
			AdditionalInfos.Cast<AdditionalInfo>()
			.Union(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.AdditionalInfos.Cast<AdditionalInfo>()))
			.Union(Invoices.Cast<JobComInvoiceHeader>().SelectMany(invoice => invoice.AdditionalInfos.Cast<AdditionalInfo>())));
		CachedProperty<IEnumerable<AdditionalInfo>> additionalInfoAtAnyLevelCached;

		bool HasMultipleInvoiceProperties<T>(Func<EUInvoiceHeader, T> getPropertyValue)
			where T : IZType
		{
			var propertyValue = default(T);
			var isFirst = true;
			var result = false;
			foreach (var invoice in Invoices)
			{
				var invoiceValue = getPropertyValue(invoice);
				if (isFirst)
				{
					isFirst = false;
					propertyValue = invoiceValue;
				}
				else if (!propertyValue.Equals(invoiceValue))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool HasBothC601SupportingDocumentAnd00100AdditionalInformation
		{
			get
			{
				var (hasC601SupportingDocument, has00100AdditionalInformation) = HasC601SupportingDocumentAnd00100AdditionalInformation;
				return hasC601SupportingDocument && has00100AdditionalInformation;
			}
		}

		public bool HasAuthorisationInwardProcessingProcedureOnShipmentLevel
		{
			get
			{
				var (hasC601SupportingDocument, _) = HasC601SupportingDocumentAnd00100AdditionalInformation;
				return hasC601SupportingDocument;
			}
		}

		public bool HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel
		{
			get
			{
				(_, var has00100AdditionalInformation) = HasC601SupportingDocumentAnd00100AdditionalInformation;
				return has00100AdditionalInformation;
			}
		}

		(bool, bool) HasC601SupportingDocumentAnd00100AdditionalInformation => Factory.GetValue(ref hasBothC601SupportingDocumentAnd00100AdditionalInformation, GetHasC601SupportingDocumentAnd00100AdditionalInformation);
		CachedProperty<(bool, bool)> hasBothC601SupportingDocumentAnd00100AdditionalInformation;

		(bool hasC601SupportingDocument, bool has00100AdditionalInformation) GetHasC601SupportingDocumentAnd00100AdditionalInformation()
		{
			var hasC601SupportingDocument = SupportingDocuments.HasAuthorisationInwardProcessingProcedure();
			var has00100AdditionalInformation = HasAuthorisationForSpecialProcedure;

			if (!hasC601SupportingDocument || !has00100AdditionalInformation)
			{
				foreach (var invoice in Invoices)
				{
					if (!hasC601SupportingDocument)
					{
						hasC601SupportingDocument = invoice.SupportingDocuments.HasAuthorisationInwardProcessingProcedure();
					}

					if (!has00100AdditionalInformation)
					{
						has00100AdditionalInformation = invoice.AdditionalInfos.HasAuthorisationForSpecialProcedure();
					}

					if (hasC601SupportingDocument && has00100AdditionalInformation)
					{
						return (true, true);
					}
				}
			}

			return (hasC601SupportingDocument, has00100AdditionalInformation);
		}

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		protected override bool IsLookupsCachedInBase => false;
		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
		{
			CusEntryInstructionLookups result;
			var declaration = JobDeclaration;
			switch (declaration?.JE_MessageType ?? ZString.Empty)
			{
				case IEJobMessageTypeList.Codes.ExitSummary:
					result = new ExitSummaryCusEntryInstructionLookups(this);
					break;
				case IEJobMessageTypeList.Codes.ReExport:
					result = new ReExportCusEntryInstructionLookups(this);
					break;
				case IEJobMessageTypeList.Codes.Export:
					result = new ExportCusEntryInstructionLookups(this);
					break;
				case IEJobMessageTypeList.Codes.Import:
					result = new ImportCusEntryInstructionLookups(this);
					break;
				default:
					result = new CusEntryInstructionLookups(this);
					break;
			}
			return result;
		}

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
		{
			CusEntryInstructionValidation result;
			var declaration = JobDeclaration;
			switch (declaration?.JE_MessageType.ToUpperInvariant() ?? ZString.Empty)
			{
				case IEJobMessageTypeList.Codes.Import:
					result = IsUCC5 ? new UCC5ImportCusEntryInstructionValidation(this) : new UCC6ImportCusEntryInstructionValidation(this);
					break;
				case IEJobMessageTypeList.Codes.Export:
					result = new ExportCusEntryInstructionValidation(this);
					break;
				case IEJobMessageTypeList.Codes.ExitSummary:
					result = new ExitSummaryCusEntryInstructionValidation(this);
					break;
				case IEJobMessageTypeList.Codes.ReExport:
					result = new ReExportCusEntryInstructionValidation(this);
					break;
				default:
					result = new CusEntryInstructionValidation(this);
					break;
			}
			return result;
		}

		protected override bool IsInnerGoodsLocationActiveCore => IsImport;

		public new ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> CusAuthorizationUsages => (CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>)base.CusAuthorizationUsages;

		public new GuaranteeForEntryInstructionCollection Guarantees => (GuaranteeForEntryInstructionCollection)base.Guarantees;

		public new EU.Business.Declaration.ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences => (EU.Business.Declaration.ICusFiscalReferenceCollection<CusFiscalReference>)base.FiscalReferences;

		protected override EU.Business.Declaration.GuaranteeForEntryInstructionCollection GetNewGuaranteeForEntryInstructionCollection() => new GuaranteeForEntryInstructionCollection(this);

		protected override Type FiscalReferenceType => typeof(CusFiscalReference);

		protected override ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

		protected override EU.Business.Declaration.ICusFiscalReferenceCollection<EU.Business.Declaration.CusFiscalReference> GetNewFiscalReferenceCollection() => new EU.Business.Declaration.CusFiscalReferenceCollection<CusFiscalReference>(this);

		void HandleAuthorizations(ZString subStyle)
		{
			if (IsSimplifiedDeclarationAuthorizationRequired && IsExport)
			{
				var codeType = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				if (CusAuthorizationUsages.All(p => p.AGC_Code != codeType))
				{
					var authorization = CusAuthorizationUsages.AddNew();
					authorization.AGC_Code = codeType;
				}
			}
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		void PopulateCH_MessageType(ZString cei_style)
		{
			if (EntryHeader != null)
			{
				EntryHeader.CH_MessageType = cei_style;
				EntryHeader.CH_MessageTypeInfo.RefreshBinding();
			}
		}

		void CreateAndPopulateAdditionalRefType1D23()
		{
			var subStyle = CEI_SubStyle;
			if (!subStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic) && !subStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF)
				&& JobDeclaration is JobDeclaration declaration && declaration.IsExport)
			{
				var dateAtOrigin = declaration.JE_DateAtOrigin;
				if (dateAtOrigin.IsValid && !AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture))
				{
					var additionalInfo = AdditionalInfos.AddNew();
					additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
					additionalInfo.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
					additionalInfo.CSI_ReferenceNumber = dateAtOrigin.ToString(Constants.DateTimeFormat.AdditionalReferenceDateTime);
				}
			}
		}

		internal void CreateAndPopulateAdditionalRefType1D24()
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.IsImport)
			{
				var dateAtDestination = declaration.JE_DateAtFinalDestination;
				if (dateAtDestination.IsValid)
				{
					var default1D24 = SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == Constants.SupportingDocumentCodes._1D24);
					if (default1D24 == null)
					{
						var new1D24 = SupportingDocuments.AddNew();
						new1D24.CSI_Code = Constants.SupportingDocumentCodes._1D24;
						new1D24.CSI_ReferenceNumber = dateAtDestination.ToString(Constants.DateTimeFormat.AdditionalReferenceDateTime);
					}
					else
					{
						default1D24.CSI_ReferenceNumber = dateAtDestination.ToString(Constants.DateTimeFormat.AdditionalReferenceDateTime);
					}
				}
			}
		}

		protected override EU.Business.Declaration.EntryInstructionValidationModesCalculator CreateNewValidationModesCalculator() => new EntryInstructionValidationModesCalculator(this);

		public override void OnLoaded()
		{
			base.OnLoaded();
			SaveForNoAmendCheck();
		}

		internal ZString OriginalAdditionalDeclarationType { get; private set; }
		void SaveForNoAmendCheck()
		{
			if ((IsExport || IsImport) && FirstActiveEntry is CusEntryHeader entry && !entry.MovementReferenceNumber.IsEmpty)
			{
				OriginalAdditionalDeclarationType = CEI_SubStyle;
			}
		}

		internal CusEntryHeader[] AllEntryHeaders => Factory.GetValue(ref allEntryHeadersCached, () => JobDeclaration?.CustomsEntryHeaders.Cast<CusEntryHeader>().Where((entry) => entry.CH_CEI_Instruction == PK).OrderBy(x => x.CH_SystemCreateTimeUtc).ToArray() ?? Array.Empty<CusEntryHeader>());
		CachedProperty<CusEntryHeader[]> allEntryHeadersCached;

		internal CusEntryHeader FirstActiveEntry => Factory.GetValue(ref firstActiveEntry, () => AllEntryHeaders.FirstOrDefault(entry => entry.IsActive));
		CachedProperty<CusEntryHeader> firstActiveEntry;

		public HashSet<ZString> AdditionalProcedures => Factory.GetValue(ref additionalProceduresCached, () => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.AdditionalProcedureCodesIncludingConcession).ToHashSet());
		CachedProperty<HashSet<ZString>> additionalProceduresCached;

		public bool IsBR11106MutuallyExclusiveCodesSubsetOfAdditionalProcedures => Factory.GetValue(ref isBR11106MutuallyExclusiveCodesSubsetOfAdditionalProceduresCached, () => UniversalReferenceConstants.ProcedureCodes.BR11106MutuallyExclusiveCodes.IsSubsetOf(AdditionalProcedures));
		CachedProperty<bool> isBR11106MutuallyExclusiveCodesSubsetOfAdditionalProceduresCached;

		public bool IsBR11107MutuallyExclusiveCodesSubsetOfAdditionalProcedures => Factory.GetValue(ref isBR11107MutuallyExclusiveCodesSubsetOfAdditionalProceduresCached, () => UniversalReferenceConstants.ProcedureCodes.BR11107MutuallyExclusiveCodes.IsSubsetOf(AdditionalProcedures));
		CachedProperty<bool> isBR11107MutuallyExclusiveCodesSubsetOfAdditionalProceduresCached;

		public bool IsProcedureF48 => Factory.GetValue(ref isProcedureF48Cached, () => AdditionalProcedures.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.F48));
		CachedProperty<bool> isProcedureF48Cached;

		public bool IsProcedureF49 => Factory.GetValue(ref isProcedureF49Cached, () => AdditionalProcedures.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.F49));
		CachedProperty<bool> isProcedureF49Cached;

		internal bool HasMRNPreviousDocuments => Factory.GetValue(ref hasMRNPreviousDocumentsCached, () => PreviousDocuments.HasMRNPreviousDocuments());
		CachedProperty<bool> hasMRNPreviousDocumentsCached;

		public Dictionary<string, int> BR8011KeyCounts => Factory.GetValue(ref countBR8011KeyCached, () =>
		{
			var keysCount = new Dictionary<string, int>();
			foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
			{
				if (line.IsBR8011PreferenceUsed() && !line.JI_ConcessionOrder.IsEmpty)
				{
					var key = line.GetBR8011Key();
					if (keysCount.TryGetValue(key, out int count))
					{
						keysCount[key] = count + 1;
					}
					else
					{
						keysCount[key] = 1;
					}
				}
			}
			return keysCount;
		});
		CachedProperty<Dictionary<string, int>> countBR8011KeyCached;

		IEnumerable<ZString> C100SupportingDocReferences => Factory.GetValue(ref c100SupportingDocReferencesCached, () => SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == SupportingDocumentCodes._C100 && !x.CSI_ReferenceNumber.IsEmpty).Select(r => r.CSI_ReferenceNumber));
		CachedProperty<IEnumerable<ZString>> c100SupportingDocReferencesCached;

		IEnumerable<ZString> InvoicesC100SupportingDocReferences => Factory.GetValue(ref c100InvoicesSupportingDocReferencesCached, () => Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.C100SupportingDocReferences));
		CachedProperty<IEnumerable<ZString>> c100InvoicesSupportingDocReferencesCached;

		public HashSet<ZString> DuplicatedEntryInstructionAndInvoiceC100SupportingDocReferences => Factory.GetValue(ref duplicatedEntryInstructionAndInvoiceC100SupportingDocReferencesCached, () => C100SupportingDocReferences.Concat(InvoicesC100SupportingDocReferences).GroupBy(s => s).SelectMany(grp => grp.Skip(1)).ToHashSet());
		CachedProperty<HashSet<ZString>> duplicatedEntryInstructionAndInvoiceC100SupportingDocReferencesCached;

		public HashSet<ZString> DuplicatedInvoiceLineC100SupportingDocReferences => Factory.GetValue(ref duplicatedInvoiceLineC100SupportingDocReferencesCached, () => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.C100SupportingDocReferences).GroupBy(s => s).SelectMany(grp => grp.Skip(1)).ToHashSet());
		CachedProperty<HashSet<ZString>> duplicatedInvoiceLineC100SupportingDocReferencesCached;

		public bool IsI1EntryAndTotalPriceLessThanOrEqualTo22EUR => CEI_Style == ImportDeclarationTypeList.Codes.I1 && IsTotalPriceLessThanOrEqualTo22EUR;

		public bool IsTotalPriceLessThanOrEqualTo22EUR => TotalEULinesPrice <= LinesPricesBR6142;

		public IEnumerable<CusAuthorizationUsage> CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner
		{
			get
			{
				var cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner = CusAuthorizationUsages.AsEnumerable();

				if (HasAnyChangeOfOwnershipProcedure && !CEI_OH_Owner.IsEmpty && OldOwner is OrgHeader oldOwner)
				{
					cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner = cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner.Where(x => x.AGC_OH_Owner != oldOwner.PK);
				}
				return cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			}
		}

		public ZBool HasAnyChangeOfOwnershipProcedure => (HasIntoWarehouseProcedure && HasOutOfWarehouseProcedure)
			|| (HasIntoOutwardProcessingProcedure && HasOutOfOutwardProcessingProcedure)
			|| (HasIntoInwardProcessingProcedure && HasOutOfInwardProcessingProcedure)
			|| (HasIntoTemporaryImportProcedure && HasOutOfTemporaryImportProcedure);

		decimal TotalEULinesPrice => Factory.GetValue(ref totalLinesPriceCached, () =>
		{
			var money = new Money(0m, EUCurrency);
			foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
			{
				money = CurrencyConverter.Add(money, line.JI_LinePriceMoney);
			}
			return money.Amount;
		});
		CachedProperty<decimal> totalLinesPriceCached;

		RefCurrency EUCurrency => euCurrency ??= Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.EuropeanUnion);
		RefCurrency euCurrency;

		const decimal LinesPricesBR6142 = 22m;
	}
}
