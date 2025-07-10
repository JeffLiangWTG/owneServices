using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module
{
	public abstract class StatementController : Customs.Module.StatementController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			IZForm form = null;
			if (businessEntity is CusStatementHeader statementHeader)
			{
				form = new StatementForm(statementHeader);
			}

			return form;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusStatementHeader);
	}
}
