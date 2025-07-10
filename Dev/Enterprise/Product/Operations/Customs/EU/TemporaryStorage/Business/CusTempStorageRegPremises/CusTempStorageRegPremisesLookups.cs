using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business
{
	public class CusTempStorageRegPremisesLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesLookups
	{
		public CusTempStorageRegPremisesLookups(AutoCusTempStorageRegPremises parent) : base(parent)
		{
		}

		protected new CusTempStorageRegPremises Parent => (CusTempStorageRegPremises)base.Parent;

		public CusAuthorisationHeaderCollectionFiltered AuthorizationNumberList
		{
			get
			{
				var parent = Parent;
				var authorizationType = GetAuthorizationType(parent.SRP_Type);
				var cusAuthorisationHeaderCollectionFiltered = new CusAuthorisationHeaderCollectionFiltered(Factory, authorizationType, Parent.AuthorizationOwner);
				cusAuthorisationHeaderCollectionFiltered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", (ZString)authorizationType, isRemovable: false));
				cusAuthorisationHeaderCollectionFiltered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", parent.AuthorizationOwner, isRemovable: false));
				return cusAuthorisationHeaderCollectionFiltered;
			}
		}

		protected virtual string GetAuthorizationType(string srpType)
		{
			return AuthorizationTypeList.Codes.TST;
		}
	}
}
