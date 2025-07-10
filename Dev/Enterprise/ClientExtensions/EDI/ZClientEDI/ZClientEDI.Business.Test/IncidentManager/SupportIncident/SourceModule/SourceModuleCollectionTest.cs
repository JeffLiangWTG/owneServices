using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SourceModuleCollection))]
	sealed class SourceModuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SourceModuleCollection>
	{
		public void TestGetSourceModule()
		{
			var collection = new SourceModuleCollection();
			var aaaSourceModule = collection.AddNew("AAA", "AAA Description", "Root > AAA", ModuleListType.MenuSection, "ROO", true, true);
			var bbbSourceModule = collection.AddNew("BBB", "BBB Description", "Root > BBB", ModuleListType.MenuSection, "ROO", true, true);

			AssertEquals(aaaSourceModule, collection.GetSourceModule("AAA", "ENT"));
			AssertEquals(bbbSourceModule, collection.GetSourceModule("bbb", "ENT"));

			var bbbSourceModule2 = collection.AddNew("BBB", "BBB Description 2", "", ModuleListType.Cr8, "", true, true);
			AssertEquals(bbbSourceModule2, collection.GetSourceModule("Bbb", ModuleListType.Cr8, "ENT"));
		}

		public void TestContainsCodeAndPath()
		{
			var collection = new SourceModuleCollection();
			var aaaSourceModule = collection.AddNew("AAA", "AAA Description", "Root > AAA", ModuleListType.MenuSection, "ROO", true, true);
			var aaa2SourceModule = collection.AddNew("AAA", "AAA Description", "Root > AAA2", ModuleListType.MenuSection, "ROO", true, true);
			var bbbSourceModule = collection.AddNew("BBB", "BBB Description", "Root > BBB", ModuleListType.MenuSection, "ROO", true, true);

			AssertEquals(true, collection.ContainsCodeAndPath("AAA", "Root > AAA"));
			AssertEquals(true, collection.ContainsCodeAndPath("AAA", "Root > AAA2"));
			AssertEquals(false, collection.ContainsCodeAndPath("AAA", "Root > AAA3"));

			AssertEquals(true, collection.ContainsCodeAndPath("bbb", "root > bbb"));
		}

		public void TestModulesAddedToCollectionAsDetectedMenuItem()
		{
			var parentSection = new ModuleSection("",
				new MultilingualLanguageText("", "", "", "", null), "", null, IconTypes.ActionsButtonActive,
				IconTypes.ActionsButtonRest, null);
			parentSection.ParentCategory =
				new ModuleCategory("", new MultilingualLanguageText("", "", "", "", null), null);

			var mockModule = new Mock<IMainFormModule>();
			mockModule.Setup(x => x.ModuleTreeID).Returns("ABC");
			mockModule.Setup(x => x.Description).Returns(new MultilingualLanguageText("", "", "", "", null));
			mockModule.Setup(x => x.ParentSection).Returns(parentSection);

			var module = mockModule.Object;

			var collection = new SourceModuleCollection();
			collection.AddNew(module, true,
				true, "ENT");

			var sourceModule = collection.GetSourceModule(module.ModuleTreeID, "ENT");

			Assert(sourceModule.ModuleListType == ModuleListType.DetectedMenuItem);
		}

		#region Implementation

		protected override SourceModuleCollection GetCollectionToTest()
		{
			return new SourceModuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SourceModule();
		}

		#endregion
	}
}
