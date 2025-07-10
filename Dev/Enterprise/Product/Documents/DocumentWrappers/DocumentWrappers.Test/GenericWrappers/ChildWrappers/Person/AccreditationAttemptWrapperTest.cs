using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Recruiter.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AccreditationAttemptWrapper))]
	sealed class AccreditationAttemptWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			Assert(true);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting => "Registry : (No Default Field Value Available on Registry)";

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			return new AccreditationAttemptWrapper(attempt, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
AccreditationAttempt
======================================================================
Name                                    Type
----------------------------------------------------------------------
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			return new AccreditationAttemptWrapper(attempt, Factory);
		}

		#endregion
	}
}
