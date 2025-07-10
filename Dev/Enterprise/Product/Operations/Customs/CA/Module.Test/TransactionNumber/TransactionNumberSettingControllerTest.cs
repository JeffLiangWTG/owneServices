using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(TransactionNumberSettingController))]
	sealed class TransactionNumberSettingControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CATransactionNumberSetting;
	}
}
