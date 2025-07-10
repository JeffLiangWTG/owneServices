using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrgContactModule))]
	sealed class OrgContactModuleModuleTest : ZFilterGridModuleTest
	{
		public void TestSelectionColumnIsBoundToContactName()
		{
			var selectionColumn = FilterGridModule.SelectionColumn as ZButtonColumn;

			AssertNotNull(selectionColumn);
			AssertEquals("ZFindButtonForAutoComplete control expects that the selection column is Contact Name", OrgContactSchema.OC_ContactName.Name, selectionColumn.DataTextField);
		}

		#region Overrides

		protected override void SetupForExcelExport()
		{
			base.SetupForExcelExport();

			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = SiteUser.LoggedInOrganisation.PK.ToString();
		}

		protected override bool CanHaveInactiveElements(Type elementType) => false;

		protected override bool AllowActiveStatusFilterTest() => false;

		protected override void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
			base.AddAdditionalFilterForActiveStatusTest(filter);

			filter.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.StartsWith, "EMAIL");
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var contact = (OrgContact)base.GetNewElement(elementType, isCancelled);
			contact.OC_OH = SiteUser.LoggedInOrganisation.PK;
			contact.OC_Email = $"EMAIL{testContactsCount++}";

			return contact;
		}

		int testContactsCount;

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();

			testContactsCount = 0;
			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = SiteUser.LoggedInOrganisation.PK.ToString();
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = $"Include{i}";
				result.Add(contact);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = $"Other{i}";
				result.Add(contact);
			}

			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		#endregion

		#region Setup

		protected override WebModuleID TestID => WebModuleIDs.OrgContact;

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault() => Array.Empty<FilterBusinessObjectDefault>();

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(OrgContactSchema.OC_ContactName.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
