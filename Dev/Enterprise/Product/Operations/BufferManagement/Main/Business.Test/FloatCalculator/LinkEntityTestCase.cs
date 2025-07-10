using System;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestsSubclassesOf(typeof(ILinkEntity),
		new Type[0],
		new[] { typeof(ProcessJobHeader), })]
	public abstract class LinkEntityTestCase<T> : BMSTestCaseWithFactory
		where T : ILinkEntity
	{
		public abstract void TestAgreedDeliveryDateInUtc();
		public abstract void TestDisplayName();

		public virtual void TestIsLeaf()
		{
			var parent = CreateNonLeafEntity();

			AssertEquals(IsLeaf(parent), parent.IsLeaf);

			var child = CreateLeafEntity(parent);

			AssertEquals(IsLeaf(parent), parent.IsLeaf);
			AssertEquals(IsLeaf(child), child.IsLeaf);
		}

		public virtual void TestLinks()
		{
			var parent = CreateNonLeafEntity();
			var postreq = CreateNonLeafEntity();
			var child = CreateLeafEntity(parent);

			var dependencyLink = CreateDependencyLink(parent, postreq);

			AssertContainsExactElementsInAnyOrder(new[] { dependencyLink }, parent.Links);
			AssertContainsExactElementsInAnyOrder(new[] { dependencyLink }, postreq.Links);
			AssertContainsExactElementsInAnyOrder(Array.Empty<ILink>(), child.Links);
		}

		public virtual void TestParents()
		{
			var parent = CreateNonLeafEntity();
			var postreq = CreateNonLeafEntity();
			var child = CreateLeafEntity(parent);

			var dependencyLink = CreateDependencyLink(parent, postreq);

			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ILinkEntity>(), parent.Parents(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ILinkEntity>(), postreq.Parents(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(new[] { parent }, child.Parents(DescendantsStrategy));
		}

		public virtual void TestChildren()
		{
			var parent = CreateNonLeafEntity();
			var postreq = CreateNonLeafEntity();
			var child = CreateLeafEntity(parent);

			var dependencyLink = CreateDependencyLink(parent, postreq);

			AssertContainsExactElementsInAnyOrder(new[] { child }, parent.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ILinkEntity>(), postreq.Children(DescendantsStrategy));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ILinkEntity>(), child.Children(DescendantsStrategy));
		}

		protected abstract ILinkEntity CreateNonLeafEntity();
		protected abstract ILinkEntity CreateLeafEntity(ILinkEntity parent);

		protected abstract ILink CreateDependencyLink(ILinkEntity prerequisite, ILinkEntity postrequisite);

		protected abstract ILinkDescendantsStrategy DescendantsStrategy { get; }

		protected abstract bool IsLeaf(ILinkEntity entity);
	}
}
