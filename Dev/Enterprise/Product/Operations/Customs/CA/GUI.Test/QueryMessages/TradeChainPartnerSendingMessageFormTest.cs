using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(TradeChainPartnerSendingMessageForm))]
	sealed class TradeChainPartnerSendingMessageFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderTCPMessageWrapper.New(org);
			var messageManager = new TradeChainPartnerMessageManager(wrapper);
			return new TradeChainPartnerSendingMessageForm(messageManager);
		}
	}
}
