using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(FieldPseudoApplicator))]
	sealed class FieldPseudoApplicatorTest : PseudoApplicatorTest<FieldPseudoApplicator>
	{
		[TestDate(2003, 01, 01)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSkipFields()
		{
			TestDateAttribute.UseUNLOCO = true;
			ZDateTime now = ZDateTime.Now;
			Dummy1.Z0_Date = now.AddDays(1);
			Dummy2.Z0_Date = now.AddDays(2);
			Dummy3.Z0_Date = now.AddDays(3);
			Dummy1.Z0_AnotherDate = now.AddDays(4);
			Dummy2.Z0_AnotherDate = now.AddDays(5);
			Dummy3.Z0_AnotherDate = now.AddDays(6);
			Dummy1.Z0_SmallDateTime = now.AddDays(7);
			Dummy2.Z0_SmallDateTime = now.AddDays(8);
			Dummy3.Z0_SmallDateTime = now.AddDays(9);
			Dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(now.AddDays(7), TimeSpan.FromHours(9));
			Dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(now.AddDays(8), TimeSpan.FromHours(9));
			Dummy3.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			Dummy1.Z0_Time = new ZTime(1, 2);
			Dummy2.Z0_Time = new ZTime(4, 5);
			Dummy3.Z0_Time = ZTime.Empty;
			Dummy1.Z0_Geography = new ZGeography("POINT (121 48)");
			Dummy2.Z0_Geography = new ZGeography("POINT (-121.2 48.9)");
			Dummy3.Z0_Geography = ZGeography.Empty;
			Factory.Save();
			OperationalActionFieldDescriptor descriptor1 = Action.FieldDescriptors.AddNew();
			descriptor1.FieldName = DummyBizoSchema.Constants.Z0_Date;
			descriptor1.EmptyBehaviour = EmptyBehaviourList.Codes.Apply;
			descriptor1.Order = 1;
			OperationalActionFieldDescriptor descriptor2 = Action.FieldDescriptors.AddNew();
			descriptor2.FieldName = DummyBizoSchema.Constants.Z0_AnotherDate;
			descriptor2.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			descriptor2.Order = 2;
			OperationalActionFieldDescriptor descriptor3 = Action.FieldDescriptors.AddNew();
			descriptor3.FieldName = DummyBizoSchema.Constants.Z0_SmallDateTime;
			descriptor3.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			descriptor3.Order = 3;
			OperationalActionFieldDescriptor descriptor4 = Action.FieldDescriptors.AddNew();
			descriptor4.FieldName = DummyBizoSchema.Constants.Z0_DateTimeOffset;
			descriptor4.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			descriptor4.Order = 4;
			OperationalActionFieldDescriptor descriptor5 = Action.FieldDescriptors.AddNew();
			descriptor5.FieldName = DummyBizoSchema.Constants.Z0_Geography;
			descriptor5.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			descriptor5.Order = 5;
			OperationalActionFieldDescriptor descriptor6 = Action.FieldDescriptors.AddNew();
			descriptor6.FieldName = DummyBizoSchema.Constants.Z0_Time;
			descriptor6.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			descriptor6.Order = 6;
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { Dummy1.PK, Dummy3.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			runner.Printer = ZGuid.NewZGuid();
			runner.Fields[0][RunnerTextField.Schema.Property] = ZDateTime.Empty;
			runner.Fields[1][RunnerTextField.Schema.Property] = ZDateTime.Empty;
			runner.Fields[2][RunnerTextField.Schema.Property] = now.AddDays(10);
			runner.Fields[3][RunnerTextField.Schema.Property] = new ZDateTimeOffset(now.AddDays(11));
			runner.Fields[4][RunnerTextField.Schema.Property] = new ZGeography("POINT (-111.8 49.1)");
			runner.Fields[5][RunnerTextField.Schema.Property] = ZTime.Empty;
			RunRunner(runner, "INFO: Starting Section: Fields ...");
			AssertEquals("Dummy1.Z0_Date", ZDateTime.Empty, Dummy1.Z0_Date);
			AssertEquals("Dummy2.Z0_Date", now.AddDays(2), Dummy2.Z0_Date);
			AssertEquals("Dummy3.Z0_Date", ZDateTime.Empty, Dummy3.Z0_Date);
			AssertEquals("Dummy1.Z0_AnotherDate", now.AddDays(4), Dummy1.Z0_AnotherDate);
			AssertEquals("Dummy2.Z0_AnotherDate", now.AddDays(5), Dummy2.Z0_AnotherDate);
			AssertEquals("Dummy3.Z0_AnotherDate", now.AddDays(6), Dummy3.Z0_AnotherDate);
			AssertEquals("Dummy1.Z0_SmallDateTime", now.AddDays(10), Dummy1.Z0_SmallDateTime);
			AssertEquals("Dummy2.Z0_SmallDateTime", now.AddDays(8), Dummy2.Z0_SmallDateTime);
			AssertEquals("Dummy3.Z0_SmallDateTime", now.AddDays(10), Dummy3.Z0_SmallDateTime);
			AssertEquals("Dummy1.Z0_DateTimeOffset", new ZDateTimeOffset(now.AddDays(11), TimeSpan.FromHours(9)), Dummy1.Z0_DateTimeOffset);
			AssertEquals("Dummy2.Z0_DateTimeOffset", new ZDateTimeOffset(now.AddDays(8), TimeSpan.FromHours(9)), Dummy2.Z0_DateTimeOffset);
			AssertEquals("Dummy3.Z0_DateTimeOffset", new ZDateTimeOffset(now.AddDays(11), TimeSpan.FromHours(11)), Dummy3.Z0_DateTimeOffset);
			AssertEquals("Dummy1.Z0_Geography", new ZGeography("POINT (-111.8 49.1)"), Dummy1.Z0_Geography);
			AssertEquals("Dummy2.Z0_Geography", new ZGeography("POINT (-121.2 48.9)"), Dummy2.Z0_Geography);
			AssertEquals("Dummy3.Z0_Geography", new ZGeography("POINT (-111.8 49.1)"), Dummy3.Z0_Geography);
			AssertEquals("Dummy1.Z0_Time", new ZTime(1, 2), Dummy1.Z0_Time);
			AssertEquals("Dummy2.Z0_Time", new ZTime(4, 5), Dummy2.Z0_Time);
			AssertEquals("Dummy3.Z0_Time", ZTime.Empty, Dummy3.Z0_Time);
		}

		public void TestRunAction_UpdateTargets()
		{
			AssertNotNull(Dummy1);
			AssertNotNull(Dummy2);
			AssertNotNull(Dummy3);
			Factory.Save();
			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			descriptor.FieldName = "Z0_VarCharMax";
			AssertEquals("precondition:", "", Dummy1.Z0_VarCharMax);
			AssertEquals("precondition:", "", Dummy2.Z0_VarCharMax);
			AssertEquals("precondition:", "", Dummy3.Z0_VarCharMax);
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { Dummy1.PK, Dummy3.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			runner.Printer = ZGuid.NewZGuid();
			runner.Fields[0][RunnerTextField.Schema.Property] = "Blaticus";
			RunRunner(runner, "INFO: Starting Section: Fields ...");
			AssertEquals("Should update", "Blaticus", Dummy1.Z0_VarCharMax);
			AssertEquals("Dummy2 not selected", "", Dummy2.Z0_VarCharMax);
			AssertEquals("Should update", "Blaticus", Dummy3.Z0_VarCharMax);
		}

		public void TestRunAction_UpdateTargetsWithInvalidValues()
		{
			string expectedLog = "INFO: Starting Section: Fields ...\r\n" + "ERROR: DummyBizo has the following errors:\r\n" + "Error - Z0_Description: Bad!\r\n" + "Error - Z0_VarCharMax: Error!\r\n" + "ERROR: DummyBizo has the following errors:\r\n" + "Error - Z0_Description: Bad!\r\n" + "Error - Z0_VarCharMax: Error!\r\n";
			AssertNotNull(Dummy1);
			AssertNotNull(Dummy2);
			Factory.Save();
			Action.FieldDescriptors.AddNew().FieldName = "Z0_Description";
			Action.FieldDescriptors.AddNew().FieldName = "Z0_VarCharMax";
			AssertEquals("precondition:", "Default", Dummy1.Z0_Description);
			AssertEquals("precondition:", "Default", Dummy2.Z0_Description);
			AssertEquals("precondition:", "", Dummy1.Z0_VarCharMax);
			AssertEquals("precondition:", "", Dummy2.Z0_VarCharMax);
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { Dummy1.PK, Dummy2.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), selectedRecords);
			runner.Fields[0][RunnerTextField.Schema.Property] = "Bad"; // Z0_Description with a value of "Bad" will be validated as an error
			runner.Fields[1][RunnerTextField.Schema.Property] = "Error"; // Z0_VarCharMax with a value of "Error" will be validated as an error
			RunRunner(runner, expectedLog, false);
		}

		public void TestRunAction_UpdateFields_CreateNullValueInShipment()
		{
			AssertShipmentHeaderUpdate("Job");
			AssertShipmentHeaderUpdate("JobHeader");

			void AssertShipmentHeaderUpdate(ZString infoName)
			{
				var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = Env.CurrentCompanyPK;
				var operationsRep = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();

				using (var shipmentModule = ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
				{
					var context = new OperationalActionContext(((IOperationalActionSupportable)shipmentModule).OperationalActionSupporter, "whatever", WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
					var action = Factory.New<OperationalAction>();
					action.Context = context;
					var descriptor = action.FieldDescriptors.AddNew();
					descriptor.FieldName = $"{infoName}+JH_GE";
					descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
					var descriptor2 = action.FieldDescriptors.AddNew();
					descriptor2.FieldName = $"{infoName}+JH_GB";
					descriptor2.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
					var descriptor3 = action.FieldDescriptors.AddNew();
					descriptor3.FieldName = $"{infoName}+JH_GS_NKRepOps";
					descriptor3.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;

					var selectedRecords = new SelectedRecords
					{ PrimaryKeys = new[] { shipment.PK } };
					var runner = new OperationalActionRunner(action, shipment.GetType(), selectedRecords)
					{ Printer = ZGuid.NewZGuid() };
					var field = (RunnerPKModuleField)runner.Fields[0];
					field.Property = department.PK;
					var field2 = (RunnerPKModuleField)runner.Fields[1];
					field2.Property = branch.PK;
					var field3 = (RunnerNKModuleField)runner.Fields[2];
					field3.Property = operationsRep.GS_Code;

					var dummyLog = new DummyOperationalActionLog();
					var factoryForChanges = new BusinessObjectFactory();
					runner.Run(dummyLog, factoryForChanges);
					factoryForChanges.Save();

					AssertEquals(department.PK, (shipment[infoName] as BusinessObject)["JH_GE"]);
					AssertEquals(branch.PK, (shipment[infoName] as BusinessObject)["JH_GB"]);
					AssertEquals(operationsRep.GS_Code, (shipment[infoName] as BusinessObject)["JH_GS_NKRepOps"]);
				}
			}
		}

		#region TestUpdatedFieldOnUnregisteredChildIsIncludedInValidation
		public void TestUpdatedFieldOnUnregisteredChildIsIncludedInValidation()
		{
			const string ExpectedLog = @"INFO: Starting Section: Fields ...
ERROR: DummyBizo has the following errors:
Error - Z0_Description: Bad!
";
			var bizo1 = Factory.New<DummyBizoWithChildren>();
			var bizo2 = Factory.New<DummyBizoWithChildren>();
			bizo1.Z0_Guid = bizo2.PK;
			AssertSame(bizo2, bizo1.ExternalDummy);
			Factory.Save();
			AssertEquals("precondition:", "Default", bizo2.Z0_Description);
			Action.FieldDescriptors.AddNew().FieldName = nameof(DummyBizoWithChildren.ExternalDummy) + "." + DummyBizoSchema.Constants.Z0_Description;
			var selectedRecords = new SelectedRecords { PrimaryKeys = new[] { bizo1.PK } };
			var runner = new OperationalActionRunner(Action, typeof(DummyBizoWithChildren), selectedRecords);
			runner.Fields[0][RunnerTextField.Schema.Property] = "Bad"; // Z0_Description with a value of "Bad" will be validated as an error
			RunRunner(runner, ExpectedLog, false);
		}

		public void TestUpdatedFieldOnUnregisteredCollectionIsIncludedInValidation()
		{
			const string ExpectedLog = @"INFO: Starting Section: Fields ...
ERROR: DummyBizo has the following errors:
Error - Z0_Description: Bad!
";
			var bizo1 = Factory.New<DummyBizoWithChildren>();
			var bizo2 = bizo1.Dummies.AddNew();
			bizo2.Z0_Guid = bizo1.PK;
			Factory.Save();
			AssertEquals("precondition:", "Default", bizo2.Z0_Description);
			Action.FieldDescriptors.AddNew().FieldName = nameof(DummyBizoWithChildren.Dummies) + "." + DummyBizoSchema.Constants.Z0_Description;
			var selectedRecords = new SelectedRecords { PrimaryKeys = new[] { bizo1.PK } };
			var runner = new OperationalActionRunner(Action, typeof(DummyBizoWithChildren), selectedRecords);
			runner.Fields[0][RunnerTextField.Schema.Property] = "Bad"; // Z0_Description with a value of "Bad" will be validated as an error
			RunRunner(runner, ExpectedLog, false);
		}

		#endregion
		#region DummyBizoWithChildren
		class DummyBizoWithChildren : DummyBusinessObjectWithDocumentSupport
		{
			public DummyBizoWithChildren(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ActionFieldFollow(true)]
			public DummyBizoWithChildren ExternalDummy => bizo2 ?? (bizo2 = Factory.Load<DummyBizoWithChildren>(Z0_Guid));
			DummyBizoWithChildren bizo2;
			[ActionFieldFollow(true)]
			public DummyBizo2Collection Dummies
			{
				get
				{
					if (dummies == null)
					{
						dummies = new DummyBizo2Collection(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, PK));
						dummies.Load();
					}

					return dummies;
				}
			}

			DummyBizo2Collection dummies;
		}

		class DummyBizo2Collection : BusinessObjectCollection<DummyBizoWithChildren>
		{
			public DummyBizo2Collection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
			{
			}
		}

		#endregion
		#region Implementation
		protected override void ConfigureForInclusion(OperationalAction action)
		{
			OperationalActionFieldDescriptor descriptor = action.FieldDescriptors.AddNew();
			descriptor.FieldName = DummyBizoSchema.Constants.Z0_VarCharMax;
			descriptor.FieldCaption = "Caption";
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Apply;
		}

		protected override void ConfigureForExclusion(OperationalAction action)
		{
			action.FieldDescriptors.RemoveAndDeleteAll();
		}

		protected override FieldPseudoApplicator NewApplicator(OperationalActionRunner runner)
		{
			return new FieldPseudoApplicator(runner);
		}

		protected override Type TargetType
		{
			get
			{
				return typeof(DummyBizoWithChildren);
			}
		}
		#endregion
	}
}
