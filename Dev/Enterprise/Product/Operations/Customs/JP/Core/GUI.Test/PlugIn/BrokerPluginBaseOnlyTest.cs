using System;

namespace Enterprise.Customs.JP.GUI.Testing
{
	class BrokerPluginBaseOnlyTest : BrokerPluginAbstractTest<CustomsBrokerageUserControl>
	{
		public void TestBrokerageControlIsCorrectType() => AssertBrokerControlType(CreateBrokerageUserControl);

		void AssertBrokerControlType(Type expected)
		{
			AssertEquals(expected, control.GetType());
		}

		protected override Type CreateBrokerageUserControl => typeof(CustomsBrokerageUserControl);
	}
}
