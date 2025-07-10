using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeAccessCodesSendingObject : AutoGuaranteeAccessCodesSendingObject
	{
		public GuaranteeAccessCodesSendingObject(CusGuaranteeHeader cusGuaranteeHeader) : base(cusGuaranteeHeader?.Factory)
		{
			CusGuaranteeHeader = Argument.NotNull(cusGuaranteeHeader, nameof(cusGuaranteeHeader));
		}
		public CusGuaranteeHeader CusGuaranteeHeader { get; }

		public override ZString GuaranteeReferenceNumber => CusGuaranteeHeader.CPH_Number;

		[List(nameof(Lookups) + "." + nameof(GuaranteeAccessCodesSendingObjectLookups.OfficeCodeList))]
		public override ZString OfficeOfGuarantee
		{
			get => base.OfficeOfGuarantee;
			set => base.OfficeOfGuarantee = value;
		}

		public GuaranteeAccessCodesSendingObjectLookups Lookups => lookups ?? (lookups = GetNewLookups());
		GuaranteeAccessCodesSendingObjectLookups lookups;

		protected virtual GuaranteeAccessCodesSendingObjectLookups GetNewLookups() => new GuaranteeAccessCodesSendingObjectLookups(this);

		protected override GuaranteeAccessCodesSendingObjectValidation GetNewValidation() => new GuaranteeAccessCodesSendingObjectValidation(this);
	}
}
