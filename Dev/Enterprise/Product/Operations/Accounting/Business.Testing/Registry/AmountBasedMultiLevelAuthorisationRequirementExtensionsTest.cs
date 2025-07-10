using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public abstract class AmountBasedMultiLevelAuthorisationRequirementExtensionsTest : TestCaseWithFactory
	{
		public void TestGetAuthorisationRequired()
		{
			#region Data Set Up

			var list = new List<AmountBasedMultiLevelAuthorisationRequirement>();

			var item1 = GetNewElement();
			item1.Range = RangeCodes.UpTo;
			item1.Amount = 10m;
			list.Add(item1);

			var item2 = GetNewElement();
			item2.Range = RangeCodes.UpTo;
			item2.Amount = 20m;
			list.Add(item2);

			var item3 = GetNewElement();
			item3.Range = RangeCodes.Above;
			item3.Amount = 20m;
			list.Add(item3);

			#endregion

			AssertNull("Null for 0", list.GetAuthorisationRequired(decimal.Zero));
			AssertEquals("Item1 for 0.01", item1, list.GetAuthorisationRequired(0.01m));
			AssertEquals("Item1 for 10", item1, list.GetAuthorisationRequired(10m));
			AssertEquals("Item2 for 10.01", item2, list.GetAuthorisationRequired(10.01m));
			AssertEquals("Item2 for 20", item2, list.GetAuthorisationRequired(20m));
			AssertEquals("Item3 for 20.01", item3, list.GetAuthorisationRequired(20.01m));
		}

		protected abstract AmountBasedMultiLevelAuthorisationRequirement GetNewElement();
	}
}