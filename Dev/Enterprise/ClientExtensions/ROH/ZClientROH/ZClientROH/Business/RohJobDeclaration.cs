using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.Rohlig
{
	public class RohJobDeclaration : JobDeclaration
	{
		public RohJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static new RohJobDeclaration New(BusinessObjectFactory factory)
		{
			return (RohJobDeclaration)factory.New(typeof(RohJobDeclaration));
		}

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new RohJobDeclarationDocumentSupporter(this);
		}
	}

	public class RohJobDeclarationDocumentSupporter : JobDeclarationDocumentSupporter
	{
		public RohJobDeclarationDocumentSupporter(RohJobDeclaration declaration) : base(declaration)
		{
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapper)
		{
			string result;

			if (CurrentCommand != null && CurrentCommand.SU_MenuName.EndsWith("Cartage Advice With Receipt") && filterType == MenuTemplateFilterType.PrintStandard)
			{
				result = ZBool.False.ToString();
			}
			else
			{
				result = base.GetMenuTemplateFilterValue(filterType, docWrapper);
			}

			return result;
		}

		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			CurrentCommand = commandAboutToBeRun; // need this to determine the selected menu.
			return base.GetDataStateBeforeRunCore(commandAboutToBeRun);
		}

		public
		IStmMenuItem CurrentCommand;
	}
}
