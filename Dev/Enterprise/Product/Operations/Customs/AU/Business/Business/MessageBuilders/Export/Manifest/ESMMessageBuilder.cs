using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ESMMessageBuilder : CMRManifestMessageBuilder, IManifestMessageBuilder
	{
		public ESMMessageBuilder(ExportCustomsManifestHeader header, bool isSlot)
			: base(header)
		{
			this.isSlot = isSlot;
		}

		public ESMMessageBuilder(ForwardingConsol consol, bool isSlot)
			: base(consol)
		{
			this.isSlot = isSlot;
			this.consol = consol;
		}

		public ZString ManifestMessageTypeCode => "ESM";

		protected override void ValidateLine(ArrayList usedCANs, ArrayList usedCCANs, IManifestLineWrapper line)
		{
			base.ValidateLine(usedCANs, usedCCANs, line);
			ZString exemptionCode = line.ExemptionCode;
			if (!exemptionCode.IsEmpty && !isSlot)
			{
				if (CMRExportExemptionCodes.GetFromExit2Exemption(exemptionCode) == CANType.Exemptions.EXLV.Code || CMRExportExemptionCodes.GetFromExit2Exemption(exemptionCode) == CANType.Exemptions.EXPE.Code)
				{
					if (line.GoodsDescription.IsEmpty)
					{
						ErrorList.Add("Because of the exemption code used on line '" + line.Reference + "', a goods description must be supplied.");
					}

					if (line.CountryOfDestination.IsEmpty)
					{
						ErrorList.Add("Because of the exemption code used on line '" + line.Reference + "', a country/region of destination must be supplied.");
					}

					if (line.GoodsOwner.IsEmpty && line.GoodsOwnerPartyID.IsEmpty)
					{
						ErrorList.Add("Because of the exemption code used on line '" + line.Reference + "', a goods owner must be supplied.");
					}
				}
			}
		}

		protected override void PopulateLOC()
		{
			if (!manifestHeaderWrapper.DepotPremiseID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC[0], LocationFunctionCodeQualifierList.PlaceOfConsolidation, manifestHeaderWrapper.DepotPremiseID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected override void PreGenerateMessageText()
		{
			base.PreGenerateMessageText();
			foreach (string error in new CMRUtilities().GetCertificateErrors(Messages.Factory))
			{
				ErrorList.Add(error);
			}

			if (consol != null)
			{
				CheckESMConsignmentCount();
			}
		}

		public static Common.MessageBuilders.MessageSubTypes GetMessageSubType(ForwardingConsol consol)
		{
			var wrapper = new FreightConsolWrapper(consol);
			return wrapper.ExportManifestMessageType;
		}

		protected override void PopulateRFF()
		{
			if (MessageSubType != Common.MessageBuilders.MessageSubTypes.Create)
			{
				ZString consolidationReferenceNumber = manifestHeaderWrapper.CAN;
				if (!consolidationReferenceNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF[0], ReferenceFunctionCodeQualifierList.ConsolidatedInvoiceNumber, consolidationReferenceNumber, null);
				}
			}
		}

		void CheckESMConsignmentCount()
		{
			if (consol.IsExport() && consol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.Australia, StringComparison.OrdinalIgnoreCase))
			{
				var maximumConsignmentLines = ZArchitecture.Environment.DataRegistry.Instance.MaximumConsignmentsLines;

				if (consol.HVLVConsignmentCount > maximumConsignmentLines)
				{
					ErrorList.Add("The total consignment lines for this Consol exceeds the maximum allowed consignments for an Export Sub Manifest Message.");
				}
			}
		}

		protected override bool IsSlotSubManifest => isSlot;

		protected override bool IsConsolidationSubManifest => !isSlot;

		protected override bool IncludeGoodsDescription => true;

		protected override bool IsMainManifest => false;

		protected override bool NILIndicatorIsAllowed => isSlot;

		protected override bool IncludeConsolLOCDetails => false;

		protected override bool IncludeAdditionalTransportInfo => false;

		protected internal override ZString DocumentName => CMRMessage.CMRMessageTypes.ESM;

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.GeneralCargoSummaryManifestReport;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.ESM;

		protected internal override Type TypeOfMessage => typeof(CMRESMMessage);

		protected bool isSlot;

		readonly ForwardingConsol consol;
	}
}
