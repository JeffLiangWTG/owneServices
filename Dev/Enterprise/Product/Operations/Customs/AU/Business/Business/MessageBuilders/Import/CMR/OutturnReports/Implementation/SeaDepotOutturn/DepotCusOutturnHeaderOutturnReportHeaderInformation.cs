using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusOutturnHeaderOutturnReportHeaderInformation : ISeaOutturnReportHeaderInformation
	{
		public DepotCusOutturnHeaderOutturnReportHeaderInformation(CusOutturnHeader header)
		{
			Header = header;
		}

		internal readonly CusOutturnHeader Header;

		#region ISeaOutturnReportHeaderInformation Members

		public ZString ResponsiblePartyID
		{
			get { return Header.C6_ResponsiblePartyID; }
		}

		public ZString EstablishmentID
		{
			get { return Header.C6_OutturningPremiseID; }
		}

		public ZString VoyageNumber
		{
			get { return Header.C6_VoyageNum; }
		}

		public ZString VesselID
		{
			get { return Header.C6_LloydsIMO; }
		}

		public IEnumerable<ISeaOutturnReportLineInformation> Lines
		{
			get { return GetLines(Header); }
		}

		public IEnumerable<ISeaOutturnReportLineInformation> MessageLines
		{
			get { return Lines; }
		}

		public IEDIMessageCollectionProvider MessagesProvider
		{
			get { return Header; }
		}

		IEnumerable<ISeaOutturnReportLineInformation> GetLines(CusOutturnHeader header)
		{
			ArrayList result = new ArrayList();
			if (header != null)
			{
				foreach (DepotCusOutturn outturn in header.Outturns)
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
