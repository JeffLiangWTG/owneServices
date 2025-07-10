using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(OpportunityValueAnalysisDefault))]
	internal sealed class OpportunityValueAnalysisDefaultTest : RegistryBusinessObjectTemplateTestCase<OpportunityValueAnalysisDefault>
	{
		public void TestValidateCode()
		{
			OpportunityValueAnalysisDefaultCollection list = new OpportunityValueAnalysisDefaultCollection();
			OpportunityValueAnalysisDefault item1 = list.AddNew();
			AssertNoNotifications(item1.CodeInfo);
			item1.Code = "";
			AssertHasErrors(item1.CodeInfo);
			item1.Code = "XXX";
			AssertNoNotifications(item1.CodeInfo);
			item1.Code = "AUD";
			AssertNoNotifications(item1.CodeInfo);

			OpportunityValueAnalysisDefault item2 = list.AddNew();
			item2.Code = "AUD";
			AssertHasError(item2.CodeInfo, "The Code has been duplicated and must be unique.");
			item2.Code = "NZD";
			AssertNoNotifications(item2.CodeInfo);
		}

		public void TestValidatePrice()
		{
			OpportunityValueAnalysisDefaultCollection list = new OpportunityValueAnalysisDefaultCollection();
			OpportunityValueAnalysisDefault item = list.AddNew();
			item.ValueInUSD = 0;
			AssertNoNotifications(item.ValueInUSDInfo);
			item.ValueInUSD = -1;
			AssertHasErrors(item.ValueInUSDInfo);
			item.ValueInUSD = 1;
			AssertNoNotifications(item.ValueInUSDInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override OpportunityValueAnalysisDefault GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override OpportunityValueAnalysisDefault GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		OpportunityValueAnalysisDefault NewPopulatedBusinessObject()
		{
			OpportunityValueAnalysisDefault result = new OpportunityValueAnalysisDefault();
			result.Code = "AUD";
			result.ValueInUSD = 0.20m;
			return result;
		}

		#endregion
	}
}
