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
	public class DiscountSuspensionPolicy : AutoDiscountSuspensionPolicy
	{
		#region Properties

		[List("Lookups.ProductList")]
		public override ZString ProductCode
		{
			get { return base.ProductCode; }
			set { base.ProductCode = value; }
		}

		[List("Lookups.PolicyList")]
		public override ZString PolicyCode
		{
			get { return base.PolicyCode; }
			set { base.PolicyCode = value; }
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DiscountSuspensionPolicy();
		}

		#endregion

		#region Lists

		public CodeLookups Lookups { get; } = new CodeLookups();

		public class CodeLookups
		{
			public ReadOnlyCodeDescriptionPairList ProductList { get; } = new ProductTypes(true);
			public ReadOnlyCodeDescriptionPairList PolicyList { get; } = new DiscountSuspensionPolicyList();
		}

		#endregion

		#region Validation

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(ProductCodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ProductCodeInfo);
		}

		public override void ValidatePolicyCode()
		{
			base.ValidatePolicyCode();
			MandatoryValidation.CheckEntered(PolicyCodeInfo);
			ListValidation.ErrorIfInvalidCode(PolicyCodeInfo);
		}

		#endregion
	}
}
