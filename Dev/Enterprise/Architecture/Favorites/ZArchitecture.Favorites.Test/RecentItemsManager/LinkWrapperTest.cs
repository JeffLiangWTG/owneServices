using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Favorites.Testing
{
	sealed class LinkWrapperTest : TestCaseWithFactory
	{
		public void TestRecordShortcut()
		{
			var recordPK = Guid.NewGuid();
			var wrapper = new LinkWrapper("BlahModule", recordPK, "http://blah", "description");

			AssertEquals("BlahModule", wrapper.ModuleName);
			AssertEquals(recordPK, wrapper.RecordKey);
			AssertEquals("http://blah", wrapper.RecordUrl);
			AssertEquals("description", wrapper.RecordDescription);
			Assert("Not a module", !wrapper.IsModule);
			AssertEquals("BlahModule" + recordPK, wrapper.UniqueKey);
		}

		public void TestRecordShortcutFromStmLink()
		{
			var recordPK = Guid.NewGuid();
			var shortcut = Factory.New<StmLink>();
			shortcut.STL_ModuleID = "BlahModule";
			shortcut.STL_ItemPK = recordPK;
			shortcut.STL_ItemUrl = "http://url";
			shortcut.STL_ItemDescription = "description";

			var wrapper = new LinkWrapper(shortcut);

			AssertEquals("BlahModule", wrapper.ModuleName);
			AssertEquals(recordPK, wrapper.RecordKey);
			AssertEquals("http://url", wrapper.RecordUrl);
			AssertEquals("description", wrapper.RecordDescription);
			Assert("Not a module", !wrapper.IsModule);
			AssertEquals("BlahModule" + recordPK, wrapper.UniqueKey);
		}

		public void TestModuleShortcut()
		{
			var wrapper = new LinkWrapper("BlahModule", Guid.Empty, string.Empty, string.Empty);

			AssertEquals("BlahModule", wrapper.ModuleName);
			AssertEquals(Guid.Empty, wrapper.RecordKey);
			AssertEquals(string.Empty, wrapper.RecordDescription);
			Assert("Is module shortcut", wrapper.IsModule);
			AssertEquals("BlahModule", wrapper.UniqueKey);
		}

		public void TestEqualShortcuts()
		{
			var shortcut = Factory.New<StmLink>();
			shortcut.STL_ModuleID = "BlahModule";
			shortcut.STL_ItemPK = new ZGuid("d32dc19c-d2b1-4bc5-980e-8e520e350ea3");
			shortcut.STL_ItemUrl = "http://url";
			shortcut.STL_ItemDescription = "description123";

			var wrapper = new LinkWrapper(shortcut);
			Assert("Shortcuts are treated as equal", LinkWrapper.AreShortcutsEqual(wrapper, shortcut));

			var shortcutWithADifferentDescription = Factory.New<StmLink>();
			shortcutWithADifferentDescription.STL_ModuleID = "BlahModule";
			shortcutWithADifferentDescription.STL_ItemPK = new ZGuid("d32dc19c-d2b1-4bc5-980e-8e520e350ea3");
			shortcutWithADifferentDescription.STL_ItemUrl = "http://url";
			shortcutWithADifferentDescription.STL_ItemDescription = "description789";
			Assert("Shortcuts are treated as equal", LinkWrapper.AreShortcutsEqual(wrapper, shortcutWithADifferentDescription));
		}

		[ExpectNoExceptions]
		public void TestShortcutWithDelimiterInDescription()
		{
			var recordPK = Guid.NewGuid();
			var wrapper = new LinkWrapper("BlahModule", recordPK, "http://blah", "description with $ sign");

			AssertEquals("BlahModule", wrapper.ModuleName);
			AssertEquals(recordPK, wrapper.RecordKey);
			AssertEquals("http://blah", wrapper.RecordUrl);
			AssertEquals("description with $ sign", wrapper.RecordDescription);
		}
	}
}
