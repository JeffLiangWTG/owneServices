using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationMessageSendingActionLookups : CusEntryHeaderMessageSendingActionLookups
	{
		public RefundApplicationMessageSendingActionLookups(RefundApplicationMessageSendingAction parent) : base(parent) { }

		public CodeDescriptionPairList RefundTypeList => Factory.GetCachedValue<AISRefundTypeList>();

		public CustomsOfficeCodeCollection CustomsOfficesList => Parent?.EntryHeader.Declaration?.Lookups?.CustomsOffices;

		public ICollection LegalBasisList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, ZDateTime.Today);

		public override CodeDescriptionPairList SendingActionTypeList => new CodeDescriptionPairList();

		protected new RefundApplicationMessageSendingAction Parent => (RefundApplicationMessageSendingAction)base.Parent;
	}
}
