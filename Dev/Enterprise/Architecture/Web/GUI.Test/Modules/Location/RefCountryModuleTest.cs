using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefCountryModule))]
	sealed class RefCountryModuleTest : ZFilterGridModuleTest
	{
		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefCountry>();
		}

		protected override bool CanHaveInactiveElements(Type elementType)
		{
			return false;
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefCountrySchema.RN_Desc, SQLComparisonOperator.StartsWith, "TEST");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			testCountriesCounts++;
			var country = result as RefCountry;
			if (country != null)
			{
				country.RN_Code = string.Format("{0:00}", testCountriesCounts);
				country.RN_Desc = string.Format("TEST{0}", testCountriesCounts);
			}
			return result;
		}

		int testCountriesCounts;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			testCountriesCounts = 0;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefCountry testObject = Factory.NewWithValidTestData<RefCountry>();
				testObject.RN_Code = "I" + i.ToString();
				testObject.RN_Desc = "Included" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefCountry testObject = Factory.NewWithValidTestData<RefCountry>();
				testObject.RN_Code = "O" + i.ToString();
				testObject.RN_Desc = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RefCountrySchema.RN_Desc, SQLComparisonOperator.StartsWith, "Included");
		}

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefCountry; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefCountrySchema.RN_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
