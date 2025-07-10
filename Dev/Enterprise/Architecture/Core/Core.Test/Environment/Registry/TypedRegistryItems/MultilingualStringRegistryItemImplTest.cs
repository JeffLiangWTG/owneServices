using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class MultilingualStringRegistryItemImplTest : TransactionedTestCase
	{
		public void TestCheckValueDataType()
		{
			var registryItem = new MultilingualStringRegistryItemImpl("", (NoResString)"", (NoResString)"", (NoResString)"", new StringRegistryDataType(),
				new TextRegistryEditorInfo(TextEditorType.TextBox),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				"Text");

			AssertNoExceptionThrown(() => registryItem.CheckValueDataType("zoo bar"));
			AssertNoExceptionThrown(() => registryItem.CheckValueDataType(ResString.GetMultilingualString("0", "zoo bar")));

			AssertExceptionThrown(typeof(ArgumentException), () => registryItem.CheckValueDataType(new ZString("zoo bar")));
			AssertExceptionThrown(typeof(ArgumentException), () => registryItem.CheckValueDataType(true));
		}
	}
}
