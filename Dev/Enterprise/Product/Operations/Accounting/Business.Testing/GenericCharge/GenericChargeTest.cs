using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericCharge
{
	[TestedType(typeof(GenericCharge))]
	public class GenericChargeTest : BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObject result = Factory.New(GetExpectedBusinessObjectType());
			return result;
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesGenericCharge()
		{
			var charge = Factory.New<GenericCharge>();

			var percentageList = new List<string>
			{
				nameof(charge.VC_Percentage)
			};

			var tester = new DecimalPlacesAttributeTester(charge);
			tester.CheckConstant(percentageList, nameof(charge.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}
	}
}
