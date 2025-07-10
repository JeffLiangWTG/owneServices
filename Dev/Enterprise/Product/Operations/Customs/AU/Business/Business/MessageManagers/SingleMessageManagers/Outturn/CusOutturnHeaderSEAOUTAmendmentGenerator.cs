using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderSEAOUTAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusOutturnHeaderSEAOUTAmendmentGenerator(CusOutturnHeader header) : base(header)
		{
			this.Header = header;
		}

		#region Overridden Methods

		protected readonly CusOutturnHeader Header;
		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo)
		{
			if (Header != null)
			{
				ISeaOutturnReportHeaderInformation headerInfo = Header;
				return new SEAOUTMessageBuilder(headerInfo, headerInfo.Lines, false, false);
			}

			return null;
		}

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo)
		{
			return ((CusOutturnHeader)bizo).Messages;
		}

		protected override ZPropertyInfo[] UniqueIdentifierInfos
		{
			get { throw new NotSupportedException("We do not go though this message to detect a unique key change"); }
		}

		protected override bool UniqueIdentifierBeingChanged
		{
			get
			{
				string factoryHash = GetUniqueKeyHash(Header);
				string dBHash = GetUniqueKeyHash(new BusinessObjectFactory().Load<CusOutturnHeader>(Header.PK));
				return factoryHash != dBHash;
			}
		}

		string GetUniqueKeyHash(CusOutturnHeader header)
		{
			if (header != null)
			{
				ISeaOutturnReportHeaderInformation headerInfo = new DepotCusOutturnHeaderOutturnReportHeaderInformation(header);
				return headerInfo.VesselID + headerInfo.VoyageNumber + headerInfo.EstablishmentID;
			}
			return ZString.Empty;
		}

		#endregion
	}
}
