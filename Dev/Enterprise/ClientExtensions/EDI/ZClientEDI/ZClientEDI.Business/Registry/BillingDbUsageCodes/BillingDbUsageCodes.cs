using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingDbUsageCodes : AutoBillingDbUsageCodes
	{
		#region Properties

		[List("Lookups.PriceHeaderCodes")]
		public override ZString PriceHeaderCode { get => base.PriceHeaderCode; set => base.PriceHeaderCode = value; }

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingDbUsageCodes();
		}

		#endregion

		#region Lists

		public CodeLookups Lookups => new CodeLookups();

		public class CodeLookups
		{
			public ReadOnlyCodeDescriptionPairList PriceHeaderCodes
			{
				get { return BillingConstants.PriceHeaderType.GetPriceHeaderTypeList(); }
			}
		}

		#endregion

		#region Validation

		public override void ValidateCategory()
		{
			base.ValidateCategory();
			MandatoryValidation.CheckEntered(CategoryInfo);
		}

		public override void ValidatePriceItemCode()
		{
			base.ValidatePriceItemCode();
			MandatoryValidation.CheckEntered(PriceItemCodeInfo);
		}

		public override void ValidatePriceHeaderCode()
		{
			base.ValidatePriceHeaderCode();
			MandatoryValidation.CheckEntered(PriceHeaderCodeInfo);
			ListValidation.ErrorIfInvalidCode(PriceHeaderCodeInfo);
		}

		#endregion
	}
}
