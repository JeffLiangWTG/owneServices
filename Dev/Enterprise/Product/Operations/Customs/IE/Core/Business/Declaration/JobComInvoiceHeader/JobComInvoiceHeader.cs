using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobComInvoiceHeader : EU.Business.Declaration.JobComInvoiceHeader
		, Integration.Customs.IE.IJobComInvoiceHeader
		, IAdditionalInfoCollectionProvider
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);

		[ChildEditable(true)]
		public new IJobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (IJobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new JobComInvChargeCollection<InvoiceCharge>(this);

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceCharge> Charges => (JobComInvChargeCollection<InvoiceCharge>)base.Charges;

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		IEnumerable<AdditionalInfo> IAdditionalInfoCollectionProvider.AdditionalInfos => AdditionalInfos.Cast<AdditionalInfo>();

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public override ZGuid JZ_JE
		{
			get => base.JZ_JE;
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;
				if (!IsCopying && oldValue != JZ_JE)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.MarkChargesAsNeedingValidation());
				}
			}
		}

		[ResourceStringData("9DD7703C-57B8-49A0-96FE-E76C10B0B154", Caption = "Commercial Ref.", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JZ_UCR { get => base.JZ_UCR; set => base.JZ_UCR = value; }

		[ResourceStringData("CA3E8AF9-FE6A-47CA-8DA5-EF48850A1F63", Caption = "Incoterm Place", FullDescription = "[14 01 036 000] UN/LOCODE or [14 01 020 000] Country code", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("1DF32EE1-E4B8-40F4-9305-41D038EADE40", ShortCaption = "[4/1] Place Code", MediumCaption = "[4/1] Inco. Place Code", Caption = "[4/1] Incoterm Place Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.ZG_AgreedPlaceCode))
				{
					var oldValue = ZG_AgreedPlaceCode;
					base.ZG_AgreedPlaceCode = value;
					if (!IsCopying && oldValue != ZG_AgreedPlaceCode && HasChanges)
					{
						JZ_IncoTermPlace = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("4136A4CC-2BDC-4700-966A-950DB83D4DBA", Caption = "Supplier")]
		public override ZGuid JZ_OH_Supplier
		{
			get => base.JZ_OH_Supplier;
			set => base.JZ_OH_Supplier = value;
		}

		[ResourceStringData("2E249E31-C106-495F-8338-7D2FA6F73B0E", Caption = "Transport Charges MoP", MediumCaption = "Trans. Chrg. MoP", ShortCaption = "Charges MoP", FullDescription = "[14 02 038 000] Transport Charges - Method of Payment", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString ZG_TransportChargesMethodOfPayment
		{
			get { return base.ZG_TransportChargesMethodOfPayment; }
			set { base.ZG_TransportChargesMethodOfPayment = value; }
		}

		[ResourceStringData("0FB1EB9D-30EA-4909-B51C-9806EC66496D", Caption = "Nature of Transaction", ShortCaption = "Nature of Trans.", FullDescription = "[99 05 001 000] Nature of Transaction", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("43F8C369-009C-4252-939A-4B3C9B0F8E13", ShortCaption = "[8/5] Trans. Nature", MediumCaption = "[8/5] Trans. Nature", Caption = "[8/5] Nature of Transaction", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JZ_ValuationCode
		{
			get => base.JZ_ValuationCode;
			set => base.JZ_ValuationCode = value;
		}

		[ResourceStringData("7490AA9D-5923-4406-9439-F4FD7D904DAD", ShortCaption = "[4/11] Inv. Amount", MediumCaption = "[4/11] Inv. Amount", Caption = "[4/11 & 4/10] Invoice Amount & Currency", FullDescription = "[4/11] Total Amount Invoiced & [4/10] Invoice Currency", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZDecimal JZ_InvoiceAmount { get => base.JZ_InvoiceAmount; set => base.JZ_InvoiceAmount = value; }

		[ResourceStringData("0D157C46-8290-4178-929C-676241B1C960", Caption = "[4/1] Incoterm", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JZ_IncoTerm { get => base.JZ_IncoTerm; set => base.JZ_IncoTerm = value; }

		[ResourceStringData("E0871434-8199-49F8-B419-762DA8BA8238", Caption = "Incoterm Location", FullDescription = "[14 01 037 000] Delivery Terms Location", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[MaxLength(35)]
		public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

		[ResourceStringData("17716024-E496-4E92-9BE2-DB078361F670", Caption = "Delivery Terms", FullDescription = "[14 01 000 000] Delivery Terms Text", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JZ_AdditionalTerms { get => base.JZ_AdditionalTerms; set => base.JZ_AdditionalTerms = value; }

		public bool HasSupportingDocumentForExportWithInvoiceNumber => Factory.GetValue(ref hasSupportingDocumentForExportWithInvoiceNumberCached, () => SupportingDocuments.Cast<SupportingDocument>().Any(supportingDoc => !supportingDoc.CSI_ReferenceNumber.IsEmpty && Constants.SupportingDocumentCodes.InvoiceDocumentTypesForExport.Contains(supportingDoc.CSI_Code)));
		CachedProperty<bool> hasSupportingDocumentForExportWithInvoiceNumberCached;

		public bool HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber => Factory.GetValue(ref hasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumberCached, () => CusEntryInstructions.Cast<CusEntryInstruction>().Any(instruction => instruction.HasSupportingDocumentForExportWithInvoiceNumber));
		CachedProperty<bool> hasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumberCached;

		public bool HasEstimatedTimeOfDepartureAdditionalInfo => Factory.GetValue(ref hasEstimatedTimeOfDepartureAdditionalInfoCached, () => AdditionalInfos.Cast<AdditionalInfo>().Any(addInfo => addInfo.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture)));
		CachedProperty<bool> hasEstimatedTimeOfDepartureAdditionalInfoCached;

		public bool HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo => Factory.GetValue(ref hasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfoCached, () => CusEntryInstructions.Cast<CusEntryInstruction>().
			Any(instruction => instruction.CEI_SubStyle != EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic && instruction.CEI_SubStyle != EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF && instruction.HasEstimatedTimeOfDepartureAdditionalInfo));
		CachedProperty<bool> hasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfoCached;

		public bool HasTransportDocument => Factory.GetValue(ref hasTransportDocumentCached, () => AdditionalInfos.Cast<AdditionalInfo>().Any(addInfo => addInfo.IsATransportDocument));
		CachedProperty<bool> hasTransportDocumentCached;

		public bool HasEntryInstructionWithTransportDocument => Factory.GetValue(ref hasEntryInstructionWithTransportDocumentCached, () => CusEntryInstructions.Cast<CusEntryInstruction>().Any(instruction => instruction.HasTransportDocument));
		CachedProperty<bool> hasEntryInstructionWithTransportDocumentCached;

		public bool HasEntryInstructionWithInvoiceHeaderHavingTransportDocument => Factory.GetValue(ref hasEntryInstructionWithInvoiceHeaderHavingTransportDocumentCached, () => CusEntryInstructions.Cast<CusEntryInstruction>().Any(instruction => instruction.HasInvoiceHeaderWithTransportDocument));
		CachedProperty<bool> hasEntryInstructionWithInvoiceHeaderHavingTransportDocumentCached;

		CachedProperty<ISet<ZString>> requestedProceduresCache;
		internal ISet<ZString> RequestedProcedures => Factory.GetValue(
		ref requestedProceduresCache,
			() => InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.JI_Calc_RequestedProcedure).Distinct().ToImmutableHashSet()
		);

		public bool HasInvoiceLineWithPreviousProcedure21Or22 => Factory.GetValue(ref hasInvoiceLineWithPreviousProcedure21Or22Cached, () => InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.JI_Calc_PreviousProcedure).Any(x => x == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._21 || x == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._22));
		CachedProperty<bool> hasInvoiceLineWithPreviousProcedure21Or22Cached;

		public bool HasInvoiceLineWithPreviousProcedure0700 => Factory.GetValue(ref hasInvoiceLineWithPreviousProcedure0700Cached, () => InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.RequestedPreviousProcedure).Any(x => x == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._0700));
		CachedProperty<bool> hasInvoiceLineWithPreviousProcedure0700Cached;

		public bool AllRelatedEntryInstructionsStatisticalValueRequired => Factory.GetValue(ref allRelatedEntryInstructionsStatisticalValueRequired, () => CusEntryInstructions.Any() && CusEntryInstructions.Cast<CusEntryInstruction>().All(instruction => instruction.StatisticalValueRequired));
		CachedProperty<bool> allRelatedEntryInstructionsStatisticalValueRequired;

		public bool HasSupportingDocumentWithCode(ZString csi_code)
		{
			hasSupportingDocumentWithCode = hasSupportingDocumentWithCode ?? new Dictionary<ZString, CachedProperty<bool>>();
			var cachedProperty = hasSupportingDocumentWithCode.GetOrAdd(csi_code, () => default);
			return Factory.GetValue(ref cachedProperty, () => SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == csi_code));
		}
		Dictionary<ZString, CachedProperty<bool>> hasSupportingDocumentWithCode;

		public bool HasU164OrU166OrN865SupportingDocument => Factory.GetValue(ref hasU164OrU166OrN865SupportingDocumentCached, () => SupportingDocumentCollectionExtensions.HasU164OrU166OrN865SupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasU164OrU166OrN865SupportingDocumentCached;

		public bool HasU165OrU167SupportingDocument => Factory.GetValue(ref hasU165OrU167SupportingDocumentCache, () => SupportingDocumentCollectionExtensions.HasU165OrU167SupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasU165OrU167SupportingDocumentCache;

		public bool HasN018SupportingDocument => Factory.GetValue(ref hasN018SupportingDocumentCached, () => SupportingDocumentCollectionExtensions.HasN018SupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasN018SupportingDocumentCached;

		public IEnumerable<SupportingDocument> SupportingDocumentsAtAnyLevel => Factory.GetValue(ref supportingDocumentsAtAnyLevelCached, () =>
			SupportingDocuments.Cast<SupportingDocument>()
			.Union(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.SupportingDocuments.Cast<SupportingDocument>())));
		CachedProperty<IEnumerable<SupportingDocument>> supportingDocumentsAtAnyLevelCached;

		public IEnumerable<AdditionalInfo> AdditionalInfosAtAnyLevel => Factory.GetValue(ref additionalInfoAtAnyLevelCached, () =>
			AdditionalInfos.Cast<AdditionalInfo>()
			.Union(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.AdditionalInfos.Cast<AdditionalInfo>())));
		CachedProperty<IEnumerable<AdditionalInfo>> additionalInfoAtAnyLevelCached;

		internal bool HasMRNPreviousDocuments => Factory.GetValue(ref hasMRNPreviousDocumentsCached, () => PreviousDocuments.HasMRNPreviousDocuments());
		CachedProperty<bool> hasMRNPreviousDocumentsCached;

		public bool HasMutuallyExclusiveSupportingDocument => Factory.GetValue(ref hasMutuallyExclusiveSupportingDocument, () => SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasMutuallyExclusiveSupportingDocument;

		public bool IsProcedureCode44 => Factory.GetValue(ref isProcedureCode44Cached, () =>
		{
			var invoiceLines = InvoiceLines;
			return invoiceLines.Any() && invoiceLines.Cast<JobComInvoiceLine>().All(x => x.IsProcedureCode44);
		});
		CachedProperty<bool> isProcedureCode44Cached;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Ireland;

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			if (PersistentDeclaration is JobDeclaration declaration)
			{
				return GetJobComInvoiceHeaderValidationByDeclarationMessageType(declaration);
			}

			return GetJobComInvoiceHeaderValidationByInvoiceDirection();
		}

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var originalEntryInstruction = InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.EntryInstruction != null).Select(x => x.EntryInstruction)
					.Distinct().OrderBy(x => x.CEI_SubStyle).ThenBy(x => x.CEI_Description).FirstOrDefault(x => x.CEI_DateForDuty.IsValid);
				return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
			}
		}

		Customs.Business.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationByInvoiceDirection()
		{
			var direction = JZ_StandAloneInvoiceDirection;
			if (direction.EqualsIgnoringCase(IEJobMessageTypeList.Codes.Export))
			{
				return new ExportJobComInvoiceHeaderValidation(this);
			}
			else if (direction.EqualsIgnoringCase(IEJobMessageTypeList.Codes.Import))
			{
				return new ImportJobComInvoiceHeaderValidation(this);
			}

			return new EU.Business.Declaration.JobComInvoiceHeaderValidation(this);
		}

		Customs.Business.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationByDeclarationMessageType(JobDeclaration declaration)
		{
			switch (declaration.JE_MessageType.ToUpperInvariant())
			{
				case IEJobMessageTypeList.Codes.ReExport:
					return new ReExportJobComInvoiceHeaderValidation(this);
				case IEJobMessageTypeList.Codes.ExitSummary:
					return new ExitSummaryJobComInvoiceHeaderValidation(this);
				case IEJobMessageTypeList.Codes.Export:
					return new ExportJobComInvoiceHeaderValidation(this);
				case IEJobMessageTypeList.Codes.Import:
					return new ImportJobComInvoiceHeaderValidation(this);
				default:
					return new EU.Business.Declaration.JobComInvoiceHeaderValidation(this);
			}
		}

		protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();

		protected override bool IsValidToDefaultIncoTermFromSupplier => JobDeclaration == null || !(JobDeclaration.IsExitSummary || JobDeclaration.IsReExport);

		public ZBool IsIncoTermPlaceVisible => ZG_AgreedPlaceCode.Length == 2 && JobDeclaration.IsExport;

		protected override bool IsApplicationCodeAllowedForDefaultingSupportingDocumentCore => true;

		public ZBool IsRuleBR4010Active => !CusEntryInstructions.Cast<CusEntryInstruction>().All(x => JobDeclaration.IsUCC5 && (x.IsH2 || x.IsH3 || x.IsH4 || x.IsI1));

		public override ZBool AddingSupportingDocumentAutomaticallyEnabled => JobDeclaration is JobDeclaration declaration && !(declaration.IsReExport || declaration.IsExitSummary) && base.AddingSupportingDocumentAutomaticallyEnabled;

		public IEnumerable<ZString> C100SupportingDocReferences => Factory.GetValue(ref c100SupportingDocReferencesCached, () => SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == SupportingDocumentCodes._C100 && !x.CSI_ReferenceNumber.IsEmpty).Select(r => r.CSI_ReferenceNumber));
		CachedProperty<IEnumerable<ZString>> c100SupportingDocReferencesCached;

		public bool Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction => Factory.GetValue(
			ref has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction,
			GetHas51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction
		);
		CachedProperty<bool> has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction;

		bool GetHas51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction() =>
			InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.IsInwardProcessingProcedure51)
			&& GetHeaderLevelSupportingDocuments().Any(SupportingDocumentCollectionExtensions.IsAuthorisationInwardProcessingProcedure)
			&& GetHeaderLevelAdditionalInfos().Any(AdditionalInfoCollectionExtensions.IsAuthorisationForSpecialProcedure);

		IEnumerable<SupportingDocument> GetHeaderLevelSupportingDocuments() => SupportingDocuments.Cast<SupportingDocument>().Union(
			CusEntryInstructions.Cast<CusEntryInstruction>().SelectMany(instruction => instruction.SupportingDocuments.Cast<SupportingDocument>())
		);

		IEnumerable<AdditionalInfo> GetHeaderLevelAdditionalInfos() => AdditionalInfos.Cast<AdditionalInfo>().Union(
			CusEntryInstructions.Cast<CusEntryInstruction>().SelectMany(instruction => instruction.AdditionalInfos.Cast<AdditionalInfo>())
		);
	}
}
