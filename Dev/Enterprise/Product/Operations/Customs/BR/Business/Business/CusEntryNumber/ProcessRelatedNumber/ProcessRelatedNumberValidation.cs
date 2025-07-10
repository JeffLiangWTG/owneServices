using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public class ProcessRelatedNumberValidation : CusEntryNumValidation
	{
		public ProcessRelatedNumberValidation(ProcessRelatedNumber entryNumber)
			: base(entryNumber)
		{
		}

		public new ProcessRelatedNumber Parent => (ProcessRelatedNumber)base.Parent;

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CE_EntryTypeInfo);
		}
	}
}
