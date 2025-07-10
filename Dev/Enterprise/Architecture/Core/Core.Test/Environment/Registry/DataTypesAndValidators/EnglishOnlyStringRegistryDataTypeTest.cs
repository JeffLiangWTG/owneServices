using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(EnglishOnlyStringRegistryDataType))]
	sealed class EnglishOnlyStringRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		[ExpectNoExceptions]
		public void TestEnglishValdation()
		{
			StringRegistryItem registryItem = new StringRegistryItem("dummy", null, (NoResString)"Dummy", null, RegistryStorageFlags.System);
			GetNewDataType().Validate(registryItem, "English", Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestNonEnglishValdation()
		{
			StringRegistryItem registryItem = new StringRegistryItem("dummy", null, (NoResString)"Dummy", null, RegistryStorageFlags.System);
			AssertExceptionThrown(
				typeof(RegistryValidationException), "Registry item Dummy only accepts Western European languages characters.",
				() => GetNewDataType().Validate(registryItem, "简体中文", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override object[] GetInvalidSamples()
		{
			List<object> samples =
				new List<object>(base.GetInvalidSamples())
				{
					"Русский",
					"简体中文"
				};
			return samples.ToArray();
		}

		protected override StringRegistryDataType GetNewDataType()
		{
			return new EnglishOnlyStringRegistryDataType(0, 10);
		}
	}
}
