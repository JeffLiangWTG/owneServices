using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	public class PrintTaskSettings : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PrintTaskSettings(PrintTask task)
			: base(task.Factory)
		{
			this.task = task;
		}

		#region Properties

		#region IsDraft

		public ZBool IsDraft
		{
			get { return fIsDraft; }
			set
			{
				fIsDraft = value;
				IsDraftInfo.RefreshBinding();
				foreach (DeliveryInstructions instructions in DocPacksDeliveryInstructions)
				{
					instructions.IsDraft = fIsDraft;
				}
			}
		}

		ZBool fIsDraft = ZBool.False;

		public ZPropertyInfo IsDraftInfo
		{
			get { return GetZPropertyInfo(nameof(IsDraft)); }
		}

		#endregion

		#region HasPrintedDocuments

		public bool HasPrintedDocuments
		{
			get
			{
				foreach (DeliveryInstructions docPackInstructions in DocPacksDeliveryInstructions)
				{
					if (docPackInstructions.HasPrintedDocuments)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region AllowPreview

		public ZBool AllowPreview
		{
			get { return PrintTask.Count < 50 ? fAllowPreview : ZBool.False; }
			set { fAllowPreview = value; }
		}

		ZBool fAllowPreview = ZBool.True;

		#endregion

		#region MultipleDocumentPacks

		public ZBool MultipleDocumentPacks
		{
			get { return PrintTask.Count > 1; }
		}

		#endregion

		#endregion

		#region Related Objects

		public PrintTask PrintTask
		{
			get
			{
				return task;
			}
		}

		public DeliveryInstructionsCollection DocPacksDeliveryInstructions
		{
			get
			{
				DeliveryInstructionsCollection instructions = new DeliveryInstructionsCollection(Factory);
				foreach (DocumentPack pack in PrintTask.GetDocumentPacks())
				{
					instructions.Add(pack.DeliveryInstructions);
				}
				return instructions;
			}
		}

		public ISecurityCheckpoint ModifyDocumentCheckPoint { get; set; }

		public DeliveryInstructions SingleDocPackInstructions
		{
			get
			{
				DeliveryInstructions result = null;
				if (PrintTask.Count == 1)
				{
					result = PrintTask[0].DeliveryInstructions;
				}
				return result;
			}
		}

		#region DeliveryOptions

		public AllowedDeliveryOptions DeliveryOptions
		{
			get { return SingleDocPackInstructions != null ? SingleDocPackInstructions.DeliveryOptions : deliveryOptions; }
			set
			{
				deliveryOptions = value;
				foreach (DeliveryInstructions instructions in this.DocPacksDeliveryInstructions)
				{
					instructions.DeliveryOptions = deliveryOptions;
				}
			}
		}

		AllowedDeliveryOptions deliveryOptions = AllowedDeliveryOptions.All;

		#endregion

		#region AllowSaveDefaults

		public bool AllowSaveDefaults
		{
			get { return SingleDocPackInstructions != null ? SingleDocPackInstructions.AllowSaveDefaults : allowSaveDefaults; }
			set
			{
				allowSaveDefaults = value;
				foreach (DeliveryInstructions instructions in this.DocPacksDeliveryInstructions)
				{
					instructions.AllowSaveDefaults = allowSaveDefaults;
				}
			}
		}

		bool allowSaveDefaults = true;

		#endregion

		#region Destination

		public DeliveryInstructionDestination Destination
		{
			get { return SingleDocPackInstructions != null ? SingleDocPackInstructions.Destination : destination; }
			set
			{
				destination = value;
				foreach (DeliveryInstructions instructions in this.DocPacksDeliveryInstructions)
				{
					instructions.Destination = destination;
				}
			}
		}

		DeliveryInstructionDestination destination = DeliveryInstructionDestination.None;

		#endregion

		#region Printer Delivery Details

		public DocDeliveryPrintDetails PrinterDelivery
		{
			get
			{
				InitialisePrinterDelivery();

				return printerDelivery;
			}
			set
			{
				printerDelivery = value;
				foreach (DeliveryInstructions instructions in this.DocPacksDeliveryInstructions)
				{
					instructions.PrinterDelivery = printerDelivery;
				}
			}
		}

		void InitialisePrinterDelivery()
		{
			if (printerDelivery == null)
			{
				printerDelivery = new DocDeliveryPrintDetails(this);
				RegisterEditableChildObject(printerDelivery);
			}
		}

		DocDeliveryPrintDetails printerDelivery;

		#endregion

		#endregion

		#region Notifications

		#region Document Processing Start

		public event EventHandler StartDocProcessing;
		public void OnStartDocProcessing()
		{
			if (StartDocProcessing != null)
			{
				StartDocProcessing(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Document Processing End

		public event EventHandler EndDocProcessing;
		public void OnEndDocProcessing()
		{
			if (EndDocProcessing != null)
			{
				EndDocProcessing(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Document Generation Start

		public event EventHandler StartDocumentGeneration;
		public void OnStartDocumentGeneration()
		{
			if (StartDocumentGeneration != null)
			{
				StartDocumentGeneration(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Document Generation End

		public event EventHandler EndDocumentGeneration;
		public void OnEndDocumentGeneration()
		{
			if (EndDocumentGeneration != null)
			{
				EndDocumentGeneration(this, EventArgs.Empty);
			}
		}

		#endregion

#if DEBUG
		public string NotificationLogs;
#endif

		#endregion

		#region Overrides

		public override string ToString()
		{
			return "PrintTaskSettings";
		}

		#endregion

		#region Implementation

		readonly PrintTask task;

		#endregion
	}
}
