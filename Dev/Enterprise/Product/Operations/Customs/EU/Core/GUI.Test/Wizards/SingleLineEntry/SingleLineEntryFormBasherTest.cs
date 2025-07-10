using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.SingleLineEntry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(SingleLineEntryForm))]
	public sealed class SingleLineEntryFormBasherTest : ZFormBasherTest
	{
		public void TestNoDeveloperNotificationOnTariffF3()
		{
			using (var form = new SingleLineEntryFormForTest(mock.Object))
			{
				form.Show();
				((IFindBoxUserControl)form.TariffFindBox).ShowEditOrViewForm();
				((IFindBoxUserControl)form.CpcFindBox).ShowEditOrViewForm();
				AssertEquals("", ErrorReporter.LastMessageReported);
			}
		}

		[ExpectNoExceptions]
		public void TestFunctional()
		{
			using (var form = new SingleLineEntryForm(mock.Object))
			{
				form.Show();
				Button okButton = (Button)form.Controls.Find("OKButton", searchAllChildren: true)[0];
				okButton.PerformClick();
				Application.DoEvents();
				mock.VerifyAll();
			}
		}

		public void TestTariffFindBox()
		{
			mock.Object.Declaration.JE_MessageType = "IMP";
			using (var form = new SingleLineEntryFormForTest(mock.Object))
			{
				form.Show();
				var tariffFindBox = (Universal.GUI.TariffFindBox)form.TariffFindBox;
				AssertEquals("IMP", tariffFindBox.TariffType);
				AssertType<Universal.GUI.TariffFindBox>("TariffFindBox", form.TariffFindBox);
			}

			mock.Object.Declaration.JE_MessageType = "EXP";
			using (var form = new SingleLineEntryFormForTest(mock.Object))
			{
				form.Show();
				var tariffFindBox = (Universal.GUI.TariffFindBox)form.TariffFindBox;
				AssertEquals("EXP", tariffFindBox.TariffType);
				AssertType<Universal.GUI.TariffFindBox>("TariffFindBox", form.TariffFindBox);
			}
		}

		public void TestTariffFindBoxOpensUniversalTariffSearchForm()
		{
			var registry = ZArchitecture.Environment.DataRegistry.Instance.RawRegistry;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (registry.ExternalBorderComplianceTool.SetTemporaryValue(Guid.Empty,
					   Guid.Empty,
					   Guid.Empty,
					   ExternalBorderComplianceToolList.Codes.None))
			using (registry.BorderWiseEnableWebSocketClient.SetTemporaryValue(Guid.Empty,
					   Guid.Empty,
					   Guid.Empty,
					   false))
			{
				using (var form = new SingleLineEntryFormForTest(mock.Object))
				{
					form.Show();

					var findBox = form.TariffFindBox;
					AssertNoExceptionThrown(() => findBox.SelectFromPopupForm(true));
					AssertType<Universal.GUI.TariffFindBox>("TariffFindBox", form.TariffFindBox);
				}
			}
		}

		public void TestTariffFindBoxOpensBorderWiseTariffSearch()
		{
			var registry = ZArchitecture.Environment.DataRegistry.Instance.RawRegistry;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (registry.ExternalBorderComplianceTool.SetTemporaryValue(Guid.Empty,
					   Guid.Empty,
					   Guid.Empty,
					   ExternalBorderComplianceToolList.Codes.BorderWiseWeb))
			using (registry.BorderWiseEnableWebSocketClient.SetTemporaryValue(Guid.Empty,
					   Guid.Empty,
					   Guid.Empty,
					   false))
			{
				using (var form = new SingleLineEntryFormForTest(mock.Object))
				{
					form.Show();

					var findBox = form.TariffFindBox;
					AssertNoExceptionThrown(() => findBox.SelectFromPopupForm());
					AssertEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)findBox).PopupForm.GetType());
				}
			}
		}

		[RequiresSTA]
		public void TestCPCFindBoxVerifyType()
		{
			var registry = ZArchitecture.Environment.DataRegistry.Instance.RawRegistry;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (registry.ExternalBorderComplianceTool.SetTemporaryValue(Guid.Empty,
					   Guid.Empty,
					   Guid.Empty,
					   ExternalBorderComplianceToolList.Codes.BorderWiseWeb))
			using (registry.BorderWiseEnableWebSocketClient.SetTemporaryValue(Guid.Empty,
					   Guid.Empty,
					   Guid.Empty,
					   false))
			{
				using (var form = new SingleLineEntryFormForTest(mock.Object))
				{
					form.Show();
					var findBox = form.CpcFindBox;

					AssertType<ZCodeFindBox>("CPCFindBox type is wrong.", findBox);
				}
			}
		}

		public void TestCPCFindBoxDoesNotUseBorderWiseTariffSearch()
		{
			var previousValue = Env.Registry.ExternalBorderComplianceTool;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			using (new DisposableAction(() => Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None, () => Env.Registry.ExternalBorderComplianceTool = previousValue))
			{
				using (var form = new SingleLineEntryFormForTest(mock.Object))
				{
					form.Show();
					var findBox = form.CpcFindBox;

					AssertNoExceptionThrown(() => findBox.SelectFromPopupForm(true));
					AssertNotEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)findBox).PopupForm.GetType());
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new SingleLineEntryForm(new SingleLineEntryManager(Factory.New<JobDeclaration>()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			mock = new Mock<ISingleLineEntryManager>();
			_ = mock.Setup(m => m.Declaration).Returns(Factory.New<JobDeclaration>());
			_ = mock.Setup(m => m.SingleLineEntry).Returns(new Business.Declaration.SingleLineEntry(Factory));
			_ = mock.Setup(m => m.Execute());
		}

		Mock<ISingleLineEntryManager> mock;

		class SingleLineEntryFormForTest : SingleLineEntryForm
		{
			public SingleLineEntryFormForTest(ISingleLineEntryManager manager) : base(manager)
			{ }

			public ZCodeFindBox TariffFindBox => base.tariffFindBox;

			public ZCodeFindBox CpcFindBox => base.CPCFindBox;
		}
	}
}
