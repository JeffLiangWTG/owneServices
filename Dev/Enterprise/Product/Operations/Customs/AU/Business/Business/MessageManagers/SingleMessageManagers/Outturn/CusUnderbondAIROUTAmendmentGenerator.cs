using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondAIROUTAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusUnderbondAIROUTAmendmentGenerator(CusUnderbond underbond) : base(underbond)
		{
			this.Underbond = underbond;
		}

		#region Overridden Methods

		protected readonly CusUnderbond Underbond;
		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo)
		{
			CusUnderbond underbond = bizo as CusUnderbond;
			IAirOutturnReportHeaderInformationProvider parent = underbond.LinkedObject as IAirOutturnReportHeaderInformationProvider;
			IAirOutturnReportHeaderInformation headerInfo = null;
			if (parent == null)
			{
				headerInfo = new AirOutturnStandAloneReportHeader(underbond);
			}
			else
			{
				headerInfo = parent.GetHeader(underbond);
			}

			if (headerInfo != null)
			{
				return new AIROUTMessageBuilder(headerInfo, headerInfo.Lines, false, false);
			}

			return null;
		}

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo)
		{
			return (bizo as CusUnderbond).Messages;
		}

		protected override ZPropertyInfo[] UniqueIdentifierInfos
		{
			get { throw new NotSupportedException("We do not go though this message to detect a unique key change"); }
		}

		protected override bool UniqueIdentifierBeingChanged
		{
			get
			{
				string factoryHash = GetUniqueKeyHash(Underbond);
				string dBHash = GetUniqueKeyHash(new BusinessObjectFactory().Load<CusUnderbond>(Underbond.PK));
				return factoryHash != dBHash;
			}
		}

		string GetUniqueKeyHash(CusUnderbond underbond)
		{
			if (underbond != null)
			{
				IAirOutturnReportHeaderInformationProvider parent = underbond.LinkedObject as IAirOutturnReportHeaderInformationProvider;
				if (parent != null)
				{
					IAirOutturnReportHeaderInformation headerInfo = parent.GetHeader(underbond);
					if (headerInfo != null)
					{
						return headerInfo.FlightNumber + headerInfo.EstimatedDateOfArrival.ToString("yyyymmdd") + headerInfo.EstablishmentID + headerInfo.DateTimeOfOutturn.ToString("yyyyMMddHHmm");
					}
				}
				return ZString.Empty;
			}
			return ZString.Empty;
		}

		#endregion
	}
}
