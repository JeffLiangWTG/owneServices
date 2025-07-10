using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public new sealed class Schema : AutoJobDeclarationMessageSendingObject.Schema
		{
			public const string DeclarationUnifiedNumber = nameof(CNJobDeclarationMessageSendingObject.DeclarationUnifiedNumber);
			public const string EntryNumber = nameof(CNJobDeclarationMessageSendingObject.EntryNumber);
			public const string MessageTypeDescription = nameof(CNJobDeclarationMessageSendingObject.MessageTypeDescription);
			public const string MessageStatusDescription = nameof(CNJobDeclarationMessageSendingObject.MessageStatusDescription);
			public const string DeclarationTypeDescription = nameof(CNJobDeclarationMessageSendingObject.DeclarationTypeDescription);
			public const string IntelligentDeclarationType = nameof(CNJobDeclarationMessageSendingObject.IntelligentDeclarationType);
		}

		public CNJobDeclarationMessageSendingObject(CusEntryHeader header) : base(header) { }

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		string fDeclarationType;
		public override ZString DeclarationType => fDeclarationType ??= Header.GetDeclarationType();

		[ResourceStringData("Enterprise.Customs.CN.Business.CNJobDeclarationMessageSendingObject|DeclarationTypeDescription", Caption = "Declaration Type")]
		public ZString DeclarationTypeDescription => Factory.GetCachedValue<DeclarationTypeList>().GetDescriptionFromCode(DeclarationType);

		public override ZString EntryStatus => Header.EntryHeaderStatusDescription;

		[ResourceStringData("Enterprise.Customs.CN.Business.CNJobDeclarationMessageSendingObject|IntelligentDeclarationType", Caption = "Intelligent Declaration Type", MediumCaption = "Intelligent Dec. Type", ShortCaption = "INT Dec. Type", FullDescription = "Intelligent Assisted Declaration Type")]
		public ZString IntelligentDeclarationType => Header.EntryInstruction is CusEntryInstruction instruction ? instruction.Lookups.IntelligentDeclarationTypeList.GetDescriptionFromCode(instruction.CEI_SubStyle) : string.Empty;

		[ResourceStringData("Enterprise.Customs.CN.Business.CNJobDeclarationMessageSendingObject|DeclarationUnifiedNumber", Caption = "Declaration Unified Number")]
		public ZString DeclarationUnifiedNumber => Header.DeclarationUnifiedNumber;

		public ZPropertyInfo DeclarationUnifiedNumberInfo => GetZPropertyInfo(Schema.DeclarationUnifiedNumber);

		[ResourceStringData("Enterprise.Customs.CN.Business.CNJobDeclarationMessageSendingObject|EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber => Header.EntryNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(Schema.EntryNumber);

		[ResourceStringData("Enterprise.Customs.CN.Business.CNJobDeclarationMessageSendingObject|MessageTypeDescription", Caption = "Entry Type")]
		public ZString MessageTypeDescription => Header.CH_MessageTypeDescription;

		public ZPropertyInfo MessageTypeDescriptionInfo => GetZPropertyInfo(Schema.MessageTypeDescription);

		[ResourceStringData("Enterprise.Customs.CN.Business.CNJobDeclarationMessageSendingObject|MessageStatusDescription", Caption = "Message Status")]
		public ZString MessageStatusDescription => Header.MessageStatusDescription;

		public ZPropertyInfo MessageStatusDescriptionInfo => GetZPropertyInfo(Schema.MessageStatusDescription);

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new CNJobDeclarationMessageSendingObjectValidation(this);

		public ZString ToMessageString() => ZString.Empty;
	}
}
