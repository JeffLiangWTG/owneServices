using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class OrganisationFilterControlTest : TestCaseWithFactory
	{
		public void TestDetailsListIndexChanged()
		{
			using (var page = new PageForTest())
			using (var control = new OrganisationFilterControlForTest())
			{
				page.Controls.Add(control);
				page.OnLoad();
				var filterBizo = (OrganisationFilterBusinessObject)page.DataSource;

				control.DetailsExposed.Bind(filterBizo);
				control.DetailsListExposed.Bind(filterBizo);
				control.OnLoad_ForTest();

				Assert(control.DetailsListExposed.AutoPostBack);

				control.DetailsExposed.Text = "Should be removed";
				filterBizo.OH_DetailsFilter = "None";
				control.DetailsListExposed.RaisePostDataChangedEventInternal();

				AssertEquals(string.Empty, control.DetailsExposed.Text);
			}
		}

		static WebFilterBusinessObjectFactory FilterBusinessObjectFactory
		{
			get
			{
				if (filterBusinessObjectFactory == null)
				{
					var webFactory = new BusinessObjectFactory();
					filterBusinessObjectFactory = new WebFilterBusinessObjectFactory(webFactory);
				}

				return filterBusinessObjectFactory;
			}
		}
		static WebFilterBusinessObjectFactory filterBusinessObjectFactory;

		class PageForTest : ZPage
		{
			protected override BusinessObject GetNewDataSource()
			{
				var orgHeader = FilterBusinessObjectFactory.New<OrganisationFilterBusinessObject>();
				var row = ((IBusinessObjectInternals)orgHeader).Row;

				return new OrganisationFilterBusinessObject(Factory, row);
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		class OrganisationFilterControlForTest : OrganisationFilterControl
		{
			public OrganisationFilterControlForTest()
			{
				Details = new ZTextBox
				{
					BindTo = "OH_Details"
				};
				DetailsList = new ZDropDownList
				{
					BindTo = "OH_DetailsFilter",
					BindToList = "OH_DetailsFilter_List"
				};
			}
			public ZDropDownList DetailsListExposed => DetailsList;
			public ZTextBox DetailsExposed => Details;

			public void OnLoad_ForTest()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}
	}
}
