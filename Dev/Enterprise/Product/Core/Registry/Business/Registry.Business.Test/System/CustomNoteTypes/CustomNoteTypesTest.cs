using System.Collections;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomNoteTypes))]
	sealed class CustomNoteTypesTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestAllCustomNoteTypes()
		{
			CustomNoteTypes noteTypes = new CustomNoteTypes();

			CustomNoteModuleAndCountry module1 = noteTypes.NoteModuleAndCountryList.AddNew();
			module1.ModuleIDName = "ModuleID1";
			module1.CountryCode = Env.CurrentCompany.Country.Code;
			CustomNoteTypeItem item1 = module1.CustomNoteTypesList.AddNew();
			item1.NoteName = "ModuleID1item1";
			item1.DefaultVisibility = "PUB";
			CustomNoteTypeItem item2 = module1.CustomNoteTypesList.AddNew();
			item2.NoteName = "ModuleID1item2";
			item2.DefaultVisibility = "PUB";

			CustomNoteModuleAndCountry module2 = noteTypes.NoteModuleAndCountryList.AddNew();
			module2.ModuleIDName = "ModuleID2";
			module2.CountryCode = "SN";
			CustomNoteTypeItem item3 = module2.CustomNoteTypesList.AddNew();
			item3.NoteName = "ModuleID2item3";
			item3.DefaultVisibility = "PUB";
			CustomNoteTypeItem item4 = module2.CustomNoteTypesList.AddNew();
			item4.NoteName = "ModuleID2item4";
			item4.DefaultVisibility = "PUB";

			AssertEquals("The ALL collection has 4 items", 4, noteTypes.AllCustomNoteTypes.Count);
			AssertEquals("ModuleID1item1", ((PredefinedNoteType)((IList)noteTypes.AllCustomNoteTypes)[0]).Description);
			AssertEquals("ModuleID1item2", ((PredefinedNoteType)((IList)noteTypes.AllCustomNoteTypes)[1]).Description);
			AssertEquals("ModuleID2item3", ((PredefinedNoteType)((IList)noteTypes.AllCustomNoteTypes)[2]).Description);
			AssertEquals("ModuleID2item4", ((PredefinedNoteType)((IList)noteTypes.AllCustomNoteTypes)[3]).Description);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CustomNoteTypes noteTypes = new CustomNoteTypes();

			CustomNoteModuleAndCountry item = noteTypes.NoteModuleAndCountryList.AddNew();
			item.ModuleIDName = "ModuleID";
			item.CountryCode = "SN";

			return noteTypes;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			CustomNoteTypes lhs = (CustomNoteTypes)originalBusinessObject;
			CustomNoteTypes rhs = (CustomNoteTypes)newBusinessObject;

			AssertEquals(lhs.NoteModuleAndCountryList.Count, rhs.NoteModuleAndCountryList.Count);
			for (int i = 0; i < lhs.NoteModuleAndCountryList.Count; i++)
			{
				CheckAllPropertiesInZPropertyInfoHashAreEqual(lhs.NoteModuleAndCountryList[i], rhs.NoteModuleAndCountryList[i]);
			}
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
