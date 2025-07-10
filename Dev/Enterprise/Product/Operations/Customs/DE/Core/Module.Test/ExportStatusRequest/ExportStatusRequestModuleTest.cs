using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(ExportStatusRequestModule))]
	public class ExportStatusRequestModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.ExportStatusRequest;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		public override void TestModuleShowsAndCanSearch()
		{
			var headerNCTS = Factory.NewWithValidTestData<StatusRequest>();
			headerNCTS.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			headerNCTS.EM_MessageType = Messaging.EDIMessageTypeList.Codes.NCTS;
			headerNCTS.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			headerNCTS.EM_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();

			var headerAES = Factory.NewWithValidTestData<StatusRequest>();
			headerAES.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			headerAES.EM_MessageType = Messaging.EDIMessageTypeList.Codes.AES;
			headerAES.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			headerAES.EM_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		public void TestBoundCollection()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(StatusRequestCollection), module.GetNewBusinessObjectCollection().GetType());
			}
		}

		public void TestAttributes()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				CombineAssertions(() =>
				{
					AssertEquals("AllowDelete", false, module.AllowDelete);
					AssertEquals("AllowEdit", false, module.AllowEdit);
				});
			}
		}
	}
}

