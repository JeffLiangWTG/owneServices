using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new AmountOrPercentageBasedThreeLevelAuthorisationRequirement this[int x]
		{
			get { return (AmountOrPercentageBasedThreeLevelAuthorisationRequirement)base[x]; }
		}

		public new AmountOrPercentageBasedThreeLevelAuthorisationRequirement AddNew()
		{
			return (AmountOrPercentageBasedThreeLevelAuthorisationRequirement)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
		}
	}
}
