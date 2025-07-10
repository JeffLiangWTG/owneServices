using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.D04A.Elements;
using Enterprise.Edifact.D04A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	public class CUKG2GCreator
	{
		public CUKG2GCreator(IG2gHeader source, ErrorCollector errorCollector)
		{
			this.source = source;
			this.errorCollector = errorCollector;
		}

		public string MakeMessageText()
		{
			return MakeMessageText(new UkCharSet());
		}

		public string MakeMessageText(UNCharacterSet charSet)
		{
			MakeMessage();
			return result.ToString(charSet);
		}

		public CUKG2GMessage MakeMessage()
		{
			errorCollector.WipeErrors();
			result = new CUKG2GMessage();
			if (source.AgentType.IsEmpty)
			{
				errorCollector.AddError("Only Type 1 or Type 2 agents may send G2G messages. Check your registry to ensure that your badge's credential has the correct agent type.");
			}
			else if (!Environment.Env.Security.AirCcsukSendExportFallbackGood2GoMessage.IsAllowed)
			{
				errorCollector.AddError(Environment.Env.Security.AirCcsukSendExportFallbackGood2GoMessage.ErrorMessageForNotAllowed);
			}
			else
			{
				MakeUNH();
				MakeBGM();
				MakeGroup1();
				MakeGroup2And3();
				MakeUNT();
			}
			return result;
		}

		void MakeUNH()
		{
			var unh = result.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageIdentifier.MessageType = "CUKG2G";
			unh.MessageIdentifier.MessageVersionNumber = "A";
			unh.MessageIdentifier.MessageReleaseNumber = "04A";
			unh.MessageIdentifier.ControllingAgency = "BT";
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;
		}

		void MakeBGM()
		{
			var bgm = result.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString("740");
			var mawb = source.MawpAndMawn;
			bgm.DocumentMessageIdentification.DocumentIdentifier = mawb;
			if (mawb.IsEmpty)
			{
				errorCollector.AddError("MAWB number");
			}
		}

		void MakeGroup1()
		{
			var grp1 = result.Group1.InstantiateAChildAndAddItToChildrenCollection();
			if (!source.MasterUcr.IsEmpty)
			{
				PopulateRff("UCN", source.MasterUcr, grp1.RFF.InstantiateAChildAndAddItToChildrenCollection());
			}
			if (!source.MawpAndMawn.IsEmpty)
			{
				PopulateRff("AWB", source.MawpAndMawn, grp1.RFF.InstantiateAChildAndAddItToChildrenCollection());
			}
			if (!source.CustomsAuthorisationReference.IsEmpty)
			{
				PopulateRff("ZZZ", source.CustomsAuthorisationReference, grp1.RFF.InstantiateAChildAndAddItToChildrenCollection());
			}
			if (!source.MasterSOE.IsEmpty)
			{
				var gei1 = grp1.GEI1.InstantiateAChildAndAddItToChildrenCollection();
				gei1.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("SOE");
				gei1.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(source.MasterSOE);

				var loc = grp1.LOC.InstantiateAChildAndAddItToChildrenCollection();
				loc.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("14");
				loc.LocationIdentification.LocationNameCode = source.Airport;
				loc.LocationIdentification.LocationName = source.Shed;
				loc.LocationIdentification.CodeListIdentificationCode = "145";
				loc.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation; //3
			}

			if (!source.MasterSOE.IsEmpty && (source.Airport.IsEmpty || source.Shed.IsEmpty))
			{
				errorCollector.AddError("Master SOE is supplied, so consol airport and shed are required");
			}

			var agentBadge = source.AgentBadge;
			if (!agentBadge.IsEmpty)
			{
				var nad = grp1.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString("CB");
				nad.PartyIdentificationDetails.PartyIdentifier = agentBadge;
			}
			else
			{
				errorCollector.AddError("Agent badge (from PIMA/profile)");
			}

			var agentType = source.AgentType;
			if (!agentType.IsEmpty)
			{
				var gei2 = grp1.GEI2.InstantiateAChildAndAddItToChildrenCollection();
				gei2.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("4");
				gei2.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(agentType);
			}
			else
			{
				errorCollector.AddError("Agent type (from registry via PIMA)");
			}
		}

		void MakeGroup2And3()
		{
			int consignmentIndex = 1;
			foreach (var child in source.Consignments)
			{
				var grp2 = result.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var gid = grp2.GID.InstantiateAChildAndAddItToChildrenCollection();
				gid.GoodsItemNumber = consignmentIndex.ToString();
				consignmentIndex++;
				PopulateGroup2_ConsignmentOnMaster(grp2, child);
			}
			if (consignmentIndex == 1) // still
			{
				errorCollector.AddError("No relevant consignments are present in this MAWB");
			}
		}

		void PopulateGroup2_ConsignmentOnMaster(CUKG2GSegmentGroup2 grp2, IG2gConsignment consignment)
		{
			var hawbNo = consignment.HouseAWBNumber;
			if (!hawbNo.IsEmpty)
			{
				PopulateRff("HWB", hawbNo, grp2.RFF.InstantiateAChildAndAddItToChildrenCollection());
			}
			if (!consignment.CustomsAuthorisationReference.IsEmpty)
			{
				PopulateRff("ZZZ", consignment.CustomsAuthorisationReference, grp2.RFF.InstantiateAChildAndAddItToChildrenCollection());
			}
			int declarationCount = 0;
			foreach (var declaration in consignment.Declarations)
			{
				MakeGroup3_DeclarationOnConsignment(grp2, declaration, consignment);
				if (!declaration.DeclarationUcr.IsEmpty)
				{
					declarationCount++;
				}
			}

			if (declarationCount == 0)
			{
				if (source.AgentType != AgentTypeForExportFallbackList.Codes.Type2)
				{
					if (source.CustomsAuthorisationReference.IsEmpty && consignment.CustomsAuthorisationReference.IsEmpty)
					{
						errorCollector.AddError("Type 1 agents cannot send G2G without a consol- or shipment-level customs authorisaton reference and without any valid declarations. Add a CAR to the AWB or add a DUCR or CAR to " + consignment.CargoWiseNumber);
					}
				}
			}
		}

		void MakeGroup3_DeclarationOnConsignment(CUKG2GSegmentGroup2 grp2, IG2gDeclaration declaration, IG2gConsignment consignment)
		{
			var ducr = declaration.DeclarationUcr;
			if (!declaration.DeclarationUcr.IsEmpty)
			{
				var grp3 = grp2.Group3.InstantiateAChildAndAddItToChildrenCollection();
				PopulateRff("ABO", ducr, grp3.RFF1.InstantiateAChildAndAddItToChildrenCollection(), declaration.DeclarationUcrPart);
				if (!declaration.DeclarationEpu.IsEmpty)
				{
					var loc1 = grp3.LOC1.InstantiateAChildAndAddItToChildrenCollection();
					loc1.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("22");
					loc1.LocationIdentification.LocationNameCode = declaration.DeclarationEpu;
				}
				if (!declaration.DeclarationENo.IsEmpty)
				{
					PopulateRff("ABT", declaration.DeclarationENo, grp3.RFF2.InstantiateAChildAndAddItToChildrenCollection());
				}
				if (!declaration.DeclarationDoe.IsEmpty)
				{
					var dtm = grp3.DTM.InstantiateAChildAndAddItToChildrenCollection();
					dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("7");
					dtm.DateTimePeriod.DateOrTimeOrPeriodText = declaration.DeclarationDoe.ToString("yyyyMMdd");
					dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("102");
				}
				if (!declaration.DeclarationSoe.IsEmpty)
				{
					var gei = grp3.GEI.InstantiateAChildAndAddItToChildrenCollection();
					gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("SOE");
					gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(declaration.DeclarationSoe);
				}

				if (declaration.IsRouteH())
				{ }  // route H or route H with a probable firm route (H1, H6, etc) - don't send airport/shed
				else
				{
					if (EnsureBothAirportAndShed(declaration) && !declaration.AirportCode.IsEmpty)
					{
						var loc1 = grp3.LOC2.InstantiateAChildAndAddItToChildrenCollection();
						loc1.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("14");
						loc1.LocationIdentification.LocationNameCode = declaration.AirportCode;
						loc1.LocationIdentification.LocationName = declaration.ShedOpId;
						loc1.LocationIdentification.CodeListIdentificationCode = "145";
						loc1.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation; //3
					}
				}

				if (!declaration.CustomsAuthorisationReference.IsEmpty)
				{
					PopulateRff("ZZZ", declaration.CustomsAuthorisationReference, grp3.RFF3.InstantiateAChildAndAddItToChildrenCollection());
				}
				CrossReferenceCarWithDucrAndAgentType(declaration);
			}
		}

		void MakeUNT()
		{
			var unt = result.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = result.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		void PopulateRff(string qualifier, string value, RFFSegment rff, string subValue = null)
		{
			rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(qualifier);
			rff.Reference.ReferenceIdentifier = value;
			if (subValue != null)
			{
				rff.Reference.DocumentLineIdentifier = subValue;
			}
		}

		bool EnsureBothAirportAndShed(IG2gDeclaration declaration)
		{
			var isOk = true;
			if ((declaration.AirportCode.IsEmpty && !declaration.ShedOpId.IsEmpty) || (!declaration.AirportCode.IsEmpty && declaration.ShedOpId.IsEmpty))
			{
				errorCollector.AddError("Declaration's airport and shed must be supplied together when route is not H. " + declaration.CargoWiseNumber);
				isOk = false;
			}
			return isOk;
		}

		//Validation for declarations
		void CrossReferenceCarWithDucrAndAgentType(IG2gDeclaration declaration)
		{
			if (declaration.DeclarationDoe.IsEmpty || declaration.DeclarationSoe.IsEmpty || declaration.DeclarationENo.IsEmpty || declaration.DeclarationEpu.IsEmpty)
			{
				if (declaration.CustomsAuthorisationReference.IsEmpty)
				{
					if (source.AgentType != AgentTypeForExportFallbackList.Codes.Type2)
					{
						errorCollector.AddError("Type 1 agents cannot omit both the customs authorisation reference and the declaration details. Obtain a CAR from the NCH and supply it. " + declaration.CargoWiseNumber);
					}
				}
			}
			else
			{
				if (source.AgentType == AgentTypeForExportFallbackList.Codes.Type1)
				{
					if (declaration.IsSoe7())
					{
						// CAR is optional
					}
					else if (declaration.IsSoe1() && declaration.IsRouteH())
					{
						if (declaration.CustomsAuthorisationReference.IsEmpty)
						{
							errorCollector.AddError("Type 1 agents must supply a CAR for route=H asnd SOE=1 declarations");
						}
					}
					else
					{
						errorCollector.AddError("Type 1 agents cannot export the goods on " + declaration.CargoWiseNumber + ". Only declarations with SOE=7, or SOE=1 & route=H, can be sent.");
					}
				}
				else if (source.AgentType == AgentTypeForExportFallbackList.Codes.Type2)
				{
					if (!declaration.IsSoe7() && !declaration.IsRouteH())
					{
						if (declaration.IsAtDEP())
						{
							errorCollector.AddError("Type 2 agents cannot send for declarations at a DEP with SOE not 7 and route not H");
						}
						else if (declaration.CustomsAuthorisationReference.IsEmpty)
						{
							errorCollector.AddError("Type 2 agents cannot send for declarations at a frontier location with SOE not 7 and route not H unless a CAR is supplied. Supply a CAR. ");
						}
					}
				}
			}
		}

		CUKG2GMessage result;
		readonly IG2gHeader source;
		readonly ErrorCollector errorCollector;
	}

	static class G2gDeclarationExtensions
	{
		internal static bool IsRouteH(this IG2gDeclaration declaration)
		{
			return declaration.DeclarationRoute == RouteOfEntryList.Codes.PreLodgePrefixTheDeclarationIsAnAdvancePreLodgedTypeAndAsSuchTheRoutingIsProvisionalOnlyChiefEquivalentH || declaration.DeclarationRoute.StartsWith(RouteOfEntryList.Codes.PreLodgePrefixTheDeclarationIsAnAdvancePreLodgedTypeAndAsSuchTheRoutingIsProvisionalOnlyChiefEquivalentH);
		}

		internal static bool IsSoe7(this IG2gDeclaration declaration)
		{
			return declaration.DeclarationSoe == ExportStyleOfEntries.Codes.PermittedToProgress;
		}

		internal static bool IsSoe1(this IG2gDeclaration declaration)
		{
			return declaration.DeclarationSoe == ExportStyleOfEntries.Codes.ConsignmentSubjectToAssociatedRoute;
		}

		internal static bool IsAtDEP(this IG2gDeclaration declaration)
		{
			return declaration.ShedOpId.StartsWith("X");
		}
	}
}
