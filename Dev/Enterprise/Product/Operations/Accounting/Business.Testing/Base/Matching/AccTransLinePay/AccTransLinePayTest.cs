using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(AccTransLinePay))]
	public class AccTransLinePayTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesAccTransLinePay()
		{
			var line = Factory.New<AccTransLinePay>();

			var localList = new List<string>
			{
				nameof(line.A7_Amount)
			};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckLocalCurrency(localList, nameof(line.LocalDecimals));
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestSaveAndDeleteBusinessObject()
		{
			base.TestSaveAndDeleteBusinessObject();
		}
	}
}
