using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceHeader : BaseJobComInvoiceHeader,
												Integration.Customs.CH.IJobComInvoiceHeader,
												Integration.Customs.ICusSupportingInfoTypeSupporter,
												ISpecialMentions,
												ICusSupportingInfoParent,
												ISupportingDocumentParent,
												ITransportDocumentParent
{
	public new class Schema : AutoJobComInvoiceHeader.Schema
	{
		public const string SpecialMentions = nameof(JobComInvoiceHeader.SpecialMentions);
	}

	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceHeader Clone() => (JobComInvoiceHeader)base.Clone();

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

	public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

	public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

	public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

	[ChildEditable(true)]
	public new JobComInvChargeCollection<InvoiceCharge> Charges => (JobComInvChargeCollection<InvoiceCharge>)base.Charges;

	[ChildEditable(true)]
	public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

	public new JobComInvoiceGroupHeader Master => (JobComInvoiceGroupHeader)base.Master;

	public new JobComInvoiceGroupHeader GroupHeader => (JobComInvoiceGroupHeader)base.GroupHeader;

	public new Bill Bill => (Bill)base.Bill;

	[ChildEditable(true)]
	public PreviousDocumentCollection PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				previousDocuments = new PreviousDocumentCollection(this);
				previousDocuments.Load();
				RegisterEditableChildObject(previousDocuments);
			}
			return previousDocuments;
		}
	}
	PreviousDocumentCollection previousDocuments;

	public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = new Dictionary<ZString, Type>();
		result.Add(Common.CH.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument));
		result.Add(Common.CH.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument));
		result.Add(Common.CH.CusSupportingInfoTypeList.Codes.TransportDocument, typeof(TransportDocument));
		return result;
	}

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceHeader|SpecialMentions", Caption = "Special Mentions")]
	[BusinessObjectMaxLengthTestExclude]
	public ZString SpecialMentions
	{
		get => SpecialMentionsNote.Text;
		set
		{
			SpecialMentionsNote.SetNoteText(this, SpecialMentionsInfo, SpecialMentionsHelper.FormatText(value));
			if (!IsValidationSuspended)
			{
				Validation.ValidateSpecialMentions();
				JobDeclaration.Invoices.Cast<JobComInvoiceHeader>().ForEach(x => x.Validation.ValidateSpecialMentions());
			}
		}
	}

	public ZPropertyInfo SpecialMentionsInfo => GetZPropertyInfo(Schema.SpecialMentions);

	HiddenTextNote SpecialMentionsNote => specialMentionsNote ?? (specialMentionsNote = new HiddenTextNote(this, SpecialMentionsHelper.NoteType.Description));
	HiddenTextNote specialMentionsNote;

	public int CountOfSpecialMentionsLines => Factory.GetCached(ref fCountOfSpecialMentionsLines, () => SpecialMentionsHelper.CountLines(SpecialMentions));
	CachedProperty<int> fCountOfSpecialMentionsLines;

	public override ZDateTime JZ_ValuationDateOverride
	{
		get => base.JZ_ValuationDateOverride;
		set
		{
			var oldValue = JZ_ValuationDateOverride;
			base.JZ_ValuationDateOverride = value;
			if (!IsCopying && oldValue != JZ_ValuationDateOverride)
			{
				InvoiceLines?.MarkAsNeedingValidation();
				InvoiceLines?.Cast<JobComInvoiceLine>()?.ForEach(x => x.NotifyCustomsOffices.MarkAsNeedingValidation());
			}
		}
	}

	public override ZGuid JZ_JE
	{
		get => base.JZ_JE;
		set
		{
			var oldValue = JZ_JE;
			base.JZ_JE = value;
			if (!IsCopying && oldValue != JZ_JE)
			{
				InvoiceLines?.MarkAsNeedingValidationIncludingChildren();
			}
		}
	}

	public override ZDecimal JZ_InvoiceAmount
	{
		get => base.JZ_InvoiceAmount;
		set
		{
			var oldValue = JZ_InvoiceAmount;
			base.JZ_InvoiceAmount = value;
			if (!IsCopying && oldValue != JZ_InvoiceAmount)
			{
				InvoiceLines?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZDecimal JZ_InvoiceCurrExRate
	{
		get => base.JZ_InvoiceCurrExRate;
		set
		{
			var oldValue = JZ_InvoiceCurrExRate;
			base.JZ_InvoiceCurrExRate = value;
			if (!IsCopying && oldValue != JZ_InvoiceCurrExRate)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JZ_InvoiceCurrExRateType
	{
		get => base.JZ_InvoiceCurrExRateType;
		set
		{
			var oldValue = JZ_InvoiceCurrExRateType;
			base.JZ_InvoiceCurrExRateType = value;
			if (!IsCopying && oldValue != JZ_InvoiceCurrExRateType)
			{
				InvoiceLines?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JZ_RX_NKInvoice_Currency
	{
		get => base.JZ_RX_NKInvoice_Currency;
		set
		{
			var oldValue = JZ_RX_NKInvoice_Currency;
			base.JZ_RX_NKInvoice_Currency = value;
			if (!IsCopying && oldValue != JZ_RX_NKInvoice_Currency)
			{
				InvoiceLines?.MarkAsNeedingValidation();
			}
		}
	}

	public ZDecimal TotalWeightInKG => Factory.GetCached(ref fTotalWeightInKG, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => new ZWeight(line.JI_Weight, line.JI_WeightUQ).InKilogramsSafe));
	CachedProperty<ZDecimal> fTotalWeightInKG;

	public ZDecimal TotalNetWeightInKG => Factory.GetCached(ref fTotalNetWeightInKG, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => new ZWeight(line.JI_NetWeight, line.JI_NetWeightUQ).InKilogramsSafe));
	CachedProperty<ZDecimal> fTotalNetWeightInKG;

	public bool HasSimplifiedInvoiceLines => Factory.GetCached(ref hasSimplifiedInvoiceLines, () => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EntryInstruction?.IsSimplified ?? false));
	CachedProperty<bool> hasSimplifiedInvoiceLines;

	public bool HasOrdinaryInvoiceLines => Factory.GetCached(ref hasOrdinaryLinvoiceLines, () => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EntryInstruction?.IsOrdinary ?? false));
	CachedProperty<bool> hasOrdinaryLinvoiceLines;

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Switzerland;

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceHeader|JZ_UCR", Caption = "Ref. No./UCR")]
	public override ZString JZ_UCR
	{
		get => base.JZ_UCR;
		set
		{
			var oldValue = JZ_UCR;
			base.JZ_UCR = value;
			if (!IsCopying && oldValue != JZ_UCR)
			{
				JobDeclaration?.MarkAsNeedingValidation();
			}
		}
	}

	protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => new JobComInvoiceHeaderValidation(this);

	protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		if (JobDeclaration != null)
		{
			return new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
		}
		return null;
	}

	#region SupportingDocuments

	[ChildEditable]
	public SupportingDocumentCollection SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocuments());
	SupportingDocumentCollection supportingDocuments;

	SupportingDocumentCollection GetSupportingDocuments()
	{
		var result = CreateNewSupportingDocumentCollection();

		result.Load();
		RegisterEditableChildObject(result);

		return result;
	}

	SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public ZDateTime DateOfValuation => EffectiveValuationDate;

	public ZDateTime EffectiveAssessmentDate => EffectiveValuationDate;

	public HugeSequenceNumberGenerator AdditionalInformationLineNumberGenerator => null;

	public void ValidateNonTradingGoods() { }

	bool ISupportingDocumentParent.IsGSPCertificateRequired => Factory.GetCached(ref fIsGSPCertificateRequired,
		() => InvoiceLines.Cast<ISupportingDocumentParent>().Any(line => line.IsGSPCertificateRequired));
	CachedProperty<bool> fIsGSPCertificateRequired;

	IEnumerable<SupportingDocument> ISupportingDocumentParent.SupportingDocumentsIncludingInherited => SupportingDocuments.Cast<SupportingDocument>();

	void ICusSupportingInfoParent.MarkAsNeedingValidation() => InvoiceLines.ForEach(l => l.MarkAsNeedingValidation());

	#endregion

	#region TransportDocuments

	[ChildEditable]
	public TransportDocumentCollection TransportDocuments => transportDocuments ?? (transportDocuments = GetTransportDocuments());
	TransportDocumentCollection transportDocuments;

	TransportDocumentCollection GetTransportDocuments()
	{
		var result = CreateNewTransportDocumentCollection();

		result.Load();
		RegisterEditableChildObject(result);

		return result;
	}

	TransportDocumentCollection CreateNewTransportDocumentCollection() => new TransportDocumentCollection(this);

	public IEnumerable<TransportDocument> TransportDocumentsIncludingInherited => TransportDocuments.Cast<TransportDocument>();

	#endregion

	protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
	{
		var collection = new InvoiceLineDependentCollection(this);
		collection.Load();
		return new JobComInvoiceLineViewCollection(this, collection);
	}

	protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new JobComInvChargeCollection<InvoiceCharge>(this);

	protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);

	protected override void OnFactorySaving()
	{
		if (!JobDeclaration?.IsExport ?? false)
		{
			TransportDocuments.RemoveAndDeleteAll();
		}

		base.OnFactorySaving();
	}

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new Strategy(this);

	class Strategy : BaseJobComInvoiceHeaderFetchStrategy
	{
		public Strategy(JobComInvoiceHeader invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
		}
	}
}
