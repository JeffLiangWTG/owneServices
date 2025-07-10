using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using UCManager = Enterprise.UniversalCopy.GUI.Testing.UniversalCopyManagerTest.UniversalCopyManagerForTest;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class UniversalCopyMultiControlColumnStyleTest : TestCaseWithDummy
	{
		public void TestLoadModuleId()
		{
			var copyTemplateTree = new CopyTemplateTree(
				GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(OrgHeader), true),
				typeof(OrgHeader),
				BusinessObjectCopyManager.CopyTreeConfiguration);

			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.Organisation.Name + "_UC";
			template.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			template.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(copyTemplateTree, template);
			template.S9_FilterName = "Test";
			template.S9_IsPublished = true;
			using (var manager = new UCManager(typeof(OrgHeader), ModuleIDs.Organisation))
			{
				using (var form = new UniversalCopyTemplateForm(template, manager))
				{
					form.Show();

					var bizo = form.Template.CopyTemplateTree.PropertyNodes
								.Cast<PropertyCopyTemplateBizo>().FirstOrDefault(q => q.Name == "OH_RSL_ShippingLine");

					var control = form.Controls.Find("ucTemplateUserControl", true)[0] as UniversalCopyTemplateUserControl;

					using (var column = new UniversalCopyMultiControlColumnStyleForTest(control, new UniversalCopyMultiCombinationControl(), new UniversalCopyMultiControlColumnStyleInfo()))
					using (var findBox = new ZGridGuidFindBox())
					{
						AssertEquals("Pre-condition", ModuleIDs.NotAssigned, findBox.ModuleID);
						column.LoadModuleId(bizo, findBox);
						AssertEquals(ModuleIDs.RefShippingLine, findBox.ModuleID);
					}
				}
			}
		}

		class UniversalCopyMultiControlColumnStyleForTest : UniversalCopyMultiControlColumnStyle
		{
			public UniversalCopyMultiControlColumnStyleForTest(UniversalCopyMultiControlColumnStyleInfo columnInfo)
			   : base(() => new UniversalCopyMultiCombinationControl(), columnInfo)
			{
			}

			public UniversalCopyMultiControlColumnStyleForTest(UniversalCopyTemplateUserControl templateUserControl, UniversalCopyMultiCombinationControl combinationControl, UniversalCopyMultiControlColumnStyleInfo columnInfo)
				: base(() => combinationControl, columnInfo)
			{
				EditControl.GetType();
				combinationControl.GetPropertyListModuleIdMethod = templateUserControl.GetPropertyListModuleId;
			}
		}
	}
}
