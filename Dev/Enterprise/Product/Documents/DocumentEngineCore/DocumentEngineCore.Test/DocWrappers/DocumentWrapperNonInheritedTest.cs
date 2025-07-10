using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocumentWrapperNonInheritedTest : TestCaseWithFactory
	{
		public void TestGetDocDataValue()
		{
			DocDataValueTestWrapper wrapper = new DocDataValueTestWrapper();
			IBODocDataProvider docDataProvider = BODocDataProvider.Get(wrapper);
			wrapper.MyProperty = "HELLO";
			AssertEquals("SAILOR", docDataProvider.GetDocDataValue("SAILOR", ZString.Empty));
			AssertEquals("", docDataProvider.GetDocDataValue("", ZString.Empty));

			AssertEquals("SAILOR", docDataProvider.GetDocDataValue("SAILOR", "{MyProperty}"));
			AssertEquals("HELLO", docDataProvider.GetDocDataValue("", "{MyProperty}"));
		}

		class DocDataValueTestWrapper : DocumentWrapper
		{
			public ZString MyProperty
			{
				get { return fMyProperty; }
				set { fMyProperty = value; }
			}
			ZString fMyProperty;

			protected override ZString GetDocDataValueOnly(ZString docDataIdentifier)
			{
				return docDataIdentifier;
			}
		}
	}
}
