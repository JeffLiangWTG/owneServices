using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.CusRes;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class UkcinvUnderstander
	{
		public enum MessageClasses
		{
			/// <summary>
			/// Export associate consignment
			/// </summary>
			EAC,

			/// <summary>
			/// Consignment Status – Shut Master
			/// </summary>
			CST,

			/// <summary>
			/// Export advance (consignment) advice
			/// </summary>
			EAA,

			/// <summary>
			/// Export arrival at a location
			/// </summary>
			EAL,

			/// <summary>
			/// Export departure from a location
			/// </summary>
			EDL,

			/// <summary>
			/// Export route or status change notification
			/// </summary>
			ERS,

			/// <summary>
			/// Asynchronous Master arrival response
			/// </summary>
			EMR,

			/// <summary>
			/// Display an Export consignment (Master or Declaration UCR). Response to CUSDEC/DEC.
			/// </summary>
			DEC,

			/// <summary>
			/// List export movements. Response to CUSDEC/LEM.
			/// </summary>
			LEM
		}

		protected UkCinvMessage ukCinv;
		public UkcinvUnderstander(UkCinvMessage messageToUnderstand)
		{
			ukCinv = messageToUnderstand;
		}

		public void ParseEmrErsEaaEal(EDIMessage inboundMessage, CusEntryHeader entryLinkedFromSysCar = null)
		{
			var result = false;
			if (Class == MessageClasses.EMR)
			{
				var report = new UkcinvUnderstanderEMR(ukCinv).Parse();
				result = new EmrReportProcessor((EmrReport)report, inboundMessage, this).DoAllProcessing();
			}
			else if (Class == MessageClasses.ERS)
			{
				var report = new UkcinvUnderstanderERS(ukCinv).Parse();
				result = new ErsReportProcessor(report, inboundMessage, this).DoAllProcessing();
			}
			else if (Class == MessageClasses.EAA || Class == MessageClasses.EAL)
			{
				var report = new UkcinvUnderstanderEaaEal(ukCinv).Parse();
				result = new EaaEalReportProcessor((EaaEalReport)report, inboundMessage, this).DoAllProcessing();
			}
			inboundMessage.EM_Status = result ? EDIMessage.Status.Received : EDIMessage.Status.Failed;
		}

		public MessageClasses Class
		{
			get
			{
				string classOfMessage = ukCinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString();
				return (MessageClasses)Enum.Parse(typeof(MessageClasses), classOfMessage, false);
			}
		}

		public string LevelOfMessage
		{
			get
			{
				ZString headerMucr = HeaderLevelMucr;
				ZString headerDucr = HeaderLevelDucr;
				if (headerMucr.IsEmpty && !headerDucr.IsEmpty)
				{
					return Business.QueryMessageFunction.LevelsOfMessage.DeclarationLevel;
				}
				else if (!headerMucr.IsEmpty && headerDucr.IsEmpty)
				{
					return Business.QueryMessageFunction.LevelsOfMessage.MasterLevel;
				}
				return null;
			}
		}

		public ZString[] DUCRs
		{
			get
			{
				switch (this.Class)
				{
					case MessageClasses.EAA:
					case MessageClasses.EAL:
						return GetOnlyLineLevelDucr();  // One DUCR per message

					case MessageClasses.ERS:
					case MessageClasses.EMR:
					case MessageClasses.DEC:
						return GetAllLineLevelDucrs();

					default:
						throw new NotSupportedException("Cannot understand message class " + this.Class);
				}
			}
		}

		public ZString[] MUCRs
		{
			get
			{
				switch (this.Class)
				{
					case MessageClasses.EAA:
					case MessageClasses.EAL:
						throw new NotSupportedException("Only EMR, DEC and ERS messages should have a MUCR");

					case MessageClasses.ERS:
					case MessageClasses.EMR:
						return new ZString[] { HeaderLevelMucr };

					case MessageClasses.DEC:
						return GetAllMucrsHeaderAndLine();

					default:
						throw new NotSupportedException("Cannot understand message class " + this.Class);
				}
			}
		}

		public ZString[] IntermediateMucrs
		{
			get
			{
				var mucrs = new List<ZString>();
				foreach (RFFSegment rff in ukCinv.RFF2)
				{
					if (rff.Reference.ReferenceFunctionCodeQualifier.ToString() == ChiefConstants.RffSegmentIdentifiers.UCN)
					{
						mucrs.Add(rff.Reference.ReferenceIdentifier);
					}
				}
				return mucrs.ToArray();
			}
		}

		ZString[] GetAllMucrsHeaderAndLine()
		{
			var mucrs = new List<ZString>();
			mucrs.Add(HeaderLevelMucr);
			mucrs.AddRange(GetLineLevelMucrs());
			return mucrs.ToArray();
		}

		ZString[] GetLineLevelMucrs()
		{
			var mucrs = new List<ZString>();

			foreach (UkCinvSegmentGroup1 group1 in ukCinv.Group1)
			{
				foreach (RFFSegment rff in group1.RFF)
				{
					if (rff.Reference.ReferenceFunctionCodeQualifier.ToString() == ChiefConstants.RffSegmentIdentifiers.UCN)
					{
						mucrs.Add(rff.Reference.ReferenceIdentifier);
					}
				}
			}
			return mucrs.ToArray();
		}

		public ZString HeaderLevelMucr
		{
			get
			{
				ZString value = ZString.Empty;
				ZString throwAway = ZString.Empty;
				AssignFromRff(ukCinv.RFF1, ChiefConstants.RffSegmentIdentifiers.UCN, ref value, ref throwAway, ref throwAway);
				return value.IsEmpty ? null : value;
			}
		}

		public ZString HeaderLevelDucr
		{
			get
			{
				ZString value = ZString.Empty;
				ZString throwAway = ZString.Empty;
				AssignFromRff(ukCinv.RFF1, ChiefConstants.RffSegmentIdentifiers.ABO, ref value, ref throwAway, ref throwAway);
				return value;
			}
		}

		ZString[] GetAllLineLevelDucrs()
		{
			var ducrs = new List<ZString>();
			foreach (UkCinvSegmentGroup1 group1 in ukCinv.Group1)
			{
				string oneDucr = GetDucrForSingleGroup1(group1).DucrAndPartWithoutChecksum;
				ducrs.Add(oneDucr);
			}
			return ducrs.ToArray();
		}

		ZString[] GetOnlyLineLevelDucr()
		{
			string firstAndOnlyDucr = GetDucrForSingleGroup1(ukCinv.Group1[0]).DucrAndPartWithoutChecksum;
			return new ZString[] { firstAndOnlyDucr };
		}

		protected DucrAndPartHelper GetDucrForSingleGroup1(UkCinvSegmentGroup1 group1)
		{
			foreach (RFFSegment rff in group1.RFF)
			{
				if (rff.Reference.ReferenceFunctionCodeQualifier.ToString() == ChiefConstants.RffSegmentIdentifiers.ABO)
				{
					var helper = new DucrAndPartHelper(rff.Reference.ReferenceIdentifier, rff.Reference.DocumentLineIdentifier);
					return helper;
				}
			}
			return null;
		}

		protected void AssignFromRff(SegmentMessageSection<RFFSegment> rffSegementGroup, ZString rffQualifier1153, ref ZString mainRef1154, ref ZString subRef1156, ref ZString subSubRef4000)
		{
			foreach (RFFSegment rff in rffSegementGroup)
			{
				if (rff.Reference.ReferenceFunctionCodeQualifier.ToString() == rffQualifier1153)
				{
					mainRef1154 = rff.Reference.ReferenceIdentifier;
					subRef1156 = rff.Reference.DocumentLineIdentifier;
					subSubRef4000 = rff.Reference.ReferenceVersionIdentifier;
					break;
				}
			}
		}

		public bool IsMasterOpen
		{
			get
			{
				if (overallInterpretation.IsEmpty)
				{
					GetInterpretation();
				}
				return isMasterOpen == "Y";
			}
		}

		ZString isMasterOpen;
		ZString overallInterpretation = "";
		public string GetInterpretation(string responseOrRequest = "Response")
		{
			//Header
			ZString airport = ZString.Empty;
			ZString shed = ZString.Empty;
			ZString epu = ZString.Empty;
			ZString crc = ZString.Empty;

			// Header references - UCRs
			overallInterpretation = string.Format("<h4>{0} {1}</h4>", Class, responseOrRequest);
			AssignAllHeaderRffs(ref overallInterpretation, ukCinv.RFF1);

			// Header location
			if (ukCinv.LOC.Count > 0)
			{
				GetLocations(ukCinv.LOC, ref airport, ref shed, ref epu, ref throwAway);
				if (!airport.IsEmpty)
				{
					overallInterpretation += string.Format("<p>Port-Shed-EPU: {0}-{1}-{2}</p>", airport, shed, epu);
				}
			}

			foreach (GEISegment gei in ukCinv.GEI)
			{
				if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.CRC_CustomsResponseCode)
				{
					crc = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
					if (!crc.IsEmpty)
					{
						overallInterpretation += string.Format("<p>CRC: {0} - {1}</p>", crc, new CRC().GetDescriptionFromCode(crc));
					}
				}
				else if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.OPN_OpenIndicator)
				{
					isMasterOpen = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
					if (!isMasterOpen.IsEmpty)
					{
						overallInterpretation += string.Format("<p>Master is open: {0}</p>", isMasterOpen);
					}
				}
			}

			// Header DTM
			AssignDateTime(ref overallInterpretation, ukCinv.DTM[0]);

			// Intermediate MUCRs (workers MUCRs on this MUCR)
			var j = 0;
			foreach (RFFSegment rff in ukCinv.RFF2)
			{
				j++;
				if (rff.Reference.ReferenceFunctionCodeQualifier.ToString() == ChiefConstants.RffSegmentIdentifiers.UCN)
				{
					overallInterpretation += string.Format("<h4>Subordinate MUCR - {0} - {1} of {2}</h4>", rff.Reference.ReferenceIdentifier, j, ukCinv.RFF2.Count);
				}
			}

			int i = 0;
			foreach (UkCinvSegmentGroup1 grp1 in ukCinv.Group1)
			{
				i++;
				overallInterpretation += string.Format("<h4>Repeating Details - {0} of {1}</h4>", i, ukCinv.Group1.Count);

				// Line references
				AssignAllHeaderRffs(ref overallInterpretation, grp1.RFF);

				// Line locations
				if (grp1.LOC.Count > 0)
				{
					airport = null;
					shed = null;
					epu = null;
					GetLocations(grp1.LOC, ref airport, ref shed, ref epu, ref throwAway);
					if (!airport.IsEmpty)  // LOC+14 can be empty
					{
						overallInterpretation += string.Format("<p>Port-Shed-EPU: {0}-{1}-{2}</p>", airport, shed, epu);
					}
				}

				// Line GEIs
				ZString soe = ZString.Empty;
				ZString roe = ZString.Empty;
				ZString ics = ZString.Empty;
				ZString typ = ZString.Empty;
				GetGEIs(ref ics, ref soe, ref roe, ref typ, ref throwAway, grp1.GEI);
				overallInterpretation += string.Format(@"<p>Route: {0} - {1}</p>
														<p>SoE/ICS: {2}  </p>",
															roe, (roe.IsEmpty ? "" : new EntryStatusList().GetDescriptionFromCode(StatusChecker.GetStatusCodeFromRouteOfEntryStatic(roe))),
														CusResEdifactParser.GetStyleAndIcsDescription(soe, ics));
				if (!typ.IsEmpty)
				{
					StagesOrTypesOfEntry stages = new StagesOrTypesOfEntry();
					overallInterpretation += string.Format("<p>Entry type: {0} ({1}) </p>", typ, stages.GetDescriptionFromCode(typ));
				}

				AssignDateTime(ref overallInterpretation, grp1.DTM[0]);

				// Agent role / location
				ZString agentRole = grp1.AUT[0].ValidationResultValue;
				ZString agentLocation = grp1.AUT[0].ValidationKeyIdentifier;
				if (!agentRole.IsEmpty || !agentLocation.IsEmpty)
				{
					overallInterpretation += string.Format(@"<p>Agent role & location: {0} {1}</p>", agentRole, agentLocation);
				}
			}

			return overallInterpretation;
		}

		protected ZDateTime AssignDateTime(ref ZString overallInterpretation, DTMSegment dtm)
		{
			var dt = ZDateTime.Empty;
			ZString goodsDtm = dtm.DateTimePeriod.DateOrTimeOrPeriodValue;
			if (!goodsDtm.IsEmpty)
			{
				var mask = dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode.ToString() == "102" ? "yyyyMMdd" : dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode.ToString() == "203" ? "yyyyMMddHHmm" : "";
				ZDateTime.TryParseExact(goodsDtm, out dt, mask);
				overallInterpretation += string.Format("<p>Date/time: {0} </p>", dt.ToString("dd/MM/yyyy HH:mm"));
			}
			return dt;
		}

		protected void AssignAllHeaderRffs(ref ZString overallInterpretation, SegmentMessageSection<RFFSegment> segmentMessageSection)
		{
			ZString ucr = null;
			ZString ucrPart = null;
			ZString nextUcr = null;
			ZString nextUcrPart = null;
			ZString masterUcr = null;
			ZString currentMasterUcr = null;
			ZString movementRef = null;
			ZString movementNum = null;

			AssignFromRff(segmentMessageSection, ChiefConstants.RffSegmentIdentifiers.ABO, ref ucr, ref ucrPart, ref throwAway);
			AssignFromRff(segmentMessageSection, ChiefConstants.RffSegmentIdentifiers.ACD, ref nextUcr, ref nextUcrPart, ref throwAway);
			AssignFromRff(segmentMessageSection, ChiefConstants.RffSegmentIdentifiers.UCN, ref masterUcr, ref throwAway, ref throwAway);
			AssignFromRff(segmentMessageSection, ChiefConstants.RffSegmentIdentifiers.FF, ref currentMasterUcr, ref throwAway, ref throwAway);
			AssignFromRff(segmentMessageSection, ChiefConstants.RffSegmentIdentifiers.AES, ref movementRef, ref throwAway, ref movementNum);

			DucrAndPartHelper ucrPartAndHelper = new DucrAndPartHelper(ucr, ucrPart);
			DucrAndPartHelper nextUcrPartAndHelper = new DucrAndPartHelper(nextUcr, nextUcrPart);
			if (!ucr.IsEmpty)
			{
				overallInterpretation += string.Format("<p>UCR (& part): {0} </p>", ucrPartAndHelper.DucrAndPartWithoutChecksum);
			}
			if (!nextUcr.IsEmpty)
			{
				overallInterpretation += string.Format("<p>Next UCR (& part): {0} </p>", nextUcrPartAndHelper.DucrAndPartWithoutChecksum);
			}
			if (!masterUcr.IsEmpty)
			{
				overallInterpretation += string.Format("<p>Master UCR: {0} </p>", masterUcr);
			}
			if (!currentMasterUcr.IsEmpty)
			{
				overallInterpretation += string.Format("<p>Current Master UCR: {0} </p>", currentMasterUcr);
			}
			if (!movementNum.IsEmpty || !movementRef.IsEmpty)
			{
				overallInterpretation += string.Format("<p>Movement reference & number: {0} <font color='green'>{1}</font> <small>Use the movement number when sending DEM messages.</small></p>", movementRef, movementNum);
			}
		}

		internal static void GetGEIs(ref ZString ics, ref ZString soe, ref ZString roe, ref ZString typ, ref ZString crc, SegmentMessageSection<GEISegment> geis)
		{
			foreach (GEISegment gei in geis)
			{
				if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.ICS_ImportCustomsStatus)
				{
					ics = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
				}
				else if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.ROE_RouteOfEntry)
				{
					roe = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
				}
				else if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.SOE_StyleOfEntry)
				{
					soe = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
				}
				else if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.TYP_TypeOfEntry)
				{
					typ = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
				}
				else if (gei.ProcessingInformationCodeQualifier == ChiefConstants.GeiSegmentTypes.CRC_CustomsResponseCode)
				{
					crc = gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString();
				}
			}
		}

		internal static void GetLocations(SegmentMessageSection<LOCSegment> locSegment, ref ZString airport, ref ZString shed, ref ZString epuNo, ref ZString epuId)
		{
			foreach (LOCSegment loc in locSegment)
			{
				if (loc.LocationFunctionCodeQualifier == "14")
				{
					airport = loc.LocationIdentification.LocationNameCode;
					shed = loc.LocationIdentification.LocationName;
					epuNo = loc.RelatedLocationOneIdentification.FirstRelatedLocationNameCode;
					epuId = loc.RelatedLocationOneIdentification.FirstRelatedLocationName;
					break;
				}
			}
		}

		protected ZString throwAway;
	}
}
