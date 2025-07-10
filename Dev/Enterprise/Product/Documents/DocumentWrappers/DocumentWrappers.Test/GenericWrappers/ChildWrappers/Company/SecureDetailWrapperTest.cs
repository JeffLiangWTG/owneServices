using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(SecureDetailWrapper))]
	sealed class SecureDetailWrapperTest : GenericWrapperTest
	{
		public void TestPublished()
		{
			SecureDetailWrapper wrapper = new SecureDetailWrapper("Published Text", true, Factory);
			AssertEquals("wrapper.Internal", "Published Text", wrapper.Internal);
			AssertEquals("wrapper.Public", "Published Text", wrapper.Public);
			AssertEquals("wrapper.IsPublished", true, wrapper.IsPublished);
		}

		public void TestUnpublished()
		{
			SecureDetailWrapper wrapper = new SecureDetailWrapper("Unpublished Text", false, Factory);
			AssertEquals("wrapper.Internal", "Unpublished Text", wrapper.Internal);
			AssertEquals("wrapper.Public", "", wrapper.Public);
			AssertEquals("wrapper.IsPublished", false, wrapper.IsPublished);
		}

		public override void TestWrapperMappingsEmpty()
		{
			SecureDetailWrapper wrapper = new SecureDetailWrapper("", false, Factory);
			AssertEquals("wrapper.Internal", "", wrapper.Internal);
			AssertEquals("wrapper.Public", "", wrapper.Public);
			AssertEquals("wrapper.IsPublished", false, wrapper.IsPublished);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new SecureDetailWrapper("value", false, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
SecureDetail                                   (Default Field: Public)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Internal                                String
IsPublished                             Bool
Public                                  String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new SecureDetailWrapper("Value", true, Factory);
		}

		#endregion
	}
}
