using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(AYCTriggerTypeRegistryItem))]
	class AYCTriggerTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<AYCTriggerTypeSettings>
	{
		protected override StronglyTypedRegistryItem<AYCTriggerTypeSettings, AYCTriggerTypeSettings> GetNewRegistryItem()
		{
			return new AYCTriggerTypeRegistryItem("AYCTriggerTypeSettings", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}

		public void TestUpdateRegistry()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var primaryCharge = BillingTestHelper.CreateChargeCode(factory, null, "PRIMARY");
			primaryCharge.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			primaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			var secondaryCharge = BillingTestHelper.CreateChargeCode(factory, null, "SECOND");
			secondaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			secondaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;

			factory.Save();

			var settings = new AYCTriggerTypeSettings()
			{
				PrimaryChargeCode = primaryCharge.AC_Code,
				SecondaryChargeCode = secondaryCharge.AC_Code
			};

			EDIDataRegistry.Instance.AYCTriggerTypeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			using (var cmd = TestConnection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'AYCTriggerTypeSettings'"))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];

					if (bytes == DBNull.Value)
					{
						Fail("Invalid value for AYCTriggerTypeSettings registry");
					}
					else
					{
						var currentValue = System.Text.Encoding.ASCII.GetString((byte[])bytes).Replace("\0", "");
						var expectedValue = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><AYCTriggerTypeSettings><PrimaryChargeCode>PRIMARY</PrimaryChargeCode><SecondaryChargeCode>SECOND</SecondaryChargeCode></AYCTriggerTypeSettings>";
						AssertEquals(expectedValue, currentValue);
					}
				}
				else
				{
					Fail("No entry for AYCTriggerTypeSettings registry");
				}
			}
		}
	}
}
