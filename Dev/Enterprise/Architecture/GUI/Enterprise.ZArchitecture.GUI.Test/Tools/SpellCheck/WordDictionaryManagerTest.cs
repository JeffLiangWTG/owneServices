using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	public class WordDictionaryManagerTest : TestCaseWithFactory
	{
		public void TestCurrentDictionary_CreateIfNeeded()
		{
			var userWithoutDictionary = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			userWithoutDictionary.GS_Code = "DNC";
			Factory.Save();
			using (LoginAsUser(userWithoutDictionary))
			{
				Assert("Since no dictionary exists, a new one should be created", !WordDictionaryManager.Instance.CurrentDictionary.IsInDatabase);
			}
		}

		public void TestCurrentDictionary_LoadIfAvailable()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = "DNC";
			var dictionary = Factory.NewWithValidTestData<WordDictionary>();
			dictionary.DIC_GS_NKStaff = user.GS_Code;
			dictionary.AddWord("MySpecialWord");
			Factory.Save();
			using (LoginAsUser(user))
			{
				Assert("Should use the existing dictionary when available", WordDictionaryManager.Instance.CurrentDictionary.Words.Contains("MySpecialWord"));
			}
		}

		public void TestCurrentDictionaryIsLoginDependant()
		{
			var u1 = CreateUserWithDictionary(Factory, "U1");
			var u2 = CreateUserWithDictionary(Factory, "U2");
			Factory.Save();
			using (LoginAsUser(u1))
			{
				AssertEquals("Should be using user1's dictionary", u1.GS_Code, WordDictionaryManager.Instance.CurrentDictionary.DIC_GS_NKStaff);
			}

			using (LoginAsUser(u2))
			{
				AssertEquals("Should be using user2's dictionary", u2.GS_Code, WordDictionaryManager.Instance.CurrentDictionary.DIC_GS_NKStaff);
			}
		}

		public void TestAddWord_SavesDictionary()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = "XD";
			Factory.Save();
			using (LoginAsUser(user))
			{
				WordDictionaryManager.Instance.AddWord("MyWord");
				var reloaded = Factory.Load<WordDictionary>(WordDictionaryManager.Instance.CurrentDictionary.PK);
				AssertContains("The added word should be immediately saved to the db", "MyWord", reloaded.DIC_WordsList);
			}
		}

		public void TestAddWord_Null()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = "XD";
			Factory.Save();
			using (LoginAsUser(user))
			{
				var oldList = WordDictionaryManager.Instance.CurrentDictionary.DIC_WordsList;
				AssertNoExceptionThrown("Null should be handled", () => WordDictionaryManager.Instance.AddWord(null));
				var newList = WordDictionaryManager.Instance.CurrentDictionary.DIC_WordsList;
				AssertEquals("No words should have been added.", oldList, newList);
			}
		}

		IDisposable LoginAsUser(IGlbStaff staff) => Env.Instance.SetTemporaryUserContext(staff.GS_LoginName, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK);
		IGlbStaff CreateUserWithDictionary(BusinessObjectFactory factory, string code)
		{
			var user = (IGlbStaff)factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = code;
			var dictionary = factory.NewWithValidTestData<WordDictionary>();
			dictionary.DIC_GS_NKStaff = code;
			return user;
		}
	}
}