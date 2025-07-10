using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrganisationModule))]
	public class OrganisationModule_Test : ZFilterGridModuleTest
	{
		#region Overrides

		protected override bool CanHaveInactiveElements(Type elementType)
		{
			return false;
		}

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);
			filter.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "TEST COMPANY");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			testCompaniesCount++;
			var organisation = result as OrgHeader;
			if (organisation != null)
			{
				organisation.OH_FullName = string.Format("TEST COMPANY {0}", testCompaniesCount);
			}
			return result;
		}

		int testCompaniesCount;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			testCompaniesCount = 0;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
				organisation.OH_FullName = "Include" + i.ToString();
				result.Add(organisation);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
				organisation.OH_FullName = "Other" + i.ToString();
				result.Add(organisation);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		#endregion

		#region Setup

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.Organisation; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			FilterBusinessObjectDefault[] result = new FilterBusinessObjectDefault[1];
			ZString property = OrganisationFilterBusinessObject.Schema.OH_DetailsFilter;
			ZString value = OrgConstants.FilterControl.OrgDetails.Common;
			result[0] = new FilterBusinessObjectDefault(property, value);
			return result;
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(OrgHeaderSchema.OH_Code.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
