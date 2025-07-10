using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocBuilderDataSource : AutoDocBuilderDataSource
	{
		public override ZBool Brokerage
		{
			get { return base.Brokerage; }
			set
			{
				base.Brokerage = value;

				if (Brokerage)
				{
					Freight = false;
				}
			}
		}

		public override ZBool Freight
		{
			get { return base.Freight; }
			set
			{
				base.Freight = value;

				if (Freight)
				{
					Brokerage = false;
				}
			}
		}
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocBuilderDataSource();
		}
	}
}
