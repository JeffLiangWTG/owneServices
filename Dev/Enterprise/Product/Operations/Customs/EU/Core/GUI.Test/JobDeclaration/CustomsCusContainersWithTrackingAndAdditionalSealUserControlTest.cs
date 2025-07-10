using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class CustomsCusContainersWithTrackingAndAdditionalSealUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalSealsGroupBoxVisibility()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertEquals(
					"AdditionalSealsGroupBox should be visible when AdditionalSealsRequired returns true.",
					true,
					userControl.FindSingle<ZGroupBox>("AdditionalSealsGroupBox").Visible
				);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals(
					"AdditionalSealsGroupBox should be hidden when AdditionalSealsRequired returns false.",
					false,
					userControl.FindSingle<ZGroupBox>("AdditionalSealsGroupBox").Visible
				);
			}
		}

		[RequiresSTA]
		public void TestBK_SealNumberColumn()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var additionalSealsGrid = userControl.FindSingle<ZGrid>("AdditionalSealsGrid");
				var sealNumberColumn = additionalSealsGrid.GetColumnStyle(CusSeal.Schema.BK_SealNumber);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, sealNumberColumn.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), sealNumberColumn.Width);
			}
		}

		public void TestZG_IsControlColumn()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var containerGrid = userControl.FindSingle<ZModuleButtonGrid>("CusContainersBoundGrid").InnerGrid;
				var controlColumn = containerGrid.GetColumnStyle(EUAddInfoSchema.Constants.ZG_IsControl);
				CombineAssertions("JobDeclaration activated the CheckBoxes", () =>
				{
					AssertEquals("Caption", "Control", containerGrid.GetColumnCaption(EUAddInfoSchema.Constants.ZG_IsControl));
					AssertEquals("Unavailable", false, controlColumn.IsUnavailable);
					AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50), controlColumn.Width);
				});
			}

			var defaultDeclaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(defaultDeclaration))
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var containerGrid = userControl.FindSingle<ZModuleButtonGrid>("CusContainersBoundGrid").InnerGrid;
				var controlColumn = containerGrid.GetColumnStyle(EUAddInfoSchema.Constants.ZG_IsControl);
				CombineAssertions("Checkboxes are not activated by default", () =>
				{
					AssertEquals("Unavailable", true, controlColumn.IsUnavailable);
				});
			}
		}

		public void TestZG_IsUnloadedColumn()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var containerGrid = userControl.FindSingle<ZModuleButtonGrid>("CusContainersBoundGrid").InnerGrid;
				var controlColumn = containerGrid.GetColumnStyle(EUAddInfoSchema.Constants.ZG_IsUnloaded);
				CombineAssertions("JobDeclaration activated the CheckBoxes", () =>
				{
					AssertEquals("Caption", "Unloaded", containerGrid.GetColumnCaption(EUAddInfoSchema.Constants.ZG_IsUnloaded));
					AssertEquals("Unavailable", false, controlColumn.IsUnavailable);
					AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50), controlColumn.Width);
				});
			}

			var defaultDeclaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(defaultDeclaration))
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var containerGrid = userControl.FindSingle<ZModuleButtonGrid>("CusContainersBoundGrid").InnerGrid;
				var controlColumn = containerGrid.GetColumnStyle(EUAddInfoSchema.Constants.ZG_IsControl);
				CombineAssertions("Checkboxes are not activated by default", () =>
				{
					AssertEquals("Unavailable", true, controlColumn.IsUnavailable);
				});
			}
		}

		public void TestContainersUserControlIsOverriden()
		{
			using (var userControl = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				AssertType(typeof(ContainersUserControl), userControl.containersUserControl1);
			}
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool AdditionalSealsRequired => JE_MessageType == EUJobMessageTypeList.Codes.Export;

			protected override ZBool ContainerControlCheckboxVisibleCore => true;

			protected override ZBool ContainerUnloadedCheckboxVisibleCore => true;
		}
	}
}
