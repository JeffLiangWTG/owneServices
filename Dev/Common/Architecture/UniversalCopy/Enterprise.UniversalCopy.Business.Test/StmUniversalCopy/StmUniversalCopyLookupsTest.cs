using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.UniversalCopy.Business.Testing
{
	internal class StmUniversalCopyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCopyTemplates()
		{
			var template1 = CreateTemplate("XXX_UC", "Template 1", GlbCompany.CurrentCompany.PK);
			var template2 = CreateTemplate("YYY_UC", "Template 2", GlbCompany.CurrentCompany.PK);
			var template3 = CreateTemplate("XXX_UC", "Template 3", GlbCompany.CurrentCompany.PK);
			var template4 = CreateTemplate("XXX_UC", "Template 4", ZGuid.NewZGuid());
			var template5 = CreateTemplate("XXX_UC", "Template 5", ZGuid.NewZGuid());

			var uc = Factory.New<StmUniversalCopy>();
			uc.GridContext = "XXX_UC";
			uc.SUC_S9_CopyTemplate = template5.PK;

			AssertContainsExactElementsInAnyOrder(new ZString[] { "Template 1", "Template 3", "Template 5" }, uc.Lookups.CopyTemplates.Select(item => item.S9_FilterName));
		}

		public void TestLoadedCopyTemplateDescriptionDoesNotContainHotKeys()
		{
			var template = CreateTemplate("XXX_UC", "T&emplate", GlbCompany.CurrentCompany.PK);
			var uc = Factory.New<StmUniversalCopy>();
			uc.GridContext = "XXX_UC";
			AssertEquals("Template", ((ICodeDescription)uc.Lookups.CopyTemplates[0]).Code);
			AssertEquals("Template", ((ICodeDescription)uc.Lookups.CopyTemplates[0]).Description);
		}

		UniversalCopyTemplate CreateTemplate(string module, string name, ZGuid companyPK)
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true), typeof(DummyBusinessObject), BusinessObjectCopyManager.CopyTreeConfiguration), template);
			template.S9_ModuleID = module;
			template.S9_FilterName = name;
			template.S9_GC = companyPK;
			return template;
		}
	}
}
