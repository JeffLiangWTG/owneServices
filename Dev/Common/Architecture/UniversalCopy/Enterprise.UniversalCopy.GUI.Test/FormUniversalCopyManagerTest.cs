using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class FormUniversalCopyManagerTest : TestCaseWithFactory
	{
		public void TestModuleId()
		{
			using (ZForm form = new ZDummyForm())
			{
				form.DataSourceType = typeof(DummyBusinessObject);
				form.ControllerID = DummyControllerIDs.Dummy;

				using (var manager = new FormUniversalCopyManager(form))
				{
					AssertEquals(typeof(DummyBusinessObject), manager.ElementType);
					AssertEquals(DummyModuleIDs.Dummy, manager.ModuleId);
				}
			}
		}

		public void TestElementType()
		{
			using (var form = new ZForm())
			{
				form.DataSourceType = null;
				AssertNull(new FormUniversalCopyManager(form).ElementType);

				form.DataSourceType = typeof(DummyBusinessObject);
				using (var manager = new FormUniversalCopyManager(form))
				{
					AssertEquals(typeof(DummyBusinessObject), manager.ElementType);
				}
			}

			using (var form = new ZForm(Factory.New<DummyBusinessObjectWithList>()))
			{
				form.DataSourceType = typeof(DummyBusinessObject);
				using (var manager = new FormUniversalCopyManager(form))
				{
					AssertEquals(typeof(DummyBusinessObjectWithList), manager.ElementType);
				}
			}
		}

		public void TestAllowUniversalCopy()
		{
			using (var form = new ZForm())
			{
				form.DataSourceType = typeof(UniversalCopyManagerTest.DummyA);
				using (var copyManager = new FormUniversalCopyManager(form))
				{
					Assert("Do not allow as DummyA has no GLOW interface with description.", !copyManager.AllowsUniversalCopy);
				}

				form.DataSourceType = typeof(UniversalCopyManagerTest.DummyB);
				using (var copyManager = new FormUniversalCopyManager(form))
				{
					Assert("Allow as DummyB has GLOW interface with description.", copyManager.AllowsUniversalCopy);
				}

				form.DataSourceType = typeof(UniversalCopyManagerTest.DummyC);
				using (var copyManager = new FormUniversalCopyManager(form))
				{
					Assert("Allow as DummyC has UniversalCopyWithExtendedEntitiesAttribute applied.", copyManager.AllowsUniversalCopy);
				}
			}
		}

		public void TestAddMenuItems()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var copyManager = new FormUniversalCopyManager(form))
			{
				AssertNull("Not added yet", ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Universal Copy"));

				copyManager.AddMenuItems();
				AssertNotNull("Should be added added", ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Universal Copy"));
			}
		}

		public void TestCopyMenuClicked_TryGetCopyTargets()
		{
			var moduleId = DummyModuleIDs.Dummy;
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleId))
			using (var form = (ZForm)module.ShowEditForm(dummy))
			using (var manager = new FormUniversalCopyManagerForTest(form))
			{
				IEnumerable<BusinessObject> actualCopyTargets;
				AssertEquals(true, manager.CopyMenuClicked_TryGetCopyTargets_Exposed(out actualCopyTargets));
				AssertContainsExactElementsInAnyOrder(new[] { form.BusinessEntity }, actualCopyTargets);

				form.SetDataBinding(null, "");
				AssertEquals(false, manager.CopyMenuClicked_TryGetCopyTargets_Exposed(out actualCopyTargets));
				AssertNull(actualCopyTargets);
			}
		}

		public void TestCopyMenuClicked_OnNewElement()
		{
			using (var form = new ZDummyForm(Factory.New<DummyBusinessObject>()) { ControllerID = DummyControllerIDs.Dummy })
			using (var manager = new FormUniversalCopyManagerForTest(form))
			{
				var openFormCount = Application.OpenForms.Count;
				manager.CopyMenuClicked_OnNewElement_Exposed(null);
				AssertEquals("Shouldn't have opened a form", openFormCount, openFormCount);

				openFormCount = Application.OpenForms.Count;
				var newElement = Factory.New<DummyBusinessObject>();
				manager.CopyMenuClicked_OnNewElement_Exposed(newElement);
				AssertEquals("Should have opened a form", openFormCount + 1, Application.OpenForms.Count);
				AssertEquals(newElement, ((ZForm)Application.OpenForms[Application.OpenForms.Count - 1]).BusinessEntity);

				foreach (var dummyForm in Application.OpenForms.OfType<ZDummyForm>().ToArray())
				{
					dummyForm.Dispose();
				}
			}
		}

		public void TestCopyMenuClicked_GetSourceElement()
		{
			using (var form = new ZDummyForm(Factory.New<UniversalCopyDummy1>()) { ControllerID = DummyControllerIDs.Dummy })
			using (var manager = new FormUniversalCopyManagerForTest(form))
			{
				var dummy = manager.CopyMenuClicked_GetSourceElement_Exposed(form.BusinessEntity as BusinessObject);
				Assert(dummy is UniversalCopyDummy2);
			}
		}

		public void TestCopyMenuClicked_GetSourceElement_WithSourceMethod()
		{
			using (var form = new ZDummyForm(Factory.New<UniversalCopyDummy>()) { ControllerID = DummyControllerIDs.Dummy })
			using (var manager = new FormUniversalCopyManagerForTest(form))
			{
				var dummy = manager.CopyMenuClicked_GetSourceElement_Exposed(form.BusinessEntity as BusinessObject);
				AssertEquals("This is customized for Universal Copy.", (dummy as UniversalCopyDummy).Z0_Description);
			}
		}

		public void TestCopyMenuClicked_GetSourceElement_WithStmTemplateRecord()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = DummyModuleIDs.Dummy.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var dummy = Factory.New<DummyBusinessObjectForTest>();
			((ITemplateRecordProvider)dummy).LoadFromTemplateRecord(templateRecord);

			var templateRecordProvider = dummy as ITemplateRecordProvider;
			templateRecordProvider.TemplateRecord = templateRecord;

			using (var form = new ZDummyForm(dummy) { ControllerID = DummyControllerIDs.Dummy })
			using (var manager = new FormUniversalCopyManagerForTest(form))
			{
				var newDummy = manager.CopyMenuClicked_GetSourceElement_Exposed(form.BusinessEntity as BusinessObject);
				AssertNotNull("CopyMenuClicked_GetSourceElementd", newDummy);
			}
		}

		public void TestSensitiveDataUnreadable_WhenUniversalCopy()
		{
			var viewDeniedMessage = "** View Denied due to Security Access **";

			var staffWithoutViewOtherReferences = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutViewOtherReferences.GS_Title = "Driver";
			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherTitle.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutViewOtherReferences.PK;

			var staffModifyAll = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffModifyAll.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = true;
			deniedSecurityRecord.GU_GS = staffWithoutViewOtherReferences.PK;
			staffWithoutViewOtherReferences.GroupSecurityPermissionsCollectionForBinding.Add(staffModifyAll);

			var staff = Factory.New<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutViewOtherReferences.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var template = Factory.New<UniversalCopyTemplate>();
				template.S9_ModuleID = ModuleIDs.GlbStaff + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
				template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "TemplateForTest" }, template);
				template.S9_FilterName = "TemplateForTest";
				template.IsActive = true;
				var entityCopyNode = new EntityCopyTemplateNode();
				entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GlbStaffSchema.Constants.GS_Title, CopyMethod = CopyMethod.Copy });
				template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
				template.PrepareForSave();
				Factory.Save();

				using (var form = new GlbStaffForm(staff))
				using (var manager = new FormUniversalCopyManagerForTest(form))
				{
					var openFormCount = Application.OpenForms.Count;
					var bizos = new List<BusinessObject>
					{
						staff
					};

					manager.CopyMenuClicked_Exposed(template.CopyTemplateTree.CopyTemplateNode, bizos);
					AssertEquals("Should have opened a form", openFormCount + 1, Application.OpenForms.Count);
					var newStaff = ((ZForm)Application.OpenForms[Application.OpenForms.Count - 1]).BusinessEntity as GlbStaff;

					AssertEquals(viewDeniedMessage, newStaff.GS_Title);

					foreach (var staffForm in Application.OpenForms.OfType<GlbStaffForm>().ToArray())
					{
						staffForm.Dispose();
					}
				}
			}
		}

		class FormUniversalCopyManagerForTest : FormUniversalCopyManager
		{
			public FormUniversalCopyManagerForTest(ZForm form) : base(form) { }

			public bool CopyMenuClicked_TryGetCopyTargets_Exposed(out IEnumerable<BusinessObject> copyTargets)
			{
				return CopyMenuClicked_TryGetCopyTargets(out copyTargets);
			}

			public void CopyMenuClicked_OnNewElement_Exposed(BusinessObject newElement)
			{
				CopyMenuClicked_OnNewElement(newElement);
			}

			public BusinessObject CopyMenuClicked_GetSourceElement_Exposed(BusinessObject selectedElement)
			{
				return CopyMenuClicked_GetSourceElement(selectedElement);
			}

			public void CopyMenuClicked_Exposed(CopyTemplateTree configurationTree, IEnumerable<BusinessObject> selectedBizos)
			{
				CopyMenuClicked(configurationTree, selectedBizos);
			}
		}

		[UniversalCopyInstanceType(InstanceType = typeof(DummyBusinessObjectForTest), CreationMethod = "NewForUniversalCopy", GetSourceMethod = "GetSourceForUniversalCopy", ShouldSyncTreeNodes = true)]
		class DummyBusinessObjectForTest : DummyBusinessObject, ITemplateRecordProvider
		{
			public DummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			void ITemplateRecordProvider.SaveToTemplateRecord()
			{
			}

			void ITemplateRecordProvider.LoadFromTemplateRecord(ITemplateRecord templateRecord)
			{
				TemplateRecord = (StmTemplateRecord)templateRecord;
				((ITemplateRecordProvider)this).IsTemplateRecord = true;
			}

			bool ITemplateRecordProvider.IsTemplateRecord { get; set; }

			ITemplateRecord ITemplateRecordProvider.TemplateRecord
			{
				get => TemplateRecord;
				set => TemplateRecord = (StmTemplateRecord)value;
			}

			public StmTemplateRecord TemplateRecord { get; set; }

			BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord)
			{
				var otherElement = factory.New(elementType);
				var otherTemplateRecordProvider = (ITemplateRecordProvider)otherElement;
				otherTemplateRecordProvider.LoadFromTemplateRecord(templateRecord);
				return otherElement;
			}

			public string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree) => null;
			protected BusinessObject GetSourceForUniversalCopy() => null;
			protected object NewForUniversalCopy(BusinessObjectFactory factory, object parentEntity) => null;
		}
	}
}
