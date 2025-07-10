using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowTempOrgRequiredField))]
	sealed class GlowTempOrgRequiredFieldTest : CodeDescriptionBoolTest
	{
		public void TestBoolReadOnly()
		{
			var glowTempOrgRequiredField = new GlowTempOrgRequiredField();
			glowTempOrgRequiredField.CodeMaxLength = 10;

			glowTempOrgRequiredField.Code = "Name";
			AssertEquals("BoolInfo.ReadOnly", true, glowTempOrgRequiredField.BoolInfo.ReadOnly);

			glowTempOrgRequiredField.Code = "AAA";
			AssertEquals("BoolInfo.ReadOnly", false, glowTempOrgRequiredField.BoolInfo.ReadOnly);
		}

		public void TestIsMandatoryReadOnly()
		{
			var glowTempOrgRequiredField = new GlowTempOrgRequiredField();
			glowTempOrgRequiredField.CodeMaxLength = 10;

			glowTempOrgRequiredField.Bool = true;
			glowTempOrgRequiredField.Code = "Name";
			AssertEquals("IsMandatoryInfo.ReadOnly", true, glowTempOrgRequiredField.IsMandatoryInfo.ReadOnly);

			glowTempOrgRequiredField.Bool = false;
			AssertEquals("IsMandatoryInfo.ReadOnly", true, glowTempOrgRequiredField.IsMandatoryInfo.ReadOnly);

			glowTempOrgRequiredField.Bool = true;
			glowTempOrgRequiredField.Code = "Address2";
			AssertEquals("IsMandatoryInfo.ReadOnly", true, glowTempOrgRequiredField.IsMandatoryInfo.ReadOnly);

			glowTempOrgRequiredField.Bool = false;
			AssertEquals("IsMandatoryInfo.ReadOnly", true, glowTempOrgRequiredField.IsMandatoryInfo.ReadOnly);

			glowTempOrgRequiredField.Bool = true;
			glowTempOrgRequiredField.Code = "AAA";
			AssertEquals("IsMandatoryInfo.ReadOnly", false, glowTempOrgRequiredField.IsMandatoryInfo.ReadOnly);

			glowTempOrgRequiredField.Bool = false;
			AssertEquals("IsMandatoryInfo.ReadOnly", true, glowTempOrgRequiredField.IsMandatoryInfo.ReadOnly);
		}

		public void TestMandatoryIsAutoUnticked()
		{
			var glowTempOrgRequiredField = new GlowTempOrgRequiredField();
			glowTempOrgRequiredField.Code = "AAA";
			glowTempOrgRequiredField.Bool = true;
			glowTempOrgRequiredField.IsMandatory = true;

			AssertEquals("Precondition", true, glowTempOrgRequiredField.IsMandatory);

			glowTempOrgRequiredField.Bool = false;
			AssertEquals("IsMandatory", false, glowTempOrgRequiredField.IsMandatory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new GlowTempOrgRequiredField
			{
				Code = "AAA",
				EnglishDescription = "AAA Description",
				Bool = true,
				IsMandatory = true
			};
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var glowTempOrgRequiredField = (GlowTempOrgRequiredField)clone;
			AssertEquals("Code", "AAA", glowTempOrgRequiredField.Code);
			AssertEquals("Description", "AAA Description", glowTempOrgRequiredField.Description);
			AssertEquals("Bool", true, glowTempOrgRequiredField.Bool);
			AssertEquals("IsClosedStatus", true, glowTempOrgRequiredField.IsMandatory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlowTempOrgRequiredField();
		}
	}
}
