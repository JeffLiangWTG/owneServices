using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDMessageBuilder : CMRCUSDECMessageBuilder
	{
		protected internal override ZString DocumentName => CMRMessage.CMRMessageTypes.EXD;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.EXD;

		protected internal override Type TypeOfMessage => typeof(CMREXDMessage);

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.GoodsDeclarationForExportation;

		protected int group1Number;

		protected internal override void GenerateMessageText()
		{
			if (cUSDEC == null)
			{
				ZString exportDeclarationNumber = Declaration.DeclarationNumber;

				ZString exportGoodsType = Declaration.JE_ExportGoodsType.Trim();

				bool isPostal = exportGoodsType == "PO";

				cUSDEC = new CUSDECMessage();
				//Heading Section

				PopulateUNH();
				PopulateBGM();

				int lOCSegmentNumber = 0;
				//Port of Loading
				if (!Declaration.JE_RL_NKPortOfLoading.IsEmpty)
				{
					MessageUtilities.PopulateLOC(cUSDEC.LOC[lOCSegmentNumber++], LocationFunctionCodeQualifierList.PlacePortOfLoading, Declaration.JE_RL_NKPortOfLoading, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}

				//Port of Discharge
				if (!Declaration.JE_RL_NKPortOfArrival.IsEmpty)
				{
					MessageUtilities.PopulateLOC(cUSDEC.LOC[lOCSegmentNumber++], LocationFunctionCodeQualifierList.PortOfDischarge, Declaration.JE_RL_NKPortOfArrival, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}

				//Country of Destination
				string countryOfDestination = Declaration.FinalDestination == null ? null : Declaration.FinalDestination.RL_RN_NKCountryCode;
				if (countryOfDestination != null && !string.IsNullOrEmpty(countryOfDestination))
				{
					MessageUtilities.PopulateLOC(cUSDEC.LOC[lOCSegmentNumber++], LocationFunctionCodeQualifierList.CountryOfDestinationOfGoods, countryOfDestination, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}

				//establishment code
				if (!Declaration.WarehouseID.IsEmpty)
				{
					MessageUtilities.PopulateLOC(cUSDEC.LOC[lOCSegmentNumber++], LocationFunctionCodeQualifierList.Warehouse, Declaration.WarehouseID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
				}

				//Export date
				ZString exportationDateString = Declaration.JE_ExportDate.ToString("yyyyMMdd");
				if (!exportationDateString.IsEmpty)
				{
					MessageUtilities.PopulateDTM(cUSDEC.DTM[0], DateTimePeriodFunctionCodeQualifierList.ExportationDate, exportationDateString, DateTimePeriodFormatCodeList.Ccyymmdd);
				}

				//Confirming export type
				ProcessingIndicatorDescriptionCodeList exportType = ProcessingIndicatorDescriptionCodeList.GetFromString(GetExportType());
				MessageUtilities.PopulateGIS(cUSDEC.GIS[0], exportType, CodeListIdentificationCodeList.FunctionalGroup, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);

				//Excisable?
				ProcessingIndicatorDescriptionCodeList excisable = AUCProcessingIndicatorDescriptionCodeList.GetFromString(GetExcisableIndicator());//AUCProcessingIndicatorDescriptionCodeList.Yes;
				MessageUtilities.PopulateGIS(cUSDEC.GIS[1], excisable, CodeListIdentificationCodeList.ExciseDuty, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);

				//Prescribed Goods?
				ProcessingIndicatorDescriptionCodeList prescribed = AUCProcessingIndicatorDescriptionCodeList.GetFromString(GetPrescribedIndicator());
				MessageUtilities.PopulateGIS(cUSDEC.GIS[2], prescribed, CodeListIdentificationCodeList.ForwardingRestrictions, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);

				group1Number = 0;
				string reportingPartyType = GetReportingPartyType();
				MessageUtilities.PopulateRFF(cUSDEC.Group1[group1Number++].RFF[0], ReferenceFunctionCodeQualifierList.CategoryOfWorkReference, reportingPartyType, null);

				PopulateConsolReference();

				if (!exportDeclarationNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(cUSDEC.Group1[group1Number++].RFF[0], ReferenceFunctionCodeQualifierList.ExportDeclaration, exportDeclarationNumber, null);
				}

				ZString exportCargoType = "";
				switch (Declaration.JE_ContainerMode)
				{
					case Core.Constants.TransportModes.Air:
					case "":
						exportCargoType = "N";
						break;
					case Core.Constants.ContainerModes.Liquid:
					case Core.Constants.ContainerModes.Bulk:
						exportCargoType = "B";
						break;
					case Core.Constants.ContainerModes.Combination:
						exportCargoType = "CO";
						break;
					case Core.Constants.ContainerModes.Containerised:
						exportCargoType = "C";
						break;
					case Core.Constants.ContainerModes.NonContainerised:
						exportCargoType = "N";
						break;
				}

				if (!exportCargoType.IsEmpty)
				{
					MessageUtilities.PopulatePAC(cUSDEC.Group1[group1Number].Group2.InstantiateAChildAndAddItToChildrenCollection().PAC[0], exportCargoType, CodeListIdentificationCodeList.TypeOfPackage, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
				}

				if (!exportGoodsType.IsEmpty)
				{
					MessageUtilities.PopulatePAC(cUSDEC.Group1[group1Number].Group2.InstantiateAChildAndAddItToChildrenCollection().PAC[0], exportGoodsType, CodeListIdentificationCodeList.MeansOfTransportIdentification, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
				}

				string flightVoyageNumber = Declaration.IsSea ? GetVoyogeNumber() : GetFlightNumber();
				string airlineCode = GetAirlineCode();
				string vesselID = GetVesselID();
				TransportMeansDescriptionCodeList transportMeans = GetTransportMeans();
				CodeListResponsibleAgencyCodeList carrierResponsibleAgency = airlineCode != null && airlineCode.Length > 0 ? CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation : null;

				if (!isPostal)
				{
					MessageUtilities.PopulateTDT(cUSDEC.Group4[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, flightVoyageNumber, transportMeans, airlineCode, carrierResponsibleAgency, vesselID);
				}

				var consigneeName = ZString.Empty;
				if (Declaration.Importer != null)
				{
					if (Declaration.Importer.IsMiscellaneous)
					{
						consigneeName = Declaration.ZA_ConsigneeNameHidden;
					}
					else
					{
						consigneeName = Declaration.Importer.OH_FullNameTruncated;
					}
				}
				if (!consigneeName.IsEmpty)
				{
					ZString consigneeCity = Declaration.ImporterCity;
					if (!consigneeCity.IsEmpty)
					{
						MessageUtilities.PopulateNAD(cUSDEC.Group6[0].NAD[0], PartyFunctionCodeQualifierList.Consignee, null, null, consigneeName, consigneeCity);
					}
				}

				if (Declaration.SupplierWrapper != null && !Declaration.SupplierWrapper.IsCompanyOrg)
				{
					ZString goodsOwnerPartyID = Declaration.SupplierWrapper.GoodsOwnerPartyID;
					if (!goodsOwnerPartyID.IsEmpty)
					{
						MessageUtilities.PopulateNAD(cUSDEC.Group6[1].NAD[0], PartyFunctionCodeQualifierList.GoodsOwner, goodsOwnerPartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
					}
				}

				var invoiceCurrency = Declaration.GoodsValue.Currency == null ? ZString.Empty : (ZString)Declaration.GoodsValue.Currency.Code;
				if (!invoiceCurrency.IsEmpty)
				{
					MessageUtilities.PopulateMOA(cUSDEC.Group8[0].MOA[0], MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount, null, invoiceCurrency);
				}

				//Section Control
				MessageUtilities.PopulateUNS(cUSDEC.UNS1[0], SectionIdentificationList.HeaderDetailSectionSeparation);

				long totalFOBAmount = 0;
				//Detail Section
				int lineNumber = 0;

				if (EntryHeader != null)
				{
					foreach (var myLine in EntryHeader.MergedLines.Cast<CusEntryLine>())
					{
						var group30Builder = new EXDSegmentGroup30EntryLineBuilder(myLine, cUSDEC.Group30[lineNumber], lineNumber);
						group30Builder.PopulateSegment();
						lineNumber++;
						totalFOBAmount += group30Builder.RoundedFOBValue;
					}
				}
				else
				{
					foreach (var myLine in Declaration.SortedInvoiceLines)
					{
						var group30Builder = new EXDSegmentGroup30Builder(myLine, cUSDEC.Group30[lineNumber], lineNumber);
						group30Builder.PopulateSegment();
						lineNumber++;
						totalFOBAmount += group30Builder.RoundedFOBValue;
					}
				}

				if (!invoiceCurrency.IsEmpty)
				{
					MessageUtilities.PopulateMOA(cUSDEC.Group8[1].MOA[0], MonetaryAmountTypeCodeQualifierList.FobValue, totalFOBAmount.ToString(), invoiceCurrency);
				}

				//Section Control
				MessageUtilities.PopulateUNS(cUSDEC.UNS2[0], SectionIdentificationList.DetailSummarySectionSeparation);

				//Summary Section
				PopulateCNTSegments();
				PopulateUNT();
			}
		}

		public EXDMessageBuilder(JobDeclaration declaration, object myDecType)
		{
			DeclarationType = (ExportDeclarationType)myDecType;

			MessageSubType = GetMessageSubType(declaration, DeclarationType);
			Messages = declaration.Messages;
			if (declaration == null)
			{
				throw new ApplicationException("EXD Message builder requires a valid job declaration to be passed to it.  The parameter passed was empty.");
			}
			this.Declaration = declaration;
			EntryHeader = declaration.EntryHeader;
			MessageInitiator = declaration.GetMessageInitiatorSafe();
		}

		public EXDMessageBuilder(CusEntryHeader cusEntryHeader, object myDecType)
		{
			if (cusEntryHeader.Declaration == null)
			{
				throw new ApplicationException("EXD Message builder requires a valid job declaration to be passed to it.  The parameter passed was empty.");
			}

			Declaration = cusEntryHeader.Declaration;
			DeclarationType = (ExportDeclarationType)myDecType;
			EntryHeader = cusEntryHeader;
			MessageInitiator = Declaration.GetMessageInitiatorSafe();
			Messages = EntryHeader.Messages;
			MessageSubType = GetMessageSubType(Declaration, DeclarationType);
		}

		protected CMRMessage myZEDIMessage;

		protected readonly JobDeclaration Declaration;

		protected readonly CusEntryHeader EntryHeader;

#if DEBUG
		internal static int CountSegments(string messageString)
		{
			int result = 0;
			bool lastCharWasEscapeChar = false;
			foreach (char @char in messageString)
			{
				if (!lastCharWasEscapeChar)
				{
					if (@char == '\'')
					{
						result++;
					}

					if (@char == '?')
					{
						lastCharWasEscapeChar = true;
					}
				}
				else
				{
					lastCharWasEscapeChar = false;
				}
			}
			return result;
		}
#endif

		protected ExportDeclarationType DeclarationType
		{
			get { return fDeclarationType; }
			set { fDeclarationType = value; }
		}
		ExportDeclarationType fDeclarationType;

		#region Export Type

		string GetExportType()
		{
			string result = "";

			if (DeclarationType == ExportDeclarationType.Confirming)
			{
				result = "Y";
			}
			else if (DeclarationType == ExportDeclarationType.Confirmed)
			{
				result = "C";
			}
			else
			{
				result = "N";
			}

			return result;
		}

		#endregion

		#region Implementation

		protected virtual void PopulateConsolReference()
		{
		}

		protected void PopulateCNTSegments()
		{
			ZInt numberOfPackages = (Declaration.IsContainerised || Declaration.IsBulk || Declaration.IsLiquid) ? new ZInt(0) : Declaration.JE_TotalNoOfPacks;
			MessageUtilities.PopulateCNT(cUSDEC.CNT[0], ControlTotalTypeCodeQualifierList.TotalNumberOfPackages, numberOfPackages.ToString());

			string numberOfContainers = Declaration.JE_ContainerCount.ToString();
			MessageUtilities.PopulateCNT(cUSDEC.CNT[1], ControlTotalTypeCodeQualifierList.NumberOfContainersToBeLoaded, numberOfContainers);
		}

		#region Business (dont move)
		string GetFlightNumber()
		{
			//FLIGHT NUMBER MUST NOT BE PROVIDED WHERE (1) MODE OF TRANSPORT IS SEA, OR (2) EXPORT GOODS TYPE IS OP (OWN POWER) OR OT (OTHER) AND MODE OF TRANSPORT IS AIR.'
			bool shipTransport = GetTransportMeans() == TransportMeansDescriptionCodeList.Ship.ToString();
			bool airTransport = GetTransportMeans() == TransportMeansDescriptionCodeList.Aircraft.ToString();
			bool ownPowerOrPostal = (Declaration.JE_ExportGoodsType.Trim() == "OP" || Declaration.JE_ExportGoodsType.Trim() == "OT");
			bool leaveOutFlightNumber = shipTransport || (ownPowerOrPostal && airTransport);

			if (leaveOutFlightNumber)
			{
				return null;
			}
			else
			{
				return Declaration.JE_VoyageFlightNo;
			}
		}
		#endregion

		#region Business (should be moved!)

		string GetExcisableIndicator()
		{
			return Declaration.AddInfo.ZA_ExcisableGoods_Hidden.ToString();
		}

		string GetPrescribedIndicator()
		{
			return Declaration.AddInfo.ZA_PrescribedGoods_Hidden.ToString();
		}

		protected static Common.MessageBuilders.MessageSubTypes GetMessageSubType(JobDeclaration declaration, ExportDeclarationType decType)
		{
			if (decType == ExportDeclarationType.Withdrawal)
			{
				return Common.MessageBuilders.MessageSubTypes.Withdraw;
			}
			else if (declaration.DeclarationNumber.IsEmpty)
			{
				return Common.MessageBuilders.MessageSubTypes.Create;
			}
			else
			{
				return Common.MessageBuilders.MessageSubTypes.Replace;
			}
		}

		string GetReportingPartyType()
		{
			return Declaration.JE_OH_Supplier == GlbCompany.CurrentCompany.GC_OH_OrgProxy ? "O" : "A";
		}

		TransportMeansDescriptionCodeList GetTransportMeans()
		{
			switch (Declaration.JE_TransportMode)
			{
				case Enterprise.Core.Constants.TransportModes.Air:
					return TransportMeansDescriptionCodeList.Aircraft;
				case Enterprise.Core.Constants.TransportModes.Sea:
					return TransportMeansDescriptionCodeList.Ship;
				default:
					return null;
			}
		}

		string GetAirlineCode()
		{
			string result = null;
			if (Declaration.IsAir && IsAirLineMandatory)
			{
				result = Declaration.JE_VoyageFlightNo.Replace(" ", "").SubstringSafe(0, 2);
			}
			return result;
		}

		bool IsAirLineMandatory
		{
			get
			{
				return Declaration.JE_ExportGoodsType == JobDeclaration.ExportGoodsType.Stores
					|| Declaration.JE_ExportGoodsType == JobDeclaration.ExportGoodsType.SpareParts
					|| Declaration.JE_ExportGoodsType == JobDeclaration.ExportGoodsType.AccompaniedBaggage;
			}
		}

		string GetVesselID()
		{
			if (Declaration.JE_TransportMode == Enterprise.Core.Constants.TransportModes.Sea && Declaration.JE_ExportGoodsType != "OP" && Declaration.JE_ExportGoodsType != "OT" && !Declaration.VesselNumber.IsEmpty)
			{
				return Declaration.VesselNumber;
			}
			return null;
		}

		string GetVoyogeNumber()
		{
			if (Declaration.JE_TransportMode == Enterprise.Core.Constants.TransportModes.Sea && Declaration.JE_ExportGoodsType != "OP" && Declaration.JE_ExportGoodsType != "OT")
			{
				return Declaration.JE_VoyageFlightNo;
			}
			return null;
		}

		#endregion
	}

	#endregion
}
