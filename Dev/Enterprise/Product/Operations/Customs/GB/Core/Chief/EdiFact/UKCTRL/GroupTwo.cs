using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCTRL
{
	public class GroupTwo
	{
		// See notes in class UkctrlRegexParser for explanation of why these getters behave as they do. 

		public List<ZString> AllFreeText
		{
			get
			{
				List<ZString> list = new List<ZString>();
				if (!ftx1.IsEmpty)
				{
					list.Add(FTX_DATA_ERROR_TEXT1);
				}
				if (!ftx2.IsEmpty)
				{
					list.Add(FTX_DATA_ERROR_TEXT2);
				}
				if (!ftx3.IsEmpty)
				{
					list.Add(FTX_DATA_ERROR_TEXT3);
				}
				if (!ftx4.IsEmpty)
				{
					list.Add(FTX_DATA_ERROR_TEXT4);
				}
				if (!ftx5.IsEmpty)
				{
					list.Add(FTX_DATA_ERROR_TEXT5);
				}
				return list;
			}
		}

		public string UCR_SEG_NO { get; set; }
		public string UCR_SEG_ERROR_CODE { get; set; }
		public string UCD_ELEMENT_NO { get; set; }
		public string UCD_COMPONENT_NO { get; set; }
		public string UCD_DATA_ERROR_CODE { get; set; }

		ZString ftx1;
		public ZString FTX_DATA_ERROR_TEXT1
		{
			set { ftx1 = value; }
			get { return UkctrlRegexParser.SwapBackPlaceholdersForEscapedStandardChars(ftx1); }
		}

		ZString ftx2;
		public ZString FTX_DATA_ERROR_TEXT2
		{
			set { ftx2 = value; }
			get { return UkctrlRegexParser.SwapBackPlaceholdersForEscapedStandardChars(ftx2); }
		}

		ZString ftx3;
		public ZString FTX_DATA_ERROR_TEXT3
		{
			set { ftx3 = value; }
			get { return UkctrlRegexParser.SwapBackPlaceholdersForEscapedStandardChars(ftx3); }
		}

		ZString ftx4;
		public ZString FTX_DATA_ERROR_TEXT4
		{
			set { ftx4 = value; }
			get { return UkctrlRegexParser.SwapBackPlaceholdersForEscapedStandardChars(ftx4); }
		}

		ZString ftx5;
		public ZString FTX_DATA_ERROR_TEXT5
		{
			set { ftx5 = value; }
			get { return UkctrlRegexParser.SwapBackPlaceholdersForEscapedStandardChars(ftx5); }
		}
	}
}
