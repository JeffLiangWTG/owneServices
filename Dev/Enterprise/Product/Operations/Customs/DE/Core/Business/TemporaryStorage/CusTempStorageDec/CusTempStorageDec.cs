using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), nameof(CusTempStorageJobHeader.CusTempStorageDecs))]
	public abstract class CusTempStorageDec : EU.Business.CusTempStorage.CusTempStorageDec, ISequenceNumberHeader
	{
		protected CusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This would resolve to an abstract class which is undesirable")]
		public new class Schema : EU.Business.CusTempStorage.CusTempStorageDec.Schema
		{
			public const string ReferenceNumber = nameof(CusTempStorageDec.ReferenceNumber);
			public const string FormattedOwnerReferenceNumber = nameof(CusTempStorageDec.FormattedOwnerReferenceNumber);
		}

		public new CusTempStorageJobHeader StorageHeader => (CusTempStorageJobHeader)base.StorageHeader;

		public static new readonly CusTempStorageDecTypeDecider TypeDecider = new CusTempStorageDecTypeDecider();

		#region Refrence Number

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ReferenceNumber_ReadOnly))]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("a7f5c6d1-95ed-4531-8ddc-0e9c19e3058e", Caption = "Reference No.", ShortCaption = "Ref. No.")]
		public virtual ZString ReferenceNumber
		{
			get
			{
				return Factory.GetValue(ref fReferenceNumber, () => SumARegistrationNumberFormatter.Format(CusEntryNumber.CE_EntryNum));
			}
			set
			{
				var hasChanged = value != ReferenceNumber;
				if (hasChanged)
				{
					CusEntryNumber.CE_EntryNum = value.KeepAlphanumericCharacters();
					ReferenceNumberInfo.RefreshBinding();
				}
			}
		}
		CachedProperty<ZString> fReferenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(Schema.ReferenceNumber);

		protected virtual ZBool ReferenceNumber_ReadOnly => true;

		public bool ReferenceNumberIssueDatePopulated => !CusEntryNumber.CE_IssueDate.IsEmpty;

		public CusEntryNumber CusEntryNumber
		{
			get
			{
				if (cusEntryNumber == null || cusEntryNumber.IsDeleted)
				{
					cusEntryNumber = CusEntryNumber.LoadOrCreate(this, EntryNumberType, Core.Constants.CountryCodes.Germany);
					RegisterEditableChildObject(cusEntryNumber);
				}
				return cusEntryNumber;
			}
		}
		CusEntryNumber cusEntryNumber;

		protected virtual ZString EntryNumberType => CusEntryNumberTypes.Germany.SumAEntryNumber;

		#endregion

		#region Properties

		[ReadOnly(true)]
		[ResourceStringData("b93fb460-2f37-48c1-9dc5-bbb1428ea8d7", Caption = "Created")]
		public override ZDateTime STH_SystemCreateTimeUtc { get => base.STH_SystemCreateTimeUtc; set => base.STH_SystemCreateTimeUtc = value; }

		[ReadOnly(true)]
		[ResourceStringData("998610bd-abe9-4b0b-aab2-1c8f2a1d6188", Caption = "Status")]
		public override ZString STH_MessageStatus { get => base.STH_MessageStatus; set => base.STH_MessageStatus = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageDecLookups.IdentificationIndicatorList))]
		[ResourceStringData("6633815d-c9f2-416e-903b-642ee0ec9ceb", Caption = "Identification Type", MediumCaption = "ID Type", ShortCaption = "Type")]
		public override ZString STH_IdentificationIndicator { get => base.STH_IdentificationIndicator; set => base.STH_IdentificationIndicator = value; }

		[ResourceStringData("692e721d-1855-4e2e-95b5-5a71e1d20d03", Caption = "Additional Information")]
		public override ZString STH_AdditionalInformation { get => base.STH_AdditionalInformation; set => base.STH_AdditionalInformation = value; }

		public bool IsAWBDeclaration => STH_IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;

		public bool IsREGDeclaration => STH_IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG;

		public bool IsSINDeclaration => STH_IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.SIN;

		[BusinessObjectTestExclude]
		[MaxLength(CusTempStorageLine.Schema.TSL_OwnerReferenceNumberMaxLength)]
		[ResourceStringData("A4774E08-4ACA-4E0D-AC8B-8475C84DD23D", Caption = "Reference")]
		public virtual ZString FormattedOwnerReferenceNumber
		{
			get
			{
				var result = STH_OwnerReferenceNumber;
				if (!result.IsEmpty && IsREGDeclaration)
				{
					result = SumARegistrationNumberFormatter.Format(result);
				}
				return result;
			}
			set
			{
				CheckMaximumLength(FormattedOwnerReferenceNumberInfo, value);
				value = value.KeepAlphanumericCharacters();
				if (STH_OwnerReferenceNumber != value)
				{
					STH_OwnerReferenceNumber = value;
				}
			}
		}

		public string ReferenceNumberColumnFieldType => STH_IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.Text);

		public ZPropertyInfo FormattedOwnerReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(FormattedOwnerReferenceNumber), x => STH_OwnerReferenceNumberInfo);

		[MaxLength(CusTempStorageLine.Schema.TSL_OwnerReferenceNumberMaxLength)]
		[ResourceStringData("D2D34B6C-19BF-4E61-B8B4-F30A63ED2496", Caption = "Reference")]
		public override ZString STH_OwnerReferenceNumber { get => base.STH_OwnerReferenceNumber; set => base.STH_OwnerReferenceNumber = value; }

		#endregion

		#region Lookups

		public new CusTempStorageDecLookups Lookups => (CusTempStorageDecLookups)base.Lookups;

		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new CusTempStorageDecLookups(this);

		#endregion

		#region Validation

		public new CusTempStorageDecValidation Validation => (CusTempStorageDecValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new CusTempStorageDecValidation(this);

		#endregion

		public override bool IsDataEmpty => false;

		public override bool CanDelete => base.CanDelete && STH_MessageStatus.IsEmpty;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("d67ea2a2-a88e-4916-bff0-90a6d91bb86f", "Declaration cannot be deleted as customs messaging has occurred");

		public CusTempStorageLine GetLine(ZString lineNumber)
		{
			CusTempStorageLine line = null;

			if (!lineNumber.IsEmpty
					&& ZInt.TryParse(lineNumber, out var sequenceNo))
			{
				line = CusTempStorageLines
					.Cast<CusTempStorageLine>()
					.FirstOrDefault(x => x.TSL_LineNo == sequenceNo);
			}

			return line;
		}

		protected static CusTempStorageDec Load(CusTempStorageJobHeader parent, ZString declarationType)
		{
			var query = new ZQuery(CusTempStorageDecSchema.STH_SJH, parent.PK);
			query.AddToFilter(CusTempStorageDecSchema.STH_DeclarationType, declarationType);
			query.OrderBy = CusTempStorageDecSchema.STH_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			return parent.Factory.LoadTop1<CusTempStorageDec>(query);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusTempStorageDecFetchStrategy(this);
	}
}
