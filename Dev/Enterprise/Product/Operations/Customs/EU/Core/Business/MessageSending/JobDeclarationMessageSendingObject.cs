using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public abstract class JobDeclarationMessageSendingObject : Customs.Business.JobDeclarationMessageSendingObject
	{
		protected JobDeclarationMessageSendingObject(Declaration.CusEntryHeader header)
			: base(header)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new sealed class Schema : Customs.Business.JobDeclarationMessageSendingObject.Schema
		{
			public const string EntryType = nameof(JobDeclarationMessageSendingObject.EntryType);
			public const string EntryStatusDescription = nameof(JobDeclarationMessageSendingObject.EntryStatusDescription);
			public const string BGMReference = nameof(JobDeclarationMessageSendingObject.BGMReference);
		}

		public new Declaration.CusEntryHeader Header => (Declaration.CusEntryHeader)base.Header;

		[CargoWiseOne.ResourceStrings.ResourceStringData("FC7FF114-0DEB-49C8-BC50-3543503B17F4", Caption = "Entry Type", ShortCaption = "Type")]
		public ZString EntryType => Header.EntryTypeFriendlyName;

		public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(Schema.EntryType);

		[CargoWiseOne.ResourceStrings.ResourceStringData("C481B560-C056-447F-9353-105AE0797920", Caption = "Entry Status Description", MediumCaption = "Entry Status Desc.", ShortCaption = "Status Desc.")]
		public ZString EntryStatusDescription => Header.EntryHeaderStatusDescription;

		public ZPropertyInfo EntryStatusDescriptionInfo => GetZPropertyInfo(Schema.EntryStatusDescription);

		[CargoWiseOne.ResourceStrings.ResourceStringData("32D04D09-668D-4D1D-9109-10313C6C6A09", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Ref. No.")]
		public ZString BGMReference => Header.CH_BGMReference;

		public ZPropertyInfo BGMReferenceInfo => GetZPropertyInfo(Schema.BGMReference);
	}
}



