using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	internal class UniversalJobLinksHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetMatchingLinksFromSource()
			=> TestGetMatchingLinksFromSource("Source", DataContextType.ForwardingShipment, null, WhsItemReceiveConsignmentSchema.Constants.Prefix, shouldFindResult: true);

		public void TestGetMatchingLinksFromSource_GivenDifferentOrg_DoesNotReturn()
			=> TestGetMatchingLinksFromSource("Source", DataContextType.ForwardingShipment, Factory.New<OrgHeader>(), WhsItemReceiveConsignmentSchema.Constants.Prefix, shouldFindResult: false);

		public void TestGetMatchingLinksFromSource_GivenDifferentKey_DoesNotReturn()
			=> TestGetMatchingLinksFromSource("Source2", DataContextType.ForwardingShipment, null, WhsItemReceiveConsignmentSchema.Constants.Prefix, shouldFindResult: false);

		public void TestGetMatchingLinksFromSource_GivenDifferentContext_DoesNotReturn()
			=> TestGetMatchingLinksFromSource("Source", DataContextType.LandTransportConsignment, null, WhsItemReceiveConsignmentSchema.Constants.Prefix, shouldFindResult: false);

		public void TestGetMatchingLinksFromSource_GivenDifferentParentTableCode_DoesNotReturn()
		=> TestGetMatchingLinksFromSource("Source", DataContextType.ForwardingShipment, null, WhsItemDispatchConsignmentSchema.Constants.Prefix, shouldFindResult: false);

		void TestGetMatchingLinksFromSource(string key, DataContextType context, OrgHeader owner, string parentTableCode, bool shouldFindResult)
		{
			var orgHeader = Factory.New<OrgHeader>();
			owner = owner ?? orgHeader;
			CreateUniversalJobLink(Factory, sourceKey: "Source", DataContextType.ForwardingShipment, orgHeader, WhsItemReceiveConsignmentSchema.Constants.Prefix);

			var results = UniversalJobLinkHelper.GetMatchingJobLinks(Factory, key, context, owner, parentTableCode).ToArray();
			var resultKeys = results.Select(l => l.GetValue(StmUniversalJobLinkSchema.UCL_SourceKey));

			var expectedResults = shouldFindResult ? new string[] { key } : Array.Empty<string>();
			AssertContainsExactElementsInAnyOrder(expectedResults, resultKeys);
		}

		public void TestGetMatchingLinksFromSource_GivenInternal_FindsLinksWithNullOwner()
		{
			CreateUniversalJobLink(Factory, "Source", DataContextType.ForwardingShipment, null, WhsItemReceiveConsignmentSchema.Constants.Prefix);

			var results = UniversalJobLinkHelper.GetMatchingJobLinks(Factory, sourceKey: "Source", DataContextType.ForwardingShipment, null, WhsItemReceiveConsignmentSchema.Constants.Prefix)?.ToArray();
			var resultKeys = results.Select(l => l.GetValue(StmUniversalJobLinkSchema.UCL_SourceKey));

			AssertContainsExactElementsInAnyOrder(new string[] { "Source" }, resultKeys);
		}

		public void TestGetMatchingLinksFromSource_GivenNullKey_ReturnsEmptyList()
		{
			var results = Array.Empty<IColumnIndexer>();
			AssertNoExceptionThrown(() => results = UniversalJobLinkHelper.GetMatchingJobLinks(Factory, sourceKey: null, DataContextType.ForwardingShipment, Factory.New<OrgHeader>(), WhsItemReceiveConsignmentSchema.Constants.Prefix)?.ToArray());

			AssertEquals(results.Length, 0);
		}

		public void TestGetMatchingLinksFromSource_GivenNullTableCode_ReturnsEmptyList()
		{
			var results = Array.Empty<IColumnIndexer>();
			AssertNoExceptionThrown(() => results = UniversalJobLinkHelper.GetMatchingJobLinks(Factory, "Source", DataContextType.ForwardingShipment, Factory.New<OrgHeader>(), null)?.ToArray());

			AssertEquals(results.Length, 0);
		}

		IStmUniversalJobLink CreateUniversalJobLink(UniversalObjectFactory universalFactory, ZString sourceKey, DataContextType dataContext, OrgHeader owner, string parentTableCode)
		{
			var link = universalFactory.BOFactory.New<IStmUniversalJobLink>();
			link.UCL_SourceKey = sourceKey;
			link.UCL_OH_Owner = owner?.PK ?? ZGuid.Empty;
			link.UCL_SourceType = dataContext.ToString();
			link.UCL_ParentTableCode = parentTableCode;
			return link;
		}
	}
}
