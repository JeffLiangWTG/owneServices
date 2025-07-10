using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CountryTierPriceCodeMappingCollection : RegistryBusinessObjectCollectionTemplate<CountryTierPriceCodeMapping>
	{
		public CountryTierPriceCodeMappingCollection() : this(null, null)
		{
		}

		public CountryTierPriceCodeMappingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryTierPriceCodeMapping(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CountryTierPriceCodeMappingCollection(fallbackLevel, factory);
		}

		protected override bool RunPreSaveValidationCore()
		{
			if (!base.RunPreSaveValidationCore())
			{
				return false;
			}

			var elements = this.Cast<CountryTierPriceCodeMapping>().ToArray();
			elements.ForEach(x => x.RemoveRowError("Duplicate Price Code + System combinations are not permitted."));

			var duplicateMappingValues = elements.GroupBy(x => x.PriceCode + x.SystemCode).Where(g => g.Count() > 1);

			foreach (var grouping in duplicateMappingValues)
			{
				foreach (var mapping in grouping)
				{
					mapping.AddRowError("Duplicate Price Code + System combinations are not permitted.");
				}
			}

			return true;
		}
	}
}
