using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Module
{
	public class StatementController : Customs.Module.StatementController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StatementForm((CusStatementHeader)businessEntity);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.FR.CustomsStatement;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusStatementHeader);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.FRCustomsStatement;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.FRCustomsStatement;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.FRCustomsStatement;
	}
}
