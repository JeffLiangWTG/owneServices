using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	[TestedType(typeof(ExitControlLayoutProvider))]
	sealed class ExitControlLayoutProviderTest : EU.ExitControl.GUI.Testing.ExitControlLayoutProviderAbstractTest<ExitControlLayoutProvider>
	{
		public void TestProviderType()
		{
			AssertType<ExitControlLayoutProvider>("Exit Control Layout Provider Type", EU.ExitControl.GUI.ExitControlLayoutProvider.GetLayoutProvider(CountryOrGroupingCode));
		}

		public override void TestReportAdditionalDocumentsGridLayout()
		{
			AssertType<EU.ExitControl.GUI.ReportAdditionalDocumentUcc6GridColumnLayout>(Provider.ReportAdditionalDocumentsGridLayout);
		}

		public override void TestContainersOrEquipmentsGridLayout()
		{
			AssertType<EU.ExitControl.GUI.ContainersOrEquipmentsUcc6GridColumnLayout>(Provider.ContainersOrEquipmentsGridLayout);
		}

		public override void TestSealsGridLayout()
		{
			AssertType<EU.ExitControl.GUI.SealsUcc6GridColumnLayout>(Provider.SealsGridLayout);
		}

		public override void TestReportItemAdditionalDocumentsGridLayout()
		{
			AssertType<EU.ExitControl.GUI.ReportItemAdditionalDocumentUcc6GridColumnLayout>(Provider.ReportItemAdditionalDocumentsGridLayout);
		}

		public override void TestConsignmentItemPackingDetailsGridLayout()
		{
			AssertType<EU.ExitControl.GUI.ConsignmentItemPackingDetailsUcc6GridColumnLayout>(Provider.ConsignmentItemPackingDetailsGridLayout);
		}

		public override void TestConsignmentItemsGridLayout()
		{
			AssertType<EU.ExitControl.GUI.ConsignmentItemsUcc6GridColumnLayout>(Provider.ConsignmentItemsGridLayout);
		}

		public override void TestAuthorizationGridColumnLayout()
		{
			AssertType<EU.ExitControl.GUI.AuthorizationGridColumnLayout>(Provider.AuthorizationGridColumnLayout);
		}

		protected override IEnumerable<Type> ExpectedAdditionalDeclarationTabPageTypes => Enumerable.Empty<Type>();

		protected override IEnumerable<ZString> ExpectedRemovableDeclarationTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedHeaderDetailsPanelLayoutType => typeof(HeaderDetailsLayout);

		protected override IEnumerable<Type> ExpectedAdditionalConsignmentTabPageTypes => Enumerable.Empty<Type>();

		protected override IEnumerable<ZString> ExpectedRemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedConsignmentsGridUserControlType => typeof(EU.ExitControl.GUI.ConsignmentsGridUserControl);

		protected override Type ExpectedConsignmentItemPanelLayoutWithGridType => typeof(EU.ExitControl.GUI.ConsignmentItemUcc6LayoutWithGrid);

		protected override IEnumerable<Type> ExpectedAdditionalReportTabPageTypes
		{
			get
			{
				yield return typeof(EU.ExitControl.GUI.ReportAdditionalDocumentsTabPage);
			}
		}

		protected override IEnumerable<ZString> ExpectedRemovableReportTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedReportsGridUserControlType => typeof(ReportsGridUserControl);

		protected override Type ExpectedReportItemsUserControlType => typeof(EU.ExitControl.GUI.ReportItemsUserControl);

		protected override Type ExpectedReportsMessagesUserControlType => typeof(EU.ExitControl.GUI.MessagesTabUserControl);

		protected override Type ExpectedContainersOrEquipmentsAndSealsUserControlType => typeof(EU.ExitControl.GUI.ContainersOrEquipmentsAndSealsUserControl);

		protected override Type ExpectedDetailsReportsGridUserControlType => typeof(EU.ExitControl.GUI.HeaderExitReportStatusUcc6GridUserControl);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Spain;

		protected override bool ExpectedAuthorizationIsActive => false;
	}
}
