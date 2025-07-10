using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	[TestedType(typeof(ExitControlLayoutProvider))]
	sealed class ExitControlLayoutProviderTest : EU.ExitControl.GUI.Testing.ExitControlLayoutProviderAbstractTest<ExitControlLayoutProvider>
	{
		public override void TestReportAdditionalDocumentsGridLayout()
		{
			AssertType<ReportAdditionalDocumentGridColumnLayout>(Provider.ReportAdditionalDocumentsGridLayout);
		}

		public override void TestContainersOrEquipmentsGridLayout()
		{
			AssertType<ContainersOrEquipmentsGridColumnLayout>(Provider.ContainersOrEquipmentsGridLayout);
		}

		public override void TestSealsGridLayout()
		{
			AssertType<SealsGridColumnLayout>(Provider.SealsGridLayout);
		}

		public override void TestReportItemAdditionalDocumentsGridLayout()
		{
			AssertType<ReportItemAdditionalDocumentGridColumnLayout>(Provider.ReportItemAdditionalDocumentsGridLayout);
		}

		public override void TestConsignmentItemPackingDetailsGridLayout()
		{
			AssertType<ConsignmentItemPackingDetailsGridColumnLayout>(Provider.ConsignmentItemPackingDetailsGridLayout);
		}

		public override void TestConsignmentItemsGridLayout()
		{
			AssertType<ConsignmentItemsGridColumnLayout>(Provider.ConsignmentItemsGridLayout);
		}

		public override void TestAuthorizationGridColumnLayout()
		{
			AssertType<AuthorizationGridColumnLayout>(Provider.AuthorizationGridColumnLayout);
		}

		protected override IEnumerable<Type> ExpectedAdditionalDeclarationTabPageTypes => Enumerable.Empty<Type>();

		protected override IEnumerable<ZString> ExpectedRemovableDeclarationTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedHeaderDetailsPanelLayoutType => typeof(HeaderDetailsLayout);

		protected override IEnumerable<Type> ExpectedAdditionalConsignmentTabPageTypes
		{
			get
			{
				yield return typeof(ConsignmentAdditionalInformationTabPage);
			}
		}

		protected override IEnumerable<ZString> ExpectedRemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedConsignmentsGridUserControlType => typeof(ConsignmentsGridUserControl);

		protected override Type ExpectedConsignmentItemPanelLayoutWithGridType => typeof(ConsignmentItemLayoutWithGrid);

		protected override IEnumerable<Type> ExpectedAdditionalReportTabPageTypes => Enumerable.Empty<Type>();

		protected override IEnumerable<ZString> ExpectedRemovableReportTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedReportsGridUserControlType => typeof(ReportsGridUserControl);

		protected override Type ExpectedReportItemsUserControlType => typeof(ReportItemsUserControl);

		protected override Type ExpectedReportsMessagesUserControlType => typeof(MessagesTabUserControl);

		protected override Type ExpectedContainersOrEquipmentsAndSealsUserControlType => typeof(ContainersOrEquipmentsAndSealsUserControl);

		protected override Type ExpectedDetailsReportsGridUserControlType => typeof(HeaderExitReportStatusGridUserControl);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Germany;

		protected override bool ExpectedAuthorizationIsActive => false;
	}
}
