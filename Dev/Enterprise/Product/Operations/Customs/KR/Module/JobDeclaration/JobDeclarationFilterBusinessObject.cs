using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		protected override ZQuery GetImporterSupplierQuery(ZGuid importer, ZGuid supplier)
		{
			ZQuery result = new ZQuery();
			if (!importer.IsEmpty)
			{
				AddSubQueriesToQuery(result, JobDeclarationSchema.JE_OH_Importer, JobComInvoiceHeaderSchema.JZ_OH_Buyer, importer);
			}

			if (!supplier.IsEmpty)
			{
				AddSubQueriesToQuery(result, JobDeclarationSchema.JE_OH_Supplier, JobComInvoiceHeaderSchema.JZ_OH_Supplier, supplier);
			}

			return result;
		}

		void AddSubQueriesToQuery(ZQuery result, SchemaGuidColumn declarationPropertyColumn, SchemaGuidColumn invoiceHeaderPropertyColumn, ZGuid propertyValue)
		{
			var invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			invoiceHeaderQuery.AddToFilter(invoiceHeaderPropertyColumn, propertyValue);

			var dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			dbOnlyResult.AddToFilter(declarationPropertyColumn, propertyValue);
			dbOnlyResult.AddSubQuery(invoiceHeaderQuery, JoinCondition.Or);
			result.AddToFilter(dbOnlyResult);
		}
	}
}
