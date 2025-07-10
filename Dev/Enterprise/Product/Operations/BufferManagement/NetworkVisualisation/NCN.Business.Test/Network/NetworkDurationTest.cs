using System;
using CargoWise.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NetworkDurationTest : NetworkTestCase
	{
		#region Default Test Data

		protected Lazy<NetworkViewModel> NetworkViewModelCreator { get; set; }

		protected NetworkViewModel NetworkViewModel => NetworkViewModelCreator.Value;
		protected IJobNetwork Network => NetworkViewModel.GetJobNetwork();

		protected ShapeNetworkEntity Diagram => Network.DiagramEntity;

		protected override void SetUp()
		{
			base.SetUp();

			UseScaledDiagram();
		}

		void UseScaledDiagram()
		{
			UseDiagram(CreateDiagram(Factory, name: "Root", isScaled: true));
		}

		void UseUnscaledDiagram()
		{
			UseDiagram(CreateDiagram(Factory, name: "Unscaled Root"));
		}

		void UseDiagram(BMNCNShape diagram)
		{
			NetworkViewModelCreator = new Lazy<NetworkViewModel>(() => CreateNetworkViewModel(diagram));
		}

		#endregion

		#region Nonscaled 

		public void TestNotScaled()
		{
			UseUnscaledDiagram();

			AssertCoordinates(Diagram, width: 0, height: 0);

			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 0, 1);
			AssertCoordinates("Width is always 0 for unscaled shapes.", Diagram, width: 0, height: 0);
		}

		#endregion

		#region Width from Implicit Duration

		public void TestDefaultSize()
		{
			AssertCoordinates("Everything starts at zero.", Diagram, width: 0, height: 0);
		}

		public void TestDefaultSize_HasShapeWithWidth()
		{
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 0, 1);
			AssertCoordinates("Diagram width offset by shape.", Diagram, width: 300, height: 0);
		}

		public void TestDefaultSize_HasShapeWidthWidthAndXOffset()
		{
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 300, 1119);
			AssertCoordinates("Diagram width offset by shape.", Diagram, width: 600, height: 0);
		}

		public void TestDefaultSize_UseRightmostShape()
		{
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 300, 1119);
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 600, 1119);
			AssertCoordinates("Diagram width offset by rightmost shape.", Diagram, width: 900, height: 0);
		}

		public void TestDefaultSize_UseRightmostShape_ReverseOrder()
		{
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 600, 1119);
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 300, 1119);
			AssertCoordinates("Diagram width offset by rightmost shape.", Diagram, width: 900, height: 0);
		}

		public void TestDefaultSize_MovingRightmostShapeFallsBack()
		{
			NetworkViewModel.CreateNewShape(Diagram).SetCoordinates(300, 100, 300, 1119);
			var shape2 = NetworkViewModel.CreateNewShape(Diagram);
			shape2.SetCoordinates(300, 100, 600, 1119);
			AssertCoordinates("Diagram width offset by rightmost shape.", Diagram, width: 900, height: 0);
			shape2.SetCoordinates(300, 100, 0, 2);
			AssertCoordinates("Diagram width shrinks as child shape moves.", Diagram, width: 600, height: 0);
		}

		public void TestDefaultSize_DoNotChangeWhenCalculationSuspended()
		{
			var shape = NetworkViewModel.CreateNewShape(Diagram);
			shape.SetCoordinates(300, 100, 600, 1119);
			AssertCoordinates(Diagram, width: 900, height: 0);

			shape.IsCalculationSuspended = true;
			shape.X = 100;
			AssertCoordinates("Diagram width stays the same.", Diagram, width: 900, height: 0);

			shape.IsCalculationSuspended = false;
			AssertCoordinates("Diagram width changes.", Diagram, width: 400, height: 0);
		}

		public void TestDefaultSize_ShapeDeleted_NoShapesLeft()
		{
			var shape = NetworkViewModel.CreateNewShape(Diagram);
			shape.SetCoordinates(300, 100, 600, 1119);
			AssertCoordinates(Diagram, width: 900, height: 0);

			shape.Delete();
			AssertCoordinates(Diagram, width: 0, height: 0);
		}

		public void TestDefaultSize_ShapeDeleted_ShapeRemains()
		{
			var shape1 = NetworkViewModel.CreateNewShape(Diagram);
			var shape2 = NetworkViewModel.CreateNewShape(Diagram);
			shape1.SetCoordinates(300, 100, 600, 1119);
			shape2.SetCoordinates(300, 100, 300, 1119);
			AssertCoordinates(Diagram, width: 900, height: 0);

			shape1.Delete();
			AssertCoordinates("Fall back to the next biggest shape.", Diagram, width: 600, height: 0);
		}

		public void TestDefaultSize_IgnoreAnnotations()
		{
			NetworkViewModel.CreateNewAnnotation(Diagram).SetCoordinates(200, 200, 200, 200);
			AssertCoordinates("Scaled width is ignored because anotations do not impact it.", Diagram, width: 0, height: 0);
		}

		#endregion
	}
}
