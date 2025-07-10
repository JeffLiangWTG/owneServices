using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;

namespace Enterprise.Client.UPE.Business.Asycuda.Testing
{
	public class IShipmentDataProxyHelperTest : TestCaseWithFactory
	{
		public void TestGetCustomCharge()
		{
			var regValue = new DecimalEffectiveDate
			{
				EffectiveDate = ZDateTime.Today.AddDays(-1),
				NewValue = 400
			};
			var declaration = Factory.New<JobDeclaration>();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Snakey";
			template.P0_ProcessType = "BRK";
			var column1 = template.GenCustomColumnDefinitions.AddNew();
			column1.XC_Name = "Code 1";
			column1.XC_Type = AddOnColumnDataType.Codes.String;
			var column2 = template.GenCustomColumnDefinitions.AddNew();
			column2.XC_Name = "Charge 1";
			column2.XC_Type = AddOnColumnDataType.Codes.Decimal;

			var customPropertiesCollection = new UserDefinedPropertyCollection(declaration);
			customPropertiesCollection.Add(new ProcessTaskTemplateMatches(template));
			foreach (var column in customPropertiesCollection)
			{
				if (column.Info.Type == typeof(ZString))
				{
					column.TrySetValue(declaration, new ZString("100"));
				}
				else
				{
					column.TrySetValue(declaration, new ZDecimal(10));
				}
			}

			var proxy = new TradeNetIShipmentDataProxy("DOK", DateTime.UtcNow, declaration);
			var helper = new IShipmentDataProxyHelper(declaration);
			AssertEquals("100", helper.CustomCode1);
			AssertEquals(new ZDecimal(10), helper.CustomCharge1);
			AssertEquals("", helper.CustomCode2);
			AssertEquals(ZDecimal.Zero, helper.CustomCharge2);
			AssertEquals("", helper.CustomCode3);
			AssertEquals(ZDecimal.Zero, helper.CustomCharge3);
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPETestHelper.TaxOrFeeTestSetUp(Factory);
		}
	}
}
