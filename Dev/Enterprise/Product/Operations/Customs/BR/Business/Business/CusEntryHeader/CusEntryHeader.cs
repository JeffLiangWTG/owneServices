using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	[SystemDefinedValues]
	public partial class CusEntryHeader : AutoBRCusEntryHeader, Integration.Customs.BR.ICusEntryHeader, IMessageAttachee
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoBRCusEntryHeader.Schema
		{
			public const int EntryAccessKeyMaxLength = 32;
			public const string EntryAccessKey = "EntryAccessKey";
			public const string CargoStatusDescription = "CargoStatusDescription";
			public const string AdministrativeStatusDescription = "AdministrativeStatusDescription";
			public const string RiskChannelDescription = "RiskChannelDescription";
			public const string ImportLicenseIdentifier = "ImportLicenseIdentifier";
			public const string ImportDeclarationNumber = "ImportDeclarationNumber";
			public const string EntryReferenceNumber = "EntryReferenceNumber";
		}

		#endregion

		#region Loader

		public new class Loader : Customs.Business.CusEntryHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusEntryHeader GetEntryHeaderByMRNQuery(ZString entryNumber, ZString messageType)
			{
				var query = GetEntryHeaderByEntryNumberQuery(CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumber, messageType);
				return Factory.LoadTop1<CusEntryHeader>(query);
			}

			public CusEntryHeader GetEntryHeaderByImportLicenseIdentifier(ZString identifier)
			{
				var query = GetEntryHeaderByEntryNumberQuery(CusEntryNumberTypes.Brazil.ImportLicenseIdentifier, identifier, BRJobMessageTypeList.Codes.ImportLicense);
				return Factory.LoadTop1<CusEntryHeader>(query);
			}

			ZQuery GetEntryHeaderByEntryNumberQuery(ZString entryType, ZString entryNumber, ZString messageType)
			{
				if (entryType.IsEmpty || entryNumber.IsEmpty || messageType.IsEmpty)
				{
					return ZQuery.NoResultQuery;
				}
				else
				{
					var dbQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
					var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
					subQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
					subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
					subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
					subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
					dbQuery.AddSubQuery(subQuery, JoinCondition.And);
					dbQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, messageType);
					dbQuery.OrderBy = CusEntryHeaderSchema.CH_SystemCreateTimeUtc.Name + " DESC";

					return dbQuery;
				}
			}
		}

		#endregion

		public bool CanSendRectification => !EntryNumber.IsEmpty;

		public ZBool IsImportLicense => CH_MessageType == MessageTypeList.Codes.LIC;

		public ZBool IsImportOnly => Declaration?.IsImportOnly ?? ZBool.False;

		public override bool ShouldLogEntryStatus => false;

		public override bool HasBeenLodgedAtCustoms => !EntryNumber.IsEmpty || !CH_EntryStatus.IsEmpty;

		public override bool IsWaitingForResponse => CH_Status == BRMessageStatusList.Codes.AwaitingResponse;

		public bool IsNotSent => CH_Status == BRMessageStatusList.Codes.NotSent;

		protected override bool IsStatusClear(string status) => status == BRMessageStatusList.Codes.Accepted;

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => !IsStatusClear(originalStatus) && IsStatusClear(newStatus);

		protected override bool IsStatusChangingFromAmendmentPendingToCleared(ZString originalStatus, ZString newStatus) => !IsStatusClear(originalStatus) && IsStatusClear(newStatus);

		public override ZString DefaultStatusDescription => ZString.Empty;

		public override bool IsFormalEntry => CH_MessageType != MessageTypeList.Codes.SUF;

		ZGuid IMessageAttachee.BranchPK => Declaration?.JE_GB ?? ZGuid.Empty;

		public ZString UniqueConsignmentReference
		{
			get
			{
				return LoadUCRNumber()?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				if (value.IsEmpty)
				{
					LoadUCRNumber()?.Delete();
				}
				else
				{
					LoadOrCreateUCRNumber(value);
				}
			}
		}

		CusEntryNumber LoadUCRNumber()
		{
			return Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(PK, CountryCode, CusEntryNumberTypes.Standard.UniqueConsignementReference, IsInDatabase));
		}

		#region EntryAccessKey

		CusEntryNumber EntryAccessKeyEntryNumber
		{
			get
			{
				if (fEntryAccessKey == null || fEntryAccessKey.Value.IsDeleted)
				{
					fEntryAccessKey = new CachedProperty<CusEntryNumber>(Factory, () =>
					{
						var entryAccessKeyEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Brazil.EAK, Core.Constants.CountryCodes.Brazil);
						RegisterEditableChildObject(entryAccessKeyEntryNumber);
						return entryAccessKeyEntryNumber;
					});
				}
				return fEntryAccessKey.Value;
			}
		}

		CachedProperty<CusEntryNumber> fEntryAccessKey;

		[MaxLength(Schema.EntryAccessKeyMaxLength)]
		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|EntryAccessKey", Caption = "Entry Access Key")]
		public ZString EntryAccessKey
		{
			get { return EntryAccessKeyEntryNumber.CE_EntryNum; }
			set
			{
				if (EntryAccessKey != value)
				{
					CheckMaximumLength(EntryAccessKeyInfo, value);
					EntryAccessKeyEntryNumber.CE_EntryNum = value;
					EntryAccessKeyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EntryAccessKeyInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EntryAccessKey); }
		}

		#endregion

		#region CH_CargoStatus

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|BR_CargoStatus", Caption = "Cargo Status")]
		public override ZString CH_CargoStatus
		{
			get => base.CH_CargoStatus;
			set => base.CH_CargoStatus = value;
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|CargoStatusDescription", Caption = "Cargo Status Description")]
		public ZString CargoStatusDescription
		{
			get => Lookups.CargoStatusList.GetDescriptionFromCode(CH_CargoStatus);
		}

		#endregion

		#region CH_AuthorityVersion

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|BR_AuthorityVersion", Caption = "Authority Version")]
		public override ZString CH_AuthorityVersion
		{
			get => base.CH_AuthorityVersion;
			set => base.CH_AuthorityVersion = value;
		}

		#endregion

		#region CH_AdministrativeStatus

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|BR_AdministrativeStatus", Caption = "Administrative Status")]
		public override ZString CH_AdministrativeStatus
		{
			get => base.CH_AdministrativeStatus;
			set => base.CH_AdministrativeStatus = value;
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|AdministrativeStatusDescription", Caption = "Administrative Status Description")]
		public ZString AdministrativeStatusDescription
		{
			get => Lookups.AdministrativeStatusList.GetDescriptionFromCode(CH_AdministrativeStatus);
		}

		#endregion

		#region CH_EntryReleaseDate
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|CH_EntryReleaseDate", Caption = "Release Date")]
		public override ZDateTime CH_EntryReleaseDate
		{
			get => base.CH_EntryReleaseDate;
			set => base.CH_EntryReleaseDate = value;
		}
		#endregion

		#region CH_RiskChannel

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.RiskChannelList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|BR_RiskChannel", Caption = "Risk Channel")]
		public override ZString CH_RiskChannel { get => base.CH_RiskChannel; set => base.CH_RiskChannel = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|RiskChannelDescription", Caption = "Risk Channel Description")]
		public ZString RiskChannelDescription
		{
			get => Lookups.RiskChannelList.GetDescriptionFromCode(CH_RiskChannel);
		}

		#endregion

		#region EntryNumberType

		protected override ZString EntryNumberType
		{
			get { return CusEntryNumberTypes.Standard.MovementReferenceNumber; }
		}

		#endregion

		public override bool HasBeenWithdrawn
		{
			get { return false; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			LogMessageRejectedIfRejected();
			base.OnFactorySavingBeforeTransactionCore();
		}

		void LogMessageRejectedIfRejected()
		{
			if (CH_Status == BRMessageStatusList.Codes.Rejected
				&& (ZString)CH_StatusInfo.OriginalValue != CH_Status
				&& !Logs.LogsNotInDB.Any(log => log.SL_SE_NKEvent == Events.MessageRejected.Code))
			{
				Logs.AddNew(Events.MessageRejected);
			}
		}

		protected override void OnFactorySaving()
		{
			ResetMessageStatusIfNeeded();
			base.OnFactorySaving();
		}

		void ResetMessageStatusIfNeeded()
		{
			if (CH_MessageType == MessageTypeList.Codes.CDI
				&& !CH_Status.IsEmpty && !IsWaitingForResponse && !CH_StatusInfo.HasChanges
				&& !MovementReferenceNumber.IsEmpty
				&& (EntryHeaderNeedsAmendment() || AnyEntryLineNeedsAmendment()))
			{
				CH_Status = ZString.Empty;
			}

			bool EntryHeaderNeedsAmendment() => CH_CustomsPostedStatusInfo.HasChanges && CH_CustomsPostedStatus.NeedsToSendMessage();
			bool AnyEntryLineNeedsAmendment() => AllEntryLines.Any(x => (!x.IsInDatabase || x.CL_CustomsPostedStatusInfo.HasChanges) && x.CL_CustomsPostedStatus.NeedsToSendMessage());
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateCH_BGMReferenceIfNeeded();
			PopulateImportLicenseIdentifierIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				CH_BGMReference = ZString.Empty;
				ImportLicenseIdentifier = ZString.Empty;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
		}

		#region Populate CH_BGMReference

		void PopulateCH_BGMReferenceIfNeeded()
		{
			if (IsFormalEntry && !IsInDatabase && !IsDeleted && CH_BGMReference.IsEmpty)
			{
				Declaration.PopulateJE_DeclarationReferenceIfNeeded();
				CH_BGMReference = Declaration.JE_DeclarationReference + BGMReferenceSeperator + GenerateSequence().ToString();
			}
		}

		ZInt GenerateSequence()
		{
			return Declaration.CustomsEntryHeaders.Where(x => x.IsFormalEntry).MaxOrDefault(x => x.Sequence) + 1;
		}

		ZInt Sequence => CH_BGMReference.IsEmpty ? ZInt.Zero : ZInt.ParseSafe(CH_BGMReference.Split(BGMReferenceSeperator).LastOrDefault(), ZInt.Zero);

		const char BGMReferenceSeperator = '-';

		#endregion

		#region EntryReferenceNumber

		public ZString EntryReferenceNumber => $"{DeclarationReference}/{CH_BGMReference}";

		#endregion

		#region Import License Identifier

		void PopulateImportLicenseIdentifierIfNeeded()
		{
			if (CH_MessageType == BRJobMessageTypeList.Codes.ImportLicense && !IsInDatabase && !IsDeleted && ImportLicenseIdentifier.IsEmpty)
			{
				ImportLicenseIdentifier = GetImportLicenseIdentifierEntryNumber() + Sequence.ToString("d3");
			}
		}

		public CusEntryNumber ImportLicenseIdentifierNumber => CusEntryNumber.Load(this, CusEntryNumberTypes.Brazil.ImportLicenseIdentifier, CountryCode);

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|ImportLicenseIdentifier", Caption = "Import License Id.")]
		public ZString ImportLicenseIdentifier
		{
			get
			{
				return ImportLicenseIdentifierNumber?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				if (value.IsEmpty)
				{
					ImportLicenseIdentifierNumber?.Delete();
				}
				else
				{
					CusEntryNumber.LoadOrCreate(this, BRJobMessageTypeList.Codes.ImportLicense, CountryCode).CE_EntryNum = value;
				}
			}
		}

		ZString GetImportLicenseIdentifierEntryNumber()
		{
			var licenseEntryNumber = Declaration.CustomsEntryHeaders.Select(t => t.ImportLicenseIdentifier).FirstOrDefault(t => t.Length >= 3);
			return licenseEntryNumber.IsEmpty ? (ZString)Env.NumberFountains.GetBRLicenseEntryNumber().GetNextFormatted(Factory) : licenseEntryNumber.Substring(0, licenseEntryNumber.Length - 3);
		}

		#endregion

		#region ImportDeclarationNumber

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryHeader|ImportDeclarationNumber", Caption = "Import Declaration JOB")]
		public ZString ImportDeclarationNumber => EntryInstruction?.LinkedImportDeclaration?.JE_DeclarationReference ?? ZString.Empty;

		#endregion

		#region AddCustomsUpdateLog

		public StmALog AddCustomsUpdateLog(ZDateTimeOffset date, string customsDeclarationNumber = null, string customsStatus = null, string description = null, string reason = null)
		{
			var parameters = new Dictionary<string, string>();
			if (customsDeclarationNumber != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsDeclarationNumber, customsDeclarationNumber);
			}
			if (customsStatus != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsStatus, customsStatus);
			}
			if (description != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, description);
			}
			if (reason != null)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason);
			}

			return Logs.AddNew(Events.CustomsUpdate, date, parameters.ToArray());
		}

		#endregion

		#region SiscomexUsageFees

		[ChildEditable(true)]
		public SiscomexUsageFeeCollection SiscomexUsageFees
		{
			get
			{
				if (fSiscomexUsageFees == null)
				{
					fSiscomexUsageFees = new SiscomexUsageFeeCollection(this);
					fSiscomexUsageFees.Load();
					RegisterEditableChildObject(fSiscomexUsageFees);
				}
				return fSiscomexUsageFees;
			}
		}
		SiscomexUsageFeeCollection fSiscomexUsageFees;

		#endregion

		public EDIMessage FindImportLicenceMessage(string applicationReference)
		{
			return Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.BRCustoms, new ZString[] { MessageTypeList.Codes.LIC }, EDIMessage.Direction.Transmit)
					.FirstOrDefault(x => x.EM_ApplicationReference == applicationReference);
		}

		public void ResetToOriginal()
		{
			CH_Status = ZString.Empty;
			CH_EntryStatus = ZString.Empty;
			CH_EntryReleaseDate = ZDateTime.Empty;
			CH_EntrySubmittedDate = ZDateTime.Empty;
			CH_AdministrativeStatus = ZString.Empty;
			CH_CargoStatus = ZString.Empty;
			CH_RiskChannel = ZString.Empty;
			DeleteAllCusEntryNumbers();

			Logs.AddNew(AutoEvents.CustomsEntryStatus, Constants.EntryStatus.NotSent);
		}

		public IEnumerable<CusEntryLineFee> AllMergedLinesFees => MergedLines.SelectMany(x => x.Fees).Cast<CusEntryLineFee>();
	}
}
