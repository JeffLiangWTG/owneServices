using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DepartureMovementsTabUserControlTest : TestCaseWithFactory
	{
		public void TestTabPagesOrder()
		{
			AssertSequencesEqual("TabPageNames", ExpectedTabPageNamesInOrder, userControl.TabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name));
		}

		public static IEnumerable<string> ExpectedTabPageNamesInOrder = new[]
		{
				"DetailsTabPage",
				"CustomFieldsTabPage"
		};

		public void TestBindingSource()
		{
			AssertEquals(typeof(INctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>), userControl.BindingSource.DataSourceType);
		}

		public void TestMovementsGrid()
		{
			userControl.AssertContainsControl<Phase5DepartureMovementsTabGridUserControl>(nameof(DepartureMovementsTabUserControl.Phase5DepartureMovementsTabGridUserControl), x => x
				.WithBindTo(".")
			);
		}

		public void TestSplitterPanelMinSize() => CombineAssertions(() =>
		{
			AssertEquals("Grid min size", 75, userControl.SplitContainer.Panel1MinSize);
			AssertEquals("Details min size", 310, userControl.SplitContainer.Panel2MinSize);
		});

		public void TestDeclarationDetailsPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZPanel>(nameof(DepartureMovementsTabUserControl.DeclarationDetailsPanel), x => x
					.WithDock(DockStyle.Top)
				));
		}

		public void TestDeclarationDetailsGroupBox()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZGroupBox>(nameof(DepartureMovementsTabUserControl.DeclarationDetailsGroupBox), x => x
					.WithLocation(0, 0)
					.WithSizeScaled(861, 141)
					.WithCaption("Declaration Details")
				));
		}

		public void TestDynamicDeclarationDetailsPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<DynamicLayoutPanel>(nameof(DepartureMovementsTabUserControl.DynamicDeclarationDetailsPanel), x => x
					.WithDock(DockStyle.Fill)
				));
		}

		[RequiresSTA]
		public void TestModeOfTransportPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZPanel>(nameof(DepartureMovementsTabUserControl.ModeOfTransportPanel), x => x
					.WithDock(DockStyle.Top)
				));
		}

		public void TestTransportBorderGroupBox()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZGroupBox>(nameof(DepartureMovementsTabUserControl.TransportBorderGroupBox), x => x
					.WithLocation(474, 0)
					.WithSizeScaled(468, 163)
					.WithCaption("Transport Border")
				));
		}

		public void TestTransportBorderDynamicLayoutPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<DynamicLayoutPanel>(nameof(DepartureMovementsTabUserControl.TransportBorderDynamicLayoutPanel), x => x
					.WithDock(DockStyle.Fill)
				));
		}

		[RequiresSTA]
		public void TestTransportDepartureGroupBox()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZGroupBox>(nameof(DepartureMovementsTabUserControl.TransportDepartureGroupBox), x => x
					.WithLocation(0, 0)
					.WithSizeScaled(468, 163)
					.WithCaption("Transport Departure")
				));
		}

		public void TestTransportDepartureDynamicLayoutPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<DynamicLayoutPanel>(nameof(DepartureMovementsTabUserControl.TransportDepartureDynamicLayoutPanel), x => x
					.WithDock(DockStyle.Fill)
				));
		}

		public void TestTransportAndPackingPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZPanel>(nameof(DepartureMovementsTabUserControl.TransportAndPackingPanel), x => x
					.WithDock(DockStyle.Top)
				));
		}

		public void TestTransportDetailsGroupBox()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<ZGroupBox>(nameof(DepartureMovementsTabUserControl.TransportDetailsGroupBox), x => x
					.WithDock(DockStyle.Fill)
					.WithCaption("Transport Details")
				));
		}

		public void TestTransportAndPackagingDynamicLayoutPanel()
		{
			WithSelectedTab(() =>
				userControl.AssertContainsControl<DynamicLayoutPanel>(nameof(DepartureMovementsTabUserControl.TransportAndPackagingDynamicLayoutPanel), x => x
					.WithDock(DockStyle.Fill)
				));
		}

		void WithSelectedTab(Action action)
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				userControl.SetBindingMember("DepartureMovementHeaders");

				form.Show();
				userControl.TabControl.SelectTab(0);

				action();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new DepartureMovementsTabUserControl();
		}
		DepartureMovementsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
