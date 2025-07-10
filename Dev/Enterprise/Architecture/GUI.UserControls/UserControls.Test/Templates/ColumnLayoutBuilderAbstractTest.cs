using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestsSubclassesOf(typeof(ColumnLayoutBuilder<,>))]
	public abstract class ColumnLayoutBuilderAbstractTest<TColumnLayoutBuilder, T, BagT> : TestCaseWithFactory
		where TColumnLayoutBuilder : ColumnLayoutBuilder<T, BagT>
		where T : BusinessObject
		where BagT : IControlBag
	{
		public void TestCommonBag()
		{
			var commonBag = ColumnLayoutBuilderForTesting.CommonBag;
			var expectedType = typeof(BagT);
			CombineAssertions(() =>
			{
				AssertType("Correct Type", expectedType, commonBag);
				AssertSame(expectedType.Name + ".Instance", expectedType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null), commonBag);
			});
		}

		public void TestMaxColumns()
		{
			AssertEquals("MaxColumns", ExpectedMaxColumns, ColumnLayoutBuilderForTesting.MaxColumns);
		}

		public void TestCaptionWidth()
		{
			AssertEquals("CaptionWidth", ExpectedCaptionWidth, ColumnLayoutBuilderForTesting.CaptionWidth);
		}

		public void TestNarrowColumnForMediumControls()
		{
			AssertEquals("NarrowColumnForMediumControls", ExpectedNarrowColumnForMediumControls, ColumnLayoutBuilderForTesting.NarrowColumnForMediumControls);
		}

		public void TestTabSequence()
		{
			AssertEquals("TabSequence", ExpectedTabSequence, ColumnLayoutBuilderForTesting.TabSequence);
		}

		protected virtual ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

		protected virtual int ExpectedMaxColumns => 2;

		protected virtual PanelLayoutTabSequence ExpectedTabSequence => PanelLayoutTabSequence.ColumnWise;

		protected virtual bool ExpectedNarrowColumnForMediumControls => false;

		protected abstract TColumnLayoutBuilder GetColumnLayoutBuilderForTesting();

		protected TColumnLayoutBuilder ColumnLayoutBuilderForTesting => columnLayoutBuilderForTesting ?? (columnLayoutBuilderForTesting = GetColumnLayoutBuilderForTesting());
		TColumnLayoutBuilder columnLayoutBuilderForTesting;
	}
}
