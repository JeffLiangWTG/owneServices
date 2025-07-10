using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.AutoCusBondDetail.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { PW_BondType, PW_BondNumber, PW_Password, PW_BondAmount, PW_RX_NKCurrency, PW_BondNumber2, PW_SuretyCode, PW_Override }, guaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestPW_BondType()
		{
			var bondTypeInfo = guaranteesGrid.GetColumnStyle(PW_BondType);
			CombineAssertions(() =>
			{
				var captionResourceString = bondTypeInfo.CaptionResourceString;
				AssertEquals("ShortCaption", "Type", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Type", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Type of Guarantee", captionResourceString.Caption);
				AssertEquals("FullDescription", "Type", captionResourceString.FullDescription);
				AssertEquals("Width", 87, bondTypeInfo.Width);
			});
		}

		public void TestPW_BondNumber()
		{
			var bondNumberInfo = guaranteesGrid.GetColumnStyle(PW_BondNumber);
			CombineAssertions(() =>
			{
				var captionResourceString = bondNumberInfo.CaptionResourceString;
				AssertEquals("ShortCaption", "GRN", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Guarantee Ref. No.", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Guarantee Reference Number", captionResourceString.Caption);
				AssertEquals("FullDescription", "GRN", captionResourceString.FullDescription);
				AssertEquals("Width", 226, bondNumberInfo.Width);
			});
		}

		public void TestPW_Password()
		{
			var passwordInfo = guaranteesGrid.GetColumnStyle(PW_Password);
			CombineAssertions(() =>
			{
				var captionResourceString = passwordInfo.CaptionResourceString;
				AssertEquals("ShortCaption", "GAC", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Access Code", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Guarantee Access Code", captionResourceString.Caption);
				AssertEquals("FullDescription", "GAC", captionResourceString.FullDescription);
				AssertEquals("Width", 144, passwordInfo.Width);
			});
		}

		public void TestPW_BondAmount()
		{
			var bondAmountInfo = guaranteesGrid.GetColumnStyle(PW_BondAmount);
			CombineAssertions(() =>
			{
				var captionResourceString = bondAmountInfo.CaptionResourceString;
				AssertEquals("ShortCaption", "Amount", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Amount", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Liability Amount", captionResourceString.Caption);
				AssertEquals("FullDescription", "Amount", captionResourceString.FullDescription);
				AssertEquals("Width", 202, bondAmountInfo.Width);
			});
		}

		public void TestPW_BondNumber2()
		{
			var bondNumber2Info = guaranteesGrid.GetColumnStyle(PW_BondNumber2);
			CombineAssertions(() =>
			{
				var captionResourceString = bondNumber2Info.CaptionResourceString;
				AssertEquals("ShortCaption", "Other Guarantee Ref.", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Other Guarantee Ref. No.", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Other Guarantee Reference Number", captionResourceString.Caption);
				AssertEquals("FullDescription", "Other Guarantee Ref.", captionResourceString.FullDescription);
				AssertEquals("Width", 247, bondNumber2Info.Width);
			});
		}

		public void TestPW_SuretyCode()
		{
			var suretyCodeInfo = guaranteesGrid.GetColumnStyle(PW_SuretyCode);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), suretyCodeInfo.Width);
		}

		public void TestPW_Override()
		{
			var overrideInfo = guaranteesGrid.GetColumnStyle(PW_Override);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60), overrideInfo.Width);
		}

		public void TestPW_OverrideInfo_ValueChanged_OpensPhase5GuaranteeCalculationLiabilityAmountForm()
		{
			var guarantee = CalculateLiabilityBizObjTestHelper.CreateGuaranteeForCalculateLiabilityBizObj(Factory).guarantee;
			var nctsHeader = guarantee.NctsHeader;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(nctsHeader.MovementHeader, string.Empty);
				form.Show();
				guaranteesGrid.CurrentRowIndex = 1;
				var currentSelectedGuarantee = (NctsGuarantee)guaranteesGrid.ListManager.GetCurrent();
				currentSelectedGuarantee.PW_Override = true;
				AssertType<Phase5GuaranteeCalculationLiabilityAmountForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestDataSource() => AssertEquals(typeof(NctsDepartureMovementHeader), userControl.DataSourceType);

		public void TestGuaranteesGrid_Binding() => AssertEquals(nameof(NctsDepartureMovementHeader.Guarantees), guaranteesGrid.BindTo);

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GuaranteesUserControl();
			guaranteesGrid = userControl.FindSingle<ZGrid>("GuaranteesGrid");
		}
		ZGrid guaranteesGrid;
		Phase5GuaranteesUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
