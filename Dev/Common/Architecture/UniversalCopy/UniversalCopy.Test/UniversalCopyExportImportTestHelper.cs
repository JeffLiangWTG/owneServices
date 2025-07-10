using CargoWise.EntityFramework;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.UniversalCopy.Test
{
	public static class UniversalCopyExportImportTestHelper
	{
		public static UniversalCopyTemplate CrateUCTemplate(BusinessObjectFactory factory, string name, bool isActive)
		{
			var template = factory.New<UniversalCopyTemplate>();

			template.S9_ModuleID = DummyModuleIDs.Dummy2 + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = name }, template);
			template.S9_FilterName = name;
			template.IsActive = isActive;

			var entityCopyNode = new EntityCopyTemplateNode();
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Number, CopyMethod = CopyMethod.Copy });

			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;

			template.PrepareForSave();
			return template;
		}
	}
}
