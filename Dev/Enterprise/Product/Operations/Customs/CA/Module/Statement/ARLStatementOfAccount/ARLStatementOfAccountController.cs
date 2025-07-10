using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class ARLStatementOfAccountController : StatementController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.CAARLStatementOfAccount;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.CAARLStatementOfAccountView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is CusStatementHeader statementHeader && statementHeader.IsCARMSOA)
			{
				return new CARMSOAStatementForm(statementHeader);
			}
			else
			{
				return base.GetForm(businessEntity);
			}
		}
	}
}
