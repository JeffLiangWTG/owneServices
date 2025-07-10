using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(GenAddOnTextNumericFilter))]
	sealed class GenAddOnTextNumericFilterTest : ModuleTextFilterTest
	{
		public void TestAsycudaPackLinePriceFilter()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_ClusterKey = 1;
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			pack1.LinePrice = 2.34;
			bill1.DutyAmount = 2.0;
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_ClusterKey = 2;
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.LinePrice = 8.0;
			bill2.DutyAmount = 5.0;
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "2.34";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);
			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == bill1.PK));
			AssertEquals(false, headers.Any(x => x.PK == bill2.PK));
			pack1.LinePrice = 2.00;
			Factory.Save();
			filterObj = new ASYCUDAManifestBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "2";
			dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);
			headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == bill1.PK));
			AssertEquals(false, headers.Any(x => x.PK == bill2.PK));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				pack1.LinePrice = 2.34;
				Factory.Save();
				filterObj = new ASYCUDAManifestBillFilterStrip();
				filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
				filter.IsActive = true;
				filter.Property = "2,34";
				dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				dbOnlyQuery.AddToFilter(filterObj.Filter);
				headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
				AssertEquals(true, headers.Any());
				AssertEquals(true, headers.Any(x => x.PK == bill1.PK));
				AssertEquals(false, headers.Any(x => x.PK == bill2.PK));
			}
		}

		public void TestDecimalPlaces()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			pack1.LinePrice = 6;
			Factory.Save();
			var filterObj = new ASYCUDAManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "6";
			AssertContains("'6'", filterObj.Filter.LiteralTextSqlFormatted);
			AssertContains("'6.0'", filterObj.Filter.LiteralTextSqlFormatted);
			AssertContains("'6.00'", filterObj.Filter.LiteralTextSqlFormatted);
			filterObj = new ASYCUDAManifestBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "6.5";
			AssertContains("'6.5'", filterObj.Filter.LiteralTextSqlFormatted);
			AssertContains("'6.50'", filterObj.Filter.LiteralTextSqlFormatted);
			filterObj = new ASYCUDAManifestBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "6.50";
			AssertContains("'6.5'", filterObj.Filter.LiteralTextSqlFormatted);
			AssertContains("'6.50'", filterObj.Filter.LiteralTextSqlFormatted);
			filterObj = new ASYCUDAManifestBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "6.05";
			AssertContains("'6.05'", filterObj.Filter.LiteralTextSqlFormatted);
			filterObj = new ASYCUDAManifestBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "6.0";
			AssertContains("'6'", filterObj.Filter.LiteralTextSqlFormatted);
			AssertContains("'6.0'", filterObj.Filter.LiteralTextSqlFormatted);
			AssertContains("'6.00'", filterObj.Filter.LiteralTextSqlFormatted);
			filterObj = new ASYCUDAManifestBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[ASYCUDAManifestBillFilterStrip.FilterConstants.PackLinePrice];
			filter.IsActive = true;
			filter.Property = "6.23";
			AssertContains("'6.23'", filterObj.Filter.LiteralTextSqlFormatted);
		}

		public void TestLinePriceTextFilterValidationErrors()
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			pack1.LinePrice = 2.0;
			bill1.DutyAmount = 2.0;
			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.LinePrice = 8.0;
			bill2.DutyAmount = 5.0;
			Factory.Save();
			var messageFilterBusinessObject = new ASYCUDAManifestBillFilterStrip();
			var textFilter = (GenAddOnTextNumericFilter)messageFilterBusinessObject["Pack Line Price"];
			messageFilterBusinessObject.FilterStrips.AddNew("Pack Line Price");
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = GenAddOnTextNumericFilter.ComparisonConstants.Exact;
			textFilter.Property = "2.123";
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "This field can only contain up to 2 decimal places.");
			textFilter.Property = "string";
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "This field requires a numerical value to be entered.");
			textFilter.Property = "2.1";
			textFilter.Validation.ValidateAll();
			AssertNoErrors(textFilter.PropertyInfo);
		}

		protected override ModuleTextFilter GetNewModuleFilter() => new GenAddOnTextNumericFilter("Pack Line Price", new ASYCUDAManifestBillFilterStrip(), "LinePrice", AsycudaPackSchema.Constants.Prefix, typeof(AsycudaPack), null, 2);

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override ZString ExpectedDescription => "Pack Line Price";

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
		}
	}
}
