using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CIN755NestedEnvelopeWrapper : ICINNested755Envelope
	{
		public const string OACICode = "PR2";
		public const string OACIDestination = "TAR";

		public CIN755NestedEnvelopeWrapper(CusEntryHeader header)
		{
			this.cusEntryHeader = Argument.NotNull(header, "CusEntryHeader cannot be null");
		}

		public ZString OACI => OACICode;

		public ZString REFERENCE => cusEntryHeader?.UniqueMessageNumber ?? ZString.Empty;

		public ZString MRN_ECS => cusEntryHeader?.MrnNumber ?? ZString.Empty;

		public ZString MAGASIN => cusEntryHeader?.Magasin ?? ZString.Empty;

		public ZString BUR_DOUANE => cusEntryHeader?.CustomOffice ?? ZString.Empty;

		public ZString DEST_OACI => OACIDestination;

		public ZString NUM_LTA => cusEntryHeader?.NumLTA ?? ZString.Empty;

		public ZString CIN => GetEdifactCINContent();

		ZString GetEdifactCINContent()
		{
			var segmentCount = 0;
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendFormat("UNB+UNOC:3+EZ1+DGV2+{0}+{1}", ZDateTime.Now.ToString("yyMMdd:HHmm"), cusEntryHeader.UniqueMessageNumber);
			segmentCount++;
			stringBuilder.AppendFormat("UNH+{0}+755:2", cusEntryHeader.UniqueMessageNumber);
			segmentCount++;
			stringBuilder.AppendFormat("BGM+35+{0}+01", cusEntryHeader.BGMReference);
			segmentCount++;
			stringBuilder.AppendFormat("DTM+137:{0}:204", cusEntryHeader.Date);
			segmentCount++;

			if (!cusEntryHeader.NumLTA.IsEmpty)
			{
				stringBuilder.AppendFormat("RFF+AWB:{0}", cusEntryHeader.NumLTA);
				segmentCount++;
				stringBuilder.AppendFormat("RFF+ACD:{0}", cusEntryHeader.NumLTA);
				segmentCount++;
			}

			if (!cusEntryHeader.OACI_Shipper.IsEmpty)
			{
				stringBuilder.AppendFormat("NAD+GY+{0}", cusEntryHeader.OACI_Shipper);
				segmentCount++;
			}

			if (!cusEntryHeader.OACI_Carrier.IsEmpty)
			{
				stringBuilder.AppendFormat("NAD+ST+{0}", cusEntryHeader.OACI_Carrier);
				segmentCount++;
			}

			stringBuilder.Append("LIN+1");
			segmentCount++;

			if (!cusEntryHeader.RefDos.IsEmpty)
			{
				stringBuilder.AppendFormat("RFF+REF:{0}", cusEntryHeader.RefDos);
				segmentCount++;
			}
			else if (!cusEntryHeader.Hwb.IsEmpty)
			{
				stringBuilder.AppendFormat("RFF+HWB:{0}", cusEntryHeader.Hwb);
				segmentCount++;
			}

			if (!cusEntryHeader.Magasin.IsEmpty)
			{
				stringBuilder.AppendFormat("LOC+{0}", cusEntryHeader.Magasin);
				segmentCount++;
			}

			if (!cusEntryHeader.PackagesCount.IsEmpty)
			{
				stringBuilder.AppendFormat("QTY+156:{0}:COL", cusEntryHeader.PackagesCount);
				segmentCount++;
			}

			if (!cusEntryHeader.GrossWeight.IsEmpty)
			{
				stringBuilder.AppendFormat("MEA+AAF+AAB+19:{0}", cusEntryHeader.GrossWeight);
				segmentCount++;
			}

			stringBuilder.Append("GIS+N:117:106");
			segmentCount++;

			if (!cusEntryHeader.MrnNumber.IsEmpty)
			{
				stringBuilder.AppendFormat("VIA+M:EX:{0}", cusEntryHeader.MrnNumber);
				segmentCount++;
			}

			if (!cusEntryHeader.CustomOffice.IsEmpty)
			{
				stringBuilder.AppendFormat("NAD+AM+++++{0}", cusEntryHeader.CustomOffice);
				segmentCount++;
			}

			if (!cusEntryHeader.BAEDate.IsEmpty)
			{
				stringBuilder.AppendFormat("DTM+254:{0}:102", cusEntryHeader.BAEDate);
				segmentCount++;
			}

			stringBuilder.AppendFormat("UNT+{0}+{1}", segmentCount.ToString(), cusEntryHeader.UniqueMessageNumber);
			stringBuilder.AppendFormat("UNZ+1+{0}", cusEntryHeader.UniqueMessageNumber);
			stringBuilder.Append("");
			return stringBuilder.ToStringWithDelimiterBetweenAppends("'");
		}

		readonly ICINMessage755 cusEntryHeader;
	}
}
