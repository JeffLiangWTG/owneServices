using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class RegistryUserAgreementTypeCollection : RegistryBusinessObjectCollection
	{
		public new RegistryUserAgreementType this[int i] => (RegistryUserAgreementType)Elements[i];

		public void AddNew(string code, MultilingualString description)
		{
			var newItem = (RegistryUserAgreementType)AddNew();
			newItem.Code = code;
			newItem.Description = description;
			newItem.Level = EdiUserAgreementLevelList.Codes.User;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RegistryUserAgreementType();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RegistryUserAgreementTypeCollection();
		}
	}
}
