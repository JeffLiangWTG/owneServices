using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CategorisedWorkflowIterationReasons))]
	sealed class CategorisedWorkflowIterationReasonsTest : RegistryBusinessObjectTest
	{
		public void TestIterationReasons()
		{
			var newIterationReasons = new WorkflowIterationReasonCollection();
			var oldIterationReasons = BizObj.IterationReasons;
			Assert(BizObj.IsRegisteredEditableChildObject(oldIterationReasons));

			BizObj.SetIterationReasons(newIterationReasons);
			Assert(!BizObj.IsRegisteredEditableChildObject(oldIterationReasons));
			Assert(BizObj.IsRegisteredEditableChildObject(newIterationReasons));
			AssertEquals(newIterationReasons, BizObj.IterationReasons);
		}

		public void TestIterationReasonValidationList()
		{
			var workflowIterationReasons = new CategorisedWorkflowIterationReasons();
			var iterReasonValidationList = workflowIterationReasons.IterationReasonValidationList;

			AssertEquals(3, iterReasonValidationList.Count);

			AssertEquals(IterationReasonValidationList.Codes.Error, iterReasonValidationList[0].Code);
			AssertEquals("Error: adds an error when reason is not supplied", iterReasonValidationList[0].Description);

			AssertEquals(IterationReasonValidationList.Codes.Warning, iterReasonValidationList[1].Code);
			AssertEquals("Warning: adds a warning when reason is not supplied", iterReasonValidationList[1].Description);

			AssertEquals(IterationReasonValidationList.Codes.None, iterReasonValidationList[2].Code);
			AssertEquals("None: adds no warning or error when reason is not supplied", iterReasonValidationList[2].Description);
		}

		public void TestIterationReasonValidation()
		{
			var workflowIterationReasons = new CategorisedWorkflowIterationReasons();

			AssertNoErrors(workflowIterationReasons.IterationReasonValidationInfo);

			workflowIterationReasons.IterationReasonValidation = "BLA";
			AssertHasError(workflowIterationReasons.IterationReasonValidationInfo, "Please enter a valid value.");

			workflowIterationReasons.IterationReasonValidation = "";
			AssertHasError(workflowIterationReasons.IterationReasonValidationInfo, "Please enter a value.");

			workflowIterationReasons.IterationReasonValidation = IterationReasonValidationList.Codes.None;
			AssertNoErrors(workflowIterationReasons.IterationReasonValidationInfo);

			workflowIterationReasons.IterationReasonValidation = IterationReasonValidationList.Codes.Warning;
			AssertNoErrors(workflowIterationReasons.IterationReasonValidationInfo);

			workflowIterationReasons.IterationReasonValidation = IterationReasonValidationList.Codes.Error;
			AssertNoErrors(workflowIterationReasons.IterationReasonValidationInfo);
		}

		public override void TestCloneValues()
		{
			var boToClone = (CategorisedWorkflowIterationReasons)GetBusinessObjectToClone();
			var newObject = (CategorisedWorkflowIterationReasons)boToClone.Clone(null, null);

			Assert(!boToClone.IterationReasons.Equals(newObject.IterationReasons));
			AssertCloneValues(newObject);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("Custom MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedTypes = (CategorisedWorkflowIterationReasons)clone;
			AssertEquals(5, clonedTypes.CodeMaxLength);
			AssertEquals("WRK", clonedTypes.Code);
			AssertEquals("Work Item", clonedTypes.Description);

			AssertEquals(2, clonedTypes.IterationReasons.Count);
			AssertEquals("BLA", clonedTypes.IterationReasons[0].Code);
			AssertEquals("Give me a reason!", clonedTypes.IterationReasons[0].Description);
			AssertEquals("HEY", clonedTypes.IterationReasons[1].Code);
			AssertEquals("I still need a reason", clonedTypes.IterationReasons[1].Description);

			AssertEquals(IterationReasonValidationList.Codes.Warning, clonedTypes.IterationReasonValidation);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CategorisedWorkflowIterationReasons)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "WRK";
			result.Description = (NoResString)"Work Item";

			var iterationReason1 = result.IterationReasons.AddNew();
			iterationReason1.Code = "BLA";
			iterationReason1.Description = (NoResString)"Give me a reason!";

			var iterationReason2 = result.IterationReasons.AddNew();
			iterationReason2.Code = "HEY";
			iterationReason2.Description = (NoResString)"I still need a reason";

			result.IterationReasonValidation = IterationReasonValidationList.Codes.Warning;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		new CategorisedWorkflowIterationReasons BizObj
		{
			get { return (CategorisedWorkflowIterationReasons)base.BizObj; }
		}
	}
}
