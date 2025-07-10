using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class AgreedRateMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public AgreedRateMessageSendingObject(CusEntryHeader entry)
			: base(entry, ElectronicDocumentTypeList.Codes._5BA)
		{
		}

		/// <summary>
		/// Please do not use this option as it is used only for the binding purpose.
		/// </summary>
		/// <param name="factory"></param>
		public AgreedRateMessageSendingObject(BusinessObjectFactory factory) : base(null, ElectronicDocumentTypeList.Codes._5BA)
		{
		}

		public new AgreedRateMessageSendingObjectValidation Validation => (AgreedRateMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new AgreedRateMessageSendingObjectValidation(this);

		[ResourceStringData("93444B28-FBDA-42F3-96B3-99AD4A2180F2", Caption = "Declaration Date")]
		public ZDateTime DeclarationDate => Header.CusEntryNumber.CE_IssueDate;

		[ResourceStringData("F683C4E2-A1B3-4BE5-BA3C-961B995A0919", Caption = "Duty Rate")]
		public ZDecimal DutyRate => Header.EntryInstruction?.CEI_AgreedDutyRate ?? ZDecimal.Zero;

		[ResourceStringData("23FA5140-74B9-448A-8BEB-6260AFB80196", Caption = "Preference Code Desc.")]
		public ZString PreferenceCodeDescription
		{
			get
			{
				var result = ZString.Empty;
				var dutyRateCode = Header.EntryInstruction?.CEI_AgreedDutyRatePreferenceCode ?? ZString.Empty;
				if (!dutyRateCode.IsEmpty)
				{
					result = KRPreferenceList.GetDescriptionFromCode(dutyRateCode);
				}
				return result;
			}
		}

		CodeDescriptionPairList KRPreferenceList => CusRefPreferenceView.Loader.GetList(Factory, Core.Constants.CountryCodes.KoreaSouth);

		public MessageSendingEntryLineObjectCollection Details
		{
			get
			{
				if (details == null)
				{
					details = new MessageSendingEntryLineObjectCollection(Header);
					details.PopulateElementsFromMergedLines(x => true, ElectronicDocumentTypeList.Codes._5BA);
				}
				return details;
			}
		}
		MessageSendingEntryLineObjectCollection details;
	}
}
