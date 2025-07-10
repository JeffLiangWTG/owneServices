using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	internal sealed class CollectionTypeDeciderTest : TestCaseWithFactory
	{
		public void TestSanity()
		{
			IDictionary<string, IList<string>> result = new SortedDictionary<string, IList<string>>();
			IList<string> errors = new List<string>();
			BusinessObjectCollectionProviderFactory providerFactory = new BusinessObjectCollectionProviderFactory();

			foreach (ITableSchema schema in GetAllTableSchemas())
			{
				string prefix = Schema.GetPrefixFromColumnName(schema.PK.Name);

				if (EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(prefix) == schema)
				{
					Type collectionType = CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(prefix);
					if (collectionType == null)
					{
						// this is acceptable, not all tables should be mapped.
					}
					else if (!typeof(IBusinessObjectCollection).IsAssignableFrom(collectionType))
					{
						errors.Add($"{collectionType} is not a business object collection");
					}
					else
					{
						Type elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(collectionType);
						ITableSchema elementSchema;

						if (elementType.IsInterface)
						{
							elementType = ObjectFactory.GetType(elementType);
						}

						if (elementType == null)
						{
							errors.Add($"unable to determine the element type from {collectionType}.");
						}
						else if (!elementType.IsSubclassOf(typeof(BusinessObject)))
						{
							errors.Add($"{elementType} is not a business object");
						}
						else if ((elementSchema = BusinessObjectFactory.GetTableSchemaFromType(elementType)) == null)
						{
							errors.Add($"unable to determine the schema from {elementType}.");
						}
						else
						{
							if (elementSchema.TableName != schema.TableName)
							{
								errors.Add($"Table name missmatch, should be {schema.TableName} but was {elementSchema.TableName}.");
							}

							if (Attribute.GetCustomAttribute(elementType, typeof(CodePropertyAttribute)) == null)
							{
								errors.Add($"{elementType} does not include a CodeProperty attribute");
							}
						}

						IBusinessObjectCollection collection;

						try
						{
							collection = providerFactory.Get(collectionType)(Factory);
						}
						catch (Exception ex)
						{
							collection = null;
							errors.Add("Exception caught getting collection:\r\n" + ex.ToString());
						}

						if (collection != null)
						{
							ModuleIdentifier identifier = ZMetaData.GetModuleId(collection);

							if (identifier == null || identifier == ModuleIDs.NotAssigned)
							{
								errors.Add($"ModuleID not defined for '{collection.GetType().Name}'");
							}
						}
					}
				}

				if (errors.Count > 0)
				{
					IList<string> oldErrors;
					if (!result.TryGetValue(prefix, out oldErrors))
					{
						result.Add(prefix, errors);
					}
					else
					{
						((List<string>)oldErrors).AddRange(errors.Where(x => !oldErrors.Contains(x)));
					}
					errors = new List<string>();
				}
			}

			AssertGroupedErrorList("These prefixes has issues.", result);
		}

		public void TestGuessCollectionTypeFromTablePrefix_NonExistentPrefix()
		{
			AssertEquals("WW", null, CollectionTypeDecider.GuessCollectionTypeFromTablePrefix("WW"));
		}

		public void TestGuessCollectionTypeFromTablePrefix_PrefixesPreviouslyMappedDirectly()
		{
			CombineAssertions("Collection types should be still guessed for previously mapped prefixes", delegate()
			{
				AssertEquals("OrgHeaderSchema", typeof(OrganisationsFindBoxCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(OrgHeaderSchema.Constants.Prefix));
				AssertEquals("OrgAddressSchema", typeof(OrgAddressCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(OrgAddressSchema.Constants.Prefix));
				AssertEquals("OrgContactSchema", typeof(OrgContactCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(OrgContactSchema.Constants.Prefix));
				AssertEquals("OrgOpportunitySchema", typeof(OrgOpportunityCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(OrgOpportunitySchema.Constants.Prefix));

				AssertEquals("RefAirlineSchema", typeof(RefAirlineCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefAirlineSchema.Constants.Prefix));
				AssertEquals("RefCarrierConsortiumSchema", typeof(RefCarrierConsortiumCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefCarrierConsortiumSchema.Constants.Prefix));
				AssertEquals("RefCityTownSchema", typeof(RefCityTownCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefCityTownSchema.Constants.Prefix));
				AssertEquals("RefCommodityCodeSchema", typeof(RefCommodityCodeCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefCommodityCodeSchema.Constants.Prefix));
				AssertEquals("RefContainerSchema", typeof(RefContainerCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefContainerSchema.Constants.Prefix));
				AssertEquals("RefCurrencySchema", typeof(RefCurrencyCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefCurrencySchema.Constants.Prefix));
				AssertEquals("RefCountrySchema", typeof(RefCountryCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefCountrySchema.Constants.Prefix));
				AssertEquals("RefCountryStatesSchema", typeof(RefCountryStatesCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefCountryStatesSchema.Constants.Prefix));
				AssertEquals("RefDocTypeSchema", typeof(RefDocTypeCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefDocTypeSchema.Constants.Prefix));
				AssertEquals("RefEquipmentSchema", typeof(RefEquipmentCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefEquipmentSchema.Constants.Prefix));
				AssertEquals("RefPackTypeSchema", typeof(RefPackTypeCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefPackTypeSchema.Constants.Prefix));
				AssertEquals("RefServiceLevelSchema", typeof(RefServiceLevelCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefServiceLevelSchema.Constants.Prefix));
				AssertEquals("RefUNLOCOSchema", typeof(RefUNLOCOCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefUNLOCOSchema.Constants.Prefix));
				AssertEquals("RefVesselSchema", typeof(RefVesselCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(RefVesselSchema.Constants.Prefix));

				AssertEquals("GlbBranchSchema", typeof(GlbBranchCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(GlbBranchSchema.Constants.Prefix));
				AssertEquals("GlbDepartmentSchema", typeof(GlbDepartmentCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(GlbDepartmentSchema.Constants.Prefix));
				AssertEquals("GlbStaffSchema", typeof(GlbStaffCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(GlbStaffSchema.Constants.Prefix));
			});
		}

		public void TestGuessCollectionTypeFromTablePrefix_SpecificPrefixes()
		{
			AssertEquals("Precondition: GlbGroupSchema - table name", "GlbGroup", GlbGroupSchema.Constants.TableName);
			AssertEquals("Precondition: GlbGroupSchema - module name", "GlbGroup", ModuleIDs.GlbGroup.ToString());
			AssertEquals("Precondition: GlbGroupSchema - module should be assigned", ModuleIDs.GlbGroup, ZMetaData.GetModuleId(new GlbGroupCollection(Factory)));
			AssertEquals("GlbGroupSchema: Collection type should be guessed when table name (GlbGroup) equals to the module identifier",
				typeof(GlbGroupCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(GlbGroupSchema.Constants.Prefix));

			AssertEquals("Precondition: ProcessTasksSchema - table name", "ProcessTasks", ProcessTasksSchema.Constants.TableName);
			AssertEquals("Precondition: ProcessTasksSchema - module name", "ProcessTasks", ModuleIDs.ProcessTasks.ToString());
			AssertEquals("Precondition: ProcessTasksSchema - module should be assigned", ModuleIDs.ProcessTasks, ZMetaData.GetModuleId(new ProcessTaskCollection(Factory)));
			AssertEquals("ProcessTasksSchema: Collection type should be guessed when table name (ProcessTasks) equals to the module identifier",
				typeof(ProcessTaskCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(ProcessTasksSchema.Constants.Prefix));

			AssertEquals("Precondition: BMSystemSchema - table name", "BMSystem", BMSystemSchema.Constants.TableName);
			AssertEquals("Precondition: BMSystemSchema - module name", "BMSystems", ModuleIDs.BMSystems.ToString());
			AssertEquals(@"BMSystemSchema: Collection type should be guessed even when table name differs from the module identifier if the table name is explicitely mapped to the module
				(the id of the module - BMSystems - has an additional 's' at the end)",
				"BMSystemCollection", CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(BMSystemSchema.Constants.Prefix).Name);

			Assert("Precondition: GlbCapabilitySchema", Schema.GetPrefixFromColumnName(GlbCapabilitySchema.PK.Name) == Schema.GetPrefixFromColumnName(EUAddInfoTaxSchema.PK.Name));
			AssertEquals(@"GlbCapabilitySchema: Collection type should be guessed
				even when there is another schema which share the same prefix if this schema is not the main one
				and if table name (GlbCapability) equals to the module identifier",
				typeof(GlbCapabilityCollection), CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(GlbCapabilitySchema.Constants.Prefix));

			Assert("Precondition: ZZRefCusMapSchema", Schema.GetPrefixFromColumnName(ZZRefCusMapSchema.PK.Name) == Schema.GetPrefixFromColumnName(ZZRefCusMapCombinedSchema.PK.Name));
			AssertEquals(@"ZZRefCusMapSchema: Collection type should NOT be guessed
				when several schemas share the same prefix
				if table name (ZZRefCusMapCombined) differs from the module identifier (ZZRefCusMap)",
				null, CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(ZZRefCusMapSchema.Constants.Prefix));

			AssertEquals("ZZRefCusCodeListCombinedSchema: Collection type should NOT be guessed as the view also shares the same prefix as ZZRefCusCodeList.",
				null, CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(ZZRefCusCodeListCombinedSchema.Constants.Prefix));

			using (var module = CollectionTypeDecider.GetZModuleByColumnNamePrefix(GlbCompanyCampaignItemSchema.Constants.Prefix))
			{
				var collection = ((ZFilterModule)module).GetNewBusinessObjectCollection();
				AssertEquals("Precondition: GlbCompanyCampaignItemSchema", "GlbCompanyCampaignItemCampaignDependentCollection", collection.GetType().Name);
				AssertEquals("Precondition: GlbCompanyCampaignItemSchema", ModuleIDs.NotAssigned, ZMetaData.GetModuleId(collection));

				AssertEquals("GlbCompanyCampaignItemSchema: Collection type should NOT be guessed when the module corresponding to the prefix returns specific collection which is not assigned to any module",
					null, CollectionTypeDecider.GuessCollectionTypeFromTablePrefix(GlbCompanyCampaignItemSchema.Constants.Prefix));
			}
		}

		public void TestGetZModuleByColumnNamePrefix()
		{
			AssertEquals(null, CollectionTypeDecider.GetZModuleByColumnNamePrefix("WW"));
			using (var module = CollectionTypeDecider.GetZModuleByColumnNamePrefix("Z0"))
			{
				AssertEquals(DummyModuleIDs.Dummy.Description, module.Description);
			}
		}

		#region Implementation
		IEnumerable<ITableSchema> GetAllTableSchemas()
		{
			foreach (Type type in typeof(EnterpriseSchema).Assembly.GetExportedTypes())
			{
				if (!typeof(Schema).IsAssignableFrom(type))
				{
					continue;
				}

				if (!typeof(ITableSchema).IsAssignableFrom(type))
				{
					continue;
				}

				if (type.IsAbstract)
				{
					continue;
				}

				FieldInfo info = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
				if (info == null)
				{
					continue;
				}

				ITableSchema schema;
				try
				{
					schema = info.GetValue(null) as ITableSchema;
				}
				catch (TargetInvocationException)
				{
					// Uncomment when schema tables fixed.
					//ErrorReporter.ReportOnce(string.Format("Exception caught trying to access {0}.Instance", type), ex.InnerException);
					continue;
				}

				if (schema == null)
				{
					continue;
				}

				yield return schema;
			}
		}
		#endregion
	}
}
