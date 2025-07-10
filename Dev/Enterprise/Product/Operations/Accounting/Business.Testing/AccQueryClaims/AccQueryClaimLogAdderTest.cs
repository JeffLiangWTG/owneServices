using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	[TestedType(typeof(AccQueryClaimLogAdder))]
	internal class AccQueryClaimLogAdderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLogComment()
		{
			AccQueryClaimLogAdder logAdder = (AccQueryClaimLogAdder)CachedBusinessObject;
			AssertEquals("", logAdder.LogComment);
			AssertEquals("", logAdder.Parent.Details);
			logAdder.LogComment = "I like to log comments";
			AssertEquals("I like to log comments", logAdder.LogComment);
			AssertEquals("", logAdder.Parent.Details);
			logAdder.AddLogToParent();
			AssertContains("I like to log comments", logAdder.Parent.Details);
		}

		public void TestValidation()
		{
			AccQueryClaimLogAdderValidation validation = ((AccQueryClaimLogAdder)CachedBusinessObject).Validation;
			AssertNotNull(validation);
			AssertEquals(CachedBusinessObject, validation.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AccQueryClaim parent = Factory.New<ARAccQueryClaim>();
			return new AccQueryClaimLogAdder(parent);
		}
	}
}
