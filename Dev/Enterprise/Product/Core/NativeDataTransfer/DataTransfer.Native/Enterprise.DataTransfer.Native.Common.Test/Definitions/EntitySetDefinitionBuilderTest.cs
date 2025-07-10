using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.DB.Exception;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	public class EntitySetDefinitionBuilderTest : TransactionedTestCase
	{
		public void TestLoadDefinition()
		{
			var element = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			AssertNoExceptionThrown(
				"It should be able to load sample xml",
				() => new EntitySetDefinitionBuilder(element).GetEntitySetDefinition()
			);
		}

		public void TestLoadDefinition_ReportExceptions()
		{
			var element = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XElement("Entities",
				new XElement("Entity",
					new XAttribute("Name", "DummyBizo"))),
					new XElement("Associations",
					new XElement("Association",
					new XAttribute("From", "DummyBizo"),
					new XAttribute("To", "DummyBizo"),
					new XAttribute("Key", "Blah_Blah")
			)));

			AssertExceptionThrown<NoColumnFoundException>(
			"Column Blah_Blah doesn't exist",
			() => new EntitySetDefinitionBuilder(element).GetEntitySetDefinition()
			);
			AssertContains("NativeSchemaSet", ErrorReporter.LastMessageReported);
			AssertEquals(1,ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestNoImportReasonAttribute()
		{
			var element = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XAttribute("NoImportReason", "Just because..."),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			var definition = new EntitySetDefinitionBuilder(element).GetEntitySetDefinition();
			AssertEquals("NoImportReason is parsed correctly", "Just because...", definition.NoImportReason);

			element = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			definition = new EntitySetDefinitionBuilder(element).GetEntitySetDefinition();
			Assert("NoImportReason is parsed correctly", string.IsNullOrEmpty(definition.NoImportReason));
		}

		public void TestLoadDefinition_Dummy()
		{
			var info = TestUtil.GetEntitySetDefinition("Dummy");

			var root = info.Root;
			AssertNotNull(root);

			var entities = info.Entities;

			Assert("Should have entity with full name: DummyBizo", entities.HasDefinition("DummyBizo"));
			Assert("Should have entity with full name: DummyBizo.DummyDependentBizo", entities.HasDefinition("DummyBizo.DummyDependentBizo"));
			Assert("Should have entity with full name: DummyBizo.DummyDependentBizo_Suffix", entities.HasDefinition("DummyBizo.DummyDependentBizo_Suffix"));
			Assert("Should have entity with full name: DummyBizo.DummyPivot", entities.HasDefinition("DummyBizo.DummyPivot"));
			Assert("Should have entity with full name: DummyBizo.OrgHeader", entities.HasDefinition("DummyBizo.OrgHeader"));
			Assert("Should have entity with full name: DummyBizo.OrgCode", entities.HasDefinition("DummyBizo.OrgCode"));

			var dummyBizo = entities.FindDefinition("DummyBizo");
			var dependent = entities.FindDefinition("DummyBizo.DummyDependentBizo");
			var dependentSuffix = entities.FindDefinition("DummyBizo.DummyDependentBizo_Suffix");
			var pivot = entities.FindDefinition("DummyBizo.DummyPivot");
			var orgHeader = entities.FindDefinition("DummyBizo.OrgHeader");
			var code = entities.FindDefinition("DummyBizo.OrgCode");

			Assert("OrgHeader should be Parent for DummyBizo", dummyBizo.Parents.Contains(orgHeader));
			Assert("Code should be Parent for DummyBizo", dummyBizo.Parents.Contains(code));
			Assert("DummyBizo Should has many DependentSuffix", dummyBizo.HasMany().Contains(dependentSuffix));
			Assert("DummyBizo Should has many Pivot", dummyBizo.HasMany().Contains(pivot));
			Assert("DummyBizo Should has one Dependent", dummyBizo.HasOne().Contains(dependent));

			AssertEquals(dummyBizo, dependent.Parent);
			AssertEquals(dummyBizo, dependentSuffix.Parent);
			AssertEquals(dummyBizo, pivot.Parent);

			Assert(orgHeader.Children.Contains(dummyBizo));
			Assert(code.Children.Contains(dummyBizo));
		}

		public void TestLoadDefinition_Dummy_LinksAdditionalChildren()
		{
			var info = TestUtil.GetEntitySetDefinition("Dummy");

			var root = info.Root;
			AssertNotNull(root);

			var entities = info.Entities;

			Assert("Should have entity with full name: DummyBizo", entities.HasDefinition("DummyBizo"));
			Assert("Should have entity with full name for DummyBizoAsExternalChild.",
				entities.HasDefinition("DummyBizo.DummyPivotForExternalChild.ChildPivotRelatedDependentBizO.DummyBizoAsExternalChild"));

			var dummyBizo = entities.FindDefinition("DummyBizo");
			var dummyPivotForExternalChild = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild");
			var childPivotRelatedDependentBizO = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild.ChildPivotRelatedDependentBizO");
			var dummyBizoAsExternalChild = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild.ChildPivotRelatedDependentBizO.DummyBizoAsExternalChild");
			
			Assert("DummyBizo should be Child for DummyPivotForExternalChild.", dummyBizo.Children.Contains(dummyPivotForExternalChild));
			Assert("DummyPivotForExternalChild should be Parent for ChildPivotRelatedDependentBizO.", dummyPivotForExternalChild.Parents.Contains(childPivotRelatedDependentBizO));
			Assert("ChildPivotRelatedDependentBizO should link additional Child DummyPivotForExternalParent.", childPivotRelatedDependentBizO.Children.Contains(dummyBizoAsExternalChild));
		}

		public void TestLoadDefinition_Dummy_LinksAdditionalParents()
		{
			var info = TestUtil.GetEntitySetDefinition("Dummy");

			var root = info.Root;
			AssertNotNull(root);

			var entities = info.Entities;

			Assert("Should have entity with full name: DummyBizo", entities.HasDefinition("DummyBizo"));
			Assert("Should have entity with full name for ParentPivotRelatedDependentBizO.",
				entities.HasDefinition("DummyBizo.DummyPivotForExternalChild.DummyPivotForExternalParent.ParentPivotRelatedDependentBizO.DummyBizoAsExternalParent"));

			var dummyBizo = entities.FindDefinition("DummyBizo");
			var dummyPivotForExternalChild = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild");
			var dummyPivotForExternalParent = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild.DummyPivotForExternalParent");
			var parentPivotRelatedDependentBizO = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild.DummyPivotForExternalParent.ParentPivotRelatedDependentBizO");
			var dummyBizoAsExternalParent = entities.FindDefinition("DummyBizo.DummyPivotForExternalChild.DummyPivotForExternalParent.ParentPivotRelatedDependentBizO.DummyBizoAsExternalParent");

			Assert("DummyBizo should be Child for DummyPivotForExternalChild.", dummyBizo.Children.Contains(dummyPivotForExternalChild));
			Assert("DummyPivotForExternalChild should be Child for DummyPivotForExternalParent.", dummyPivotForExternalChild.Children.Contains(dummyPivotForExternalParent));
			Assert("DummyPivotForExternalParent should be Parent for ParentPivotRelatedDependentBizO.", dummyPivotForExternalParent.Parents.Contains(parentPivotRelatedDependentBizO));
			Assert("ParentPivotRelatedDependentBizO should link additional Parent DummyBizoAsExternalParent.", parentPivotRelatedDependentBizO.Parents.Contains(dummyBizoAsExternalParent));
		}

		public void TestLoadDefinition_AlllDefinitions()
		{
			TestUtil.GetEntitySetDefinition("Vessel");
			TestUtil.GetEntitySetDefinition("Airline");
			TestUtil.GetEntitySetDefinition("Container");
			TestUtil.GetEntitySetDefinition("CommodityCode");
			TestUtil.GetEntitySetDefinition("Country");
			TestUtil.GetEntitySetDefinition("DangerousGood");
			TestUtil.GetEntitySetDefinition("ServiceLevel");
			TestUtil.GetEntitySetDefinition("TransportZone");
			var statementEntityDefinition = TestUtil.GetEntitySetDefinition("CusStatement");
			AssertEquals("Import is not supported", "Statements are generated when system receives statement messages from Customs. System will not generate statements from Native Xml messages.", statementEntityDefinition.NoImportReason);
			Assert("All Definition should be able to load", true);
		}

		public void TestLoadDefinition_Product()
		{
			var definition = TestUtil.GetEntitySetDefinition("Product");

			var manufacturer1 = definition.Root.Children.FirstOrDefault(c => c.EntityName == "CusClassPartPivot")
				.Children.FirstOrDefault(c => c.EntityName == "CusUSClassification")
				.Parents.FirstOrDefault(p => p.EntityName == "Manufacturer");

			var orgCusCode1 = manufacturer1.Children.Single(p => p.EntityName == "OrgCusCode");
			AssertNotNull("Manufacturer should has an OrgCusCode entity", orgCusCode1);
			AssertNotNull("OrgCusCode entity should has a CustomsRegNo property", orgCusCode1.PropertyDefinitions["CustomsRegNo"]);
			AssertNotNull("OrgCusCode entity should has a CodeType property", orgCusCode1.PropertyDefinitions["CodeType"]);
			AssertNotNull("OrgCusCode entity should has a CodeCountry entity", orgCusCode1.Parents.Single(p => p.EntityName == "CodeCountry"));

			var manufacturer2 = definition.Root.Children.FirstOrDefault(c => c.EntityName == "CusClassPartPivot")
				.Children.FirstOrDefault(c => c.EntityName == "ComponentCusClassPartPivot")
				.Children.FirstOrDefault(c => c.EntityName == "CusUSClassification")
				.Parents.FirstOrDefault(p => p.EntityName == "Manufacturer");

			var orgCusCode2 = manufacturer2.Children.Single(p => p.EntityName == "OrgCusCode");
			AssertNotNull("Manufacturer should has an OrgCusCode entity", orgCusCode2);
			AssertNotNull("OrgCusCode entity should has a CustomsRegNo property", orgCusCode2.PropertyDefinitions["CustomsRegNo"]);
			AssertNotNull("OrgCusCode entity should has a CodeType property", orgCusCode2.PropertyDefinitions["CodeType"]);
			AssertNotNull("OrgCusCode entity should has a CodeCountry entity", orgCusCode2.Parents.Single(p => p.EntityName == "CodeCountry"));
		}

		public void TestUseBatchingAttribute()
		{
			var elementTrue = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XAttribute("UseBatching", "true"),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			var elementFalse = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XAttribute("UseBatching", "false"),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			var elementMissing = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			AssertEquals(true, new EntitySetDefinitionBuilder(elementTrue).GetEntitySetDefinition().UseBatching);
			AssertEquals(false, new EntitySetDefinitionBuilder(elementFalse).GetEntitySetDefinition().UseBatching);
			AssertEquals(false, new EntitySetDefinitionBuilder(elementMissing).GetEntitySetDefinition().UseBatching);
		}

		public void TestLoadDefinition_Rate()
		{
			var definition = TestUtil.GetEntitySetDefinition("Rate");
			AssertEquals(true, definition.UseBatching);
			AssertEquals("Internal entities should be in defined in order of processing precedence, parent before child",
				"RatingHeader, RateEntry, RateLines, RateLineItems, StmNote, JobDocAddress, OrgAddress",
				string.Join(", ", definition.Entities.Where(x => !x.IsExternal).Select(x => x.TableName)));
		}

		public void TestLoadDefinition_TransportZone()
		{
			var definition = TestUtil.GetEntitySetDefinition("TransportZone");
			AssertEquals("Internal entities should be in defined in order of processing precedence, parent before child",
				"RateTransportProvider, RateTransportZones, RateTransportZoneItem",
				string.Join(", ", definition.Entities.Where(x => !x.IsExternal).Select(x => x.TableName)));
		}

		public void TestSetEntitiesTwiceIsReported()
		{
			var element = new XElement("NativeSchemaSet",
				new XAttribute("Name", "Dummy"),
				new XAttribute("Root", "DummyBizo"),
				new XElement("Entities",
					new XElement("Entity",
						new XAttribute("Name", "DummyBizo"))));

			var entitySetDefinition = new EntitySetDefinitionBuilder(element).GetEntitySetDefinition();
			entitySetDefinition.SetEntities(null, null);

			AssertEquals("This setter should only ever be called once from EntitySetDefinitionBuilder", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
		}

		#endregion
	}
}
