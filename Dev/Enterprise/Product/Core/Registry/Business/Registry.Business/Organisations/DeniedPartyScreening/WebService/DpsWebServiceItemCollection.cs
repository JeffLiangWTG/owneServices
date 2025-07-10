using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DpsWebServiceItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DpsWebServiceItemCollection()
			: base()
		{
		}

		public DpsWebServiceItemCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DpsWebServiceItem();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DpsWebServiceItemCollection();
		}

		public new DpsWebServiceItem AddNew()
		{
			return (DpsWebServiceItem)base.AddNew();
		}

		public new DpsWebServiceItem this[int i]
		{
			get { return (DpsWebServiceItem)Elements[i]; }
		}

		public DpsWebServiceItemCollection DefaultValue => new DpsWebServiceItemCollection()
		{
			new DpsWebServiceItem() { Code = "SYD1", WebServiceUrl = (NoResString)"https://dpsv4.wisegrid.net", Role = RoleHelper.Code.Production },
			new DpsWebServiceItem() { Code = "ORD1", WebServiceUrl = (NoResString)"https://dpsv4-usord.wisegrid.net", Role = RoleHelper.Code.ProductionFailover },
			new DpsWebServiceItem() { Code = "STG1", WebServiceUrl = (NoResString)"https://dpsv4-test.wisegrid.net", Role = RoleHelper.Code.Staging },
		};

		protected override bool AllowRemoveCore => Count != 1;
	}
}
