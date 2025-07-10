using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using static Enterprise.DataTransfer.Native.Utils.Models.GraphNodeExtension;

namespace Enterprise.DataTransfer.Native.Utils.Models
{
	public class GraphNodeExtensionTest : TestCase
	{
		public void TestFindPath_UsesCache()
		{
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			a.ChildrenCollection.Add(b);

			var nodeA = new Mock<IGraphNode<MockGraphNode>>();
			var nodeB = new Mock<IGraphNode<MockGraphNode>>();

			nodeA.SetupGet(n => n.ID).Returns(a.ID);
			nodeB.SetupGet(n => n.ID).Returns(b.ID);
			nodeA.SetupGet(n => n.Self).Returns(a);
			nodeB.SetupGet(n => n.Self).Returns(b);

			var calculationCount = 0;

			Stack<MockGraphNode> PathCalculator()
			{
				calculationCount++;
				return new Stack<MockGraphNode>(new[] { a });
			}

			FindPathCache<MockGraphNode>.GetPath(a.ID, b.ID, PathCalculator);
			AssertEquals(1, calculationCount);
			FindPathCache<MockGraphNode>.GetPath(a.ID, b.ID, PathCalculator);
			AssertEquals(1, calculationCount);

			var pathFinderMock = new Mock<IPathFinder<MockGraphNode>>();
			pathFinderMock.Setup(p => p.GetPath(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Func<Stack<MockGraphNode>>>())).Returns(new Stack<MockGraphNode>());

			var result = nodeA.Object.Self.FindPath(nodeB.Object.Self, pathFinderMock.Object);

			pathFinderMock.Verify(p => p.GetPath(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Func<Stack<MockGraphNode>>>()), Times.Once);
			AssertNotNull(result);
		}

		public void TestReaderWriterLock()
		{
			var startReadEvent = new AutoResetEvent(false);
			var endWriteEvent = new AutoResetEvent(false);
			var dummyStack = new Stack<int>();

			var source = Guid.NewGuid();
			var target = Guid.NewGuid();

			FindPathCache<int>.GetPath(source, target, () => dummyStack);

			bool readIsDoneDuringWrite = true;

			var writerThread = new Thread(() => FindPathCache<int>.GetPath(Guid.NewGuid(), Guid.NewGuid(), () =>
			{
				startReadEvent.Set();
				readIsDoneDuringWrite = endWriteEvent.WaitOne(TimeSpan.FromSeconds(2));
				return dummyStack;
			}));

			writerThread.Start();

			startReadEvent.WaitOne();
			FindPathCache<int>.GetPath(source, target, () => dummyStack);
			endWriteEvent.Set();

			Assert(!readIsDoneDuringWrite);
		}

		public void TestIsRelative_CaseOne()
		{
			//    B -> A <- C
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");
			a.ChildrenCollection.Add(b);
			a.ChildrenCollection.Add(c);

			b.ParentCollection.Add(a);
			c.ParentCollection.Add(a);

			Assert(a.IsRelative(a));
			Assert(a.IsRelative(b));
			Assert(a.IsRelative(c));
			Assert(b.IsRelative(a));
			Assert(b.IsRelative(b));
			Assert(b.IsRelative(c));
			Assert(c.IsRelative(a));
			Assert(c.IsRelative(b));
			Assert(c.IsRelative(c));
		}

		public void TestIsRelative_CaseTwo()
		{
			//    A -> B -> C
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");
			a.ChildrenCollection.Add(b);
			b.ParentCollection.Add(a);
			b.ChildrenCollection.Add(c);
			c.ParentCollection.Add(b);

			Assert(a.IsRelative(a));
			Assert(a.IsRelative(b));
			Assert(a.IsRelative(c));
			Assert(b.IsRelative(a));
			Assert(b.IsRelative(b));
			Assert(b.IsRelative(c));
			Assert(c.IsRelative(a));
			Assert(c.IsRelative(b));
			Assert(c.IsRelative(c));
		}

		public void TestIsRelative_CaseThree()
		{
			//    A -> B -> C -> A
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");

			a.ChildrenCollection.Add(b);
			b.ParentCollection.Add(a);
			b.ChildrenCollection.Add(c);
			c.ParentCollection.Add(b);
			c.ChildrenCollection.Add(a);
			a.ParentCollection.Add(c);

			Assert(a.IsRelative(a));
			Assert(a.IsRelative(b));
			Assert(a.IsRelative(c));
			Assert(b.IsRelative(a));
			Assert(b.IsRelative(b));
			Assert(b.IsRelative(c));
			Assert(c.IsRelative(a));
			Assert(c.IsRelative(b));
			Assert(c.IsRelative(c));
		}

		public void TestIsRelative_CaseFour()
		{
			//    A <- B -> C
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");

			b.ChildrenCollection.Add(a);
			a.ParentCollection.Add(b);
			b.ChildrenCollection.Add(c);
			c.ParentCollection.Add(b);

			Assert(a.IsRelative(a));
			Assert(a.IsRelative(b));
			Assert(a.IsRelative(c));
			Assert(b.IsRelative(a));
			Assert(b.IsRelative(b));
			Assert(b.IsRelative(c));
			Assert(c.IsRelative(a));
			Assert(c.IsRelative(b));
			Assert(c.IsRelative(c));
		}

