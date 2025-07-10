using System;
using System.Reflection;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[TestedType(typeof(JobConsolCostAttrib))]
	public class JobConsolCostAttribTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestEC_Name_AcceptedValues()
		{
			var constants = typeof(JobChargeAttribTypeList.Codes).GetFields(BindingFlags.Public | BindingFlags.Static);

			Assert("Just to verify that it doesn't return 0 due to invalid query above and the test passes", constants.Length > 0);

			foreach (var constant in constants)
			{
				var code = (string)constant.GetValue(null);

				var attribute = Factory.NewWithValidTestData<JobConsolCostAttrib>();
				attribute.E6A_Name = code;

				AssertNoExceptionThrown(FormattableString.Invariant($"A new possible name {code} has been updated. Please update a constraint for E6A_Name so that it accepts the new name."), () => Factory.Save());
			}
		}
	}
}
