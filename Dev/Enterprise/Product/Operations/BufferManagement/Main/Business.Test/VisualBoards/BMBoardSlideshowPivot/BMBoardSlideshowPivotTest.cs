using System;
using CargoWise.Types;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSlideshowPivot))]
	public class BMBoardSlideshowPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var pivot = Factory.New<BMBoardSlideshowPivot>();
			AssertEquals((short)BoardSlideshowViewModel.DefaultBoardRefreshSeconds, pivot.MC_DurationInSeconds);
		}

		public void TestDefaultValues_WhenRegistryIsReallyBig()
		{
			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)((IntRegistryDataType)BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.DataType).UpperBound);

			var pivot = Factory.New<BMBoardSlideshowPivot>();
			AssertEquals(short.MaxValue, pivot.MC_DurationInSeconds);
		}

		public void TestSystemProperty()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			var pivot = Factory.New<BMBoardSlideshowPivot>();
			pivot.MC_MB_Board = board.PK;
			AssertEquals(ZGuid.Empty, pivot.SystemPK);

			pivot.OnLoaded();
			AssertEquals(system.PK, pivot.SystemPK);
		}
	}
}
