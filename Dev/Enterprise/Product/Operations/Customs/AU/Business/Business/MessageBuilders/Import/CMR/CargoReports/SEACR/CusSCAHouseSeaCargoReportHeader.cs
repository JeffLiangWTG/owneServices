using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseSeaCargoReportHeader : ISeaCargoReportHeader
	{
		public CusSCAHouseSeaCargoReportHeader(CusSCAHouse sCAHouse, EDIMessageCollection messages)
		{
			if (Object.ReferenceEquals(sCAHouse, null))
			{
				throw new ArgumentNullException(nameof(sCAHouse), "Parameter must be a valid CusSCAHouse");
			}
			this.sCAHouse = sCAHouse;
			this.messages = messages;
		}

		public CusSCAHouseSeaCargoReportHeader(CusSCAHouse sCAHouse)
			: this(sCAHouse, sCAHouse.Messages)
		{
		}

		#region ICargoReportHeader Members

		readonly EDIMessageCollection messages;

		public EDIMessageCollection Messages
		{
			get { return messages; }
		}

		public ZString MethodOfPayment
		{
			get { return sCAHouse.CA_PrepaidCollectOther; }
		}

		public ZString Origin
		{
			get { return sCAHouse.CA_RL_NK_PortOfOrigin; }
		}

		public ZString Destination
		{
			get { return sCAHouse.CA_RL_NK_PortOfDestination; }
		}

		public ZString Loading
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_RL_NKPortOfLoading;
				}
				return result;
			}
		}

		public ZString Discharge
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_RL_NKPortOfDischarge;
				}
				return result;
			}
		}

		public ZString FirstArrivalPort
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_RL_NKPortOfFirstArrival;
				}
				return result;
			}
		}

		public ZString[] Routings
		{
			get { return null; }
		}

		public ZString ConsigneeGeneralAddress
		{
			get
			{
				return IsUnmatchedOrganisation(sCAHouse.Consignee?.PK ?? ZGuid.Empty) ?
					UnmatchedItemText("Consignee:-", ADDRESSStr, OrganisationTypes.Consignee) :
					ConsigneeStreet + " " + ConsigneeStreet2 + " " + ConsigneeCity + " " + ConsigneePostCode + " " + ConsigneeCountry;
			}
		}

		public ZString ConsignorGeneralAddress
		{
			get
			{
				return IsUnmatchedOrganisation(sCAHouse.Consignor?.PK ?? ZGuid.Empty) ?
					UnmatchedItemText("Consignor:-", ADDRESSStr, OrganisationTypes.Consignor) :
					ConsignorStreet + " " + ConsignorStreet2 + " " + ConsignorCity + " " + ConsignorPostCode + " " + ConsignorCountry;
			}
		}

		public ZString NotifyPartyGeneralAddress
		{
			get
			{
				return NotifyPartyStreet + " " + NotifyPartyStreet2 + " " + NotifyPartyCity + " " + NotifyPartyPostCode + " " + NotifyPartyCountry;
			}
		}

		#region Unmatched Organisation

		string UnmatchedItemText(string itemToSearchFor, string segmentToReturn, OrganisationTypes orgType)
		{
			LoadNoteAndShipmentRelatedUnMatchOrgRecordsIfNotAlreadyLoaded();

			string result = "";
			if (foundNote != null)
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
					var itemIndex = foundNote.ST_NoteText.IndexOf(itemToSearchFor);
					if (itemIndex > 0)
					{
						var noteText = foundNote.ST_NoteText.Substring(itemIndex);
						var segmentIndex = noteText.IndexOf(segmentToReturn);
						if (segmentIndex > 0)
						{
							result = noteText.Substring(segmentIndex + segmentToReturn.Length + 1);
							var endPosition = result.IndexOf("\r\n");
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

		void LoadNoteAndShipmentRelatedUnMatchOrgRecordsIfNotAlreadyLoaded()
		{
			if (!hasLoadedNote)
			{
				var shipment = sCAHouse.Factory.Load<ForwardingShipment>(sCAHouse.CA_JS);
				if (shipment != null)
				{
					var foundNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
					if (foundNotes.Length > 0)
					{
						foundNote = foundNotes[0];
					}
					shipmentRelatedUnmatchOrgRecords = new UnmatchOrgRecords(shipment);
				}

				hasLoadedNote = true;
			}
		}

		bool hasLoadedNote;
		StmNote foundNote;
		UnmatchOrgRecords shipmentRelatedUnmatchOrgRecords;

		ZString GetOrgAddressFromOrgUnmatchNote(UnmatchOrgRecord orgRecord)
		{
			var builder = new ZStringBuilder();
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

		#region Consignee

		public ZString ConsigneeName
		{
			get
			{
				return IsUnmatchedOrganisation(sCAHouse.Consignee?.PK ?? ZGuid.Empty) ?
					UnmatchedItemText("Consignee:-", NAMEStr, OrganisationTypes.Consignee) :
					sCAHouse.CA_ConsigneeName.ToString();
			}
		}

		public ZString ConsigneeStreet
		{
			get { return sCAHouse.CA_ConsigneeAddress1; }
		}

		public ZString ConsigneeStreet2
		{
			get { return sCAHouse.CA_ConsigneeAddress2; }
		}

		public ZString ConsigneeCity
		{
			get { return sCAHouse.CA_ConsigneeSuburb; }
		}

		public ZString ConsigneePostCode
		{
			get { return sCAHouse.CA_ConsigneePostcode; }
		}

		public ZString ConsigneeCountry
		{
			get { return sCAHouse.CA_RN_NKConsigneeCountryCode; }
		}

		public ZString ConsigneeIdentifier => sCAHouse.CA_ConsigneeIdentifier;

		public ZString ConsigneeABN => CargoReportHelper.GetConsigneeABN(sCAHouse.CA_ConsigneeBusinessNumber);

		public ZString ConsigneeCAC => CargoReportHelper.GetConsigneeCAC(sCAHouse.CA_ConsigneeBusinessNumber);

		public ZString ConsigneeTIN => Declaration.Business.CargoHelper.GetTraderIdentificationNumber(sCAHouse.Consignee);

		#endregion

		#region Consignor

		public ZString ConsignorName
		{
			get
			{
				return IsUnmatchedOrganisation(sCAHouse.Consignor?.PK ?? ZGuid.Empty) ?
					UnmatchedItemText("Consignor:-", NAMEStr, OrganisationTypes.Consignor) :
					sCAHouse.CA_ConsignorName.ToString();
			}
		}

		public ZString ConsignorStreet
		{
			get { return sCAHouse.CA_ConsignorAddress1; }
		}

		public ZString ConsignorStreet2
		{
			get { return sCAHouse.CA_ConsignorAddress2; }
		}

		public ZString ConsignorCity
		{
			get { return sCAHouse.CA_ConsignorSuburb; }
		}

		public ZString ConsignorPostCode
		{
			get { return sCAHouse.CA_ConsignorPostcode; }
		}

		public ZString ConsignorCountry
		{
			get { return sCAHouse.CA_RN_NKConsignorCountryCode; }
		}

		public ZString ConsignorIdentifier => sCAHouse.CA_ConsignorIdentifier;

		public ZString ConsignorVendor => sCAHouse.CA_VendorIdentifier;

		public ZString ConsignorTIN => Declaration.Business.CargoHelper.GetTraderIdentificationNumber(sCAHouse.Consignor);

		#endregion

		public OrgHeader NotifyParty
		{
			get { return sCAHouse.Notify; }
		}

		#region NotifyParty

		public ZString NotifyPartyName => sCAHouse.CA_NotifyName.ToString();

		public ZString NotifyPartyStreet => sCAHouse.CA_NotifyAddress1;

		public ZString NotifyPartyStreet2 => sCAHouse.CA_NotifyAddress2;

		public ZString NotifyPartyCity => sCAHouse.CA_NotifySuburb;

		public ZString NotifyPartyPostCode => sCAHouse.CA_NotifyPostcode;

		public ZString NotifyPartyCountry => sCAHouse.CA_RN_NKNotifyCountryCode;

		#endregion

		#endregion

		public ZString OceanBill
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_OceanBill;
				}
				return result;
			}
		}

		public ZString HouseBill
		{
			get { return sCAHouse.CA_HouseBill; }
		}

		public ZString ParentBill
		{
			get { return sCAHouse.AggregatedMasterHouseBill; }
		}

		public ZString Voyage
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_Voyage;
				}
				return result;
			}
		}

		public ZString LloydsNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_LloydsIMO;
				}
				return result;
			}
		}

		public ZString ResponsiblePartyID
		{
			get
			{
				ZString result = ZString.Empty;
				if (!sCAHouse.CA_ResponsiblePartyID.IsEmpty)
				{
					result = sCAHouse.CA_ResponsiblePartyID;
				}
				else if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_ResponsiblePartyID;
				}
				return result;
			}
		}

		public ZString OriginCountry
		{
			get { return sCAHouse.CA_RN_NKGoodsOrigin; }
		}

		public bool CanDelaySending
		{
			get
			{
				var result = false;
				var oceanBill = sCAHouse.OceanBill;
				if (oceanBill != null && oceanBill.CB_DateOfArrival.IsValid && !oceanBill.CB_RL_NKPortOfDischarge.IsEmpty && oceanBill.PortOfDischarge != null)
				{
					var lateTimeframe = AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.Value;
					var checkDate = ZDateTime.UtcNow.AddHours(lateTimeframe).AddMinutes(15).ToDateTime();
					var arrivalDateUTC = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(oceanBill.CB_RL_NKPortOfDischarge, oceanBill.CB_DateOfArrival.ToDateTime());
					result = arrivalDateUTC > checkDate;
				}
				return result;
			}
		}

		public bool IsConsolidation
		{
			get
			{
				return sCAHouse.CA_IsMasterHouse;
			}
		}

		public bool IsBureau
		{
			get
			{
				bool result = false;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_IsBureau;
				}
				return result;
			}
		}

		public ZString PrincipalID
		{
			get
			{
				ZString result = ZString.Empty;
				if (sCAHouse.OceanBill != null)
				{
					result = sCAHouse.OceanBill.CB_PrincipalID;
				}
				return result;
			}
		}

		public BusinessObject BusinessObject
		{
			get { return sCAHouse; }
		}

		public ISeaCargoReportLine[] Lines
		{
			get { return GetLines(sCAHouse); }
		}

		public ISeaCargoReportLine[] DatabaseLines
		{
			get { return GetLines(new BusinessObjectFactory().Load<CusSCAHouse>(sCAHouse.PK)); }
		}

		ISeaCargoReportLine[] GetLines(CusSCAHouse house)
		{
			ArrayList result = new ArrayList();
			if (house != null)
			{
				foreach (CusSCAPivot pivot in house.Pivot)
				{
					if (pivot.Container != null)
					{
						result.Add(new CusSCAPivotSeaCargoReportLine(pivot));
					}
				}
			}
			return (ISeaCargoReportLine[])result.ToArray(typeof(ISeaCargoReportLine));
		}

		#region Implementation

		protected CusSCAHouse sCAHouse;

		internal const string NAMEStr = "NAME:";
		internal const string ADDRESSStr = "ADDRESS:";

		#endregion
	}
}
