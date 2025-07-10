using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(QualityIterationAssignmentCollection))]
	public class QualityIterationAssignmentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<QualityIterationAssignmentCollection>
	{
		public void TestIsQiEnabledForReleaseGroup()
		{
			var collection = new QualityIterationAssignmentCollection();
			collection.AddNew("AAA", ZBool.True);
			collection.AddNew("BBB", ZBool.False);

			Assert("IsQiEnabledForReleaseGroup did not return the correct value for the release group 'AAA'.", collection.IsQiEnabledForReleaseGroup("AAA"));
			Assert("IsQiEnabledForReleaseGroup did not return the correct value for the release group 'BBB'.", !collection.IsQiEnabledForReleaseGroup("BBB"));

			AssertExceptionThrown<ArgumentException>("The specified release group doesn't exist, so an ArgumentException should have been thrown.", () =>
			{
				collection.IsQiEnabledForReleaseGroup("Panda Parade");
			});
		}

		public void TestIsReleaseGroupSpecified()
		{
			var collection = new QualityIterationAssignmentCollection();
			collection.AddNew("AAA", ZBool.True);

			Assert("The release group was added so the method should return true.", collection.IsReleaseGroupSpecified("AAA"));
			Assert("The release group was not added so the method should return false.", !collection.IsReleaseGroupSpecified("BBB"));
		}

		public void TestAddDuplicateReleaseGroup_ShouldThrowException()
		{
			var collection = new QualityIterationAssignmentCollection();
			collection.AddNew("AAA", ZBool.True);

			AssertExceptionThrown<InvalidOperationException>("The release group was added twice, so an exception should be thrown.", () =>
			{
				collection.AddNew("AAA", ZBool.False);
			});
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override QualityIterationAssignmentCollection GetCollectionToTest()
		{
			return new QualityIterationAssignmentCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new QualityIterationAssignment(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
