using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldCountry))]
	sealed class StmSystemDefinedFieldCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationType()
		{
			AssertEquals("Validation.GetType()", typeof(StmSystemDefinedFieldCountryValidation), FieldCountry.Validation.GetType());
		}

		#region Implementation

		StmSystemDefinedFieldCountry FieldCountry
		{
			get
			{
				if (fFieldCountry == null)
				{
					fFieldCountry = Factory.New<StmSystemDefinedFieldCountry>();
				}
				return fFieldCountry;
			}
		}

		StmSystemDefinedFieldCountry fFieldCountry;

		#endregion
	}
}
