using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class PurgeTypeList : CodeDescriptionPairList
	{
		public static CodeDescriptionPair ApplicationCode
		{
			get { return new CodeDescriptionPair("ApplicationCode", (NoResString)"Application Code"); }
		}

		public static CodeDescriptionPair MessageType
		{
			get { return new CodeDescriptionPair("MessageType", (NoResString)"Message Type"); }
		}

		public static CodeDescriptionPair MessageSubType
		{
			get { return new CodeDescriptionPair("MessageSubType", (NoResString)"Message Sub Type"); }
		}

		public static CodeDescriptionPair MessageTypeAndSubType
		{
			get { return new CodeDescriptionPair("MessageTypeAndSubType", (NoResString)"Message Type and Sub Type"); }
		}

		public PurgeTypeList()
		{
			Add(ApplicationCode);
			Add(MessageType);
			Add(MessageSubType);
			Add(MessageTypeAndSubType);
		}

		public static ICodeDescription GetPurgeType(string description)
		{
			return new PurgeTypeList().ToArray().FirstOrDefault(x => x.Description == description);
		}
	}
}
