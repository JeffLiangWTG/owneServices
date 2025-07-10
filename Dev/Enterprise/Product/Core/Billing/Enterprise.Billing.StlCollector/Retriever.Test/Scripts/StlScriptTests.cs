using System.Data;
using System.Linq;
using CargoWise.Billing.Collectors;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class StlScriptTests : TransactionedTestCase
	{
		public void TestUseSmallDateTimeParameters()
		{
			var script = new DummyScript { DateTypeOverride = StlDateType.DateTime };
			var retriever = new RefStlScriptRetriever(script);
			var parameters = ((IStlScript)retriever).GetInputParameters(AusydMonthRange.New(2017, 9));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.DateTime));

			script.DateTypeOverride = StlDateType.SmallDateTime;
			parameters = ((IStlScript)retriever).GetInputParameters(AusydMonthRange.New(2017, 9));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}

		#region SuppressResourceStringsCheckRegion

#pragma warning disable 618 //StaticStlScript is obsolete, please don't use this class in new code

		internal class DummyScript : RefStlScriptWithDefaults
		{
			public StlDateType DateTypeOverride { get; set; }
			public StlDataGrain StlItemGrainOverride { get; set; }

			public override string DateType => RefStlScriptHelper.DateTypeToCode(DateTypeOverride);
			public override string DataGranularity => RefStlScriptHelper.DataGrainToCode(StlItemGrainOverride);
			public override string FeatureCode => "DUM";
			public override string RoleName => "Dummy";
			public override string ModuleName => "Dummy";
			public override string FunctionName => "Dummy";
			public override string FeatureName => "Dummy";
			public override string CompanyCode => string.Empty;
			public override string BranchCode => string.Empty;
			public override string TransactionDateUtc => "Z0_Date";
			public override string GuidReference => "Z0_PK";
			public override string BillingReference1 => "CONVERT(CHAR(36), Z0_PK)";
			public override string BillingReference2 => "Z0_VarCharMax";
			public override string FromClause => "	DummyBizo";
			public override string WhereClause => "1=1";
			public override string AdditionalRefs => "Z0_VarBinaryMax";
		}

		internal class DummyDateTimeOffsetScript : RefStlScriptWithDefaults
		{
			public override string FeatureCode => "DUO";
			public override string RoleName => "Dummy DateTimeOffset";
			public override string ModuleName => "Dummy";
			public override string FunctionName => "Dummy";
			public override string FeatureName => "Dummy DateTimeOffset";
			public override string CompanyCode => string.Empty;
			public override string BranchCode => string.Empty;
			public override string TransactionDateUtc => "Z0_DateTimeOffset";
			public override string GuidReference => "Z0_PK";
			public override string BillingReference1 => "CONVERT(CHAR(36), Z0_PK)";
			public override string BillingReference2 => "Z0_VarCharMax";
			public override string FromClause => "	DummyBizo";
			public override string WhereClause => "1=1";
			public override string DateType => RefStlScriptHelper.DateTypeToCode(StlDateType.DateTimeOffset);
		}

#pragma warning restore 618 //StaticStlScript is obsolete, please don't use this class in new code

		#endregion
	}
}
