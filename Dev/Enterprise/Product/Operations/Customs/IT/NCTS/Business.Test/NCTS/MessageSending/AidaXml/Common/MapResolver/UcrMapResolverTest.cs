using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class UcrMapResolverTest : TestCaseWithFactory
{
	#region Node Types

	const string NodeTypeBill = "Bill";
	const string NodeTypeHeader = "Header";
	const string NodeTypeItem = "Item";

	#endregion

	#region Tests

	public void TestUcrResolution() => TestCases.Select(
		(tc, i) => new { tc, i }).ForEach(v =>
			CombineAssertions($"Test case {v.i + 1}", () =>
				CreateAssertions(v.tc).ForEach(a => a())));

	#endregion

	#region Assertion Creation Subroutines

	List<Action> CreateAssertions(ResolverTestNode<string> node)
	{
		AssertEquals($"Node type should be '{NodeTypeHeader}'", NodeTypeHeader, node.NodeType);

		var header = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = header.MovementHeader;
		movementHeader.BM_UniqueConsignmentReference = node.Prop;

		List<Action> assertions =
		[
			() => AssertEquals(
				$"MovementHeader.BM_UniqueConsignmentReference '${movementHeader.BM_UniqueConsignmentReference}' should resolve to '${node.ExpectedResolvedProp}'",
				node.ExpectedResolvedProp, movementHeader.ResolveUcr()),
			() => AssertEquals(
				$"Header.MovementHeader.BM_UniqueConsignmentReference '${header.MovementHeader.BM_UniqueConsignmentReference}' should resolve to '${node.ExpectedResolvedProp}'",
				node.ExpectedResolvedProp, header.ResolveUcr())
		];

		node.Children.ForEach(c => assertions.AddRange(CreateAssertions(c, header)));
		AssertEquals($"Number of assertions ({assertions.Count}) should be equal to the number of nodes to test + 1", node.NodeCount() + 1, assertions.Count);
		return assertions;
	}

	List<Action> CreateAssertions(ResolverTestNode<string> node, NctsHeader header)
	{
		AssertEquals($"Node type should be '{NodeTypeBill}'", NodeTypeBill, node.NodeType);

		var bill = header.Bills.AddNew();
		bill.B0_ReferenceID = node.Prop;

		List<Action> assertions =
		[
			() => AssertEquals(
				$"Bill.B0_ReferenceID '${bill.B0_ReferenceID}' should resolve to '${node.ExpectedResolvedProp}'",
				node.ExpectedResolvedProp, bill.ResolveUcr())
		];

		node.Children.ForEach(c => assertions.Add(CreateAssertion(c, bill)));
		return assertions;
	}

	Action CreateAssertion(ResolverTestNode<string> node, NctsBill bill)
	{
		AssertEquals($"Node type should be '{NodeTypeItem}'", NodeTypeItem, node.NodeType);

		var goodsItem = bill.GoodsItems.AddNew();
		goodsItem.BY_CommercialReferenceNumber = node.Prop;

		return () => AssertEquals(
			$"GoodsItem.BY_CommercialReferenceNumber '${goodsItem.BY_CommercialReferenceNumber}' should resolve to '${node.ExpectedResolvedProp}'",
			node.ExpectedResolvedProp, goodsItem.ResolveUcr());
	}

	#endregion

	#region Test Case List

	List<ResolverTestNode<string>> TestCases =>
	[
		// Test case 1
		new ResolverTestNode<string>(NodeTypeHeader, string.Empty, "ES",
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty)),
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty))),
		// Test case 2
		new ResolverTestNode<string>(NodeTypeHeader, "NO", "ES",
			new ResolverTestNode<string>(NodeTypeBill, "FR", string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty)),
			new ResolverTestNode<string>(NodeTypeBill, "DE", string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "ES", string.Empty))),
		// Test case 3
		new ResolverTestNode<string>(NodeTypeHeader, "NO", string.Empty,
			new ResolverTestNode<string>(NodeTypeBill, "NO", string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "DE", "DE"),
				new ResolverTestNode<string>(NodeTypeItem, "ES", "ES")),
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "FR", "FR"),
				new ResolverTestNode<string>(NodeTypeItem, "ES", "ES"))),
		// Test case 4
		new ResolverTestNode<string>(NodeTypeHeader, "NO", string.Empty,
			new ResolverTestNode<string>(NodeTypeBill, "NO", "NO",
				new ResolverTestNode<string>(NodeTypeItem, "NO", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, string.Empty, string.Empty)),
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "FR", "FR"),
				new ResolverTestNode<string>(NodeTypeItem, "ES", "ES"))),
		// Test case 5
		new ResolverTestNode<string>(NodeTypeHeader, "FR", string.Empty,
			new ResolverTestNode<string>(NodeTypeBill, "NO", "NO",
				new ResolverTestNode<string>(NodeTypeItem, "NO", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "NO", string.Empty)),
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "NO", "NO"),
				new ResolverTestNode<string>(NodeTypeItem, string.Empty, "FR"))),
		// Test case 6
		new ResolverTestNode<string>(NodeTypeHeader, "FR", "NO",
			new ResolverTestNode<string>(NodeTypeBill, "NO", string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "NO", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, string.Empty, string.Empty)),
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "NO", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "NO", string.Empty))),
		// Test case 7
		new ResolverTestNode<string>(NodeTypeHeader, "PL", string.Empty,
			new ResolverTestNode<string>(NodeTypeBill, "ES", string.Empty,
				new ResolverTestNode<string>(NodeTypeItem, "PL", "PL"),
				new ResolverTestNode<string>(NodeTypeItem, "ES", "ES")),
			new ResolverTestNode<string>(NodeTypeBill, "PL", "IT",
				new ResolverTestNode<string>(NodeTypeItem, "IT", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "IT", string.Empty))),
		// Test case 8
		new ResolverTestNode<string>(NodeTypeHeader, "PL", string.Empty,
			new ResolverTestNode<string>(NodeTypeBill, string.Empty, "PL",
				new ResolverTestNode<string>(NodeTypeItem, string.Empty, string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, string.Empty, string.Empty)),
			new ResolverTestNode<string>(NodeTypeBill, "PL", "IT",
				new ResolverTestNode<string>(NodeTypeItem, "IT", string.Empty),
				new ResolverTestNode<string>(NodeTypeItem, "IT", string.Empty))),
	];

	#endregion
}

sealed class ResolverTestNode<T>(string nodeType, T prop, T expectedResolvedProp, params ResolverTestNode<T>[] children)
{
	public string NodeType { get; } = nodeType;
	public T Prop { get; } = prop;
	public T ExpectedResolvedProp { get; } = expectedResolvedProp;
	public ResolverTestNode<T>[] Children { get; } = children ?? [];

	public int NodeCount()
	{
		var nodeCount = 1;
		Children.ForEach(c => nodeCount += c.NodeCount());
		return nodeCount;
	}
}