		public void TestFindPath_CaseOne()
		{
			var a = new MockGraphNode("A");

			var path = a.FindPath(a);

			AssertEquals(1, path.Count);
			Assert(path.Contains(a));
		}

		public void TestFindPath_CaseTwo()
		{
			//    A -> B
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");

			a.ChildrenCollection.Add(b);

			b.ParentCollection.Add(a);

			var path = a.FindPath(b);

			Assert(path.Count > 0);
			Assert(path.Contains(b));
			Assert(path.Contains(a));
		}

		public void TestFindPath_CaseThree()
		{
			//    A -> B -> C
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");

			a.ChildrenCollection.Add(b);
			b.ChildrenCollection.Add(c);

			c.ParentCollection.Add(b);
			b.ParentCollection.Add(a);

			var path = a.FindPath(c);
			Assert(path.Count > 0);
			Assert(path.Contains(c));
			Assert(path.Contains(a));

			path = c.FindPath(a);
			Assert(path.Count > 0);
			Assert(path.Contains(c));
			Assert(path.Contains(a));
		}

		public void TestFindPath_CaseFour()
		{
			//    A <- B -> C
			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");

			b.ChildrenCollection.Add(a);
			b.ChildrenCollection.Add(c);

			a.ParentCollection.Add(b);
			c.ParentCollection.Add(b);

			var path = a.FindPath(c);
			Assert(path.Count > 0);
			Assert(path.Contains(c));
			Assert(path.Contains(a));

			path = c.FindPath(a);
			Assert(path.Count > 0);
			Assert(path.Contains(c));
			Assert(path.Contains(a));
		}

		public void TestFindShortestPathWithSharedNodes1()
		{
			/*
			 *     A
			 *   / |
			 *  B  |
			 *   \ |
			 *     C
			 *     |
			 *     D
			 */

			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");
			var d = new MockGraphNode("D");

			a.ChildrenCollection.Add(b);
			a.ChildrenCollection.Add(c);
			b.ChildrenCollection.Add(c);
			c.ChildrenCollection.Add(d);

			var path = a.FindPath(d);

			AssertEquals(3, path.Count);
			AssertEquals(a, path.Pop());
			AssertEquals(c, path.Pop());
			AssertEquals(d, path.Pop());
		}

		public void TestFindShortestPathWithSharedNodes2()
		{
			/*
			 *     A
			 *   / |
			 *  B  |
			 *  |  D
			 *  C  |
			 *   \ |
			 *     E
			 *     |
			 *     F
			 */

			var a = new MockGraphNode("A");
			var b = new MockGraphNode("B");
			var c = new MockGraphNode("C");
			var d = new MockGraphNode("D");
			var e = new MockGraphNode("E");
			var f = new MockGraphNode("F");

			a.ChildrenCollection.Add(b);
			a.ChildrenCollection.Add(d);
			b.ChildrenCollection.Add(c);
			c.ChildrenCollection.Add(e);
			d.ChildrenCollection.Add(e);
			e.ChildrenCollection.Add(f);

			var path = a.FindPath(f);

			AssertEquals(4, path.Count);
			AssertEquals(a, path.Pop());
			AssertEquals(d, path.Pop());
			AssertEquals(e, path.Pop());
			AssertEquals(f, path.Pop());
		}
	}

	public class MockGraphNode : IGraphNode<MockGraphNode>, IEquatable<MockGraphNode>
	{
		public MockGraphNode(string name)
		{
			Name = name;
			ID = Guid.NewGuid();
		}
		public string Name { get; private set; }

		public Guid ID { get; set; }

		#region IGraphNode<MockGraphNode> Members

		public MockGraphNode Self { get; set; }

		public MockGraphNode Parent { get; set; }

		public IEnumerable<MockGraphNode> Parents
		{
			get { return ParentCollection; }
		}

		public List<MockGraphNode> ParentCollection
		{
			get { return parentCollection; }
		}
		readonly List<MockGraphNode> parentCollection = new List<MockGraphNode>();

		public IEnumerable<MockGraphNode> Children
		{
			get { return ChildrenCollection; }
		}

		public List<MockGraphNode> ChildrenCollection
		{
			get { return childrenCollection; }
		}
		readonly List<MockGraphNode> childrenCollection = new List<MockGraphNode>();

		#endregion

		#region IGraphNode Members

		IGraphNode IGraphNode.Self
		{
			get { return Self; }
		}

		IGraphNode IGraphNode.Parent
		{
			get { return Parent; }
		}

		IEnumerable<IGraphNode> IGraphNode.Parents
		{
			get { return Parents.OfType<IGraphNode>(); }
		}

		IEnumerable<IGraphNode> IGraphNode.Children
		{
			get { return Children.OfType<IGraphNode>(); }
		}

		Guid IGraphNode.ID
		{
			get { return iGraphNodeID; }
		}
		readonly Guid iGraphNodeID = Guid.NewGuid();

		#endregion

		#region IEquatable<MockGraphNode> Members

		public override string ToString()
		{
			return this.Name;
		}

		public bool Equals(MockGraphNode other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return this.Name == other.Name;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(MockGraphNode))
			{
				return false;
			}

			return Equals((MockGraphNode)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this.GetType().GetHashCode() ^ Name.GetHashCode());
			}
		}
		#endregion

	}
}
