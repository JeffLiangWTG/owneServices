using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContactSalutation))]
	sealed class ContactSalutationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestSalutationFieldMaxLength()
		{
			var contact = new ContactSalutation(Factory);
			AssertEquals(50, contact.SalutationInfo.MaxLength);
		}

		public new void TestClone()
		{
			ContactSalutation businessObjectToClone = (ContactSalutation)GetBusinessObjectToClone();
			ContactSalutation clone = (ContactSalutation)businessObjectToClone.Clone(businessObjectToClone.CurrentFallbackLevel, businessObjectToClone.Factory);

			Assert("Clone should be a different instance.", businessObjectToClone != clone);
			AssertEquals("CurrentFallbackLevel", businessObjectToClone.CurrentFallbackLevel, clone.CurrentFallbackLevel);
			AssertEquals("CurrentFactory", businessObjectToClone.Factory, clone.Factory);

			AssertEquals(businessObjectToClone.Salutation, clone.Salutation);
			AssertEquals(businessObjectToClone.Gender, clone.Gender);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			FallbackLevel currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			ContactSalutation result = new ContactSalutation(currentFallbackLevel, Factory);
			result.Salutation = (NoResString)"Dear";
			result.Gender = "Male";
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ContactSalutation result = new ContactSalutation();
			result.Salutation = (NoResString)"Dear";
			result.Gender = "Male";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
