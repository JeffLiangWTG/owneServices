using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.EMCS.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.EMCS.Business.XmlSerializers")]
	public class EmcsGroupNotification : GroupNotification
	{
		public EmcsGroupNotification()
		{
		}

		public EmcsGroupNotification(ZString sendMode, ZGuid sendGroupPK) : base(sendMode, sendGroupPK)
		{
		}

		public EmcsGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override CodeDescriptionPairList GetSendModeList() => new CodeDescriptionPairList(OLookUpEditType.EmailTo);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new EmcsGroupNotification(fallbackLevel, factory);
	}
}
