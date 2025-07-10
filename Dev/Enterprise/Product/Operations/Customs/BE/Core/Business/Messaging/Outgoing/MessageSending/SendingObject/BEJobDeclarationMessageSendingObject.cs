using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.Business;

public abstract class BEJobDeclarationMessageSendingObject : EU.Business.JobDeclarationMessageSendingObject
{
	public BEJobDeclarationMessageSendingObject(Declaration.CusEntryHeader header) : base(header)
	{
	}

	public static class BESchema
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Schema")]
		public const string Description = "Description";
		public const string IsTestDeclaration = "IsTestDeclaration";
	}

	public new Declaration.CusEntryHeader Header => (Declaration.CusEntryHeader)base.Header;

	[ResourceStringData("NPBO:Enterprise.Customs.BE.Business.MessageSending.MessageSendingObject|TypeOfEntry", ShortCaption = "Ent. Type", Caption = "Entry Type")]
	[List("Lookups" + "." + "EntryTypeList")]
	[MaxLength(3)]
	public virtual ZString TypeOfEntry
	{
		get => typeOfEntry;
		set
		{
			CheckMaximumLength(TypeOfEntryInfo, value);
			SetNonPersistentPropertyValue(TypeOfEntryInfo, ref typeOfEntry, value);
		}
	}
	ZString typeOfEntry;

	public ZPropertyInfo TypeOfEntryInfo => GetZPropertyInfo(nameof(TypeOfEntry));

	[ResourceStringData("Enterprise.Customs.BE.Business.JobDeclarationMessageSendingObject|Description", Caption = "Description")]
	public ZString Description => Header.EntryInstruction?.CEI_Description ?? ZString.Empty;

	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(BESchema.Description);

	public ZString ProcedureType => Header.EntryInstruction?.CEI_Style ?? ZString.Empty;

	public virtual ZPropertyInfo ProcedureTypeInfo => GetZPropertyInfo(nameof(ProcedureType));

	public virtual ZString Variant => Header.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

	public virtual ZPropertyInfo VariantInfo => GetZPropertyInfo(nameof(Variant));

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		IsTestDeclaration = Header.Declaration.ZG_IsTrainingDeclaration;
	}

	[ResourceStringData("Enterprise.Customs.BE.Business.JobDeclarationMessageSendingObject|IsTestDeclaration", Caption = "Test?")]
	public ZBool IsTestDeclaration
	{
		get => isTestDeclaration;
		set => SetNonPersistentPropertyValue(IsTestDeclarationInfo, ref isTestDeclaration, value);
	}
	ZBool isTestDeclaration;

	public ZPropertyInfo IsTestDeclarationInfo => GetZPropertyInfo(BESchema.IsTestDeclaration);
}
