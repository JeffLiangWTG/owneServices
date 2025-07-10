using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class NotificationRolesSearchControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var request = HttpContext.Current.Request;
			var isReadOnly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			isReadOnly.SetValue(request.Params, false, null);
			request.Params.Add("$$$__LASTFOCUSID", "$$$_FindButton");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			using (var page = new BasePage())
			using (var searchControl = new NotificationRolesSearchControlForTest())
			{
				var flags = new List<bool>();
				searchControl.ModuleID = WebModuleIDs.CargoWiseEDINotificationRolesContacts;
				searchControl.Page = page;
				searchControl.FilterStripBizO = new NotificationRolesDataSource(Factory, org.PK);
				AssertEquals(true, searchControl.IsResultsRelatedOperation);
				searchControl.OnIsResultsRelatedOperationChanged += (_, __) =>
				{
					flags.Add(searchControl.IsResultsRelatedOperation);
				};
				searchControl.OnLoadForTest();
				var columnProvider = ((ZFilterStripGridModule)searchControl.Module).ColumnProvider;
				var cols = string.Join("\r\n", columnProvider.AllColumns.Select(x => x.HeaderText));
				AssertEquals(@"Contact Name
Email
Company Name
UNLOCO
CSV
A/R
BOR
ERA
IST
CCP", cols);
				AssertNotNull(searchControl.BulkUpdateButton);
				AssertEquals(2, flags.Count);
				AssertEquals(false, flags[0]);
				AssertEquals(true, flags[1]);
			}
		}

		class NotificationRolesSearchControlForTest : NotificationRolesSearchControl
		{
			public void OnLoadForTest()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}
	}
}