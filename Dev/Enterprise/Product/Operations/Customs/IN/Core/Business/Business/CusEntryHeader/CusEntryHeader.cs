using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public partial class CusEntryHeader : IMessageAttachee
{
	public override bool HasBeenWithdrawn => false;

	public override ZString CH_BGMReference
	{
		get
		{
			return base.CH_BGMReference;
		}
		set
		{
			var oldValue = CH_BGMReference;
			base.CH_BGMReference = value;
			if (!IsCopying && oldValue != CH_BGMReference)
			{
				EntryInstruction?.LocalReferenceNumberInfo.RefreshBinding();
			}
		}
	}

	public override ZDateTime CH_SystemCreateTimeUtc
	{
		get
		{
			return base.CH_SystemCreateTimeUtc;
		}
		set
		{
			var oldValue = CH_SystemCreateTimeUtc;
			base.CH_SystemCreateTimeUtc = value;
			if (!IsCopying && oldValue != CH_SystemCreateTimeUtc)
			{
				EntryInstruction?.LocalReferenceNumberDateInfo.RefreshBinding();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.MessageStatusList))]
	public override ZString CH_Status { get => base.CH_Status; set => base.CH_Status = value; }

	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_EntryStatusList))]
	public override ZString CH_EntryStatus { get => base.CH_EntryStatus; set => base.CH_EntryStatus = value; }

	public ZDateTime CreateTime => createTime.IsValid ? createTime : createTime = CH_SystemCreateTimeUtc.ToLocalBranchTime();
	ZDateTime createTime;

	ZGuid IMessageAttachee.BranchPK => Declaration?.JE_GB ?? ZGuid.Empty;

	IBusinessObjectCollection IMessageAttachee.Messages => Messages;

	ZString IMessageAttachee.MessageOwner => Declaration?.JE_CustomsOffice ?? ZString.Empty;

	ZString IMessageAttachee.MessageStatus { get => CH_Status; set => CH_Status = value; }

	ZString IMessageAttachee.CustomsStatus { get => CH_EntryStatus; set => CH_EntryStatus = value; }

	ZString IMessageAttachee.CalculateStatusAfterSending(ZString messageType) => ZString.Empty;

	public void RollbackChangesOnStatus()
	{
		CH_Status = IsInDatabase ? (ZString)CH_StatusInfo.OriginalValue : ZString.Empty;
		CH_EntryStatus = IsInDatabase ? (ZString)CH_EntryStatusInfo.OriginalValue : ZString.Empty;
	}

	public override void OnSaving()
	{
		base.OnSaving();
		if (CH_BGMReference.IsEmpty && ShouldPopulateBGMReferenceOnSaving && !IsInDatabase)
		{
			CH_BGMReference = LocalReferenceNumberGenerator.Generate(Declaration.JE_MessageType, ZDate.Today);
		}
	}

	bool ShouldPopulateBGMReferenceOnSaving => Declaration != null && (Declaration.IsExport || Declaration.IsImport);

	LocalReferenceNumberGenerator LocalReferenceNumberGenerator => localReferenceNumberGenerator ??= new LocalReferenceNumberGenerator(Factory);
	LocalReferenceNumberGenerator localReferenceNumberGenerator;

	public IReadOnlyList<SWConstituent> SWConstituents => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.SWConstituents.Cast<SWConstituent>()).ToArray();

	public IReadOnlyList<JobWork> JobWorks => InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.JobWorks.Cast<JobWork>()).ToArray();

	#region Loader

	public new class Loader : Customs.Business.CusEntryHeader.Loader
	{
		public Loader(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusEntryHeader FindByEntryNumber(string entryType, string entryNumber, ZDateTime issueDate)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var childQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			childQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			childQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, issueDate);
			childQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			childQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.India);
			query.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, childQuery, JoinCondition.And);
			return Factory.LoadTop1<CusEntryHeader>(query);
		}
	}

	#endregion
}
