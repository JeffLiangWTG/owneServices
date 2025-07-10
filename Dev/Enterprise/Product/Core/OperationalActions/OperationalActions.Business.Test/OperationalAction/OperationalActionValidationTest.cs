using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateMeaningfulAction()
		{
			const string error1 = "This action is not meaningful. Please add a document, defined process, field or event.";
			const string error2 = "This action is not meaningful. Please add a defined process, field or event.";
			Action.FieldDescriptors.RemoveAndDeleteAll();
			Action.MethodDescriptors.RemoveAndDeleteAll();
			Action.DocumentPivots.RemoveAndDeleteAll();
			Action.SU_SE_NKDocumentEvent = "";
			Action.RunPreSaveValidation();
			AssertHasRowError(Action, error1);
			Action.SU_SE_NKDocumentEvent = Events.Arrival.Code;
			Action.RunPreSaveValidation();
			AssertNoRowError(Action, error1);
			Action.SU_SE_NKDocumentEvent = "";
			Action.DocumentPivots.AddNew();
			Action.RunPreSaveValidation();
			AssertNoRowError(Action, error1);
			Action.DocumentPivots.RemoveAndDeleteAll();
			Action.FieldDescriptors.AddNew();
			Action.RunPreSaveValidation();
			AssertNoRowError(Action, error1);
			Action.FieldDescriptors.RemoveAndDeleteAll();
			Action.MethodDescriptors.AddNew();
			Action.RunPreSaveValidation();
			AssertNoRowError(Action, error1);
			actionSupporter = new MockOperationalActionSupportable(typeof(DummyBusinessObject)).OperationalActionSupporter;
			context = null;
			action = null;
			Action.FieldDescriptors.RemoveAndDeleteAll();
			Action.MethodDescriptors.RemoveAndDeleteAll();
			Action.DocumentPivots.RemoveAndDeleteAll();
			Action.SU_SE_NKDocumentEvent = "";
			Action.RunPreSaveValidation();
			AssertHasRowError(Action, error2);
		}

		public void TestValidateAll()
		{
			Action.SU_IsSystemDefined = true;
			using (Action.GetValidationSuspender())
			{
				Action.SU_Calc_IsPublished = false;
			}

			AssertNoErrors(Action);
			Action.Validation.ValidateAll();
			AssertHasErrors(Action.SU_Calc_IsPublishedInfo);
		}

		public void TestValidateSU_Calc_IsPublished()
		{
			Action.SU_Calc_IsPublished = false;
			Action.SU_IsSystemDefined = true;
			AssertHasError(Action.SU_Calc_IsPublishedInfo, "System-defined Operational Actions must be published.");
			Action.SU_Calc_IsPublished = true;
			AssertNoErrors(Action.SU_Calc_IsPublishedInfo);
			Action.SU_IsSystemDefined = false;
			Action.SU_Calc_IsPublished = false;
			AssertNoErrors(Action.SU_Calc_IsPublishedInfo);
		}

		public void TestValidateSU_MenuName()
		{
			Action.SU_MenuName = ZString.Empty;
			AssertHasError(Action.SU_MenuNameInfo, "Please enter an " + Action.SU_MenuNameInfo.Description + ".");
			Action.SU_MenuName = "x";
			AssertNoErrors(Action.SU_MenuNameInfo);
			OperationalActionContext context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			OperationalActionCollection actions = new OperationalActionCollection(Factory, context);
			actions.Add(Action);
			OperationalAction anotherAction = actions.AddNew();
			anotherAction.SU_MenuName = "x";
			AssertHasError(anotherAction.SU_MenuNameInfo, "The Menu Name and path has been duplicated and must be unique.");
			anotherAction.SU_MenuPath = "path";
			anotherAction.Validation.ValidateSU_MenuName();
			AssertNoErrors("SU_MenuName is " + anotherAction.SU_MenuName, anotherAction.SU_MenuNameInfo);
			Action.SU_MenuPath = "/path//";
			anotherAction.Validation.ValidateSU_MenuName();
			AssertHasError(anotherAction.SU_MenuNameInfo, "The Menu Name and path has been duplicated and must be unique.");
			anotherAction.SU_MenuName = "y";
			AssertNoErrors(anotherAction.SU_MenuNameInfo);
		}

		public void TestValidateSU_SE_NKDocumentEvent()
		{
			Action.SU_SE_NKDocumentEvent = "---";
			AssertHasError(Action.SU_SE_NKDocumentEventInfo, "Enter a valid Event.");
			Action.SU_SE_NKDocumentEvent = ZString.Empty;
			AssertNoErrors(Action.SU_SE_NKDocumentEventInfo);
			Action.SU_SE_NKDocumentEvent = Events.Arrival.Code;
			AssertNoErrors(Action.SU_SE_NKDocumentEventInfo);
		}

		public void TestValidateSU_FilterList()
		{
			Action.SU_FilterList = "XXX";
			AssertHasError(Action.SU_FilterListInfo, "unexpected end of expression");
			Action.SU_FilterList = "";
			AssertNoNotifications(Action.SU_FilterListInfo);
			Action.SU_FilterList = "XXX == \"YY\"";
			AssertHasError(Action.SU_FilterListInfo, "'XXX' is not a recognized constraint");
			Action.SU_FilterList = "Country == \"AU\"";
			AssertNoNotifications(Action.SU_FilterListInfo);
		}

		#region Implementation
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
