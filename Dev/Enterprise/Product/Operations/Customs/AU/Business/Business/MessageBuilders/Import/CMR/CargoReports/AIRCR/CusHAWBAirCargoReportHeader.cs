using System.Collections;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBAirCargoReportHeader : IAirCargoReportHeader
	{
		public CusHAWBAirCargoReportHeader(CusHAWBBase hAWB)
		{
			HAWB = hAWB;
			MasterBill = HAWB.MAWB;
		}

		#region ICargoReportHeader Members

		public EDIMessageCollection Messages
		{
			get { return HAWB.Messages; }
		}

		public ZString MethodOfPayment
		{
			get { return HAWB.CS_FreightPrepaidCollect; }
		}

		public ZString Origin
		{
			get { return HAWB.CS_RL_NKOrigin; }
		}

		public ZString Destination
		{
			get { return HAWB.CS_RL_NKDestination; }
		}

		public ZString Loading
		{
			get { return MasterBill?.CM_RL_NKLoadPort ?? ZString.Empty; }
		}

		public ZString Discharge
		{
			get { return MasterBill?.CM_RL_NKDischargePort ?? ZString.Empty; }
		}

		public ZString ResponsiblePartyID
		{
			get
			{
				ZString result = HAWB.CS_ResponsiblePartyID;
				if (result.IsEmpty && MasterBill != null)
				{
					result = MasterBill.CM_ResponsiblePartyID;
				}
				return result;
			}
		}

		public ZString[] Routings
		{
			get
			{
				ArrayList values = new ArrayList();
				if (MasterBill != null)
				{
					if (!MasterBill.CM_RL_NKRoutePort1.IsEmpty)
					{
						values.Add(MasterBill.CM_RL_NKRoutePort1);
					}

					if (!MasterBill.CM_RL_NKRoutePort2.IsEmpty)
					{
						values.Add(MasterBill.CM_RL_NKRoutePort2);
					}

					if (!MasterBill.CM_RL_NKRoutePort3.IsEmpty)
					{
						values.Add(MasterBill.CM_RL_NKRoutePort3);
					}
				}

				return (ZString[])values.ToArray(typeof(ZString));
			}
		}

		public ZString FirstArrivalPort
		{
			get
			{
				ZString result = "";
				if (!HAWB.CS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.Australia))
				{
					result = MasterBill?.CM_RL_NKFirstArrivalPort ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZString ConsigneeGeneralAddress
		{
			get
			{
				ZString result = "";

				if (IsUnmatchedOrganisation(HAWB.Consignee?.PK ?? ZGuid.Empty))
				{
					result = UnmatchedItemText("Consignee:-", ADDRESSStr, OrganisationTypes.Consignee);
				}
				else
				{
					result = ConsigneeStreet + " " + ConsigneeStreet2 + " " + ConsigneeCity + " " + ConsigneeState + " " + ConsigneePostCode + " " + ConsigneeCountry;
				}
				return result;
			}
		}

		#region Unmatched Organisation

		string UnmatchedItemText(string itemToSearchFor, string segmentToReturn, OrganisationTypes orgType)
		{
			string result = "";
			if (fFoundNote == null)
			{
				ForwardingShipment shipment = HAWB.Factory.Load<ForwardingShipment>(HAWB.CS_JS);
				if (shipment != null)
				{
					StmNote[] foundNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
					if (foundNotes.Length > 0)
					{
						fFoundNote = foundNotes[0];
					}

					shipmentRelatedUnmatchOrgRecords = new UnmatchOrgRecords(shipment);
				}
			}
			if (fFoundNote != null)
			{
				UnmatchOrgRecord orgRecord = null;

				if (shipmentRelatedUnmatchOrgRecords != null && shipmentRelatedUnmatchOrgRecords.OrgDetailsList.Length > 0)
				{
					orgRecord = shipmentRelatedUnmatchOrgRecords.FindUnmatchOrgByOrgType(orgType);
					if (orgRecord != null)
					{
						if (segmentToReturn == NAMEStr)
						{
							result = orgRecord.OrganisationName;
						}
						else if (segmentToReturn == ADDRESSStr)
						{
							result = GetOrgAddressFromOrgUnmatchNote(orgRecord);
						}
					}
				}
				if (orgRecord == null)
				{
					int itemIndex = fFoundNote.ST_NoteText.IndexOf(itemToSearchFor);
					if (itemIndex > 0)
					{
						string noteText = fFoundNote.ST_NoteText.Substring(itemIndex);
						int segmentIndex = noteText.IndexOf(segmentToReturn);
						if (segmentIndex > 0)
						{
							result = noteText.Substring(segmentIndex + segmentToReturn.Length + 1);
							int endPosition = result.IndexOf("\r\n");
							if (endPosition > 0)
							{
								result = result.Substring(0, endPosition);
							}
						}
					}
				}
			}
			return result;
		}
		StmNote fFoundNote;
		UnmatchOrgRecords shipmentRelatedUnmatchOrgRecords;

		ZString GetOrgAddressFromOrgUnmatchNote(UnmatchOrgRecord orgRecord)
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(orgRecord.AddressLine1);
			builder.AppendIfNotEmpty(orgRecord.AddressLine2);
			builder.AppendIfNotEmpty(orgRecord.City);
			builder.AppendIfNotEmpty(orgRecord.PostCode);
			builder.AppendIfNotEmpty(orgRecord.StateOrProvince);
			builder.AppendIfNotEmpty(orgRecord.Country);
			return builder.ToStringWithDelimiterBetweenAppends(" ");
		}

		bool IsUnmatchedOrganisation(ZGuid pK)
		{
			return (pK == Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
		}

		#endregion

		public ZString ConsigneeName
		{
			get
			{
				if (IsUnmatchedOrganisation(HAWB.Consignee?.PK ?? ZGuid.Empty))
				{
					return UnmatchedItemText("Consignee:-", NAMEStr, OrganisationTypes.Consignee);
				}
				else
				{
					return HAWB.CS_ConsigneeName;
				}
			}
		}

		public ZString ConsigneeStreet
		{
			get { return HAWB.CS_ConsigneeStreet; }
		}

		public ZString ConsigneeStreet2
		{
			get { return HAWB.CS_ConsigneeStreet2; }
		}

		public ZString ConsigneeCity
		{
			get { return HAWB.CS_ConsigneeCity; }
		}

		public ZString ConsigneeState
		{
			get { return HAWB.CS_ConsigneeState; }
		}

		public ZString ConsigneePostCode
		{
			get { return HAWB.CS_ConsigneePostcode; }
		}

		public ZString ConsigneeCountry
		{
			get { return HAWB.CS_RN_NKConsigneeCountry; }
		}

		public ZString ConsignorGeneralAddress
		{
			get
			{
				ZString result = "";
				if (IsUnmatchedOrganisation(HAWB.Consignor?.PK ?? ZGuid.Empty))
				{
					result = UnmatchedItemText("Consignor:-", ADDRESSStr, OrganisationTypes.Consignor);
				}
				else
				{
					result = ConsignorStreet + " " + ConsignorStreet2 + " " + ConsignorCity + " " + ConsignorState + " " + ConsignorPostCode + " " + ConsignorCountry;
				}
				return result;
			}
		}

		public ZString ConsignorName
		{
			get
			{
				if (IsUnmatchedOrganisation(HAWB.Consignor?.PK ?? ZGuid.Empty))
				{
					return UnmatchedItemText("Consignor:-", NAMEStr, OrganisationTypes.Consignor);
				}
				else
				{
					return HAWB.CS_ConsignorName;
				}
			}
		}

		public ZString ConsignorStreet
		{
			get { return HAWB.CS_ConsignorStreet; }
		}

		public ZString ConsignorStreet2
		{
			get { return HAWB.CS_ConsignorStreet2; }
		}

		public ZString ConsignorCity
		{
			get { return HAWB.CS_ConsignorCity; }
		}

		public ZString ConsignorState
		{
			get { return HAWB.CS_ConsignorState; }
		}

		public ZString ConsignorPostCode
		{
			get { return HAWB.CS_ConsignorPostcode; }
		}

		public ZString ConsignorCountry
		{
			get { return HAWB.CS_RN_NKConsignorCountry; }
		}

		public ZString ConsigneeIdentifier => HAWB.CS_ConsigneeIdentifier;

		public ZString ConsigneeABN => CargoReportHelper.GetConsigneeABN(HAWB.CS_ConsigneeBusinessNumber);

		public ZString ConsigneeCAC => CargoReportHelper.GetConsigneeCAC(HAWB.CS_ConsigneeBusinessNumber);

		public ZString ConsigneeTIN => Declaration.Business.CargoHelper.GetTraderIdentificationNumber(HAWB.Consignee);

		public ZString ConsignorIdentifier => HAWB.CS_ConsignorIdentifier;

		public ZString ConsignorVendor => HAWB.CS_VendorIdentifier;

		public ZString ConsignorTIN => Declaration.Business.CargoHelper.GetTraderIdentificationNumber(HAWB.Consignor);

		public ZString MasterHouseBill
		{
			get { return HAWB.AggregatedCoLoadMaster; }
		}

		public ZString HAWBNum
		{
			get { return HAWB.CS_HAWB; }
		}

		public ZString MAWB
		{
			get { return MasterBill?.CM_MAWB ?? ZString.Empty; }
		}

		ZString IAirCargoReportHeader.MatchConsignmentReference
		{
			get { return HAWB.CS_fPartShipConsignmentReference; }
		}

		public ZString FlightNo
		{
			get { return MasterBill?.CM_FlightNo ?? ZString.Empty; }
		}

		public ZDateTime ArivalDate
		{
			get { return MasterBill?.CM_ArrivalDate ?? ZDateTime.Empty; }
		}

		public bool IsMasterHouse
		{
			get { return HAWB.CS_IsMasterHouse; }
		}

		public bool IsDocuments
		{
			get { return HAWB.IsDocuments; }
		}

		public bool IsPersonalEffects
		{
			get { return HAWB.CS_IsPersonalEffects; }
		}

		public bool IsSelfAssessedClearance
		{
			get { return HAWB.CS_IsSelfAssessedClearance; }
		}

		public int PackageCount
		{
			get { return HAWB.CS_PiecesManifested; }
		}

		public ZString GoodsDescription
		{
			get { return HAWB.CS_GoodsDescription; }
		}

		public ZDecimal Weight
		{
			get { return HAWB.CS_Weight; }
		}

		public ZString WeightUQ
		{
			get { return HAWB.CS_WeightUQ; }
		}

		public ZDecimal GoodsValue
		{
			get { return HAWB.CS_GoodsValue; }
		}

		public ZString GoodsValueCurrency
		{
			get { return HAWB.CS_RX_NKGoodsCurrency; }
		}

		public bool IsHVLVSpecialReporter
		{
			get { return HAWB.CS_IsSpecialReporter; }
		}

		public bool IsRemailSpecialReporter
		{
			get { return HAWB.CS_IsRemailReporter; }
		}

		public bool IsBureau
		{
			get { return MasterBill?.CM_IsBureau ?? false; }
		}

		public bool CanDelaySending
		{
			get
			{
				var result = false;
				var mawb = MasterBill;
				if (mawb != null && mawb.CM_ArrivalDate.IsValid && !mawb.CM_RL_NKDischargePort.IsEmpty && mawb.DischargePort != null)
				{
					var arrivalDateUTC = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(mawb.CM_RL_NKDischargePort, mawb.CM_ArrivalDate.ToDateTime());
					var checkDate = ZDateTime.UtcNow.AddHours(4).AddMinutes(15).ToDateTime();
					result = arrivalDateUTC > checkDate;
				}
				return result;
			}
		}

		ZString ICargoReportHeader.NotifyPartyName => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyStreet => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyStreet2 => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyCity => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyPostCode => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyCountry => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyGeneralAddress => ZString.Empty;

		#endregion

		#region Implementation

		protected readonly CusHAWBBase HAWB;
		protected readonly CusMAWBBase MasterBill;

		internal const string NAMEStr = "NAME:";
		internal const string ADDRESSStr = "ADDRESS:";
		#endregion
	}
}
