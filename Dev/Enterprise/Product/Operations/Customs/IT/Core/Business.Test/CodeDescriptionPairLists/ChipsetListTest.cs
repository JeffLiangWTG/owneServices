using System;
using CargoWise.Cryptoki.Common.ClientServerApi;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ChipsetListTest : TestCase
{
	public void TestAllElementsExistInClientServerApiChipsetList()
	{
		var chipsetCodes = new ChipsetList().GetAllCodes();

		CombineAssertions(
			"The following Chipsets are not supported by CargoWise.Cryptoki.Common.ClientServerApi. Please add them to the Chipset enum and Pkcs11LibraryLoader class.",
			() =>
			{
				foreach (var chipsetCode in chipsetCodes)
				{
					Assert(chipsetCode, Enum.TryParse<Chipset>(chipsetCode, out _));
				}
			});
	}
}
