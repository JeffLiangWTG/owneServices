using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Testing
{
	sealed class DummyConversationProviderWithSpecificHyperlinks : DummyConversationProvider, IConversationParentHyperlinkProvider
	{
		public DummyConversationProviderWithSpecificHyperlinks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public string HyperlinkToUse { get; set; }
		public Func<IConversationParticipant, bool> ShouldUseProviderFunc { get; set; }

		string IConversationParentHyperlinkProvider.GetHyperlinkToConversationParent() => HyperlinkToUse;

		bool IConversationParentHyperlinkProvider.ShouldUseThisProviderForHyperlink(IConversationParticipant participant) => ShouldUseProviderFunc?.Invoke(participant) ?? false;
	}
}
