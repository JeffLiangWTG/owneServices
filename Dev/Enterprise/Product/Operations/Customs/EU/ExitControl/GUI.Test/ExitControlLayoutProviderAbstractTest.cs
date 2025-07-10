using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestsSubclassesOf(typeof(IExitControlLayoutProvider))]
	public abstract class ExitControlLayoutProviderAbstractTest<T> : TestCaseWithFactory
		where T : IExitControlLayoutProvider, new()
	{
		public void TestAdditionalDeclarationTabPages()
		{
			AssertContainsExactElementsInExactOrder(ExpectedAdditionalDeclarationTabPageTypes, Provider.AdditionalDeclarationTabPages.Select(x => x.GetType()));
		}

		public void TestRemovableDeclarationTabPageNames()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedRemovableDeclarationTabPageNames, Provider.RemovableDeclarationTabPageNames);
		}

		public void TestHeaderDetailsPanelLayout()
		{
			AssertEquals(ExpectedHeaderDetailsPanelLayoutType, Provider.HeaderDetailsPanelLayout.GetType());
		}

		public void TestAdditionalConsignmentTabPages()
		{
			AssertContainsExactElementsInExactOrder(ExpectedAdditionalConsignmentTabPageTypes, Provider.AdditionalConsignmentTabPages.Select(x => x.GetType()));
		}

		public void TestRemovableConsignmentTabPageNames()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedRemovableConsignmentTabPageNames, Provider.RemovableConsignmentTabPageNames);
		}

		public void TestConsignmentItemPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedConsignmentItemPanelLayoutWithGridType, Provider.ConsignmentItemPanelLayoutWithGrid.GetType());
		}

		public void TestAdditionalReportTabPages()
		{
			AssertContainsExactElementsInExactOrder(ExpectedAdditionalReportTabPageTypes, Provider.AdditionalReportTabPages.Select(x => x.GetType()));
		}

		public void TestRemovableReportTabPageNames()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedRemovableReportTabPageNames, Provider.RemovableReportTabPageNames);
		}

		public void TestCreateReportsGridUserControl()
		{
			if (ExpectedReportsGridUserControlType == null)
			{
				AssertNull(Provider.CreateReportsGridUserControl());
			}
			else
			{
				var reportsGridUserControl = Provider.CreateReportsGridUserControl();
				AssertNotNull("reportsGridUserControl.ReportsGrid", reportsGridUserControl.ReportsGrid);
				using (var control = (ZUserControl)reportsGridUserControl)
				{
					AssertType(ExpectedReportsGridUserControlType, control);
				}
			}
		}

		public void TestCreateReportItemsUserControl()
		{
			if (ExpectedReportItemsUserControlType == null)
			{
				AssertNull(Provider.CreateReportItemsUserControl());
			}
			else
			{
				using (var control = (Control)Provider.CreateReportItemsUserControl())
				{
					AssertType(ExpectedReportItemsUserControlType, control);
				}
			}
		}

		public void TestCreateDetailsReportGridUserControl()
		{
			if (ExpectedDetailsReportsGridUserControlType == null)
			{
				AssertNull(Provider.CreateDetailsReportGridUserControl());
			}
			else
			{
				using (var control = (ZUserControl)Provider.CreateDetailsReportGridUserControl())
				{
					AssertType(ExpectedDetailsReportsGridUserControlType, control);
				}
			}
		}

		[RequiresSTA]
		public void TestCreateReportsMessagesUserControl()
		{
			if (ExpectedReportsMessagesUserControlType == null)
			{
				AssertNull(Provider.CreateReportsMessagesUserControl());
			}
			else
			{
				using (var control = (Control)Provider.CreateReportsMessagesUserControl())
				{
					AssertType(ExpectedReportsMessagesUserControlType, control);
				}
			}
		}

		public void TestContainersOrEquipmentsAndSealsUserControl()
		{
			AssertEquals(ExpectedContainersOrEquipmentsAndSealsUserControlType, Provider.ContainersOrEquipmentsAndSealsUserControlType);
		}

		public void TestAuthorizationIsActive()
		{
			AssertEquals(ExpectedAuthorizationIsActive, Provider.AuthorizationIsActive);
		}

		protected abstract IEnumerable<Type> ExpectedAdditionalDeclarationTabPageTypes { get; }

		protected abstract IEnumerable<ZString> ExpectedRemovableDeclarationTabPageNames { get; }

		protected abstract Type ExpectedHeaderDetailsPanelLayoutType { get; }

		protected abstract IEnumerable<Type> ExpectedAdditionalConsignmentTabPageTypes { get; }

		protected abstract IEnumerable<ZString> ExpectedRemovableConsignmentTabPageNames { get; }

		protected abstract Type ExpectedConsignmentsGridUserControlType { get; }

		protected abstract Type ExpectedConsignmentItemPanelLayoutWithGridType { get; }

		protected abstract IEnumerable<Type> ExpectedAdditionalReportTabPageTypes { get; }

		protected abstract IEnumerable<ZString> ExpectedRemovableReportTabPageNames { get; }

		protected abstract Type ExpectedReportsGridUserControlType { get; }

		protected abstract Type ExpectedReportItemsUserControlType { get; }

		protected abstract Type ExpectedReportsMessagesUserControlType { get; }

		protected abstract Type ExpectedContainersOrEquipmentsAndSealsUserControlType { get; }

		protected abstract Type ExpectedDetailsReportsGridUserControlType { get; }

		protected abstract string CountryOrGroupingCode { get; }

		protected abstract bool ExpectedAuthorizationIsActive { get; }

		public abstract void TestReportAdditionalDocumentsGridLayout();

		public abstract void TestContainersOrEquipmentsGridLayout();

		public abstract void TestSealsGridLayout();

		public abstract void TestReportItemAdditionalDocumentsGridLayout();

		public abstract void TestConsignmentItemPackingDetailsGridLayout();

		public abstract void TestConsignmentItemsGridLayout();

		public abstract void TestAuthorizationGridColumnLayout();

		protected virtual IExitControlLayoutProvider GetProvider() => ExitControlLayoutProvider.GetLayoutProvider(CountryOrGroupingCode);

		protected override void SetUp()
		{
			base.SetUp();
			Provider = GetProvider();
		}

		protected IExitControlLayoutProvider Provider { get; private set; }
	}
}
