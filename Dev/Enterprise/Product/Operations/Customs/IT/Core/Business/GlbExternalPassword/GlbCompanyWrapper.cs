using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Integration.CustomsIntegration.IT;

namespace Enterprise.Customs.IT.Business;

public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IGlbCompanyWrapper
{
	public GlbCompanyWrapper(GlbCompany company) : base(company)
	{
	}

	public static GlbCompanyWrapper Get(GlbCompany company)
	{
		return company?.Factory.GetCachedValue(company.PK.ToString(), () => new GlbCompanyWrapper(company));
	}

	public override bool IsValidWrapper => true;

	#region PasswordCollection

	[ChildEditable]
	public GlbMauExternalPasswordCollection PasswordCollection
	{
		get
		{
			if (passwordCollection == null)
			{
				passwordCollection = new GlbMauExternalPasswordCollection(Company);
				passwordCollection.Load();
				RegisterEditableChildObject(passwordCollection);
			}

			return passwordCollection;
		}
	}
	GlbMauExternalPasswordCollection passwordCollection;

	IGlbMauExternalPasswordCollection IGlbCompanyWrapper.PasswordCollection => PasswordCollection;

	#endregion
}
