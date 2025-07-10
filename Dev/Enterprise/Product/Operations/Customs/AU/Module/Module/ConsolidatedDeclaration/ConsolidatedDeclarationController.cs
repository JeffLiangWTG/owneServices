using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public class ConsolidatedDeclarationController : Customs.Module.ConsolidatedDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(ConsolidatedDeclaration);

		protected override IZForm CreateEditForm(IBusiness businessEntity)
		{
			return new ConsolidatedDeclarationForm(businessEntity as ConsolidatedDeclaration, new ConsolidatedDeclarationFormAdaptationsProvider());
		}
	}
}
