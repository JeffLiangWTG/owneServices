using System.Globalization;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;

namespace Enterprise.Client.UPE.Business.GSSi
{
	public class GSSiMessage
	{
		public GSSiMessage(ZString trackingNumber, ZString buildingID, IShipmentStatusData currentQueue) : this(trackingNumber, buildingID, currentQueue, currentQueue.HoldReasonCode)
		{
		}

		public GSSiMessage(ZString trackingNumber, ZString buildingID, IShipmentStatusData currentQueue, ZString holdReasonCode)
		{
			header = new GSSiMsgHeader();
			body = new GSSiMsgBody();

			if (!buildingID.IsEmpty)
			{
				header.buildingID = buildingID;
			}

			ZString currentUTCTime = ZDateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
			body.PKG_TCK_NR = trackingNumber;
			body.PKG_INL_DEL_STS_CD = currentQueue.ShipmentStatus;
			body.PKG_XCP_RSN_CD = holdReasonCode;
			body.AD_COR_IR = currentQueue.AddressCorrectionIndicator ? "Y" : "N";
			body.EVT_TS = currentUTCTime;
			body.CTM_NSP_REQ_IR = currentQueue.InspectIndicator ? "Y" : "N";
			body.OUB_SRT_DT = currentUTCTime.Left(8);
		}

		public override string ToString()
		{
			return headerString + bodyString;
		}

		#region Implmentation

		#region MessageParts

		class GSSiMsgHeader
		{
			public readonly ZString formatVersion = "01";
			public readonly ZString replyNeeded = "ERR";
			public ZString buildingID = "       ";
			public readonly ZString commSpecId = "7340      ";
			public readonly ZString isXML = "N";
		}

		class GSSiMsgBody
		{
			public readonly ZString DXO_REC_NR = "7340";
			ZString fPKG_TCK_NR = "".PadLeft(35, ' ');

			public ZString PKG_TCK_NR
			{
				get
				{
					return fPKG_TCK_NR;
				}
				set
				{
					fPKG_TCK_NR = value.PadRight(35, ' ');
				}
			}

			ZString fPKG_INL_DEL_STS_CD = "  ";
			public ZString PKG_INL_DEL_STS_CD
			{
				get
				{
					return fPKG_INL_DEL_STS_CD;
				}
				set
				{
					fPKG_INL_DEL_STS_CD = value.PadLeft(2, '0');
				}
			}

			ZString fPKG_XCP_RSN_CD = "  ";
			public ZString PKG_XCP_RSN_CD
			{
				get
				{
					return fPKG_XCP_RSN_CD;
				}
				set
				{
					fPKG_XCP_RSN_CD = value.PadLeft(2, ' ');
				}
			}
			public readonly ZString PKG_COD_IR = " ";

			ZString fAD_COR_IR = " ";
			public ZString AD_COR_IR
			{
				get
				{
					return fAD_COR_IR;
				}
				set
				{
					fAD_COR_IR = value;
				}
			}
			public readonly ZString CMY_CTF_ORG_MNU_CD = "  ";
			public ZString EVT_TS = " ";
			public readonly ZString PKG_CSF_TYP_CD = "I";

			ZString fCTM_NSP_REQ_IR = " ";

			public ZString CTM_NSP_REQ_IR
			{
				get
				{
					return fCTM_NSP_REQ_IR;
				}
				set
				{
					fCTM_NSP_REQ_IR = value;
				}
			}
			public ZString OUB_SRT_DT = " ";
			public readonly ZString REC_END_IDD_TE = "\x0D\x0A";
		}

		#endregion

		readonly GSSiMsgHeader header;
		readonly GSSiMsgBody body;

		ZString headerString
		{
			get
			{
				return header.formatVersion + header.replyNeeded + header.buildingID + header.commSpecId + header.isXML;
			}
		}

		ZString bodyString
		{
			get
			{
				return body.DXO_REC_NR +
							body.PKG_TCK_NR +
							body.PKG_INL_DEL_STS_CD +
							body.PKG_XCP_RSN_CD +
							body.PKG_COD_IR +
							body.AD_COR_IR +
							body.CMY_CTF_ORG_MNU_CD +
							body.EVT_TS +
							body.PKG_CSF_TYP_CD +
							body.CTM_NSP_REQ_IR +
							body.OUB_SRT_DT +
							body.REC_END_IDD_TE;
			}
		}

		#endregion
	}
}
