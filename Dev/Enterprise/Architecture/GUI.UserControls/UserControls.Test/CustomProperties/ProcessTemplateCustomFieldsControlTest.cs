using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ProcessTemplateCustomFieldsControlTest : CustomPropertiesControlTest
	{
		public void TestOriginalCustomBOInChildrenShouldBeCleared()
		{
			var bo = Factory.New<DummyWithCustomFieldsWorkflow>();
			bo.Z0_Code = "abc";
			var cusBO = ((BusinessObject)bo).GetCustomBusinessObject(true);
			AssertEquals(1, ((IBusiness)bo).Children.OfType<CustomBusinessObject>().Count());
			using (var form = new DummyForm(bo))
			{
				form.Show();
				AssertEquals(0, ((IBusiness)bo).Children.OfType<CustomBusinessObject>().Count());
			}
		}

		public void TestAffectedPropertyProvider()
		{
			var bo = Factory.New<DummyWithWorkflow>();
			bo.Parent.Z0_Description = "AAA";
			using (var form = new DummyForm(bo))
			{
				form.Show();
				form.CustomFieldsControl.ResetDataBindingDone = false;
				bo.Parent.Z0_Description = "BBB";
				Assert("ProcessTemplateCustomFieldsControl should be refreshed", form.CustomFieldsControl.ResetDataBindingDone);
			}
		}

		public void TestAffectedPropertyProvider_NoTemplateChanged()
		{
			var bo = Factory.New<DummyWithWorkflow>();
			bo.Parent.Z0_Description = "AAA";
			using (var form = new DummyForm(bo))
			{
				form.Show();
				form.CustomFieldsControl.ResetDataBindingDone = false;
				bo.Parent.Z0_Description = "CCC";
				Assert("ProcessTemplateCustomFieldsControl should be refreshed", !form.CustomFieldsControl.ResetDataBindingDone);
			}
		}

		public void TestAffectedPropertyProvider_CustomFieldsControlDisposed()
		{
			var bo = Factory.New<DummyWithWorkflow>();
			bo.Parent.Z0_Description = "AAA";
			using (var form = new DummyForm(bo))
			{
				form.IsDisposedControlTest = true;
				form.Show();
				form.CustomFieldsControl.ResetDataBindingDone = false;
				bo.Parent.Z0_Description = "BBB";
				Assert("Disposed ProcessTemplateCustomFieldsControl should be refreshed", !form.CustomFieldsControl.ResetDataBindingDone);
			}
		}

		public void TestCustomLabelsConfigOrgProviderWorkflow_RefreshesWhenChanged()
		{
			var bo = Factory.New<DummyCustomLabelsConfigOrgProviderWorkflow>();
			using (var form = new DummyForm(bo))
			{
				form.IsDisposedControlTest = true;
				form.Show();
				form.CustomFieldsControl.ResetDataBindingDone = false;
				bo.ConfigOrg = Factory.New<OrgHeader>();
				AssertEquals(true, form.CustomFieldsControl.ResetDataBindingDone);
				form.CustomFieldsControl.ResetDataBindingDone = false;
				bo.ConfigOrg = null;
				AssertEquals(true, form.CustomFieldsControl.ResetDataBindingDone);
			}
		}

		public void TestCustomFieldProviderWorkflow_RefreshesWhenPropertiesThatAffectWorkflowChanged()
		{
			var dummyLoader = new DummyWorkflowDescriptorLoader();
			using (ObjectFactory.Substitute<IWorkflowDescriptorLoader>(dummyLoader))
			{
				var bo = Factory.New<DummyWithCustomFieldsWorkflow>();
				bo.Z0_Code = "abc";
				using (var form = new DummyForm(bo))
				{
					form.Show();
					Assert("Precondition", bo.updatedCustomObject);
					bo.updatedCustomObject = false;
					form.CustomFieldsControl.ResetDataBindingDone = false;
					bo.Z0_Code = "xyz";
					Assert("Custom object recalculated when changing PropertiesThatAffectWorkflow", bo.updatedCustomObject);
				}
			}
		}

		#region Implementation

		class ProcessTemplateCustomFieldsControlForTesting : ProcessTemplateCustomFieldsControl
		{
			public ProcessTemplateCustomFieldsControlForTesting()
			{
				processTaskTemplateLoader = new DummyProcessTaskTemplateLoader();
			}
		}

		class DummyProcessTaskTemplateLoader : IProcessTaskTemplateLoader
		{
			readonly DummyProcessTaskTemplate dummyProcessTaskTemplateAC = new DummyProcessTaskTemplate();
			readonly DummyProcessTaskTemplate dummyProcessTaskTemplateB = new DummyProcessTaskTemplate();
			public IProcessTaskTemplate FindTemplateForScreenLayout(IWorkflowProviderCore host, bool ignoreCache)
			{
				if (host is DummyWithWorkflow workflow)
				{
					if (workflow.Parent.Z0_Description == "AAA" || workflow.Parent.Z0_Description == "CCC")
					{
						return dummyProcessTaskTemplateAC;
					}

					return dummyProcessTaskTemplateB;
				}
				return new DummyProcessTaskTemplate();
			}

			IProcessTaskTemplateMatches IProcessTaskTemplateLoader.FindMatches(IWorkflowProviderCore workflowProvider, bool ignoreCache, bool includeUniversalTemplates, bool includeOnlyUniversalTemplates)
			{
				return new ProcessTaskTemplateMatches(FindTemplateForScreenLayout(workflowProvider, ignoreCache));
			}
		}

		class DummyProcessTaskTemplate : IProcessTaskTemplate
		{
			public ICustomColumnDefinition[] CustomColumnDefinitions => Array.Empty<ICustomColumnDefinition>();
			public IFormCustomisationSettings FormCustomisationSettings => null;
			public ZString P0_CustomFieldFallback => FallbackTypeList.Codes.NeverFallback;
			public ZBlob P0_FormState => null;
			ZGuid IIdentified.Identifier => ZGuid.Empty;
		}

		class DummyWorkflowDescriptorLoader : IWorkflowDescriptorLoader
		{
			public IWorkflowDescriptor GetWorkflowDescriptor(ZString workflowType) => new DummyWorkflowDescriptor();
		}

		class DummyWorkflowDescriptor : IWorkflowDescriptor
		{
			public string Code => null;
			public IMultilingualString Description => null;
			public Type WorkflowProviderType => null;
			public string[] GetPropertiesThatAffectWorkflow() => new[] { "Z0_Code" };
			public bool GetLogIsValidForDateDefaulting(IStmALog log) => false;
			public BusinessObject[] GetUDFMacroDataContext(ITriggerConditions workflowItem, BusinessObject parent) => null;
			public BusinessObject GetBizOForTest(BusinessObjectFactory factory) => null;
		}

		class DummyForm : ZForm
		{
			public DummyForm(IDummyParentProvider bizo)
				: base(bizo)
			{
				CustomFieldsControl = new ProcessTemplateCustomFieldsControlForTesting();
				Controls.Add(CustomFieldsControl);
				bizo.Parent.Z0_DescriptionInfo.ValueChanged += Z0_DescriptionInfo_ValueChanged;
			}

			public bool IsDisposedControlTest;

			void Z0_DescriptionInfo_ValueChanged(object sender, EventArgs e)
			{
				if (IsDisposedControlTest)
				{
					Controls.Remove(CustomFieldsControl);
					CustomFieldsControl.Dispose();
				}
			}

			public readonly ProcessTemplateCustomFieldsControlForTesting CustomFieldsControl;
		}

		interface IDummyParentProvider
		{
			DummyBusinessObject Parent { get; }
		}

		internal class DummyWithWorkflow : DummyBusinessObject, IDummyParentProvider, IWorkflowProviderCore, IWorkflowAffectedPropertyProvider
		{
			public DummyWithWorkflow(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IWorkflowProviderCore Members

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			public ZString WorkflowType => "XXX";

			#endregion

			#region IWorkflowAffectedPropertyProvider Members

			public ZPropertyInfo[] PropertyThatAffectWorkflowChanged => new[] { Parent.Z0_DescriptionInfo };

			#endregion

			DummyBusinessObject fDummyBusinessObjectParent;
			public DummyBusinessObject Parent => fDummyBusinessObjectParent ?? (fDummyBusinessObjectParent = Factory.New<DummyBusinessObject>());
		}

		internal class DummyCustomLabelsConfigOrgProviderWorkflow : DummyBusinessObject, IDummyParentProvider, IWorkflowProviderCore, ICustomLabelsConfigOrgProvider
		{
			public DummyCustomLabelsConfigOrgProviderWorkflow(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			#region IWorkflowProviderCore Members

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			public ZString WorkflowType => "XXX";

			#endregion

			DummyBusinessObject fDummyBusinessObjectParent;
			public DummyBusinessObject Parent => fDummyBusinessObjectParent ?? (fDummyBusinessObjectParent = Factory.New<DummyBusinessObject>());
			public OrgHeader ConfigOrg
			{
				get
				{
					return configOrg;
				}
				set
				{
					if (configOrg != value)
					{
						configOrg = value;
						ConfigOrgChanged?.Invoke(null, new EventArgs());
					}
				}
			}
			OrgHeader configOrg;
			public event EventHandler ConfigOrgChanged;
		}

		[UserDefinedValues]
		internal class DummyWithCustomFieldsWorkflow : DummyBusinessObject, IDummyParentProvider, IWorkflowProviderCore, ICustomFieldProvider
		{
			public DummyWithCustomFieldsWorkflow(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			#region IWorkflowProviderCore Members

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			public ZString WorkflowType => "XXX";

			#endregion

			#region ICustomFieldProvider Members

			CustomBusinessObject customBizo;

			public virtual CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			{
				if (customBizo == null || shouldRefresh)
				{
					updatedCustomObject = true;
					var customPropertyCollection = new UserDefinedPropertyCollection(this);
					customBizo = new CustomBusinessObject(Factory, this, customPropertyCollection);
				}
				return customBizo;
			}

			#endregion

			public bool updatedCustomObject;
			DummyBusinessObject fDummyBusinessObjectParent;
			public DummyBusinessObject Parent => fDummyBusinessObjectParent ?? (fDummyBusinessObjectParent = Factory.New<DummyBusinessObject>());
		}

		#endregion
	}
}
