using CargoWise.Types;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Freight.CFS.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CFSShipmentStatusProvider : Freight.CFS.Business.CFSShipmentStatusProvider
		, Integration.Customs.CA.ICFSShipmentStatusProvider
	{
		public CFSShipmentStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
		}

		public static new CFSShipmentStatusProvider New(CFSShipment shipment)
		{
			return (CFSShipmentStatusProvider)Freight.CFS.Business.CFSShipmentStatusProvider.New(shipment);
		}

		#region Properties

		CFSShipmentRNSStatusProvider StatusProvider
		{
			get
			{
				if (fStatusProvider == null)
				{
					fStatusProvider = new CFSShipmentRNSStatusProvider(shipment);
				}

				return fStatusProvider;
			}
		}

		CFSShipmentRNSStatusProvider fStatusProvider;

		ReleaseStatus ReleaseUpdate
		{
			get
			{
				if (fReleaseUpdate == null)
				{
					fReleaseUpdate = StatusProvider.ReleaseUpdate;
				}

				return fReleaseUpdate;
			}
		}

		ReleaseStatus fReleaseUpdate;

		#endregion

		#region ICFSShipmentStatusProvider

		protected override StatusClass StatusClassCore()
		{
			var result = StatusClass.Held;

			if (processingIndicator == ProcessingIndicatorCodedList.GoodsReleased.ToString()
				|| processingIndicator == ProcessingIndicatorCodedList.Import.ToString())
			{
				result = StatusClass.Clear;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced.ToString())
			{
				result = StatusClass.Warning;
			}

			return result;
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			bool result = false;

			if (shipment.HasErrors)
			{
				ui.ShowError(Res.GetString("7040778a-dada-4c3c-90e3-a1a634358f8e", "Please clear all errors before saving."));
			}
			else
			{
				if (processingIndicator == ProcessingIndicatorCodedList.GoodsReleased.ToString()
					|| processingIndicator == ProcessingIndicatorCodedList.Import.ToString())
				{
					result = true;
				}
				else
				{
					string statusDescription = Res.GetString("d85eae95-ee25-4e81-9de2-53651625bbf0", "The release status of this shipment is {0}.", Status);

					if (processingIndicator == ProcessingIndicatorCodedList.GoodsRequiredForExamination.ToString()
					 || processingIndicator == ProcessingIndicatorCodedList.GoodsDetained.ToString()
					 || processingIndicator == ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival.ToString()
					 || processingIndicator == ProcessingIndicatorCodedList.TransactionAwaitingProcessing.ToString())
					{
						ui.ShowError(statusDescription);
					}
					else if (processingIndicator == ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced.ToString())
					{
						result = ui.Ask(Res.GetString("be949b19-f376-4aa7-b94b-23ecc8980fe8", "{0} Continue with Contingency Release?", statusDescription));
					}
					else
					{
						ui.ShowWarning(statusDescription);
						result = true;
					}
				}
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
		ZString processingIndicator
		{
			get
			{
				return ReleaseUpdate == null ? ZString.Empty : ReleaseUpdate.RL_ProcessingIndicator;
			}
		}

		protected override ZString DetailsFromMessagesCore
		{
			get
			{
				return StatusProvider.MessageInterpretation;
			}
		}

		protected override ZString StatusCore()
		{
			return StatusProvider.ReleaseStatus;
		}

		protected override ZString ShortStatusCore()
		{
			return StatusProvider.ReleaseStatusCode;
		}

		#endregion
	}
}
