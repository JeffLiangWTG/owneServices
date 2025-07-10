using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Rohlig.Testing
{
	public class RohJobDeclarationDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetMenuTemplateFilterValue()
		{
			RohJobDeclaration declaration = Factory.New<RohJobDeclaration>();
			Assembly assembly = Assembly.Load("DocumentWrappers");
			Type docDeclarationType = assembly.GetType("Enterprise.DocumentWrappers.Customs.AU.DocDeclaration");
			MethodInfo newMethod = docDeclarationType.GetMethod("New", new Type[] { typeof(JobDeclaration), typeof(BusinessObjectFactory) });
			DocumentWrapper docWrapper = (DocumentWrapper)newMethod.Invoke(null, new object[] { declaration, Factory });
			RohJobDeclarationDocumentSupporter documentSupporter = (RohJobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Cartage Advice With Receipt";
			documentSupporter.CurrentCommand = menuItem;
			AssertEquals("Print Standard", ZBool.False.ToString(), documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, docWrapper));
		}
	}
}
