using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EES.Business
{
	public class EESShipmentDocumentSupporter : ForwardingShipmentDocumentSupporter
	{
		public EESShipmentDocumentSupporter(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		#region Overrides

		protected override TitleCopyCountPair GetHBLDocumentTitle(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair titles = null;

			if (Shipment.JS_HouseBillOfLadingType == "EEX" && (pivot.SI_PrintCopyType == nameof(PrintCopyType.PRN) || pivot.SI_PrintCopyType == nameof(PrintCopyType.ALL)))
			{
				titles = new TitleCopyCountPair("ORIGINAL", Convert.ToByte(EESDataRegistry.Instance.NumberOfOriginalBillsToBePrintedOnDotMatrix));
			}
			else
			{
				titles = base.GetHBLDocumentTitle(parentDocumentMenuName, parentBusinessObject, pivot);
			}

			return titles;
		}

		#endregion

	}
}
