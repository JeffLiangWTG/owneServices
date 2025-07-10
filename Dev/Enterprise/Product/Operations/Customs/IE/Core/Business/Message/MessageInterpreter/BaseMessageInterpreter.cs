using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public abstract class BaseMessageInterpreter<TDataProvider>
	{
		protected BaseMessageInterpreter(BaseEDIMessage message, TDataProvider provider)
		{
			this.message = Argument.NotNull(message, nameof(message));
			this.provider = Argument.NotNull(provider, nameof(provider));
		}
		protected readonly BaseEDIMessage message;
		protected readonly TDataProvider provider;
		protected BusinessObjectFactory factory => message.Factory;

		protected ZDate MessageCreatedDate
		{
			get
			{
				if (!messageCreatedDate.HasValue)
				{
					messageCreatedDate = message.EM_SystemCreateTimeUtc.Date;
					if (messageCreatedDate.Value.IsEmpty)
					{
						messageCreatedDate = ZDate.Today;
					}
				}
				return messageCreatedDate.Value;
			}
		}
		ZDate? messageCreatedDate;

		protected string GetDescriptionFromCode(ZString code, ZString codeType, string country = Core.Constants.CountryCodes.Ireland) => MessageInterpreterHelper.GetDescriptionFromCode(factory, code, codeType, MessageCreatedDate, country);

		protected string GetCodeAndDescription(ZString code, ZString codeType, string dataGrouping = Core.Constants.CountryCodes.Ireland) => MessageInterpreterHelper.GetCodeAndDescription(factory, code, codeType, MessageCreatedDate, dataGrouping);

		protected string GetCodeAndDescription(ZString code, ICodeDescriptionPairList list) => MessageInterpreterHelper.GetCodeAndDescription(code, list);
	}
}
