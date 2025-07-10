using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class MessageVisualObjectCollection : NonPersistentBusinessObjectCollection<MessageVisualObject>
{
	public MessageVisualObjectCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	protected override bool AllowNewCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject() => new MessageVisualObject(new EmptyMessageContentProvider(Factory));

	sealed class EmptyMessageContentProvider : IMessageContentProvider
	{
		public EmptyMessageContentProvider(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; }

		ZString IMessageContentProvider.ProcedureCode => string.Empty;

		byte[] IMessageContentProvider.GetMessageData() => Array.Empty<byte>();
	}
}
