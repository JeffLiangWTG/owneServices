using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefVesselModule))]
	sealed class RefVesselModuleTest : ZFilterGridModuleTest
	{
		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefVesselSchema.RV_CustomAttrib1, SQLComparisonOperator.StartsWith, "TEST");
		}

		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefVessel>();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			testVesselsCount++;
			var vessel = result as RefVessel;
			if (vessel != null)
			{
				vessel.RV_CustomAttrib1 = string.Format("TEST{0}", testVesselsCount);
			}
			return result;
		}

		int testVesselsCount;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			testVesselsCount = 0;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefVessel testObject = Factory.NewWithValidTestData<RefVessel>();
				testObject.RV_CustomAttrib1 = "Included" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefVessel testObject = Factory.NewWithValidTestData<RefVessel>();
				testObject.RV_CustomAttrib1 = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RefVesselSchema.RV_CustomAttrib1, SQLComparisonOperator.StartsWith, "Included");
		}

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefVessel; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefVesselSchema.RV_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
