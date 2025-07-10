using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D04A.Elements;
using Enterprise.Edifact.D04A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL
{
	public class GenralMessageGenerator
	{
		public const string GenralMessageCode = "GENRAL";
		public const string GenralMessageCodeShortForMessageType = "GEN";

		public string MakeMessageToParticipant(ZString payload, UNCharacterSet charSet, bool isPreformattedIntoLinesOf70, string commonAccessReference = "")
		{
			GenralMessage genral = new GenralMessage();
			UNHSegment unh = genral.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = GenralMessageCode;
			unh.MessageIdentifier.MessageVersionNumber = "0";
			unh.MessageIdentifier.MessageReleaseNumber = "912";
			unh.MessageIdentifier.ControllingAgency = "UN";
			if (String.IsNullOrEmpty(commonAccessReference))
			{
				commonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;
			}
			unh.CommonAccessReference = commonAccessReference;
			BGMSegment bgm = genral.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(GenralPurpose.Codes.Text);
			bgm.DocumentMessageName.CodeListIdentificationCode = "ZZZ";

			var group1 = genral.Group1.InstantiateAChildAndAddItToChildrenCollection();
			MSGSegment msg = group1.MSG.InstantiateAChildAndAddItToChildrenCollection();
			msg.OriginOfMessage = GenralSender.Codes.AnotherCcsUkParticipant;

			// Each FTX can hld 350 chars, can have up to 4 FTXs.  Total 1400 chars. 
			if (isPreformattedIntoLinesOf70)
			{
				FTXSegment ftx = null;
				var linesOf70 = Regex.Split(payload, System.Environment.NewLine);
				for (int i = 0; i < linesOf70.Length; i++)  // 20 lines max
				{
					if (i == 20)
					{
						break;
					}

					if (i % 5 == 0)
					{
						ftx = group1.FTX.InstantiateAChildAndAddItToChildrenCollection();
						ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("AAA");
					}
					var data = new ZString(linesOf70[i]).Left(70);
					if (i % 5 == 0)
					{
						ftx.TextLiteral.FreeText1 = data;
					}
					else if (i % 5 == 1)
					{
						ftx.TextLiteral.FreeText2 = data;
					}
					else if (i % 5 == 2)
					{
						ftx.TextLiteral.FreeText3 = data;
					}
					else if (i % 5 == 3)
					{
						ftx.TextLiteral.FreeText4 = data;
					}
					else if (i % 5 == 4)
					{
						ftx.TextLiteral.FreeText5 = data;
					}
				}
			}
			else
			{
				payload = payload.Replace(System.Environment.NewLine, " ").Replace("  ", " ");
				int countOfFtxSegments = 1;
				List<ZString> blocksOf350 = new List<ZString> { payload.Left(350), payload.SubstringSafe(350, 350), payload.SubstringSafe(700, 350), payload.SubstringSafe(1050, 350) };
				foreach (ZString blockOf350 in blocksOf350)
				{
					if (countOfFtxSegments == 5)
					{
						break;
					}

					if (!blockOf350.IsEmpty)
					{
						FTXSegment ftx = group1.FTX.InstantiateAChildAndAddItToChildrenCollection();
						ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("AAA");
						ftx.TextLiteral.FreeText1 = blockOf350.Left(70);
						ftx.TextLiteral.FreeText2 = blockOf350.SubstringSafe(70, 70);
						ftx.TextLiteral.FreeText3 = blockOf350.SubstringSafe(140, 70);
						ftx.TextLiteral.FreeText4 = blockOf350.SubstringSafe(210, 70);
						ftx.TextLiteral.FreeText5 = blockOf350.SubstringSafe(280, 70);
					}
					countOfFtxSegments++;
				}
			}

			UNTSegment unt = genral.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = genral.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;// SYS-MRN 

			return genral.ToString(charSet);
		}
	}
}
