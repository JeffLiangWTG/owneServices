using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AWBRoundingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new AWBRounding AddNew()
		{
			return (AWBRounding)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWBRoundingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AWBRounding();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new AWBRounding this[int i]
		{
			get { return (AWBRounding)Elements[i]; }
		}

		public AWBRounding this[string key]
		{
			get
			{
				foreach (AWBRounding rounding in this)
				{
					if (rounding.AWBType == key)
					{
						return rounding;
					}
				}

				return null;
			}
		}

		public static AWBRoundingCollection GetDefault()
		{
			AWBRoundingCollection result = new AWBRoundingCollection();

			AWBRounding rounding = result.AddNew();
			rounding.AWBType = AWBRounding.Keys.AgentMaster;
			rounding.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			rounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;

			rounding = result.AddNew();
			rounding.AWBType = AWBRounding.Keys.DirectMaster;
			rounding.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			rounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;

			rounding = result.AddNew();
			rounding.AWBType = AWBRounding.Keys.House;
			rounding.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			rounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;

			rounding = result.AddNew();
			rounding.AWBType = AWBRounding.Keys.MasterHouse;
			rounding.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			rounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;

			return result;
		}
	}
}
