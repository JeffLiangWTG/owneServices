using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionPaymentController
	{
		public CommissionPaymentController(ZForm parentForm, ICommissionPayable commissionPayable)
		{
			Argument.NotNull(parentForm, "parentForm");
			Argument.NotNull(commissionPayable, "commissionPayable");

			this.parentForm = parentForm;
			this.CommissionPayable = commissionPayable;
		}

		readonly ZForm parentForm;
		protected readonly ICommissionPayable CommissionPayable;

		public bool PromptUserForProcessPayment()
		{
			if (!RunPreProcessPaymentValidation())
			{
				return false;
			}

			var dialogResult = Globals.Message.Show(
				Res.GetString("9dad9ab7-0b07-4c6a-aeec-97e67fa8c238", "Are you sure you want to flag these entity commissions as paid?"),
				ProcessPaymentCaption,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				DialogResult.No);

			if (dialogResult != DialogResult.Yes)
			{
				return false;
			}

			using (DocumentPack documentPack = new DocumentPack())
			using (Report report = GetNewCommissionPaymentSummaryReport(documentPack))
			using (PrintTask printTask = new PrintTask())
			{
				documentPack.Add(report);
				printTask.Add(documentPack);

				var deliveryDest = printTask.Run(Env.Security.None);
				if (deliveryDest != DeliveryInstructionDestination.None &&
					deliveryDest != DeliveryInstructionDestination.Preview &&
					deliveryDest != DeliveryInstructionDestination.UserCancelled)
				{
					ProcessPayment();
					try
					{
						CommissionPayable.Factory.Save();

						Globals.Message.ShowInformation(Res.GetString("eae190e5-501e-430b-8966-f05aa2c69cb8", "Payment successfully processed."), ProcessPaymentCaption);
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex, parentForm);
					}
					finally
					{
						parentForm.Close();
					}

					return true;
				}
			}

			return false;
		}

		protected virtual bool CheckForObsoleteLines()
		{
			return CommissionPayable.CommissionLinesForPayment.Any(x => x.IsPaid || x.IsCancelled);
		}

		protected virtual bool RunPreProcessPaymentValidation()
		{
			if (!Env.Security.CommissionProcessPayment.IsAllowed)
			{
				Env.Security.CommissionProcessPayment.ShowError();
				return false;
			}

			if (CheckForObsoleteLines())
			{
				Globals.Message.ShowError(Res.GetString("d945a8cc-a8d1-4ccd-aad5-5a0839b640bc", "Cannot process payment because it contains entity commissions that have already been paid or canceled."), UnableToProcessPaymentCaption);
				return false;
			}

			if (!CommissionPayable.CommissionLinesForPayment.Any())
			{
				Globals.Message.ShowError(Res.GetString("f4a2410e-396f-4877-90a8-a4800096b57b", "No entity commissions were selected for payment."), UnableToProcessPaymentCaption);
				return false;
			}

			foreach (var line in CommissionPayable.CommissionLinesForPayment)
			{
				line.Validation.ValidateFullyPaymentOfARInvoices();
			}

			if (CommissionPayable.CommissionLinesForPayment.Any(x => x.Validation.ShouldStopPaymentForProcess))
			{
				Globals.Message.ShowError(Res.GetString("87463a7c-76ed-4e62-80e7-8d70681f4a41", "Selected entity commission(s) have issues that must be fixed before they can be paid. Please reference the warning(s) for additional information."), UnableToProcessPaymentCaption);
				return false;
			}

			return true;
		}

		void ProcessPayment()
		{
			foreach (var line in CommissionPayable.CommissionLinesForPayment.Select(x => x.AccCommissionLine))
			{
				line.CL0_PaidDateTimeUtc = ZDateTime.UtcNow;
			}
		}

		#region Templates

		Report GetNewCommissionPaymentSummaryReport(DocumentPack documentPack)
		{
			var documentWrappers = CommissionPayable.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommissionPayment, null);
			var docWrapper = documentWrappers.Single();
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(CommissionPaymentSummaryTemplate);
			return new Report(documentPack, excelTemplate, docWrapper, CommissionPaymentSummaryReportName, null, DocumentDirection.ANY, false);
		}

		public StmTemplate CommissionPaymentSummaryTemplate
		{
			get
			{
				if (!commissionPaymentSummaryTemplateInitialized)
				{
					commissionPaymentSummaryTemplateInitialized = true;

					var query = new ZQuery(StmTemplateSchema.SO_Name, CommissionPaymentSummaryTemplateName);
					query.AddToFilter(StmTemplateSchema.SO_DataContext, Constants.DataContext.CommissionPayment);

					commissionPaymentSummaryTemplate = CommissionPayable.Factory.LoadTop1<StmTemplate>(query);
				}

				return commissionPaymentSummaryTemplate;
			}
		}
		bool commissionPaymentSummaryTemplateInitialized;
		StmTemplate commissionPaymentSummaryTemplate;

		static string CommissionPaymentSummaryTemplateName
		{
			get { return OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value ? LocalCommissionPaymentSummaryTemplateName : GlobalCommissionPaymentSummaryTemplateName; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string LocalCommissionPaymentSummaryTemplateName = "Commission Payment Summary";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string GlobalCommissionPaymentSummaryTemplateName = "Commission Payment Summary (Global)";

		ZString CommissionPaymentSummaryReportName
		{
			get { return Res.GetString("12eb5d55-e81b-455d-8566-aa105458e09e", "Commission Payment Summary"); }
		}

		#endregion

		#region Messages

		protected static string ProcessPaymentCaption
		{
			get { return Res.GetString("425443a3-9a35-4cd6-8835-62becd7d5e9e", "Process Payment"); }
		}

		protected static string UnableToProcessPaymentCaption
		{
			get { return Res.GetString("3264223c-0aeb-4575-b0d2-b55fbb18b8b6", "Unable to Process Payment"); }
		}

		#endregion
	}
}
