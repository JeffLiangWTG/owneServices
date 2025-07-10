using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageMinimumFee : AutoUsageMinimumFee
	{
		public UsageMinimumFee()
		{
		}

		public UsageMinimumFee(BusinessObjectFactory factory)
			: base(factory) { }

		public UsageMinimumFee(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public UsageMinimumFee(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new UsageMinimumFee(fallbackLevel, factory);
			return clone;
		}

		public static UsageMinimumFee GetDefaultValue()
		{
			return new UsageMinimumFee();
		}

		[List("Lookups.ProductList")]
		public override ZString ProductCode { get => base.ProductCode; set => base.ProductCode = value; }

		[List("Lookups.PriceListCodeList")]
		public override ZString PriceListCode { get => base.PriceListCode; set => base.PriceListCode = value; }

		#region Validations

		public override void ValidateMinimumFeeCode()
		{
			base.ValidateMinimumFeeCode();
			MandatoryValidation.CheckEntered(MinimumFeeCodeInfo);
		}

		public override void ValidatePriceListCode()
		{
			base.ValidatePriceListCode();
			MandatoryValidation.CheckEntered(PriceListCodeInfo);
			ListValidation.ErrorIfInvalidCode(PriceListCodeInfo);
			if (!PriceListCode.IsEmpty)
			{
				if (ParentCollections.Any(x => x.OfType<UsageMinimumFee>().Any(y => y.PK != PK && y.PriceListCode == PriceListCode)))
				{
					PriceListCodeInfo.AddError("This is not allowed. No duplicate product/Pricelist.");
				}
			}
		}

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(ProductCodeInfo);
			if (!ProductCode.IsEmpty)
			{
				if (ParentCollections.Any(x => x.OfType<UsageMinimumFee>().Any(y => y.PK != PK && y.ProductCode == ProductCode)))
				{
					ProductCodeInfo.AddError("This is not allowed. No duplicate product/Pricelist.");
				}
			}
		}

		#endregion

		#region Lookups

		public CodeLookups Lookups
		{
			get => new CodeLookups(this);
		}

		public class CodeLookups
		{
			public CodeLookups(UsageMinimumFee parent)
			{
				this.parent = parent;
			}

			readonly UsageMinimumFee parent;

			public ReadOnlyCodeDescriptionPairList ProductList
			{
				get
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					foreach (var item in EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.OfType<UsageBillingPriceList>().GroupBy(x => x.ProductCode).Select(x => x.First()))
					{
						var productCode = item.ProductCode;
						var description = item.Lookups.ProductList.GetDescriptionFromCode(productCode);
						codeDescriptionPairList.Add(new CodeDescriptionPair(productCode.ToString(), description));
					}
					return new ReadOnlyCodeDescriptionPairList(codeDescriptionPairList);
				}
			}

			public ReadOnlyCodeDescriptionPairList PriceListCodeList
			{
				get
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					var productCode = parent.ProductCode;
					foreach (var item in EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.OfType<UsageBillingPriceList>().Where(x => x.ProductCode == productCode))
					{
						var priceListCode = item.PriceListCode;
						var description = item.Description;
						codeDescriptionPairList.Add(new CodeDescriptionPair(priceListCode.ToString(), description));
					}
					return new ReadOnlyCodeDescriptionPairList(codeDescriptionPairList);
				}
			}
		}

		#endregion
	}
}
