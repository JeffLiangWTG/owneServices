using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class GblCompanyExportTest : TestCaseWithFactory
	{
		public void TestCompanyIncludesCFXAndJobExRateConfigurations()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			glbCompany.CompanyName = "FROSTY ICE CREAM TRUCK";
			glbCompany.GC_Phone = "123456789";

			var acc1 = glbCompany.AccCFXConfigurations.AddNew();
			acc1.JCF_CFXPercentage = 8;
			acc1.JCF_RX_NKCurrency = "USD";
			acc1.JCF_ServiceDirection = "IMP";
			acc1.JCF_TransportMode = "AIR";

			var acc2 = glbCompany.AccCFXConfigurations.AddNew();
			acc2.JCF_CFXPercentage = 5;
			acc2.JCF_RX_NKCurrency = "USD";
			acc2.JCF_ServiceDirection = "EXP";
			acc2.JCF_TransportMode = "SEA";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = "FROST";
			branch1.GB_Phone = "1231421287";

			var acc3 = branch1.AccCFXConfigurations.AddNew();
			acc3.JCF_CFXPercentage = 8;
			acc3.JCF_RX_NKCurrency = "USD";
			acc3.JCF_ServiceDirection = "IMP";
			acc3.JCF_TransportMode = "AIR";

			var jobExRateConfig = glbCompany.AccExchangeRateConfigurations.AddNew();
			jobExRateConfig.JCE_Preference = "TDR";
			jobExRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = "SEL";
			jobExRateConfig.JCE_ServiceDirection = "IMP";
			jobExRateConfig.JCE_TransportMode = "AIR";

			glbCompany.Branches.Add(branch1);

			Factory.Save();

			string actualMessage = "";
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(glbCompany))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualMessage));

			CombineAssertions(delegate
			{
				AssertEquals("Should have 2 AccCFXUpliftConfigurationViewCollection.", 2, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationViewCollection"));
				AssertEquals("Should have 3 AccCFXUpliftConfigurationView.", 3, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationView"));
				AssertEquals("Should have 1 AccExchangeRateConfigurationViewCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "AccExchangeRateConfigurationViewCollection"));
				AssertEquals("Should have 1 AccExchangeRateConfigurationView.", 1, element.Descendants().Count(e => e.Name.LocalName == "AccExchangeRateConfigurationView"));
			});

			element = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "GlbBranch");
			AssertNotNull(element);

			CombineAssertions(delegate
			{
				AssertEquals("Should have 1 AccCFXUpliftConfigurationViewCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationViewCollection"));
				AssertEquals("Should have 1 AccCFXUpliftConfigurationView.", 1, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationView"));
			});
		}
		public void TestBranchIncludesCFXConfigurations()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();

			glbCompany.CompanyName = "FROSTY ICE CREAM TRUCK";
			glbCompany.GC_Phone = "123456789";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = "FROST";
			branch1.GB_Phone = "1231421287";

			var acc1 = branch1.AccCFXConfigurations.AddNew();
			acc1.JCF_CFXPercentage = 8;
			acc1.JCF_RX_NKCurrency = "USD";
			acc1.JCF_ServiceDirection = "IMP";
			acc1.JCF_TransportMode = "AIR";

			glbCompany.Branches.Add(branch1);

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_BranchName = "CREAMS";
			branch2.GB_Phone = "1221321421";

			var acc2 = branch2.AccCFXConfigurations.AddNew();
			acc2.JCF_CFXPercentage = 5;
			acc2.JCF_RX_NKCurrency = "USD";
			acc2.JCF_ServiceDirection = "EXP";
			acc2.JCF_TransportMode = "SEA";

			glbCompany.Branches.Add(branch2);

			var acc3 = glbCompany.AccCFXConfigurations.AddNew();
			acc3.JCF_CFXPercentage = 3;
			acc3.JCF_RX_NKCurrency = "USD";
			acc3.JCF_ServiceDirection = "IMP";
			acc3.JCF_TransportMode = "AIR";

			Factory.Save();

			string actualMessage = "";
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(glbCompany))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualMessage));

			CombineAssertions(delegate
			{
				AssertEquals("Should have 3 AccCFXUpliftConfigurationViewCollection.", 3, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationViewCollection"));
				AssertEquals("Should have 3 AccCFXUpliftConfigurationView.", 3, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationView"));
			});

			var branchElements = element.Descendants().Where(e => e.Name.LocalName == "GlbBranch");
			AssertEquals("Should export two branches", 2, branchElements.Count());
			foreach (var branchElement in branchElements)
			{
				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 AccCFXUpliftConfigurationViewCollection.", 1, branchElement.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationViewCollection"));
					AssertEquals("Should have 1 AccCFXUpliftConfigurationView.", 1, branchElement.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationView"));
				});
			}
		}
	}
}
