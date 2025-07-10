using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZNumberRangeControlTest : TestCaseWithFactory
	{
		public void TestCreateNumberRangeControlsDecimal()
		{
			PrepareNumberRangeControl("Number Range (Decimal)");

			AssertEquals("filterStrip.Controls.Count", 6, filterStrip.Controls.Count);

			AssertEquals("The text must be and", "and", andLabel.Text);

			AssertEquals("fromCalcEdit.Decimals", DummyBizoSchema.Z0_Decimal.Scale, fromCalcEdit.Decimals);
			AssertEquals("fromCalcEdit.Decimals", 0, toCalcEdit.Decimals);

			AssertEquals("fromCalcEdit.CoreZ.BindToType", typeof(ZDecimal), GetBindToType(fromCalcEdit));
			AssertEquals("toCalcEdit.CoreZ.BindToType", typeof(ZDecimal), GetBindToType(toCalcEdit));
		}

		public void TestCreateNumberRangeControlsShort()
		{
			PrepareNumberRangeControl("Number Range (Short)");

			AssertEquals("fromCalcEdit.Decimals", 0, fromCalcEdit.Decimals);
			AssertEquals("fromCalcEdit.Decimals", 0, toCalcEdit.Decimals);

			AssertEquals("fromCalcEdit.CoreZ.BindToType", typeof(ZShort), GetBindToType(fromCalcEdit));
			AssertEquals("toCalcEdit.CoreZ.BindToType", typeof(ZShort), GetBindToType(toCalcEdit));
		}

		Type GetBindToType(ZCalcEdit calcEdit)
		{
			return (Type)typeof(ZCalcEditCore).GetProperty("BindToType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(calcEdit.Core, null);
		}

		public void TestCheckArrowButtonsAvailability()
		{
			PrepareNumberRangeControl("Number Range (Decimal)");

			var numberRangeFilter = (ModuleNumberRangeFilter)stripBizO["Number Range (Decimal)"];
			numberRangeFilter.MinValue = 1M;
			numberRangeFilter.MaxValue = 3M;
			propertySearchDropEdit.SelectItem("Equal to");
			propertySearchDropEdit.CommitBoundValue();
			Assert(propertySearchDropEdit.Visible);
			Assert(fromCalcEdit.Visible);
			Assert(!andLabel.Visible);
			Assert(!toCalcEdit.Visible);
			Assert(upButton.Visible);
			Assert(downButton.Visible);

			numberRangeFilter.Property1 = 2M;
			Assert(upButton.Enabled);
			Assert(downButton.Enabled);

			numberRangeFilter.Property1 = 3M;
			Assert(!upButton.Enabled);
			Assert(downButton.Enabled);

			numberRangeFilter.Property1 = 4M;
			Assert(!upButton.Enabled);
			Assert(!downButton.Enabled);

			numberRangeFilter.Property1 = -1M;
			Assert(!upButton.Enabled);
			Assert(!downButton.Enabled);

			numberRangeFilter.Property1 = 1M;
			Assert(upButton.Enabled);
			Assert(!downButton.Enabled);
		}

		public void TestVisible()
		{
			PrepareNumberRangeControl("Number Range (Decimal)");

			Assert(propertySearchDropEdit.Visible);
			Assert(fromCalcEdit.Visible);
			Assert(andLabel.Visible);
			Assert(toCalcEdit.Visible);
			Assert(!upButton.Visible);
			Assert(!downButton.Visible);

			propertySearchDropEdit.SelectItem("Greater than");
			propertySearchDropEdit.CommitBoundValue();
			Assert(propertySearchDropEdit.Visible);
			Assert(fromCalcEdit.Visible);
			Assert(!andLabel.Visible);
			Assert(!toCalcEdit.Visible);
			Assert(!upButton.Visible);
			Assert(!downButton.Visible);

			propertySearchDropEdit.SelectItem("Less than");
			propertySearchDropEdit.CommitBoundValue();
			Assert(propertySearchDropEdit.Visible);
			Assert(!fromCalcEdit.Visible);
			Assert(!andLabel.Visible);
			Assert(toCalcEdit.Visible);
			Assert(!upButton.Visible);
			Assert(!downButton.Visible);

			propertySearchDropEdit.SelectItem("Between");
			propertySearchDropEdit.CommitBoundValue();
			Assert(propertySearchDropEdit.Visible);
			Assert(fromCalcEdit.Visible);
			Assert(andLabel.Visible);
			Assert(toCalcEdit.Visible);
			Assert(!upButton.Visible);
			Assert(!downButton.Visible);

			propertySearchDropEdit.SelectItem("Equal to");
			propertySearchDropEdit.CommitBoundValue();
			Assert(propertySearchDropEdit.Visible);
			Assert(fromCalcEdit.Visible);
			Assert(!andLabel.Visible);
			Assert(!toCalcEdit.Visible);
			Assert(upButton.Visible);
			Assert(downButton.Visible);
		}

		public void TestVisibleInMultilanguages()
		{
			using (var mockChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("Filter|NumberRangeSearchList|GreaterThanFilter", new ResourceStringData("Filter|NumberRangeSearchList|GreaterThanFilter", "大于"));
				mockChs.Put("Filter|NumberRangeSearchList|LessThanFilter", new ResourceStringData("Filter|NumberRangeSearchList|GreaterThanFilter", "小于"));
				mockChs.Put("Filter|NumberRangeSearchList|BetweenFilter", new ResourceStringData("Filter|NumberRangeSearchList|GreaterThanFilter", "介于"));
				mockChs.Put("Filter|NumberRangeSearchList|EqualToFilter", new ResourceStringData("Filter|NumberRangeSearchList|GreaterThanFilter", "等于"));

				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
				{
					PrepareNumberRangeControl("Number Range (Decimal)");

					Assert(propertySearchDropEdit.Visible);
					Assert(fromCalcEdit.Visible);
					Assert(andLabel.Visible);
					Assert(toCalcEdit.Visible);

					propertySearchDropEdit.SelectItem(ResString.GetMultilingualString("Filter|NumberRangeSearchList|GreaterThanFilter", "Greater than or equal to"));
					propertySearchDropEdit.CommitBoundValue();
					Assert(propertySearchDropEdit.Visible);
					Assert(fromCalcEdit.Visible);
					Assert(!andLabel.Visible);
					Assert(!toCalcEdit.Visible);

					propertySearchDropEdit.SelectItem(Res.GetString("Filter|NumberRangeSearchList|LessThanFilter", "Less than or equal to"));
					propertySearchDropEdit.CommitBoundValue();
					Assert(propertySearchDropEdit.Visible);
					Assert(!fromCalcEdit.Visible);
					Assert(!andLabel.Visible);
					Assert(toCalcEdit.Visible);

					propertySearchDropEdit.SelectItem(Res.GetString("Filter|NumberRangeSearchList|BetweenFilter", "Between"));
					propertySearchDropEdit.CommitBoundValue();
					Assert(propertySearchDropEdit.Visible);
					Assert(fromCalcEdit.Visible);
					Assert(andLabel.Visible);
					Assert(toCalcEdit.Visible);

					propertySearchDropEdit.SelectItem(Res.GetString("Filter|NumberRangeSearchList|EqualToFilter", "Equal to"));
					propertySearchDropEdit.CommitBoundValue();
					Assert(propertySearchDropEdit.Visible);
					Assert(fromCalcEdit.Visible);
					Assert(!andLabel.Visible);
					Assert(!toCalcEdit.Visible);
				}
			}
		}

		public void TestLocation()
		{
			PrepareNumberRangeControl("Number Range (Decimal)");

			propertySearchDropEdit.SelectItem("Between");
			propertySearchDropEdit.CommitBoundValue();
			var firstCalcEditLeft = fromCalcEdit.Left;
			var secondCalcEditLeft = toCalcEdit.Left;

			propertySearchDropEdit.SelectItem("Greater than");
			propertySearchDropEdit.CommitBoundValue();
			AssertEquals(firstCalcEditLeft, fromCalcEdit.Left);

			propertySearchDropEdit.SelectItem("Less than");
			propertySearchDropEdit.CommitBoundValue();
			AssertEquals(firstCalcEditLeft, toCalcEdit.Left);

			propertySearchDropEdit.SelectItem("Equal to");
			propertySearchDropEdit.CommitBoundValue();
			AssertEquals(firstCalcEditLeft, fromCalcEdit.Left);
		}

		void PrepareNumberRangeControl(string numberRangeType)
		{
			stripBizO = new MockFilterStripBizO();
			mockForm = new MockFilterStripForm(stripBizO);
			mockForm.Show();
			filterStrip = mockForm.AddFilterStrip(numberRangeType);

			AssertEquals("filterStrip.Controls.Count", 6, filterStrip.Controls.Count);

			numberRangeControl = (ZNumberRangeControl)filterStrip.Controls[5];
			propertySearchDropEdit = (ZFilterStripDropEdit)numberRangeControl.Controls[0];
			fromCalcEdit = (ZCalcEdit)numberRangeControl.Controls[1];
			andLabel = (ZLabel)numberRangeControl.Controls[2];
			toCalcEdit = (ZCalcEdit)numberRangeControl.Controls[3];
			upButton = (ZButton)numberRangeControl.Controls[4];
			downButton = (ZButton)numberRangeControl.Controls[5];
		}

		protected override void TearDown()
		{
			if (mockForm != null)
			{
				mockForm.Dispose();
			}

			base.TearDown();
		}

		MockFilterStripBizO stripBizO;
		MockFilterStripForm mockForm;
		ZFilterStrip filterStrip;
		ZNumberRangeControl numberRangeControl;
		ZFilterStripDropEdit propertySearchDropEdit;
		ZCalcEdit fromCalcEdit;
		ZLabel andLabel;
		ZCalcEdit toCalcEdit;
		ZButton upButton;
		ZButton downButton;
	}
}
