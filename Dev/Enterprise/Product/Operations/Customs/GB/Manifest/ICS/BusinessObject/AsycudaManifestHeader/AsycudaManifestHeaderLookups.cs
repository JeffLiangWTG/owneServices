using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeaderBase parent) : base(parent)
		{
		}
		public new AsycudaManifestHeaderBase Parent => (AsycudaManifestHeaderBase)base.Parent;

		public CodeDescriptionPairList ProfileList
		{
			get
			{
				var branch = Parent.Branch;
				var pwdList = new GlbExternalPasswordCollection_GB(branch.Company);
				string eori = branch.OrgProxy.GetEuIdentificationNumber();

				var list = new CodeDescriptionPairList();
				pwdList.Load();
				foreach (GlbExternalPassword_GB pwd in pwdList)
				{
					if (pwd.Status == PasswordStatusList.Codes.Valid && pwd.EORI == eori)
					{
						list.AddPair(pwd.Badge);
					}
				}
				return list;
			}
		}

		public override CodeDescriptionPairList RegistrationStatusList => new RegistrationStatusList();

		protected override CodeDescriptionPairList SpecificCircumstanceListCore
		{
			get => Factory.GetCachedValue("SpecificCircumstanceList.GBSS", () =>
			{
				var list = new SpecificCircumstanceList();
				list.RemoveCode(Enterprise.Customs.ASYCUDA.Business.SpecificCircumstanceList.Codes.A);
				return list;
			});
		}
	}
}
