using System;
using System.Text.RegularExpressions;
using Enterprise.Edifact;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCTRL
{
	/*
	 
	 Thank you for looking at this class. 
	 Your interest is appreciated. 
	 
	 You may be surprised to see that we use regexes instead of Enterprise.Edifact.  This is because the UKCTRL is 
	 a custom spin on the standard UN CONTRL message.  However, not only does it redefine segment groups, it also redefines
	 segments (as well as inventing its own).  Specifically, it introduces the UCX segment (no problem), but makes a new definition of
	 another one.  I think from memory it's UCM. Its spin is to introduce element 0068 for SYS-CAR.  This doesn't exist in the UN's UCM.
	 Annoyingly our Edifact engine refuses to heed when I redefine UCM, stickly resolutely to its understanding of UCM, ie the UN version.
	 This is no good to us.  We need SYS-CAR. 
	 Therefore I have written this regular expression parser. 
	  
	 The regex looks like this, expanded to one line per segment for clarity.  If you think you can do better, please don't try.  It's too delicate.

		UNH\+(?<UNH_SYS_MRN>\w{0,14})\+UKCTRL\:1\:912\:UK\:109100'
		UCM\+(?<UCM_MSG_MRN>\w{0,14})\:(?<UCM_MSG_SYS_CAR>\w{0,35})\+(?<UCM_MSG_TYPE>\w{0,6})\:(?<UCM_MSG_VSN>\w{0,3})\:(?<UCM_MSG_REL_NO>\w{0,3})\:(?<UCM_MSG_CNTR_AGNCY>\w{0,2})\:(?<UCM_MSG_ASG_CODE>\w{0,6})'
		UCX\+(?<UCX_ACTION_CODE>\w{0,3})\+?(?<UCX_MSG_ERROR_CODE>\w{0,3})'
		(?<GROUPTWO>UCR\+(?<UCR_SEG_NO>\d{0,5})\+?(?<UCR_SEG_ERROR_CODE>\w{0,3})'
		UCD\+(?<UCD_ELEMENT_NO>\d{0,3})\:?(?<UCD_COMPONENT_NO>\d{0,3})\+(?<UCD_DATA_ERROR_CODE>\w{0,3})'
		(FTX\+AAO\+\+\+(?<FTX_DATA_ERROR_TEXT1>[^:+']{0,70})(\:(?<FTX_DATA_ERROR_TEXT2>[^:+']{0,70}))?(\:(?<FTX_DATA_ERROR_TEXT3>[^:+']{0,70}))?(\:(?<FTX_DATA_ERROR_TEXT4>[^:+']{0,70}))?(\:(?<FTX_DATA_ERROR_TEXT5>[^:+']{0,70}))?')?)

	 I know you want to fiddle with it.
	 I know you want to use the proper Enterprise.Edifact version.
	 But please don't do either.
	
	 Thank you for you consideration. 
	  
	 DJC
	 Jan 2010. 
	 
	 */

	public class UkctrlRegexParser
	{
		/// <summary>
		/// Accepts a standard or MCP UKCTRL. Converts MCP to standard and does all its work in standard.  Then replaces special (escaped) 
		/// chars from the elements themselves, runs the regex-based parser, then flips the special chars back in *for the text fields only*.  
		/// It does not expect special chars anywhere except the in FTX.Text.TextLiteral1 etc components. All other elements/components will 
		/// only hold predefined codes (such as "G4", "UK" or "CUSDEC"), or ints, which will not contain special chars. 
		/// It allows any number of group twos per UCM but it only expects one UCM per UNH. It expects no UCI, as per UKCTRL definition 
		/// ("UKCTRL CHIEF Error Response Message", Version 9.0 March 2005). But this is OK as we only ever upload on message per interchange.
		/// Another limitation is that we use single-char placeholders during our swapping. Message data must therefore not contain these values. 
		/// We can receive data from chief in 4 ways (MCP, CNS, NES/EDCS and Gems edifact), but only the latter may not use A-characters. 
		/// Gems edifact may be set to use B-characters for delimiters. Need to check this.  Workaround - use DITXTEDIA, not DITXTEDIB trigger.
		/// Finally, it alows 15 (not 14) chars in the UNH message reference number (SYS-MRN), since MCP send us this. 
		/// </summary>
		public UkctrlMessage Parse(string edifactInput)
		{
			string workingText = CheckDelimiters(edifactInput);
			UkctrlHeader header = GetHeader(workingText);
			GroupTwoCollection collection = GetGroupTwos(workingText);
			UkctrlMessage ukctrlMessage = new UkctrlMessage(header, collection);
			return ukctrlMessage;
		}

		string CheckDelimiters(string edifactInput)
		{
			if (edifactInput.Contains("UNH+") && edifactInput.EndsWith("'"))  // standard
			{
				return TemporarilySwapOutEscapedStandardChars(edifactInput);
			}
			else if (edifactInput.Contains("UNH#") && edifactInput.EndsWith("{")) // MCP
			{
				string asMcp = new EdifactDelimiterConverter(EdifactDelimiterConverter.McpDelimiters).Convert(edifactInput);
				return CheckDelimiters(asMcp);
			}

			throw new NotSupportedException("Only standard and MCP delimiters are supported.");
		}

		//Public 'cos the GroupTwo class uses it during getters. 
		public static string SwapBackPlaceholdersForEscapedStandardChars(string edifactInput)
		{
			string working = edifactInput.Replace(PlaceholderComponent.ToString(), ":");
			working = working.Replace(PlaceholderElement.ToString(), "+");
			working = working.Replace(PlaceholderSegment.ToString(), "'");
			return working;
		}

		static string TemporarilySwapOutEscapedStandardChars(string edifactInput)
		{
			// This is not very pretty. But it makes the regex much much more simple if we do not have to worry about special chars within the body of elements themselves.
			string working = edifactInput.Replace("?:", PlaceholderComponent.ToString());
			working = working.Replace("?+", PlaceholderElement.ToString());
			working = working.Replace("?'", PlaceholderSegment.ToString());
			return working;
		}

		GroupTwoCollection GetGroupTwos(string workingText)
		{
			string uCR_SegmentNo = @"(?<UCR_SEG_NO>\d{0,5})";
			string uCR_SegmentErrorCode = @"(?<UCR_SEG_ERROR_CODE>\w{0,3})";
			string ucrPattern = string.Format(@"UCR\+{0}\+?{1}'", uCR_SegmentNo, uCR_SegmentErrorCode);

			string uCD_ELEMENT_NO = @"(?<UCD_ELEMENT_NO>\d{0,3})";
			string uCD_COMPONENT_NO = @"(?<UCD_COMPONENT_NO>\d{0,3})";
			string uCD_DATA_ERROR_CODE = @"(?<UCD_DATA_ERROR_CODE>\w{0,3})";
			string ucdPattern = string.Format(@"UCD\+{0}\:?{1}\+{2}'", uCD_ELEMENT_NO, uCD_COMPONENT_NO, uCD_DATA_ERROR_CODE);

			string fTX_DATA_ERROR_TEXT1 = @"(?<FTX_DATA_ERROR_TEXT1>[^:+']{0,70})";
			string fTX_DATA_ERROR_TEXT2 = @"\:(?<FTX_DATA_ERROR_TEXT2>[^:+']{0,70})";  // note the delimiters
			string fTX_DATA_ERROR_TEXT3 = @"\:(?<FTX_DATA_ERROR_TEXT3>[^:+']{0,70})";
			string fTX_DATA_ERROR_TEXT4 = @"\:(?<FTX_DATA_ERROR_TEXT4>[^:+']{0,70})";
			string fTX_DATA_ERROR_TEXT5 = @"\:(?<FTX_DATA_ERROR_TEXT5>[^:+']{0,70})";
			string ftxPattern = string.Format(@"FTX\+...\+\+\+{0}({1})?({2})?({3})?({4})?'", fTX_DATA_ERROR_TEXT1, fTX_DATA_ERROR_TEXT2, fTX_DATA_ERROR_TEXT3, fTX_DATA_ERROR_TEXT4, fTX_DATA_ERROR_TEXT5);

			string oneGroupTwoBlock = string.Format("(?<GROUPTWO>{0}{1}({2})?)", ucrPattern, ucdPattern, ftxPattern);  // strictly, FTX is optional			
			Regex regex = new Regex(oneGroupTwoBlock);
			MatchCollection matches = regex.Matches(workingText);

			GroupTwoCollection coll = new GroupTwoCollection();
			foreach (Match match in matches)
			{
				GroupTwo g2 = new GroupTwo();
				g2.UCR_SEG_NO = match.Groups[nameof(ElementsOfInterest.UCR_SEG_NO)].Value;
				g2.UCR_SEG_ERROR_CODE = match.Groups[nameof(ElementsOfInterest.UCR_SEG_ERROR_CODE)].Value;
				g2.UCD_DATA_ERROR_CODE = match.Groups[nameof(ElementsOfInterest.UCD_DATA_ERROR_CODE)].Value;
				g2.UCD_ELEMENT_NO = match.Groups[nameof(ElementsOfInterest.UCD_ELEMENT_NO)].Value;
				g2.UCD_COMPONENT_NO = match.Groups[nameof(ElementsOfInterest.UCD_COMPONENT_NO)].Value;
				g2.FTX_DATA_ERROR_TEXT1 = match.Groups[nameof(ElementsOfInterest.FTX_DATA_ERROR_TEXT1)].Value;
				g2.FTX_DATA_ERROR_TEXT2 = match.Groups[nameof(ElementsOfInterest.FTX_DATA_ERROR_TEXT2)].Value;
				g2.FTX_DATA_ERROR_TEXT3 = match.Groups[nameof(ElementsOfInterest.FTX_DATA_ERROR_TEXT3)].Value;
				g2.FTX_DATA_ERROR_TEXT4 = match.Groups[nameof(ElementsOfInterest.FTX_DATA_ERROR_TEXT4)].Value;
				g2.FTX_DATA_ERROR_TEXT5 = match.Groups[nameof(ElementsOfInterest.FTX_DATA_ERROR_TEXT5)].Value;
				coll.Add(g2);
			}
			return coll;
		}

		static UkctrlHeader GetHeader(string workingText)
		{
			string sYS_MRN = @"(?<UNH_SYS_MRN>\w{0,15})"; //SYS-MRN an..14 // UK spec say 14, MCP examples have 15.  Tsk tsk. 
			string messageIdentifier = @"UKCTRL";
			string messageTypeVersionNumber = "1";
			string messageTypeReleaseNumber = "912";  // we only understand 912:1:UK
			string controllingAgency = "UK";
			string hmrcAsgCode = "109100";
			string unhPattern = string.Format(@"UNH\+{0}\+{1}\:{2}\:{3}\:{4}\:{5}'", sYS_MRN, messageIdentifier, messageTypeVersionNumber, messageTypeReleaseNumber, controllingAgency, hmrcAsgCode);

			string mSG_MRN = @"(?<UCM_MSG_MRN>\w{0,14})";
			string mSG_SYS_CAR = @"(?<UCM_MSG_SYS_CAR>\w{0,35})";
			string mSG_TYPE = @"(?<UCM_MSG_TYPE>\w{0,6})";
			string mSG_VSN = @"(?<UCM_MSG_VSN>\w{0,3})";
			string mSG_REL_NO = @"(?<UCM_MSG_REL_NO>\w{0,3})";
			string mSG_CNTR_AGNCY = @"(?<UCM_MSG_CNTR_AGNCY>\w{0,2})";
			string mSG_ASG_CODE = @"(?<UCM_MSG_ASG_CODE>\w{0,6})";
			string ucmPattern = string.Format(@"UCM\+{0}\:{1}\+{2}\:{3}\:{4}\:{5}\:{6}'", mSG_MRN, mSG_SYS_CAR, mSG_TYPE, mSG_VSN, mSG_REL_NO, mSG_CNTR_AGNCY, mSG_ASG_CODE);

			string uCX_ACTION_CODE = @"(?<UCX_ACTION_CODE>\w{0,3})";
			string uCX_MSG_ERROR_CODE = @"(?<UCX_MSG_ERROR_CODE>\w{0,3})";
			string ucxPattern = string.Format(@"UCX\+{0}\+?{1}'", uCX_ACTION_CODE, uCX_MSG_ERROR_CODE);  // note second element is optional

			string headerPattern = unhPattern + ucmPattern + ucxPattern;

			Regex regex = new Regex(headerPattern);
			Match match = regex.Match(workingText);
			UkctrlHeader ukctrlHeader = new UkctrlHeader();

			ukctrlHeader.UNH_SYS_MRN = match.Groups[nameof(ElementsOfInterest.UNH_SYS_MRN)].Value;
			ukctrlHeader.UCM_MSG_ASG_CODE = match.Groups[nameof(ElementsOfInterest.UCM_MSG_ASG_CODE)].Value;
			ukctrlHeader.UCM_MSG_CNTR_AGNCY = match.Groups[nameof(ElementsOfInterest.UCM_MSG_CNTR_AGNCY)].Value;
			ukctrlHeader.UCM_MSG_MRN = match.Groups[nameof(ElementsOfInterest.UCM_MSG_MRN)].Value;
			ukctrlHeader.UCM_MSG_REL_NO = match.Groups[nameof(ElementsOfInterest.UCM_MSG_REL_NO)].Value;
			ukctrlHeader.UCM_MSG_SYS_CAR = match.Groups[nameof(ElementsOfInterest.UCM_MSG_SYS_CAR)].Value;
			ukctrlHeader.UCM_MSG_TYPE = match.Groups[nameof(ElementsOfInterest.UCM_MSG_TYPE)].Value;
			ukctrlHeader.UCM_MSG_VSN = match.Groups[nameof(ElementsOfInterest.UCM_MSG_VSN)].Value;
			ukctrlHeader.UCX_ACTION_CODE = match.Groups[nameof(ElementsOfInterest.UCX_ACTION_CODE)].Value;
			ukctrlHeader.UCX_MSG_ERROR_CODE = match.Groups[nameof(ElementsOfInterest.UCX_MSG_ERROR_CODE)].Value;

			return ukctrlHeader;
		}

		const char PlaceholderComponent = '\f';
		const char PlaceholderSegment = '\a';
		const char PlaceholderElement = '\b';

		[CodeAlive("This enum is used to index the named groups in the regex.  It is used by the GroupTwoCollection class to get the values from the matches.")]
		enum ElementsOfInterest
		{
			UNH_SYS_MRN,
			UCM_MSG_MRN,
			UCM_MSG_SYS_CAR,
			UCM_MSG_TYPE, // of uploaded data that was rejected 
			UCM_MSG_VSN,
			UCM_MSG_REL_NO,
			UCM_MSG_CNTR_AGNCY,
			UCM_MSG_ASG_CODE,
			UCX_ACTION_CODE,
			UCX_MSG_ERROR_CODE,
			UCR_SEG_NO,
			UCR_SEG_ERROR_CODE,
			UCD_ELEMENT_NO,
			UCD_COMPONENT_NO,
			UCD_DATA_ERROR_CODE,
			FTX_DATA_ERROR_TEXT1,
			FTX_DATA_ERROR_TEXT2,
			FTX_DATA_ERROR_TEXT3,
			FTX_DATA_ERROR_TEXT4,
			FTX_DATA_ERROR_TEXT5,
		}
	}
}
