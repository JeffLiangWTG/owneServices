using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.UI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class BusinessObjectDataSourceTest : TestCaseWithFactory
	{
		public void TestBusinessObjectDataSource()
		{
			BusinessObjectDataSource dataSource = new BusinessObjectDataSource();
			using (dataSource)
			{
				dataSource.Page = new TestZPage();
				dataSource.BindTo = "States";
				RefCountryStatesDependentCollection collection = (RefCountryStatesDependentCollection)dataSource.Select();
				AssertEquals(8, collection.Count);
			}
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "TypeName has to be of type Enterprise.ZArchitecture.Web.Business.BusinessObjectDataSourceProvider")]
		public void TestBusinessObjectDataSource_NotUsingBusinessObjectDataSourceProvider()
		{
			using (BusinessObjectDataSource dataSource = new BusinessObjectDataSource())
			{
				dataSource.Page = new TestZPage();
				dataSource.TypeName = typeof(int).FullName;
				dataSource.SelectMethod = "ToString";
				dataSource.Select();
			}
		}

		class TestZPage : ZPage
		{
			public TestZPage()
			{
				LoadOrCreateDataSource();
			}

			protected override BusinessObject GetNewDataSource()
			{
				return RefCountry.LoadFromCountryCode(Factory, "AU");
			}
		}
	}
}
