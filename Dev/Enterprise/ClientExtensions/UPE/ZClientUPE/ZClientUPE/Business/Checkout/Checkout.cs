using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine.Integration;
using Enterprise.DocumentEngine.Public;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class Checkout : NonPersistentBusinessObject, IObsoleteValidation
	{
		public Checkout(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateTrackingNumber();
			ValidateLabelPrinter();
			ValidateInvoicePrinter();

			base.RunPreSaveValidationCore();
		}

		#region Tracking Number

		[MaxLength(CusHAWB.Schema.CS_HAWBMaxLength)]
		public ZString TrackingNumber
		{
			get { return fTrackingNumber; }
			set
			{
				if (fTrackingNumber != value)
				{
					CheckMaximumLength(TrackingNumberInfo, value);
					SetNonPersistentPropertyValue(TrackingNumberInfo, ref		fTrackingNumber, value);
					ResetCallouts();
					if (!IsValidationSuspended)
					{
						ValidateTrackingNumber();
					}
				}
			}
		}
		ZString fTrackingNumber;

		public ZPropertyInfo TrackingNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TrackingNumber)); }
		}

		public void ValidateTrackingNumber()
		{
			TrackingNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TrackingNumberInfo);
			if (Callouts.Length == 0)
			{
				TrackingNumberInfo.AddError("Tracking number is not found.");
			}
		}

		#endregion

		#region LabelPrinter

		public virtual ZGuid LabelPrinter
		{
			get { return fLabelPrinter; }
			set
			{
				fLabelPrinter = value;
				if (!IsValidationSuspended)
				{
					ValidateLabelPrinter();
				}
				LabelPrinterInfo.RefreshBinding();
			}
		}
		ZGuid fLabelPrinter;

		protected virtual void ValidateLabelPrinter()
		{
			LabelPrinterInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LabelPrinterInfo);
			TypeValidation.CheckValidGuid(LabelPrinterInfo);
		}

		public ZPropertyInfo LabelPrinterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(LabelPrinter)); }
		}

		#endregion

		#region InvoicePrinter

		public virtual ZGuid InvoicePrinter
		{
			get { return fInvoicePrinter; }
			set
			{
				fInvoicePrinter = value;
				if (!IsValidationSuspended)
				{
					ValidateInvoicePrinter();
				}
				InvoicePrinterInfo.RefreshBinding();
			}
		}
		ZGuid fInvoicePrinter;

		protected virtual void ValidateInvoicePrinter()
		{
			InvoicePrinterInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(InvoicePrinterInfo);
			TypeValidation.CheckValidGuid(InvoicePrinterInfo);
		}

		public ZPropertyInfo InvoicePrinterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(InvoicePrinter)); }
		}

		#endregion

		#region Callouts

		Callout[] Callouts
		{
			get
			{
				if (fCallouts == null)
				{
					ZDBOnlyQuery calloutQuery = new ZDBOnlyQuery(typeof(Callout));

					ZDBOnlySubQuery relatedWaybillSubQuery = new ZDBOnlySubQuery(typeof(JobRelatedWayBill), JobRelatedWayBillSchema.EB_ParentID);
					relatedWaybillSubQuery.AddToFilter(JobRelatedWayBillSchema.EB_WaybillNumber, TrackingNumber);
					calloutQuery.AddSubQuery(relatedWaybillSubQuery, JoinCondition.And);
					fCallouts = Factory.Load<Callout>(calloutQuery);
				}
				return fCallouts;
			}
		}
		Callout[] fCallouts;

		void ResetCallouts()
		{
			fCallouts = null;
		}

		#endregion

		#region Bind To Lists

		public CodeDescriptionPairList PrinterNames
		{
			get
			{
				if (printerNames == null)
				{
					IPrinterListRetriever printerListRetriever = ObjectFactory.Get<IPrinterListRetriever>();
					printerNames = printerListRetriever.Retrieve();
				}
				return printerNames;
			}
		}
		CodeDescriptionPairList printerNames;

		#endregion

		#region Print

		public void Print()
		{
			var documentPrinter = new DocumentPrinter();
			foreach (Callout callout in Callouts)
			{
				documentPrinter.Print(RoutingLabelPK, LabelPrinter, callout);
				if (callout.IsCOD)
				{
					documentPrinter.Print(TaxInvoicePK, InvoicePrinter, callout);
				}
			}
		}

		/// <summary>
		/// UPE Specific Unique Routine Label Menu Item.
		/// </summary>
		ZGuid RoutingLabelPK
		{
			get
			{
				if (routingLabelPK.IsEmpty)
				{
					routingLabelPK = GetDocumentPK(DocumentRoutingLabel);
				}
				return routingLabelPK;
			}
		}
		ZGuid routingLabelPK;
		const string DocumentRoutingLabel = "Routing Label";

		/// <summary>
		/// UPE Specific Unique Tax Invoice Menu Item.
		/// </summary>
		ZGuid TaxInvoicePK
		{
			get
			{
				if (taxInvoicePK.IsEmpty)
				{
					taxInvoicePK = GetDocumentPK(DocumentTaxInvoice);
				}
				return taxInvoicePK;
			}
		}
		ZGuid taxInvoicePK;
		const string DocumentTaxInvoice = "Tax Invoice";

		ZGuid GetDocumentPK(string documentName)
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, documentName);
			return Factory.LoadTop1<StmMenuItem>(filter).PK;
		}

		#endregion

		#region Form State

		public void SaveFormState()
		{
			string formState = LabelPrinter + "~" + InvoicePrinter;
			Env.Registry.SetFilterCriteria(GetType().FullName, formState);
		}

		public void SetFormState()
		{
			ZString formState = Env.Registry.GetFilterCriteria(base.GetType().FullName);
			int seperatorIndex = formState.IndexOf("~");

			if (seperatorIndex > -1)
			{
				string labelPrinterGuid = formState.Left(seperatorIndex);
				string invoicePrinterGuid = formState.SubstringSafe(seperatorIndex + 1);
				try
				{
					ZGuid defaultLabelPrinter = new ZGuid(labelPrinterGuid);
					if (IsPrinterQueuePKValid(defaultLabelPrinter))
					{
						LabelPrinter = defaultLabelPrinter;
					}

					ZGuid defaultInvoicePrinter = new ZGuid(invoicePrinterGuid);
					if (IsPrinterQueuePKValid(defaultInvoicePrinter))
					{
						InvoicePrinter = defaultInvoicePrinter;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					//if this fails for whatever reason, then no need to do anything 
					//only issue will be that previous settings will be forgotten
					//next time this will recover
				}
			}
		}

		bool IsPrinterQueuePKValid(ZGuid printerQueuePK)
		{
			if (printerQueuePK.IsValid)
			{
				foreach (CodeElement element in PrinterNames)
				{
					if ((ZGuid)element.PK == printerQueuePK)
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion
	}
}
