using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderSeaCargoReportHeader : ISeaCargoReportHeader
	{
		public CusSeaManOBLHeaderSeaCargoReportHeader(CusSeaManOBLHeader header)
		{
			this.header = header;
		}

		public ZString HouseBill
		{
			get { return ZString.Empty; }
		}

		public ZString ParentBill
		{
			get { return ZString.Empty; }
		}

		public ZString OceanBill
		{
			get { return header.BO_OceanBill; }
		}

		public ZString Voyage
		{
			get { return header.TransportHeader.BT_VoyageNum; }
		}

		public ZString LloydsNumber
		{
			get { return header.TransportHeader.BT_LloydsIMO; }
		}

		public ZString ResponsiblePartyID
		{
			get { return header.TransportHeader.BT_ResponsiblePartyID; }
		}

		public OrgHeader NotifyParty
		{
			get { return null; }
		}

		public ZString OriginCountry
		{
			get { return header.BO_RN_NKGoodsCountryOfOrigin; }
		}

		public bool IsConsolidation
		{
			get { return header.BO_FreightForwarderIndicator; }
		}

		public bool IsBureau
		{
			get { return false; }
		}

		public ZString PrincipalID
		{
			get { return header.TransportHeader.BT_PrincipalID; }
		}

		public BusinessObject BusinessObject
		{
			get { return header; }
		}

		public ZString MethodOfPayment
		{
			get { return header.BO_PaymentMethod; }
		}

		public ZString Origin
		{
			get { return header.BO_RL_NKOriginPort; }
		}

		public ZString Destination
		{
			get { return header.BO_RL_NKDestinationPort; }
		}

		public ZString Loading
		{
			get { return header.BO_RL_NKLoadPort; }
		}

		public ZString Discharge
		{
			get { return header.BO_RL_NKDischargePort; }
		}

		public ZString FirstArrivalPort
		{
			get
			{
				if (!Discharge.StartsWith(Core.Constants.CountryCodes.Australia) && !Destination.StartsWith(Core.Constants.CountryCodes.Australia))
				{
					return header.TransportHeader.FirstPortOfArrival;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public bool CanDelaySending
		{
			get { return false; }
		}

		public ZString ConsigneeGeneralAddress
		{
			get { return ConsigneeStreet + " " + ConsigneeStreet2 + " " + ConsigneeCity + " " + ConsigneePostCode + " " + ConsigneeCountry; }
		}

		public ZString ConsignorGeneralAddress
		{
			get { return ConsignorStreet + " " + ConsignorStreet2 + " " + ConsignorCity + " " + ConsignorPostCode + " " + ConsignorCountry; }
		}

		public ZString ConsigneeName
		{
			get { return header.BO_ConsigneeName; }
		}

		public ZString ConsigneeStreet
		{
			get { return header.BO_ConsigneeAddress1; }
		}

		public ZString ConsigneeStreet2
		{
			get { return header.BO_ConsigneeAddress2; }
		}

		public ZString ConsigneeCity
		{
			get { return header.BO_ConsigneeCity; }
		}

		public ZString ConsigneePostCode
		{
			get { return header.BO_ConsigneePostCode; }
		}

		public ZString ConsigneeCountry
		{
			get
			{
				ZString result = "";
				if (header.ConsigneeCountryCode != null)
				{
					result = header.ConsigneeCountryCode.Code;
				}
				return result;
			}
		}

		public ZString ConsignorName
		{
			get { return header.BO_ConsignorName; }
		}

		public ZString ConsignorStreet
		{
			get { return header.BO_ConsignorAddress1; }
		}

		public ZString ConsignorStreet2
		{
			get { return header.BO_ConsignorAddress2; }
		}

		public ZString ConsignorCity
		{
			get { return header.BO_ConsignorCity; }
		}

		public ZString ConsignorPostCode
		{
			get { return header.BO_ConsignorPostCode; }
		}

		public ZString ConsignorCountry
		{
			get
			{
				ZString result = "";
				if (header.ConsignorCountryCode != null)
				{
					result = header.ConsignorCountryCode.Code;
				}
				return result;
			}
		}

		public ZString ConsigneeIdentifier => ZString.Empty;

		public ZString ConsigneeABN => ZString.Empty;

		public ZString ConsigneeCAC => ZString.Empty;

		public ZString ConsigneeTIN => ZString.Empty;

		public ZString ConsignorIdentifier => ZString.Empty;

		public ZString ConsignorVendor => ZString.Empty;

		public ZString ConsignorTIN => ZString.Empty;

		public ZString[] Routings
		{
			get { return null; }
		}

		public ISeaCargoReportLine[] Lines
		{
			get { return GetLines(header); }
		}

		public ISeaCargoReportLine[] DatabaseLines
		{
			get { return GetLines(new BusinessObjectFactory().Load<CusSeaManOBLHeader>(header.PK)); }
		}

		ZString ICargoReportHeader.NotifyPartyName => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyStreet => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyStreet2 => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyCity => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyPostCode => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyCountry => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyGeneralAddress => ZString.Empty;

		ISeaCargoReportLine[] GetLines(CusSeaManOBLHeader header)
		{
			ArrayList result = new ArrayList();
			if (header != null)
			{
				foreach (CusSeaManOBLDetail detail in header.Details)
				{
					result.Add(new CusSeaManOBLDetailsSeaCargoReportLine(detail));
				}
			}
			return (ISeaCargoReportLine[])result.ToArray(typeof(ISeaCargoReportLine));
		}

		#region Implementation

		readonly CusSeaManOBLHeader header;

		#endregion
	}
}
