using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrgAddressModule))]
	public class OrgAddressModuleTest : ZFilterGridModuleTest
	{
		public void TestSelectionColumnIsBoundToAddress1()
		{
			var selectionColumn = FilterGridModule.SelectionColumn as ZButtonColumn;

			AssertNotNull(selectionColumn);
			AssertEquals("ZFindButtonForAutoComplete control expects that the selection column is Address 1", OrgAddressSchema.OA_Address1.Name, selectionColumn.DataTextField);
		}

		#region Overrides

		protected override DataGridColumn[] ExpectedDefaultGridColumns => base.ExpectedDefaultGridColumns.Take(4).ToArray();

		protected override bool CanHaveInactiveElements(Type elementType)
		{
			return false;
		}

		protected override bool AllowActiveStatusFilterTest() => false;

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);

			filter.AddToFilter(OrgAddressSchema.OA_Email, SQLComparisonOperator.StartsWith, "EMAIL");
		}

		protected override void SetupForExcelExport()
		{
			base.SetupForExcelExport();

			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = SiteUser.LoggedInOrganisation.PK.ToString();
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var address = (OrgAddress)base.GetNewElement(elementType, isCancelled);
			address.OA_Code = $"T{testAddressCount++}";
			address.OA_Email = $"EMAIL{testAddressCount}";
			address.OA_OH = SiteUser.LoggedInOrganisation.PK;

			return address;
		}
		int testAddressCount;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();

			testAddressCount = 0;
			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = SiteUser.LoggedInOrganisation.PK.ToString();
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_Email = $"Test{i}";
				result.Add(address);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_Email = $"Other{i}";
				result.Add(address);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(OrgAddressSchema.OA_Email, SQLComparisonOperator.StartsWith, "Test");
		}

		#endregion

		#endregion

		#region Setup

		protected override WebModuleID TestID => WebModuleIDs.OrgAddress;

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault() => Array.Empty<FilterBusinessObjectDefault>();

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(OrgAddressSchema.OA_Address1.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
