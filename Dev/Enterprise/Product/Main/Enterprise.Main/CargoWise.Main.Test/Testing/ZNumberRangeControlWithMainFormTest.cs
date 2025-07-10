using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class ZNumberRangeControlWithMainFormTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGreaterThan()
		{
			propertySearchDropEdit.SelectItem("Greater than");
			propertySearchDropEdit.CommitBoundValue();
			Application.DoEvents();

			fromCalcEdit.Focus();
			KeySender.PostKeyDown(fromCalcEdit, Keys.D4);
			Application.DoEvents();
			KeySender.PostKeyDown(fromCalcEdit, Keys.Enter);
			Application.DoEvents();

			AssertSqlText("OM_CMAcheivableClientRevenue >= @CWO4_");
			AssertSqlText("@CWO1_ = \"1\"");
			AssertSqlText("@CWO4_ = \"4\"");
		}

		[StressTest]
		[RequiresSTA]
		public void TestLessThan()
		{
			propertySearchDropEdit.SelectItem("Less than");
			propertySearchDropEdit.CommitBoundValue();
			Application.DoEvents();

			toCalcEdit.Focus();
			KeySender.PostKeyDown(toCalcEdit, Keys.D4);
			Application.DoEvents();
			KeySender.PostKeyDown(toCalcEdit, Keys.Enter);
			Application.DoEvents();

			AssertSqlText("OM_CMAcheivableClientRevenue <= @CWO4_");
			AssertSqlText("@CWO1_ = \"1\"");
			AssertSqlText("@CWO4_ = \"4\"");
		}

		[RequiresSTA]
		public void TestEqualTo()
		{
			propertySearchDropEdit.SelectItem("Equal to");
			propertySearchDropEdit.CommitBoundValue();
			Application.DoEvents();

			fromCalcEdit.Focus();
			KeySender.PostKeyDown(fromCalcEdit, Keys.D4);
			Application.DoEvents();
			KeySender.PostKeyDown(fromCalcEdit, Keys.Enter);
			Application.DoEvents();

			AssertSqlText("OM_CMAcheivableClientRevenue = @CWO4_");
			AssertSqlText("@CWO1_ = \"1\"");
			AssertSqlText("@CWO4_ = \"4\"");
		}

		[StressTest]
		[RequiresSTA]
		public void TestDefaultBetween()
		{
			propertySearchDropEdit.SelectItem("Between");
			propertySearchDropEdit.CommitBoundValue();
			Application.DoEvents();

			AssertSqlText("OM_CMAcheivableClientRevenue = @CWO4_");
			AssertSqlText("@CWO1_ = \"1\"");
			AssertSqlText("@CWO4_ = \"0\"");
		}

		[StressTest]
		[RequiresSTA]
		public void TestBetween()
		{
			propertySearchDropEdit.SelectItem("Between");
			propertySearchDropEdit.CommitBoundValue();
			Application.DoEvents();

			toCalcEdit.Focus();
			KeySender.PostKeyDown(toCalcEdit, Keys.D4);
			Application.DoEvents();
			KeySender.PostKeyDown(toCalcEdit, Keys.Enter);
			Application.DoEvents();

			AssertSqlText("OM_CMAcheivableClientRevenue >= @CWO4_ and OM_CMAcheivableClientRevenue <= @CWO5_");
			AssertSqlText("@CWO1_ = \"1\"");
			AssertSqlText("@CWO4_ = \"0\"");
			AssertSqlText("@CWO5_ = \"4\"");
		}

		[RequiresSTA]
		public void TestIncorrectBetween()
		{
			propertySearchDropEdit.SelectItem("Between");
			propertySearchDropEdit.CommitBoundValue();
			Application.DoEvents();

			fromCalcEdit.Focus();
			KeySender.PostKeyDown(fromCalcEdit, Keys.D4);
			Application.DoEvents();
			KeySender.PostKeyDown(fromCalcEdit, Keys.Enter);
			Application.DoEvents();

			toCalcEdit.Focus();
			KeySender.PostKeyDown(toCalcEdit, Keys.D3);
			Application.DoEvents();
			KeySender.PostKeyDown(toCalcEdit, Keys.Enter);
			Application.DoEvents();

			AssertErrorMessage("There are errors. Please correct these before searching.");
		}

		void PrepareFilter()
		{
			testMainForm = new MainFormTestCase.TestMainForm();
			testMainForm.Show();
			var containersMainFormModule = new MainFormModule(ModuleIDs.Organisation);
			testMainForm.OpenModule(containersMainFormModule, true);

			var filterStrip = (ZFilterStrip)(testMainForm.Controls.Find("ZFilterStrip", true)[0]);
			var filterDescriptionDropEdit = filterStrip.FilterDescriptionDropEdit;
			filterDescriptionDropEdit.Focus();
			filterDescriptionDropEdit.SelectItem("Achievable Business");
			filterDescriptionDropEdit.CommitBoundValue();
			Application.DoEvents();

			filterStripControl = (ZFilterStripControl)filterStrip.Parent.Parent;
			numberRangeControl = (ZNumberRangeControl)filterStrip.CurrentFilterControls[0];
			propertySearchDropEdit = (ZFilterStripDropEdit)numberRangeControl.Controls[0];
			fromCalcEdit = (ZCalcEdit)numberRangeControl.Controls[1];
			toCalcEdit = (ZCalcEdit)numberRangeControl.Controls[3];
		}

		void AssertSqlText(string subStr)
		{
			AssertCommon(CheckType.SqlText, subStr);
		}

		void AssertErrorMessage(string subStr)
		{
			AssertCommon(CheckType.ErrMsg, subStr);
		}

		enum CheckType
		{
			SqlText, ErrMsg
		}

		void AssertCommon(CheckType checkType, string subStr, string expectedTableName = OrgHeaderSchema.Constants.TableName)
		{
			var savedValue = EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult;
			EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = false;

			try
			{
				filterStripControl.Find();
				string result;

				if (checkType == CheckType.SqlText)
				{
					result = SqlEventTracker.Instance.SqlEventList.Last(s => Regex.IsMatch(s, "\\b" + expectedTableName + "\\b", RegexOptions.IgnoreCase));
				}
				else
				{
					result = UnitTestUserNotification.Instance.LastMessage.Text;
				}

				AssertContains(subStr, result);
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = savedValue;
			}
		}

		MainFormTestCase.TestMainForm testMainForm;
		ZFilterStripControl filterStripControl;
		ZNumberRangeControl numberRangeControl;
		ZFilterStripDropEdit propertySearchDropEdit;
		ZCalcEdit fromCalcEdit;
		ZCalcEdit toCalcEdit;

		protected override void SetUp()
		{
			PrepareFilter();
			base.SetUp();
		}

		protected override void TearDown()
		{
			if (testMainForm != null)
			{
				testMainForm.Dispose();
			}

			base.TearDown();
		}
	}
}
