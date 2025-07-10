using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class GlbBrokerExternalPasswordLookups : GlbExternalPasswordLookups
{
	public GlbBrokerExternalPasswordLookups(GlbBrokerExternalPassword parent) : base(parent)
	{
	}

	public CodeDescriptionPairList Nodes
	{
		get
		{
			var nodeList = new CodeDescriptionPairList();
			foreach (var accountWithCompany in AllAccountsWithCompany)
			{
				nodeList.AddPair(accountWithCompany.Account.AccountNode, accountWithCompany.Company.CompanyName);
			}
			return nodeList;
		}
	}
}
