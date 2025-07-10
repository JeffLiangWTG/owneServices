using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class HouseBillPrintInstructions : IPrintInstructions, IPrePrintProcessor
	{
		public HouseBillPrintInstructions(IDocumentPivot pivot, IReadOnlyDictionary<string, string> parameters, object parent)
		{
			this.pivot = Argument.NotNull(pivot, nameof(pivot));
			this.parameters = parameters;
			this.parent = parent;
		}

		readonly IDocumentPivot pivot;
		readonly IReadOnlyDictionary<string, string> parameters;
		readonly object parent;

		public string Title => pivot.DocumentTitle;
		public string[] DeliveryModes => pivot.DeliveryModes;

		public string AttachmentFilename
		{
			get
			{
				if (attachmentFilename == null)
				{
					parameters?.TryGetValue(nameof(AttachmentFilename), out attachmentFilename);
				}

				return attachmentFilename;
			}
		}
		string attachmentFilename;

		public string DocumentName
		{
			get
			{
				if (documentName == null)
				{
					parameters?.TryGetValue(nameof(DocumentName), out documentName);
				}

				return documentName;
			}
		}
		string documentName;

		const string express = "EXPRESS";
		const string original = "ORIGINAL";
		const string copy = "COPY";

		public string GetDeliveryTitle(string deliveryMode)
		{
			if (parent is Forwarding.IForwardingShipment shipment
				&& shipment.JS_ReleaseType == Enterprise.Core.Constants.ShipmentReleaseTypes.ExpressBofL)
			{
				return express;
			}

			if (deliveryMode != nameof(PrintCopyType.PRN)
				&& deliveryMode != nameof(PrintCopyType.ALL))
			{
				return copy;
			}

			return pivot.DocumentTitle.ToUpperInvariant();
		}

		public int GetNumberOfCopies(string deliveryMode)
		{
			if (parent is Forwarding.IForwardingShipment shipment)
			{
				var res = shipment.JS_ReleaseType == Enterprise.Core.Constants.ShipmentReleaseTypes.ExpressBofL
					? GetNumberOfCopiesForExpressBOL(shipment)
					: GetNumberOfCopiesForNonExpressBOL(shipment);

				return string.Compare(deliveryMode, nameof(PrintCopyType.PRN), StringComparison.OrdinalIgnoreCase) == 0
					|| string.Compare(deliveryMode, nameof(PrintCopyType.ALL), StringComparison.OrdinalIgnoreCase) == 0
					? res
					: Math.Min(1, res);
			}

			return 1;
		}

		int GetNumberOfCopiesForExpressBOL(Forwarding.IForwardingShipment shipment)
		{
			return pivot.DocumentTitle.Equals(copy, StringComparison.InvariantCultureIgnoreCase)
				? shipment.JS_NoCopyBills
				: 0;
		}

		int GetNumberOfCopiesForNonExpressBOL(Forwarding.IForwardingShipment shipment)
		{
			return string.Compare(pivot.DocumentTitle, original, StringComparison.OrdinalIgnoreCase) == 0
				? shipment.JS_NoOriginalBills
				: shipment.JS_NoCopyBills;
		}

		public IEnumerable<KeyValuePair<string, string>> GetParametersForDocumentDeliveryLog(string documentName)
		{
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name,
				(NoResString)"Bill Of Lading"); // event reference constant

			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
				pivot.DocumentTitle);
		}

		public IPrePrintProcessingResult DoPrePrintProcessing(IDocument document, BusinessObjectFactory deliveryFactory, bool isDraft)
		{
			if (isDraft)
			{
				return null;
			}

			var dateOfIssue = ZDateTime.Today;

			if (deliveryFactory != null
				&& parent is Forwarding.IForwardingShipment shipment)
			{
				var reloadedShipment = deliveryFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);

				if (reloadedShipment != null
					&& reloadedShipment.JS_HouseBillIssueDate.IsEmpty)
				{
					reloadedShipment.JS_HouseBillIssueDate = ZDateTime.Today;
				}
			}

			if (document?.Data is IDynamicData data
				&& data.Value is IHouseBill houseBill
				&& houseBill.DateOfIssue.IsEmpty)
			{
				var dateOfIssueDynamicData = data.GetDynamicProperty(nameof(houseBill.DateOfIssue));
				dateOfIssueDynamicData.SetValue(dateOfIssue);
				dateOfIssueDynamicData.AcceptChanges();

				return new PrePrintProcessingResult(
					document,
					new Dictionary<string, object>
						{
							[nameof(houseBill.DateOfIssue)] = dateOfIssue
						});
			}

			return null;
		}
	}
}
