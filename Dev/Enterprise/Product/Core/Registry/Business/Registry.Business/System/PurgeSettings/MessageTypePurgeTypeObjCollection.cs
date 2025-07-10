using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class MessageTypePurgeTypeObjCollection : MessageTypeObjCollection
	{
		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MessageTypePurgeTypeObjCollection();
		}

		public MessageTypePurgeTypeObjCollection Add(ZString type, ZString description, ZShort purgeTime, ZGuid purgeTimeUnit)
		{
			Add(new MessageTypeObj()
			{
				MessageType = type,
				MessageTypeDescription = description,
				PurgeTime = purgeTime,
				PurgeTimeUnit = purgeTimeUnit,
				PurgeType = PurgeTypeList.MessageType
			});

			return this;
		}
	}
}
