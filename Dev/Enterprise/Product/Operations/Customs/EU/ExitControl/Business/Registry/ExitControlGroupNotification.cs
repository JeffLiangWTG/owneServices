using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.Customs.EU.ExitControl.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.ExitControl.Business.XmlSerializers")]
	public sealed class ExitControlGroupNotification : Enterprise.Registry.Business.Customs.GroupNotification
	{
		public ExitControlGroupNotification()
		{
		}
		public ExitControlGroupNotification(ZString sendMode, ZGuid sendGroupPk)
			: base(sendMode, sendGroupPk)
		{
		}
		public ExitControlGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		protected override CodeDescriptionPairList GetSendModeList()
			=> new CodeDescriptionPairList(OLookUpEditType.EmailTo);
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new ExitControlGroupNotification(fallbackLevel, factory);
	}
}
