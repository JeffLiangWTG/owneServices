using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiLicenceSettingCollectionNonDependent : ActiveBusinessObjectCollection<EdiLicenceSetting>
	{
		public EdiLicenceSettingCollectionNonDependent(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(EdiLicenceSetting)))
		{
		}
	}
}


