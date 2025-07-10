using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ExitControlLayoutProvider))]
	sealed class ExitControlLayoutProviderBaseOnlyTest : ExitControlLayoutProviderAbstractTest<ExitControlLayoutProvider>
	{
		public void TestObjectFactoryContents()
		{
			CombineAssertions(() =>
			{
				var expectedExitControlLayoutProviders = ExpectedExitControlLayoutProviders;
				foreach (var provider in expectedExitControlLayoutProviders)
				{
					var countryOrGroupingCode = provider.Key;
					var countryOrGroupingCodeProvider = ExitControlLayoutProvider.GetLayoutProvider(countryOrGroupingCode);
					AssertEquals($"CountryOrGroupingCode-{countryOrGroupingCode} has expected Type", provider.Value, countryOrGroupingCodeProvider.GetType());
				}
				AssertEquals("CountryOrGroupingCode-null has expected Type", typeof(ExitControlLayoutProvider), ExitControlLayoutProvider.GetLayoutProvider(null).GetType());
			});
		}

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

		protected override IEnumerable<Type> ExpectedAdditionalConsignmentTabPageTypes => Enumerable.Empty<Type>();

		protected override IEnumerable<ZString> ExpectedRemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedConsignmentsGridUserControlType => typeof(ConsignmentsGridUserControl);

		protected override Type ExpectedConsignmentItemPanelLayoutWithGridType => typeof(ConsignmentItemLayoutWithGrid);

		protected override IEnumerable<Type> ExpectedAdditionalReportTabPageTypes => Enumerable.Empty<Type>();

		protected override IEnumerable<ZString> ExpectedRemovableReportTabPageNames => Enumerable.Empty<ZString>();

		protected override Type ExpectedReportsGridUserControlType => typeof(ReportsGridUserControl);

		protected override Type ExpectedReportItemsUserControlType => null;

		protected override Type ExpectedReportsMessagesUserControlType => typeof(MessagesTabUserControl);

		protected override Type ExpectedContainersOrEquipmentsAndSealsUserControlType => typeof(ContainersOrEquipmentsAndSealsUserControl);

		protected override Type ExpectedDetailsReportsGridUserControlType => typeof(HeaderExitReportStatusGridUserControl);

		protected override bool ExpectedAuthorizationIsActive => false;

		protected override string CountryOrGroupingCode => "ZZ";

		Dictionary<string, Type> ExpectedExitControlLayoutProviders => new Dictionary<string, Type>()
		{
			{ "Default", typeof(ExitControlLayoutProvider) },
			{ string.Empty, typeof(ExitControlLayoutProvider) },
		};
	}
}
