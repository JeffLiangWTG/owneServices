using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeableWeightRoundingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new ChargeableWeightRounding AddNew()
		{
			return (ChargeableWeightRounding)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeableWeightRoundingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChargeableWeightRounding();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new ChargeableWeightRounding this[int i]
		{
			get { return (ChargeableWeightRounding)Elements[i]; }
		}

		public static ChargeableWeightRoundingCollection GetDefault()
		{
			ChargeableWeightRoundingCollection result = new ChargeableWeightRoundingCollection();
			ChargeableWeightRounding rounding = result.AddNew();
			rounding.RoundingMode = nameof(ChargeableWeightRoundingType.None);
			rounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;
			return result;
		}
	}
}
