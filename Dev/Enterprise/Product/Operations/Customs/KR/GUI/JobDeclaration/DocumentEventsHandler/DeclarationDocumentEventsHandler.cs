using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class DeclarationDocumentEventsHandler : IDocumentEventsHandler
	{
		public DocumentSupporter DocumentSupporter { get; set; }
		public JobDeclarationDocumentSupporter DeclarationDocumentSupporter => (JobDeclarationDocumentSupporter)DocumentSupporter;
		public bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			var englishMenuName = menuItem.SU_MenuNameMultilingual.GetUnresolvedString();
			return englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ExportGoodsInspectionResultReport
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.InspectionPlanAndResultReport
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.CorrectionNoticeOfCountryOfOrigin
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.NoticeOfCorrectionReviewResults
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.NoticeOfCustomsMandatedAmendment
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.RefundRequest
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.LocalExportGoodsInspectionResultReport
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration
				|| englishMenuName == JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration;
		}

		public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (DeclarationDocumentSupporter != null)
			{
				DeclarationDocumentSupporter.DocumentGenerationActions.InitialiseFor(e.MenuItem);

				if (DeclarationDocumentSupporter.AreMultipleEntriesRelevant || (DeclarationDocumentSupporter.DocumentGenerationActions.IsAmendmentRelevant && DeclarationDocumentSupporter.DocumentGenerationActions.Count > 0))
				{
					if (Globals.CanShowDialogs)
					{
						var result = ZFormModaliser.ShowDialogAndDispose(new DocumentGeneratingActionForm(DeclarationDocumentSupporter.DocumentGenerationActions));
						if (result == System.Windows.Forms.DialogResult.Cancel)
						{
							e.Cancel = true;
						}
						else
						{
							if (DeclarationDocumentSupporter.DocumentGenerationActions.HasErrors())
							{
								Globals.Message.Show(Res.GetString("BB0F942F-6E4F-49F3-B8CD-77E82C9EF13C", "There are errors. Please check and rectify the problems before attempting to print this document again.")
														, Res.GetString("F13B3569-50B6-4A06-BDA6-6378F59C62AC", "Error")
														, ZMessageBoxButtons.OK
														, ZMessageBoxIcon.Error
													);
								e.Cancel = true;
							}
						}
					}
				}
				else if (DeclarationDocumentSupporter.DocumentGenerationActions.Count == 1)
				{
					var documentAction = DeclarationDocumentSupporter.DocumentGenerationActions[0];
					if (documentAction.HasErrors)
					{
						Globals.Message.Show(
								Res.GetString("F97AD2D0-1F75-4242-910E-766A6DCEE8F8", "There are following errors. Please check and rectify the problems before attempting to print this document again.") + "\r\n" +
								documentAction.GetErrors().ToUniqueMessageListString("\r\n"),
								Res.GetString("E847F310-D1BC-468F-BC32-82D9B596AE73", "Errors"),
								ZMessageBoxButtons.OK,
								ZMessageBoxIcon.Error,
								ZDialogResult.OK
							);
						e.Cancel = true;
					}
					else if (documentAction.HasMessageErrors || documentAction.HasWarnings)
					{
						var notifications = new ZStringBuilder();
						if (documentAction.HasWarnings)
						{
							notifications.Append(documentAction.GetWarnings().ToUniqueMessageListString());
						}
						if (documentAction.HasMessageErrors)
						{
							notifications.Append(documentAction.GetMessageErrors().ToUniqueMessageListString());
						}

						if (!notifications.IsEmpty)
						{
							var messageBoxResult = Globals.Message.Show(
								Res.GetString("4E37FD78-3D2D-46FD-B545-3F64212A84E8", "There are notifications. Are you sure you wish to print this document?") + "\r\n" +
								notifications.ToStringWithNewLineBetweenAppends(),
								Res.GetString("AA217823-DEFA-43DB-93C0-204A57C83E4E", "Warning"),
								ZMessageBoxButtons.YesNo,
								ZMessageBoxIcon.Warning,
								ZDialogResult.Yes
							);
							if (messageBoxResult != ZDialogResult.Yes)
							{
								e.Cancel = true;
							}
						}
					}
				}
			}
		}

		public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
		}
	}
}
