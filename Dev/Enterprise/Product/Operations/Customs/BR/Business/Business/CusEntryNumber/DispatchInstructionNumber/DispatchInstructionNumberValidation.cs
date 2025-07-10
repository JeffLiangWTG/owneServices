using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public class DispatchInstructionNumberValidation : CusEntryNumValidation
	{
		public DispatchInstructionNumberValidation(DispatchInstructionNumber entryNumber)
			: base(entryNumber)
		{
		}

		public new DispatchInstructionNumber Parent => (DispatchInstructionNumber)base.Parent;

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CE_EntryTypeInfo);
		}

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CE_EntryNumInfo);
		}
	}
}
