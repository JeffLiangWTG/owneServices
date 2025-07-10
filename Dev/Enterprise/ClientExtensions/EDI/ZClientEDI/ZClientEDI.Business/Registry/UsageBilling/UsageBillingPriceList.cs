using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageBillingPriceList : AutoUsageBillingPriceList
	{
		public UsageBillingPriceList()
		{
		}

		public UsageBillingPriceList(BusinessObjectFactory factory)
			: base(factory) { }

		public UsageBillingPriceList(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public UsageBillingPriceList(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new UsageBillingPriceList(fallbackLevel, factory);
			return clone;
		}

		public static UsageBillingPriceList GetDefaultValue()
		{
			return new UsageBillingPriceList();
		}

		[List("Lookups.ProductList")]
		public override ZString ProductCode { get => base.ProductCode; set => base.ProductCode = value; }

		[List("Lookups.UsageCategoryList")]
		public override ZString RawUsageCategory { get => base.RawUsageCategory; set => base.RawUsageCategory = value; }

		#region Validations

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(ProductCodeInfo);
		}

		public override void ValidateRawUsageCategory()
		{
			base.ValidateRawUsageCategory();
			MandatoryValidation.CheckEntered(RawUsageCategoryInfo);
			ListValidation.ErrorIfInvalidCode(RawUsageCategoryInfo);

			if (!ProductCode.IsEmpty && !RawUsageCategory.IsEmpty)
			{
				if (ParentCollections.Any(x => x.OfType<UsageBillingPriceList>().Any(y => y.PK != PK && y.RawUsageCategory == RawUsageCategory))
					|| BillingConstants.GetAllBillingSystems().ContainsCode(RawUsageCategory))
				{
					RawUsageCategoryInfo.AddError("The raw usage category must be unique.");
				}
			}
		}

		public override void ValidatePriceListCode()
		{
			base.ValidatePriceListCode();
			MandatoryValidation.CheckEntered(PriceListCodeInfo);

			if (!ProductCode.IsEmpty && !PriceListCode.IsEmpty)
			{
				if (ParentCollections.Any(x => x.OfType<UsageBillingPriceList>().Any(y => y.PK != PK && y.PriceListCode == PriceListCode))
					|| BillingConstants.GetAllBillingSystems().ContainsCode(PriceListCode))
				{
					PriceListCodeInfo.AddError("The price list system code must be unique.");
				}
			}
		}

		#endregion

		#region Lookups

		public CodeLookups Lookups
		{
			get => CurrentFactory.GetCachedValue("UsageBillingPriceList.Lookups", () => new CodeLookups());
		}

		public class CodeLookups
		{
			public ReadOnlyCodeDescriptionPairList ProductList { get; } = new ProductTypes(false);
			public ReadOnlyCodeDescriptionPairList UsageCategoryList => EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value;
		}

		#endregion
	}
}
