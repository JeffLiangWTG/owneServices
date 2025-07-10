#if DEBUG

using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public partial class AREnquiryModule
	{
		public void HandleNew_ForTestOnly(string transactionType)
		{
			HandleNew(transactionType);
		}

		public void ShowStatementForm_ForTestOnly(ZBool statementPack)
		{
			ShowStatementForm(statementPack);
		}

		public ZBool IsCurrentOrgInactiveAndValid_ForTestOnly => IsCurrentOrgInactiveAndValid;

		public MultilingualString PrintStatementMenuText_ForTestOnly => PrintStatementMenuText;

		public MultilingualString PrintStatementPackMenuText_ForTestOnly => PrintStatementPackMenuText;

		public string OrganisationIsInvalidMessageText_ForTestOnly => OrganisationIsInvalidMessageText;
	}
}

#endif
