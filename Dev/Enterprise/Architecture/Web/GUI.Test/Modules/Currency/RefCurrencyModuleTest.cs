using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefCurrencyModule))]
	sealed class RefCurrencyModuleTest : ZFilterGridModuleTest
	{
		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefCurrency>();
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefCurrencySchema.RX_Desc, SQLComparisonOperator.StartsWith, "TEST");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			testCurrenciesCount++;
			var currency = result as RefCurrency;
			if (currency != null)
			{
				currency.RX_Code = string.Format("{0}", testCurrenciesCount);
				currency.RX_Desc = string.Format("TEST{0}", testCurrenciesCount);
			}
			return result;
		}

		int testCurrenciesCount;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			testCurrenciesCount = 0;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefCurrency testObject = Factory.NewWithValidTestData<RefCurrency>();
				testObject.RX_Desc = "Included" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefCurrency testObject = Factory.NewWithValidTestData<RefCurrency>();
				testObject.RX_Desc = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RefCurrencySchema.RX_Desc, SQLComparisonOperator.StartsWith, "Included");
		}

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefCurrency; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefCurrencySchema.RX_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
