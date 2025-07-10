using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryNumHelperForCargoControlNumber = Enterprise.Customs.Business.CusEntryNumHelperForCargoControlNumber;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ForwardedManifestSupporter
	{
		public ForwardedManifestSupporter(ACIHouseBillMessage forwardedManifestMessage)
		{
			this.forwardedManifestMessage = forwardedManifestMessage;
			Argument.NotNull(forwardedManifestMessage, "forwardedManifestMessage");
			Argument.NotNull(forwardedManifestMessage.CargoControlNumber, "forwardedManifestMessage.CargoControlNumber");
		}
		readonly ACIHouseBillMessage forwardedManifestMessage;

		static ZDBOnlyQuery DeclarationsByCCNQuery(string ccn)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration)) { OrderBy = JobDeclarationSchema.JE_DeclarationReference.Name };
			jobDeclarationQuery.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.Equal, ccn), JoinCondition.And);
			return jobDeclarationQuery;
		}

		static ZDBOnlyQuery ShipmentsByCCNQuery(string ccn)
		{
			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment)) { OrderBy = JobShipmentSchema.JS_UniqueConsignRef.Name };
			shipmentQuery.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobShipmentSchema.Constants.TableName, SQLComparisonOperator.Equal, ccn), JoinCondition.Or);
			return shipmentQuery;
		}

		public JobDeclaration[] GetMatchingDeclarations()
		{
			return forwardedManifestMessage.Factory.Load<JobDeclaration>(DeclarationsByCCNQuery(forwardedManifestMessage.CargoControlNumber));
		}

		public ForwardingShipment[] GetMatchingShipments()
		{
			return forwardedManifestMessage.Factory.Load<ForwardingShipment>(ShipmentsByCCNQuery(forwardedManifestMessage.CargoControlNumber));
		}

		public JobDeclaration FindOrCreateJobDeclarationMatchingOnCCN()
		{
			var query = DeclarationsByCCNQuery(forwardedManifestMessage.CargoControlNumber);
			var matchingDeclarations = forwardedManifestMessage.Factory.Load<JobDeclaration>(query);
			if (matchingDeclarations.Length > 1)
			{
				return null;
			}

			if (matchingDeclarations.Length == 1)
			{
				return matchingDeclarations[0];
			}

			var newDeclaration = forwardedManifestMessage.Factory.New<JobDeclaration>();
			var unmatchedOrgBuilder = new ZStringBuilder();
			var otherInfoBuilder = new ZStringBuilder();
			foreach (INameAndAddress nameAndAddress in forwardedManifestMessage.Wrapper.NamesAndAddresses)
			{
				if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.Consignee)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Consignee, unmatchedOrgBuilder);
					newDeclaration.JE_OH_Importer = orgHeaderPK;
				}
				else if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.Consignor)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Consignor, unmatchedOrgBuilder);
					newDeclaration.JE_OH_Supplier = orgHeaderPK;
				}
				else if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.DeliveryParty)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Consignee, unmatchedOrgBuilder, reportNotMatched: false);
					if (orgHeaderPK.IsValid)
					{
						newDeclaration.ImporterDeliveryAddress.OrganisationPK = orgHeaderPK;
					}
					else
					{
						var deliveryAddress = newDeclaration.ImporterDeliveryAddress;
						deliveryAddress.E2_AddressOverride = true;
						deliveryAddress.E2_CompanyName = nameAndAddress.Name;
						deliveryAddress.E2_Address1 = nameAndAddress.Address1;
						deliveryAddress.E2_Address2 = nameAndAddress.Address2;
						deliveryAddress.E2_City = nameAndAddress.City;
						deliveryAddress.E2_State = nameAndAddress.State;
						deliveryAddress.E2_Postcode = nameAndAddress.PostCode;
						deliveryAddress.E2_RN_NKCountryCode = nameAndAddress.Country;
						deliveryAddress.E2_Phone = nameAndAddress.Telephone;
						deliveryAddress.E2_Contact = nameAndAddress.ContactName;
					}
				}
				else
				{
					AddNameAndAddressNote(nameAndAddress, newDeclaration.Notes);
				}
			}
			newDeclaration.JE_MessageType = Enterprise.Customs.CA.Business.JobMessageTypeList.Codes.Import;
			newDeclaration.JE_HouseBill = forwardedManifestMessage.CargoControlNumber.SubstringSafe(4).TrimStart();
			var number = newDeclaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = forwardedManifestMessage.CargoControlNumber;
			number = newDeclaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			number.CE_EntryNum = forwardedManifestMessage.PrimaryCargoControlNumber;
			if (!forwardedManifestMessage.Wrapper.UCN.IsNullOrEmpty())
			{
				number = newDeclaration.AdditionalReferenceNumbers.AddNew();
				number.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
				number.CE_EntryNum = forwardedManifestMessage.Wrapper.UCN;
			}
			if (!forwardedManifestMessage.Wrapper.B2BComments.IsNullOrEmpty())
			{
				newDeclaration.Notes.AddNew(true, Res.GetString("b6994cb5-bc26-4f08-92fb-e5cd6a5abae8", "Forwarded Manifest B2B Notes"), forwardedManifestMessage.Wrapper.B2BComments);
			}
			newDeclaration.JE_TransportMode = new CusCAeMHTransportModeList().GetCodeFromDescription(forwardedManifestMessage.Wrapper.ModeOfTransport);
			var volume = ZDecimal.Zero;
			if (ZDecimal.TryParse(forwardedManifestMessage.Wrapper.Volume, out volume))
			{
				newDeclaration.JE_TotalVolume = volume;
				newDeclaration.JE_TotalVolumeUnit = CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(forwardedManifestMessage.Wrapper.VolumeUnits, forwardedManifestMessage.Factory);
			}
			if (!forwardedManifestMessage.Wrapper.SpecialInstructions.IsNullOrEmpty())
			{
				newDeclaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, forwardedManifestMessage.Wrapper.SpecialInstructions);
			}
			if (forwardedManifestMessage.Wrapper.LOCDestinationSegment != null)
			{
				if (!forwardedManifestMessage.Wrapper.PortOfDestination.IsNullOrEmpty())
				{
					newDeclaration.JE_CustomsOffice = forwardedManifestMessage.Wrapper.PortOfDestination;
				}

				if (!forwardedManifestMessage.Wrapper.SubLocationOfDestination.IsNullOrEmpty())
				{
					newDeclaration.JE_LocationOfGoods = forwardedManifestMessage.Wrapper.SubLocationOfDestination;
				}
			}
			if (forwardedManifestMessage.Wrapper.LOCDischargeSegment != null)
			{
				if (!forwardedManifestMessage.Wrapper.PortOfDischarge.IsNullOrEmpty())
				{
					newDeclaration.CA_UnladingOffice = forwardedManifestMessage.Wrapper.PortOfDischarge;
				}

				if (!forwardedManifestMessage.Wrapper.SubLocationOfDischarge.IsNullOrEmpty())
				{
					otherInfoBuilder.Append(Res.GetString("3a1ea996-ef60-4d5d-83cd-8c61862687d5", "Sub-location where discharged: {0}", forwardedManifestMessage.Wrapper.SubLocationOfDischarge));
				}
			}
			if (forwardedManifestMessage.Wrapper.Group143 != null)
			{
				var consolidator = forwardedManifestMessage.Wrapper.Consolidator;
				AddNameAndAddressNote(consolidator, newDeclaration.Notes);
			}
			if (!forwardedManifestMessage.Wrapper.DangerousGoodsSpecialInstructions.IsNullOrEmpty())
			{
				newDeclaration.Notes.AddNew(true, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, forwardedManifestMessage.Wrapper.DangerousGoodsSpecialInstructions);
			}
			foreach (IContainerAndSeals containerAndSeal in forwardedManifestMessage.Wrapper.ContainersAndSeals)
			{
				if (!containerAndSeal.ContainerNumber.IsNullOrEmpty())
				{
					var container = newDeclaration.CusContainers.AddNew();
					container.CO_ContainerNumber = containerAndSeal.ContainerNumber;
					int i = 0;
					foreach (IContainerSeal seal in containerAndSeal.ContainerSeals)
					{
						if (i == 0)
						{
							container.CO_Seal = seal.SealNumber;
						}
						else if (i == 1)
						{
							container.CO_SecondSeal = seal.SealNumber;
						}
						else
						{
							otherInfoBuilder.Append(Res.GetString("3aa84fdc-d8c6-4bc4-8a98-bbb41ebabd44", "Additional Seal: {0} on container: {1}", seal.SealNumber, containerAndSeal.ContainerNumber));
						}
						i++;
					}
				}
			}
			int j = 1;
			foreach (IConsignmentLine line in forwardedManifestMessage.Wrapper.ConsignmentLines)
			{
				ZInt packs = 0;
				ZInt.TryParse(line.CargoQuantity, out packs);
				var bill = newDeclaration.Bills.Count > 0 ? newDeclaration.Bills[0] : null;
				var packGroup = bill != null && bill.PackingGroups.Count > 0 ? bill.PackingGroups[0] : null;

				if (j == 1)
				{
					newDeclaration.JE_GoodsDescription = line.CargoDescription;
					newDeclaration.JE_TotalNoOfPacks = packs;
					newDeclaration.JE_TotalNoOfPacksPackType = line.CargoUnitOfMeasure;
					var package = packGroup != null && packGroup.Packages.Count > 0 ? packGroup.Packages[0] : null;
					if (package != null && package.CW_PackQty != packs)
					{
						package.CW_PackQty = packs;
						package.CW_PackType = line.CargoUnitOfMeasure;
					}
				}
				else
				{
					if (packGroup != null)
					{
						var package = packGroup.Packages.AddNew();
						package.CW_PackQty = packs;
						package.CW_PackType = line.CargoUnitOfMeasure;
					}
					otherInfoBuilder.Append(Res.GetString("7b7aa747-4454-46ad-833a-ad091e8e9535", "Goods Line {0} description of goods: {1}", j, line.CargoDescription));
				}
				if (!line.HsCommodityCode.IsNullOrEmpty())
				{
					otherInfoBuilder.Append(Res.GetString("86441c85-8b5c-4bab-bcd2-447147259a54", "Goods Line {0} HS Code: {1}", j, line.HsCommodityCode));
				}

				if (!line.MarksAndNumbers.IsNullOrEmpty())
				{
					otherInfoBuilder.Append(Res.GetString("9463254b-5133-48cc-8c42-ea654deaa33c", "Goods Line {0} Marks & Numbers: {1}", j, line.MarksAndNumbers));
				}

				foreach (IUNDGCode dgCode in line.UNDGCodes)
				{
					otherInfoBuilder.Append(Res.GetString("db9473eb-7b02-433e-b3b5-aba348121976", "Goods Line {0} UNDG Code: {1}", j, dgCode.UNDGCode));
				}
				j++;
			}
			var weight = ZDecimal.Zero;
			if (ZDecimal.TryParse(forwardedManifestMessage.Wrapper.CargoWeight, out weight))
			{
				newDeclaration.JE_TotalWeight = weight;
				newDeclaration.JE_TotalWeightUnit = CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(forwardedManifestMessage.Wrapper.CargoWeightUnits, forwardedManifestMessage.Factory);
			}
			if (!otherInfoBuilder.IsEmpty)
			{
				newDeclaration.Notes.AddNew(true, Res.GetString("d33289d1-9997-407f-a0f5-18467e5d35a6", "Forwarded Manifest Other Information"), otherInfoBuilder.ToStringWithNewLineBetweenAppends());
			}
			if (!unmatchedOrgBuilder.IsEmpty)
			{
				newDeclaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, unmatchedOrgBuilder.ToStringWithNewLineBetweenAppends());
			}
			return newDeclaration;
		}

		ZGuid FindMathingOrg(INameAndAddress nameAndAddress, OrganisationTypes orgType, ZStringBuilder unmatchedOrgBuilder, bool reportNotMatched = true)
		{
			Xsd.Organisation criteria = new Xsd.Organisation();
			criteria.OrganisationDetails.Name = nameAndAddress.Name;
			criteria.OrganisationDetails.Location.Country = nameAndAddress.Country;
			criteria.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address = criteria.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = nameAndAddress.Address1;
			address.AddressLine2 = nameAndAddress.Address1;
			address.CityOrSuburb = nameAndAddress.City;
			address.PostCode = nameAndAddress.PostCode;
			address.StateOrProvince = nameAndAddress.State;
			var matching = new OrganisationMatching(new BusinessObjectFactoryProvider(forwardedManifestMessage.Factory), Xsd.XmlInterchange.Empty, new NotificationBuffer());
			var result = matching.FindOrganisationPK(criteria, null, orgType);
			if (!result.IsValid && reportNotMatched)
			{
				unmatchedOrgBuilder.Append(Res.GetString("f2dab44f-db87-4527-9e7e-879e705fa2bb", "Unmatched Organization for {0}", GetNameAndAddressType(nameAndAddress.AddressType)));
				unmatchedOrgBuilder.Append(nameAndAddress.Name);
				unmatchedOrgBuilder.AppendIfNotEmpty(nameAndAddress.Address);
				if (nameAndAddress.CTASegment != null)
				{
					unmatchedOrgBuilder.AppendIfNotEmpty(nameAndAddress.ContactName);
				}

				if (nameAndAddress.COMSegment != null)
				{
					unmatchedOrgBuilder.AppendIfNotEmpty(nameAndAddress.Telephone);
				}
			}
			return result;
		}

		void AddNameAndAddressNote(INameAndAddress nameAndAddress, Notes notes)
		{
			var noteBuilder = new ZStringBuilder(Res.GetString("A9DB64BD-D91E-411F-A64A-B66F02DB191A", "Name/Address for {0}", GetNameAndAddressType(nameAndAddress.AddressType)));
			noteBuilder.Append(nameAndAddress.Name);
			noteBuilder.AppendIfNotEmpty(nameAndAddress.Address);
			if (nameAndAddress.CTASegment != null)
			{
				noteBuilder.AppendIfNotEmpty(nameAndAddress.ContactName);
			}

			if (nameAndAddress.COMSegment != null)
			{
				noteBuilder.AppendIfNotEmpty(nameAndAddress.Telephone);
			}

			notes.AddNew(true, Res.GetString("f936e474-4e03-43f5-83da-fc941ea18ecb", "Forwarded Manifest Address"), noteBuilder.ToStringWithNewLineBetweenAppends());
		}

		internal static ZString GetNameAndAddressType(PartyFunctionCodeQualifierList code)
		{
			if (code == PartyFunctionCodeQualifierList.Consignee)
			{
				return Res.GetString("263083c6-23a6-466b-90e9-51f6dde4803a", "Consignee");
			}
			else if (code == PartyFunctionCodeQualifierList.Consignor)
			{
				return Res.GetString("9135a94d-700e-4007-87fd-595efe0a93dc", "Shipper");
			}
			else if (code == PartyFunctionCodeQualifierList.DeliveryParty)
			{
				return Res.GetString("3a5e04b4-c260-491a-b669-1b03b625ad85", "Delivery Address");
			}
			else if (code == PartyFunctionCodeQualifierList.NotifyParty)
			{
				return Res.GetString("a733bf27-aa2d-4cfa-b196-c8a9b32f6025", "Notify Party");
			}
			else if (code == PartyFunctionCodeQualifierList.MutuallyDefined)
			{
				return Res.GetString("0342e23d-cd99-4e88-94d5-d1b72ad6beee", "Place of Consolidation");
			}
			else if (code == PartyFunctionCodeQualifierList.ContactParty)
			{
				return Res.GetString("1450fd87-cac1-4632-a9ec-f14561870cc4", "UNDG Contact");
			}
			else if (code == PartyFunctionCodeQualifierList.Consolidator)
			{
				return Res.GetString("3AC74D96-0C96-4CBD-9788-E2D8F3DDE936", "Consolidator");
			}
			else if (code == PartyFunctionCodeQualifierList.FreightForwarder)
			{
				return Res.GetString("8ec5f86c-ddbd-46c1-9221-5fb955da323d", "Freight Forwarder");
			}

			return ZString.Empty;
		}

		public CFSShipment AddNewShipmentToLoadList(CFSLoadListConsol loadListConsol)
		{
			var newShipment = loadListConsol.Factory.New<CFSShipment>();

			var unmatchedOrgBuilder = new ZStringBuilder();
			var otherInfoBuilder = new ZStringBuilder();
			foreach (INameAndAddress nameAndAddress in forwardedManifestMessage.Wrapper.NamesAndAddresses)
			{
				if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.Consignee)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Consignee, unmatchedOrgBuilder);
					newShipment.ConsigneeDocumentaryAddress.OrganisationPK = orgHeaderPK;
				}
				else if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.Consignor)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Consignor, unmatchedOrgBuilder);
					newShipment.ConsignorDocumentaryAddress.OrganisationPK = orgHeaderPK;
				}
				else if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.DeliveryParty)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Consignee, unmatchedOrgBuilder, reportNotMatched: false);
					if (orgHeaderPK.IsValid)
					{
						newShipment.ConsigneeDeliveryAddress.OrganisationPK = orgHeaderPK;
					}
					else
					{
						var deliveryAddress = newShipment.ConsigneeDeliveryAddress;
						deliveryAddress.E2_AddressOverride = true;
						deliveryAddress.E2_CompanyName = nameAndAddress.Name;
						deliveryAddress.E2_Address1 = nameAndAddress.Address1;
						deliveryAddress.E2_Address2 = nameAndAddress.Address2;
						deliveryAddress.E2_City = nameAndAddress.City;
						deliveryAddress.E2_State = nameAndAddress.State;
						deliveryAddress.E2_Postcode = nameAndAddress.PostCode;
						deliveryAddress.E2_RN_NKCountryCode = nameAndAddress.Country;
						deliveryAddress.E2_Phone = nameAndAddress.Telephone;
						deliveryAddress.E2_Contact = nameAndAddress.ContactName;
					}
				}
				else if (nameAndAddress.AddressType == PartyFunctionCodeQualifierList.FreightForwarder)
				{
					var orgHeaderPK = FindMathingOrg(nameAndAddress, OrganisationTypes.Forwarder, unmatchedOrgBuilder);
					if (orgHeaderPK.IsValid)
					{
						newShipment.JS_OH_HandledOnBehalfOfForwarder = orgHeaderPK;
					}
				}
				else
				{
					AddNameAndAddressNote(nameAndAddress, newShipment.Notes);
				}
			}
			if (forwardedManifestMessage.Wrapper.Group143 != null)
			{
				var consolidator = forwardedManifestMessage.Wrapper.Consolidator;
				AddNameAndAddressNote(consolidator, newShipment.Notes);
			}

			var number = newShipment.Numbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = forwardedManifestMessage.CargoControlNumber;
			number = newShipment.Numbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			number.CE_EntryNum = forwardedManifestMessage.PrimaryCargoControlNumber;
			if (!forwardedManifestMessage.Wrapper.UCN.IsNullOrEmpty())
			{
				number = newShipment.Numbers.AddNew();
				number.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
				number.CE_EntryNum = forwardedManifestMessage.Wrapper.UCN;
			}

			newShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			newShipment.JS_HouseBill = forwardedManifestMessage.CargoControlNumber.SubstringSafe(4).TrimStart();
			newShipment.JS_TransportMode = new CusCAeMHTransportModeList().GetCodeFromDescription(forwardedManifestMessage.Wrapper.ModeOfTransport);

			if (!forwardedManifestMessage.Wrapper.B2BComments.IsNullOrEmpty())
			{
				newShipment.Notes.AddNew(true, Res.GetString("b6994cb5-bc26-4f08-92fb-e5cd6a5abae8", "Forwarded Manifest B2B Notes"), forwardedManifestMessage.Wrapper.B2BComments);
			}
			if (!forwardedManifestMessage.Wrapper.SpecialInstructions.IsNullOrEmpty())
			{
				newShipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, forwardedManifestMessage.Wrapper.SpecialInstructions);
			}
			if (!forwardedManifestMessage.Wrapper.DangerousGoodsSpecialInstructions.IsNullOrEmpty())
			{
				newShipment.Notes.AddNew(true, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, forwardedManifestMessage.Wrapper.DangerousGoodsSpecialInstructions);
			}
			if (forwardedManifestMessage.Wrapper.LOCDestinationSegment != null)
			{
				if (!forwardedManifestMessage.Wrapper.PortOfDestination.IsNullOrEmpty())
				{
					newShipment.JS_RL_NKDestination = CACustomsCodesResolver.MatchingUNLOCO(forwardedManifestMessage.Wrapper.PortOfDestination, loadListConsol.Factory);
				}
			}

			var volume = ZDecimal.Zero;
			if (ZDecimal.TryParse(forwardedManifestMessage.Wrapper.Volume, out volume))
			{
				newShipment.JS_ActualVolume = volume;
				newShipment.JS_UnitOfVolume = CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(forwardedManifestMessage.Wrapper.VolumeUnits, forwardedManifestMessage.Factory);
			}
			var weight = ZDecimal.Zero;
			if (ZDecimal.TryParse(forwardedManifestMessage.Wrapper.CargoWeight, out weight))
			{
				newShipment.JS_ActualWeight = weight;
				newShipment.JS_UnitOfWeight = CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(forwardedManifestMessage.Wrapper.CargoWeightUnits, forwardedManifestMessage.Factory);
			}

			if (newShipment.OuterPackLines.Count == 0)
			{
				newShipment.OuterPackLines.AddNew();
				newShipment.OuterPackLines[0].JL_ActualVolume = newShipment.JS_ActualVolume;
				newShipment.OuterPackLines[0].JL_ActualVolumeUQ = newShipment.JS_UnitOfVolume;
				newShipment.OuterPackLines[0].JL_ActualWeight = newShipment.JS_ActualWeight;
				newShipment.OuterPackLines[0].JL_ActualWeightUQ = newShipment.JS_UnitOfWeight;
			}

			int j = 1;
			foreach (IConsignmentLine line in forwardedManifestMessage.Wrapper.ConsignmentLines)
			{
				ZInt packs = 0;
				ZInt.TryParse(line.CargoQuantity, out packs);
				CFSPackLine packLine;

				if (j == 1)
				{
					newShipment.JS_OuterPacks = packs;
					newShipment.JS_F3_NKPackType = line.CargoUnitOfMeasure;
					newShipment.JS_GoodsDescription = line.CargoDescription;
					newShipment.JS_MarksAndNumbers = line.MarksAndNumbers;

					packLine = newShipment.OuterPackLines[0];
				}
				else
				{
					packLine = newShipment.OuterPackLines.AddNew();
				}

				packLine.JL_PackageCount = packs;
				packLine.JL_F3_NKPackType = line.CargoUnitOfMeasure;
				packLine.JL_Description = line.CargoDescription;
				packLine.JL_MarksAndNumbers = line.MarksAndNumbers;

				j++;
			}

			foreach (IContainerAndSeals containerAndSeal in forwardedManifestMessage.Wrapper.ContainersAndSeals)
			{
				if (!containerAndSeal.ContainerNumber.IsNullOrEmpty())
				{
					var container = loadListConsol.Containers.FirstOrDefault(c => ((CFSContainer)c).JC_ContainerNum == containerAndSeal.ContainerNumber);

					if (container == null)
					{
						foreach (IContainerSeal seal in containerAndSeal.ContainerSeals)
						{
							otherInfoBuilder.Append(Res.GetString("7ffa13e8-419f-4e98-9bdc-affafc8dbc76", "Missing Seal: {0} on container: {1}", seal.SealNumber, containerAndSeal.ContainerNumber));
						}
					}
				}
			}

			if (forwardedManifestMessage.Wrapper.LOCDischargeSegment != null && !forwardedManifestMessage.Wrapper.SubLocationOfDischarge.IsNullOrEmpty())
			{
				var warehouse = CACSubLocation.Load(forwardedManifestMessage.Factory, forwardedManifestMessage.Wrapper.SubLocationOfDischarge).CusCodeList;

				if (warehouse != null)
				{
					newShipment.LocationWhsGuid = warehouse.PK;
				}
				else
				{
					otherInfoBuilder.Append(Res.GetString("17de4341-aa0a-4b74-b2d8-a460c795c506", "Release warehouse(sub-location) code: {0}", forwardedManifestMessage.Wrapper.SubLocationOfDischarge));
				}
			}

			if (!otherInfoBuilder.IsEmpty)
			{
				newShipment.Notes.AddNew(true, Res.GetString("a0c7477d-8a1c-4270-8aaa-e2c3c818be0d", "Forwarded Manifest Other Information"), otherInfoBuilder.ToStringWithNewLineBetweenAppends());
			}
			if (!unmatchedOrgBuilder.IsEmpty)
			{
				newShipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, unmatchedOrgBuilder.ToStringWithNewLineBetweenAppends());
			}

			loadListConsol.Shipments.Add(newShipment);
			return newShipment;
		}
	}
}
