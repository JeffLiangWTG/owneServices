using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOJobDeclarationValidation : JobDeclarationValidation
	{
		public LPCOJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			if (Parent.FixedJobMessageType != BRJobMessageTypeList.Codes.LPCO)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("0eba1b53-2605-4ade-8115-05948de138f9", "This Shipment Type can only be used on the LPCO module (Operate > Customs > LPCO)."));
			}
			base.CheckJE_MessageTypeIsEnteredOrValid();
		}
	}
}
