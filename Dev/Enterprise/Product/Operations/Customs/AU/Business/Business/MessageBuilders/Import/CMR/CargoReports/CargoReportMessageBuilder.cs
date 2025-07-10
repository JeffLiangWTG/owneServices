using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Edifact.Utilities;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CargoReportMessageBuilder : CMRCUSCARMessageBuilder
	{
		public CargoReportMessageBuilder(ICargoReportHeader header)
			: this(header, ZString.Empty)
		{
		}

		public CargoReportMessageBuilder(ICargoReportHeader header, ZString messageOwnerSiteID)
			: base(messageOwnerSiteID)
		{
			this.header = header;
		}

		public OrgHeader ResponsibleParty
		{
			get { return GlbCompany.CurrentCompany.OrgProxy; }
		}

		protected internal override DocumentNameCodeList DocumentNameCode
		{
			get { return DocumentNameCodeList.CargoDeclarationArrival; }
		}

		protected ICargoReportHeader header;

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateGroup1();
				if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
				{
					PopulateGroup2();
				}
				PopulateGroup4();
				if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
				{
					PopulateGISSegments();
					PopulateGroup7();
				}
				PopulateUNT();
			}
		}

		#region Group1

		protected virtual void PopulateGroup1()
		{
			if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				//				if (!Header.MethodOfPayment.IsEmpty) //TODO: replace this when/if Customs fixes Incident4
				{
					MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.PaymentReference, new CMRUtilities().ConvertOldAirCargoPaymentType(header.MethodOfPayment), null);
				}
			}
		}

		#endregion

		#region Group2

		protected internal virtual void PopulateGroup2()
		{
			SegmentGroup2 consigneeGroup = CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection();
			PopulateWithGeneralDetailsNAD(consigneeGroup, PartyFunctionCodeQualifierList.Consignee, header.ConsigneeName, header.ConsigneeGeneralAddress);
			var consignorGroup = CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection();
			PopulateWithGeneralDetailsNAD(consignorGroup, PartyFunctionCodeQualifierList.Consignor, header.ConsignorName, header.ConsignorGeneralAddress);

			var consignorTIN = header.ConsignorTIN;
			if (!consignorTIN.IsEmpty)
			{
				var partyIdentifier = consignorTIN.Replace(" ", "").Replace("-", "").Substring(0, 35);
				consignorGroup.NAD[0].PartyIdentificationDetails.PartyIdentifier = partyIdentifier;
			}

			CargoReportHelper.DoICSRelease(() =>
			{
				var consigneeTIN = header.ConsigneeTIN;
				if (!consigneeTIN.IsEmpty)
				{
					MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.AuthorizedTraderTransit, consigneeTIN, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}
				else
				{
					var abn = header.ConsigneeABN;
					if (!abn.IsEmpty)
					{
						MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.AuthorizedImporter, abn, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
					}

					var cac = header.ConsigneeCAC;
					if (!cac.IsEmpty)
					{
						MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.SubEntity, cac, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
					}

					var consigneeId = header.ConsigneeIdentifier;
					if (!consigneeId.IsEmpty)
					{
						MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.Importer, consigneeId, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
					}
				}

				var consignorIdentifier = header.ConsignorIdentifier;
				if (!consignorIdentifier.IsEmpty)
				{
					MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.Supplier, consignorIdentifier, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}
			});

			PopulateGroup2ResponsiblePartyID();
		}

		protected virtual void PopulateGroup2ResponsiblePartyID()
		{
			ZString responsiblePartyClientID = header.ResponsiblePartyID;
			if (responsiblePartyClientID.IsEmpty)
			{
				responsiblePartyClientID = new OrgHeaderWrapper(ResponsibleParty).GoodsOwnerPartyID;
			}

			if (!responsiblePartyClientID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.ResponsibleParty, responsiblePartyClientID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		internal void PopulateWithGeneralDetailsNAD(SegmentGroup2 group2, PartyFunctionCodeQualifierList partyType, ZString name, ZString details)
		{
			group2.NAD[0].PartyFunctionCodeQualifier = partyType;

			TextSplitter nameSplitter = new TextSplitter(35);
			nameSplitter.Text = name.Replace("\r\n", " ");
			group2.NAD[0].NameAndAddress.NameAndAddressLine1 = nameSplitter[0];
			group2.NAD[0].NameAndAddress.NameAndAddressLine2 = nameSplitter[1];

			TextSplitter addressSplitter = new TextSplitter(35);
			addressSplitter.Text = details.Replace("\r\n", " ");
			group2.NAD[0].NameAndAddress.NameAndAddressLine3 = addressSplitter[0];
			group2.NAD[0].NameAndAddress.NameAndAddressLine4 = addressSplitter[1];
			group2.NAD[0].NameAndAddress.NameAndAddressLine5 = addressSplitter[2];
		}

		protected void PopulateNAD(SegmentGroup2 group2, PartyFunctionCodeQualifierList partyType, OrgHeader organisation)
		{
			MessageUtilities.PopulateNAD(group2.NAD[0], partyType, null, null, organisation.OH_FullNameTruncated, organisation.MainAddress.OA_City);

			ZString address1 = organisation.MainAddress.OA_Address1;
			ZString address2 = organisation.MainAddress.OA_Address2;
			if (address1.Length > 35)
			{
				address2 = address1.Substring(35) + address2;
				address1 = address1.Left(35);
			}
			address2 = address2.Left(35);

			group2.NAD[0].Street.StreetAndNumberPOBox1 = address1;
			group2.NAD[0].Street.StreetAndNumberPOBox2 = address2;
			group2.NAD[0].PostalIdentificationCode = organisation.MainAddress.OA_PostCode;
			if (organisation.ClosestPort != null && organisation.ClosestPort.Country != null)
			{
				group2.NAD[0].CountryNameCode = organisation.ClosestPort.RL_RN_NKCountryCode;
			}
		}

		protected void PopulateNADWithEnteredDetails(SegmentGroup2 group2, PartyFunctionCodeQualifierList partyType, ZString name, ZString addressLine1, ZString addressLine2, ZString city, ZString postCode, ZString countryCode)
		{
			MessageUtilities.PopulateNAD(group2.NAD[0], partyType, null, null, name, city);
			if (addressLine1.Length > 35)
			{
				addressLine2 = addressLine1.Substring(35) + addressLine2;
				addressLine1 = addressLine1.Left(35);
			}
			addressLine2 = addressLine2.Left(35);

			group2.NAD[0].Street.StreetAndNumberPOBox1 = addressLine1;
			group2.NAD[0].Street.StreetAndNumberPOBox2 = addressLine2;
			group2.NAD[0].PostalIdentificationCode = postCode;
			group2.NAD[0].CountryNameCode = countryCode;
		}

		#endregion

		#region Group4

		protected virtual void PopulateGroup4()
		{
			PopulateLocations();
		}

		protected virtual internal void PopulateLocations()
		{
			if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				if (!header.Destination.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfDestination, header.Destination, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
				if (!header.Loading.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.OriginalPortOfLoading, header.Loading, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
				if (!header.FirstArrivalPort.IsEmpty && !header.Destination.StartsWith(Core.Constants.CountryCodes.Australia))
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlacePortOfFirstEntry, header.FirstArrivalPort, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}

				if (header.Routings != null)
				{
					foreach (ZString routing in header.Routings)
					{
						if (!routing.IsEmpty)
						{
							MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.Routing, routing, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
						}
					}
				}
			}
		}

		#endregion

		#region GIS

		protected virtual void PopulateGISSegments()
		{
		}

		#endregion

		#region Group7

		protected virtual void PopulateGroup7()
		{
		}

		#endregion
	}
}
