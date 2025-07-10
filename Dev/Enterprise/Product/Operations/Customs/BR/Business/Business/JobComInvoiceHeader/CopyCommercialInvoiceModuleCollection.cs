using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class CopyCommercialInvoiceModuleCollection : CommercialInvoiceCollection
	{
		public CopyCommercialInvoiceModuleCollection(JobDeclaration declaration) : base(declaration.Factory)
		{
			destinationDeclaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration destinationDeclaration;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();

			var excludeQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.NotEqual, destinationDeclaration.PK);
			excludeQuery.AddToFilter(new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.Equal, null), JoinCondition.Or);
			query.AddToFilter(excludeQuery);
			return query;
		}
	}
}
