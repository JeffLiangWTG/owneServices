using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class DeclarationMessageSendingObject : JobDeclarationMessageSendingObject, IMessageSendingObject
{
	public DeclarationMessageSendingObject(CusEntryHeader header) : base(header)
	{
	}

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new class Schema : AutoJobDeclarationMessageSendingObject.Schema
	{
		public const string LocalReferenceNumberDate = nameof(DeclarationMessageSendingObject.LocalReferenceNumberDate);
		public const string CustomsHouse = nameof(DeclarationMessageSendingObject.CustomsHouse);
		public const string Description = nameof(DeclarationMessageSendingObject.Description);
		public const string ShippingBillNumber = nameof(DeclarationMessageSendingObject.ShippingBillNumber);
		public const string ShippingBillDate = nameof(DeclarationMessageSendingObject.ShippingBillDate);
	}

	public IMessageAttachee MessageAttachee => Header;

	[List(nameof(Lookups) + "." + nameof(DeclarationMessageSendingObjectLookups.MessageTypes))]
	[ResourceStringData("INCustoms|DeclarationMessageSendingObject|MessageType", Caption = "Message Type")]
	public override ZString MessageType => base.MessageType;

	protected override bool MessageType_ReadOnly => false;

	[ResourceStringData("INCustoms|DeclarationMessageSendingObject|LocalReferenceNumberDate", Caption = "LRN Date",
		MediumCaption = "Date", ShortCaption = "Date")]
	public ZDateTime LocalReferenceNumberDate => Header.CreateTime;

	public ZPropertyInfo LocalReferenceNumberDateInfo => GetZPropertyInfo(Schema.LocalReferenceNumberDate);

	[ResourceStringData("INCustoms|DeclarationMessageSendingObject|CustomsHouse", Caption = "Customs House", ShortCaption = "Cus. House", MediumCaption = "Cus. House")]
	public ZString CustomsHouse => Header.Declaration?.JE_CustomsOffice ?? ZString.Empty;

	public ZPropertyInfo CustomsHouseInfo => GetZPropertyInfo(Schema.CustomsHouse);

	[ResourceStringData("Enterprise.Customs.IN.Business.DeclarationMessageSendingObject|DeclarationType", Caption = "Declaration Type", MediumCaption = "Dec. Type", ShortCaption = "Dec. Ty.")]
	public override ZString DeclarationType => base.DeclarationType;

	[ResourceStringData("Enterprise.Customs.IN.Business.DeclarationMessageSendingObject|ShippingBillNumber", Caption = "SB No.", MediumCaption = "SB No.", ShortCaption = "SB No.")]
	public ZString ShippingBillNumber => Header?.EntryInstruction?.ShippingBillNumber ?? ZString.Empty;

	public ZPropertyInfo ShippingBillNumberInfo => GetZPropertyInfo(Schema.ShippingBillNumber);

	[ResourceStringData("Enterprise.Customs.IN.Business.DeclarationMessageSendingObject|ShippingBillDate", Caption = "SB No.Date.", MediumCaption = "SB Dt.", ShortCaption = "SB Dt.")]
	public ZDate ShippingBillDate => Header?.EntryInstruction?.ShippingBillDate.Date ?? ZDate.Empty;

	public ZPropertyInfo ShippingBillDateInfo => GetZPropertyInfo(Schema.ShippingBillDate);

	[ResourceStringData("Enterprise.Customs.IN.Business.DeclarationMessageSendingObject|Description", Caption = "Description", MediumCaption = "Desc.", ShortCaption = "Desc.")]
	public ZString Description => Header?.EntryInstruction?.CEI_Description ?? ZString.Empty;

	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

	public DeclarationMessageSendingObjectLookups Lookups => lookups ??= GetNewLookups();
	DeclarationMessageSendingObjectLookups lookups;

	public new DeclarationMessageSendingObjectValidation Validation => (DeclarationMessageSendingObjectValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new DeclarationMessageSendingObjectValidation(this);

	DeclarationMessageSendingObjectLookups GetNewLookups() => new(this);
}
