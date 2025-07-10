using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HouseBillOfLadingTermsAndConditionsCollection : RegistryImageCollection
	{
		public new HouseBillOfLadingTermsAndConditions this[int i]
		{
			get { return (HouseBillOfLadingTermsAndConditions)Elements[i]; }
		}

		public new HouseBillOfLadingTermsAndConditions AddNew()
		{
			return (HouseBillOfLadingTermsAndConditions)base.AddNew();
		}

		#region Find By Code/Delivery Mode

		public new IEnumerable<HouseBillOfLadingTermsAndConditions> FindByCode(string code)
		{
			return this.Cast<HouseBillOfLadingTermsAndConditions>().Where(image => image.Code == code);
		}

		public HouseBillOfLadingTermsAndConditions FindByCodeForDeliveryModeALL(string code)
		{
			return FindByCode(code).FirstOrDefault(image => image.IsDeliveryModeALL);
		}

		public HouseBillOfLadingTermsAndConditions FindByCodeAndDeliveryMode(string code, string deliveryMode)
		{
			return FindByCode(code).FirstOrDefault(image => image.DeliveryMode == deliveryMode) ?? FindByCodeForDeliveryModeALL(code);
		}

		#endregion

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HouseBillOfLadingTermsAndConditionsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HouseBillOfLadingTermsAndConditions();
		}
	}
}
