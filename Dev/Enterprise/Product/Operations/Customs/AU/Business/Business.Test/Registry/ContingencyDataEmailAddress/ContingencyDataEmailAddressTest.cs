using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ContingencyDataEmailAddress))]
	sealed class ContingencyDataEmailAddressTest : Registry.Business.Testing.RegistryBusinessObjectTestCaseBase
	{
		public void TestManyEmailsEntered()
		{
			BizObj.Description = (NoResString)"tim.van@cargowise.com;leon.ball@cargowise.";
			AssertHasError(BizObj.DescriptionInfo, ContingencyDataEmailAddress.InvalidEmail);

			BizObj.Description = (NoResString)"tim.van@cargowise.com;leon.ball@cargowise.com";
			AssertNoError(BizObj.DescriptionInfo, ContingencyDataEmailAddress.InvalidEmail);
		}

		public void TestMaxCodeAndDescriptionLength()
		{
			AssertEquals("MaxCodeLength", 40, BizObj.CodeMaxLength);
			AssertEquals("MaxDescriptionLength", ContingencyDataEmailAddress.MAX_DESCRIPTION_LENGTH, BizObj.MaxDescriptionLengthInternal);
		}

		public void TestValidateDescription()
		{
			AssertNoErrors("Precondition: Description should not have errors.", BizObj.DescriptionInfo);

			BizObj.Description = (NoResString)"";
			AssertHasError(BizObj.DescriptionInfo, "Please enter an Email Address.");

			BizObj.Description = (NoResString)"123";
			AssertHasError(BizObj.DescriptionInfo, ContingencyDataEmailAddress.InvalidEmail);

			BizObj.Description = (NoResString)"test@test.com";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		#region Implementation

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ContingencyDataEmailAddress result = new ContingencyDataEmailAddress();
			result.Code = "A";
			result.Description = (NoResString)"B";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override string CodeDisplayName
		{
			get { return "Description"; }
		}

		new ContingencyDataEmailAddress BizObj
		{
			get { return (ContingencyDataEmailAddress)base.BizObj; }
		}

		#endregion
	}
}
