using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.Business;

public class CusEntryHeader : AutoCHCusEntryHeader
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoCHCusEntryHeader.Schema
	{
		public const string SelectionResult = nameof(CusEntryHeader.SelectionResult);
		public const string SelectionResultDescription = nameof(CusEntryHeader.SelectionResultDescription);
		public const string AccessCode = nameof(CusEntryHeader.AccessCode);
		public const string PhaseStatusDescription = nameof(CusEntryHeader.PhaseStatusDescription);
	}

	protected override ZString HumanReadableNameCore => Res.GetString("577FA3DC-4752-4A9A-8A5F-C4F4ED349298", "Customs Entry Header");

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusEntryHeaderFetchStrategy(this);

	public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

	public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

	public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

	[ChildEditable(true)]
	public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

	[ChildEditable(true)]
	public new Customs.Business.ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

	public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

	protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

	protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

	protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

	protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

	protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

	protected override string GetTaxCode()
	{
		return Core.Constants.Customs.CusEntryFeeTypes.VAT;
	}

	protected override void OnFactorySavingBeforeTransactionCore()
	{
		base.OnFactorySavingBeforeTransactionCore();

		LogStatusChangeIfChanged();
	}

	protected void LogStatusChangeIfChanged()
	{
		if ((ZString)CH_StatusInfo.OriginalValue != CH_Status)
		{
			var reference = CreateMessageStatusChangeEventReference();
			if (!Logs.LogsNotInDB.Any(log => log.SL_SE_NKEvent == Events.MessageStatusChange.Code))
			{
				Logs.AddNew(Events.MessageStatusChange, reference);
			}
		}
	}

	ZString CreateMessageStatusChangeEventReference()
	{
		var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, (ZString)CH_StatusInfo.OriginalValue },
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, CH_Status }
			};
		return StmALog.GenerateEventReference(ZString.Empty, parameters);
	}

	public ZString SelectionResult => MovementReferenceCusEntryNumber?.CE_EntryStatus ?? ZString.Empty;

	public ZPropertyInfo SelectionResultInfo => GetZPropertyInfo(Schema.SelectionResult);

	public ZString SelectionResultDescription => Lookups.SelectionResultList.GetDescriptionFromCode(SelectionResult);

	public ZPropertyInfo SelectionResultDescriptionInfo => GetZPropertyInfo(Schema.SelectionResultDescription);

	[ResourceStringData("CH.Business.CusEntryHeader|CH_EntrySubmittedDate", Caption = "Submitted Date", FullDescription = "Submitted Date of the Entry", ShortCaption = "Sub. Date")]
	public override ZDateTime CH_EntrySubmittedDate { get => base.CH_EntrySubmittedDate; set => base.CH_EntrySubmittedDate = value; }

	[ResourceStringData("CH.Business.CusEntryHeader|AccessCode", Caption = "Access Code", FullDescription = "Access Code of the Entry", ShortCaption = "Acc. Code")]
	public ZString AccessCode => AccessCodeCusEntryNumber?.CE_EntryNum ?? ZString.Empty;

	public ZPropertyInfo AccessCodeInfo => GetZPropertyInfo(Schema.AccessCode);

	[ResourceStringData("CH.Business.CusEntryHeader|MessageStatusDescription", Caption = "Message Status Description", FullDescription = "Description of the Message Status", MediumCaption = "Message Status Desc.", ShortCaption = "Msg. Status Desc.")]
	public override ZString MessageStatusDescription => base.MessageStatusDescription;

	[MaxLength(3)]
	[ResourceStringData("CH.Business.CusEntryHeader|CH_PhaseStatus", Caption = "Phase Status", ShortCaption = "Ph. Status")]
	public override ZString CH_PhaseStatus { get => base.CH_PhaseStatus; set => base.CH_PhaseStatus = value; }

	[ResourceStringData("CH.Business.CusEntryHeader|PhaseStatusDescription", Caption = "Phase Status Description", FullDescription = "Description of the Phase Status", MediumCaption = "Phase Status Desc.", ShortCaption = "Ph. Status Desc.")]
	public ZString PhaseStatusDescription => Lookups.PhaseStatusList.GetDescriptionFromCode(CH_PhaseStatus);

	public ZPropertyInfo PhaseStatusDescriptionInfo => GetZPropertyInfo(Schema.PhaseStatusDescription);

	[ResourceStringData("CH.Business.CusEntryHeader|EComplaintStatusList", Caption = "ECom Status", FullDescription = "Last ECom Status", ShortCaption = "ECom")]
	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.EComplaintStatusList))]
	[MaxLength(Schema.CH_LastEComplaintStatusMaxLength)]
	public override ZString CH_LastEComplaintStatus { get => base.CH_LastEComplaintStatus; set => base.CH_LastEComplaintStatus = value; }

	[ResourceStringData("CH.Business.CusEntryHeader|EComMessageStatusDescription", Caption = "ECom Status Description", FullDescription = "Description of the Last ECom Status", ShortCaption = "ECom Status")]
	public ZString EComMessageStatusDescription
	{
		get { return Lookups.EComplaintStatusList.GetDescriptionFromCode(CH_LastEComplaintStatus) ?? Res.GetString("46EACFCE-E1CC-4F99-ACA9-13A51688DA2A", "Unknown"); }
	}

	[ResourceStringData("CH.Business.CusEntryHeader|ConfirmedDuty", Caption = "Confirmed Duty", FullDescription = "Sum of confirmed Duties", ShortCaption = "Conf. DTY")]
	public ZDecimal ConfirmedDuty
	{
		get { return ConfirmedCharges.Where(c => c.C1_ChargeType != Core.Constants.Customs.CusEntryFeeTypes.VAT).Sum(c => c.C1_ChargeAmount); }
	}

	[ResourceStringData("CH.Business.CusEntryHeader|ConfirmedVAT", Caption = "Confirmed VAT", FullDescription = "Sum of confirmed VAT", ShortCaption = "Conf. VAT")]
	public ZDecimal ConfirmedVAT
	{
		get { return ConfirmedCharges.Where(c => c.C1_ChargeType == Core.Constants.Customs.CusEntryFeeTypes.VAT).Sum(c => c.C1_ChargeAmount); }
	}

	#region Implementation

	public CusEntryNumber AccessCodeCusEntryNumber => CusEntryNumber.Load(this, CusEntryNumberTypes.Switzerland.AccessCode, CountryCode);

	public bool IsDeclarationIntegrated => Declaration?.IsDeclarationIntegrated ?? true;

	protected override string GetDutyCode()
	{
		return IsDeclarationIntegrated ? Customs.Business.FeeTypeList.Codes.A00 : base.GetDutyCode();
	}

	#endregion

	protected override ZString EntryNumberType => CusEntryNumberTypes.Standard.MovementReferenceNumber;

	public override ZString DefaultStatusDescription => ZString.Empty;

	public override bool ShouldLogCustomsClearedToDeclarationOrShipment => false;

	public override bool ShouldLogEntryStatus => true;

	protected override bool IsStatusClear(string status) => status == CHLogicalStatusList.Codes.Accepted;

	protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => !IsStatusClear(originalStatus) && IsStatusClear(newStatus);

	protected override bool IsStatusChangingFromAmendmentPendingToCleared(ZString originalStatus, ZString newStatus) => !IsStatusClear(originalStatus) && IsStatusClear(newStatus);

	public override bool HasBeenWithdrawn => false;

	public EComCHEDIMessageCollectionView EComMessages => new EComCHEDIMessageCollectionView((CHEDIMessageCollection)base.Messages);

	public NonEComCHEDIMessageCollectionView NonEComMessages => new NonEComCHEDIMessageCollectionView((CHEDIMessageCollection)base.Messages);

	protected override EDIMessageCollection GetNewMessageCollection() => new CHEDIMessageCollection(this);

	protected override ZDateTime GetCustomsClearedDateWithoutUsingLoggedEvent() => MovementReferenceCusEntryNumber?.CE_IssueDate ?? ZDateTime.Now;

	public override ZString ClearanceEventReference => CreateClearanceEventReference(MovementReferenceCusEntryNumber?.CE_EntryNum ?? ZString.Empty, CH_EntryStatus);

	internal ZString CreateClearanceEventReference(ZString entryNumber, ZString entryStatus)
	{
		var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsDeclarationNumber, entryNumber },
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsStatus, entryStatus }
			};

		return StmALog.GenerateEventReference(ZString.Empty, parameters);
	}

	public bool IsMessageStatusSent => CH_Status.ToString() is CHLogicalStatusList.Codes.Sent or CHLogicalStatusList.Codes.Acknowledged;

	public void AccessCodeSetter(ZString entryNumber, ZDateTime issueDate) => SetEntryNumber(CusEntryNumberTypes.Switzerland.AccessCode, entryNumber, issueDate);

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateCH_BGMReferenceIfRequired();
	}

	public void PopulateCH_BGMReferenceIfRequired()
	{
		PopulateNumberPropertyIfRequired(CH_BGMReferenceInfo, x => (ZString)Env.NumberFountains.GetCHTraderDeclarationNumber(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.LicenceKeyIdentifier).GetNextFormatted(Factory));
	}

	public int GetLatestDeclarationVersion() => int.TryParse(EntryNumber.Split('.').ElementAtOrDefault(1), out var version) ? version : 0;

	public ZString CloseEComplaint()
	{
		var message = ZString.Empty;

		switch (CH_LastEComplaintStatus.ToString())
		{
			case EComplaintStatusList.Codes.NotSent:
				message = Res.GetString("80DC283B-DDBE-40F4-BA77-8702FFFE09C8", "There is no ECom open.");
				break;
			case EComplaintStatusList.Codes.Sent:
				message = Res.GetString("637EBDFD-A92B-4823-A5D9-0C43486FAAED", "Cannot close, a response is pending.");
				break;
			case EComplaintStatusList.Codes.Closed:
				message = Res.GetString("624560EE-2C45-49F2-B219-D8AB0258B95C", "The ECom has already been closed.");
				break;
			default:
				var oldEComplainStatus = CH_LastEComplaintStatus;
				CH_LastEComplaintStatus = EComplaintStatusList.Codes.Closed;
				Logs.AddNew(Enterprise.ZArchitecture.Business.Events.EComStatusChange, new KeyValuePair<string, string>[]
				{
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, CH_LastEComplaintStatus),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Old, oldEComplainStatus),
				});
				break;
		}

		return message;
	}

	protected override ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPaymentCode)
	{
		var result = ZDecimal.Zero;
		var rateCodes = GetRateCodes(chargeTypeElement.Code);

		var useConfirmedCharges = ConfirmedCharges.Any() || HasAnyConfirmedFeesOnAnyMergedLine;

		foreach (var entryLine in MergedLines)
		{
			var fees = useConfirmedCharges ? entryLine.ConfirmedFees.Cast<CusEntryLineFee>() : entryLine.Fees.Cast<CusEntryLineFee>();
			var nonZeroFees = fees.Where(x => x.CF_ChargeAmount != 0).ToArray();
			if (nonZeroFees.Any())
			{
				foreach (var rateCode in rateCodes)
				{
					result += nonZeroFees.Where(x => x.CF_ChargeType == rateCode).Sum(x => x.CF_ChargeAmount);
				}
			}
		}

		return result;
	}

	public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPaymentCode, ILogger logger)
	{
		var paidBy = feeCode == CusEntryFeeTypes.VAT ? Declaration.JE_VATPaidBy : Declaration.JE_PaymentMethod;
		return paidBy == DeclarationPayerList.Codes.Declarant;
	}

	public new class Loader : BusinessObject.Loader
	{
		public Loader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(CusEntryHeader);
		}

		public CusEntryHeader FindByEntryNumber(string gdrn, bool anyVersion = false, string entryType = CusEntryNumberTypes.Standard.MovementReferenceNumber, string jobMessageType = null, ZGuid? excludeJobDeclarationPK = null)
		{
			if (gdrn.IsNullOrEmpty())
			{
				return null;
			}

			if (anyVersion)
			{
				gdrn = CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(gdrn);
			}

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			if (jobMessageType != null)
			{
				declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, jobMessageType);
			}
			if (excludeJobDeclarationPK != null)
			{
				declarationSubQuery.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, excludeJobDeclarationPK);
			}

			entryHeaderQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);

			var branchesSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchesSubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			declarationSubQuery.AddSubQuery(branchesSubQuery, JoinCondition.And);

			var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);

			entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, gdrn);
			if (anyVersion)
			{
				entryNumberSubQuery.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, gdrn + ".");
			}

			entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);
			entryHeaderQuery.AddSubQuery(entryNumberSubQuery, JoinCondition.And);

			return Factory.LoadTop1<CusEntryHeader>(entryHeaderQuery);
		}

		public CusEntryHeader FindByBGMReference(ZString bgmReference)
		{
			if (bgmReference.IsEmpty)
			{
				return null;
			}
			var filter = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, bgmReference);
			return Factory.LoadTop1<CusEntryHeader>(filter);
		}
	}
}
