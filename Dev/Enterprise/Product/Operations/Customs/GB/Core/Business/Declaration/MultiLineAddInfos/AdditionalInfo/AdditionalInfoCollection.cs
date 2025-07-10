using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfoCollection : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection
		, Integration.Customs.GB.IAdditionalInfoCollection
	{
		public AdditionalInfoCollection(BusinessObject parent) : base(parent)
		{ }

		public new AdditionalInfo this[int i] => (AdditionalInfo)base[i];

		public new AdditionalInfo AddNew() => (AdditionalInfo)base.AddNew();

		#region IAdditionalInfoCollection Implements

		Integration.Customs.GB.IAdditionalInfo Integration.Customs.GB.IAdditionalInfoCollection.this[int index] => this[index];

		Integration.Customs.GB.IAdditionalInfo Integration.Customs.GB.IAdditionalInfoCollection.AddNew() => AddNew();

		#endregion
	}
}
