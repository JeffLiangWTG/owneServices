using System;
using System.Collections.Generic;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestsSubclassesOf(typeof(IBufferedItem),
		new Type[0],
		new[]
		{
			typeof(BMNCNBufferShape),
			typeof(BMNCNShapeDefaultDiagram),
			typeof(BMNCNRootDiagramShape),
		})]
	public abstract class BufferedItemTestCase : NetworkTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
		public void TestRelatedBuffers()
		{
			AssertContainsExactElementsInAnyOrder(GetExpectedRelatedBuffers(), BufferedItem.GetRelatedBuffers());
		}

		[TestDate(2017, 01, 18, 11, 21, 0)]
		public void TestStartableTime()
		{
			var expectedStartableTime = GetExpectedStartableTime();
			var expectedDateTime = expectedStartableTime.IsValid && !expectedStartableTime.IsEmpty ? expectedStartableTime.ToDateTime() : default(DateTime);

			AssertEquals(expectedDateTime, BufferedItem.StartableTime);
		}

		public void TestPlannedDurationInMinutes()
		{
			AssertEquals(GetExpectedPlannedDurationInMinutes(), BufferedItem.PlannedDurationInMinutes);
		}

		public void TestRemainingEstimateInMinutes()
		{
			AssertEquals(GetExpectedRemainingEstimateInMinutes(), BufferedItem.RemainingEstimateInMinutes);
		}

		IBufferedItem BufferedItem => bufferedItem ?? (bufferedItem = GetBufferedItem());
		IBufferedItem bufferedItem;

		protected abstract IBufferedItem GetBufferedItem();

		protected abstract IEnumerable<IBuffer> GetExpectedRelatedBuffers();

		protected abstract ZDateTime GetExpectedStartableTime();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected abstract int GetExpectedPlannedDurationInMinutes();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected abstract int GetExpectedRemainingEstimateInMinutes();
	}
}
