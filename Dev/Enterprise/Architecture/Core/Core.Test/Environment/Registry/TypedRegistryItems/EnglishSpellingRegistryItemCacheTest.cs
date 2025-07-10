using System;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EnglishSpellingRegistryItemCacheTest : TestCase
	{
		public void TestDbHitsForSameValue()
		{
			var mls = (NoResString)"American English Spelling";
			var ri = new EnglishSpellingRegistryItemImpl("Blah", mls, mls, mls, RegistryStorageFlags.All);
			var guid = new Guid("AAFE9959-9A2C-4D19-B09F-3A14CE25FE44");
			var defaultValue = ri.GetDefaultValue(guid, Guid.Empty, Guid.Empty);
			using (Db.Connection.TrackExecutedCommands())
			{
				var defaultValue2 = ri.GetDefaultValue(guid, Guid.Empty, Guid.Empty);
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Db.Connection.ExecutedCommands);
			}
		}
	}
}
