using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class OrgHeaderSource : DocBaseWrapper, IOrganisationDetails
	{
		OrgHeaderSource(OrgHeader orgHeader, BusinessObjectFactory factory)
			: base(orgHeader, factory)
		{
		}

		public static OrgHeaderSource New(OrgHeader orgHeader, BusinessObjectFactory factoryForWrapper)
		{
			return (orgHeader != null) ? factoryForWrapper.GetCachedValue
				(
					orgHeader.PK.ToStringKey(),
					delegate
					{
						return new OrgHeaderSource(orgHeader, factoryForWrapper);
					},
					CacheStalenessPolicy.StaleOnFactorySave
				) : null;
		}

		protected OrgHeader OrgHeader
		{
			get { return (OrgHeader)WrappedObject; }
		}

		#region Part Attributes

		public ZString PartAttrib1Name
		{
			get { return OrgHeader.PartAttributeManager.PartAttributeName1; }
		}

		public ZString PartAttrib2Name
		{
			get { return OrgHeader.PartAttributeManager.PartAttributeName2; }
		}

		public ZString PartAttrib3Name
		{
			get { return OrgHeader.PartAttributeManager.PartAttributeName3; }
		}

		public ZBool HasPartAttrib1
		{
			get { return OrgHeader.PartAttributeManager.IsPartAttributeUsedByOrganisation(1); }
		}

		public ZBool HasPartAttrib2
		{
			get { return OrgHeader.PartAttributeManager.IsPartAttributeUsedByOrganisation(2); }
		}

		public ZBool HasPartAttrib3
		{
			get { return OrgHeader.PartAttributeManager.IsPartAttributeUsedByOrganisation(3); }
		}

		public ZBool HasSerialNumber
		{
			get { return OrgHeader.PartAttributeManager.IsSerialNumberUsedByOrganisation; }
		}

		#endregion

		#region Contacts

		public DocContactsCollection Contacts
		{
			get
			{
				using (OrgHeader.ToggleShowSystemGeneratedContactsFlagTemporarily(Env.Registry.ShowSystemGeneratedContactsOnOrgDocuments))
				{
					return new DocContactsCollection(OrgHeader.Contacts, Factory);
				}
			}
		}

		public DocContacts SalesContact
		{
			get { return DefaultContact(ContactType.Sales); }
		}

		public DocContacts ExportFowardingContact
		{
			get { return DefaultContact(ContactType.ExportFreightAgent); }
		}

		public DocContacts ImportFowardingContact
		{
			get { return DefaultContact(ContactType.ImportFreightAgent); }
		}

		public DocContacts DefaultContact(ContactType defaultContactType)
		{
			OrgContact contact = new DefaultContactFinder(OrgHeader).DefaultContact(defaultContactType);
			return DocContacts.New(contact, Factory);
		}

		public DocContacts DefaultContact(Guid menuItem, ContactType defaultContactType)
		{
			OrgContact contact = new DefaultContactFinder(OrgHeader).DefaultContact(menuItem, defaultContactType);
			return DocContacts.New(contact, Factory);
		}

		public DocContacts ARContact
		{
			get { return DefaultContact(ContactType.Receivables); }
		}

		#endregion

		#region Addresses

		public DocAddressCollection Addresses
		{
			get { return new DocAddressCollection(OrgHeader.Addresses, Factory); }
		}

		public DocDocAddressCollection DocAddresses
		{
			get { return new DocDocAddressCollection(OrgHeader.Addresses, Factory); }
		}

		public DocAddress MainAddress
		{
			get { return DocAddress.New(OrgHeader.MainAddress, Factory); }
		}

		public DocAddress OrganisationPADPostalAddress
		{
			get
			{
				DocAddress result = null;
				OrgAddressList pADAddresses = OrgHeader.Addresses.AddressesOfType(OrgAddressType.PickupAndDelivery);

				if (pADAddresses.Count > 0)
				{
					result = DocAddress.New(pADAddresses[0], Factory);
				}

				return result;
			}
		}

		#endregion

		#region Cus Code Collection
		public DocCusCodeCollection CustomCodes
		{
			get { return new DocCusCodeCollection(OrgHeader.CustomsCodes, Factory); }
		}
		#endregion

		#region AppointedAgentPorts
		public DocAppointedAgentPortsCollection AppointedAgentPorts
		{
			get { return new DocAppointedAgentPortsCollection(OrgHeader.AppointedAgentPorts, Factory); }
		}
		#endregion

		#region Basic Organisation Details
		public ZString OrgType
		{
			get
			{
				ZString result = "";
				if (IsSalesLead)
				{
					result += Res.GetString("3ea71313-ef72-48ce-a009-d40f900d2a49", "Sales Lead") + ", ";
				}

				if (IsBroker)
				{
					result += Res.GetString("f2332e1b-230d-4050-85f5-c45bf64a1d17", "Broker") + ", ";
				}

				if (IsForwarder)
				{
					result += Res.GetString("432f853f-d2eb-4985-872c-6a8d8ef21820", "Forwarder") + ", ";
				}

				if (IsCompetitor)
				{
					result += Res.GetString("cb26131c-92e5-40d6-9173-4d9e3cc0c2df", "Competitor") + ", ";
				}

				if (IsConsignee)
				{
					result += Res.GetString("8becb049-ae2e-4265-82be-81e58f59cf28", "Consignee") + ", ";
				}

				if (IsConsignor)
				{
					result += Res.GetString("c9d0c02c-e30b-46ad-b972-9c5aef87e3f5", "Consignor") + ", ";
				}

				if (IsCreditor)
				{
					result += Res.GetString("e16d3069-3d96-454d-9b0b-af982a110ec0", "Creditor") + ", ";
				}

				if (IsDebtor)
				{
					result += Res.GetString("36b6cf9d-f437-4525-b955-1aae2a1349a3", "Debtor") + ", ";
				}

				if (IsTransportClient)
				{
					result += Res.GetString("650878a8-c841-43ec-9f35-487bcbb4f016", "Transport Client") + ", ";
				}

				if (result.IsEmpty)
				{
					return "";
				}
				else
				{
					return result.Substring(0, result.Length - 2);
				}
			}
		}

		public ZString[] AllNotes
		{
			get { return GetAllNotesInStringArray(OrgHeader); }
		}

		public ZDateTime CurrentDate
		{
			get { return ZDateTime.Today; }
		}

		public ZString AdditionalAddressInformation
		{
			get { return OrgHeader.MainAddress.OA_AdditionalAddressInformation; }
		}

		public ZString Address1
		{
			get { return OrgHeader.MainAddress.OA_Address1; }
		}

		public ZString Address2
		{
			get { return OrgHeader.MainAddress.OA_Address2; }
		}

		public ZString City
		{
			get { return OrgHeader.MainAddress.OA_City; }
		}

		public ZString Code
		{
			get { return OrgHeader.OH_Code; }
		}

		public ZString Email
		{
			get { return OrgHeader.MainAddress.OA_Email; }
		}

		public ZString Fax
		{
			get { return OrgHeader.MainAddress.OA_Fax_Formatted; }
		}

		public ZString BusinessRegNo
		{
			get { return OrgHeader.PrimaryRegistrationNumber.Number; }
		}

		public ZString BusinessRegType
		{
			get { return OrgHeader.PrimaryRegistrationNumber.NumberTypeForDisplay; }
		}

		public ZString Name
		{
			get { return OrgHeader.OH_FullName; }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(OrgHeader.Branch, Factory); }
		}

		public ZBool IsActive
		{
			get { return OrgHeader.OH_IsActive; }
		}

		public ZBool IsAirCTO
		{
			get { return OrgHeader.OH_IsAirCTO; }
		}

		public ZBool IsAirLine
		{
			get { return OrgHeader.OH_IsAirLine; }
		}

		public ZBool IsAirWholesaler
		{
			get { return OrgHeader.OH_IsAirWholesaler; }
		}

		public ZBool IsBroker
		{
			get { return OrgHeader.OH_IsBroker; }
		}

		public ZBool IsCompetitor
		{
			get { return OrgHeader.OH_IsCompetitor; }
		}

		public ZBool IsConsignee
		{
			get { return OrgHeader.OH_IsConsignee; }
		}

		public ZBool IsConsignor
		{
			get { return OrgHeader.OH_IsConsignor; }
		}

		public ZBool IsContainerPark
		{
			get { return OrgHeader.OH_IsContainerYard; }
		}

		public ZBool IsCreditor
		{
			get { return OrgHeader.OH_IsCreditor; }
		}

		public ZBool IsDebtor
		{
			get { return OrgHeader.OH_IsDebtor; }
		}

		public ZBool IsForwarder
		{
			get { return OrgHeader.OH_IsForwarder; }
		}

		public ZBool IsInlandWaterwayProvider
		{
			get { return OrgHeader.OH_IsInlandWaterwayProvider; }
		}

		public ZBool IsLineHaulProvider
		{
			get { return OrgHeader.OH_IsLineHaulProvider; }
		}

		public ZBool IsLocalTransport
		{
			get { return OrgHeader.OH_IsLocalTransport; }
		}

		public ZBool IsMiscFreightServices
		{
			get { return OrgHeader.OH_IsMiscFreightServices; }
		}

		public ZBool IsPackDepot
		{
			get { return OrgHeader.OH_IsPackDepot; }
		}

		public ZBool IsRailProvider
		{
			get { return OrgHeader.OH_IsRailProvider; }
		}

		public ZBool IsSalesLead
		{
			get { return OrgHeader.OH_IsSalesLead; }
		}

		public ZBool IsSeaCTO
		{
			get { return OrgHeader.OH_IsSeaCTO; }
		}

		public ZBool IsSeaWholesaler
		{
			get { return OrgHeader.OH_IsSeaWholesaler; }
		}

		public ZBool IsShippingLine
		{
			get { return OrgHeader.OH_IsShippingLine; }
		}

		public ZBool IsShippingProvider
		{
			get { return OrgHeader.OH_IsShippingProvider; }
		}

		public ZBool IsTempAccount
		{
			get { return OrgHeader.OH_IsTempAccount; }
		}

		public ZBool IsTransportClient
		{
			get { return OrgHeader.OH_IsTransportClient; }
		}

		public ZBool IsUnpackDepot
		{
			get { return OrgHeader.OH_IsUnpackDepot; }
		}

		public ZBool IsWarehouseClient
		{
			get { return OrgHeader.OH_IsWarehouseClient; }
		}

		public ZBool IsUserFlag1
		{
			get { return OrgHeader.OH_IsUserFlag1; }
		}

		public ZBool IsUserFlag2
		{
			get { return OrgHeader.OH_IsUserFlag2; }
		}

		public ZBool IsUserFlag3
		{
			get { return OrgHeader.OH_IsUserFlag3; }
		}

		public ZBool IsUserFlag4
		{
			get { return OrgHeader.OH_IsUserFlag4; }
		}

		public ZBool IsUserFlag5
		{
			get { return OrgHeader.OH_IsUserFlag5; }
		}

		public ZBool IsUserFlag6
		{
			get { return OrgHeader.OH_IsUserFlag6; }
		}

		public ZBool IsUserFlag7
		{
			get { return OrgHeader.OH_IsUserFlag7; }
		}

		public ZBool IsUserFlag8
		{
			get { return OrgHeader.OH_IsUserFlag8; }
		}

		public ZBool IsUserFlag9
		{
			get { return OrgHeader.OH_IsUserFlag9; }
		}

		public ZBool IsUserFlag10
		{
			get { return OrgHeader.OH_IsUserFlag10; }
		}

		public ZBool IsUserFlag11
		{
			get { return OrgHeader.OH_IsUserFlag11; }
		}

		public ZBool IsUserFlag12
		{
			get { return OrgHeader.OH_IsUserFlag12; }
		}

		public ZBool IsUserFlag13
		{
			get { return OrgHeader.OH_IsUserFlag13; }
		}

		public ZBool IsUserFlag14
		{
			get { return OrgHeader.OH_IsUserFlag14; }
		}

		public ZBool IsUserFlag15
		{
			get { return OrgHeader.OH_IsUserFlag15; }
		}

		public ZBool IsUserFlag16
		{
			get { return OrgHeader.OH_IsUserFlag16; }
		}

		public ZBool IsUserFlag17
		{
			get { return OrgHeader.OH_IsUserFlag17; }
		}

		public ZBool IsUserFlag18
		{
			get { return OrgHeader.OH_IsUserFlag18; }
		}

		public ZBool IsUserFlag19
		{
			get { return OrgHeader.OH_IsUserFlag19; }
		}

		public ZBool IsUserFlag20
		{
			get { return OrgHeader.OH_IsUserFlag20; }
		}

		public ZBool IsUserFlag21
		{
			get { return OrgHeader.OH_IsUserFlag21; }
		}

		public ZBool IsUserFlag22
		{
			get { return OrgHeader.OH_IsUserFlag22; }
		}

		public ZBool IsUserFlag23
		{
			get { return OrgHeader.OH_IsUserFlag23; }
		}

		public ZBool IsUserFlag24
		{
			get { return OrgHeader.OH_IsUserFlag24; }
		}

		public ZBool IsUserFlag25
		{
			get { return OrgHeader.OH_IsUserFlag25; }
		}

		public ZBool IsUserFlag26
		{
			get { return OrgHeader.OH_IsUserFlag26; }
		}

		public ZBool IsUserFlag27
		{
			get { return OrgHeader.OH_IsUserFlag27; }
		}

		public ZBool IsUserFlag28
		{
			get { return OrgHeader.OH_IsUserFlag28; }
		}

		public ZBool IsUserFlag29
		{
			get { return OrgHeader.OH_IsUserFlag29; }
		}

		public ZBool IsUserFlag30
		{
			get { return OrgHeader.OH_IsUserFlag30; }
		}

		public ZBool IsUserFlag31
		{
			get { return OrgHeader.OH_IsUserFlag31; }
		}

		public ZBool IsUserFlag32
		{
			get { return OrgHeader.OH_IsUserFlag32; }
		}

		public ZString Mobile
		{
			get { return OrgHeader.MainAddress.OA_Mobile_Formatted; }
		}

		public ZString Phone
		{
			get { return OrgHeader.MainAddress.OA_Phone_Formatted; }
		}

		public ZString PostCode
		{
			get { return OrgHeader.MainAddress.OA_PostCode; }
		}

		public DocUNLOCO Loco
		{
			get { return DocUNLOCO.New(OrgHeader.UNLOCO, Factory); }
		}

		public ZString State
		{
			get { return OrgHeader.MainAddress.OA_State; }
		}

		public ZString Web
		{
			get { return OrgHeader.MainWebURL.PU_URL; }
		}

		public ZString HandlingInstructions
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, OrgHeader); }
		}

		/// <summary>
		/// Will check for notes for TransportMode (include ALL context) first, and if found, only those notes will be returned.
		/// If no notes exist for the TransportMode, as NoteContextFreightMode context, notes will be checked for the ContainerMode as NoteContextFreightMode
		/// </summary>
		/// <param name="transportMode"></param>
		/// <param name="containerMode"></param>
		/// <returns></returns>
		public ZString GetHandlingInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			ZString result = PickupOrDeliveryHandlingInstructions(transportMode);
			if (result.IsEmpty)
			{
				result = PickupOrDeliveryHandlingInstructions(containerMode);
			}
			return result;
		}

		public ZString GetHandlingInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			ZString result = PickupOrDeliveryHandlingInstructions(direction, transportMode);
			if (result.IsEmpty)
			{
				result += PickupOrDeliveryHandlingInstructions(direction, containerMode);
			}
			else
			{
				if (containerMode == Core.Constants.ContainerModes.FCL || containerMode == Core.Constants.ContainerModes.LCL)
				{
					ZString instructionByContainer = PickupOrDeliveryHandlingInstructions(direction, containerMode);
					if (instructionByContainer.Length > 0)
					{
						ZString[] transportNotes = result.Split('\n');
						ZString[] containerNotes = instructionByContainer.Split('\n');
						foreach (string containerTypeNote in containerNotes)
						{
							bool includeThisContainerNote = true;
							foreach (string transportTypeNote in transportNotes)
							{
								if (DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(transportTypeNote, containerTypeNote))
								{
									includeThisContainerNote = false;
									break;
								}
							}

							if (includeThisContainerNote)
							{
								result += "\n";
								result += containerTypeNote;
							}
						}
					}
				}
			}
			return result.TrimEnd('\n');
		}

		public ZString CartageInstructions
		{
			get { return PickupOrDeliveryCartageInstructions(); }
		}

		/// <summary>
		/// Will check for notes for TransportMode (include ALL context) first, and if found, only those notes will be returned.
		/// If no notes exist for the TransportMode, as NoteContextFreightMode context, notes will be checked for the ContainerMode as NoteContextFreightMode
		/// </summary>
		/// <param name="transportMode"></param>
		/// <param name="containerMode"></param>
		/// <returns></returns>
		public ZString GetCartageInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			ZString result = PickupOrDeliveryCartageInstructions(transportMode);
			if (result.IsEmpty)
			{
				result = PickupOrDeliveryCartageInstructions(containerMode);
			}
			return result;
		}

		public ZString GetCartageInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			ZString result = PickupOrDeliveryCartageInstructions(direction, transportMode);
			if (result.IsEmpty)
			{
				result = PickupOrDeliveryCartageInstructions(direction, containerMode);
			}
			else
			{
				if (containerMode == Core.Constants.ContainerModes.FCL || containerMode == Core.Constants.ContainerModes.LCL)
				{
					ZString instructionByContainer = PickupOrDeliveryCartageInstructions(direction, containerMode);
					if (instructionByContainer.Length > 0)
					{
						ZString[] transportNotes = result.Split('\n');
						ZString[] containerNotes = instructionByContainer.Split('\n');
						foreach (string containerTypeNote in containerNotes)
						{
							bool includeThisContainerNote = true;
							foreach (string transportTypeNote in transportNotes)
							{
								if (DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(transportTypeNote, containerTypeNote))
								{
									includeThisContainerNote = false;
									break;
								}
							}

							if (includeThisContainerNote)
							{
								result += "\n";
								result += containerTypeNote;
							}
						}
					}
				}
			}
			return result.TrimEnd('\n');
		}

		public ZString LocalCustomsClientCode
		{
			get { return OrgHeader.LocalCustomsClientCode; }
		}

		public ZString LocalCustomsCarrierCode
		{
			get { return OrgHeader.LocalCustomsCarrierCode; }
		}

		public ZString LocalBusinessRegNo
		{
			get { return OrgHeader.LocalBusinessRegNo; }
		}

		public ZString LocalCustomsSupplierCode
		{
			get { return OrgHeader.LocalCustomsSupplierCode; }
		}

		public ZString LocalRebateUserCode
		{
			get { return OrgHeader.LocalRebateUserCode; }
		}

		public ZString LocalVATCode
		{
			get { return OrgHeader.LocalVATCode; }
		}

		public ZString GetSpecialInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			ZString result = PickupOrDeliverySpecialInstructions(transportMode);
			if (result.IsEmpty)
			{
				result = PickupOrDeliveryHandlingInstructions(containerMode);
			}
			return result;
		}

		public ZString GetSpecialInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			ZString result = PickupOrDeliverySpecialInstructions(direction, transportMode);
			if (result.IsEmpty)
			{
				result += PickupOrDeliverySpecialInstructions(direction, containerMode);
			}
			else
			{
				if (containerMode == Core.Constants.ContainerModes.FCL || containerMode == Core.Constants.ContainerModes.LCL)
				{
					ZString instructionByContainer = PickupOrDeliverySpecialInstructions(direction, containerMode);
					if (instructionByContainer.Length > 0)
					{
						ZString[] transportNotes = result.Split('\n');
						ZString[] containerNotes = instructionByContainer.Split('\n');
						foreach (string containerTypeNote in containerNotes)
						{
							bool includeThisContainerNote = true;
							foreach (string transportTypeNote in transportNotes)
							{
								if (DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(transportTypeNote, containerTypeNote))
								{
									includeThisContainerNote = false;
									break;
								}
							}

							if (includeThisContainerNote)
							{
								result += "\n";
								result += containerTypeNote;
							}
						}
					}
				}
			}
			return result.TrimEnd('\n');
		}

		public DocOrgStaffAssignmentsCollection StaffAssignments
		{
			get { return fStaffAssignments ?? (fStaffAssignments = new DocOrgStaffAssignmentsCollection(OrgHeader.StaffAssignments, Factory)); }
		}
		DocOrgStaffAssignmentsCollection fStaffAssignments;

		#endregion

		#region Custom Labels

		public ZString CustomAttrib1
		{
			get { return OrgHeader.MiscServ.OM_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return OrgHeader.MiscServ.OM_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return OrgHeader.MiscServ.OM_CustomAttrib3; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return OrgHeader.MiscServ.OM_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return OrgHeader.MiscServ.OM_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return OrgHeader.MiscServ.OM_CustomDecimal3; }
		}

		public ZDateTime CustomDate1
		{
			get { return OrgHeader.MiscServ.OM_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return OrgHeader.MiscServ.OM_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return OrgHeader.MiscServ.OM_CustomDate3; }
		}

		public ZBool CustomFlag1
		{
			get { return OrgHeader.MiscServ.OM_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return OrgHeader.MiscServ.OM_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return OrgHeader.MiscServ.OM_CustomFlag3; }
		}

		#endregion

		#region Custom Fields

		public ZString ApprovedExporterCode
		{
			get { return (OrgHeader.CountryData != null ? OrgHeader.CountryData.OV_EXExportPermissionDetails : ZString.Empty); }
		}

		public ZString CarrierMasterBillPrefix
		{
			get { return OrgHeader.MiscServ?.Airline?.RM_EagleAddedAirlinePrefixOrAccountingCode ?? ZString.Empty; }
		}

		public ZString CarrierAirlinePrefix
		{
			get
			{
				ZString result = ZString.Empty;

				if (OrgHeader.MiscServ?.Airline != null)
				{
					result = OrgHeader.MiscServ.Airline.RM_TwoCharacterCode;
				}

				return result;
			}
		}

		public ZString PostalAddress
		{
			get { return GetPostalAddress(); }
		}

		public ZString PostalAddressInEnglish
		{
			get { return GetPostalAddress(true); }
		}

		ZString GetPostalAddress(bool inEnglish = false)
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var docCompany = DocCompany.New(currentCompany, Factory);

			var formatter = new WrapperPostalAddressFormatter(DocOrganisation.New(OrgHeader, Factory), docCompany);
			formatter.EnglishOnly = inEnglish;

			return formatter.PostalAddress();
		}

		public ZString PostalAddressExcludeCountryIfSame
		{
			get
			{
				var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				DocCompany docCompany = DocCompany.New(currentCompany, Factory);
				return new WrapperPostalAddressFormatter(DocOrganisation.New(OrgHeader, Factory), docCompany, false).PostalAddress();
			}
		}

		public ZString PostalAddressExcludeName
		{
			get
			{
				if (PostalAddress.ToString().StartsWith(Name, true, null))
				{
					return PostalAddress.SubstringSafe(Name.Length).TrimStart('\n');
				}
				else
				{
					return PostalAddress;
				}
			}
		}
		public DocCountry Country
		{
			get
			{
				return MainAddress.Country;
			}
		}

		public DocMiscServ MiscServ
		{
			get { return DocMiscServ.New(OrgHeader.MiscServ, Factory); }
		}

		public DocCountryData CountryData
		{
			get { return DocCountryData.New(OrgHeader.CountryData, Factory); }
		}

		public ZString SCAC
		{
			get
			{
				ZString result = "";

				if (OrgHeader != null)
				{
					result = OrgHeader.SCACCode;
				}

				return result;
			}
		}

		public ZString ABN
		{
			get
			{
				RefCountry aUCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
				if (aUCountry != null)
				{
					return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, aUCountry);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString GST
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.GSTCode, GlbCompany.CurrentCompany.Country); }
		}

		public ZString CBR
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.BrokerageRegistration, GlbCompany.CurrentCompany.Country); }
		}

		public ZString CCC
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, GlbCompany.CurrentCompany.Country); }
		}

		public ZString CSC
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.SupplierCode, GlbCompany.CurrentCompany.Country); }
		}

		public ZString CCD
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, GlbCompany.CurrentCompany.Country); }
		}

		public ZString CID
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientID, GlbCompany.CurrentCompany.Country); }
		}

		public ZString LSC
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.LegacySystemCode, GlbCompany.CurrentCompany.Country); }
		}

		public ZString PAN
		{
			get { return OrgHeader.CustomsCodes.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, Core.Constants.CountryCodes.India); }
		}

		public ZString Kennitala
		{
			get
			{
				RefCountry iceland = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Iceland);
				return iceland == null ? ZString.Empty : OrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.Kennitala, iceland);
			}
		}

		public ZString DisbursmentTerms
		{
			get { return OrgHeader.DisbursmentTerms; }
		}

		public ZString ShortDisbursementTerms
		{
			get { return OrgHeader.ShortDisbursementTerms; }
		}

		public ZString ShortInvoiceTerms
		{
			get { return OrgHeader.ShortInvoiceTerms; }
		}

		#endregion

		#region Address Rules

		public DocAddress DeliverAddress
		{
			get { return DocAddress.New(OrgHeader.GetAddressWithFallback(AddressType.DLV), Factory); }
		}

		public DocDocAddress DeliverDocAddress
		{
			get { return DocDocAddress.New(OrgHeader.GetAddressWithFallback(AddressType.DLV), Factory); }
		}

		public DocAddress PickUpAddress
		{
			get { return DocAddress.New(OrgHeader.GetAddressWithFallback(AddressType.PIC), Factory); }
		}

		public DocDocAddress PickUpDocAddress
		{
			get { return DocDocAddress.New(OrgHeader.GetAddressWithFallback(AddressType.PIC), Factory); }
		}

		public DocAddress PostalAddressMain
		{
			get { return DocAddress.New(OrgHeader.GetAddressWithFallback(AddressType.PST), Factory); }
		}

		public DocAddress ARAddress
		{
			get
			{
				return DocAddress.New(OrgHeader.AddressForSendingARDocuments, Factory);
			}
		}

		#endregion

		#region Spliting of Name

		public ZString SplitFullName1
		{
			get
			{
				SplitFullName();
				return fSplitFullName1;
			}
		}

		public ZString SplitFullName2
		{
			get
			{
				SplitFullName();
				return fSplitFullName2;
			}
		}

		protected ZString fSplitFullName1;
		protected ZString fSplitFullName2;

		protected void SplitFullName()
		{
			if (fSplitFullName1.IsEmpty && fSplitFullName2.IsEmpty)
			{
				ZString[] splitFormattedName = WrapTextForAColumn(Name, 25).Split('\n');

				if (splitFormattedName.Length > 0)
				{
					fSplitFullName1 = splitFormattedName[0];
				}

				if (splitFormattedName.Length > 1)
				{
					fSplitFullName2 = splitFormattedName[1];
				}
			}
		}

		#endregion
	}
}
