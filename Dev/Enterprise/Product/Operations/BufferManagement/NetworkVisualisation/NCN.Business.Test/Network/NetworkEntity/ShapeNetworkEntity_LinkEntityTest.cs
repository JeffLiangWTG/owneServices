using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ShapeNetworkEntity))]
	class ShapeNetworkEntity_LinkEntityTest : LinkEntityTestCase<ShapeNetworkEntity>
	{
		public override void TestDisplayName()
		{
			Assert(true);
		}

		public override void TestAgreedDeliveryDateInUtc()
		{
			Assert(true);
		}

		public override void TestParents()
		{
			var parent = CreateNonLeafEntity();
			var postreq = CreateNonLeafEntity();
			var child = CreateLeafEntity(parent);

			var dependencyLink = CreateDependencyLink(parent, postreq);

			AssertContainsExactElementsInAnyOrder(new[] { network.DiagramEntity }, parent.Parents(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { network.DiagramEntity }, postreq.Parents(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { parent }, child.Parents(DescendantsStrategy));
		}

		protected override ILinkDescendantsStrategy DescendantsStrategy
		{
			get { return new ShapeNetworkEntityDescendantsStrategy(); }
		}

		protected override ILink CreateDependencyLink(ILinkEntity prerequisite, ILinkEntity postrequisite)
		{
			var prereq = (ShapeNetworkEntity)prerequisite;
			var postreq = (ShapeNetworkEntity)postrequisite;

			return (NetworkAttachment)prereq.Network.CreateRelationship(prereq, postreq);
		}

		protected override ILinkEntity CreateLeafEntity(ILinkEntity parent)
		{
			return networkViewModel.CreateNewShape((ShapeNetworkEntity)parent);
		}

		protected override ILinkEntity CreateNonLeafEntity()
		{
			return networkViewModel.CreateNewShape(network.DiagramEntity);
		}

		protected override bool IsLeaf(ILinkEntity entity)
		{
			return !((ShapeNetworkEntity)entity).Children.Any();
		}

		protected override void SetUp()
		{
			base.SetUp();
			networkViewModel = NetworkTestCase.CreateNetworkViewModel(NetworkTestCase.CreateDiagram(Factory));
			network = networkViewModel.GetJobNetwork();
		}

		NetworkViewModel networkViewModel;
		IJobNetwork network;
	}
}
