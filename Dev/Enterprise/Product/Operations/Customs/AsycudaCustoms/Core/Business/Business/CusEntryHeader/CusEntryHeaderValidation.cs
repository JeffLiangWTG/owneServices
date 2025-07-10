using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		protected override void CheckCH_BGMReference()
		{
			base.CheckCH_BGMReference();
			if (Parent.CH_BGMReference.IsEmpty)
			{
				Parent.CH_BGMReferenceInfo.AddMessageError(Res.GetString("FB556628-C753-4400-A2AB-4C8D54BCDDEF", "The Reference Number of is required for messaging. Please go to menu 'Brokerage - Allocate Entry Reference Number' to allocate references."));
			}
		}

		protected override void CheckEntryNumber()
		{
			base.CheckEntryNumber();
			if (Parent.EntryNumber.IsEmpty && (Parent.Declaration?.IsBondedWarehouseAutomationOn ?? false))
			{
				Parent.EntryNumberInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.EntryNumberInfo.HumanReadableName));
			}
		}
	}
}
