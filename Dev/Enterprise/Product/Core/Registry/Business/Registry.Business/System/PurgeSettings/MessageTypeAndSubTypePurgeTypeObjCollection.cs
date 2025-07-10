using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class MessageTypeAndSubTypePurgeTypeObjCollection : MessageTypeObjCollection
	{
		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MessageSubTypePurgeTypeObjCollection();
		}

		public MessageTypeAndSubTypePurgeTypeObjCollection Add(ZString type, ZString description, ZString subType, ZString subTypeDescription, ZShort purgeTime, ZGuid purgeTimeUnit)
		{
			Add(new MessageTypeObj()
			{
				MessageType = type,
				MessageTypeDescription = description,
				MessageSubType = subType,
				MessageSubTypeDescription = subTypeDescription,
				PurgeTime = purgeTime,
				PurgeTimeUnit = purgeTimeUnit,
				PurgeType = PurgeTypeList.MessageTypeAndSubType
			});

			return this;
		}
	}
}
