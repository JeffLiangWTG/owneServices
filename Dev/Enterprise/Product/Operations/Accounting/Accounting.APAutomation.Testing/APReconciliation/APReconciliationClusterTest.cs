using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using WiseRates.Tools;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public abstract class APReconciliationClusterTest : TestCaseWithFactory
	{
		public void TestAddNodes()
		{
			var mockLineProvider = new Mock<IAPReconciliationLineProvider>();
			var node1 = new APReconciliationNode("Node1", mockLineProvider.Object);
			var cluster = GetCluster(200M, "AUD", "AUD", new[] { node1 });
			AssertEquals(1, (cluster as IAPReconciliationCluster).Nodes.Count());

			var node2 = new APReconciliationNode("Node2", mockLineProvider.Object);
			cluster.AddNodes(node2);
			AssertEquals(2, (cluster as IAPReconciliationCluster).Nodes.Count());
		}

		public void TestReconciliation_SingleMatchingAccrual_LocalAmount()
		{
			const string localCurrency = "AUD";
			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = localCurrency, LocalExTaxAmount = 80M };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine1 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine2 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = "USD", OSExTaxAmount = 80M, LocalExTaxAmount = 120M };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine2 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(120M, clusterCurrency: localCurrency, localCurrency, new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { rLine2 }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Never);
		}

		public void TestReconciliation_SingleMatchingAccrual_OSAmount()
		{
			const string clusterCurrency = "USD";
			const string localCurrency = "AUD";

			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = clusterCurrency, OSExTaxAmount = 80M, LocalExTaxAmount = 120M };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine1 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine2 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = clusterCurrency, OSExTaxAmount = 60M, LocalExTaxAmount = 100M };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine2 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(80M, clusterCurrency, localCurrency, new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { rLine1 }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Never);
		}

		public void TestReconciliation_ForeignCurrencyError()
		{
			const string clusterCurrency = "USD";
			const string localCurrency = "AUD";

			var rLine1 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = localCurrency, OSExTaxAmount = 120M, LocalExTaxAmount = 120M };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine1 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine2 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = clusterCurrency, OSExTaxAmount = 80M, LocalExTaxAmount = 120M };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine2 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(120M, clusterCurrency, localCurrency, new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
			AssertEquals(AccDraftInvoiceProcessingErrors.Keys.ForeignCurrency, reconDetails.FailureReasonCode);
			AssertEquals(FormattableString.Invariant($"[{cluster.Key}]: Cannot reconcile foreign currency accruals unless the invoice is in the local currency"), reconDetails.FailureReason);
			AssertEquals(AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, reconDetails.FailureReasonAsLogableError.ErrorContext);

			AssertNull(reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Never);
		}

		public void TestReconciliation_IncludesEmptyCreditors()
		{
			const string localCurrency = "AUD";
			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = localCurrency, LocalExTaxAmount = 80.00m };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine1 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine2 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = localCurrency, LocalExTaxAmount = 120.00m };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine2 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(120M, clusterCurrency: localCurrency, localCurrency, new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { rLine2 }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Never);
		}

		public void TestReconciliation_IncludesSettlementGroupCreditors()
		{
			var rLine1 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = "AUD", LocalExTaxAmount = 80.00m };
			var rLine2 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = "AUD", LocalExTaxAmount = 120.00m };
			var mockLineProvider = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.SettlementGroupCreditors)).Returns(new[] { rLine1, rLine2 });
			var node = new APReconciliationNode("Node", mockLineProvider.Object);

			var mockAccrualFilterTypesProvider = new Mock<IAPReconciliationAccrualFilterTypesProvider>();
			mockAccrualFilterTypesProvider.Setup(m => m.GetFilterTypes()).Returns(new[]
			{
				APReconciliationAccrualFilterTypes.MatchingCreditor,
				APReconciliationAccrualFilterTypes.EmptyCreditor,
				APReconciliationAccrualFilterTypes.SettlementGroupCreditors
			});
			var cluster = GetCluster(120M, "AUD", "AUD", new[] { node }, mockAccrualFilterTypesProvider.Object) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { rLine2 }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Never);
		}

		public void TestReconciliation_IncludesAllOtherCreditors()
		{
			var rLine1 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = "AUD", LocalExTaxAmount = 80.00m };
			var rLine2 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid(), OSCurrency = "AUD", LocalExTaxAmount = 120.00m };
			var mockLineProvider = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.SettlementGroupCreditors)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.AllOtherCreditors)).Returns(new[] { rLine1, rLine2 });
			var node = new APReconciliationNode("Node", mockLineProvider.Object);

			var mockAccrualFilterTypesProvider = new Mock<IAPReconciliationAccrualFilterTypesProvider>();
			mockAccrualFilterTypesProvider.Setup(m => m.GetFilterTypes()).Returns(new[]
			{
				APReconciliationAccrualFilterTypes.MatchingCreditor,
				APReconciliationAccrualFilterTypes.EmptyCreditor,
				APReconciliationAccrualFilterTypes.SettlementGroupCreditors,
				APReconciliationAccrualFilterTypes.AllOtherCreditors
			});
			var cluster = GetCluster(120M, "AUD", "AUD", new[] { node }, mockAccrualFilterTypesProvider.Object) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { rLine2 }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Never);
		}

		public void TestReconciliation_OneCombinationFound()
		{
			var rLine1 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid() };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine1 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine2 = new APReconciliationLine { LineIdentifier = ZGuid.NewZGuid() };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine2 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(120M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			mockSummator.Setup(
				x => x.Sumup(
					It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine> { rLine1, rLine2 }, null, true))
					, 120M
					, It.IsAny<Func<APReconciliationLine, decimal>>()
				)
			).Returns(() => new List<(string, IEnumerable<APReconciliationLine>)> { ("AAA", new List<APReconciliationLine> { rLine1, rLine2 }) });

			var reconDetails = cluster.TryToReconcile(mockSummator.Object);
			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { rLine1, rLine2 }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(1));
		}

		public void TestReconciliation_MultipleCombinationsFound()
		{
			var rLine11 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 50m };
			var rLine12 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 60m };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine11, rLine12 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine21 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 150m };
			var rLine22 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 140m };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine21, rLine22 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(200M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			mockSummator.Setup(
			  x => x.Sumup(
				It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine> { rLine11, rLine12, rLine21, rLine22 }, null, true))
				, 200M
				, It.IsAny<Func<APReconciliationLine, decimal>>()
			  )
			).Returns(() => throw new APAReconciliationTooManyMatchesFoundException("too many matches"));

			var reconDetails = cluster.TryToReconcile(mockSummator.Object);
			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
			AssertEquals(AccDraftInvoiceProcessingErrors.Keys.MultipleCombinationsOfAccrualsFound, reconDetails.FailureReasonCode);
			AssertEquals(FormattableString.Invariant($"[{cluster.Key}]: Multiple matching accruals found"), reconDetails.FailureReason);
			AssertEquals(AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, reconDetails.FailureReasonAsLogableError.ErrorContext);
			AssertNull(reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(1));
		}

		public void TestReconciliation_WhenThereIsNoAccrual()
		{
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new List<APReconciliationLine>());
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new List<APReconciliationLine>());
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(150M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			mockSummator.Setup(
			  x => x.Sumup(
				It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine>(), null, true))
				, 150M
				, It.IsAny<Func<APReconciliationLine, decimal>>()
			  )
			).Returns(() => new List<(string, IEnumerable<APReconciliationLine>)>());

			var reconDetails = cluster.TryToReconcile(mockSummator.Object);
			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
			AssertEquals(AccDraftInvoiceProcessingErrors.Keys.NoAccrual, reconDetails.FailureReasonCode);
			AssertEquals(FormattableString.Invariant($"[{cluster.Key}]: No accruals found"), reconDetails.FailureReason);
			AssertEquals(AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, reconDetails.FailureReasonAsLogableError.ErrorContext);
			AssertNull(reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(0));
		}

		public void TestReconciliation_WhenTimeoutExceptionIsThrown()
		{
			var rLine11 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 50m };
			var rLine12 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 60m };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine11, rLine12 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine21 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 150m };
			var rLine22 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 140m };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine21, rLine22 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(200M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockAccrualSummator = new Mock<IAccrualSummator>();
			mockAccrualSummator.Setup(
				x => x.Sumup(
					It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine> { rLine11, rLine12, rLine21, rLine22 }, null, true))
					, 200M
					, It.IsAny<Func<APReconciliationLine, decimal>>()
					)
				).Returns(() => throw new APAReconciliationTimeoutException("timeout"));

			var reconDetails = cluster.TryToReconcile(mockAccrualSummator.Object);
			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
			AssertEquals(AccDraftInvoiceProcessingErrors.Keys.ReconciliationTimeout, reconDetails.FailureReasonCode);
			AssertEquals(FormattableString.Invariant($"[{cluster.Key}]: Reconciliation aborted due to time out"), reconDetails.FailureReason);
			AssertEquals(AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, reconDetails.FailureReasonAsLogableError.ErrorContext);
			AssertNull(reconDetails.ReconciliableAccruals);
			mockAccrualSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(1));
		}

		public void TestReconciliation_ThereAreAccrualsButNoCombinationAddsUpToClusterTotal()
		{
			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 30m };
			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider1.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine1 });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var rLine2 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid(), OSExTaxAmount = 70m };
			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor)).Returns(Array.Empty<APReconciliationLine>());
			mockLineProvider2.Setup(m => m.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor)).Returns(new[] { rLine2 });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(100m, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			mockSummator.Setup(
				x => x.Sumup(
					It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine> { rLine1, rLine2 }, null, true))
					, 100M
					, It.IsAny<Func<APReconciliationLine, decimal>>()
				)
			).Returns(() => new List<(string, IEnumerable<APReconciliationLine>)>());

			var reconDetails = cluster.TryToReconcile(mockSummator.Object);
			AssertEquals(ExpectedReconciliationResultWhenNoMatchFound, reconDetails.Result);
			AssertEquals(ExpectedFailureReasonWhenNoMatchFoundErrorCode, reconDetails.FailureReasonCode);
			AssertEquals(ExpectedFailureReasonWhenNoMatchFound, reconDetails.FailureReason);
			AssertEquals(ExpectedFailureReasonWhenNoMatchFound.IsNullOrEmpty() ? string.Empty : AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, reconDetails.FailureReasonAsLogableError?.ErrorContext ?? string.Empty);
			AssertNull(reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(1));
		}

		public void TestReconciliation_WhenClusterAmountIsZero()
		{
			var mockLineProvider = new Mock<IAPReconciliationLineProvider>();
			var node1 = new APReconciliationNode("Node1", mockLineProvider.Object);
			var node2 = new APReconciliationNode("Node2", mockLineProvider.Object);

			var cluster = GetCluster(0M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			mockSummator.Setup(
			  x => x.Sumup(
				It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine>(), null, true))
				, 0M
				, It.IsAny<Func<APReconciliationLine, decimal>>()
			  )
			).Returns(() => new List<(string, IEnumerable<APReconciliationLine>)>());
			var reconDetails = cluster.TryToReconcile(mockSummator.Object);
			AssertEquals(ExpectedReconciliationResultWhenAmountIsZero, reconDetails.Result);
			AssertEquals(ExpectedFailureReasonWhenAmountIsZero, reconDetails.FailureReason);
			Assert((reconDetails.ReconciliableAccruals != null) == (reconDetails.Result == APReconciliationResultTypes.Success));
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(0));
		}

		public void TestReconciliation_WhenNodeContainsDuplicatedLines()
		{
			var rLine = new APReconciliationLine() { LineIdentifier = new ZGuid("F668C664-4505-4FA5-B791-6938325C6F01"), OSExTaxAmount = 80.00m };
			var dummyResult = new APReconciliationLine() { LineIdentifier = new ZGuid("9A42360D-304A-4E57-90CE-956850B2930B"), OSExTaxAmount = 180.00m };

			var mockAccrualFilterTypeProvider = new Mock<IAPReconciliationAccrualFilterTypesProvider>();
			mockAccrualFilterTypeProvider.Setup(m => m.GetFilterTypes()).Returns(new[] { APReconciliationAccrualFilterTypes.MatchingCreditor });

			var mockLineProvider1 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider1.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine, rLine });
			var node1 = new APReconciliationNode("Node1", mockLineProvider1.Object);

			var mockLineProvider2 = new Mock<IAPReconciliationLineProvider>();
			mockLineProvider2.Setup(m => m.GetLines(It.IsAny<APReconciliationAccrualFilterTypes>())).Returns(new[] { rLine });
			var node2 = new APReconciliationNode("Node2", mockLineProvider2.Object);

			var cluster = GetCluster(160M, clusterCurrency: "AUD", localCurrency: "AUD", new[] { node1, node2 }, mockAccrualFilterTypeProvider.Object) as IAPReconciliationCluster;
			var mockSummator = new Mock<IAccrualSummator>();
			mockSummator.Setup(
				x => x.Sumup(
					It.Is<List<APReconciliationLine>>(accruals => accruals.SequenceEqualIgnoringOrder(new List<APReconciliationLine>() { rLine }, null, true))
					, 160M
					, It.IsAny<Func<APReconciliationLine, decimal>>()
				)
			).Returns(() => new List<(string, IEnumerable<APReconciliationLine>)>() { (string.Empty, new[] { dummyResult }) });

			var reconDetails = cluster.TryToReconcile(mockSummator.Object);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertEquals(string.Empty, reconDetails.FailureReason);
			AssertContainsExactElementsInAnyOrder(new[] { dummyResult }, reconDetails.ReconciliableAccruals);
			mockSummator.Verify(x => x.Sumup(It.IsAny<List<APReconciliationLine>>(), It.IsAny<decimal>(), It.IsAny<Func<APReconciliationLine, decimal>>()), Times.Exactly(1));
		}

		protected abstract APReconciliationCluster GetCluster(decimal clusterAmount, string clusterCurrency, string localCurrency, IEnumerable<APReconciliationNode> nodes, IAPReconciliationAccrualFilterTypesProvider accrualFilterTypesProvider = null);
		protected abstract APReconciliationResultTypes ExpectedReconciliationResultWhenNoMatchFound { get; }
		protected abstract string ExpectedFailureReasonWhenNoMatchFoundErrorCode { get; }
		protected abstract string ExpectedFailureReasonWhenNoMatchFound { get; }
		protected abstract APReconciliationResultTypes ExpectedReconciliationResultWhenAmountIsZero { get; }
		protected abstract string ExpectedFailureReasonWhenAmountIsZero { get; }
	}
}
