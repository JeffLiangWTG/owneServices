using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(RefServiceLevelModule))]
	sealed class RefServiceLevelModuleTest : ZFilterGridModuleTest
	{
		protected override void BeforeCollectionLoadForActiveStatusTest()
		{
			base.BeforeCollectionLoadForActiveStatusTest();
			SiteUser.LoggedInOrganisation.OrgServiceLevels.RefreshFromDb();
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(RefServiceLevelSchema.RS_Description, SQLComparisonOperator.StartsWith, "TEST");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			testServiceLevelsCount++;
			var serviceLevel = result as RefServiceLevel;
			if (serviceLevel != null)
			{
				serviceLevel.RS_Code = string.Format("{0}", testServiceLevelsCount);
				serviceLevel.RS_Description = string.Format("TEST{0}", testServiceLevelsCount);
				var orgServiceLevel = Factory.New<OrgServiceLevel>();
				orgServiceLevel.PM_RS = serviceLevel.PK;
				orgServiceLevel.PM_OH = SiteUser.LoggedInOrganisation.PK;
				orgServiceLevel.PM_IsPublished = true;
				orgServiceLevel.PM_RS_NKSrvLvl = serviceLevel.RS_Code;
			}
			return result;
		}

		int testServiceLevelsCount;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			testServiceLevelsCount = 0;
		}

		protected override System.Collections.IList GetNewFilterGridCollection()
		{
			return new List<RefServiceLevel>();
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.RefServiceLevel; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RefServiceLevelSchema.RS_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
