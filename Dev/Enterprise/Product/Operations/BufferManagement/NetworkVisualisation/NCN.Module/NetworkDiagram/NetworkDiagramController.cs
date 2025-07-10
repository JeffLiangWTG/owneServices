using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module
{
	public class NetworkDiagramController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public NetworkDiagramController()
			: this(DiagramType.NonScaled)
		{
		}

		public NetworkDiagramController(DiagramType newDiagramType)
		{
			this.newDiagramType = newDiagramType;
		}

		readonly DiagramType newDiagramType;

		#region ZController Overrides

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new NetworkDiagramForm((BMNCNShape)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.NetworkDiagram; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.NetworkDiagram; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMNCNShape); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();

			if (newDiagramType == DiagramType.Scaled)
			{
				diagram.SwitchToScaled();
				diagram.BNS_Name = Res.GetString("EE4F3403-4CF0-4030-928D-083C1C87BB5A", "New Scaled Diagram");
			}

			return diagram;
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.NetworkDiagramDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.NetworkDiagramEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NetworkDiagramNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NetworkDiagram; }
		}

		#endregion
	}
}
