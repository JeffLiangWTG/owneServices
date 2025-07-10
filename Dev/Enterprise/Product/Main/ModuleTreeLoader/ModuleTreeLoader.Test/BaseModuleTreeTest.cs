using System.Collections;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Main.ModuleTreeLoader;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing;

public abstract class BaseModuleTreeTest : TransactionedTestCase
{
	protected int AssertModuleAdded(string message, bool expectModuleToExist, ModuleTreeLoaderConstant.Entry category, ModuleTreeLoaderConstant.Entry section, string moduleID)
	{
		var moduleIndexWithinSection = -1;
		var category1 = Tree.Categories[category.Name];
		var section1 = category1.Sections[section.Name];

		if (section1 != null)
		{
			var sectionHashtable = new Hashtable(section1.Modules);
			var module = (IMainFormModule)sectionHashtable[moduleID];
			if (module != null)
			{
				moduleIndexWithinSection = new ArrayList((ICollection)section1.Modules.Values).IndexOf(module);
			}
		}

		var found = moduleIndexWithinSection != -1;
		AssertEquals(message, expectModuleToExist, found);
		return moduleIndexWithinSection;
	}

	protected override void SetUp()
	{
		base.SetUp();

		Tree = new ModuleTree();
		Loader = new ModuleTreeLoader();
		Loader.Initialise(Tree, Env.Security);
	}

	protected ModuleTreeLoader Loader;
	protected ModuleTree Tree;
}
