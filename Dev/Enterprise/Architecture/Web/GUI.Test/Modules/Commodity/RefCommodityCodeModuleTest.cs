using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefCommodityCodeModule))]
	sealed class RefCommodityCodeModuleTest : ZFilterGridModuleTest
	{
		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefCommodityCode>();
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefCommodityCodeSchema.RH_Description, SQLComparisonOperator.StartsWith, "TEST");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var commodity = result as RefCommodityCode;
			if (commodity != null)
			{
				commodity.RH_Description = string.Format("TEST{0}", DateTime.Now.Ticks);
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefCommodityCode testObject = Factory.NewWithValidTestData<RefCommodityCode>();
				testObject.RH_Description = "Included" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefCommodityCode testObject = Factory.NewWithValidTestData<RefCommodityCode>();
				testObject.RH_Description = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RefCommodityCodeSchema.RH_Description, SQLComparisonOperator.StartsWith, "Included");
		}

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefCommodityCode; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefCommodityCodeSchema.RH_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
