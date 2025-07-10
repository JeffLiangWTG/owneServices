using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Registry.Business.ExportStatementSetting;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ExportStatementSetting))]
	class ExportStatementSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateName()
		{
			ExportStatementSettingCollection settings = new ExportStatementSettingCollection();
			ExportStatementSetting setting = settings.AddNew();
			settings.RunPreSaveValidation();
			AssertHasErrorContaining(setting.CodeInfo, "Please enter a Code.");

			setting.Code = "NDR";
			AssertNoErrorContaining(setting.CodeInfo, "Please enter a Code.");

			ExportStatementSetting setting2 = settings.AddNew();
			setting2.Code = setting.Code;
			settings.RunPreSaveValidation();
			AssertHasErrorContaining(setting2.CodeInfo, "Duplicate Codes are entered.");

			setting2.Code = "AES";
			settings.RunPreSaveValidation();
			AssertEquals("HasErrors", false, setting2.CodeInfo.HasErrors());
		}

		public void TestExportStatementSettingConstructorWithManyParams()
		{
			CountryExportStatementSetting countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			ExportStatementSetting setting = new ExportStatementSetting(countrySetting,
				"AES",
				"NO SED REQ. - AES",
				"",
				"XTN",
				"ITN",
				"UDF",
				true,
				false,
				false,
				true,
				true,
				false);

			AssertEquals("Parent", countrySetting.GetHashCode(), setting.Parent.GetHashCode());
			AssertEquals("Code", "AES", setting.Code);
			AssertEquals("Statement", "NO SED REQ. - AES", setting.Statement);
			AssertEquals("Statement", "NO SED REQ. - AES", setting.StatementDescription);
			AssertEquals("Field1", "XTN", setting.Field1);
			AssertEquals("Field2", "ITN", setting.Field2);
			AssertEquals("Visibility", "UDF", setting.Visibility);
			AssertEquals("UseOnHawb", true, setting.UseOnHawb);
			AssertEquals("UseOnDirectIATAMawb", false, setting.UseOnDirectIATAMawb);
			AssertEquals("UseOnConsolidationMawb", false, setting.UseOnConsolidationMawb);
			AssertEquals("UseOnHouseBillOfLading", true, setting.UseOnHouseBillOfLading);
			AssertEquals("UseOnDirectMasterBillOfLading", true, setting.UseOnDirectMasterBillOfLading);
			AssertEquals("UseOnConsolidationMasterBillOfLading", false, setting.UseOnConsolidationMasterBillOfLading);
		}

		public void TestValidation()
		{
			ExportStatementSetting bizObj = new ExportStatementSetting();
			bizObj.RunPreSaveValidation();
			AssertHasErrorContaining(bizObj.CodeInfo, "Please enter a Code.");
			AssertHasErrorContaining(bizObj.StatementInfo, "Please enter a Statement.");

			bizObj.Code = "NAR";
			bizObj.Statement = "SED Attached";
			AssertNoErrorContaining(bizObj.CodeInfo, "Please enter a Code.");
			AssertNoErrorContaining(bizObj.StatementInfo, "Please enter a Statement.");

			bizObj.Field1 = "G";
			AssertHasError(bizObj.Field1Info, "Enter a valid selection.");
			bizObj.Field2 = "G";
			AssertHasError(bizObj.Field2Info, "Enter a valid selection.");

			CodeDescriptionPairList list = bizObj.StatementFieldTypeList;
			foreach (CodeDescriptionPair pair in list)
			{
				bizObj.Field1 = pair.Code;
				AssertEquals("There should be no error", false, bizObj.Field1Info.HasError("Enter a valid selection."));
				bizObj.Field2 = pair.Code;
				AssertEquals("There should be no error", false, bizObj.Field2Info.HasError("Enter a valid selection."));
			}
		}

		public void TestCountryStatementFieldTypeList_IsSame_ForUSAOrTerritory()
		{
			var countrySettings = new CountryExportStatementSetting
			{
				CountryCode = "AU"
			};

			var setting = new ExportStatementSetting(countrySettings)
			{
				Code = "ABC",
				Statement = "DEF",
				Field1 = "GHI",
				Field2 = "JKL"
			};

			var usaAndTerritoriesList = new List<string>()
			{
				Constants.CountryCodes.PuertoRico,
				Constants.CountryCodes.UnitedStates,
				Constants.CountryCodes.Guam,
				Constants.CountryCodes.NorthernMarianaIslands,
				Constants.CountryCodes.VirginIslands,
				Constants.CountryCodes.AmericanSamoa
			};

			AssertEquals("Pre-Condition - should be an empty pair-list", new CodeDescriptionPairList(), setting.StatementFieldTypeList);

			foreach (var country in usaAndTerritoriesList)
			{
				countrySettings.CountryCode = country;

				CombineAssertions($"The country: {country} should have populated StatementFieldTypeList", delegate
				{
					AssertEquals(new SEDStatementFieldType(), setting.StatementFieldTypeList as SEDStatementFieldType);
				});
			}

			countrySettings.CountryCode = Constants.CountryCodes.NewZealand;
			AssertEquals("There should be no items in the dropdown (empty pair list)", new CodeDescriptionPairList(), setting.StatementFieldTypeList);
		}

		public void TestRedefaultOtherVisibilitySettings_ForUSAOrTerritory()
		{
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "AU" };
			var exportStatementSettings = new[]
			{
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "ABC", Statement = "ABC", StatementDescription = "ABC"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = UsExportStatementSettings.ReferenceSet.First().Code, Statement = UsExportStatementSettings.ReferenceSet.First().Statement, StatementDescription = UsExportStatementSettings.ReferenceSet.First().StatementDescription
					}
			};
			var systemDefinedExportStatementSetting = exportStatementSettings.Where(c => c.Code == UsExportStatementSettings.ReferenceSet.First().Code).First();
			AssertEquals(false, systemDefinedExportStatementSetting.UseOnHawb);
			AssertEquals(false, systemDefinedExportStatementSetting.UseOnDirectIATAMawb);
			AssertEquals(false, systemDefinedExportStatementSetting.UseOnConsolidationMawb);
			AssertEquals(false, systemDefinedExportStatementSetting.UseOnHouseBillOfLading);
			AssertEquals(false, systemDefinedExportStatementSetting.UseOnDirectMasterBillOfLading);
			AssertEquals(false, systemDefinedExportStatementSetting.UseOnConsolidationMasterBillOfLading);

			systemDefinedExportStatementSetting.Visibility = VisibilityList.Mandatory;
			AssertEquals("The current country code does not belong to the United States or its territories, and even if Visibility is changed to MAN, we will not modify its other settings", false, systemDefinedExportStatementSetting.UseOnHawb);
			AssertEquals("The current country code does not belong to the United States or its territories, and even if Visibility is changed to MAN, we will not modify its other settings", false, systemDefinedExportStatementSetting.UseOnDirectIATAMawb);
			AssertEquals("The current country code does not belong to the United States or its territories, and even if Visibility is changed to MAN, we will not modify its other settings", false, systemDefinedExportStatementSetting.UseOnConsolidationMawb);
			AssertEquals("The current country code does not belong to the United States or its territories, and even if Visibility is changed to MAN, we will not modify its other settings", false, systemDefinedExportStatementSetting.UseOnHouseBillOfLading);
			AssertEquals("The current country code does not belong to the United States or its territories, and even if Visibility is changed to MAN, we will not modify its other settings", false, systemDefinedExportStatementSetting.UseOnDirectMasterBillOfLading);
			AssertEquals("The current country code does not belong to the United States or its territories, and even if Visibility is changed to MAN, we will not modify its other settings", false, systemDefinedExportStatementSetting.UseOnConsolidationMasterBillOfLading);

			foreach (var countryCode in Constants.CountryCodes.UsaAndTerritoriesList)
			{
				countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = countryCode };
				exportStatementSettings = new[]
				{
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "ABC", Statement = "ABC", StatementDescription = "ABC"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = UsExportStatementSettings.ReferenceSet.First().Code, Statement = UsExportStatementSettings.ReferenceSet.First().Statement, StatementDescription = UsExportStatementSettings.ReferenceSet.First().StatementDescription
					}
				};

				var firstExportStatementSetting = exportStatementSettings.Where(c => c.Code == "ABC").First();
				firstExportStatementSetting.Visibility = VisibilityList.Mandatory;
				AssertEquals("The first Export Statement Setting is not system-difined, so its other options will not be changed to true", false, firstExportStatementSetting.UseOnHawb);
				AssertEquals("The first Export Statement Setting is not system-difined, so its other options will not be changed to true", false, firstExportStatementSetting.UseOnDirectIATAMawb);
				AssertEquals("The first Export Statement Setting is not system-difined, so its other options will not be changed to true", false, firstExportStatementSetting.UseOnConsolidationMawb);
				AssertEquals("The first Export Statement Setting is not system-difined, so its other options will not be changed to true", false, firstExportStatementSetting.UseOnHouseBillOfLading);
				AssertEquals("The first Export Statement Setting is not system-difined, so its other options will not be changed to true", false, firstExportStatementSetting.UseOnDirectMasterBillOfLading);
				AssertEquals("The first Export Statement Setting is not system-difined, so its other options will not be changed to true", false, firstExportStatementSetting.UseOnConsolidationMasterBillOfLading);

				systemDefinedExportStatementSetting = exportStatementSettings.Where(c => c.Code == UsExportStatementSettings.ReferenceSet.First().Code).First();
				AssertEquals(false, systemDefinedExportStatementSetting.UseOnHawb);
				AssertEquals(false, systemDefinedExportStatementSetting.UseOnDirectIATAMawb);
				AssertEquals(false, systemDefinedExportStatementSetting.UseOnConsolidationMawb);
				AssertEquals(false, systemDefinedExportStatementSetting.UseOnHouseBillOfLading);
				AssertEquals(false, systemDefinedExportStatementSetting.UseOnDirectMasterBillOfLading);
				AssertEquals(false, systemDefinedExportStatementSetting.UseOnConsolidationMasterBillOfLading);

				systemDefinedExportStatementSetting.Visibility = VisibilityList.Mandatory;
				AssertEquals("The second Export Statement Setting is system-difined, so its other options will be changed to true", true, systemDefinedExportStatementSetting.UseOnHawb);
				AssertEquals("The second Export Statement Setting is system-difined, so its other options will be changed to true", true, systemDefinedExportStatementSetting.UseOnDirectIATAMawb);
				AssertEquals("The second Export Statement Setting is system-difined, so its other options will be changed to true", true, systemDefinedExportStatementSetting.UseOnConsolidationMawb);
				AssertEquals("The second Export Statement Setting is system-difined, so its other options will be changed to true", true, systemDefinedExportStatementSetting.UseOnHouseBillOfLading);
				AssertEquals("The second Export Statement Setting is system-difined, so its other options will be changed to true", true, systemDefinedExportStatementSetting.UseOnDirectMasterBillOfLading);
				AssertEquals("The second Export Statement Setting is system-difined, so its other options will be changed to true", true, systemDefinedExportStatementSetting.UseOnConsolidationMasterBillOfLading);

				systemDefinedExportStatementSetting.UseOnHawb = false;
				systemDefinedExportStatementSetting.UseOnDirectIATAMawb = false;
				systemDefinedExportStatementSetting.UseOnConsolidationMawb = false;
				systemDefinedExportStatementSetting.UseOnHouseBillOfLading = false;
				systemDefinedExportStatementSetting.UseOnDirectMasterBillOfLading = false;
				systemDefinedExportStatementSetting.UseOnConsolidationMasterBillOfLading = false;

				systemDefinedExportStatementSetting.Visibility = VisibilityList.UserDefined;
				AssertEquals("When Visibility is changed to UDF, we will not modify any other settings", false, systemDefinedExportStatementSetting.UseOnHawb);
				AssertEquals("When Visibility is changed to UDF, we will not modify any other settings", false, systemDefinedExportStatementSetting.UseOnDirectIATAMawb);
				AssertEquals("When Visibility is changed to UDF, we will not modify any other settings", false, systemDefinedExportStatementSetting.UseOnConsolidationMawb);
				AssertEquals("When Visibility is changed to UDF, we will not modify any other settings", false, systemDefinedExportStatementSetting.UseOnHouseBillOfLading);
				AssertEquals("When Visibility is changed to UDF, we will not modify any other settings", false, systemDefinedExportStatementSetting.UseOnDirectMasterBillOfLading);
				AssertEquals("When Visibility is changed to UDF, we will not modify any other settings", false, systemDefinedExportStatementSetting.UseOnConsolidationMasterBillOfLading);
			}
		}

		public void TestIsInvalid_WhenSettingIsNotFromDefaultUSList_ForUsaAndTerritoriesList()
		{
			foreach (var countryCode in Constants.CountryCodes.UsaAndTerritoriesList)
			{
				// Arrange
				var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = countryCode };
				var exportStatementSettings = new[]
				{
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "NAR", Statement = "SED Attached", StatementDescription = "SED Attached"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "NDR", Statement = "NO SED REQ. 30.55 (h) FTSR", StatementDescription = "NO SED REQ. 30.55 (h) FTSR"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "AES", Statement = "NO SED REQ.-AES", StatementDescription = "NO SED REQ.-AES"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a)"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "TOT", Statement = "NOEEI §30.37(b)", StatementDescription = "NOEEI §30.37(b)"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "TMP", Statement = "NOEEI §30.37(r)", StatementDescription = "NOEEI §30.37(r)"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "ARM", Statement = "NOEEI §30.39", StatementDescription = "NOEEI §30.39"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "UNK", Statement = "Some Statement", StatementDescription = "Some Statement Description"
					},
				};

				CombineAssertions(() =>
				{
					foreach (var exportStatementSetting in exportStatementSettings)
					{
						// Act
						var hasValidationError = exportStatementSetting.RowErrors.Contains(
							"This entry is not a valid US Export Statement and must be removed from the list.");

						// Assert
						Assert($"Export Statement Setting {exportStatementSetting.Code} is expected to be invalid.",
							hasValidationError);
					}
				});
			}
		}

		public void TestIsValid_WhenSettingIsFromDefaultUSList()
		{
			// Arrange
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "AU" };
			var exportStatementSettings = new[]
			{
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "NAR", Statement = "SED Attached", StatementDescription = "SED Attached"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "NDR", Statement = "NO SED REQ. 30.55 (h) FTSR", StatementDescription = "NO SED REQ. 30.55 (h) FTSR"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "AES", Statement = "NO SED REQ.-AES", StatementDescription = "NO SED REQ.-AES"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TOT", Statement = "NOEEI §30.37(b)", StatementDescription = "NOEEI §30.37(b)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TMP", Statement = "NOEEI §30.37(r)", StatementDescription = "NOEEI §30.37(r)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "ARM", Statement = "NOEEI §30.39", StatementDescription = "NOEEI §30.39"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "UNK", Statement = "Some Statement", StatementDescription = "Some Statement Description"
				}
			};

			CombineAssertions(() =>
			{
				foreach (var exportStatementSetting in exportStatementSettings)
				{
					// Act
					var isValid = !exportStatementSetting.Notifications.Any();

					// Assert
					Assert($"Export Statement Setting {exportStatementSetting.Code} is expected to be valid.", isValid);
				}
			});
		}

		public void TestIsReadOnly_WhenSettingIsFromDefaultUSList_ForUsaAndTerritoriesList()
		{
			foreach (var countryCode in Constants.CountryCodes.UsaAndTerritoriesList)
			{
				// Arrange
				var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = countryCode };
				var exportStatementSettings = new[]
				{
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "PRF", Statement = "AES", StatementDescription = "AES Proof of Filing Citation"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "ASH", Statement = "AES", StatementDescription = "AES Split Shipments"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a) - Low Value (<$2501)"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "TOT", Statement = "NOEEI §30.37(b)", StatementDescription = "NOEEI §30.37(b) - Tools of trade"
					},
					new ExportStatementSetting(countryExportStatementSetting)
					{
						Code = "TMP", Statement = "NOEEI §30.37(r)", StatementDescription = "NOEEI §30.37(r) - Return of Temporary Import Bond"
					}
				};

				CombineAssertions(() =>
				{
					foreach (var exportStatementSetting in exportStatementSettings)
					{
						// Act
						var isNameReadOnly = exportStatementSetting.Code_ReadOnly;
						var isStatementReadOnly = exportStatementSetting.Statement_ReadOnly;
						var isStatementDescriptionReadOnly = exportStatementSetting.StatementDescription_ReadOnly;

						// Assert
						Assert($"{nameof(ExportStatementSetting.Code)} property of Export Statement Setting {exportStatementSetting.Code} is expected to be read-only.",
							isNameReadOnly);
						Assert($"{nameof(ExportStatementSetting.Statement)} property of Export Statement Setting {exportStatementSetting.Code} is expected to be read-only.",
							isStatementReadOnly);
						Assert($"{nameof(ExportStatementSetting.StatementDescription)} property of Export Statement Setting {exportStatementSetting.Code} is expected to be read-only.",
							isStatementDescriptionReadOnly);
					}
				});
			}
		}

		public void TestIsReadOnly_WhenSettingIsNotFromDefaultUSList_ForUSA()
		{
			// Arrange
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "US" };
			var exportStatementSettings = new[]
			{
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "NAR", Statement = "SED Attached", StatementDescription = "SED Attached"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TOT", Statement = "NOEEI §30.37(b)", StatementDescription = "NOEEI §30.37(b)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "UNK", Statement = "Some Statement", StatementDescription = "Some Statement Description"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TSW", Statement = "NOEEI §30.37(f)", StatementDescription = "NOEEI §30.37(f)"
				}
			};

			CombineAssertions(() =>
			{
				foreach (var exportStatementSetting in exportStatementSettings)
				{
					// Act
					var isNameReadOnly = exportStatementSetting.Code_ReadOnly;
					var isStatementReadOnly = exportStatementSetting.Statement_ReadOnly;
					var isStatementDescriptionReadOnly = exportStatementSetting.StatementDescription_ReadOnly;

					// Assert
					Assert($"{nameof(ExportStatementSetting.Code)} property of Export Statement Setting {exportStatementSetting.Code} is expected to be read-only.",
						isNameReadOnly);
					Assert($"{nameof(ExportStatementSetting.Statement)} property of Export Statement Setting {exportStatementSetting.Code} is expected to be read-only.",
						isStatementReadOnly);
					Assert($"{nameof(ExportStatementSetting.StatementDescription)} property of Export Statement Setting {exportStatementSetting.Code} is expected to be read-only.",
						isStatementDescriptionReadOnly);
				}
			});
		}

		public void TestIsNotReadOnly_WhenSettingIsFromDefaultUSList()
		{
			// Arrange
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "AU" };
			var exportStatementSettings = new[]
			{
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "PRF", Statement = "AES", StatementDescription = "AES Proof of Filing Citation"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "ASH", Statement = "AES", StatementDescription = "AES Split Shipments"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a) - Low Value (<$2501)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TOT", Statement = "NOEEI §30.37(b)", StatementDescription = "NOEEI §30.37(b) - Tools of trade"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TMP", Statement = "NOEEI §30.37(r)", StatementDescription = "NOEEI §30.37(r) - Return of Temporary Import Bond"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "UNK", Statement = "Some Statement", StatementDescription = "Some Statement Description"
				},
			};

			CombineAssertions(() =>
			{
				foreach (var exportStatementSetting in exportStatementSettings)
				{
					// Act
					var isNameReadOnly = exportStatementSetting.Code_ReadOnly;
					var isStatementReadOnly = exportStatementSetting.Statement_ReadOnly;
					var isStatementDescriptionReadOnly = exportStatementSetting.StatementDescription_ReadOnly;

					// Act & Assert
					Assert(
						$"{nameof(ExportStatementSetting.Code)} property of Export Statement Setting {exportStatementSetting.Code} is not expected to be read-only.",
						!isNameReadOnly);
					Assert(
						$"{nameof(ExportStatementSetting.Statement)} property of Export Statement Setting {exportStatementSetting.Code} is not expected to be read-only.",
						!isStatementReadOnly);
					Assert(
						$"{nameof(ExportStatementSetting.StatementDescription)} property of Export Statement Setting {exportStatementSetting.Code} is not expected to be read-only.",
						!isStatementDescriptionReadOnly);
				}
			});
		}

		public void TestCannotDelete_WhenSettingIsFromDefaultUSList_ForUSA()
		{
			// Arrange
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "US" };
			var exportStatementSettings = new[]
			{
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "PRF", Statement = "AES", StatementDescription = "AES Proof of Filing Citation"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a) - Low Value (<$2501)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "ARM", Statement = "NOEEI §30.39", StatementDescription = "NOEEI §30.39 - Shipments to US armed services"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "DIP", Statement = "NOEEI §30.37(i)", StatementDescription = "NOEEI §30.37(i) - Diplomatic pouches"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TSW", Statement = "NOEEI §30.37(f)", StatementDescription = "NOEEI §30.37(f) - 15 CFR 772 Technology and software"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "BUS", Statement = "NOEEI §30.37(k)", StatementDescription = "NOEEI §30.37(k) - Company records"
				},
			};

			CombineAssertions(() =>
			{
				foreach (var exportStatementSetting in exportStatementSettings)
				{
					// Act
					var canDelete = exportStatementSetting.CanDelete;

					// Assert
					Assert($"Export Statement Setting {exportStatementSetting.Code} must not be allowed to be deleted.", !canDelete);
				}
			});
		}

		public void TestCanDelete_WhenSettingIsNotFromDefaultUSList_ForUSA()
		{
			// Arrange
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "US" };
			var exportStatementSettings = new[]
			{
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "PRF", Statement = "PRF", StatementDescription = "AES Proof of Filing Citation"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "OTH", Statement = "NOEEI §30.37(i)", StatementDescription = "NOEEI §30.37(i) - Diplomatic pouches"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TST", Statement = "Test Statement", StatementDescription = "Test Statement Description"
				},
			};

			CombineAssertions(() =>
			{
				foreach (var exportStatementSetting in exportStatementSettings)
				{
					// Act
					var canDelete = exportStatementSetting.CanDelete;

					// Assert
					Assert($"Export Statement Setting {exportStatementSetting.Code} must be allowed to be deleted.", canDelete);
				}
			});
		}

		public void TestCanDelete_WhenSettingIsFromDefaultUSList()
		{
			// Arrange
			var countryExportStatementSetting = new CountryExportStatementSetting { CountryCode = "AU" };
			var exportStatementSettings = new[]
			{
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "PRF", Statement = "AES", StatementDescription = "AES Proof of Filing Citation"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "LOW", Statement = "NOEEI §30.37(a)", StatementDescription = "NOEEI §30.37(a) - Low Value (<$2501)"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "ARM", Statement = "NOEEI §30.39", StatementDescription = "NOEEI §30.39 - Shipments to US armed services"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "DIP", Statement = "NOEEI §30.37(i)", StatementDescription = "NOEEI §30.37(i) - Diplomatic pouches"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "TSW", Statement = "NOEEI §30.37(f)", StatementDescription = "NOEEI §30.37(f) - 15 CFR 772 Technology and software"
				},
				new ExportStatementSetting(countryExportStatementSetting)
				{
					Code = "BUS", Statement = "NOEEI §30.37(k)", StatementDescription = "NOEEI §30.37(k) - Company records"
				},
			};

			CombineAssertions(() =>
			{
				foreach (var exportStatementSetting in exportStatementSettings)
				{
					// Act
					var canDelete = exportStatementSetting.CanDelete;

					// Assert
					Assert($"Export Statement Setting {exportStatementSetting.Code} must be allowed to be deleted.", canDelete);
				}
			});
		}

		public void TestStatementFieldTypeList()
		{
			ExportStatementSetting bizObj = new ExportStatementSetting();
			AssertEquals("StatementFieldTypeList", GetExpctedStatementFieldTypeListType(), bizObj.StatementFieldTypeList.GetType());
		}

		protected virtual Type GetExpctedStatementFieldTypeListType()
		{
			return typeof(CodeDescriptionPairList);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ExportStatementSetting result = new ExportStatementSetting();
			result.Code = "NDR";
			result.Statement = "NO SED REQ. 30.55 (h) FTSR";
			return result;
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

		#endregion
	}
}
