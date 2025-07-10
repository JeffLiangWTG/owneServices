using System;
using Enterprise.Client.HEN.GUI;
using Enterprise.Customs.AU.Declaration.GUI;
using NUnit.Framework;

namespace Enterprise.Client.ZClientHEN.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInitialiseAndUnitialise()
		{
			ClientOverride.Instance.Uninitialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("Menu should be unintialised; normal action data menu item", typeof(EDIMenu), menu.GetType());
			}

			ClientOverride.Instance.Initialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("Menu should be initialised; return HEN action data menu item", typeof(HENMenu), menu.GetType());
			}

			ClientOverride.Instance.Uninitialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("Menu should be unintialised; should be back to normal action data menu item", typeof(EDIMenu), menu.GetType());
			}
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
