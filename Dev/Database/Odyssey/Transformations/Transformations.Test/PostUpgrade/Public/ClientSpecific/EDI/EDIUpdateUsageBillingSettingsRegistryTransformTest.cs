using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.ClientSpecific.EDI
{
	[TestedType(typeof(EDIUpdateUsageBillingSettingsRegistryTransform))]
	public class EDIUpdateUsageBillingSettingsRegistryTransformTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var registryValue = Encoding.Unicode.GetString(RegistryHelper.GetStmDataValue(RegistryName));
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-16""?>
<UsageBillingSettings>
<Products>
<PriceLists>
<ProductCode>WTA</ProductCode>
<RawUsageCategory>WTA</RawUsageCategory>
<PriceListCode>WTA</PriceListCode>
<Description>WiseTech Academy</Description>
</PriceLists>
<PriceLists>
<ProductCode>ABC</ProductCode>
<RawUsageCategory>ABC</RawUsageCategory>
<PriceListCode>DEF</PriceListCode>
<Description>GHI</Description>
</PriceLists>
</Products>
<BranchRestrictions />
</UsageBillingSettings>",
				registryValue.Replace("><", ">\r\n<"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EDIUpdateUsageBillingSettingsRegistryTransform();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiBilledUsage", "CREATE TABLE dbo.EdiBilledUsage (ID INT)");
			RegistryHelper.InsertStmDataRow(RegistryName, "BIN", Encoding.Unicode.GetBytes(xmlData));
		}

		readonly RegistryTransformationHelper RegistryHelper = new RegistryTransformationHelper();
		const string RegistryName = "UsageBillingSettings";

		const string xmlData = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UsageBillingSettings>
<Products>
<PriceLists>
<ProductCode>WTA</ProductCode>
<PriceListCode>WTA</PriceListCode>
<Description>WiseTech Academy</Description>
</PriceLists>
<PriceLists>
<ProductCode>ABC</ProductCode>
<PriceListCode>DEF</PriceListCode>
<Description>GHI</Description>
</PriceLists>
</Products>
<BranchRestrictions />
</UsageBillingSettings>";
	}
}
