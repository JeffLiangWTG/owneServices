using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SupplyChainSecurityOrganisationToUseCollection : RegistryBusinessObjectCollection
	{
		public SupplyChainSecurityOrganisationToUseCollection()
			: base()
		{
		}

		public SupplyChainSecurityOrganisationToUseCollection(ZString countryCode)
			: base()
		{
			this.CountryCode = countryCode;
		}

		#region Add / Remove

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region GetClone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			SupplyChainSecurityOrganisationToUseCollection clone = GetNewCollection();
			clone.CodeMaxLength = CodeMaxLength;
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		#endregion

		#region Collection

		protected virtual SupplyChainSecurityOrganisationToUseCollection GetNewCollection()
		{
			return new SupplyChainSecurityOrganisationToUseCollection(CountryCode);
		}

		public new SupplyChainSecurityOrganisationToUse this[int i]
		{
			get { return (SupplyChainSecurityOrganisationToUse)base[i]; }
		}

		public SupplyChainSecurityOrganisationToUse this[string key]
		{
			get { return this.Cast<SupplyChainSecurityOrganisationToUse>().FirstOrDefault(x => x.Code == key); }
		}

		public new SupplyChainSecurityOrganisationToUse AddNew()
		{
			return (SupplyChainSecurityOrganisationToUse)base.AddNew();
		}

		public SupplyChainSecurityOrganisationToUse Add(ZString code, MultilingualString description, ZString validationCode)
		{
			SupplyChainSecurityOrganisationToUse result = AddNew();
			result.Code = code;
			result.Description = description;
			result.ValidationCode = validationCode;

			return result;
		}

		#endregion

		#region Business Object

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupplyChainSecurityOrganisationToUse(CurrentFallbackLevel);
		}

		#endregion

		public ZString CountryCode { get; set; }
	}
}
