using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryProviderTest : TransactionedTestCase
	{
		public void TestIndexGeneratedIfNotExists()
		{
			// Arrange
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(Array.Empty<RegistryItemSet>());
			var registry = CreateRegistry();

			// Act
			_ = registry.GetSortedTopLevelCategories();

			// Assert
			Assert("Index should be generated", !string.IsNullOrEmpty(GetStoredIndexContent()));
		}

		public void TestIndexNotReGenerated()
		{
			// Arrange
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(Array.Empty<RegistryItemSet>());
			_ = CreateRegistry().GetSortedTopLevelCategories();
			var writeTimeUtc = new DateTime(2023, 8, 6, 12, 30, 0);
			SetRegistryIndexLastEditTimeUtc(writeTimeUtc);

			// Act
			_ = CreateRegistry();

			// Assert
			AssertEquals("Should not regenerate INDEX when up-to-date", writeTimeUtc, GetRegistryIndexLastEditTimeUtc());
		}

		public void TestProductivityWiseChangeInvalidatesIndex()
		{
			void Test(bool enable, string message)
			{
				// Arrange
				using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(!enable))
				{
					setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(Array.Empty<RegistryItemSet>());
					_ = CreateRegistry().GetSortedTopLevelCategories();

					var oldWriteTimeUtc = new DateTime(2023, 8, 6, 12, 30, 0);
					SetRegistryIndexLastEditTimeUtc(oldWriteTimeUtc);

					// Act
					using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(enable))
					{
						_ = CreateRegistry().GetSortedTopLevelCategories();
					}

					// Assert
					AssertGreaterThan(message, GetRegistryIndexLastEditTimeUtc(), oldWriteTimeUtc);
				}
			}

			Test(enable: true, "Enabling ProductivityWise invalidates INDEX");
			Test(enable: false, "Disabling ProductivityWise invalidates INDEX");
		}

		public void TestUpgradeInvalidatesIndex()
		{
			void Test(bool addInitialUpgrade, string message)
			{
				// Arrange
				ClearAllStmUpgrades();
				if (addInitialUpgrade)
				{
					SetCurrentStmUpgrade(new Version(1, 0, 0, 0), new DateTime(2023, 8, 6, 4, 2, 0));
				}

				setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(Array.Empty<RegistryItemSet>());
				_ = CreateRegistry().GetSortedTopLevelCategories();

				var oldWriteTimeUtc = new DateTime(2023, 8, 6, 12, 30, 0);
				SetRegistryIndexLastEditTimeUtc(oldWriteTimeUtc);

				// Act
				SetCurrentStmUpgrade(new Version(2, 0, 0, 0), new DateTime(2023, 8, 7, 6, 5, 0));
				_ = CreateRegistry().GetSortedTopLevelCategories();

				// Assert
				AssertGreaterThan(message, GetRegistryIndexLastEditTimeUtc(), oldWriteTimeUtc);
			}

			Test(addInitialUpgrade: true, "New Upgrade invalidates INDEX");
			Test(addInitialUpgrade: false, "First Upgrade invalidates INDEX");
		}

		public void TestRelatedFilesChange()
		{
			void Test(bool hasUpgrade, bool shouldInvalidate, string message)
			{
				// Arrange
				ClearAllStmUpgrades();
				if (hasUpgrade)
				{
					SetCurrentStmUpgrade(new Version(1, 0), new DateTime(2023, 8, 6, 18, 0, 0));
				}

				using (var tempDirectory = new TempDirectory())
				{
					var file1 = Path.Combine(tempDirectory, "file1.asm");
					File.WriteAllBytes(file1, new byte[] { 1, 1, 1 });
					new FileInfo(file1).LastWriteTimeUtc = new DateTime(2023, 8, 6, 18, 0, 0);

					relatedFilesLocatorMock.Setup(r => r.GetRelatedFilePaths(It.IsAny<IEnumerable<RegistryItemSet>>())).Returns(new[] { file1 });
					setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(Array.Empty<RegistryItemSet>());
					_ = CreateRegistry().GetSortedTopLevelCategories();

					var oldWriteTimeUtc = new DateTime(2023, 8, 6, 12, 30, 0);
					SetRegistryIndexLastEditTimeUtc(oldWriteTimeUtc);

					// Act
					File.WriteAllBytes(file1, new byte[] { 2, 2, 2 });
					_ = CreateRegistry().GetSortedTopLevelCategories();

					// Assert
					if (shouldInvalidate)
					{
						AssertGreaterThan(message, GetRegistryIndexLastEditTimeUtc(), oldWriteTimeUtc);
					}
					else
					{
						AssertEquals(message, oldWriteTimeUtc, GetRegistryIndexLastEditTimeUtc());
					}
				}
			}

			Test(hasUpgrade: false, shouldInvalidate: true, "Changes in related files with no StmUpgrade should invalidate registry index");
			Test(hasUpgrade: true, shouldInvalidate: false, "Changes in related files with existing StmUpgrade should not invalidate registry index");
		}

		public void TestIndexIsGeneratedCorrectly()
		{
			// Arrange
			var sets = new RegistryItemSet[] { new Set1(), new Set2(), new Set3() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);

			// Act
			var registry = CreateRegistry();

			// Assert
			AssertIndexHierarchy(new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1", "Item3" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
				["cat-2"] = new Dictionary<string, object>
				{
					["cat-2-1"] = new Dictionary<string, object>
					{
						["cat-2-1-1"] = new Dictionary<string, object>
						{
							["cat-2-1-1-1"] = new Dictionary<string, object>
							{
								[ItemsTestKey] = new[] { "Item4" }
							},
						},
						[ItemsTestKey] = new[] { "Item5" },
					},
				},
			}, registry);
		}

		public void TestIndexIsStoredCorrectly()
		{
			// Arrange
			using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(false)) // CargoWise
			{
				SetCurrentStmUpgrade(new Version(1, 1, 1, 1), new DateTime(2023, 8, 4, 1, 2, 0)); // EXE20230804010200
				var sets = new RegistryItemSet[] { new Set1(), new Set2() };
				setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
				var registry = CreateRegistry();

				// Act
				_ = registry.GetSortedTopLevelCategories();

				// Assert
				AssertXmlEquals("Index is stored wrong", GetIndexContentForTest(Version), GetStoredIndexContent());
			}
		}

		public void TestClientCodeIndex_NewRegistry()
		{
			// Arrange
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(false)) // CargoWise
			{
				SetCurrentStmUpgrade(new Version(1, 1, 1, 1), new DateTime(2023, 8, 4, 1, 2, 0)); // EXE20230804010200
				var sets = new RegistryItemSet[] { new Set1(), new Set2() };
				setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
				var registry = CreateRegistry();

				// Act
				_ = registry.GetSortedTopLevelCategories();

				// Assert
				AssertXmlEquals("Index is stored wrong", GetIndexContentForTest("CargoWise-EDI-EXE20230804010200"), GetStoredIndexContent());
			}
		}

		public void TestClientCodeIndex_NoNewRegistry()
		{
			// Arrange
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.AGS))
			using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(false)) // CargoWise
			{
				SetCurrentStmUpgrade(new Version(1, 1, 1, 1), new DateTime(2023, 8, 4, 1, 2, 0)); // EXE20230804010200
				var sets = new RegistryItemSet[] { new Set1(), new Set2() };
				setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
				var registry = CreateRegistry();

				// Act
				_ = registry.GetSortedTopLevelCategories();

				// Assert
				AssertXmlEquals("Index is stored wrong", GetIndexContentForTest(Version), GetStoredIndexContent());
			}
		}

		public void TestIndexIsLoadedCorrectly()
		{
			AssertIndexIsLoaded(GetIndexContentForTest(Version), new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1", "Item3" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
			});
		}

		void AssertIndexIsLoaded(string content, Dictionary<string, object> expectedHierarchy)
		{
			// Arrange
			using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(false)) // CargoWise
			{
				SetCurrentStmUpgrade(new Version(1, 1, 1, 1), new DateTime(2023, 8, 4, 1, 2, 0)); // EXE20230804010200
				SetStoredIndexContent(content);
				var sets = new RegistryItemSet[] { new Set1(), new Set2(), new Set3() };
				setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);

				// Act
				var registry = CreateRegistry();

				// Assert
				AssertIndexHierarchy(expectedHierarchy, registry);
			}
		}

		public void TestIndexIsLoadedWhenRegistryItemDoesNotExists()
		{
			AssertIndexIsLoaded(GetIndexContentForTest(Version).Replace(nameof(Set2.Item3), "ITEM3_1"), new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
			});
		}

		public void TestBadIndexContentIsReported()
		{
			using (Env.Registry.RawRegistry.SetTemporaryProductivityWiseModeEnabledForTest(false)) // CargoWise
			{
				SetCurrentStmUpgrade(new Version(1, 1, 1, 1), new DateTime(2023, 8, 4, 1, 2, 0)); // EXE20230804010200

				void Test(string message, string content)
				{
					setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(new RegistryItemSet[] { new Set1() });
					SetStoredIndexContent(content);

					AssertExceptionThrown<FormatException>(message, () => CreateRegistry().GetSortedTopLevelCategories());
				}

				Test("Non-XML", "Bad Content");
				Test("Bad XML", "<Registry ");
				Test("Bad Index Element", $"<Registry Version=\"{Version}\"><BadIndex></BadIndex></Registry>");
				Test("Missing SetTypes Element", $"<Registry Version=\"{Version}\"><Index><Categories /></Index></Registry>");
				Test("Missing Categories Element", $"<Registry Version=\"{Version}\"><Index><SetTypes /></Index></Registry>");
				Test("Missing SetTypes Count", $"<Registry Version=\"{Version}\"><Index><SetTypes /><Categories /></Index></Registry>");
				Test("Multiple SetTypes", $"<Registry Version=\"{Version}\"><Index><SetTypes Count=\"0\"/><SetTypes Count=\"0\"/><Categories /></Index></Registry>");
				Test("Multiple Categories", $"<Registry Version=\"{Version}\"><Index><SetTypes Count=\"0\"/><Categories /><Categories /></Index></Registry>");
			}
		}

		public void TestGetSortedTopLevelCategories()
		{
			// Arrange
			var sets = new RegistryItemSet[] { new Set1() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);

			var registry = CreateRegistry();

			// Act
			var categories = registry.GetSortedTopLevelCategories().Select(c => c.DisplayText);

			// Assert
			AssertContainsExactElementsInExactOrder(new[] { "cat-1" }, categories);
		}

		public void TestHandlesCategoryTranslations()
		{
			// Arrange
			var set1 = new Set1();
			var sets = new RegistryItemSet[] { set1 };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);

			var registry1 = CreateRegistry();
			var categories1 = registry1.GetSortedTopLevelCategories().Select(c => c.DisplayText).ToList();

			set1.Cat1.Translation = "Translated Category 1";
			set1.Cat11.Translation = "Translated Category 1-1";
			var registry2 = CreateRegistry();

			// Act
			var categories2 = registry2.GetSortedTopLevelCategories().Select(c => c.DisplayText);

			// Assert
			AssertContainsExactElementsInExactOrder(new[] { "cat-1" }, categories1);
			AssertContainsExactElementsInExactOrder(new[] { "Translated Category 1" }, categories2);
		}

		public void TestRetrievesDynamicRegistryItems()
		{
			// Arrange
			var dynamicSet = new DynamicSet();
			var sets = new RegistryItemSet[] { new Set1(), dynamicSet };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			var staticHierarchy = new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
			};

			AssertIndexHierarchy(staticHierarchy, CreateRegistry());

			dynamicSet.Add("Item3-Dynamic", new[] { "cat-1", "cat-1-1" });

			var dynamicHierarchy = new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1", "Item3-Dynamic" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
			};

			// Act
			var registry = CreateRegistry();

			// Assert
			AssertIndexHierarchy(dynamicHierarchy, registry);
		}

		public void TestMergeCategoriesIsCaseInsensitive()
		{
			// Arrange
			var dynamicSet = new DynamicSet();
			dynamicSet.Add("Item3-Dynamic", new[] { "CAT-1" });
			var sets = new RegistryItemSet[] { new Set1(), dynamicSet };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			var registry = CreateRegistry();

			// Act
			var topCategories = registry.GetSortedTopLevelCategories().ToList();

			// Assert
			AssertEquals(string.Join(",", topCategories.Select(c => c.DisplayText)), 1, topCategories.Count);
		}

		public void TestIncludesDynamicCategories()
		{
			// Arrange
			var dynamicSet = new DynamicSet();
			var sets = new RegistryItemSet[] { new Set1(), dynamicSet };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			var staticHierarchy = new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
			};

			AssertIndexHierarchy(staticHierarchy, CreateRegistry());

			dynamicSet.Add("Item3-Dynamic", new[] { "cat-dyn" });

			var dynamicHierarchy = new Dictionary<string, object>
			{
				["cat-1"] = new Dictionary<string, object>
				{
					["cat-1-1"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item1" }
					},
					["cat-1-2"] = new Dictionary<string, object>
					{
						[ItemsTestKey] = new[] { "Item2" }
					},
				},
				["cat-dyn"] = new Dictionary<string, object>
				{
					[ItemsTestKey] = new[] { "Item3-Dynamic" }
				}
			};

			// Act
			var registry = CreateRegistry();

			// Assert
			AssertIndexHierarchy(dynamicHierarchy, registry);
		}

		public void TestHandlesCategoryEscapeCharacter()
		{
			// Arrange
			var dynamicSet = new DynamicSet();
			var sets = new RegistryItemSet[] { dynamicSet };
			dynamicSet.Add("rock", new[] { @"AC\/DC" });
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);

			var registry = CreateRegistry();

			// Act
			var categories = registry.GetSortedTopLevelCategories().Select(c => c.DisplayText);

			// Assert
			AssertContainsExactElementsInExactOrder(new[] { "AC/DC" }, categories);
		}

		public void TestHandlesSlashAddedInTranslation()
		{
			// Arrange
			var set1 = new Set1();
			var sets = new RegistryItemSet[] { set1 };
			set1.Cat11.Translation = "A/C/D/C";
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);

			var registry = CreateRegistry();

			// Act
			var cat1 = registry.GetSortedTopLevelCategories().First();
			var categories = registry.GetSortedContent(cat1.Key).Categories.Select(c => c.DisplayText);

			// Assert
			AssertContainsExactElementsInExactOrder(new[] { "A/C/D/C", "cat-1-2" }, categories);
		}

		public void TestItemVisibility()
		{
			// Arrange
			var sets = new RegistryItemSet[] { new Set4() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			visibilityMock.Setup(s => s.IsVisible(It.Is<IRegistryItem>(r => r.Name == "Item7"))).Returns(false);

			var registry = CreateRegistry();

			// Act
			var catZ = registry.GetSortedTopLevelCategories().First();
			var catZz = registry.GetSortedContent(catZ.Key).Categories.First();
			var items = registry.GetSortedContent(catZz.Key).Items.Select(n => n.Caption);

			// Assert
			AssertContainsExactElementsInExactOrder(new[] { "Item 6" }, items);
		}

		public void TestHasAnyVisibleStaticItemRecursive_ErrorReport()
		{
			// Arrange
			var sets = new RegistryItemSet[] { new Set4() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			visibilityMock.Setup(s => s.IsVisible(It.Is<IRegistryItem>(r => r.Name == "Item6" || r.Name == "Item7"))).Returns(false);

			var registry = CreateRegistry();

			// Act
			var catZ = registry.GetSortedTopLevelCategories().First();
			var categories = registry.GetSortedContent(catZ.Key).Categories.Select(c => c.DisplayText);

			// Now that the RegistryStaticIndex is filled out with the old sets, setup with new ones to reproduce the error
			var changedSets = new RegistryItemSet[] { new Set3() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(changedSets);

			registry = CreateRegistry();

			// Act
			_ = registry.GetSortedTopLevelCategories();
			AssertEquals(ErrorReporter.LastKeyReported, "GetStaticItems");

			AssertEquals(ErrorReporter.LastMessageReported, "Missing type(s): Enterprise.Registry.Business.Testing.RegistryProviderTest+Set4\r\n");
			ErrorReporter.Clear();
		}

		public void TestCategoryVisibility()
		{
			// Arrange
			var sets = new RegistryItemSet[] { new Set4() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			visibilityMock.Setup(s => s.IsVisible(It.Is<IRegistryItem>(r => r.Name == "Item6" || r.Name == "Item7"))).Returns(false);

			var registry = CreateRegistry();

			// Act
			var catZ = registry.GetSortedTopLevelCategories().First();
			var categories = registry.GetSortedContent(catZ.Key).Categories.Select(c => c.DisplayText);

			// Assert
			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), categories);
		}

		public void TestTopLevelCategoryVisibility()
		{
			// Arrange
			var sets = new RegistryItemSet[] { new Set4() };
			setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(sets);
			visibilityMock.Setup(s => s.IsVisible(It.IsAny<IRegistryItem>())).Returns(false);

			var registry = CreateRegistry();

			// Act
			var topLevelCategories = registry.GetSortedTopLevelCategories().Select(c => c.DisplayText);

			// Assert
			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), topLevelCategories);
		}

		public void TestRelatedFilesMaxWriteDate()
		{
			void Test((string name, DateTime lastWriteTime)[] files, DateTime expected)
			{
				// Arrange
				using (var tempDirectory = new TempDirectory())
				{
					var filePaths = new List<string>();
					foreach (var (name, lastWriteTime) in files)
					{
						var path = Path.Combine(tempDirectory, name);
						File.WriteAllText(path, string.Empty);
						new FileInfo(path).LastWriteTime = lastWriteTime;
						filePaths.Add(path);
					}

					relatedFilesLocatorMock.Setup(r => r.GetRelatedFilePaths(It.IsAny<IEnumerable<RegistryItemSet>>())).Returns(filePaths);
					setLocatorMock.Setup(s => s.GetRegistryItemSets()).Returns(Array.Empty<RegistryItemSet>());
					var provider = new RegistryProvider(setLocatorMock.Object, relatedFilesLocatorMock.Object);

					// Act
					var max = provider.GetRelatedFilesMaxWriteTime();

					// Assert
					AssertEquals(expected, max);
				}
			}

			Test(new[] { ("file1", new DateTime(2023, 8, 7, 19, 1, 2, 3)) }, new DateTime(2023, 8, 7, 19, 1, 2, 3));
			Test(Array.Empty<(string, DateTime)>(), DateTime.MinValue);
			Test(new[]
			{
				("file1", new DateTime(2023, 4, 7, 5, 1, 2, 3)),
				("file2", new DateTime(2023, 5, 7, 5, 1, 2, 3)),
				("file3", new DateTime(2023, 6, 7, 5, 1, 2, 3)),
			}, new DateTime(2023, 6, 7, 5, 1, 2, 3));
		}

		public void TestDefaultRegistryRelatedFileLocator()
		{
			// Arrange
			var relatedFilesLocator = new RegistryProvider.DefaultRegistryRelatedFileLocator();
			var sets = new RegistryItemSet[] { new Set1(), new Set2(), new Set3(), new RawDataRegistry() };

			// Act
			var files = relatedFilesLocator.GetRelatedFilePaths(sets).ToList();

			// Assert
			AssertContainsExactElementsInAnyOrder(new[] { typeof(Set1).Assembly.Location, typeof(RawDataRegistry).Assembly.Location }, files);
		}

		protected override void SetUp()
		{
			SetStoredIndexContent(string.Empty);

			setLocatorMock = new Mock<IRegistryItemSetLocator>(MockBehavior.Strict);
			relatedFilesLocatorMock = new Mock<IRegistryRelatedFilesLocator>(MockBehavior.Strict);
			relatedFilesLocatorMock.Setup(r => r.GetRelatedFilePaths(It.IsAny<IEnumerable<RegistryItemSet>>())).Returns(Array.Empty<string>());
			visibilityMock = new Mock<IRegistryItemVisibility>(MockBehavior.Strict);
			visibilityMock.Setup(v => v.IsVisible(It.IsAny<IRegistryItem>())).Returns(true);

			base.SetUp();
		}

		IRegistry CreateRegistry()
		{
			return new RegistryProvider(setLocatorMock.Object, relatedFilesLocatorMock.Object).CreateRegistry(visibilityMock.Object);
		}

		static void ClearAllStmUpgrades()
		{
			Db.Connection.ExecuteNonQuery($@"
UPDATE {StmUpgradeSchema.Constants.SqlSchemaName}.{StmUpgradeSchema.Constants.TableName} SET
	{StmUpgradeSchema.Constants.SZ_Status} = 'APL',
	{StmUpgradeSchema.Constants.SZ_SystemLastEditTimeUtc} = GetUtcDate(),
	{StmUpgradeSchema.Constants.SZ_SystemLastEditUser} = '~BP'
WHERE
	{StmUpgradeSchema.Constants.SZ_Status} = @status
", cmd => cmd.AddParameterBasedOnDbColumn("@status", "CUR", StmUpgradeSchema.SZ_Status));

			Db.Connection.ExecuteNonQuery($"DELETE {StmUpgradeSchema.Constants.SqlSchemaName}.{StmUpgradeSchema.Constants.TableName}");
		}

		void SetCurrentStmUpgrade(Version version, DateTime exeDate)
		{
			ValidateSmallDateTime(exeDate);

			Db.Connection.ExecuteNonQuery($@"
UPDATE {StmUpgradeSchema.Constants.SqlSchemaName}.{StmUpgradeSchema.Constants.TableName} SET
	{StmUpgradeSchema.Constants.SZ_Status} = 'RDY',
	{StmUpgradeSchema.Constants.SZ_SystemLastEditTimeUtc} = GetUtcDate(),
	{StmUpgradeSchema.Constants.SZ_SystemLastEditUser} = '~BP'
WHERE
	{StmUpgradeSchema.Constants.SZ_Status} = @status
", cmd => cmd.AddParameterBasedOnDbColumn("@status", "CUR", StmUpgradeSchema.SZ_Status));
			Db.Connection.ExecuteNonQuery($@"
INSERT INTO {StmUpgradeSchema.Constants.SqlSchemaName}.{StmUpgradeSchema.Constants.TableName}
(
	{StmUpgradeSchema.Constants.PK},
	{StmUpgradeSchema.Constants.SZ_MajorVersion},
	{StmUpgradeSchema.Constants.SZ_MinorVersion},
	{StmUpgradeSchema.Constants.SZ_Release},
	{StmUpgradeSchema.Constants.SZ_Patch},
	{StmUpgradeSchema.Constants.SZ_Status},
	{StmUpgradeSchema.Constants.SZ_ExeVersionDate},
	{StmUpgradeSchema.Constants.SZ_SystemCreateTimeUtc},
	{StmUpgradeSchema.Constants.SZ_SystemCreateUser},
	{StmUpgradeSchema.Constants.SZ_SystemLastEditTimeUtc},
	{StmUpgradeSchema.Constants.SZ_SystemLastEditUser}
)
VALUES
(
	NEWID(),
	@MajorVersion,
	@MinorVersion,
	@Release,
	@Patch,
	'CUR',
	@ExeDate,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)", cmd =>
			{
				cmd.AddParameter("MajorVersion", SqlDbType.Int, version.Major);
				cmd.AddParameter("MinorVersion", SqlDbType.Int, version.Minor);
				cmd.AddParameter("Release", SqlDbType.Int, version.Build);
				cmd.AddParameter("Patch", SqlDbType.Int, version.Revision);
				cmd.AddParameter("ExeDate", SqlDbType.DateTime, exeDate);
			});
		}

		DateTime GetRegistryIndexLastEditTimeUtc()
		{
			return (DateTime)TestConnection.ExecuteScalar($@"
SELECT
	{StmDataSchema.Constants.SD_SystemLastEditTimeUtc}
FROM
	{StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}
WHERE
	{StmDataSchema.Constants.SD_Name} = @name",
	cmd => cmd.AddParameterBasedOnDbColumn("@name", nameof(SystemDataRegistry.RegistryIndex), StmDataSchema.SD_Name));
		}

		void SetRegistryIndexLastEditTimeUtc(DateTime lastEditTimeUtc)
		{
			ValidateSmallDateTime(lastEditTimeUtc);

			var rowsAffected = TestConnection.ExecuteNonQuery($@"
UPDATE {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} SET
	{StmDataSchema.Constants.SD_SystemLastEditTimeUtc} = @dt,
	{StmDataSchema.Constants.SD_SystemLastEditUser} = '~BP'
WHERE
	{StmDataSchema.Constants.SD_Name} = @name", cmd =>
			{
				cmd.AddParameter("@dt", SqlDbType.SmallDateTime, lastEditTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@name", nameof(SystemDataRegistry.RegistryIndex), StmDataSchema.SD_Name);
			});

			AssertEquals("1 Row should be updated", 1, rowsAffected);
		}

		void ValidateSmallDateTime(DateTime dt)
		{
			if (dt.Second != 0 || dt.Millisecond != 0)
			{
				throw new InvalidOperationException("Avoid seconds and milliseconds in smalldatetime tests.");
			}
		}

		void AssertXmlEquals(string message, string expectedStr, string actualStr)
		{
			var expected = XElement.Parse(expectedStr);
			var actual = XElement.Parse(actualStr);

			if (XNode.DeepEquals(expected, actual))
			{
				Assert(true);
			}
			else
			{
				AssertEquals(message, expected.ToString(), actual.ToString());
			}
		}

		string GetStoredIndexContent()
		{
			return SystemDataRegistry.Instance.RegistryIndex.Value;
		}

		void SetStoredIndexContent(string content)
		{
			SystemDataRegistry.Instance.RegistryIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, content);
		}

		void AssertIndexHierarchy(Dictionary<string, object> expectedHierarchy, IRegistry registry, object categoryKey = null)
		{
			IEnumerable<RegistryCategoryRef> categories;
			if (categoryKey != null)
			{
				if (!expectedHierarchy.TryGetValue(ItemsTestKey, out var itemsObj))
				{
					itemsObj = Array.Empty<string>();
				}

				expectedHierarchy.Remove(ItemsTestKey);
				var content = registry.GetSortedContent(categoryKey);

				categories = content.Categories;
				AssertContainsExactElementsInExactOrder(((string[])itemsObj), content.Items.Select(i => i.Name));
			}
			else
			{
				categories = registry.GetSortedTopLevelCategories();
			}

			foreach (var category in categories)
			{
				if (!expectedHierarchy.TryGetValue(category.DisplayText, out var expected))
				{
					Fail($"Category '{category.DisplayText}' was not expected");
				}

				expectedHierarchy.Remove(category.DisplayText);
				AssertIndexHierarchy((Dictionary<string, object>)expected, registry, category.Key);
			}

			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), expectedHierarchy.Keys);
		}

		const string ItemsTestKey = "ITEMS";

		Mock<IRegistryItemSetLocator> setLocatorMock;
		Mock<IRegistryRelatedFilesLocator> relatedFilesLocatorMock;
		Mock<IRegistryItemVisibility> visibilityMock;

		class TestString : MultilingualString
		{
			public TestString(string unresolved, string translation = null)
			{
				this.unresolved = unresolved ?? throw new ArgumentNullException(nameof(unresolved));
				Translation = translation;
			}

			public override string ToString(string language)
			{
				return Translation ?? unresolved;
			}

			public override string GetUnresolvedString()
			{
				return unresolved;
			}

			public override string ToString() => ToString("En");

			public string Translation { get; set; }
			readonly string unresolved;
		}

		class TestRegistryItemSet : RegistryItemSet
		{
			public override bool IsForProductivityWise => false;

			MultilingualString Text(string text, string translated = null)
			{
				return new TestString(text, translated);
			}

			protected MultilingualString Category(params MultilingualString[] text)
			{
				return CombineCategories(text);
			}

			protected MultilingualString Category(params string[] text)
			{
				return CombineCategories(text.Select(t => Text(t)).ToArray());
			}

			protected MultilingualString Caption(string text)
			{
				return Text(text);
			}

			protected MultilingualString Hint(string text)
			{
				return Text(text);
			}
		}

		class Set1 : TestRegistryItemSet
		{
			public TestString Cat1 { get; }
			public TestString Cat11 { get; }

			public Set1()
			{
				Cat1 = new TestString("cat-1");
				Cat11 = new TestString("cat-1-1");

				Item1 = new IntRegistryItem("Item1", Category(Cat1, Cat11), Caption("Item 1"), Hint("Item 1 hint"), RegistryStorageFlags.All);
				Item2 = new IntRegistryItem("Item2", Category("cat-1", "cat-1-2"), Caption("Item 2"), Hint("Item 2 hint"), RegistryStorageFlags.All);
			}

			public IntRegistryItem Item1 { get; private set; }

			public IntRegistryItem Item2 { get; }
		}

		class Set2 : TestRegistryItemSet
		{
			public Set2()
			{
				Item3 = new IntRegistryItem("Item3", Category("cat-1", "cat-1-1"), Caption("Item 3"), Hint("Item 3 hint"), RegistryStorageFlags.All);
			}

			public IntRegistryItem Item3 { get; }
		}

		class Set3 : TestRegistryItemSet
		{
			public Set3()
			{
				Item4 = new IntRegistryItem("Item4", Category("cat-2", "cat-2-1", "cat-2-1-1", "cat-2-1-1-1"), Caption("Item 4"), Hint("Item 4 hint"), RegistryStorageFlags.All);
				Item5 = new IntRegistryItem("Item5", Category("cat-2", "cat-2-1"), Caption("Item 5"), Hint("Item 5 hint"), RegistryStorageFlags.All);
			}

			public IntRegistryItem Item4 { get; }
			public IntRegistryItem Item5 { get; }
		}

		class Set4 : TestRegistryItemSet
		{
			public Set4()
			{
				Item6 = new IntRegistryItem("Item6", Category("cat-Z", "cat-ZZ"), Caption("Item 6"), Hint("Item 6 hint"), RegistryStorageFlags.All);
				Item7 = new IntRegistryItem("Item7", Category("cat-Z", "cat-ZZ"), Caption("Item 7"), Hint("Item 7 hint"), RegistryStorageFlags.All);
				Item8 = new IntRegistryItem("Item8", Category("cat-Z"), Caption("Item 8"), Hint("Item 8 hint"), RegistryStorageFlags.All);
			}

			public IntRegistryItem Item6 { get; }
			public IntRegistryItem Item7 { get; }
			public IntRegistryItem Item8 { get; }
		}

		class DynamicSet : TestRegistryItemSet
		{
			public void Add(string name, string[] category)
			{
				items.Add(new IntRegistryItem(name, Category(category), Caption($"{name} caption"), Hint($"{name} hint"), RegistryStorageFlags.All));
			}

			protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
			{
				return items;
			}

			readonly List<IRegistryItem> items = new List<IRegistryItem>();
		}

		const string Version = "CargoWise-N/A-EXE20230804010200";
		string GetIndexContentForTest(string version) => $@"
<Registry Version=""{version}"">
	<Index>
		<SetTypes Count=""2"">
			<SetType Index=""0"" Type=""{typeof(Set1).AssemblyQualifiedName}"" />
			<SetType Index=""1"" Type=""{typeof(Set2).AssemblyQualifiedName}"" />
		</SetTypes>
		<Categories>
			<Category Name=""T0.Item1[0]"">
				<Category Name=""T0.Item1[1]"">
					<Items>
						<ItemSet Type=""0"">
							<Property>{nameof(Set1.Item1)}</Property>
						</ItemSet>
						<ItemSet Type=""1"">
							<Property>{nameof(Set2.Item3)}</Property>
						</ItemSet>
					</Items>
				</Category>
				<Category Name=""T0.Item2[1]"">
					<Items>
						<ItemSet Type=""0"">
							<Property>{nameof(Set1.Item2)}</Property>
						</ItemSet>
					</Items>
				</Category>
			</Category>
		</Categories>
	</Index>
</Registry>";
	}
}
