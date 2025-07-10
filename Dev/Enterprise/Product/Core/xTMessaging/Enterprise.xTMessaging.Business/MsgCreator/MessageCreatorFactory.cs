using System.Collections;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.xTMessaging.Business
{
	public static class MessageCreatorFactory
	{
		public static IEDIMessageCreator GetMessageCreator(string messageCreatorAttribute, EDIInterchange interchange, ILogger logger)
		{
			Argument.NotNull(interchange, nameof(interchange));
			Argument.NotNull(logger, nameof(logger));

			ObjectHandle objectHandle = null;
			var creatorTypes = ObjectFactory.Get<Hashtable>("EDIMessageCreators");

			objectHandle = (creatorTypes[messageCreatorAttribute] ??
				creatorTypes[string.Concat(interchange.EI_ApplicationCode, "_", interchange.EI_InterchangeType)] ??
				creatorTypes[interchange.EI_InterchangeType.ToString()]) as ObjectHandle;

			return objectHandle?.GetObject(interchange, logger) as IEDIMessageCreator;
		}
	}
}
