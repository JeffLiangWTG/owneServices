using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExdocLineCompleteCollection : BusinessObjectCollection<QuarantineExDocLine>
	{
		public QuarantineExdocLineCompleteCollection(BaseJobDeclaration jobDeclaration)
			: base(jobDeclaration.Factory)
		{
			this.JobDeclaration = jobDeclaration;
		}

		#region Implementation
		protected readonly BaseJobDeclaration JobDeclaration;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			if (JobDeclaration.InvoiceLines.Count == 0)
			{
				filter.IsNoResultQuery = true;
			}
			else
			{
				filter.AddToFilter(QuarantineExDocLineSchema.QL_JI, JobDeclaration.InvoiceLines.GetPKs());
			}
			return filter;
		}

		#endregion
	}
}
