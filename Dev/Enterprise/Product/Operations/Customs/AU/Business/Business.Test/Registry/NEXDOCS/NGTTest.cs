using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NGT))]
	sealed class NGTTest : RegistryBusinessObjectTemplateTestCase<NGT>
	{
		public void TestValidatePassword()
		{
			var reporter = new NGT
			{
				Password = "123"
			};

			reporter.Password = "";
			AssertHasErrorContaining(reporter.PasswordInfo, MandatoryValidation.MustBeEntered);

			reporter.Password = "12343";
			AssertNoErrorContaining(reporter.PasswordInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestPasswordStatus()
		{
			var reporter = new NGT { };
			AssertEquals("Password does not exist", ZString.Empty, reporter.PasswordStatus);

			reporter = new NGT { Password = "123" };
			AssertEquals("Password exists", Core.Constants.PasswordOK, reporter.PasswordStatus);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
		protected override NGT GetBusinessObjectToClone()
		{
			return new NGT
			{
				Password = "1"
			};
		}

		protected override NGT GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
