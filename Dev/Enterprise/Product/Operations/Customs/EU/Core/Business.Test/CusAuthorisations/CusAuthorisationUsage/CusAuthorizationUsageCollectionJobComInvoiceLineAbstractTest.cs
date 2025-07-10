using CargoWise.Schema;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

[TestsSubclassesOf(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>))]
public abstract class CusAuthorizationUsageCollectionJobComInvoiceLineAbstractTest<TCollection, TCusAuthorizationUsage> : CusAuthorizationUsageCollectionAbstractTest<TCollection, TCusAuthorizationUsage, JobComInvoiceLine>
	where TCollection : CusAuthorizationUsageCollection<TCusAuthorizationUsage, JobComInvoiceLine>
	where TCusAuthorizationUsage : CusAuthorizationUsage
{
	protected sealed override SchemaGuidColumn ExpectedFKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;
}
