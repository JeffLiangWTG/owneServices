using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TNT.NZ;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT
{
	internal class QuantumShipmentRecord : ConsignmentRecord
	{
		internal static QuantumShipmentRecord New(ZString branchCode, ZString mBagNo, ZString line)
		{
			QuantumShipmentRecord result = null;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				result = new QuantumShipmentRecord(branchCode, mBagNo, line);
			}
			else
			{
				result = new NZQuantumShipmentRecord(branchCode, mBagNo, line);
			}
			return result;
		}

		internal QuantumShipmentRecord(ZString branchCode, ZString mBagNo, ZString line)
			: base(line)
		{
			this.BranchCode = branchCode;
			this.MBagNo = mBagNo;
		}

		public readonly ZString BranchCode;
		public readonly ZString MBagNo;

		#region Field Property Override

		public override ZString HouseBill
		{
			get { return base.HouseBill.Trim(); }
		}

		public override ZString Origin
		{
			get { return base.Origin.Trim(); }
		}

		public override ZString Destination
		{
			get { return base.Destination.Trim(); }
		}

		public ZString OriginDestinationAndBranch_ToCartageWaybill
		{
			get { return Origin.PadRight(3) + "-" + Destination.PadRight(3) + "-" + BranchCode.PadRight(3); }
		}

		#region Consignor

		public override ZString ConsignorLegacyCode
		{
			get { return base.ConsignorLegacyCode.KeepChars("1234567890").Left(OrgCusCode.Schema.OK_CustomsRegNoMaxLength); }
		}

		public override ZString ConsignorName
		{
			get { return base.ConsignorName.Trim().Left(OrgHeader.Schema.OH_FullNameMaxLength); }
		}

		public override ZString ConsignorAddress1
		{
			get { return base.ConsignorAddress1.Trim().Left(OrgAddress.Schema.OA_Address1MaxLength); }
		}

		public override ZString ConsignorAddress2
		{
			get { return base.ConsignorAddress2.Trim().Left(OrgAddress.Schema.OA_Address2MaxLength); }
		}

		public override ZString ConsignorCity
		{
			get { return base.ConsignorCity.Trim().Left(OrgAddress.Schema.OA_CityMaxLength); }
		}

		public override ZString ConsignorState
		{
			get { return base.ConsignorState.Trim().Left(OrgAddress.Schema.OA_StateMaxLength); }
		}

		public override ZString ConsignorPostCode
		{
			get { return base.ConsignorPostCode.Trim().Left(OrgAddress.Schema.OA_PostCodeMaxLength); }
		}

		public override ZString ConsignorCountry
		{
			get { return base.ConsignorCountry.Trim(); }
		}

		public override ZString ConsignorPhone
		{
			get { return base.ConsignorPhone.Trim().Left(OrgAddress.Schema.OA_PhoneMaxLength); }
		}

		public override ZString ConsignorContactName
		{
			get { return base.ConsignorContactName.Trim().Left(OrgContact.Schema.OC_ContactNameMaxLength); }
		}

		public override ZString ConsignorContactPhone
		{
			get { return base.ConsignorContactPhone.Trim().Left(OrgContact.Schema.OC_PhoneMaxLength); }
		}

		#endregion

		#region Pickup

		public override ZString PickupName
		{
			get { return base.PickupName.Trim().Left(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength); }
		}

		public override ZString PickupAddress1
		{
			get { return base.PickupAddress1.Trim().Left(OrgAddress.Schema.OA_Address1MaxLength); }
		}

		public override ZString PickupAddress2
		{
			get { return base.PickupAddress2.Trim().Left(OrgAddress.Schema.OA_Address2MaxLength); }
		}

		public override ZString PickupCity
		{
			get { return base.PickupCity.Trim().Left(OrgAddress.Schema.OA_CityMaxLength); }
		}

		public override ZString PickupState
		{
			get { return base.PickupState.Trim().Left(OrgAddress.Schema.OA_StateMaxLength); }
		}

		public override ZString PickupPostCode
		{
			get { return base.PickupPostCode.Trim().Left(OrgAddress.Schema.OA_PostCodeMaxLength); }
		}

		public override ZString PickupCountry
		{
			get { return base.PickupCountry.Trim(); }
		}

		public override ZString PickupPhone
		{
			get { return base.PickupPhone.Trim().Left(OrgAddress.Schema.OA_PhoneMaxLength); }
		}

		public override ZString PickupContactName
		{
			get { return base.PickupContactName.Trim().Left(OrgContact.Schema.OC_ContactNameMaxLength); }
		}

		public override ZString PickupContactPhone
		{
			get { return base.PickupContactPhone.Trim().Left(OrgContact.Schema.OC_PhoneMaxLength); }
		}

		#endregion

		#region Consignee

		public override ZString ConsigneeName
		{
			get { return base.ConsigneeName.Trim().Left(OrgHeader.Schema.OH_FullNameMaxLength); }
		}

		public override ZString ConsigneeAddress1
		{
			get { return base.ConsigneeAddress1.Trim().Left(OrgAddress.Schema.OA_Address1MaxLength); }
		}

		public override ZString ConsigneeAddress2
		{
			get { return base.ConsigneeAddress2.Trim().Left(OrgAddress.Schema.OA_Address2MaxLength); }
		}

		public override ZString ConsigneeCity
		{
			get { return base.ConsigneeCity.Trim().Left(OrgAddress.Schema.OA_CityMaxLength); }
		}

		public override ZString ConsigneeState
		{
			get { return base.ConsigneeState.Trim().Left(OrgAddress.Schema.OA_StateMaxLength); }
		}

		public override ZString ConsigneePostCode
		{
			get { return base.ConsigneePostCode.Trim().Left(OrgAddress.Schema.OA_PostCodeMaxLength); }
		}

		public override ZString ConsigneeCountry
		{
			get { return base.ConsigneeCountry.Trim(); }
		}

		public override ZString ConsigneePhone
		{
			get { return base.ConsigneePhone.Trim().Left(OrgAddress.Schema.OA_PhoneMaxLength); }
		}

		public override ZString ConsigneeFax
		{
			get { return base.ConsigneeFax.Trim().Left(OrgAddress.Schema.OA_FaxMaxLength); }
		}

		public override ZString ConsigneeTelex
		{
			get { return base.ConsigneeTelex.Trim().Left(OrgAddress.Schema.OA_PhoneMaxLength); }
		}

		public override ZString ConsigneeContactPhone
		{
			get { return base.ConsigneeContactPhone.Trim().Left(OrgContact.Schema.OC_PhoneMaxLength); }
		}

		public override ZString ConsigneeContactName
		{
			get { return base.ConsigneeContactName.Trim().Left(OrgContact.Schema.OC_ContactNameMaxLength); }
		}

		#endregion

		#region Delivery

		public override ZString DeliveryName
		{
			get { return base.DeliveryName.Trim().Left(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength); }
		}

		public override ZString DeliveryAddress1
		{
			get { return base.DeliveryAddress1.Trim().Left(OrgAddress.Schema.OA_Address1MaxLength); }
		}

		public override ZString DeliveryAddress2
		{
			get { return base.DeliveryAddress2.Trim().Left(OrgAddress.Schema.OA_Address2MaxLength); }
		}

		public override ZString DeliveryCity
		{
			get { return base.DeliveryCity.Trim().Left(OrgAddress.Schema.OA_CityMaxLength); }
		}

		public override ZString DeliveryState
		{
			get { return base.DeliveryState.Trim().Left(OrgAddress.Schema.OA_StateMaxLength); }
		}

		public override ZString DeliveryPostCode
		{
			get { return base.DeliveryPostCode.Trim().Left(OrgAddress.Schema.OA_PostCodeMaxLength); }
		}

		public override ZString DeliveryCountry
		{
			get { return base.DeliveryCountry.Trim(); }
		}

		public override ZString DeliveryPhone
		{
			get { return base.DeliveryPhone.Trim().Left(OrgAddress.Schema.OA_PhoneMaxLength); }
		}

		public override ZString DeliveryContactName
		{
			get { return base.DeliveryContactName.Trim().Left(OrgContact.Schema.OC_ContactNameMaxLength); }
		}

		public override ZString DeliveryContactPhone
		{
			get { return base.DeliveryContactPhone.Trim().Left(OrgContact.Schema.OC_PhoneMaxLength); }
		}

		#endregion

		public bool IsSubHousebill
		{
			get { return (DocumentIndicator.Trim().ToUpper() == "D"); }
		}

		public override ZString TDoc
		{
			get { return base.TDoc.Trim(); }
		}

		public ZString ECN
		{
			get { return TDoc; }  // The same field is used to provide both values
		}

		public ZString IncoTerm
		{
			get { return Core.Constants.IncoTerms.FreeOnBoard; }
		}

		#endregion

		public ForwardingShipment FindFirstMatchingShipment(ForwardingConsol consol)
		{
			return FindFirstMatchingShipmentCore(consol);
		}

		protected virtual ForwardingShipment FindFirstMatchingShipmentCore(ForwardingConsol consol)
		{
			ForwardingShipment result = null;

			ZQuery shipmentFilter = new ZQuery(JobShipmentSchema.JS_HouseBill, HouseBill);
			shipmentFilter.AddToFilter(new ZQuery(JobShipmentSchema.JS_CartageWaybill, SQLComparisonOperator.StartsWith, OriginDestinationAndBranch_ToCartageWaybill.SubstringSafe(0, 7)));
			ForwardingShipment[] forwardingShipments = consol.Factory.Load<ForwardingShipment>(shipmentFilter);

			foreach (ForwardingShipment shipment in forwardingShipments)
			{
				if (PackageCount == shipment.JS_OuterPacks)
				{
					result = shipment;
					break;
				}
			}

			return result;
		}

		#region Create Shipment

		public ForwardingShipment CreateShipment(BusinessObjectFactory factory, bool createDeclaration, INotifications notify)
		{
			return CreateShipmentCore(factory, createDeclaration, notify);
		}

		protected virtual ForwardingShipment CreateShipmentCore(BusinessObjectFactory factory, bool createDeclaration, INotifications notify)
		{
			ForwardingShipment shipment = factory.New<ForwardingShipment>();
			((ISupportDataImporting)shipment).IsImportingData = true;
			try
			{
				MapConsigneeConsignor(shipment, factory, notify);

				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
				shipment.JS_E_DEP = ZDateTime.Today;

				Mapper.SetPropertyInfoValue(shipment.JS_HouseBillInfo, HouseBill, ForeignKeyType.None, notify);
				Mapper.SetPropertyInfoValue(shipment.JS_CartageWaybillInfo, OriginDestinationAndBranch_ToCartageWaybill, ForeignKeyType.None, notify);
				Mapper.SetPropertyInfoValue(shipment.JS_RL_NKOriginInfo, Origin, ForeignKeyType.PortNK, notify);
				Mapper.SetPropertyInfoValue(shipment.JS_RL_NKDestinationInfo, Destination, ForeignKeyType.PortNK, notify);
				Mapper.SetPropertyInfoValue(shipment.JS_INCOInfo, IncoTerm, ForeignKeyType.None, notify);
				shipment.JS_GoodsValue = GoodsValue;
				Mapper.SetPropertyInfoValue(shipment.JS_RX_NKGoodsValueCurrInfo, GoodsCurrency, ForeignKeyType.CurrencyNK, notify);
				shipment.JS_OuterPacks = PackageCount;
				Mapper.SetPropertyInfoValue(shipment.JS_F3_NKPackTypeInfo, Core.Constants.PkgUnit.Piece, ForeignKeyType.None, notify);
				shipment.JS_ActualWeight = Weight;
				Mapper.SetPropertyInfoValue(shipment.JS_UnitOfWeightInfo, Core.Constants.Weight.Kilograms, ForeignKeyType.None, notify);

				SetShipmentCustomsEntryNumber(shipment);
				if (createDeclaration)
				{
					// Last thing to do, as it copies a whole bunch of properties from the shipment
					CreateNewDeclarationForShipment(shipment);
				}
			}
			finally
			{
				((ISupportDataImporting)shipment).IsImportingData = false;
			}

			return shipment;
		}

		protected virtual void MapConsigneeConsignor(ForwardingShipment shipment, BusinessObjectFactory factory, INotifications notify)
		{
			OrgHeader consignor = GetOrCreateConsignor(factory, notify);
			OrgHeader consignee = GetOrCreateConsignee(factory, notify);

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			SetNotifyPartyContact(shipment, consignee);
			Mapper.SetPropertyInfoValue(shipment.ConsignorDocumentaryAddress.E2_ContactInfo, ConsignorContactName, ForeignKeyType.None, notify);
			Mapper.SetPropertyInfoValue(shipment.ConsigneeDocumentaryAddress.E2_ContactInfo, ConsigneeContactName, ForeignKeyType.None, notify);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = DeliveryAddressPK;
		}

		protected TNTStringToBusinessObjectFieldConverter Mapper = TNTStringToBusinessObjectFieldConverter.Instance;

		protected void SetNotifyPartyContact(CommonShipment shipment, OrgHeader consignee)
		{
			if (!ConsigneeContactName.IsEmpty)
			{
				OrgContact notifyParty = GetOrgContact(consignee, shipment.Factory);
				if (notifyParty != null)
				{
					shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.OC_OH;
					shipment.NotifyPartyDocumentaryAddress.E2_Contact = notifyParty.OC_ContactName;
				}
			}
		}

		internal virtual void SetShipmentCustomsEntryNumber(ForwardingShipment shipment)
		{
			if (TDoc.Length == 3 || TDoc.Length == 4)
			{
				if (TDoc.Length == 3)
				{
					shipment.CustomsEntryNumberType = TDoc;
				}
				else if (TDoc.Length == 4)
				{
					shipment.CustomsEntryNumberType = GetEntryTypeCode(TDoc);
				}

				if (!TDoc.StartsWith("EX"))
				{
					shipment.CustomsEntryNumber = ECN;
				}
			}
			else if (TDoc.Length == 0)
			{
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.CountrySpecificDefaultEntryNumberType("AU", shipment.IsImport());

				if (shipment.IsCrossTrade())
				{
					CusHAWB cusHawb = shipment.Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, HouseBill));
					if (cusHawb != null)
					{
						shipment.CustomsEntryNumber = cusHawb.CS_TranshipmentEntryNum;
					}
				}
			}
			else if (TDoc.Length > 4)
			{
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.CountrySpecificDefaultEntryNumberType("AU", shipment.IsImport());
				shipment.CustomsEntryNumber = TDoc;
			}
		}

		string GetEntryTypeCode(string entryType)
		{
			string entryTypeCode = "";
			switch (entryType)
			{
				case "EXDC":
					entryTypeCode = "XDC";
					break;
				case "EXDD":
					entryTypeCode = "XDD";
					break;
				case "EXLV":
					entryTypeCode = "XLV";
					break;
				case "EXML":
					entryTypeCode = "XML";
					break;
				case "EXPE":
					entryTypeCode = "XPE";
					break;
				case "EXSP":
					entryTypeCode = "XSP";
					break;
				case "EXTI":
					entryTypeCode = "XTI";
					break;
				default:
					entryTypeCode = "UNK";  // unknown entry type
					break;
			}

			return entryTypeCode;
		}

		/// <summary>
		/// Create a declaration for a newly created shipment
		/// </summary>
		protected virtual void CreateNewDeclarationForShipment(ForwardingShipment shipment)
		{
			if (!shipment.CustomsEntryNumberType.StartsWith("EX"))
			{
				JobDeclaration declaration = (JobDeclaration)shipment.Factory.New(typeof(JobDeclaration));
				declaration.JE_JS = shipment.PK;
				JobDeclarationSynchroniser plugInSynchroniser = new JobDeclarationSynchroniser(declaration);
				plugInSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));

				declaration.IsImportingData = true;

				declaration.JE_JS = shipment.PK;
				if (!declaration.JE_DateAtOrigin.IsValid)
				{
					declaration.JE_DateAtOrigin = ZDateTime.Today;
				}

				declaration.JE_ExportDate = declaration.JE_DateAtOrigin;

				// TODO: See why PlugInSynchroniser is copying the consignor/consignee ports to these fields
				declaration.JE_RL_NKOrigin = shipment.JS_RL_NKOrigin;
				declaration.JE_RL_NKFinalDestination = shipment.JS_RL_NKDestination;
				declaration.JE_RL_NKPortOfLoading = declaration.JE_RL_NKOrigin;
				declaration.JE_RL_NKPortOfArrival = declaration.JE_RL_NKFinalDestination;
				declaration.JE_RL_NKPortOfFirstArrival = ZString.Empty;
				GlbBranch decBranch = (GlbBranch)shipment.Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, BranchCode);
				declaration.JE_GB = decBranch != null ? decBranch.PK : ZGuid.Empty;

				declaration = (JobDeclaration)CreateDefaultInvoiceForDeclaration(shipment, declaration);
			}
		}

		/// <summary>
		/// Create a default invoice and invoice line for the declaration
		/// <summary>

		internal

 Customs.Business.BaseJobDeclaration CreateDefaultInvoiceForDeclaration(ForwardingShipment shipment, Customs.Business.BaseJobDeclaration createdDec)
		{
			Customs.Business.BaseJobComInvoiceHeader defaultInv = createdDec.Invoices.AddNew();
			defaultInv.JZ_InvoiceNumber = "1";
			defaultInv.JZ_OH_Supplier = shipment.ConsignorPK;
			defaultInv.JZ_RX_NKInvoice_Currency = shipment.Consignor.MiscServ.OM_RX_NKFWDefCurrency;
			defaultInv.JZ_InvoiceAmount = shipment.JS_GoodsValue;
			if (shipment.GoodsValueCurr != null)
			{
				defaultInv.JZ_RX_NKInvoice_Currency = shipment.GoodsValueCurr.RX_Code;
			}
			defaultInv.JZ_IncoTerm = shipment.JS_INCO;
			defaultInv.JZ_Weight = shipment.JS_ActualWeight;
			defaultInv.JZ_WeightUQ = shipment.JS_UnitOfWeight;
			defaultInv = CreateDefaultInvoiceLine(shipment, defaultInv);
			return createdDec;
		}

		/// <summary>
		/// Create a default invoice and invoice line for the declaration
		/// <summary>
		protected Customs.Business.BaseJobComInvoiceHeader CreateDefaultInvoiceLine(ForwardingShipment shipment, Customs.Business.BaseJobComInvoiceHeader createdInv)
		{
			Customs.Business.BaseJobComInvoiceLine defaultInvLine = createdInv.JobComInvoiceLines.AddNew();
			defaultInvLine.JI_Calc_Invoice = createdInv.JZ_InvoiceNumber;
			defaultInvLine.JI_LineNo = 1;
			defaultInvLine.JI_InvoiceQuantity = Convert.ToDecimal(shipment.JS_OuterPacks);
			defaultInvLine.JI_InvoiceUQ = shipment.JS_F3_NKPackType;
			defaultInvLine.JI_LinePrice = shipment.JS_GoodsValue;
			defaultInvLine.JI_Weight = shipment.JS_ActualWeight;
			defaultInvLine.JI_WeightUQ = shipment.JS_UnitOfWeight;
			return createdInv;
		}

		#endregion

		#region CreateAndSaveShipment

		public ForwardingShipment CreateAndSaveShipment(BusinessObjectFactory factory, INotifications notify, int percentComplete, bool createDeclaration)
		{
			return CreateShipment(factory, createDeclaration, notify);
		}

		#endregion

		#region Implementation

		#region Consignor Matching/Creation

		protected virtual OrgHeader GetConsignorByLegacyCode(BusinessObjectFactory factory, INotifications notify, TemporaryOrganisationCreator temporaryCreator)
		{
			OrgHeader result = null;

			ZQuery regNumberFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			regNumberFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, ConsignorLegacyCode);
			OrgCusCode[] matchingCustomCodes = (OrgCusCode[])factory.Load(typeof(OrgCusCode), regNumberFilter);

			if (matchingCustomCodes.Length > 0)
			{
				OrgHeader matchingHeader = OrgHeader.Find(
					factory, ConsignorName, temporaryCreator.GetPortCode(ConsignorCountry, ConsignorState, ConsignorCity), ConsignorAddress1);

				if (matchingHeader != null)
				{
					// Check if the MatchingHeader matches one of the LegacySystemCode
					foreach (OrgCusCode matchingCustomCode in matchingCustomCodes)
					{
						if (matchingHeader.PK == matchingCustomCode.Header.PK)
						{
							result = matchingHeader;
							break;
						}
					}
				}

				// If no match, add Consignor as a contact of a LegacySystemCode-Organisation
				if (result == null)
				{
					result = matchingCustomCodes[0].Header;
					AddConsignorAsAnOrganisationContact(result);
				}
			}
			return result;
		}

		protected OrgHeader GetOrCreateConsignor(BusinessObjectFactory factory, INotifications notify)
		{
			OrgHeader result = null;
			TemporaryOrganisationCreator temporaryCreator = new TemporaryOrganisationCreator(factory);

			if (!ConsignorLegacyCode.IsEmpty)
			{
				result = GetConsignorByLegacyCode(factory, notify, temporaryCreator);
			}

			if (result == null)
			{
				OrgHeader temporaryOrg = temporaryCreator.CreateConsignor(TNTOrganisation.Consignor(this), notify);
				result = GetMatchedOrganisation(temporaryOrg, factory);
			}

			result.OH_IsConsignor = true;
			UpdateConsignorContact(result, factory);
			return result;
		}

		OrgHeader GetMatchedOrganisation(OrgHeader temporaryOrg, BusinessObjectFactory factory)
		{
			OrgHeader result = temporaryOrg;

			if (temporaryOrg != null)
			{
				OrgMatcher matcher = GetOrgMatcher(factory);
				OrgMatchResult matchingResult = matcher.MatchToSimilarOrganisations(temporaryOrg);

				if (matchingResult.MatchingOrg != null)
				{
					result = matchingResult.MatchingOrg;
					temporaryOrg.Delete();
				}
			}

			return result;
		}

		protected virtual OrgMatcher GetOrgMatcher(BusinessObjectFactory factory)
		{
			return new OrgMatcher(factory);
		}

		protected void AddConsignorAsAnOrganisationContact(OrgHeader organisation)
		{
			if (!IsDuplicateContact(organisation, ConsignorName))
			{
				OrgContact newContact = organisation.Contacts.AddNew();
				newContact.OC_ContactName = ConsignorName;
				newContact.OC_Phone = ConsignorPhone;
				newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;

				OrgAddress consignorAddress;

				if (organisation.MainAddress.OA_Address1.IsEmpty)
				{
					consignorAddress = organisation.MainAddress;
					consignorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
				}
				else
				{
					consignorAddress = organisation.Addresses.AddNew();
					consignorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Miscellaneous);
				}

				consignorAddress.OA_CompanyNameOverride = ConsignorName;
				consignorAddress.OA_Address1 = ConsignorAddress1;
				consignorAddress.OA_Address2 = ConsignorAddress2;
				consignorAddress.OA_City = ConsignorCity;
				consignorAddress.OA_State = ConsignorState;
				consignorAddress.OA_PostCode = ConsignorPostCode;
				newContact.OC_OA_OrgAddress = consignorAddress.PK;
			}
		}

		protected bool IsDuplicateContact(OrgHeader organisation, ZString contactName)
		{
			bool result = false;
			ZString trimUpperContactName = contactName.Trim().ToUpper();

			for (int i = 0; i < organisation.Contacts.Count; i++)
			{
				if (organisation.Contacts[i].OC_ContactName.Trim().ToUpper() == trimUpperContactName)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected void UpdateConsignorContact(OrgHeader consignor, BusinessObjectFactory factory)
		{
			if (!ConsignorContactName.IsEmpty)
			{
				ZQuery contactFilter = new ZQuery(OrgContactSchema.OC_ContactName, ConsignorContactName);
				contactFilter.AddToFilter(OrgContactSchema.OC_OH, consignor.PK);
				OrgContact organisationContact = (OrgContact)factory.LoadTop1(typeof(OrgContact), contactFilter);
				if (organisationContact == null)
				{
					OrgContact newContact = consignor.Contacts.AddNew();
					newContact.OC_ContactName = ConsignorContactName;
					newContact.OC_Phone = ConsignorContactPhone;
					newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;
					newContact.Documents.AddNew();
					newContact.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
				}
				else
				{
					organisationContact.OC_Phone = ConsignorContactPhone;
				}
			}
		}

		#endregion

		#region Consignee Matching/Creation

		protected OrgHeader GetOrCreateConsignee(BusinessObjectFactory factory, INotifications notify)
		{
			TemporaryOrganisationCreator temporaryCreator = new TemporaryOrganisationCreator(factory);

			OrgHeader temporaryOrg = temporaryCreator.CreateConsignee(TNTOrganisation.Consignee(this), notify);
			OrgHeader result = GetMatchedOrganisation(temporaryOrg, factory);

			UpdateConsigneeContact(result, factory);

			AddDeliveryAddressIfNoMatch(factory, result, notify);

			return result;
		}

		protected OrgContact GetOrgContact(OrgHeader consignee, BusinessObjectFactory factory)
		{
			ZQuery contactFilter = new ZQuery(OrgContactSchema.OC_ContactName, ConsigneeContactName);
			contactFilter.AddToFilter(OrgContactSchema.OC_OH, consignee.PK);
			OrgContact result = (OrgContact)factory.LoadTop1(typeof(OrgContact), contactFilter);
			return result;
		}

		protected void UpdateConsigneeContact(OrgHeader consignee, BusinessObjectFactory factory)
		{
			if (!ConsigneeContactName.IsEmpty)
			{
				OrgContact organisationContact = GetOrgContact(consignee, factory);
				if (organisationContact == null)
				{
					OrgContact newContact = consignee.Contacts.AddNew();
					newContact.OC_ContactName = ConsigneeContactName;
					newContact.OC_Phone = ConsigneeContactPhone;
					newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;
					newContact.Documents.AddNew();
					newContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
				}
				else
				{
					organisationContact.OC_Phone = ConsigneeContactPhone;
				}
			}
		}

		protected void AddDeliveryAddressIfNoMatch(BusinessObjectFactory factory, OrgHeader consignee, INotifications notify)
		{
			DeliveryAddressPK = ZGuid.Empty;
			if (!DeliveryAddress1.IsEmpty)
			{
				ZQuery deliveryAddressFilter = new ZQuery(OrgAddressSchema.OA_OH, consignee.PK);
				deliveryAddressFilter.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, DeliveryName);
				deliveryAddressFilter.AddToFilter(OrgAddressSchema.OA_Address1, DeliveryAddress1);
				OrgAddress[] deliveryAddressesCollection = (OrgAddress[])consignee.Factory.Load(typeof(OrgAddress), deliveryAddressFilter);
				OrgAddressCollection deliveryAddresses = new OrgAddressCollection(factory);
				foreach (OrgAddress address in deliveryAddressesCollection)
				{
					if (address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery))
					{
						deliveryAddresses.Add(address);
					}
				}

				if (deliveryAddresses.Count == 0)
				{
					OrgAddress deliveryAddress;

					if (consignee.MainAddress.OA_Address1.IsEmpty)
					{
						deliveryAddress = consignee.MainAddress;
						deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
					}
					else
					{
						deliveryAddress = consignee.Addresses.AddNew();
						deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
					}

					TemporaryOrganisationCreator temporaryCreator = new TemporaryOrganisationCreator(factory);
					temporaryCreator.UpdateOrgAddress(deliveryAddress, TNTOrganisation.Delivery(this), notify);

					DeliveryAddressPK = deliveryAddress.PK;
				}
				else
				{
					DeliveryAddressPK = deliveryAddresses[0].PK;
				}
			}
		}

		#endregion

		ZGuid DeliveryAddressPK;

#endregion

			}
}
