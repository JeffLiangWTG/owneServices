using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.BR.Business.XmlSerializers")]
	public sealed class UniqueNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public UniqueNumberCustomisation() : this(true)
		{
		}

		public UniqueNumberCustomisation(bool supportDirection)
		{
			Categories = NumberCustomisationElementCategories.Standard;
			SupportsDirection = supportDirection;
		}

		public bool SupportsDirection
		{
			get => supportsDirection;
			set
			{
				supportsDirection = value;

				if (UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction] is BillOfLadingNumberCustomisationElement direction)
				{
					if (supportsDirection)
					{
						Elements.Add(direction);
					}
					else
					{
						Elements.Remove(direction);
					}
				}
			}
		}
		bool supportsDirection;

		[ReadOnly(true)]
		public override ZString CheckDigitAlgorithm { get => base.CheckDigitAlgorithm; set => base.CheckDigitAlgorithm = value; }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UniqueNumberCustomisation(SupportsDirection);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var billOfLadingNumberCustomisation = (UniqueNumberCustomisation)clone;
			billOfLadingNumberCustomisation.SupportsDirection = supportsDirection;
		}
	}
}
