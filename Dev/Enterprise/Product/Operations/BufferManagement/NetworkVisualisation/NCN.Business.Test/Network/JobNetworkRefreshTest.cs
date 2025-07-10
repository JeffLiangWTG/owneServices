using System.Collections.Generic;
using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class JobNetworkRefreshTest : NetworkTestCase
	{
		public void TestCreateNewShape()
		{
			var shape = NetworkViewModel.CreateNewShape(Diagram);
			AssertRefreshes(new RefreshArgs(RefreshType.EntitiesReloaded), new RefreshArgs(RefreshType.EntityAdded, shape));
		}

		public void TestDeleteShape()
		{
			var shape = NetworkViewModel.CreateNewShape(Diagram);
			Refreshes.Clear();

			Network.DeleteEntity(shape);
			AssertRefreshes(new RefreshArgs(RefreshType.EntitiesReloaded), new RefreshArgs(RefreshType.EntityRemoved, shape));
		}

		public void TestHideShape()
		{
			var shape = NetworkViewModel.CreateNewShape(Diagram);
			Refreshes.Clear();

			Network.HideEntity(shape);
			AssertRefreshes(new RefreshArgs(RefreshType.EntitiesReloaded), new RefreshArgs(RefreshType.EntityRemoved, shape));
		}

		public void TestCreateRelationship()
		{
			var shape1 = NetworkViewModel.CreateNewShape(Diagram);
			var shape2 = NetworkViewModel.CreateNewShape(Diagram);
			Refreshes.Clear();

			Network.CreateRelationship(shape1, shape2);
			AssertRefreshes(new RefreshArgs(RefreshType.RelationshipAdded, shape1, shape2));
		}

		public void TestDeleteRelationship()
		{
			var shape1 = NetworkViewModel.CreateNewShape(Diagram);
			var shape2 = NetworkViewModel.CreateNewShape(Diagram);
			var relationship = Network.CreateRelationship(shape1, shape2);
			Refreshes.Clear();

			Network.DeleteRelationship(relationship);
			AssertRefreshes(new RefreshArgs(RefreshType.RelationshipRemoved, shape1, shape2));
		}

		[GuiTest]
		public void TestEditEntity()
		{
			Network.EditEntity(Diagram);
			AssertRefreshes(new RefreshArgs(RefreshType.EntityEdited, Diagram));
		}

		[TestDate(2019, 06, 01)]
		public void TestApprove()
		{
			var shape = (IProposedNetworkEntity)NetworkViewModel.CreateNewShape(Diagram);
			var shapeEntity = Network.Entities.GetInstance(shape);

			var propertiesUpdated = new List<string>();

			shapeEntity.PropertyChanged += (s, e) => propertiesUpdated.Add(e.PropertyName);

			Diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			Refreshes.Clear();

			new ApproveDiagramAction(NetworkViewModel).Execute();

			AssertEquals(1, Refreshes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { nameof(ShapeNetworkEntity.EntityState), nameof(NodeViewModel.StartDateReadableText), nameof(NodeViewModel.FinishDateReadableText) }, propertiesUpdated);
		}

		[TestDate(2019, 06, 01)]
		public void TestUnapprove()
		{
			var shape = (IProposedNetworkEntity)NetworkViewModel.CreateNewShape(Diagram);
			var shapeEntity = Network.Entities.GetInstance(shape);

			Diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			new ApproveDiagramAction(NetworkViewModel).Execute();

			Factory.Save();
			Refreshes.Clear();

			var propertiesUpdated = new List<string>();
			shapeEntity.PropertyChanged += (s, e) => propertiesUpdated.Add(e.PropertyName);

			new UnapproveDiagramAction(NetworkViewModel).Execute();

			AssertEquals(1, Refreshes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { nameof(ShapeNetworkEntity.EntityState) }, propertiesUpdated);
		}

		public void TestPinShape_ShouldRefreshEntityState()
		{
			var shape = (INetworkEntity)NetworkViewModel.CreateNewShape(Diagram);
			var shapeEntity = Network.Entities.GetInstance(shape);

			var propertiesUpdated = new List<string>();
			shapeEntity.PropertyChanged += (s, e) => propertiesUpdated.Add(e.PropertyName);

			Refreshes.Clear();

			shapeEntity.Shape.PinShape(NetworkViewModel);

			AssertEquals(true, shapeEntity.IsPinned);
			AssertContainsExactElementsInAnyOrder(new[] { nameof(ShapeNetworkEntity.EntityState) }, propertiesUpdated);

			Refreshes.Clear();
			propertiesUpdated.Clear();

			shapeEntity.Shape.UnPinShape(NetworkViewModel);

			AssertEquals(false, shapeEntity.IsPinned);
			AssertContainsExactElementsInAnyOrder(new[] { nameof(ShapeNetworkEntity.EntityState) }, propertiesUpdated);
		}

		public void TestCreateProjectBuffer()
		{
			var shape = (IProposedNetworkEntity)NetworkViewModel.CreateNewShape(Diagram);

			Network.SwitchToScaled();
			Refreshes.Clear();

			new CreateProjectBufferAction(NetworkViewModel).Execute();
			AssertRefreshes(new RefreshArgs(RefreshType.EntitiesReloaded), new RefreshArgs(RefreshType.RedrawDiagram));
		}

		public void TestChangeBackGround()
		{
			var shape = NetworkViewModel.CreateNewShape(Diagram);
			Refreshes.Clear();

			shape.BackColor = Color.Red;

			AssertRefreshes(new RefreshArgs(RefreshType.RedrawDiagram));
		}

		#region Implementation

		void AssertRefreshes(params RefreshArgs[] refreshes)
		{
			AssertEquals("Expect the same number of refreshes", refreshes.Length, Refreshes.Count);

			CombineAssertions(() =>
			{
				for (int i = 0; i < refreshes.Length; i++)
				{
					AssertEquals($"Expected refresh number {i} to have refresh Type", refreshes[i].RefreshType, Refreshes[i].RefreshType);

					AssertContainsExactElementsInAnyOrder($"Expected refresh number {i} to have entities", new NetworkEntityEqualityComparer(), t => t.Name, refreshes[i].Entities, Refreshes[i].Entities);
				}
			});
		}

		class NetworkEntityEqualityComparer : IEqualityComparer<INetworkEntity>
		{
			public bool Equals(INetworkEntity x, INetworkEntity y)
			{
				return x.EntityPK == y.EntityPK;
			}

			public int GetHashCode(INetworkEntity obj)
			{
				return obj.EntityPK.GetHashCode();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Diagram = CreateDiagram(Factory, name: "Root", isScaled: true);
			NetworkViewModel = CreateNetworkViewModel(Diagram);
			Network = NetworkViewModel.GetJobNetwork();
			Refresher = Network.Refresher;
			Refreshes = new List<RefreshArgs>();

			Refresher.Refreshed += (s, e) =>
			{
				Refreshes.Add(e);
			};
		}

		BMNCNShape Diagram { get; set; }
		NetworkViewModel NetworkViewModel { get; set; }
		IJobNetwork Network { get; set; }
		INetworkRefresher Refresher { get; set; }
		List<RefreshArgs> Refreshes { get; set; }

		#endregion
	}
}
