using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobDocsAndCartageValidation : Freight.Forwarding.Business.ForwardingDocsAndCartageValidation
	{
		public WoolworthsJobDocsAndCartageValidation(WoolworthsJobDocsAndCartage parent) : base(parent)
		{
		}

		public new WoolworthsJobDocsAndCartage Parent
		{
			get { return (WoolworthsJobDocsAndCartage)base.Parent; }
		}

		public WoolworthsJobDeclaration Declaration
		{
			get
			{
				ZQuery filter = new ZQuery(JobDeclarationSchema.PK, SQLComparisonOperator.Equal, Parent.JP_ParentID);
				return (WoolworthsJobDeclaration)Parent.Factory.LoadTop1(typeof(WoolworthsJobDeclaration), filter);
			}
		}
	}
}
