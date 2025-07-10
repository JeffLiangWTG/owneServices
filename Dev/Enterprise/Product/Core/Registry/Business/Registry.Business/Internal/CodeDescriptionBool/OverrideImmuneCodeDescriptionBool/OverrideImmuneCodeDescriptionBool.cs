using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OverrideImmuneCodeDescriptionBool : CodeDescriptionBool
	{
		protected bool BoolReadOnly
		{
			get { return SystemDefined; }
		}

		[ReadOnlyMember(nameof(BoolReadOnly))]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set { base.Bool = value; }
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OverrideImmuneCodeDescriptionBool();
		}

		#endregion
	}
}
