using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefUNLOCOModule))]
	sealed class RefUNLOCOModuleTest : ZFilterGridModuleTest
	{
		[ExpectNoExceptions]
		public void TestLoadCollectionWithOrderBy()
		{
			using (var filterGridModule = ZWebModuleFactory.Create(TestID, new BusinessObjectFactory(), TestPage))
			{
				SetupForCollectionLoadDBHitsWithDBOnlyQueryTests();
				AssertNotNull("Module has a valid grid collection", filterGridModule.GridCollection);

				var filter = new ZDBOnlyQuery(GetCollectionElementType())
				{
					OrderBy = "RL_Code"
				};
				SetupDBOnlyQuery(filter);
				Factory.Save();
				filterGridModule.LoadCollection(filterGridModule.CreateNewFilterBusinessObject(), filter);
			}
		}

		protected override bool CanHaveInactiveElements(Type elementType)
		{
			return false;
		}

		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefUNLOCO>();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(typeof(RefUNLOCO), isCancelled);
			var location = result as RefUNLOCO;
			if (location != null)
			{
				location.RL_PortName = string.Format("TST{0}", DateTime.Now.Ticks);
			}
			return result;
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.StartsWith, "TST");
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefUNLOCO testObject = Factory.NewWithValidTestData<RefUNLOCO>();
				testObject.RL_PortName = "Included" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefUNLOCO testObject = Factory.NewWithValidTestData<RefUNLOCO>();
				testObject.RL_PortName = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.StartsWith, "Included");
		}

		#endregion

		#region Setup

		[StressTest]
		public override void TestAllBindablePropertiesHavePropertyInfo()
		{
			base.TestAllBindablePropertiesHavePropertyInfo();
			base.TestAllGridColumnsAreExportableToExcel();
		}

		public override void TestAllGridColumnsAreExportableToExcel()
		{
			Assert("Is being tested inside of 'TestAllBindablePropertiesHavePropertyInfo'", true);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefUNLOCO; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefUNLOCOSchema.RL_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
