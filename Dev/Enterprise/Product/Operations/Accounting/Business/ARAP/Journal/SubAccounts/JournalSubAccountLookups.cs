using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class JournalSubAccountLookups : AccTransactionHeaderSubAccountLookups
	{
		public JournalSubAccountLookups(JournalSubAccount parent) : base(parent)
		{
			Parent = parent;
		}

		protected new JournalSubAccount Parent;

		public IBusinessObjectCollection SubAccountList => SubAccountHelper.GetSubAccountList(Factory, Parent.AHS_SubClassParentTableCode);

		public CodeDescriptionPairList SubAccountTypeList => SubAccountHelper.GetSubAccountTypeList(Factory);
	}
}
