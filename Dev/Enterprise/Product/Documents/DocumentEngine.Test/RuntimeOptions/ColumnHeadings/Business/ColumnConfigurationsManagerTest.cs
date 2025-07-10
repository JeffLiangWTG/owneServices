using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ColumnConfigurationsManagerTest : TestCaseWithFactory
	{
		public void TestOrganisationsOnlyGetHitOnceOnLoad()
		{
			ColumnConfigurationManager[] settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
			AssertEquals("Precondition: GetSettings().Length", 2, settings.Length);
			AssertEquals("Precondition: GetSettings()[0]", HeadingManager.DefaultTemplateConfigurationManager, settings[0]);
			AssertEquals("Precondition: GetSettings()[1]", HeadingManager.CompanyDefaultConfigurationManager, settings[1]);

			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "FATGUTS1";
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "FATGUTS2";
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();
			client3.OH_Code = "FATGUTS3";

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("1") }), "Sheet1"));
			HeadingManager.AddLinkedField(new LookupFieldForTest(Factory, new OrgHeaderCollectionProvider(otherFactory)) { FieldName = "Field", DisplayName = "Field" });
			HeadingManager.SaveToFilterField = "Field";
			new CombinedConfigurationManager(HeadingManager, client1.PK, client1.OH_Code, "", "Test1").Save();
			new CombinedConfigurationManager(HeadingManager, client2.PK, client2.OH_Code, "", "Test2").Save();
			new CombinedConfigurationManager(HeadingManager, client3.PK, client3.OH_Code, "", "Test3").Save();

			AssertEquals("Precondition: Table Hit Counter for OrgHeader", 0, otherFactory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));
			HeadingManager.SetCombinedConfigurationManagers(null); // Reset to force loading from dbo.StmData
			settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
			AssertEquals("Precondition: GetSettings().Length", 5, settings.Length);
			AssertEquals("Precondition: GetSettings()[0]", HeadingManager.DefaultTemplateConfigurationManager, settings[0]);
			AssertEquals("Precondition: GetSettings()[1]", HeadingManager.CompanyDefaultConfigurationManager, settings[1]);
			AssertEquals("Precondition: GetSettings()[2]", "FATGUTS1 (as Field) - Test1", settings[2].ToString());
			AssertEquals("Precondition: GetSettings()[3]", "FATGUTS2 (as Field) - Test2", settings[3].ToString());
			AssertEquals("Precondition: GetSettings()[4]", "FATGUTS3 (as Field) - Test3", settings[4].ToString());

			AssertEquals("Should only have been one DB Hit.", 1, otherFactory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));
		}

		public void TestSavesToIsIGuidNotifier()
		{
			ReportColumnSettingKeys keys = new ReportColumnSettingKeys("Hello", ZGuid.NewZGuid(), ZGuid.NewZGuid());
			HeadingManager.AddLinkedField(new MultipleChoice(this.Factory));
			HeadingManager.SaveToFilterField = "TestField";

			AssertNoExceptionThrown(delegate
			{ HeadingManager.LoadConfigFromKeys(keys); });
		}

		public void TestGetHeadingText()
		{
			var headings = new[]
			{
				new ColumnHeading("x", "x", "z", 0, 0, 0, false),
				new ColumnHeading("y", "y", "y", 0, 0, 0, false)
			};
			AssertEquals("GetHeadingText(\"x\")", "", HeadingManager.GetHeadingText("Sheet1", "x"));
			HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings = new ColumnHeadingCollection(headings);
			AssertEquals("GetHeadingText(\"x\")", "z", HeadingManager.GetHeadingText("Sheet1", "x"));
			AssertEquals("GetHeadingText(\"y\")", "y", HeadingManager.GetHeadingText("Sheet1", "y"));
		}

		public void TestIsEmpty()
		{
			AssertEquals("IsEmpty", true, HeadingManager.IsEmpty);
			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("1") }), "Sheet1"));
			AssertEquals("IsEmpty", false, HeadingManager.IsEmpty);
		}

		public void TestIsReportTitleChangeable()
		{
			AssertEquals("isReportTitleChangeable", false, HeadingManager.IsReportTitleChangeable);
		}

		public void TestJsonConverter()
		{
			var headings = new ColumnHeading[]
			{
				new ColumnHeading("1", "1d", "a", 3, 4, 5, true),
				new ColumnHeading("2", "2d", "b", 4, 5, 0, false),
				new ColumnHeading("3", "3d", "c", 5, 6, 7, false),
				new ColumnHeading("4", "4d", "d", 6, 7, 8, false)
			};
			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headings), "Sheet1"));

			var result = JsonConverterHelper.Serialize(HeadingManager);
			var deserialisedValue = JsonConverterHelper.Deserialize<ColumnConfigurationsManager>(result);

			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1", "1d", "a", 3, 20, 5, false));
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("2", "2d", "b", 12, 10, 6, true));
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("4", "4d", "x", 6, 7, 100, false));
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("5", "5d", "e", 7, 8, 9, false));

			HeadingManager.UpdateFromDeserialisedValue(deserialisedValue, null);
			AssertEquals("Headings.Count", 4, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "1", "1d", "a", 3, 20, 5, true);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1], "2", "2d", "b", 12, 5, 6, false);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2], "4", "4d", "d", 6, 7, 8, false);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[3], "5", "5d", "e", 7, 8, 9, true);
		}

		public void TestSettingNames()
		{
			ColumnConfigurationManager[] settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
			AssertEquals("GetSettings().Length", 2, settings.Length);
			AssertEquals("GetSettings()[0]", HeadingManager.DefaultTemplateConfigurationManager, settings[0]);
			AssertEquals("GetSettings()[1]", HeadingManager.CompanyDefaultConfigurationManager, settings[1]);

			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("1") }), "Sheet1"));
			new CombinedConfigurationManager(HeadingManager, "Test").Save();
			new CombinedConfigurationManager(HeadingManager, "SecondTest").Save();

			settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
			AssertEquals("GetSettings().Length", 4, settings.Length);
			AssertEquals("GetSettings()[0]", HeadingManager.DefaultTemplateConfigurationManager, settings[0]);
			AssertEquals("GetSettings()[1]", HeadingManager.CompanyDefaultConfigurationManager, settings[1]);
			AssertEquals("GetSettings()[2]", "SecondTest", settings[2].ToString());
			AssertEquals("GetSettings()[3]", "Test", settings[3].ToString());

			HeadingManager.UpdateFromDeserialisedValue(new ColumnConfigurationsManager(HeadingManager.ReportID, HeadingManager.Scheduled), null);
			settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
			AssertEquals("GetSettings().Length", 5, settings.Length);
			AssertEquals("GetSettings()[0].GetType()", typeof(SavedScheduleConfigurationManager), settings[0].GetType());
		}

		public void TestSettingNamesForWeb()
		{
			Globals.IsWeb = true;
			try
			{
				ColumnConfigurationManager[] settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
				AssertEquals("GetSettings().Length", 2, settings.Length);
				AssertEquals("GetSettings()[0]", HeadingManager.DefaultTemplateConfigurationManager, settings[0]);
				AssertEquals("GetSettings()[1]", HeadingManager.CompanyDefaultConfigurationManager, settings[1]);

				GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();
				otherCompany.GC_Code = "AAA";
				otherCompany.GC_Name = "AAA Company";
				otherCompany.GC_RN_NKCountryCode = "AU";
				otherCompany.GC_RX_NKLocalCurrency = "AUD";
				GlbBranch otherBranch = otherCompany.Branches.AddNew();
				otherBranch.GB_Code = "ABR";

				GlbCompany inactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
				inactiveCompany.GC_Code = "BBB";
				inactiveCompany.GC_Name = "BBB Company";
				inactiveCompany.GC_RN_NKCountryCode = "AU";
				inactiveCompany.GC_RX_NKLocalCurrency = "AUD";
				inactiveCompany.GC_IsActive = false;
				GlbBranch inactiveBranch = inactiveCompany.Branches.AddNew();
				inactiveBranch.GB_Code = "BBR";

				OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("1") }), "Sheet1"));
				HeadingManager.AddLinkedField(new LookupFieldForTest(Factory, new OrgHeaderCollectionProvider(Factory)) { FieldName = "Field", DisplayName = "Field" });
				HeadingManager.SaveToFilterField = "Field";
				new CombinedConfigurationManager(HeadingManager, client.PK, client.OH_Code, "", "Test").Save();
				using (Environment.Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())) // Have to change CurrentCompany because configurations are always saved with CurrentCompany code
				{
					new CombinedConfigurationManager(HeadingManager, client.PK, client.OH_Code, "", "SecondTest", otherCompany.GC_Code).Save();
				}
				using (Environment.Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, inactiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					new CombinedConfigurationManager(HeadingManager, client.PK, client.OH_Code, "", "ThrirdTest", inactiveCompany.GC_Code).Save();
				}

				HeadingManager.SetCombinedConfigurationManagers(null); // Reset to force loading from dbo.StmData
				settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
				AssertEquals("GetSettings().Length", 4, settings.Length);
				AssertEquals("GetSettings()[0]", HeadingManager.DefaultTemplateConfigurationManager, settings[0]);
				AssertEquals("GetSettings()[1]", HeadingManager.CompanyDefaultConfigurationManager, settings[1]);
				AssertEquals("GetSettings()[2]", "AAA Company(AAA) - SecondTest", settings[2].ToString());
				AssertEquals("GetSettings()[3]", string.Format("{0}({1}) - Test", GlbCompany.CurrentCompany.GC_Name, GlbCompany.CurrentCompany.GC_Code), settings[3].ToString());

				HeadingManager.UpdateFromDeserialisedValue(new ColumnConfigurationsManager(HeadingManager.ReportID, HeadingManager.Scheduled), null);
				settings = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
				AssertEquals("GetSettings().Length", 5, settings.Length);
				AssertEquals("GetSettings()[0].GetType()", typeof(SavedScheduleConfigurationManager), settings[0].GetType());
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestTemplateSetting()
		{
			AssertNotNull("TemplateSetting", HeadingManager.DefaultTemplateConfigurationManager);
		}

		public void TestAllConfigsSort()
		{
			ColumnConfigurationManager[] savedConfigurations = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();

			CombinedConfigurationManager newPerFilterBBBAAA = new CombinedConfigurationManager(HeadingManager, ZGuid.NewZGuid(), "BBB", "Whatever", "AAA");
			newPerFilterBBBAAA.Save();
			CombinedConfigurationManager newPerFilterBBBBBB = new CombinedConfigurationManager(HeadingManager, ZGuid.NewZGuid(), "BBB", "Whatever", "BBB");
			newPerFilterBBBBBB.Save();
			CombinedConfigurationManager newPerNameAAA = new CombinedConfigurationManager(HeadingManager, "AAA");
			newPerNameAAA.Save();
			CombinedConfigurationManager newPerNameCCC = new CombinedConfigurationManager(HeadingManager, "CCC");
			newPerNameCCC.Save();

			savedConfigurations = HeadingManager.ConfigurationManagersForAllSavedConfigurations.ToArray();
			AssertEquals("CompanySetting should be first", HeadingManager.DefaultTemplateConfigurationManager, savedConfigurations[0]);
			AssertEquals("Template default should be Second", HeadingManager.CompanyDefaultConfigurationManager, savedConfigurations[1]);
			AssertEquals("AAA filter config should be third", newPerNameAAA, savedConfigurations[2]);
			AssertEquals("BBBAAA filter config should be fourth", newPerFilterBBBAAA, savedConfigurations[3]);
			AssertEquals("BBBBBB filter config should be fifth", newPerFilterBBBBBB, savedConfigurations[4]);
			AssertEquals("CCC filter config should be last", newPerNameCCC, savedConfigurations[5]);
		}

		public void TestGetSetCombinedConfigurationManagers()
		{
			List<CombinedConfigurationManager> testCombinedConfigurationManagers = new List<CombinedConfigurationManager>(2);
			CombinedConfigurationManager newPerFilterBBBAAA = new CombinedConfigurationManager(HeadingManager, ZGuid.NewZGuid(), "BBB", "Whatever", "AAA");
			testCombinedConfigurationManagers.Add(newPerFilterBBBAAA);
			CombinedConfigurationManager newPerFilterBBBBBB = new CombinedConfigurationManager(HeadingManager, ZGuid.NewZGuid(), "BBB", "Whatever", "BBB");
			testCombinedConfigurationManagers.Add(newPerFilterBBBBBB);

			HeadingManager.SetCombinedConfigurationManagers(testCombinedConfigurationManagers);
			AssertEquals(testCombinedConfigurationManagers, HeadingManager.GetCombinedConfigurationManagers());
		}

		#region Implementation
		[Serializable]
		class LookupFieldForTest : LookupField
		{
			public LookupFieldForTest(BusinessObjectFactory factory, CollectionProvider collectionProvider)
				: base(factory)
			{
				this.fCollectionProvider = collectionProvider;
			}

#if NETFRAMEWORK
			protected LookupFieldForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(new BusinessObjectFactory())
			{
			}
#endif
		}

		ColumnConfigurationsManager HeadingManager
		{
			get
			{
				if (headingManager == null)
				{
					headingManager = new ColumnConfigurationsManager(ZGuid.NewZGuid(), false);
					headingManager.CurrentConfiguration.Worksheets.AddNew("Sheet1");
				}
				return headingManager;
			}
		}
		ColumnConfigurationsManager headingManager;
		#endregion
	}
}
