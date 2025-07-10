using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.ARAP.HotCheque
{
	public class HotChequePrintManager : IDocumentEvents
	{
		public HotChequePrintManager(ZGuid hotChequePK, BusinessObjectFactory factory)
			: this(factory.Load<AccHotCheque>(hotChequePK), factory)
		{
		}

		public HotChequePrintManager(AccHotCheque chequeToPrint, BusinessObjectFactory factory)
		{
			this.ChequeToPrint = chequeToPrint;
			this.Factory = factory;
			chequeToPrint.DocumentSupporter.Initialise(this);
		}

		public void AutoPrintCheque(ZGuid printQueuePk)
		{
			this.PrintQueuePk = printQueuePk;
			Print();
		}

		public void Print()
		{
			string templateName = ChequeToPrint.GetChequeTemplateName();

			if (string.IsNullOrEmpty(templateName))
			{
				Globals.Message.ShowInformation(Res.GetString("d5ec3b92-29f3-4491-a156-e1edd5e87f02", "'Auto Print Check' option not setup. To print check, please check the Check Book and Bank Account setup."), Res.GetString("61f1e150-b008-4c28-be7a-002b34bac577", "Print Hot Check"));
			}
			else if (!AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(templateName, ChequeToPrint))
			{
				Globals.Message.ShowInformation(AccPrintingUtility.GetMissingChequeTemplateMenuErrorMessage(templateName), Res.GetString("61f1e150-b008-4c28-be7a-002b34bac577", "Print Hot Check"));
			}
			else if (Validate())
			{
				DeliveryInstructionDestination result = DeliveryInstructionDestination.None;
				AccPrintingUtility printUtil = new AccPrintingUtility(Factory, Enterprise.Core.Constants.DataContext.Cheques);

				if (PrintQueuePk.IsValid)
				{
					if (!Globals.IsTest)
					{
						result = printUtil.PrintDocument(ChequeToPrint, templateName, AllowedDeliveryOptions.All, PrintQueuePk, false);
					}
					else
					{
						ChequeIsAutoPrinted = ZBool.True;
						PrinterPassedForPrinting = PrintQueuePk;
					}
				}
				else
				{
					if (!Globals.IsTest)
					{
						result = printUtil.PrintDocument(ChequeToPrint, templateName, AllowedDeliveryOptions.HardCopyOnly, ZGuid.Empty, false);
					}
				}
				RaiseDocumentPrinted(result);
			}
		}

		#region Implementation

		protected AccHotCheque ChequeToPrint;
		protected BusinessObjectFactory Factory;
		ZGuid PrintQueuePk;

		protected bool Validate()
		{
			return !ChequeToPrint.AQ_Printed || ValidateReprint();
		}

		protected bool ValidateReprint()
		{
			if (PrintQueuePk != ZGuid.Empty)
			{
				//This cheque is being auto printed. The AQ_Printed was set by PaymentChequeNumberAllocator before it was actually printed.
				return true;
			}
			else
			{
				if (Env.Security.ReprintReallocateCheque.IsAllowed)
				{
					return Globals.Message.Show(Res.GetString("d410120a-6fb0-404d-9d65-4be12b8cf441", "This Check is already printed. Do you want to re-print it?"), Res.GetString("61f1e150-b008-4c28-be7a-002b34bac577", "Print Hot Check"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("572f354c-7434-45c8-a3c0-eac6ed0493f1", "This check is already printed."), Res.GetString("61f1e150-b008-4c28-be7a-002b34bac577", "Print Hot Check"));
					return false;
				}
			}
		}

		/// <summary>
		/// This method is currently not being used by Remittance Advice Print. Implemented to suppress compiler warning.
		/// </summary>
		protected void RaiseDocumentPrintRequested()
		{
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(this, new DocumentCancelEventArgs(null));
			}
		}

		/// <summary>
		/// This method is currently not being used by Remittance Advice Print. Implemented to suppress compiler warning.
		/// </summary>
		protected void RaiseDocumentPrePreviewed(DeliveryInstructionDestination instructionsDestination)
		{
			if (instructionsDestination != DeliveryInstructionDestination.UserCancelled)
			{
				DocumentPrePreviewed?.Invoke(this, new DocumentPrintedEventArgs(instructionsDestination, null));
			}
		}

		/// <summary>
		/// This method is currently not being used by Remittance Advice Print. Implemented to suppress compiler warning.
		/// </summary>
		protected void RaiseDocumentPrePrinted(DeliveryInstructionDestination instructionsDestination)
		{
			if (instructionsDestination != DeliveryInstructionDestination.UserCancelled)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, new DocumentPrintedEventArgs(instructionsDestination, null));
				}
			}
		}

		protected void RaiseDocumentPrinted(DeliveryInstructionDestination instructionsDestination)
		{
			if (instructionsDestination != DeliveryInstructionDestination.UserCancelled)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, new DocumentPrintedEventArgs(instructionsDestination, null));
				}
			}
		}

		#endregion

		#region IDocumentEvents Members

		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public event DocumentPrintedEventHandler DocumentPrinted;

		#endregion

		#region TestHelper Members

		public ZBool ChequeIsAutoPrinted;
		public ZGuid PrinterPassedForPrinting;

		#endregion
	}
}
