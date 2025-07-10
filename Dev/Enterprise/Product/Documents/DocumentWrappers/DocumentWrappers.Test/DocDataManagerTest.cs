using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocDataManagerTest : TestCaseWithFactory
	{
		public void TestGetValue()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_GoodsDescription = "YEEEEEHA!!!";
			var docNote = DocumentNote.LoadNote(declaration);
			docNote.SetSystemDefinedFieldValue("GoodsDescription", "");
			Factory.Save();
			var docDataManager = (IDocDataManager)Activator.CreateInstance(ObjectFactory.GetType<IDocDataManager>(), new object[] { declaration });
			AssertEquals("", docDataManager.GetValue("GoodsDescription"));
		}
	}
}
