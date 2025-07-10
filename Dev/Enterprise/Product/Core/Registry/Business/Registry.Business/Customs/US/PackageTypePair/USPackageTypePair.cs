using System.Xml.Serialization;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Customs.US
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class USPackageTypePair : PackageTypePair
	{
		protected override PackageTypePair GetInstanceForClone()
		{
			return new USPackageTypePair();
		}

		public override CodeDescriptionPairList CustomsPackageTypesList
		{
			get
			{
				if (customsPackageTypesList == null)
				{
					customsPackageTypesList = (CodeDescriptionPairList)ObjectFactory.Get<Integration.Customs.US.IShippingOrPackingingUnitList>();
				}
				return customsPackageTypesList;
			}
		}
		CodeDescriptionPairList customsPackageTypesList;
	}
}
