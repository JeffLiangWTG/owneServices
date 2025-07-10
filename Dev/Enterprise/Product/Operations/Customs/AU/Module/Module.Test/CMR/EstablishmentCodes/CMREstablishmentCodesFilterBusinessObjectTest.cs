using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMREstablishmentCodesFilterBusinessObject))]
	sealed class CMREstablishmentCodesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.Code]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.EndDate]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.Name]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.PortCode]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.PremisesIndicator]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.StartDate]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.SubType]);
			AssertNotNull(filter[CMREstablishmentCodesFilterBusinessObject.Schema.Type]);
		}

		public void TestTextFilters()
		{
			var estCode = GetEstablishmentCodes("XXXXX", "YYYY", "ZZZ", "Dummy");
			var estCode1 = GetEstablishmentCodes("XXXX1", "YYY1", "ZZ1", "Dummy1");
			var estCode2 = GetEstablishmentCodes("XXXX2", "YYY2", "ZZ2", "Dummy2");
			var estCode3 = GetEstablishmentCodes("XXXX3", "YYY3", "ZZ3", "Dummy3");
			var estCode4 = GetEstablishmentCodes("XXXX4", "YYY4", "ZZ4", "Dummy4");
			var filter = new CMREstablishmentCodesFilterBusinessObject();
			var textFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.Code] as ModuleTextFilter;
			textFilter.Property = "XXXX1";
			textFilter.IsActive = true;
			var result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode1, result[0]);
			textFilter.IsActive = false;
			textFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.Type] as ModuleTextFilter;
			textFilter.Property = "YYY2";
			textFilter.IsActive = true;
			result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode2, result[0]);
			textFilter.IsActive = false;
			textFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.SubType] as ModuleTextFilter;
			textFilter.Property = "ZZ3";
			textFilter.IsActive = true;
			result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode3, result[0]);
			textFilter.IsActive = false;
			textFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.Name] as ModuleTextFilter;
			textFilter.Property = "Dummy4";
			textFilter.IsActive = true;
			result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode4, result[0]);
			textFilter.IsActive = false;
		}

		public void TestDateFilters()
		{
			var estCode = GetEstablishmentCodes("XXXXX", "YYY~", "ZZZ", "Dummy");
			estCode.EC_EstablishmentStartDate = ZDateTime.Today.AddDays(-5);
			estCode.EC_EstablishmentEndDate = ZDateTime.Today.AddDays(5);
			var estCode1 = GetEstablishmentCodes("XXXX1", "YYY~", "ZZ1", "Dummy1");
			estCode1.EC_EstablishmentStartDate = ZDateTime.Today.AddDays(-10);
			estCode1.EC_EstablishmentEndDate = ZDateTime.Today.AddDays(10);
			var filter = new CMREstablishmentCodesFilterBusinessObject();
			var textFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.Type] as ModuleTextFilter;
			textFilter.Property = "YYY~";
			textFilter.IsActive = true;
			var dateFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.StartDate] as ModuleDateFilter;
			dateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			dateFilter.Property2 = ZDateTime.Today;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			var result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode, result[0]);
			dateFilter.IsActive = false;
			dateFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.EndDate] as ModuleDateFilter;
			dateFilter.Property1 = ZDateTime.Today;
			dateFilter.Property2 = ZDateTime.Today.AddDays(7);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode, result[0]);
			dateFilter.IsActive = false;
		}

		public void TestNkFilters()
		{
			var estCode = GetEstablishmentCodes("XXXXX", "YYYY", "ZZZ", "Dummy");
			estCode.EC_EstablishmentPortCode = "PPPPP";
			var estCode1 = GetEstablishmentCodes("XXXX1", "YYY1", "ZZ1", "Dummy1");
			estCode1.EC_EstablishmentPortCode = "PPPP1";
			var filter = new CMREstablishmentCodesFilterBusinessObject();
			var nkFilter = filter[CMREstablishmentCodesFilterBusinessObject.Schema.PortCode] as ModuleNkFilter;
			nkFilter.Property = "PPPPP";
			nkFilter.IsActive = true;
			var result = Factory.Load<CMREstablishmentCodes>(filter.Filter);
			AssertEquals("1 code", 1, result.Length);
			AssertEquals("1st code", estCode, result[0]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CMREstablishmentCodesFilterBusinessObject();

		CMREstablishmentCodes GetEstablishmentCodes(ZString code, ZString type, ZString subType, ZString name)
		{
			var result = Factory.New<CMREstablishmentCodes>();
			result.EC_EstablishmentCode = code;
			result.EC_EstablishmentType = type;
			result.EC_EstablishmentSubType = subType;
			result.EC_EstablishmentName = name;
			return result;
		}
	}
}
