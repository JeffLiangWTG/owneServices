using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUImportClassificationFilterBusinessObject))]
	sealed class AUImportClassificationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTariffFilter()
		{
			importClass.CC_TariffNum = "8504.40.90 80";
			AUTariffModuleFilter tariffFilter = (AUTariffModuleFilter)filterBO["Tariff No"];
			tariffFilter.CC_TariffNum = "8504409080";
			tariffFilter.IsActive = true;
			AssertEquals(tariffFilter.CC_TariffNum, "8504.40.90 80");
			Classification[] found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("8504.40.90 80", found[0].CC_TariffNum);
			tariffFilter.CC_TariffNum = "8504.40.90 80";
			found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("8504.40.90 80", found[0].CC_TariffNum);
			tariffFilter.CC_TariffNum = "8504";
			found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be one record found", 1, found.Length);
			AssertEquals("8504.40.90 80", found[0].CC_TariffNum);
			tariffFilter.CC_TariffNum = "8505";
			found = (Classification[])Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be no records found", 0, found.Length);
		}

		public void TestImportClassificationFilter()
		{
			importClass.CC_LookupCode = "090210ahon";
			ModuleTextFilter lookupFilter = (ModuleTextFilter)filterBO["Lookup Code"];
			lookupFilter.IsActive = true;
			lookupFilter.Property = "hon";
			lookupFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			BusinessObject[] found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("One record", 1, found.Length);
		}

		public void TestInstrumentCodeFilter()
		{
			importClass.CC_AddInfo = "InstrumentCode_Hidden=YYY*InstrumentType_Hidden=TC1*TreatmentCode_Hidden=505";
			ModuleTextFilter instrumentCodeFilter = (ModuleTextFilter)filterBO["Instrument Code"];
			instrumentCodeFilter.IsActive = true;
			instrumentCodeFilter.Property = "YYY";
			BusinessObject[] found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be one record found", 1, found.Length);
			instrumentCodeFilter.Property = "XXX";
			found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There is no record that has instrument code XXX", 0, found.Length);
		}

		public void TestInstrumentTypeFilter()
		{
			importClass.CC_AddInfo = "InstrumentCode_Hidden=YYY*InstrumentType_Hidden=AAA*TreatmentCode_Hidden=505";
			ModuleTextFilter instrumentTypeFilter = (ModuleTextFilter)filterBO["Instrument Type"];
			instrumentTypeFilter.IsActive = true;
			instrumentTypeFilter.Property = "AAA";
			BusinessObject[] found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be one record found", 1, found.Length);
			instrumentTypeFilter.Property = "MMM";
			found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be no record found", 0, found.Length);
		}

		public void TestTreatmentCodeFilter()
		{
			importClass.CC_AddInfo = "InstrumentCode_Hidden=YYY*InstrumentType_Hidden=AAA*TreatmentCode_Hidden=LLL";
			ModuleTextFilter treatmentCodeFilter = (ModuleTextFilter)filterBO["Treatment Code"];
			treatmentCodeFilter.IsActive = true;
			treatmentCodeFilter.Property = "LLL";
			BusinessObject[] found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be one record found", 1, found.Length);
			treatmentCodeFilter.Property = "QQQ";
			found = Factory.Load(typeof(Classification), filterBO.Filter);
			AssertEquals("There should be no record found", 0, found.Length);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AUImportClassificationFilterBusinessObject();

		AUImportClassificationFilterBusinessObject filterBO;
		Classification importClass;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new AUImportClassificationFilterBusinessObject();
			importClass = Factory.New<Classification>();
			importClass.CC_ClassificationType = "IMP";
			CMRCodeLists codeLists = CMRCodeLists.New(filterBO.Factory);
			codeLists.CI_CodeType = CMRCodeLists.CodeTypes.INSTRMTTYP;
			codeLists.CI_Code = "99";
			codeLists.CI_Name = "NewCodeName";
			CMRInstrument instrument = CMRInstrument.New(filterBO.Factory);
			instrument.IN_Number = "1000";
			instrument.IN_Type = "99";
		}
	}
}
