using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class MessageChooserItem : ASYCUDA.Business.MessageChooserItem
{
	public MessageChooserItem(MessageChooser chooser, ISelectionItem item, bool showStatus) : base(chooser, item, showStatus)
	{
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			EntryType = EntryTypes.Codes.Original;
		}
	}

	public new class Schema : AutoMessageChooserItem.Schema
	{
		public const int EntryTypeMaxLength = 1;
		public const int SubjectCodeMaxLength = 3;
		public const int SubjectMaxLength = 512;
	}

	public new AsycudaBill Bill => (AsycudaBill)base.Bill;

	[MaxLength(Schema.EntryTypeMaxLength)]
	[ResourceStringData("88c02886-b740-411a-8469-2112694e5b91", Caption = "Entry Type")]
	[List(nameof(Lookups) + "." + nameof(MessageChooserItemLookups.EntryTypeList))]
	public ZString EntryType
	{
		get => entryType;
		set
		{
			SetNonPersistentPropertyValue(EntryTypeInfo, ref entryType, value);

			if (entryType == EntryTypes.Codes.Cancellation)
			{
				SubjectCode = SubjectCodes.Codes.ProvideJustificationForCanceling;
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateEntryType();
				Validation.ValidateSubjectCode();
				Validation.ValidateSubject();
			}
		}
	}
	ZString entryType;

	public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(nameof(EntryType));

	[ResourceStringData("3e699801-4e2c-44a0-89d5-b0d8e910dd47", Caption = "BOL Number")]
	public ZString BOLNumber => Bill.ABL_BillNumber;

	public ZPropertyInfo BOLNumberInfo => GetZPropertyInfo(nameof(BOLNumber));

	[ResourceStringData("1cb11214-4d3f-497a-9870-aec62c06347a", Caption = "Customs Status")]
	public ZString CustomsStatus => Bill.ABL_BillStatus;

	public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(nameof(CustomsStatus));

	[MaxLength(Schema.SubjectCodeMaxLength)]
	[ResourceStringData("000fd79a-48ec-42da-8c79-15356b08e595", Caption = "Subject Code")]
	[List(nameof(Lookups) + "." + nameof(MessageChooserItemLookups.SubjectCodeList))]
	public ZString SubjectCode
	{
		get => subjectCode;
		set
		{
			SetNonPersistentPropertyValue(SubjectCodeInfo, ref subjectCode, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateSubjectCode();
			}
		}
	}
	ZString subjectCode;

	public ZPropertyInfo SubjectCodeInfo => GetZPropertyInfo(nameof(SubjectCode));

	[MaxLength(Schema.SubjectMaxLength)]
	[ResourceStringData("d9356f26-3e25-4f96-abee-2621b098822d", Caption = "Subject")]
	public ZString Subject
	{
		get => subject;
		set
		{
			SetNonPersistentPropertyValue(SubjectInfo, ref subject, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateSubject();
			}
		}
	}
	ZString subject;

	public ZPropertyInfo SubjectInfo => GetZPropertyInfo(nameof(Subject));

	[ResourceStringData("a12acd0e-f640-4621-b5d0-d4856ce3a912", Caption = "Cargo Type")]
	public ZString CargoType => Bill.ABL_CargoType;

	public ZPropertyInfo CargoTypeInfo => GetZPropertyInfo(nameof(CargoType));

	public MessageChooserItemLookups Lookups => lookups ??= new MessageChooserItemLookups(this);
	MessageChooserItemLookups lookups;

	public new MessageChooserItemValidation Validation => (MessageChooserItemValidation)base.Validation;

	protected override ASYCUDA.Business.MessageChooserItemValidation GetNewValidation() => new MessageChooserItemValidation(this);
}
