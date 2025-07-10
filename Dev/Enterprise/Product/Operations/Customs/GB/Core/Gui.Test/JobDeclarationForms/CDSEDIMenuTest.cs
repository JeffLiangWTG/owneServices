using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	class CDSEDIMenuTest : TestCase
	{
		public void TestSendsMessagesToCustomsType()
		{
			var menu = new CDSEDIMenuForTest();
			AssertType<CDSSendsMessagesToCustomsGUI>(menu.SendsMessagesToCustoms);
		}

		class CDSEDIMenuForTest : CDSEDIMenu
		{
			public new ISendsMessagesToCustoms SendsMessagesToCustoms => base.SendsMessagesToCustoms;
		}
	}
}
