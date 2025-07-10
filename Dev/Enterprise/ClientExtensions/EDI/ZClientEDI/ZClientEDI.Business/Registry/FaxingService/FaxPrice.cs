using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class FaxPrice : AutoFaxPrice, ICurrentFactory
	{
		public FaxPrice() { }

		[List("Lookups.Currencies")]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		public FaxPriceLookups Lookups
		{
			get { return lookups ?? (lookups = new FaxPriceLookups(this)); }
		}
		FaxPriceLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FaxPrice();
		}

		#region ICurrentFactory Members

		BusinessObjectFactory ICurrentFactory.CurrentFactory
		{
			get { return CurrentFactory; }
		}

		#endregion

		#region Validation

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			ListValidation.ErrorIfInvalidCode(CodeInfo, Lookups.Currencies);
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
			}
		}

		public override void ValidatePrice()
		{
			base.ValidatePrice();
			MandatoryValidation.CheckNotNegative(PriceInfo);
			MandatoryValidation.CheckNotZero(PriceInfo);
		}

		#endregion
	}
}

