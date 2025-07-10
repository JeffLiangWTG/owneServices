using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomNoteModuleAndCountry))]
	sealed class NoteModuleAndCountryTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CustomNoteModuleAndCountry noteModule = new CustomNoteModuleAndCountry();
			noteModule.ModuleIDName = "ModuleID";
			noteModule.CountryCode = "SN";

			CustomNoteTypeItem item = noteModule.CustomNoteTypesList.AddNew();
			item.IsTextOnly = ZBool.True;
			item.IsAppendingNote = ZBool.True;
			item.IsReadOnlyAfterAdd = ZBool.False;
			item.ForceRead = ZBool.False;
			item.DefaultVisibility = nameof(StmNoteVisibility.PRV);

			return noteModule;
		}

		public void TestAllCustomNoteTypesAndCustomNoteTypesForModuleAndThisCountry()
		{
			CustomNoteTypes noteTypes = new CustomNoteTypes();

			CustomNoteModuleAndCountry module1 = noteTypes.NoteModuleAndCountryList.AddNew();
			module1.ModuleIDName = "ModuleID1";
			module1.CountryCode = CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL;
			CustomNoteTypeItem item1 = module1.CustomNoteTypesList.AddNew();
			item1.NoteName = "ModuleID1item1";
			item1.DefaultVisibility = "PUB";
			item1.IsTextOnly = false;
			CustomNoteTypeItem item2 = module1.CustomNoteTypesList.AddNew();
			item2.NoteName = "ModuleForTest";
			item2.DefaultVisibility = "PUB";
			item2.IsTextOnly = true;

			CustomNoteModuleAndCountry module2 = noteTypes.NoteModuleAndCountryList.AddNew();
			module2.ModuleIDName = "ModuleID2";
			module2.CountryCode = CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL;
			CustomNoteTypeItem item3 = module2.CustomNoteTypesList.AddNew();
			item3.NoteName = "ModuleID2item1";
			item3.DefaultVisibility = "PUB";
			item3.IsTextOnly = false;
			CustomNoteTypeItem item4 = module2.CustomNoteTypesList.AddNew();
			item4.NoteName = "ModuleForTest";
			item4.DefaultVisibility = "PUB";
			item4.IsTextOnly = false;

			NoteTypeCollection collection1 = noteTypes.GetNoteTypesForModuleAndThisCountry("ModuleID2");
			NoteTypeCollection collection2 = noteTypes.AllCustomNoteTypes;

			Assert(collection1.IsPredefinedNoteTypeByDescription("ModuleForTest"));
			Assert(collection2.IsPredefinedNoteTypeByDescription("ModuleForTest"));

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var stmNote = Factory.NewWithValidTestData<StmNote>();
			stmNote.Master = (IStmNoteParent)declaration;

			stmNote.ST_IsCustomDescription = false;
			stmNote.ST_Description = "ModuleForTest";
			System.Reflection.PropertyInfo propertyInfo = stmNote.GetType().GetProperty("ST_Description_List");
			propertyInfo.SetValue(stmNote, collection1);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, noteTypes);
			Assert(!stmNote.ST_IsTextOnly);

			CustomNoteModuleAndCountry module3 = noteTypes.NoteModuleAndCountryList.AddNew();
			var countryCodes = module3.CountriesOfAllCompanies;
			AssertEquals("Expecting two countries for global companies in test DB", 2, countryCodes.Count);
			var countryCodesExtracted = countryCodes.ToArray().Select(i => i[RefCountrySchema.RN_Code].ToString()).ToList();
			AssertCollectionContains("Expecting AU in countries for global companies in test DB", "AU", countryCodesExtracted);
			AssertCollectionContains("Expecting SG in countries for global companies in test DB", "SG", countryCodesExtracted);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
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
