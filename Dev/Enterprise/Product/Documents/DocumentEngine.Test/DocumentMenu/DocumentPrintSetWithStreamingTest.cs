using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentPrintSetWithStreamingTest : TestCaseWithFactory
	{
		public void TestDocumentPackListIsEnumeratedOnlyOnce()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			var task = new DocumentPrintSetWithStreaming(documentCommand, NbOfDocumentPacks, GetDocumentPacks());
			task.DeliveryInstructionsDefaultPK = GuidToFixedDocumentMenu;

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				task.Run(Env.Security.None);
				AssertEquals(NbOfDocumentPacks, NbOfEnumerationHits);
			}
		}

		public void TestDocumentPackListIsEnumeratedOnlyOnce_ResetCachedReports()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			var task = new DocumentPrintSetWithStreaming(documentCommand, NbOfDocumentPacks, GetDocumentPacks());
			task.DeliveryInstructionsDefaultPK = GuidToFixedDocumentMenu;

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				task.Run(Env.Security.None);
				task.ResetCachedReports();
				AssertEquals(NbOfDocumentPacks, NbOfEnumerationHits);
			}
		}

		IEnumerable<DocumentPack> GetDocumentPacks()
		{
			for (int i = 0; i < NbOfDocumentPacks; i++)
			{
				++NbOfEnumerationHits;
				yield return new DocumentPack();
			}
		}

		#region implementation

		int NbOfEnumerationHits;
		const int NbOfDocumentPacks = 7;
		ZGuid GuidToFixedDocumentMenu;

		void SetUpPrinter()
		{
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";
			Factory.Save();
			GuidToFixedDocumentMenu = stmMenuItem.PK;
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
			defaultPrinter.SDP_SQ_Printer = Factory.New<IStmPrintQueue>().PK;
			defaultPrinter.SDP_NumberOfCopies = 1;
			Factory.Save();
		}

		public class DocumentPrintSetWithStreamingForTest : DocumentPrintSetWithStreaming
		{
			public DocumentPrintSetWithStreamingForTest(DocumentCommand command, int docPacksCount, IEnumerable<DocumentPack> docPacks, bool supportsLanguageSelection = false)
				: base(command, docPacksCount, docPacks, supportsLanguageSelection)
			{
			}

			public bool IsPreviewAllowed_ExposedForTest
			{
				get { return base.IsPreviewAllowed; }
			}

			public bool NeedPrinterForAutoDelivery_ExposedForTest
			{
				get { return base.NeedPrinterForAutoDelivery; }
			}

			public bool AllowedToPerformAutoDelivery
			{
				get { return base.PermissionForAutoDelivery; }
			}
		}
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			NbOfEnumerationHits = 0;
			SetUpPrinter();
		}
	}
}
