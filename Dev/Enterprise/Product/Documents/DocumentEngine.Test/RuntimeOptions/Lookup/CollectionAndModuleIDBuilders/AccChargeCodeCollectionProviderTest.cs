using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccChargeCodeCollectionProvider))]
	sealed class AccChargeCodeCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		public void TestCollectionCompleteFilter()
		{
			AssertEquals("AC_GC = CONVERT('" + Env.CurrentCompany.PK.ToString() + "', 'System.Guid')", Provider.Collection.CompleteFilter.LiteralTextADO);
		}

		protected override Type ExpectedCollectionType => typeof(AccChargeCodeCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccChargeCode;

		protected override int ExpectedMaxLength => AccChargeCodeSchema.AC_Code.MaxLength;
	}
}
