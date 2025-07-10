using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingSystemChargeCodeMapping : AutoBillingSystemChargeCodeMapping
	{
		#region Properties

		[List("Lookups.ProductList")]
		public override ZString ProductCode
		{
			get { return base.ProductCode; }
			set { base.ProductCode = value; }
		}

		[List("Lookups.SystemList")]
		public override ZString SystemCode
		{
			get { return base.SystemCode; }
			set { base.SystemCode = value; }
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingSystemChargeCodeMapping();
		}

		#endregion

		#region Lists

		public CodeLookups Lookups => new CodeLookups(this);

		public class CodeLookups
		{
			public CodeLookups(BillingSystemChargeCodeMapping map)
			{
				mapping = map;
			}
			readonly BillingSystemChargeCodeMapping mapping;

			public ReadOnlyCodeDescriptionPairList ProductList
			{
				get { return new ProductTypes(true); }
			}

			public ReadOnlyCodeDescriptionPairList SystemList
			{
				get
				{
					if (mapping.ProductCode == ProductTypes.Codes.Enterprise)
					{
						var result = BillingConstants.GetBillingSystemList();
						result.AddPairsIfNotExist(EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.GetPriceListCodeDescriptionPairList().OfType<ICodeDescription>());
						return result;
					}
					else
					{
						return new ReadOnlyCodeDescriptionPairList();
					}
				}
			}
		}

		#endregion

		#region Validation

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
		}

		public override void ValidateSubModule()
		{
			base.ValidateSubModule();

			var allSubModulMappings = this.ParentCollections
				.Cast<BillingSystemChargeCodeMappingCollection>()
				.SelectMany(x => x.GetSubModuleMappings(ProductCode, SystemCode));

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(SubModuleInfo, allSubModulMappings);
		}

		#endregion
	}
}

