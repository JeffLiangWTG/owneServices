using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ResolutionAndClosureBehaviourRegistryEditor))]
	public class ResolutionAndClosureBehaviourRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			var dataType = new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
			var editorInfo = new ResolutionAndClosureBehaviourRegistryEditorInfo
			(
				new MultilingualString[]
				{
					(NoResString)"Criticality",
					(NoResString)"Product"
				},
				(NoResString)"Enabled",
				null, true, false,
				new bool[] { false, false },
				new bool[] { false, true }
			);

			return new ResolutionAndClosureBehaviourRegistryEditor(dataType, editorInfo, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !(editorPane as ResolutionAndClosureBehaviourControl).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ResolutionAndClosureBehaviourControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var editorInfo = new ResolutionAndClosureBehaviourRegistryEditorInfo
			(
				new MultilingualString[]
				{
					(NoResString)"Criticality",
					(NoResString)"Product"
				},
				(NoResString)"Enabled",
				null, true, false,
				new bool[] { false, false },
				new bool[] { false, true }
			);

			return new ResolutionAndClosureBehaviourRegistryItem(
				"ResolutionAndClosureBehaviour",
				(NoResString)"Category",
				(NoResString)"Resolution and Closure Behaviour",
				(NoResString)"Configure the behaviour of the CLOSED (CLS) status and set number of days to auto-close from RESOLVED (RES) and PENDING CUSTOMER (PND)",
				RegistryStorageFlags.System,
				editorInfo,
				new ResolutionAndClosureBehaviourCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var allDescriptions = new MultilingualString[]
			{
				(NoResString)"All Criticalities that are not listed",
				(NoResString)"All Products that are not listed"
			};

			var codeLists = new CodeDescriptionPairList[]
			{
				new IncidentApprovalLookups(null).CriticalityList,
				new SupportIncidentLookups(new BusinessObjectFactory()).ProductList,
				null
			};

			var defaultValue = new ResolutionAndClosureBehaviourCollection(true, 3, 3, allDescriptions, codeLists);
			defaultValue.AddSystemChildren(defaultValue.Add("CR1", (NoResString)"All Criticalities that are not listed"));
			return new object[] { defaultValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
