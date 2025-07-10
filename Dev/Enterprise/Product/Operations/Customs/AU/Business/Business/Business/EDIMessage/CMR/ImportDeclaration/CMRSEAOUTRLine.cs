using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEAOUTRLine
	{
		public CMRSEAOUTRLine(SegmentGroup4 group4)
		{
			Argument.NotNull(group4, "SegmentGroup4");
			this.group4 = group4;
		}
		readonly SegmentGroup4 group4;

		//		(CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054) NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H1
		//    CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054 REPORT NOT FOUND FOR CHANGE/DELETE SCO LINE CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H1
		public ZString FTXText
		{
			get
			{
				if (fTXText.IsEmpty)
				{
					fTXText = group4.FTX.Count > 0 ? group4.FTX[0].TextLiteral.FreeTextValue1 : string.Empty;
					if (fTXText.StartsWith("("))
					{
						int i = fTXText.IndexOf(")");
						if (i >= 0)
						{
							fTXText = fTXText.SubstringSafe(i + 1).Trim();
						}
					}
					else if (fTXText.StartsWith("CT="))
					{
						int i = fTXText.IndexOf(" ");
						if (i >= 0)
						{
							fTXText = fTXText.SubstringSafe(i + 1).Trim();
						}
					}
				}
				return fTXText;
			}
		}
		ZString fTXText;

		ZString paramValue(string param)
		{
			ZString result = ZString.Empty;
			int i = FTXText.IndexOf(param);
			if (i >= 0)
			{
				result = FTXText.SubstringSafe(i + param.Length);
				i = result.IndexOf(",");
				if (i >= 0)
				{
					result = result.SubstringSafe(0, i);
				}
			}
			return result;
		}

		public ZString ContainerNumber
		{
			get
			{
				if (containerNumber.IsEmpty)
				{
					containerNumber = paramValue("CNT=");
				}

				return containerNumber;
			}
		}
		ZString containerNumber;

		public ZString MasterBillNumber
		{
			get
			{
				if (masterBillNumber.IsEmpty)
				{
					masterBillNumber = paramValue("OBL=");
				}

				return masterBillNumber;
			}
		}
		ZString masterBillNumber;

		public ZString HouseBillNumber
		{
			get
			{
				if (houseBillNumber.IsEmpty)
				{
					houseBillNumber = paramValue("HBL=");
				}

				return houseBillNumber;
			}
		}
		ZString houseBillNumber;

		public ZString ERC
		{
			get
			{
				if (eRC.IsEmpty)
				{
					foreach (ERCSegment ercSeg in group4.ERC)
					{
						if (ercSeg.ApplicationErrorDetail.CodeListIdentificationCode == CodeListIdentificationCodeList.ApplicationErrorCode)
						{
							eRC = ercSeg.ApplicationErrorDetail.ApplicationErrorIdentification;
							break;
						}
					}
				}
				return eRC;
			}
		}
		ZString eRC;

		public override string ToString()
		{
			return (ContainerNumber + "/" + MasterBillNumber + "/" + HouseBillNumber).ToUpper();
		}
	}
}
