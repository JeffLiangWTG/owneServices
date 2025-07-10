using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AWSPrivateCA))]
	public class AWSPrivateCATest : RegistryBusinessObjectTemplateTestCase<AWSPrivateCA>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AWSPrivateCA GetBusinessObjectToClone()
		{
			return new AWSPrivateCA();
		}

		protected override AWSPrivateCA GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion

		#region Test Validation

		public void TestIssuingCAValidate()
		{
			var cAArn = new AWSPrivateCA();
			AssertNoErrors(cAArn.IssuingCAInfo);

			cAArn.IsEnabled = true;
			cAArn.ValidateIssuingCA();
			AssertHasError(cAArn.IssuingCAInfo, "Please enter a value.");

			cAArn.IssuingCA = "Test";
			AssertHasError(cAArn.IssuingCAInfo, "Enter a valid selection.");

			cAArn.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			AssertNoErrors(cAArn.IssuingCAInfo);

			var collection = new AWSPrivateCACollection();
			var ca1 = collection.AddNew();
			ca1.IsEnabled = true;
			ca1.AccessKey = "Test";
			ca1.SecretKey = "Test";
			ca1.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			ca1.Arn = "Test";
			var ca2 = collection.AddNew();
			ca2.IsEnabled = true;
			ca2.AccessKey = "Test2";
			ca2.SecretKey = "Test2";
			ca2.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			ca2.Arn = "Test2";
			AssertHasError(ca2.IssuingCAInfo, "The Issuing CA has been duplicated and must be unique.");
		}

		public void TestArnValidate()
		{
			var cAArn = new AWSPrivateCA();
			AssertNoErrors(cAArn.ArnInfo);

			cAArn.IsEnabled = true;
			cAArn.ValidateArn();
			AssertHasError(cAArn.ArnInfo, "Please enter a value.");

			cAArn.Arn = "arn:aws:acm-pca";
			AssertNoErrors(cAArn.ArnInfo);
		}

		public void TestAccessKeyValidate()
		{
			var cAArn = new AWSPrivateCA();
			AssertNoErrors(cAArn.AccessKeyInfo);

			cAArn.IsEnabled = true;
			cAArn.ValidateAccessKey();
			AssertHasError(cAArn.AccessKeyInfo, "Please enter a value.");

			cAArn.AccessKey = "AKIARFHWFJSBXYOXWBXL";
			AssertNoErrors(cAArn.AccessKeyInfo);
		}

		public void TestSecretKeyValidate()
		{
			var cAArn = new AWSPrivateCA();
			AssertNoErrors(cAArn.SecretKeyInfo);

			cAArn.IsEnabled = true;
			cAArn.ValidateSecretKey();
			AssertHasError(cAArn.SecretKeyInfo, "Please enter a value.");

			cAArn.SecretKey = "pik4+se9HK6aoDDfb4nl1z3S1xqfJ+bxTRBCSWbJ";
			AssertNoErrors(cAArn.SecretKeyInfo);
		}

		#endregion
	}
}
