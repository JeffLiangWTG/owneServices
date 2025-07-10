using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebRefServiceLevelFilterBusinessObject))]
	sealed class WebRefServiceLevelFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		#region TestFilterReturnOnlyActiveServiceLevels

		public void TestFilterReturnOnlyActiveServiceLevels()
		{
			TestCaseHelper.ClearTable(RefServiceLevelSchema.Constants.TableName);

			RefServiceLevel serviceLevel1 = CreateServiceLevel(false.ToString());
			RefServiceLevel serviceLevel2 = CreateServiceLevel(false.ToString());
			RefServiceLevel serviceLevel3 = CreateServiceLevel(true.ToString());
			RefServiceLevel serviceLevel4 = CreateServiceLevel(false.ToString());
			RefServiceLevel serviceLevel5 = CreateServiceLevel(true.ToString());

			Factory.Save();

			WebRefServiceLevelFilterBusinessObject serviceLevelFilterBO = (WebRefServiceLevelFilterBusinessObject)GetNewBusinessObject();
			WebServiceLevelCollection collection = new WebServiceLevelCollection(Factory, serviceLevelFilterBO.Filter);

			AssertEquals("Collection should contain 2 service levels", 2, collection.Count);
			Assert("Collection should contain serviceLevel3", collection.Contains(serviceLevel3));
			Assert("Collection should contain serviceLevel5", collection.Contains(serviceLevel5));
		}

		#endregion

		#region TestServiceLevelCodeFilter

		public void TestServiceLevelCodeFilter()
		{
			TestCaseHelper.ClearTable(RefServiceLevelSchema.Constants.TableName);

			RefServiceLevel serviceLevel1 = CreateServiceLevel("ABC");
			RefServiceLevel serviceLevel2 = CreateServiceLevel("BEU");
			RefServiceLevel serviceLevel3 = CreateServiceLevel("BAD");

			Factory.Save();

			WebRefServiceLevelFilterBusinessObject serviceLevelFilterBO = (WebRefServiceLevelFilterBusinessObject)GetNewBusinessObject();
			serviceLevelFilterBO.Code = "AB";
			WebServiceLevelCollection collection = new WebServiceLevelCollection(Factory, serviceLevelFilterBO.Filter);

			AssertEquals("Collection should contain 1 service level", 1, collection.Count);
			Assert("Collection should contain serviceLevel1", collection.Contains(serviceLevel1));

			serviceLevelFilterBO.Code = "B";
			collection = new WebServiceLevelCollection(Factory, serviceLevelFilterBO.Filter);

			AssertEquals("Collection should contain 2 service levels", 2, collection.Count);
			Assert("Collection should contain serviceLevel2", collection.Contains(serviceLevel2));
			Assert("Collection should contain serviceLevel3", collection.Contains(serviceLevel3));
		}

		#endregion

		#region TestServiceLevelDescriptionFilter

		public void TestServiceLevelDescriptionFilter()
		{
			TestCaseHelper.ClearTable(RefServiceLevelSchema.Constants.TableName);

			RefServiceLevel serviceLevel1 = CreateServiceLevel("A", "Super level");
			RefServiceLevel serviceLevel2 = CreateServiceLevel("B", "Good enough level");
			RefServiceLevel serviceLevel3 = CreateServiceLevel("C", "Below bad level");

			Factory.Save();

			WebRefServiceLevelFilterBusinessObject serviceLevelFilterBO = (WebRefServiceLevelFilterBusinessObject)GetNewBusinessObject();
			serviceLevelFilterBO.Description = "Good";
			WebServiceLevelCollection collection = new WebServiceLevelCollection(Factory, serviceLevelFilterBO.Filter);

			AssertEquals("Collection should contain 1 service level", 1, collection.Count);
			Assert("Collection should contain serviceLevel2", collection.Contains(serviceLevel2));

			serviceLevelFilterBO.Description = "level";
			collection = new WebServiceLevelCollection(Factory, serviceLevelFilterBO.Filter);

			AssertEquals("Collection should contain 3 service levels", 3, collection.Count);
			Assert("Collection should contain serviceLevel1", collection.Contains(serviceLevel1));
			Assert("Collection should contain serviceLevel2", collection.Contains(serviceLevel2));
			Assert("Collection should contain serviceLevel3", collection.Contains(serviceLevel3));
		}

		#endregion

		#region Implementation

		RefServiceLevel CreateServiceLevel(params string[] paramsArray)
		{
			RefServiceLevel serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			if (paramsArray.Length > 0)
			{
				bool isActive = false;
				if (bool.TryParse(paramsArray[0], out isActive))
				{
					serviceLevel.RS_IsActive = isActive;
				}
				else
				{
					serviceLevel.RS_Code = paramsArray[0];
					if (paramsArray.Length > 1)
					{
						serviceLevel.RS_Description = paramsArray[1];
					}
				}
			}

			return serviceLevel;
		}

		#endregion
	}
}
