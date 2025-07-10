using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class AdditionalTabPageForTest1 : ITabPage
	{
		public ResourceStringData Caption => NoResourceStringData.GetData("Additional Tab Page For Test #1");

		public ZUserControl CreateUserControl() => new AdditionalTabPageForTest1UserControl();

		public ZString UserControlBindingMember => "Property1";
	}

	class AdditionalTabPageForTest2 : ITabPage
	{
		public ResourceStringData Caption => NoResourceStringData.GetData("Additional Tab Page For Test #2");

		public ZUserControl CreateUserControl() => new AdditionalTabPageForTest2UserControl();

		public ZString UserControlBindingMember => "Property2";
	}

	sealed class AdditionalTabPageForTest1UserControl : ZUserControl
	{
	}

	sealed class AdditionalTabPageForTest2UserControl : ZUserControl
	{
	}
}
