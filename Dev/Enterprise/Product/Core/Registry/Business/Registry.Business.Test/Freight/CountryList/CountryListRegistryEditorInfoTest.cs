using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CountryListRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataTypeToBeEdited()
		{
			IRegistryEditorInfo editorInfo = new CountryListRegistryEditorInfo();
			AssertEquals("BaseDataTypeToBeEdited", typeof(Guid[]), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
