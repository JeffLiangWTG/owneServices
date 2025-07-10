using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;
using Enterprise.Customs.GB.CNS.ServiceTasks;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.CNS
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.CNS.XmlSerializers")]
	partial class CNScargoStatus : IUcnProvider, IPortAuthorityHoldApplicationProvider, IEntryNumberProvider
	{
		// IPortAuthorityHoldApplicationProvider
		AddOrRemove IPortAuthorityHoldApplicationProvider.DirectionOfApplication
		{
			get
			{
				return (AddOrRemove)Enum.Parse(typeof(AddOrRemove), this.MessageHeader.MessageType.ToString(), true);
			}
		}

		string IPortAuthorityHoldApplicationProvider.HoldAuthority
		{
			get { throw new NotImplementedException(); }
		}

		string IPortAuthorityHoldApplicationProvider.HoldType
		{
			get { return this.MessageDetail.Hold; }
		}

		ZDateTime IPortAuthorityHoldApplicationProvider.Date
		{
			get
			{
				return DateStampHeaderId;
			}
		}

		internal ZString HoldExplanation
		{
			get
			{
				if (!String.IsNullOrEmpty(MessageDetail.Hold))
				{
					return new CnsHoldTypes().GetDescriptionFromCode(MessageDetail.Hold);
				}
				return string.Empty;
			}
		}

		internal ZDateTime DateStampHeaderId
		{
			get
			{
				ZDateTime date;
				ZDateTime.TryParseExact(MessageHeader.HeaderID, out date, "yyMMddHHmmss");
				if (!date.IsValid)
				{
					date = ZDateTime.Now;
				}
				return date;
			}
		}

		public bool IsClear
		{
			get
			{
				return MessageHeader.MessageType == MessageType.CLEARED || MessageDetail.Clearance == "CL";
			}
		}

		#region IUcnProvider
		public ZString UcnNumberProperlyTruncated
		{
			get { return TruncateCnsUcn(this.MessageDetail.UCN); }
		}

		public static ZString TruncateCnsUcn(ZString fullUcnFromCns)
		{
			//For rationale behind this, see the file "Re HA020983 - Re CNS Messaging for Elite.msg" in the Documentation fodler of this project
			if (fullUcnFromCns.EndsWith("00") && fullUcnFromCns.Length == 14)
			{   // e.g. ELF1A054700100 really means 
				//		ELF1A0547001
				return fullUcnFromCns.Substring(0, fullUcnFromCns.Length - 2);
			}
			else
			{
				// ELF1A054700102 should go back verbatim
				return fullUcnFromCns;
			}
		}

		public ZString UcnNumberVerbatim
		{
			get { return this.MessageDetail.UCN; }
		}
		#endregion

		public override string ToString()
		{
			HtmlTableCreator table = new HtmlTableCreator(new string[] { "Field", "Value" });
			table.WriteRow("Advice type", this.MessageHeader.MessageType);
			table.WriteRow("Advice datetime (header ID)", DateStampHeaderId);
			table.WriteRow("UCN", UcnNumberProperlyTruncated);
			table.WriteRow("Container", this.MessageDetail.Container);
			table.WriteRow("BoL", this.MessageDetail.BillOfLading);
			table.WriteRow("Clearance code", this.MessageDetail.Clearance);
			table.WriteRow("Hold type", this.HoldExplanation);
			table.WriteRow("NoP", this.MessageDetail.Packages);
			table.WriteRow("Weight", this.MessageDetail.Weight);
			table.WriteRow("Site", this.MessageDetail.Site);
			if (!String.IsNullOrEmpty(MessageDetail.CHIEFEntryNo))
			{ table.WriteRow("Entry Number", this.EntryNumber); }
			if (!String.IsNullOrEmpty(MessageDetail.CHIEFEntryRoute))
			{ table.WriteRow("Entry Route", MessageDetail.CHIEFEntryRoute); }
			if (!String.IsNullOrEmpty(MessageDetail.CHIEFEntryDate))
			{
				ZDateTime zdate = ZDateTime.Empty;
				ZDateTime.TryParseExact(MessageDetail.CHIEFEntryDate, out zdate, "yyyyMMdd");
				table.WriteRow("Entry Date", zdate.ToShortDateString());
			}
			return table.ToHtml();
		}

		public ZString OriginalXml
		{
			get
			{
				return Parser.Write(this);
			}
		}

		public ZString EntryNumber
		{
			get { return MessageDetail.CHIEFepu + "-" + MessageDetail.CHIEFEntryNo; }
		}

		public ZDateTime EntryDate
		{
			get { return ParseChiefEntryDate(MessageDetail.CHIEFEntryDate); }
		}

		ZDateTime ParseChiefEntryDate(string chiefEntryDateString)
		{
			var chiefEntryDate = ZDateTime.Empty;
			ZDateTime.TryParseExact(chiefEntryDateString, out chiefEntryDate, "yyyyMMdd");
			if (chiefEntryDate == ZDateTime.Invalid)
			{
				ZDateTime.TryParseExact(chiefEntryDateString, out chiefEntryDate, "yyyy-MM-dd");
			}
			return chiefEntryDate;
		}

		public static class Parser
		{
			public static CNScargoStatus Read(string xmlText)
			{
				using (MemoryStream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(xmlText)))
				{
					return Read(stream);
				}
			}

			public static CNScargoStatus Read(Stream stream)
			{
				ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(CNScargoStatus));

				XmlSerializerNamespaces myNameSpc = new XmlSerializerNamespaces();
				myNameSpc.Add("", "");

				return (CNScargoStatus)serialiser.Deserialize(stream);
			}

			public static ZString Write(CNScargoStatus updater)
			{
				ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(CNScargoStatus));

				XmlSerializerNamespaces myNameSpc = new XmlSerializerNamespaces();
				myNameSpc.Add("", "");

				MemoryStream memStream;
				memStream = new MemoryStream();
				XmlTextWriter xmlWriter = new XmlTextWriter(memStream, Encoding.UTF8);
				xmlWriter.Namespaces = true;
				serialiser.Serialize(xmlWriter, updater, myNameSpc);
				xmlWriter.Close();
				memStream.Close();
				string xml = Encoding.UTF8.GetString(memStream.GetBuffer());
				xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
				xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
				return xml;
			}
		}
	}
}
