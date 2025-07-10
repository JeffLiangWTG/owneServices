using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaOutturnStandAloneReportHeader : ISeaOutturnReportHeaderInformation
	{
		public SeaOutturnStandAloneReportHeader(CusOutturnHeader header)
		{
			this.header = header;
		}

		readonly CusOutturnHeader header;

		#region ISeaOutturnReportHeaderInformation Members

		public ZString ResponsiblePartyID
		{
			get { return header.C6_ResponsiblePartyID; }
		}

		public ZString EstablishmentID
		{
			get { return header.C6_OutturningPremiseID; }
		}

		public ZString VoyageNumber
		{
			get { return header.C6_VoyageNum; }
		}

		public ZString VesselID
		{
			get { return header.C6_LloydsIMO; }
		}

		public IEnumerable<ISeaOutturnReportLineInformation> Lines
		{
			get { return GetLines(header); }
		}

		public IEnumerable<ISeaOutturnReportLineInformation> MessageLines
		{
			get { return Lines; }
		}

		public IEDIMessageCollectionProvider MessagesProvider
		{
			get { return header; }
		}

		IEnumerable<ISeaOutturnReportLineInformation> GetLines(CusOutturnHeader outturnHeader)
		{
			ArrayList result = new ArrayList();
			if (outturnHeader != null)
			{
				foreach (DepotCusOutturn outturn in outturnHeader.Outturns)
				{
					if (!outturn.C5_CargoReceiptDate.IsEmpty)
					{
						result.Add(new DepotCusOutturnOutturnReportLineInformation(outturn));
					}
				}
			}

			return (IEnumerable<ISeaOutturnReportLineInformation>)result.ToArray(typeof(ISeaOutturnReportLineInformation));
		}

		#endregion
	}
}
