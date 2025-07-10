using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.Business.XmlSerializers")]
	public class ExportGroupNotification : GroupNotification
	{
		public ExportGroupNotification()
		{
		}

		public ExportGroupNotification(ZString sendMode, ZGuid sendGroupPK) : base(sendMode, sendGroupPK)
		{
		}

		public ExportGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override CodeDescriptionPairList GetSendModeList() => new CodeDescriptionPairList(OLookUpEditType.EmailTo);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new ExportGroupNotification(fallbackLevel, factory);
	}
}
