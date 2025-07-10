using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class JobDeclarationMessageSendingObject : Customs.Business.JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateAll();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new sealed class Schema : Customs.Business.JobDeclarationMessageSendingObject.Schema
		{
			public const string MessageTypeDescription = "MessageTypeDescription";
			public const string EntryType = "EntryType";
			public const string EntryStatusDescription = "EntryStatusDescription";
			public const string Procedure = "Procedure";
			public const string EntryInstructionDescription = "EntryInstructionDescription";
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;
		CusEntryInstruction EntryInstruction => Header.EntryInstruction;

		[CargoWiseOne.ResourceStrings.ResourceStringData("F6BA4D01-1CCA-440A-8C74-ECFE6FEFFFAC", ShortCaption = "Msg. Type", Caption = "Message Type")]
		public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

		[CargoWiseOne.ResourceStrings.ResourceStringData("129FE53D-6AC1-488B-92A6-28020634F04F", ShortCaption = "Msg. Type Desc.", Caption = "Message Type Description")]
		public ZString MessageTypeDescription => Factory.GetCachedValue<ILEDIMessageSubTypeList>().GetDescriptionFromCode(MessageType);

		public ZPropertyInfo MessageTypeDescriptionInfo => GetZPropertyInfo(Schema.MessageTypeDescription);

		[CargoWiseOne.ResourceStrings.ResourceStringData("A1C24FB0-1D82-4213-A4BA-55B913024DCF", Caption = "Entry Type")]
		public override ZString DeclarationType => base.DeclarationType;

		[CargoWiseOne.ResourceStrings.ResourceStringData("6DABB973-502D-4ADB-AFB6-1093766CB353", Caption = "Entry Status")]
		public override ZString EntryStatus => base.EntryStatus;

		#region EntryInstruction
		[CargoWiseOne.ResourceStrings.ResourceStringData("6FD73F3A-C496-42FC-A5DB-02301FE674DA", Caption = "Procedure")]
		public ZString Procedure
		{
			get
			{
				var result = ZString.Empty;
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					result = entryInstruction.CEI_FormattedProcedure;
				}
				return result;
			}
		}

		public ZPropertyInfo ProcedureInfo => GetZPropertyInfo(Schema.Procedure);

		[CargoWiseOne.ResourceStrings.ResourceStringData("F3868E26-E0EB-4C39-AC59-7091D76BBEE9", ShortCaption = "Entry Type Desc.", Caption = "Entry Type Description")]
		public ZString EntryInstructionDescription => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZPropertyInfo EntryInstructionDescriptionInfo => GetZPropertyInfo(Schema.EntryInstructionDescription);
		#endregion

		protected override ZString GetDefaultMessageType()
		{
			return Header.Declaration.IsImport
				? ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest
				: ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest;
		}
	}
}
