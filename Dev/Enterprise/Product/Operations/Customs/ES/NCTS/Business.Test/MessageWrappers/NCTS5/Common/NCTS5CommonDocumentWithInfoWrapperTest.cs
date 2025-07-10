using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonDocumentWithInfoWrapperTest : WrapperHelperTest<NCTS5CommonDocumentWithInfoWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new NCTS5CommonDocumentWithInfoWrapper(null, 0));
		}

		public void TestConstructorWithCodeAndRefAndRef2()
		{
			var wrapper = new NCTS5CommonDocumentWithInfoWrapper("Code", "Reference", "Reference2", 5);
			AssertEquals("Expected filled Name", "Code", wrapper.Name);
			AssertEquals("Expected filled Number", "Reference", wrapper.Number);
			AssertEquals("Expected filled ComplementaryInformation", "Reference2", wrapper.ComplementaryInformation);
			AssertEquals("Expected filled SequenceNumber", "5", wrapper.SequenceNumber);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("Expected filled ComplementaryInformation", "additionalData", wrapper.ComplementaryInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<CusSupportingInfo>();
			document.CSI_ReferenceNumber2 = "additionalData";
			wrapper = new NCTS5CommonDocumentWithInfoWrapper(document, 1);
		}

		CusSupportingInfo document;
		NCTS5CommonDocumentWithInfoWrapper wrapper;

		protected override NCTS5CommonDocumentWithInfoWrapper GetProvider() => wrapper;
	}
}
