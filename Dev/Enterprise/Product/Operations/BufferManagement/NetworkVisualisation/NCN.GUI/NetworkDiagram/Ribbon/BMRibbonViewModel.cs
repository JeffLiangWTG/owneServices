using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class BMRibbonViewModel : DefaultNetworkRibbonViewModel
	{
#pragma warning disable CS0618
		public BMRibbonViewModel(NetworkViewModel networkViewModel, NetworkUserControl control)
			: base(networkViewModel, control)
		{
			if (networkViewModel == null || !HasResources())
			{
				IsSuccessfullyConstructed = false;
				return;
			}

			var homeInsertGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("DefaultRibbon|Home|Insert", "Insert"), (NoResString)"Shape"); // This is a resource identifier
			HomeTab.Groups.InsertAfterItem(HomeActionsGroup, homeInsertGroup);

			homeInsertGroup.Items.Add(new RibbonButtonViewModel(this, "BMRibbon|Home|Insert|Shape", new CreateShapeAction(networkViewModel))); // This is a resource identifier
			homeInsertGroup.Items.Add(new RibbonButtonViewModel(this, "BMRibbon|Home|Insert|Annotation", new CreateAnnotationAction(networkViewModel))); // This is a resource identifier
			homeInsertGroup.Items.Add(new RibbonMenuButtonViewModel(this, "BMRibbon|Home|Insert|Job", new CreateJobActionCollection(networkViewModel))); // This is a resource identifier
			homeInsertGroup.Items.Add(new RibbonButtonViewModel(this, "BMRibbon|Home|Insert|Workflow", new CreateWorkflowAction(networkViewModel))); // This is a resource identifier

			var homeLinkedEntityGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|LinkedEntity", "Linked Entity"), (NoResString)"Link"); // This is a resource identifier
			HomeTab.Groups.InsertAfterItem(HomeEditRemoveGroup, homeLinkedEntityGroup);
			homeLinkedEntityGroup.Items.Add(new RibbonButtonViewModel(this, RibbonImageLayout.SmallImageOnly, new OpenLinkedEntityAction(networkViewModel))); // This is a resource identifier
			homeLinkedEntityGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|LinkedEntity|LinkWorkflow", "Link Workflow"), RibbonImageLayout.SmallImageOnly, new LinkToWorkflowAction(networkViewModel))); // This is a resource identifier
			homeLinkedEntityGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|LinkedEntity|LinkDiagram", "Link Diagram"), RibbonImageLayout.SmallImageOnly, new LinkToDiagramAction(networkViewModel))); // This is a resource identifier
			homeLinkedEntityGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|LinkedEntity|LinkFromClipboard", "Link from Clipboard"), RibbonImageLayout.SmallImageOnly, new LinkFromClipboardAction(networkViewModel))); // This is a resource identifier
			homeLinkedEntityGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|LinkedEntity|UnlinkEntity", "Unlink Entity"), RibbonImageLayout.SmallImageOnly, new UnlinkEntityAction(networkViewModel))); // This is a resource identifier

			var homeStatusGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|Status", "Status"), (NoResString)"Status"); // This is a resource identifier
			HomeTab.Groups.InsertAfterItem(homeLinkedEntityGroup, homeStatusGroup);
			homeStatusGroup.Items.Add(new RibbonMenuButtonViewModel(this, new SetStatusAction(networkViewModel))); // This is a resource identifier

			var miscGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Home|Misc", ""), (NoResString)"Shape"); // This is a resource identifier
			HomeTab.Groups.InsertAfterItem(HomeAffinitiesGroup, miscGroup);
			miscGroup.Items.Add(new RibbonButtonViewModel(this, new ValidateWorkflowLoopsAction(networkViewModel)));
			miscGroup.Items.Add(new RibbonToggleButtonViewModel(this, new ShowHideShapeInspectorAction(networkViewModel)));

			var diagramCloningGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Diagram|Cloning", "Cloning"), (NoResString)"Copy"); // This is a resource identifier
			DiagramTab.Groups.InsertAtStart(diagramCloningGroup);
			diagramCloningGroup.Items.Add(new RibbonButtonViewModel(this, new CloneDiagramAction(networkViewModel))); // This is a resource identifier
			diagramCloningGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Diagram|Cloning|OpenLinkedEntity", "Create a Scaled Copy"), new SwitchToScaledModeAction(networkViewModel))); // This is a resource identifier
			diagramCloningGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Diagram|Cloning|ExtendedDiagram", "Create an Extended Diagram"), new ExtendedNetworkAction(networkViewModel))); // This is a resource identifier

			var shapeListGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Diagram|ShapeList", "Shape List"), (NoResString)"Shape"); // This is a resource identifier
			DiagramTab.Groups.Add(shapeListGroup);
			shapeListGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Diagram|ShapeList|Open", "Open Shape List"), new OpenShapeListAction(networkViewModel))); // This is a resource identifier

			var shapeConversionGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Shape|Conversion", "Conversion"), (NoResString)"Shape"); // This is a resource identifier
			ShapeTab.Groups.InsertAtStart(shapeConversionGroup);
			shapeConversionGroup.Items.Add(new RibbonButtonViewModel(this, new ConvertToWorkflowsAction(networkViewModel))); // This is a resource identifier

			var shapeViewGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Shape|View", "View"), "PopOut"); // This is a resource identifier
			ShapeTab.Groups.Add(shapeViewGroup);
			shapeViewGroup.Items.Add(new RibbonButtonViewModel(this, new OpenAsDiagramAction(networkViewModel))); // This is a resource identifier

			var scheduleTab = new RibbonTabViewModel(ResString.GetMultilingualString("BMRibbon|Schedule", "Schedule"));
			Tabs.InsertAfterItem(ShapeTab, scheduleTab);

			var scheduleResourceDependenciesGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|ResourceDependencies", "Resource Dependencies"), "ArrowRight");
			scheduleTab.Groups.Add(scheduleResourceDependenciesGroup);

			scheduleResourceDependenciesGroup.Items.Add(new RibbonButtonViewModel(this,
				ResString.GetMultilingualString("BMRibbon|Schedule|ResourceDependencies|CreateResourceDependency", "Create Dependency"), new CreateResourceDependencyAction(networkViewModel, DependencyDirection.Uniform))); // This is a resource identifier
			scheduleResourceDependenciesGroup.Items.Add(new RibbonToggleButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|ResourceDependencies|ShowResourceDependency", "Show Dependency"), new ToggleResourceDependencyVisibilityAction(networkViewModel))); // This is a resource identifier

			var schedulePushEntitiesGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|PushEntities", "Push Entities"), "ArrowLeft"); // This is a resource identifier
			scheduleTab.Groups.Add(schedulePushEntitiesGroup);

			schedulePushEntitiesGroup.Items.Add(new RibbonButtonViewModel(this,
				ResString.GetMultilingualString("BMRibbon|Schedule|PushEntities|AsEarlyAsPossible", "As Early As Possible"),
				new PushAllEntitiesAction.PushEntityChildAction(networkViewModel, PushDirection.Early))); // This is a resource identifier
			schedulePushEntitiesGroup.Items.Add(new RibbonButtonViewModel(this,
				ResString.GetMultilingualString("BMRibbon|Schedule|PushEntities|AsLateAsPossible", "As Late As Possible"),
				new PushAllEntitiesAction.PushEntityChildAction(networkViewModel, PushDirection.Late))); // This is a resource identifier

			var scheduleApprovalGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|Approval", "Approval"), (NoResString)"Approve"); // This is a resource identifier
			scheduleTab.Groups.Add(scheduleApprovalGroup);

			scheduleApprovalGroup.Items.Add(new RibbonButtonViewModel(this, new ApproveDiagramAction(networkViewModel)));
			scheduleApprovalGroup.Items.Add(new RibbonButtonViewModel(this, new UnapproveDiagramAction(networkViewModel)));
			scheduleApprovalGroup.Items.Add(new RibbonButtonViewModel(this, new ApproveNonApprovedShapesAction(networkViewModel)));

			var schedulePinningGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|Pinning", "Pinning Shapes"), (NoResString)"Pin"); // This is a resource identifier
			scheduleTab.Groups.Add(schedulePinningGroup);

			//TODO: name here can be made dynamic to reflect the ancestor to pin to
			//will be done in the next workflow of WI00062123
			schedulePinningGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|Pinning|Pin", "Pin"), new PinShapeAction(networkViewModel))); // This is a resource identifier
			schedulePinningGroup.Items.Add(new RibbonButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|Pinning|Unpin", "Unpin"), new UnpinShapeAction(networkViewModel))); // This is a resource identifier

			var scheduleBuffersGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|Schedule|Buffers", "Buffers"), (NoResString)"Buffer"); // This is a resource identifier
			scheduleTab.Groups.Add(scheduleBuffersGroup);

			scheduleBuffersGroup.Items.Add(new RibbonButtonViewModel(this, new SuggestBufferAction(networkViewModel)));
			scheduleBuffersGroup.Items.Add(new RibbonButtonViewModel(this, new CreateProjectBufferAction(networkViewModel)));
			scheduleBuffersGroup.Items.Add(new RibbonMenuButtonViewModel(this, new AddBufferAction(networkViewModel)));
			scheduleBuffersGroup.Items.Add(new RibbonButtonViewModel(this, new AcceptBufferAction(networkViewModel)));
#if WINZOR
			var viewMiscGroup = new RibbonGroupViewModel(this, ResString.GetMultilingualString("BMRibbon|View|ChannelView", "ChannelView"), (NoResString)"ChannelView"); // This is a resource identifier
			ViewTab.Groups.Add(viewMiscGroup);
			viewMiscGroup.Items.Add(new RibbonToggleButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|View|Misc|FreezeChannelHeaders", "Freeze Channel Headers"), new FreezeChannelHeadersAction(networkViewModel, control))); // This is a resource identifier
			viewMiscGroup.Items.Add(new RibbonToggleButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|View|Misc|FreezeTimeLabels", "Freeze Time Labels"), new FreezeTimeLabelsAction(networkViewModel, control))); // This is a resource identifier
			viewMiscGroup.Items.Add(new RibbonMenuButtonViewModel(this, ResString.GetMultilingualString("BMRibbon|View|ChannelView|Channels", "Channels"), new ChannelViewAction(networkViewModel, control))); // This is a resource identifier
#endif
		}
#pragma warning restore CS0618

		protected override void LoadResources()
		{
			base.LoadResources();
#if !WINZOR
			// referencing the resource file by its full path using pack URIs (the resource file should be defined with Build Action set to Resource):
			// https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/pack-uris-in-wpf
			AddResourceFile((NoResString)@"pack://application:,,,/Enterprise.BufferManagement.NetworkVisualisation.GUI;component/NetworkDiagram/Ribbon/BMRibbonResources.xaml"); // This is a location of resources
#else
			AddResources(BMRibbonResources.Icons);
			AddResources(BMRibbonResources.IconsInBase64);
#endif
		}
	}
}
