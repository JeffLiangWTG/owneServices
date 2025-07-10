using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ComponentRelationship))]
	public class ComponentRelationshipTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var componentRelationship = BMSTestHelper.CreateComponentRelationship(Factory);

			AssertEquals(ZGuid.Empty, componentRelationship.FC_FS_System);
			AssertEquals(BMComponentTypeList.Codes.ComponentRelationship, componentRelationship.FC_Type);
		}

		public void TestSetType_ReportsErrorWhenNotView()
		{
			var componentRelationship = BMSTestHelper.CreateComponentRelationship(Factory);

			componentRelationship.FC_Type = BMComponentTypeList.Codes.Bucket;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to set a different type on a component relationship. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSetSystem_ReportsErrorWhenNotNull()
		{
			var componentRelationship = BMSTestHelper.CreateComponentRelationship(Factory);
			var system = BMSTestHelper.CreateSystem(Factory);

			componentRelationship.FC_FS_System = system.PK;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to set a system on a component relationship. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAddComponentFetchHints_DBHits()
		{
			const int numRelationshipsAndSystems = 6;
			const int numBuffersPerRelationship = 6;

			var relationships = CreateMultipleRelationships(numRelationshipsAndSystems, numBuffersPerRelationship);
			var newFactory = Factory.CreateNewFactory();

			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentLinkSchema.Constants.TableName, 1 },
			};

			var loadedRelationships = relationships.Select(relationship => newFactory.Load<ComponentRelationship>(relationship.PK)).ToArray();

			using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true))
			{
				ComponentRelationship.AddComponentFetchHints(loadedRelationships);

				var relatedComponents = loadedRelationships
					.SelectMany(relationship => relationship.RelatedComponentLinks)
					.Select(link => newFactory.Load<BMComponent>(link.FL_FC_ComponentTo));

				AssertEquals("Failed to load all components in relationships.", numRelationshipsAndSystems * numBuffersPerRelationship, relatedComponents.Count());
			}
		}

		#region Logs

		public void TestNoStmALogs()
		{
			var relationship = Factory.NewWithValidTestData<ComponentRelationship>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, relationship.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			relationship.FC_IsActive = false;
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			relationship.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		IEnumerable<ComponentRelationship> CreateMultipleRelationships(int numRelationshipsAndSystems, int numBuffersPerRelationship)
		{
			var relationships = new List<ComponentRelationship>();

			for (var i = 0; i < numRelationshipsAndSystems; i++)
			{
				relationships.Add(BMSTestHelper.CreateComponentRelationship(Factory, name: "Relationship " + i));
				var system = BMSTestHelper.CreateSystem(Factory);

				for (var j = 0; j < numBuffersPerRelationship; ++j)
				{
					var component = BMSTestHelper.CreateBuffer(system, "Component " + j);
					BMSTestHelper.CreateComponentRelationshipLink(Factory, relationships[i], component);
				}
			}

			return relationships;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return AddValidCollectionData((ComponentRelationship)base.GetBusinessObjectForFetchForLoad());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return AddValidCollectionData((ComponentRelationship)base.GetNewBusinessObjectForDeleteTest(factory));
		}

		BusinessObject AddValidCollectionData(ComponentRelationship component)
		{
			foreach (var childComponent in component.ChildComponents)
			{
				childComponent.FC_BufferTimespanInMinutes = 60;
			}

			return component;
		}

		#endregion
	}
}
