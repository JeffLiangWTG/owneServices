using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business.Testing
{
	class StmUniversalCopyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSU_CopyObjectTableCode()
		{
			var copy = Factory.New<StmUniversalCopy>();
			copy.SUC_CopyObjectTableCode = "XXX";
			AssertHasErrorContaining(copy.SUC_CopyObjectTableCodeInfo, "Scheduled Universal Copy is not currently supported on the selected entity type");
			copy.SUC_CopyObjectTableCode = "GG";
			AssertNoErrors(copy.SUC_CopyObjectTableCodeInfo);
		}

		public void TestCheckSUC_S9_CopyTemplate_UnpublishedTemplate()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = "XXX_UC";
			template.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true)), template);
			template.S9_IsPublished = false;

			var copy = Factory.New<StmUniversalCopy>();
			copy.SUC_S9_CopyTemplate = template.PK;

			var schedule = Factory.New<StmUniversalCopyScheduleTask>();
			schedule.S5_ParentID = copy.PK;
			schedule.S5_GB = EnvProxy.Instance.CurrentBranch.PK;

			copy.Validation.ValidateAll();
			AssertHasError(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published and will not be available to other users.");
			AssertNoWarnings(copy.SUC_S9_CopyTemplateInfo);

			template.S9_IsPublished = true;
			copy.Validation.ValidateAll();
			AssertNoErrors(copy.SUC_S9_CopyTemplateInfo);
			AssertNoWarnings(copy.SUC_S9_CopyTemplateInfo);

			template.S9_GC = ZGuid.NewZGuid();
			copy.Validation.ValidateAll();
			AssertHasError(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and is not available under selected Branch.");
			AssertNoWarning(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and may be not available when this Schedule Task is running.");
			AssertHasWarning(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and is not available under current Company.");

			schedule.S5_GB = ZGuid.Empty;
			copy.Validation.ValidateAll();
			AssertNoErrors(copy.SUC_S9_CopyTemplateInfo);
			AssertHasWarning(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and may be not available when this Schedule Task is running.");
			AssertHasWarning(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and is not available under current Company.");

			template.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			copy.Validation.ValidateAll();
			AssertNoErrors(copy.SUC_S9_CopyTemplateInfo);
			AssertHasWarning(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and may be not available when this Schedule Task is running.");
			AssertNoWarning(copy.SUC_S9_CopyTemplateInfo, "Selected Copy Template is not published for all Companies and is not available under current Company.");

			template.S9_GC = ZGuid.Empty;
			copy.Validation.ValidateAll();
			AssertNoErrors(copy.SUC_S9_CopyTemplateInfo);
			AssertNoWarnings(copy.SUC_S9_CopyTemplateInfo);
		}

		public void TestCheckCopyTemplateProducesValidCopy()
		{
			var source = Factory.NewWithValidTestData<Dummy2>();
			source.Z0_Code = "Val1";
			source.Z0_NVarChar = "Val2";
			Factory.Save();

			var copyTemplateTree = new CopyTemplateTree(typeof(Dummy2));
			var propertyNodeCode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == "Z0_Code");
			propertyNodeCode.CopyMethod = CopyMethod.Copy;

			var copyBizo = (Dummy2)new BusinessObjectCopyManager().Copy(source, copyTemplateTree).Object;
			copyBizo.RunPreSaveValidation();
			AssertHasError(copyBizo.Z0_NVarCharInfo, "Please enter a value.");

			var universalCopyTemplate = Factory.New<UniversalCopyTemplate>();
			universalCopyTemplate.S9_ModuleID = "XXX_UC";
			universalCopyTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			universalCopyTemplate.S9_IsPublished = true;

			var scheduleTaskWithError = CreateUniversalCopyScheduleTaskObject(source, universalCopyTemplate, copyTemplateTree);
			scheduleTaskWithError.RunPreSaveValidation();
			AssertHasError(scheduleTaskWithError.Parent.SUC_S9_CopyTemplateInfo,
@"Selected Copy Template produces invalid or incomplete copy record with following validation errors:
Error - Z0_NVarChar: Please enter a value.");

			var propertyNodeNVarChar = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == "Z0_NVarChar");
			propertyNodeNVarChar.CopyMethod = CopyMethod.Copy;

			copyBizo = (Dummy2)new BusinessObjectCopyManager().Copy(source, copyTemplateTree).Object;
			copyBizo.RunPreSaveValidation();
			AssertNoErrors(copyBizo.Z0_NVarCharInfo);

			var scheduleTaskNoError = CreateUniversalCopyScheduleTaskObject(source, universalCopyTemplate, copyTemplateTree);
			scheduleTaskNoError.RunPreSaveValidation();
			AssertNoErrors(scheduleTaskNoError.Parent.SUC_S9_CopyTemplateInfo);
		}

		public void TestCheckCopyTemplateProducesValidCopy_CanHandleInvalidData()
		{
			var source = Factory.NewWithValidTestData<Dummy2>();
			source.Z0_Description = "some dummy strings that can't be converted to Datetime";
			Factory.Save();

			var copyTemplateTree = new CopyTemplateTree(typeof(Dummy2));

			var propertyNodeDate = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == "Z0_Date");
			propertyNodeDate.CopyMethod = CopyMethod.Property;
			propertyNodeDate.Value = "Z0_Description";

			var copyResult = new BusinessObjectCopyManager().Copy(source, copyTemplateTree);
			var expectedError = "Cannot set property DummyBizo.Z0_Date of type DateTime with value 'some dummy strings that can't be converted to Datetime'.";
			AssertEquals(expectedError, copyResult.ErrorMessage);

			var universalCopyTemplate = Factory.New<UniversalCopyTemplate>();
			universalCopyTemplate.S9_ModuleID = "XXX_UC";
			universalCopyTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			universalCopyTemplate.S9_IsPublished = true;

			var scheduleTask = CreateUniversalCopyScheduleTaskObject(source, universalCopyTemplate, copyTemplateTree);
			scheduleTask.RunPreSaveValidation();
			AssertHasError(scheduleTask.Parent.SUC_S9_CopyTemplateInfo, @"Selected Copy Template produces invalid or incomplete copy record with following validation errors:
" + expectedError);
		}

		StmUniversalCopyScheduleTask CreateUniversalCopyScheduleTaskObject(Dummy2 source, UniversalCopyTemplate universalCopyTemplate, CopyTemplateTree copyTemplateTree)
		{
			var scheduleTaskNoError = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			scheduleTaskNoError.S5_ScheduleDescription = "Schedule Task Description";
			scheduleTaskNoError.Parent.SUC_CopyObjectId = source.PK;
			scheduleTaskNoError.Parent.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			scheduleTaskNoError.Parent.SUC_S9_CopyTemplate = universalCopyTemplate.PK;
			scheduleTaskNoError.Parent.Template.CopyTemplateTree = new CopyTemplateTreeBizo(copyTemplateTree, universalCopyTemplate);
			scheduleTaskNoError.Parent.CopyObject = source;
			return scheduleTaskNoError;
		}

		class Dummy2 : DummyBusinessObject
		{
			public Dummy2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override DummyBizoValidation GetNewValidation()
			{
				return new Dummy2Validation(this);
			}
		}

		class Dummy2Validation : DummyBizoValidation
		{
			public Dummy2Validation(Dummy2 parent) : base(parent) { }

			protected override void CheckZ0_NVarChar()
			{
				MandatoryValidation.CheckEntered(Parent.Z0_NVarCharInfo);
			}
		}
	}
}
