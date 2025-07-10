using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class MessageSubTypePurgeTypeObjCollection : MessageTypeObjCollection
	{
		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MessageSubTypePurgeTypeObjCollection();
		}

		public MessageSubTypePurgeTypeObjCollection Add(ZString subType, ZString description, ZShort purgeTime, ZGuid purgeTimeUnit)
		{
			Add(new MessageTypeObj()
			{
				MessageSubType = subType,
				MessageSubTypeDescription = description,
				PurgeTime = purgeTime,
				PurgeTimeUnit = purgeTimeUnit,
				PurgeType = PurgeTypeList.MessageSubType
			});

			return this;
		}
	}
}
