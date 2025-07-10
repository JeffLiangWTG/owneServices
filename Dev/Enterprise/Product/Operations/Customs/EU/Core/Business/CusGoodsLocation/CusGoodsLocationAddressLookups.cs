using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocationAddressLookups : JobDocAddressLookups
	{
		public CusGoodsLocationAddressLookups(CusGoodsLocationAddress parent)
			: base(parent)
		{
		}

		public virtual ICollection AuthorisationNumberList
		{
			get
			{
				var parent = Parent;
				var numbers = new CusAuthorisationHeaderCollectionFiltered(Factory, parent.E2_GovRegNumType, parent.IdentificationHolderPK);
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", parent.IdentificationHolderPK, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber, "Property", GetAuthorisationNumber()));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", parent.E2_GovRegNumType));
				return numbers;
			}
		}

		protected virtual ZString GetAuthorisationNumber() => Parent.E2_GovRegNum;

		public new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

		public virtual OrgHeaderCollection OrganisationList => new OrgHeaderCollection(Factory);
	}
}
