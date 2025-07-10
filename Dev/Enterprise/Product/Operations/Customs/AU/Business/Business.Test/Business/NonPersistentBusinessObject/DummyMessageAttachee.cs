using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class DummyMessageAttachee : IMessageAttachee
	{
		public DummyMessageAttachee(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}

		public BusinessObjectFactory Factory
		{
			get { return fFactory; }
		}
		readonly BusinessObjectFactory fFactory;

		public ZString UserFriendlyCodeExposed;
		public ZString UserFriendlyCode
		{
			get { return UserFriendlyCodeExposed; }
		}

		public ZBool IsValidToSendForOriginalExposed;
		public ZBool IsValidToSendForAmendExposed;
		public ZBool IsValidToSendForWithdrawalExposed;

		public ZBool IsValidToSendThisMessageType(MessageAttacheeMessageType messageType)
		{
			ZBool result = false;
			if (messageType == MessageAttacheeMessageType.Original)
			{
				result = IsValidToSendForOriginalExposed;
			}
			else if (messageType == MessageAttacheeMessageType.Amend)
			{
				result = IsValidToSendForAmendExposed;
			}
			else if (messageType == MessageAttacheeMessageType.Withdraw)
			{
				result = IsValidToSendForWithdrawalExposed;
			}
			return result;
		}
	}
}
