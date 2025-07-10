using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public class DocDeliveryPrintDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocDeliveryPrintDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocDeliveryPrintDetails(BusinessObjectFactory factory, int copies)
			: base(factory)
		{
			NumberOfCopies = copies;
			NumberOfCopies_ReadOnly = true;
		}

		public DocDeliveryPrintDetails(DeliveryInstructions instructions)
			: base(instructions.Factory)
		{
			this.instructions = instructions;
		}

		public DocDeliveryPrintDetails(PrintTaskSettings printTaskInstructions)
			: base(printTaskInstructions.Factory)
		{
			this.printTaskInstructions = printTaskInstructions;
		}

		readonly DeliveryInstructions instructions;
		readonly PrintTaskSettings printTaskInstructions;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			NumberOfCopies = 1;
		}

		#endregion

		#region Properties

		#region Print Queue

		[List("PrinterNames")]
		public ZGuid PrintQueuePK
		{
			get { return printQueuePK; }
			set
			{
				if (SetNonPersistentPropertyValue(PrintQueuePKInfo, ref printQueuePK, value)
					&& !IsValidationSuspended)
				{
					ValidatePrintQueue();
				}
			}
		}
		ZGuid printQueuePK;

		public ZPropertyInfo PrintQueuePKInfo => GetZPropertyInfo(nameof(PrintQueuePK), "Printer");

		public StmPrintQueue PrintQueue
		{
			get { return (StmPrintQueue)Factory.Load(typeof(StmPrintQueue), PrintQueuePK); }
		}

		#endregion

		#region Number Of Copies

		public ZInt NumberOfCopies
		{
			get { return numberOfCopies; }
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value)
					&& !IsValidationSuspended)
				{
					ValidateNumberOfCopies();
				}
			}
		}

		public ZPropertyInfo NumberOfCopiesInfo
		{
			get { return GetZPropertyInfo(nameof(NumberOfCopies)); }
		}

		public bool NumberOfCopies_ReadOnly { get; set; }

		ZInt numberOfCopies = 1;

		#endregion

		#region ShowOnlyPrintersUserCanPrintTo

		public ZBool ShowOnlyPrintersUserCanPrintTo
		{
			get { return DocumentsDataRegistry.Instance.ShowOnlyPrintersUserCanPrintTo; }
			set
			{
				DocumentsDataRegistry.Instance.ShowOnlyPrintersUserCanPrintTo = value;
				printers = null;
				ShowOnlyPrintersUserCanPrintToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowOnlyPrintersUserCanPrintToInfo
		{
			get { return GetZPropertyInfo(nameof(ShowOnlyPrintersUserCanPrintTo)); }
		}

		#endregion

		#endregion

		#region Validation

		#region Print Queue

		public void ValidatePrintQueue()
		{
			PrintQueuePKInfo.ClearAllNotifications();

			if (((printTaskInstructions != null && printTaskInstructions.HasPrintedDocuments)
				|| (instructions != null && instructions.HasPrintedDocuments && (instructions.DocumentsToBeDelivered.Count == 0 || instructions.DocumentsToBeDelivered.OfType<IDeliverable>().Any(deliverable => deliverable.PrinterDetails.PrintQueue == null))))
				&& PrintQueuePK.IsEmpty)
			{
				PrintQueuePKInfo.AddError(Res.GetString("4c5ecf93-fd81-4618-ba3f-970db96b8559", "You must select a printer as some of your documents are set to be printed."));
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(PrintQueuePKInfo, Printers);
				PrintQueueValidation.ValidatePrintQueue(PrintQueuePKInfo);
			}
		}

		#endregion

		#region Number Of Copies

		public void ValidateNumberOfCopies()
		{
			NumberOfCopiesInfo.ClearAllNotifications();
			if (NumberOfCopies < 0)
			{
				NumberOfCopiesInfo.AddError(Res.GetString("4570c66f-a1d8-4d4e-84ab-38d5a222bcae", "Number of Printed Copies cannot be set to a number less than 0."));
			}
			else if (NumberOfCopies > short.MaxValue)
			{
				NumberOfCopiesInfo.AddError(Res.GetString("B0890C07-4746-42AF-8D14-A3DBD6508EB3", "Number of Printed Copies cannot be set to a number bigger than 32,767."));
			}
			else if (NumberOfCopies > 10)
			{
				NumberOfCopiesInfo.AddWarning(Res.GetString("1F41DFCF-699C-4447-A0CE-A65D6F5D342B", "Number of Printed Copies exceeds 10."));
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePrintQueue();
			ValidateNumberOfCopies();
		}

		#endregion

		#region Lookups

		#region Printers

		public virtual StmPrintQueueCollection Printers
		{
			get
			{
				return printers ??= StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, ShowOnlyPrintersUserCanPrintTo);
			}
		}
		StmPrintQueueCollection printers;

		internal StmPrintQueueCollection GetOnlinePrintersRegardlessOfAllowable()
		{
			return StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, false, true);
		}

		public CodeDescriptionPairList PrinterNames
		{
			get { return Printers.GetOnlinePrinterNames(); }
		}

		#endregion

		#endregion
	}
}
