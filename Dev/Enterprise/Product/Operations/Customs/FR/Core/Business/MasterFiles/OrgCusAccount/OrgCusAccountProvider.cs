using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class OrgCusAccountProvider : Enterprise.MasterFiles.Business.OrgCusAccountProvider
	{
		public OrgCusAccountProvider()
			: base(Core.Constants.CountryCodes.France)
		{
		}

		public override Enterprise.MasterFiles.Business.OrgCusAccountLookups GetNewLookups(OrgCusAccount cusAccount) => new OrgCusAccountLookups(cusAccount);

		public override Enterprise.MasterFiles.Business.OrgCusAccountValidation GetNewValidation(OrgCusAccount cusAccount) => new OrgCusAccountValidation(cusAccount);

		public override ZString CZ_IssuerFieldType => nameof(FieldType.TextCodeFindBox);

		public override ZBool ShouldDefaultTypeWhenAble => true;
	}
}
