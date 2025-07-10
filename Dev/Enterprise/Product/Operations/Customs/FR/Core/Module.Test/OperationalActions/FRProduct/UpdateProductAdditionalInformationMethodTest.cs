using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(UpdateProductAdditionalInformationMethod))]
	public class UpdateProductAdditionalInformationMethodTest : OperationalActionMethodTest<UpdateProductAdditionalInformationMethod>
	{
		protected override UpdateProductAdditionalInformationMethod NewMethod()
		{
			return new UpdateProductAdditionalInformationMethod();
		}

		public void TestGetFilterRequirements()
		{
			var filterRequirements = Method.GetFilterRequirements();

			AssertContainsExactElementsInAnyOrder(
				"Method should meet constraints of french jurisdiction and Delta I/E enabled for imports or exports.",
				new List<string>
				{
					$"{new FilterIsUnderFrenchCustomsJurisdictionConstraint().Name}:Y",
					$"{new FilterIsDeltaIEEnabledForImportsOrExportsConstraint().Name}:Y"
				},
				filterRequirements.Select(s => $"{s.ConstraintName}:{string.Join(",", s.Select(v => v))}"));
		}

		public void TestNameAndDescription()
		{
			AssertEquals("Update Product Additional Information", Method.Name);
			AssertEquals("Update Product Additional Information", Method.Description);
		}

		public void TestTypeOfGuiControl()
		{
			AssertEquals(true, Method.HasControl);
			using (var control = Method.NewGuiControl())
			{
				AssertType("Control should be of type UpdateProductAdditionalInformationApplicatorControl.", typeof(UpdateProductAdditionalInformationApplicatorControl), control);
			}
		}
	}
}
