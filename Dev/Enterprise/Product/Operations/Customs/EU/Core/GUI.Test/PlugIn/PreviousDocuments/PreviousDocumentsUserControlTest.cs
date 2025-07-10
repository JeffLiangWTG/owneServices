using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class PreviousDocumentsUserControlTest : PreviousDocumentUserControlAbstractTest<PreviousDocumentsUserControl, JobDeclaration>
	{
		protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
		{
			return new[]
			{
				(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
				(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
				(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle)),
			};
		}

		public void TestGridColumnLayoutProvider() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			using var control = new PreviousDocumentsUserControlForTest(declaration);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				AssertType<NonUCC6PreviousDocumentGridColumnLayout>(control.GetGridColumnLayoutProvider_Exposed());
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertType<UCC6PreviousDocumentGridColumnLayout>(control.GetGridColumnLayoutProvider_Exposed());
			}
		});

		sealed class PreviousDocumentsUserControlForTest : LayoutPreviousDocumentsUserControl
		{
			public PreviousDocumentsUserControlForTest(JobDeclaration declaration) {
				JobDeclaration = declaration;
			}
			internal IGridColumnLayoutProvider GetGridColumnLayoutProvider_Exposed() => GetGridColumnLayoutProvider();
		}
	}
}
