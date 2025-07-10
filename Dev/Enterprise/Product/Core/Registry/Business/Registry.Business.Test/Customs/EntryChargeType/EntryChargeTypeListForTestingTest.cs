using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeListForTesting))]
	sealed class EntryChargeTypeListForTestingTest : EntryChargeTypeListTestCase
	{
		protected override EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new EntryChargeTypeListForTesting();
		}

		protected override ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
