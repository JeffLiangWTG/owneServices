using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Customs.GB.Chief.Messaging;
using Enterprise.Edifact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Chief.CusDec
{
	public class CusDecCreatorUkcinv
	{
		public CusDecCreatorUkcinv(JobDeclaration declaration, CusEntryHeader cusEntryHeader, GbDes242MessageFunction mucrMessageFunction, ErrorCollector errorCollector)
		{
			this.gbDes242MessageFunction = mucrMessageFunction;
			iUkCinvWrapper = new GbChiefExportHeader(cusEntryHeader);
			hmrc109 = CodeListResponsibleAgencyCodeList.GetFromString("109");
			this.errorCollector = errorCollector;
		}

		protected CusDecCreatorUkcinv()
		{ }

		public string Create()
		{
			return Create(CusDecCreator.CharacterSetDefault);
		}

		public string Create(UNCharacterSet charSet)
		{
			CreateUkcinvShellNoUnt();
			MakeMasterOptGEI();

			if (gbDes242MessageFunction is GbDes242MessageFunction.MucrAssociate)
			{
				HeaderGroup1RffABO();
				HeaderGroup1RffUCN();
			}
			else if (gbDes242MessageFunction is GbDes242MessageFunction.MucrDisAssociate)
			{
				HeaderGroup1RffABO();
			}
			else if (gbDes242MessageFunction is GbDes242MessageFunction.MucrClose)
			{
				HeaderGroup1RffUCN();
			}
			else if (gbDes242MessageFunction is GbInventoryManagementMessageFunction)
			{
				MakeArrivalDepartureMessage();
				MakeGroup1RffAES();
			}
			FooterUNS1_D();
			FooterUNS2_S();
			FooterUNT();

			return ukCinvMessage.ToString(charSet);
		}

		protected virtual void MakeGroup1RffAES()
		{
		}

		void MakeMasterOptGEI()
		{
			if (!iUkCinvWrapper.MasterOpt.IsEmpty && !(gbDes242MessageFunction is GbInventoryManagementMessageFunction.Departure))
			{
				var gei = ukCinvMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
				gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("OPT");
				gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(iUkCinvWrapper.MasterOpt);  //MASTER-OPT
			}
		}

		void MakeArrivalDepartureMessage()
		{
			GbInventoryManagementMessageFunction howAsInventory = gbDes242MessageFunction as GbInventoryManagementMessageFunction;
			if (iUkCinvWrapper.LocationOfGoods.IsEmpty)
			{
				errorCollector.AddError("Location of goods", new ErrorInfo("30", "mandatory"));
			}

			if (howAsInventory.Level == GbInventoryManagementMessageFunction.MasterOrDeclaration.Master)
			{
				if (iUkCinvWrapper.MasterUniqueConsignmentReference.IsEmpty)
				{
					errorCollector.AddError("Master UCR", new ErrorInfo("44", "mandatory"));
				}
				HeaderGroup1RffABO(iUkCinvWrapper.MasterUniqueConsignmentReference, string.Empty);
			}
			if (howAsInventory.Level == GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)
			{
				HeaderGroup1RffABO(iUkCinvWrapper.DeclarationUniqueConsignmentReference, iUkCinvWrapper.DeclarationUniqueConsignmentReferencePartSuffix);
			}

			MakeLOCLocationOfGoods();
			if (gbDes242MessageFunction is GbInventoryManagementMessageFunction.ArrivalActual)
			{
				MakeDTMArrivalDateTime();
			}
			else if (gbDes242MessageFunction is GbInventoryManagementMessageFunction.Departure)
			{
				MakeDTMDepartureDateTime();
			}
			MakeTDTTransportModeAndId();
		}

		protected virtual void MakeTDTTransportModeAndId()
		{
			TDTSegment tdt = ukCinvMessage.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
			tdt.ModeOfTransport.TransportModeNameCode = iUkCinvWrapper.TransportModeAtTheBorderBox25;
			tdt.TransportIdentification.TransportMeansIdentificationName = iUkCinvWrapper.TransportIdentityAtTheBorderBox21;
			tdt.TransportIdentification.TransportMeansNationalityCode = iUkCinvWrapper.TransportNationalityAtTheBorderBox21;
		}

		protected virtual void MakeLOCLocationOfGoods()
		{
			string location = iUkCinvWrapper.LocationOfGoods.Right(3);
			LOCSegment loc = ukCinvMessage.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("14");
			loc.LocationIdentification.LocationNameCode = location;  // GBLHR becomes LHR
			loc.LocationIdentification.CodeListIdentificationCode = CodeListIdentificationCodeList.GetFromString("156");
			loc.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.GetFromString("109");
			loc.LocationIdentification.LocationName = iUkCinvWrapper.ShedCode;
		}

		void MakeDTMArrivalDateTime()
		{
			if (!iUkCinvWrapper.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises.IsEmpty)
			{
				DTMSegment dtm = ukCinvMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("178");
				dtm.DateTimePeriod.DateOrTimeOrPeriodValue = iUkCinvWrapper.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises.ToString("yyyyMMddHHmm");
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("203");
			}
		}

		void MakeDTMDepartureDateTime()
		{
			if (!iUkCinvWrapper.DateAndTimeTheGoodsWillBeLeavingLCPPremises.IsEmpty)
			{
				DTMSegment dtm = ukCinvMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("189");
				dtm.DateTimePeriod.DateOrTimeOrPeriodValue = iUkCinvWrapper.DateAndTimeTheGoodsWillBeLeavingLCPPremises.ToString("yyyyMMdd");
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("102");
			}
		}

		protected virtual RFFSegment HeaderGroup1RffUCN()
		{
			RFFSegment rffUcn = ukCinvMessage.RFF1.InstantiateAChildAndAddItToChildrenCollection();
			rffUcn.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.UCN);
			rffUcn.Reference.ReferenceIdentifier = iUkCinvWrapper.MasterUniqueConsignmentReference.GetCusDecValue(ChiefDataElementsLengths.Codes.MASTER_UCR);
			return rffUcn;
		}

		protected virtual void HeaderGroup1RffABO()
		{
			HeaderGroup1RffABO(iUkCinvWrapper.DeclarationUniqueConsignmentReference, iUkCinvWrapper.DeclarationUniqueConsignmentReferencePartSuffix);
		}

		protected void HeaderGroup1RffABO(ZString ducr, ZString ducrPart)
		{
			RFFSegment rffAbo = ukCinvMessage.RFF1.InstantiateAChildAndAddItToChildrenCollection();
			rffAbo.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.ABO);
			rffAbo.Reference.ReferenceIdentifier = ducr;

			if (!ducrPart.IsEmpty)
			{
				rffAbo.Reference.DocumentLineIdentifier = ducrPart; //DECLN-PART-NO
			}
		}

		void CreateUkcinvShellNoUnt()
		{
			this.ukCinvMessage = new UkCinvMessage();
			UNHSegment unh = HeaderGroup0UNH();
			unh.MessageIdentifier.AssociationAssignedCode = "109" + gbDes242MessageFunction.AsgCode;  // HMRC-ASG-CODE			
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;    // SYS-CAR
			HeaderGroup0BGM(gbDes242MessageFunction.FunctionCode);
		}

		UNHSegment HeaderGroup0UNH()
		{
			// UNH
			UNHSegment unh = ukCinvMessage.UNH[0];
			unh.MessageReferenceNumber = CusDecCreator.SYS_MRN;  //SYS-MRN
			unh.MessageIdentifier.MessageType = "UKCINV";
			unh.MessageIdentifier.MessageVersionNumber = "D";
			unh.MessageIdentifier.MessageReleaseNumber = "00A";
			unh.MessageIdentifier.ControllingAgency = "UN";
			return unh;
		}

		void HeaderGroup0BGM(string messageCode)
		{
			// BGM segment
			BGMSegment bgm = ukCinvMessage.BGM[0];
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(messageCode);  // e.g. EAC
			bgm.DocumentMessageName.CodeListResponsibleAgencyCode = hmrc109;
			bgm.DocumentMessageName.CodeListIdentificationCode = CodeListIdentificationCodeList.GetFromString("105");
		}

		void FooterUNS2_S()
		{
			ukCinvMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = "S";
		}

		void FooterUNS1_D()
		{
			ukCinvMessage.UNS1.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = "D";
		}

		void FooterUNT()
		{
			UNTSegment unt = ukCinvMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = ukCinvMessage.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = CusDecCreator.SYS_MRN;// SYS-MRN
		}

		protected UkCinvMessage ukCinvMessage;
		protected CodeListResponsibleAgencyCodeList hmrc109;
		protected IUkCinvWrapper iUkCinvWrapper;
		protected GbDes242MessageFunction gbDes242MessageFunction;
		protected ErrorCollector errorCollector;
	}
}
