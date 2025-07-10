using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalAction))]
	sealed class OperationalActionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMenunameMultilingualProperty()
		{
			var translatableActionMenuItem = Factory.New<StmMenuItem>();
			translatableActionMenuItem.SU_MenuName = "TranslatableAction";
			translatableActionMenuItem.SU_MenuType = "ACT";
			translatableActionMenuItem.SU_BusinessContext = Environment.Env.CurrentUser.Initials;
			translatableActionMenuItem.SU_GS_NKStaffCode = "";
			Factory.Save();
			var normalOperationAction = Factory.New<OperationalAction>();
			normalOperationAction.SU_MenuName = "EnglishOnlyAction";
			var translatableOperationalAction = Factory.Load<OperationalAction>(translatableActionMenuItem.PK);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				string key;
				mockRes.Put(key = translatableActionMenuItem.SU_MenuNameInfo.CustomizableDataResourceStrings.Source.GetKey(null, "TranslatableAction"), new ResourceStringData(key, "This is the Chinese Translation"));
				AssertEquals("EnglishOnlyAction", normalOperationAction.MenuNameMultilingual);
				AssertEquals("This is the Chinese Translation", translatableOperationalAction.MenuNameMultilingual);
			}
		}

		public void TestEnsureFiltersForMethod()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			OperationalActionMethod method1 = ActionSupporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithLargeGUI);
			OperationalActionMethod method2 = ActionSupporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithOversizedGUI);
			Action.SU_FilterList = "";
			Action.EnsureFiltersForMethod(method1);
			AssertEquals(@"country in (""AU"", ""NZ"", ""SG"")", Action.SU_FilterList);
			Action.SU_FilterList = @"country == ""NZ""";
			Action.EnsureFiltersForMethod(method1);
			AssertEquals(@"country == ""NZ""", Action.SU_FilterList);
			Action.SU_FilterList = @"country == ""NZ"" || country == ""GB""";
			Action.EnsureFiltersForMethod(method1);
			AssertEquals(@"country in (""AU"", ""NZ"", ""SG"") && (country == ""NZ"" || country == ""GB"")", Action.SU_FilterList);
			Action.SU_FilterList = @"department == ""BRN""";
			Action.EnsureFiltersForMethod(method2);
			AssertEquals(@"country == ""AU"" && department == ""BRN""", Action.SU_FilterList);
		}

		public void TestSavingDeletedFieldDescriptors()
		{
			Action.FieldDescriptors.RemoveAndDeleteAll();
			OperationalActionFieldDescriptor descriptor = Action.FieldDescriptors.AddNew();
			Factory.Save();
			Action.FieldDescriptors.RemoveAndDelete(descriptor);
			AssertEquals("Action should have changes", true, Action.HasChanges);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OperationalAction actionInNewFactory = newFactory.Load<OperationalAction>(Action.PK);
			actionInNewFactory.Context = Action.Context;
			AssertEquals("Action should have been saved with the descriptor removed", 0, actionInNewFactory.FieldDescriptors.Count);
		}

		public void TestSavingDeletedMethodDescriptors()
		{
			Action.MethodDescriptors.RemoveAndDeleteAll();
			OperationalActionMethodDescriptor descriptor = Action.MethodDescriptors.AddNew();
			Factory.Save();
			Action.MethodDescriptors.RemoveAndDelete(descriptor);
			AssertEquals("Action should have changes", true, Action.HasChanges);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OperationalAction actionInNewFactory = newFactory.Load<OperationalAction>(Action.PK);
			actionInNewFactory.Context = Action.Context;
			AssertEquals("Action should have been saved with the descriptor removed", 0, actionInNewFactory.MethodDescriptors.Count);
		}

		public void TestFieldDescriptorsUpdatedByDataRefresh()
		{
			Action.FieldDescriptors.RemoveAndDeleteAll();
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OperationalAction actionInNewFactory = newFactory.Load<OperationalAction>(Action.PK);
			action.FieldDescriptors.AddNew().FieldName = "Blaticus";
			newFactory.Save();
			AssertEquals("New field should be available", 1, action.FieldDescriptors.Count);
			AssertEquals("New field should be correct field", "Blaticus", action.FieldDescriptors[0].FieldName);
		}

		public void TestMethodDescriptorsUpdatedByDataRefresh()
		{
			Action.MethodDescriptors.RemoveAndDeleteAll();
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OperationalAction actionInNewFactory = newFactory.Load<OperationalAction>(Action.PK);
			OperationalActionMethodDescriptor methodDescriptor = action.MethodDescriptors.AddNew();
			methodDescriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;
			newFactory.Save();
			AssertEquals("New field should be available", 1, action.MethodDescriptors.Count);
			AssertEquals("New field should be correct field", ActionMethodProviderIDs.DummyWithMethods.Guid, action.MethodDescriptors[0].MethodGroup);
			AssertEquals("New field should be correct field", TestingConstants.DummyActionMethodWithGUI, action.MethodDescriptors[0].MethodID);
		}

		public void TestActionSupportable()
		{
			Action.Context = null;
			AssertNull("ActionSupportable", Action.Context);
			AssertEquals("SU_BusinessContext", ZString.Empty, Action.SU_BusinessContext);
			Action.Context = Context;
			AssertEquals("ActionSupportable", Context, Action.Context);
			AssertEquals("SU_BusinessContext", MockOperationalActionSupportable.BusinessContext.ToString(), Action.SU_BusinessContext);
		}

		public void TestCanDelete()
		{
			ICanDelete canDelete = Action;
			Action.SU_IsSystemDefined = true;
			AssertEquals("CanDelete", false, canDelete.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "System-defined Operational Actions cannot be deleted.", canDelete.ReasonForNotAbleToDelete);
			Action.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("CanDelete", true, canDelete.CanDelete);
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = false;
			AssertEquals("CanDelete", true, canDelete.CanDelete);
		}

		public void TestDocumentPivots()
		{
			OperationalActionDocumentPivot documentPivot = Factory.New<OperationalActionDocumentPivot>();
			DocumentCommand document = Factory.New<DocumentCommand>();
			documentPivot.SF_SU_Inward = Action.PK;
			documentPivot.SF_SU_Outward = document.PK;
			AssertEquals("DocumentPivots.Count", 1, Action.DocumentPivots.Count);
			AssertEquals("DocumentPivots[0]", documentPivot, Action.DocumentPivots[0]);
			AssertEquals("IsRegisteredEditableChildObject(DocumentPivots)", true, Action.IsRegisteredEditableChildObject(Action.DocumentPivots));
		}

		public void TestDocumentPivotsAreDeletedTogether()
		{
			OperationalActionDocumentPivot documentPivot1 = Action.DocumentPivots.AddNew();
			OperationalActionDocumentPivot documentPivot2 = Action.DocumentPivots.AddNew();
			Action.Delete();
			AssertEquals("documentPivot1.IsDeleted", true, documentPivot1.IsDeleted);
			AssertEquals("documentPivot1.IsDeleted", true, documentPivot1.IsDeleted);
		}

		public void TestEditingMode()
		{
			OperationalActionMenuEditableHelperTest.TestEditingMode(Action);
			Action.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("DocumentPivots.EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, Action.DocumentPivots.EditingMode);
			Action.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("DocumentPivots.EditingMode", MenuEditingMode.AllowAll, Action.DocumentPivots.EditingMode);
		}

		public void TestEditingModeOnDeletedBusinessObject()
		{
			Action.Delete();
			AssertEquals(false, Action.ReadOnly);
		}

		public void TestFieldDescriptors()
		{
			OperationalActionFieldDescriptorCollection initialFieldDescriptors = Action.FieldDescriptors;
			AssertEquals("FieldDescriptors.Count", 0, initialFieldDescriptors.Count);
			AssertEquals("IsRegisteredEditableChildObject(initialFieldDescriptors)", true, Action.IsRegisteredEditableChildObject(initialFieldDescriptors));
			initialFieldDescriptors.AddNew().FieldName = "Moo";
			Action.SU_ActionDataUpdateBlob = initialFieldDescriptors.SaveToBlob();
			OperationalActionFieldDescriptorCollection newFieldDescriptors = Action.FieldDescriptors;
			AssertEquals("FieldDescriptors.Count", 1, newFieldDescriptors.Count);
			AssertEquals("FieldDescriptors[0].FieldName", "Moo", newFieldDescriptors[0].FieldName);
			AssertSame("newFieldDescriptors == initialFieldDescriptors", initialFieldDescriptors, newFieldDescriptors);
			AssertEquals("IsRegisteredEditableChildObject(newFieldDescriptors)", true, Action.IsRegisteredEditableChildObject(newFieldDescriptors));
		}

		public void TestMethodDescriptors()
		{
			ZGuid groupid = ZGuid.NewZGuid();
			OperationalActionMethodDescriptorCollection initialMethodDescriptors = Action.MethodDescriptors;
			AssertEquals("FieldDescriptors.Count", 0, initialMethodDescriptors.Count);
			AssertEquals("IsRegisteredEditableChildObject(initialMethodDescriptors)", true, Action.IsRegisteredEditableChildObject(initialMethodDescriptors));
			OperationalActionMethodDescriptor methodDescriptor1 = initialMethodDescriptors.AddNew();
			methodDescriptor1.MethodGroup = groupid;
			methodDescriptor1.MethodID = TestingConstants.DummyActionMethodWithGUI;
			Action.SU_ActionMenusAndMethodsBlob = initialMethodDescriptors.SaveToBlob();
			OperationalActionMethodDescriptorCollection newMethodDescriptors = Action.MethodDescriptors;
			AssertEquals("MethodDescriptors.Count", 1, newMethodDescriptors.Count);
			AssertEquals("MethodDescriptors[0].MethodGroup", groupid, newMethodDescriptors[0].MethodGroup);
			AssertEquals("MethodDescriptors[0].MethodName", TestingConstants.DummyActionMethodWithGUI, newMethodDescriptors[0].MethodID);
			AssertSame("newMethodDescriptors == initialMethodDescriptors", initialMethodDescriptors, newMethodDescriptors);
			AssertEquals("IsRegisteredEditableChildObject(newMethodDescriptors)", true, Action.IsRegisteredEditableChildObject(newMethodDescriptors));
		}

		public void TestLookups()
		{
			AssertEquals("Lookups.GetType()", typeof(OperationalActionLookups), Action.Lookups.GetType());
		}

		public void TestSU_ActionDataUpdateBlobIsUpdatedOnSave()
		{
			Factory.Save();
			AssertEquals("SU_ActionDataUpdateBlob", ZBlob.Empty, Action.SU_ActionDataUpdateBlob);
			Action.FieldDescriptors.AddNew().FieldName = "Moo";
			AssertEquals("SU_ActionDataUpdateBlob", ZBlob.Empty, Action.SU_ActionDataUpdateBlob);
			Factory.Save();
			AssertEquals("SU_ActionDataUpdateBlob", Action.FieldDescriptors.SaveToBlob(), Action.SU_ActionDataUpdateBlob);
			Action.FieldDescriptors.AddNew().FieldName = "Oink";
			Factory.Save();
			AssertEquals("SU_ActionDataUpdateBlob", Action.FieldDescriptors.SaveToBlob(), Action.SU_ActionDataUpdateBlob);
		}

		public void TestSU_ActionMenusAndMethodsBlobIsUpdatedOnSave()
		{
			ZGuid group1Id = ZGuid.NewZGuid();
			ZGuid group2Id = ZGuid.NewZGuid();
			Factory.Save();
			AssertEquals("SU_ActionMenusAndMethodsBlob", ZBlob.Empty, Action.SU_ActionMenusAndMethodsBlob);
			OperationalActionMethodDescriptor methodDescriptor1 = Action.MethodDescriptors.AddNew();
			methodDescriptor1.MethodGroup = group1Id;
			methodDescriptor1.MethodID = TestingConstants.DummyActionMethodWithGUI;
			AssertEquals("SU_ActionMenusAndMethodsBlob", ZBlob.Empty, Action.SU_ActionMenusAndMethodsBlob);
			Factory.Save();
			AssertEquals("SU_ActionMenusAndMethodsBlob", Action.MethodDescriptors.SaveToBlob(), Action.SU_ActionMenusAndMethodsBlob);
			OperationalActionMethodDescriptor methodDescriptor2 = Action.MethodDescriptors.AddNew();
			methodDescriptor2.MethodGroup = group2Id;
			methodDescriptor2.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			Factory.Save();
			AssertEquals("SU_ActionMenusAndMethodsBlob", Action.MethodDescriptors.SaveToBlob(), Action.SU_ActionMenusAndMethodsBlob);
		}

		public void TestSU_Calc_IsPublished()
		{
			AssertEquals("SU_Calc_IsPublished", true, Action.SU_Calc_IsPublished);
			AssertEquals("SU_GS_NKStaffCode", "", Action.SU_GS_NKStaffCode);
			Action.SU_Calc_IsPublished = false;
			AssertEquals("SU_Calc_IsPublished", false, Action.SU_Calc_IsPublished);
			AssertEquals("SU_GS_NKStaffCode", GlbStaff.CurrentUser.GS_Code, Action.SU_GS_NKStaffCode);
			Action.SU_Calc_IsPublished = true;
			AssertEquals("SU_Calc_IsPublished", true, Action.SU_Calc_IsPublished);
			AssertEquals("SU_GS_NKStaffCode", "", Action.SU_GS_NKStaffCode);
		}

		public void TestSU_MenuType()
		{
			AssertEquals("SU_MenuType", Core.Constants.StmMenuItemTypes.OperationalActions, Action.SU_MenuType);
		}

		public void TestValidation()
		{
			AssertEquals("Validation.GetType()", typeof(OperationalActionValidation), Action.Validation.GetType());
		}

		public void TestGettingMethodDescriptors_NoOrderValidationError()
		{
			Action.MethodDescriptors.RemoveAndDeleteAll();
			var methodDescriptor1 = Action.MethodDescriptors.AddNew();
			var methodDescriptor2 = Action.MethodDescriptors.AddNew();
			methodDescriptor1.Order = 2;
			methodDescriptor2.Order = 1;
			methodDescriptor1.MethodGroup = ActionMethodProviderIDs.General.Guid;
			methodDescriptor1.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			methodDescriptor2.MethodGroup = ActionMethodProviderIDs.General.Guid;
			methodDescriptor2.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedAction = newFactory.Load<OperationalAction>(Action.PK);
			loadedAction.Context = Context;
			var methodDescriptors = loadedAction.MethodDescriptors;
			AssertEquals(2, methodDescriptors.Count);
			AssertNoErrors(loadedAction.MethodDescriptors[0].OrderInfo);
			AssertNoErrors(loadedAction.MethodDescriptors[1].OrderInfo);
		}

		public void TestSetMethodDescriptorOrder_ShouldRevalidateRelatedOrder()
		{
			Action.MethodDescriptors.RemoveAndDeleteAll();
			var methodDescriptor1 = Action.MethodDescriptors.AddNew();
			var methodDescriptor2 = Action.MethodDescriptors.AddNew();
			methodDescriptor1.Order = 1;
			methodDescriptor2.Order = 2;
			methodDescriptor1.Order = 2;
			AssertHasErrors(methodDescriptor1.OrderInfo);
			AssertHasErrors(methodDescriptor2.OrderInfo);
			methodDescriptor1.Order = 1;
			AssertNoErrors(methodDescriptor1.OrderInfo);
			AssertNoErrors(methodDescriptor2.OrderInfo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			OperationalAction result = (OperationalAction)base.GetNewBusinessObject();
			result.Context = Context;
			return result;
		}

		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = (OperationalAction)GetNewBusinessObject();
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
