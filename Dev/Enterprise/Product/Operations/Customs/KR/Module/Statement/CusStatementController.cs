using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	class CusStatementController : Customs.Module.StatementController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.KR.CustomsStatement;
		public override Type TypeOfTopLevelBusinessObject => typeof(CusStatementHeader);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.KRCustomsStatement;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.KRCustomsStatement;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new Statements((CusStatementHeader)businessEntity);
		}
	}
}
