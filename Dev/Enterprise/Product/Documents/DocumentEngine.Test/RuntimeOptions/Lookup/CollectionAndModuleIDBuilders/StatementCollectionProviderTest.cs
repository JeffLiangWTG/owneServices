using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestsSubclassesOf(typeof(StatementCollectionProvider), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	public abstract class StatementCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.Get<ICusStatementHeaderCollection>("ICusStatementHeaderCollection", Factory).GetType();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.US.USCustomsStatement;

		protected override int ExpectedMaxLength => CusStatementHeaderSchema.B2_StatementNumber.MaxLength;
	}
}
