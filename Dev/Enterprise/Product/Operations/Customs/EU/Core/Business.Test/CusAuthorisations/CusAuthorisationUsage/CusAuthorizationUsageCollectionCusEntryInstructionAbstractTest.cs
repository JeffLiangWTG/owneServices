using CargoWise.Schema;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

[TestsSubclassesOf(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>))]
public abstract class CusAuthorizationUsageCollectionCusEntryInstructionAbstractTest<TCollection, TCusAuthorizationUsage, TCusEntryInstruction> : CusAuthorizationUsageCollectionAbstractTest<TCollection, TCusAuthorizationUsage, TCusEntryInstruction>
	where TCollection : CusAuthorizationUsageCollection<TCusAuthorizationUsage, TCusEntryInstruction>
	where TCusAuthorizationUsage : CusAuthorizationUsage
	where TCusEntryInstruction : CusEntryInstruction
{
	protected sealed override SchemaGuidColumn ExpectedFKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;
}
