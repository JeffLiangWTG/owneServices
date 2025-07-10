using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(CopyTemplateTreeBizo))]
	class CopyTemplateTreeBizoTest : EntityCopyTemplateBizoTest<CopyTemplateTreeBizo>
	{
		public override void TestHumanReadableName()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			AssertEquals("Copy Template", copyTree.HumanReadableName);
		}

		public void TestConfigurationName()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			copyTree.CopyTemplateNode.ConfigurationName = "abc";
			AssertEquals("abc", copyTree.ConfigurationName);

			copyTree.ConfigurationName = "xyz";
			AssertEquals("xyz", copyTree.CopyTemplateNode.ConfigurationName);
		}

		public void TestIsFiltered()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			copyTree.CopyTemplateNode.ConfigurationSource = ConfigurationSourceCodes.FilteredRecord;
			Assert(copyTree.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord);
			copyTree.CopyTemplateNode.ConfigurationSource = ConfigurationSourceCodes.Selected;
			Assert(!(copyTree.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord));

			copyTree.ConfigurationSource = ConfigurationSourceCodes.FilteredRecord;
			Assert(copyTree.CopyTemplateNode.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord);
			copyTree.ConfigurationSource = ConfigurationSourceCodes.Selected;
			Assert(!(copyTree.CopyTemplateNode.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord));
		}

		public void TestOrderby()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			copyTree.EntityFilter = new EntityFilter();
			copyTree.EntityFilter.OrderBy = "xyz";
			AssertEquals("First-time initialization from EntityFilter", "xyz", copyTree.OrderBy);

			copyTree.EntityFilter.OrderBy = "abc";
			AssertEquals("Should not be re-read from EntityFilter", "xyz", copyTree.OrderBy);

			copyTree.OrderBy = "123";
			AssertEquals("Should not change EntityFilter yet", "abc", copyTree.EntityFilter.OrderBy);

			copyTree.OrderByDescending = true;
			AssertEquals("123 DESC", copyTree.OrderBy);

			copyTree.OrderByField = "qwerty";
			AssertEquals("qwerty DESC", copyTree.OrderBy);
		}

		public void TestEntityFilter()
		{
			var filter1 = new EntityFilter();
			var filter2 = new EntityFilter();

			var copyTree = GetNewCopyTemplateNodeBizo();

			copyTree.CopyTemplateNode.Filter = filter1;
			AssertSame(filter1, copyTree.EntityFilter);

			copyTree.EntityFilter = filter2;
			AssertSame(filter2, copyTree.CopyTemplateNode.Filter);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CopyTemplateTreeBizoValidation), GetNewCopyTemplateNodeBizo().Validation.GetType());
		}

		public void TestValidateConfigurationName()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			copyTree.Validation.ValidateConfigurationName();
			Assert("Should have error about mandatory configuration name.", copyTree.ConfigurationNameInfo.HasErrors());

			copyTree.ConfigurationName = "xyz";
			copyTree.Validation.ValidateConfigurationName();
			Assert(!copyTree.ConfigurationNameInfo.HasErrors());
		}

		public void TestValidateOrderBy()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			copyTree.Validation.ValidateOrderBy();
			Assert(!copyTree.OrderByInfo.HasErrors());

			copyTree.ConfigurationSource = ConfigurationSourceCodes.FilteredRecord;
			copyTree.Validation.ValidateOrderBy();
			Assert("Order by is not mandatory - no errors expected.", !copyTree.OrderByInfo.HasErrors());

			copyTree.OrderBy = DummyBizoSchema.Constants.Z0_Code + " DESC, " + DummyBizoSchema.Constants.Z0_Description + " ASC";
			copyTree.Validation.ValidateOrderBy();
			Assert(!copyTree.OrderByInfo.HasErrors());

			copyTree.OrderBy = "xyz";
			copyTree.Validation.ValidateOrderBy();
			Assert("Should have error about wrong column name.", copyTree.OrderByInfo.HasErrors());
		}

		public void TestValidationErrorIfNothingToCopy()
		{
			var propertyNode = new PropertyCopyTemplateNode { CopyMethod = CopyMethod.Copy };

			var innerNode = new EntityCopyTemplateNode();
			innerNode.Nodes.Add(propertyNode);

			var relatedEntityNode = new RelatedEntityCopyTemplateNode
			{
				CopyMethod = RelatedEntityCopyMethod.None,
				InnerNode = innerNode
			};

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(relatedEntityNode);

			var copyTree = new CopyTemplateTree { InnerNode = entityNode, ConfigurationName = "Test" };
			var copyTreeBizo = new CopyTemplateTreeBizo(copyTree, null);

			copyTreeBizo.Validation.ValidateConfigurationName();
			AssertNoErrors(copyTreeBizo.ConfigurationNameInfo);
			copyTreeBizo.Validation.ValidateAll();
			AssertHasRowError("Should have error about nothing selected to copy.", copyTreeBizo, "Unable to save this template: there is nothing selected on main element to copy.");

			relatedEntityNode.CopyMethod = RelatedEntityCopyMethod.Copy;

			copyTreeBizo.Validation.ValidateConfigurationName();
			AssertHasRowError("Still has validation error.", copyTreeBizo, "Unable to save this template: there is nothing selected on main element to copy.");

			copyTreeBizo.Validation.ValidateAll();
			AssertNoErrors(copyTreeBizo);
		}

		public void TestIsNominated()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			copyTree.CopyTemplateNode.ConfigurationSource = ConfigurationSourceCodes.NominatedRecord;
			Assert(copyTree.ConfigurationSource == ConfigurationSourceCodes.NominatedRecord);
			copyTree.CopyTemplateNode.ConfigurationSource = ConfigurationSourceCodes.Selected;
			Assert(!(copyTree.ConfigurationSource == ConfigurationSourceCodes.NominatedRecord));

			copyTree.ConfigurationSource = ConfigurationSourceCodes.NominatedRecord;
			Assert(copyTree.CopyTemplateNode.ConfigurationSource == ConfigurationSourceCodes.NominatedRecord);
			copyTree.ConfigurationSource = ConfigurationSourceCodes.Selected;
			Assert(!(copyTree.CopyTemplateNode.ConfigurationSource == ConfigurationSourceCodes.NominatedRecord));
		}

		public void TestConfigurationSource()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();

			copyTree.CopyTemplateNode.ConfigurationSource = ConfigurationSourceCodes.Selected;
			AssertEquals("SLT", copyTree.ConfigurationSource);

			copyTree.ConfigurationSource = ConfigurationSourceCodes.FilteredRecord;
			AssertEquals("FIR", copyTree.CopyTemplateNode.ConfigurationSource);

			copyTree.CopyTemplateNode.ConfigurationSource = ConfigurationSourceCodes.NominatedRecord;
			AssertEquals("NOR", copyTree.ConfigurationSource);
			AssertEquals("NOR", copyTree.CopyTemplateNode.ConfigurationSource);
		}

		public void TestConfigurationSources()
		{
			var copyTree = GetNewCopyTemplateNodeBizo();
			var sources = copyTree.ConfigurationSources;
			AssertEquals(3, sources.Count);
			var allCodes = sources.GetAllCodes();
			Assert(allCodes.Contains(ConfigurationSourceCodes.Selected));
			Assert(allCodes.Contains(ConfigurationSourceCodes.FilteredRecord));
			Assert(allCodes.Contains(ConfigurationSourceCodes.NominatedRecord));
		}

		public void TestNominatedRecord()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			var copyTree = GetNewCopyTemplateNodeBizo();
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo(JobShipmentSchema.Constants.JS_HouseBill, template);

			Assert(copyTree.NominatedRecordPk.IsEmpty);
			copyTree.NominatedRecordPk = propertyCopyBizo.PK;
			Assert(!copyTree.NominatedRecordPk.IsEmpty);

			AssertNotNull(propertyCopyBizo.NominatedObjectsCollection);
		}

		public void TestCopyTemplateTreeDefaultValue()
		{
			var copyTemplateTree = new CopyTemplateTree();
			AssertEquals(copyTemplateTree.ConfigurationSource, ConfigurationSourceCodes.Selected);
		}

		public void TestValidateNominatedRecordPk()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.Organisation + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var copyTree = CreateNewCopyTemplateNodeBizo(DummyBizoSchema.Constants.TableName, template);

			Factory.Save();

			copyTree.ConfigurationSource = ConfigurationSourceCodes.NominatedRecord;
			copyTree.Validation.ValidateNominatedRecordPk();
			Assert("Should have error about mandatory nominated record PK.", copyTree.NominatedRecordPkInfo.HasErrors());

			copyTree.NominatedRecordPk = org.PK;
			copyTree.Validation.ValidateNominatedRecordPk();
			Assert(!copyTree.NominatedRecordPkInfo.HasErrors());
		}

		#region Implementation

		protected override CopyTemplateTreeBizo GetNewCopyTemplateNodeBizo()
		{
			return CreateNewCopyTemplateNodeBizo(DummyBizoSchema.Constants.TableName, null);
		}

		CopyTemplateTreeBizo CreateNewCopyTemplateNodeBizo(string tableName, UniversalCopyTemplate parent)
		{
			return new CopyTemplateTreeBizo(CreateNewNode(tableName), parent);
		}

		CopyTemplateTree CreateNewNode(string tableName)
		{
			return
				new CopyTemplateTree
				{
					Id = Guid.NewGuid().ToString(),
					Name = tableName,
					TableName = tableName
				};
		}

		public override void TestRunPreSaveValidationCoreLoadsChildCollections()
		{
			Assert("Not relevant on a root element", true);
		}

		protected override void SetEntityCopyTemplateBizoToHaveData(CopyTemplateTreeBizo entityCopyTemplateBizo)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
