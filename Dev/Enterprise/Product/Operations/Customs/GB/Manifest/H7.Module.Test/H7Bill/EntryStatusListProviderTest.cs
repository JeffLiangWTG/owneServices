using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Module.Testing
{
	sealed class EntryStatusListProviderTest : TestCaseWithFactory
	{
		public void TestCustomsStatusList()
		{
			var provider = new EntryStatusListProvider();
			var list = (CodeDescriptionPairList)provider.EntryStatusList(Factory, "GB", "IMPORT");
			var expectedList = new List<(string, string)>()
			{
				("ACC", "Declaration has been legally accepted"),
				("RCV", "Message has been registered"),
				("CTL", "Declaration is subject to physical control"),
				("DOC", "Declaration is subject to physical control"),
				("TAX", "Duties and taxes have been calculated and are due"),
				("CLR", "Declaration is now cleared"),
				("CAN", "Declaration has been canceled"),
			};

			AssertCodeDescriptionPairList(list, expectedList.ToArray());
		}
	}
}
