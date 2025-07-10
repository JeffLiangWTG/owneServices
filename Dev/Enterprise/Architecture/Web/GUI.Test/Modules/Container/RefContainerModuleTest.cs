using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefContainerModule))]
	sealed class RefContainerModuleTest : ZFilterGridModuleTest
	{
		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefContainer>();
		}

		protected override bool CanHaveInactiveElements(Type elementType)
		{
			return false;
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefContainerSchema.RC_Description, SQLComparisonOperator.StartsWith, "TEST");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var container = result as RefContainer;
			if (container != null)
			{
				container.RC_Description = string.Format("TEST{0}", DateTime.Now.Ticks);
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefContainer testObject = Factory.NewWithValidTestData<RefContainer>();
				testObject.RC_Description = "Included" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				RefContainer testObject = Factory.NewWithValidTestData<RefContainer>();
				testObject.RC_Description = "Other" + i.ToString();
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RefContainerSchema.RC_Description, SQLComparisonOperator.StartsWith, "Included");
		}

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefContainer; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			FilterBusinessObjectDefault[] result = Array.Empty<FilterBusinessObjectDefault>();
			return result;
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefContainerSchema.RC_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
