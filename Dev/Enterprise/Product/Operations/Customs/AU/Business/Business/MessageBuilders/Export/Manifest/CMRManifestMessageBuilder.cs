using System.Collections;
using System.Collections.Specialized;
using System.Text;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRManifestMessageBuilder : CMRCUSCARMessageBuilder
	{
		public CMRManifestMessageBuilder(ExportCustomsManifestHeader header)
			: this(header, ZString.Empty)
		{
		}

		public CMRManifestMessageBuilder(ExportCustomsManifestHeader header, ZString ownerCompanyABN)
			: base(ownerCompanyABN)
		{
			Messages = header.Messages;
			manifestHeaderWrapper = new ExportCustomsManifestHeaderManifestHeaderWrapper(header);
		}

		public CMRManifestMessageBuilder(ForwardingConsol consol)
			: base(ZString.Empty)
		{
			Messages = consol.Messages;
			manifestHeaderWrapper = new FreightConsolManifestHeaderWrapper(consol);
		}

		protected override void SetAdditionalEDIMessageDetails(EDIMessage message)
		{
			if (SetStatusToPending)
			{
				message.EM_Status = EDIMessage.Status.Pending;
			}
		}

		public bool DontSendAnyLines;

		protected StringCollection fErrorList;
		protected StringCollection ErrorList
		{
			get
			{
				if (fErrorList == null)
				{
					GenerateMessageText();
				}

				return fErrorList;
			}
		}

		#region ConsolMessageBuilder

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				fErrorList = new StringCollection();
				PreGenerateMessageText();
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateLOC();
				PopulateRFF();
				PopulateTDT();
				PopulateLOCs();
				PopulateDTM();
				PopulateGroup7s();
				PopulateGISs();
				PopulateCNTs();
				PopulateUNT();
			}
		}

		protected virtual void PopulateLOC()
		{
		}

		protected virtual void PreGenerateMessageText()
		{
		}

		public int ErrorCount
		{
			get { return ErrorList.Count; }
		}

		public string Errors
		{
			get
			{
				var result = new StringBuilder();
				foreach (var error in ErrorList)
				{
					result.Append(error + "\r\n");
				}

				return result.ToString();
			}
		}

		#endregion

		#region Implementation
		protected abstract bool IsConsolidationSubManifest { get; }
		protected abstract bool IsSlotSubManifest { get; }
		protected abstract bool IncludeGoodsDescription { get; }

		protected abstract bool IsMainManifest { get; }
		protected abstract bool NILIndicatorIsAllowed { get; }
		protected abstract bool IncludeConsolLOCDetails { get; }
		protected abstract bool IncludeAdditionalTransportInfo { get; }

		protected virtual bool IsContingencyCANWriteOff
		{
			get { return false; }
		}

		protected ZString messageReferenceNumber;

		protected IManifestHeaderWrapper manifestHeaderWrapper;

		bool atLeastOneLine;

		protected abstract void PopulateRFF();

		protected void PopulateTDT()
		{
			if (!manifestHeaderWrapper.IsAir && !manifestHeaderWrapper.IsSea)
			{
				ErrorList.Add("The transport mode must be air or sea to send manifest messages");
			}

			var transportMeansDescriptionCode = manifestHeaderWrapper.IsAir ? TransportMeansDescriptionCodeList.Aircraft : (manifestHeaderWrapper.IsSea ? TransportMeansDescriptionCodeList.Ship : null);
			string airlineCode = null;
			string vesselID = null;
			string conveyanceReferenceNumber = null;
			if (IncludeAdditionalTransportInfo)
			{
				conveyanceReferenceNumber = manifestHeaderWrapper.IsAir ? manifestHeaderWrapper.FlightNumber : manifestHeaderWrapper.VoyageNumber;
				if (string.IsNullOrEmpty(conveyanceReferenceNumber))
				{
					conveyanceReferenceNumber = null;
				}

				if (manifestHeaderWrapper.IsAir)
				{
					airlineCode = manifestHeaderWrapper.AirlineCode;
				}

				if (manifestHeaderWrapper.IsSea && !manifestHeaderWrapper.VesselID.IsEmpty)
				{
					vesselID = manifestHeaderWrapper.VesselID;
				}
			}

			MessageUtilities.PopulateTDT(CUSCAR.Group4[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, conveyanceReferenceNumber, transportMeansDescriptionCode, airlineCode, CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, vesselID);
		}

		protected void PopulateLOCs()
		{
			if (IncludeConsolLOCDetails)
			{
				var lOCNumber = -1;
				if (!manifestHeaderWrapper.PortOfLoading.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC[++lOCNumber], LocationFunctionCodeQualifierList.PlaceOfDeparture, manifestHeaderWrapper.PortOfLoading, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);
				}

				if (!manifestHeaderWrapper.CountryOfDischarge.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC[++lOCNumber], LocationFunctionCodeQualifierList.CountryOfDestinationOfGoods, manifestHeaderWrapper.CountryOfDischarge, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
			}
		}

		protected virtual void PopulateDTM()
		{
			var dateOfDeparture = manifestHeaderWrapper.DateOfDeparture;
			if (manifestHeaderWrapper.CAN.IsEmpty && !manifestHeaderWrapper.CCAN.IsEmpty)
			{
				dateOfDeparture = ZDateTime.Now;
			}

			if (!dateOfDeparture.IsEmpty)
			{
				MessageUtilities.PopulateDTM(CUSCAR.Group4[0].DTM[0], DateTimePeriodFunctionCodeQualifierList.DepartureDateTime, dateOfDeparture.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
			}
			else
			{
				ErrorList.Add("A date of departure is required to send manifest messages");
			}
		}

		protected void PopulateGroup7s()
		{
			if (MessageSubType != Common.MessageBuilders.MessageSubTypes.Withdraw && !DontSendAnyLines)
			{
				var usedCANs = new ArrayList();
				var usedCCANs = new ArrayList();
				atLeastOneLine = false;
				var cMRManifestLineDetailsBuilder = new CMRManifestLineDetailsBuilder(IsMainManifest);
				foreach (var line in manifestHeaderWrapper.Lines)
				{
					if (line.LineActionCode == LineAction.Delete)
					{
						cMRManifestLineDetailsBuilder.PopulateSegment(CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection(), manifestHeaderWrapper, line);
					}
					else
					{
						ValidateLine(usedCANs, usedCCANs, line);
						if (!line.CAN.IsEmpty || !line.CCAN.IsEmpty || !line.ExemptionCode.IsEmpty)
						{
							atLeastOneLine = true;
							if (MessageSubType != Common.MessageBuilders.MessageSubTypes.ReplaceHeader)
							{
								cMRManifestLineDetailsBuilder.PopulateSegment(CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection(), manifestHeaderWrapper, line);
							}
						}
					}
				}
			}
		}

		protected virtual void ValidateLine(ArrayList usedCANs, ArrayList usedCCANs, IManifestLineWrapper line)
		{
			if (!line.CAN.IsEmpty)
			{
				if (usedCANs.Contains(line.CAN))
				{
					ErrorList.Add("The CAN '" + line.CAN + "' is used duplicated on line '" + line.Reference + "'");
				}
				else
				{
					usedCANs.Add(line.CAN);
				}

				var cANError = new Common.AU.CMR.CANValidation().GetInvalidReason(line.CAN);
				if (!cANError.IsEmpty)
				{
					ErrorList.Add("The CAN '" + line.CAN + "' on line '" + line.Reference + "' is invalid: " + cANError);
				}
			}

			if (!line.CCAN.IsEmpty)
			{
				if (usedCCANs.Contains(line.CCAN))
				{
					ErrorList.Add("The C-CAN '" + line.CCAN + "' is used duplicated on line '" + line.Reference + "'");
				}
				else
				{
					usedCCANs.Add(line.CCAN);
				}

				if (line.CCAN.Length != 14)
				{
					ErrorList.Add("The C-CAN '" + line.CCAN + "' on line '" + line.Reference + "' is not 14 characters long.");
				}
			}

			if (line.CAN.IsEmpty && line.CCAN.IsEmpty && line.ExemptionCode.IsEmpty)
			{
				ErrorList.Add("The line '" + line.Reference + "' does not have a CAN, CCAN or Exemption code entered for it.");
			}

			if (manifestHeaderWrapper.IsAir && line.PackCount == 0)
			{
				ErrorList.Add("The line '" + line.Reference + "' has zero packages which is invalid for air manifests.");
			}

			if (manifestHeaderWrapper.IsSea && line.PackCount == 0 && line.ContainerCount == 0)
			{
				ErrorList.Add("The line '" + line.Reference + "' has zero packages and zero containers which is invalid for sea manifests.");
			}
		}

		protected void PopulateGISs()
		{
			var gISNumber = -1;
			AUCProcessingIndicatorDescriptionCodeList aUCProcessingIndicatorDescriptionCode;
			CodeListIdentificationCodeList codeListIdentificationCode;

			if (!atLeastOneLine && NILIndicatorIsAllowed)
			{
				aUCProcessingIndicatorDescriptionCode = AUCProcessingIndicatorDescriptionCodeList.NilCargoReportIndicator;
				codeListIdentificationCode = CodeListIdentificationCodeList.CustomsIndicator;
				MessageUtilities.PopulateGIS(CUSCAR.GIS[++gISNumber], aUCProcessingIndicatorDescriptionCode, codeListIdentificationCode, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}

			if (IsConsolidationSubManifest)
			{
				aUCProcessingIndicatorDescriptionCode = AUCProcessingIndicatorDescriptionCodeList.ConsolidationSubManifest;
				codeListIdentificationCode = CodeListIdentificationCodeList.ShipmentDescription;
				MessageUtilities.PopulateGIS(CUSCAR.GIS[++gISNumber], aUCProcessingIndicatorDescriptionCode, codeListIdentificationCode, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}

			if (IsSlotSubManifest)
			{
				aUCProcessingIndicatorDescriptionCode = AUCProcessingIndicatorDescriptionCodeList.SlotSubManifest;
				codeListIdentificationCode = CodeListIdentificationCodeList.ShipmentDescription;
				MessageUtilities.PopulateGIS(CUSCAR.GIS[++gISNumber], aUCProcessingIndicatorDescriptionCode, codeListIdentificationCode, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected void PopulateCNTs()
		{
			var packageNumber = manifestHeaderWrapper.TotalPackageCount;
			if (packageNumber != 0)
			{
				MessageUtilities.PopulateCNT(CUSCAR.CNT.InstantiateAChildAndAddItToChildrenCollection(), ControlTotalTypeCodeQualifierList.TotalNumberOfPackages, packageNumber.ToString());
			}

			var emptyContainerNumber = manifestHeaderWrapper.TotalEmptyContainerCount;
			if (!manifestHeaderWrapper.IsAir && emptyContainerNumber != 0)
			{
				MessageUtilities.PopulateCNT(CUSCAR.CNT.InstantiateAChildAndAddItToChildrenCollection(), ControlTotalTypeCodeQualifierList.TotalNumberOfEmptyContainers, emptyContainerNumber.ToString());
			}

			var containerNumber = manifestHeaderWrapper.TotalContainerCount;
			if (!manifestHeaderWrapper.IsAir && containerNumber != 0)
			{
				MessageUtilities.PopulateCNT(CUSCAR.CNT.InstantiateAChildAndAddItToChildrenCollection(), ControlTotalTypeCodeQualifierList.NumberOfContainersToBeLoaded, containerNumber.ToString());
			}
		}

		#endregion
	}
}
