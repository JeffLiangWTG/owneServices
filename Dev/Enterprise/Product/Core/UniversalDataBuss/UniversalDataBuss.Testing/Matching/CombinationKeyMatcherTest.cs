using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Testing.Matching
{
	class CombinationKeyMatcherTest : TestCaseWithFactory
	{
		public void TestTooManyMatchesReturnsNull()
		{
			for (var i = 0; i < 6; i++)
			{
				Factory.NewWithValidTestData<DummyBusinessObject>();
			}
			Factory.Save();

			var logger = new XMLDummyLogger();
			CombinationKeyMatcherParentLimit.OverrideParentLimitForTesting(5);
			var matcher = new CombinationKeyMatcherForTest(Factory, new Dummy(), logger);
			AssertExceptionThrown<MessageProcessingBusinessFailureException>(
				"This should throw a process failure exception.",
				"Too many parents to match against. Please ensure that any Additional References used are unique identifiers.",
				() =>   matcher.GetBestMatch());
		}

		class CombinationKeyMatcherForTest : CombinationKeyMatcher<DummyBusinessObject, IReferencesParent>
		{
			public CombinationKeyMatcherForTest(BusinessObjectFactory factory, IReferencesParent referencesParent, IXmlImportLogger logger) : base(factory, referencesParent, logger)
			{
			}
			protected override void BuildFallbackMatchDelegates(IReferencesParent referencesParent) { }
			protected override void BuildMatchingQueryAndMatchDelegates(IReferencesParent referencesParent)
			{
				AddPossibleMatch(DummyBizoSchema.Z0_IsValid, false, new MatchDelegate(_ => 0));
			}
			protected override bool CheckLatestParent(DummyBusinessObject parent, DummyBusinessObject parentToCompare) => false;

			protected override ZQuery GetFullQuery(ZQuery initialMatchingQuery, IReferencesParent referencesParent)
			{
				return initialMatchingQuery;
			}
		}

		class Dummy : IReferencesParent { }

		class XMLDummyLogger : IXmlImportLogger
		{
			public bool IsUpdatingConsol { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public bool HasIgnoredModule { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public bool OrgMatchingDisabled => false;

			public ITopLevelDataObject TopLevelDataObject => throw new System.NotImplementedException();

			public IDataContextDataObject TopLevelDataContext => throw new System.NotImplementedException();

			public IEnumerable<ISimpleLog> Logs => throw new System.NotImplementedException();

			public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

			public void FireDataImportedToBusinessObject(BusinessObject targetBO) => throw new System.NotImplementedException();
			public void Log(LogType type, string message) => throw new System.NotImplementedException();
			public void LogBoth(LogType type, string message) => LogString = type.ToString() + "|" + message;
			public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey) => throw new System.NotImplementedException();

			public void LogErrorToServiceTaskOnly(string message)
			{
				LogBoth(LogType.Error, message);
			}

			public string LogString { get; set; }
		}
	}
}
