using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	/// <summary>
	/// This is to be used when a message is to be attached to CusEntryHeader and 'messagetype' is an original message which allows amendment.
	/// </summary>
	/// <typeparam name="TDataProvider"></typeparam>
	public abstract class CusEntryHeaderOriginalMessageSender<TDataProvider> : MessageSender<CusEntryHeader, TDataProvider>
		where TDataProvider : IMessageDataProvider
	{
		protected CusEntryHeaderOriginalMessageSender(IEnumerable<CusEntryHeader> parents, BusinessObjectFactory factory)
			: base(parents, factory)
		{
			if (parents == null)
			{
				throw new System.ArgumentException("parents must not be null.");
			}

			var declarations = parents.Select(x => x.Declaration).Distinct();
			foreach (var declaration in declarations)
			{
				declaration.SetValidationModeOnElectronicMessaging(MessageType);
			}
		}

		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);
			parent.Declaration.RemoveValidationModeOnElectronicMessaging(MessageType);
		}

		protected override void CreateSnapshotCore(CusEntryHeader parent, TDataProvider messageDataProvider)
		{
			base.CreateSnapshotCore(parent, messageDataProvider);
			var stream = KRXmlObjectSerializer.Serialize(messageDataProvider);
			ReserveStream(stream);
			AccumulativeAmendmentManager.CreateNewSnapshot(parent, MessageType, stream);
		}
	}
}
