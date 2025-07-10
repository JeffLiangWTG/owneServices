using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonCartage : DocBaseWrapperWithJobHeader, IDocCartageAdvice, Integration.DocumentWrappers.IDocCommonCartage
	{
		protected DocCommonCartage(CommonCartage commonCartage, BusinessObjectFactory factoryToWrap)
			: base(commonCartage, factoryToWrap)
		{
		}

		public static DocCommonCartage New(CommonCartage commonCartage, BusinessObjectFactory factoryToWrap)
		{
			DocCommonCartage result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(commonCartage, factoryToWrap);
			}
			else if (commonCartage != null)
			{
				result = new DocCommonCartage(commonCartage, factoryToWrap);
			}

			return result;
		}

		protected delegate DocCommonCartage NewDelegate(CommonCartage commonCartage, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public CommonCartage CommonCartage
		{
			get { return (CommonCartage)WrappedObject; }
		}

		public override string ToString()
		{
			return ConsignmentID;
		}

		public ZString Context
		{
			get { return "CARTAGE"; }
		}

		#region Related Objects

		#region BookedMoves

		public DocCommonBookedMoveCollection BookedMoves
		{
			get { return new DocCommonBookedMoveCollection(CommonCartage); }
		}

		#endregion

		#region Transport Legs

		public DocCommonCartageLegCollection TransportLegs
		{
			get { return CartageLegs; }
		}

		#endregion

		#region Cartage

		public DocCommonCartage Cartage
		{
			get { return this; }
		}

		#endregion

		#region Doc Containers

		public DocCommonContainerCollection Containers
		{
			get { return new DocCommonContainerCollection(CommonCartage); }
		}

		#endregion

		#region CartageLegs

		public DocCommonCartageLegCollection CartageLegs
		{
			get
			{
				if (commonCartageLegs == null)
				{
					commonCartageLegs = new DocCommonCartageLegCollection(CommonCartage);
					commonCartageLegs.SortOnPlannedPickupTime();
				}
				return commonCartageLegs;
			}
		}
		DocCommonCartageLegCollection commonCartageLegs;

		#endregion

		#endregion

		public ZString Description
		{
			get { return CommonCartage.JJ_GoodsDescription; }
		}

		#region ZString Fields

		public ZString Address1Type
		{
			get { return CommonCartage.FirstDocAddress != null ? CommonCartage.FirstDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString Address2Type
		{
			get { return CommonCartage.SecondDocAddress != null ? CommonCartage.SecondDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString Address3Type
		{
			get { return CommonCartage.ThirdDocAddress != null ? CommonCartage.ThirdDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString TransportInfo
		{
			get
			{
				ZString result = Voyage;
				if (!CommonCartage.IsAir)
				{
					if (Vessel != null)
					{
						result = Vessel.Code + " / " + Voyage + " / " + Vessel.LloydsNumber;
					}
					else
					{
						result = " / " + Voyage + " / ";
					}
				}
				return result;
			}
		}

		public ZString DropMode
		{
			get
			{
				var result = CommonCartage.JJ_DropMode;
				if (!result.IsEmpty)
				{
					ZString description = CommonCartage.Lookups.DropModes.GetDescriptionFromCode(result);
					if (!description.IsEmpty)
					{
						result += " - " + description;
					}
				}
				return result;
			}
		}

		[DocumentField("Handling Instructions")]
		public ZString HandlingInstructions
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, CommonCartage));
				result.AppendIfNotEmpty(GetNotes(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, CommonCartage));
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString CartageInstructions
		{
			get { return PickupOrDeliveryCartageInstructions(); }
		}

		#endregion

		#region CommonCartage Fields

		public ZDateTime PickupRequiredBy
		{
			get { return CommonCartage.JJ_EstimatedPickup; }
		}

		public ZString ContainerLine
		{
			get
			{
				ZString result = ZString.Empty;
				int count = 0;

				foreach (CommonContainer container in CommonCartage.Containers)
				{
					if (!container.JC_ContainerNum.IsEmpty)
					{
						count++;

						if (count > MaximumContainersOnALine)
						{
							result = result.TrimEndIncludingWhiteSpace(',') + " ...";
							break;
						}

						result += container.JC_ContainerNum + ", ";
					}
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		const int MaximumContainersOnALine = 7;

		public ZString TransportHeading
		{
			get { return (CommonCartage.IsAir) ? Res.GetString("77592f8e-caf4-46a1-ae21-ffcf6ec4d908", "FLIGHT NO") : Res.GetString("af4f2a25-b737-424f-a578-96cd13cda081", "VESSEL / VOYAGE / IMO(Lloyds)"); }
		}

		public ZInt TopLevelLegs
		{
			get
			{
				var cartageType = CommonCartage.CartageType;
				return cartageType != null ? cartageType.AllCartageLegTypes.Count : 1;
			}
		}

		public ZBool HasContainers
		{
			get { return CommonCartage.Containers.Any(); }
		}

		public ZBool IsContainerised
		{
			get { return CommonCartage.IsContainerised; }
		}

		public ZDateTime ActualJCL
		{
			get { return CommonCartage.JJ_A_JCL; }
		}

		public ZDateTime AvailFrom
		{
			get { return CommonCartage.FCLAvailabilityDate; }
		}

		public ZDateTime AvailTo
		{
			get { return CommonCartage.FCLStorageDate; }
		}

		public ZString CartageType
		{
			get { return CommonCartage.JJ_E3_NKJobType; }
		}

		public ZString ConsignmentID
		{
			get { return CommonCartage.JJ_ConsignmentID; }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(CommonCartage.Branch, Factory); }
		}

		public ZString GoodsDescription
		{
			get { return CommonCartage.JJ_GoodsDescription; }
		}

		public override DocJobHeader JobHeader
		{
			get
			{
				ZQuery query = new ZQuery(JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, CommonCartage.PK);
				return DocJobHeader.New(Factory.LoadTop1<JobHeader>(query), Factory);
			}
		}

		public DocDocAddress FirstAddress
		{
			get { return DocDocAddress.New(CommonCartage.FirstDocAddress, Factory); }
		}

		public DocDocAddress SecondAddress
		{
			get { return DocDocAddress.New(CommonCartage.SecondDocAddress, Factory); }
		}

		public DocDocAddress ThirdAddress
		{
			get { return DocDocAddress.New(CommonCartage.ThirdDocAddress, Factory); }
		}

		public DocDocAddress FourthAddress
		{
			get { return DocDocAddress.New(CommonCartage.FourthDocAddress, Factory); }
		}

		public DocOrganisation ClientID
		{
			get { return DocOrganisation.New(CommonCartage.LocalClientAddress, Factory); }
		}

		public ZString FirstAddressHeading
		{
			get { return CommonCartage.FirstDocAddress != null ? CommonCartage.FirstDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString SecondAddressHeading
		{
			get { return CommonCartage.SecondDocAddress != null ? CommonCartage.SecondDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString ThirdAddressHeading
		{
			get { return CommonCartage.ThirdDocAddress != null ? CommonCartage.ThirdDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString FourthAddressHeading
		{
			get { return CommonCartage.FourthDocAddress != null ? CommonCartage.FourthDocAddress.AddressCaption : ZString.Empty; }
		}

		public ZString OrderReferenceNumber
		{
			get { return CommonCartage.JJ_OrderReferenceNumber; }
		}

		public ZInt OuterPacks
		{
			get { return CommonCartage.JJ_OuterPacks; }
		}

		public ZString OuterPacksType
		{
			get { return CommonCartage.JJ_F3_NKPackType; }
		}

		public ZString QuoteNumber
		{
			get { return CommonCartage.JJ_QuoteNumber; }
		}

		public DocVessel Vessel
		{
			get { return DocVessel.New(CommonCartage.Factory, CommonCartage.Vessel); }
		}

		public ZString VesselName
		{
			get { return CommonCartage.Vessel; }
		}

		public ZDecimal Volume
		{
			get { return CommonCartage.JJ_Volume; }
		}

		public ZString VolumeUQ
		{
			get { return CommonCartage.JJ_VolumeUQ; }
		}

		public ZString Voyage
		{
			get { return CommonCartage.VoyageFlight; }
		}

		public ZString WaybillNumber
		{
			get { return CommonCartage.JJ_WaybillNumber; }
		}

		public ZDecimal Weight
		{
			get { return CommonCartage.GrossWeight > 0m ? CommonCartage.GrossWeight : CommonCartage.JJ_Weight; }
		}

		public ZString WeightUQ
		{
			get { return CommonCartage.JJ_WeightUQ; }
		}

		public ZString WeightHeading
		{
			get { return CommonCartage.IsContainerised ? Res.GetString("37020b61-f4ef-4d4e-b656-1a14c44cc019", "GROSS WEIGHT") : Res.GetString("b71713fd-19ac-4ca8-8fd4-19be9134aa6e", "GOODS WEIGHT"); }
		}

		#endregion

		#region CoverSheet Fields

		#region Sailing

		public ZString SailingDatesHeading
		{
			get
			{
				string result = "";
				if (Sailing != null)
				{
					if (IsExport)
					{
						result = (HasContainers ? Res.GetString("add39834-5bd0-4772-937d-8bf344da929b", "FCL") : Res.GetString("c1aaf49c-2451-4b58-ae2f-c8bf236db5b8", "LCL")) + " " + Res.GetString("161fde0e-2fb6-46fa-8650-22dc1f058d76", "RECEIVING");
					}
					else if (IsImport)
					{
						result = (HasContainers ? Res.GetString("add39834-5bd0-4772-937d-8bf344da929b", "FCL") : Res.GetString("c1aaf49c-2451-4b58-ae2f-c8bf236db5b8", "LCL")) + " " + Res.GetString("db3a6916-edc7-47fc-8f51-df55983c7d2b", "PICKUP");
					}
				}
				return result;
			}
		}

		public ZString SailingPortOfLoading
		{
			get { return CommonCartage.PortOfLoading; }
		}

		public ZString SailingPortOfArrival
		{
			get { return CommonCartage.PortOfDischarge; }
		}

		public ZString SailingETA
		{
			get { return CommonCartage.E_ARV.IsEmpty ? ZString.Empty : (ZString)CommonCartage.E_ARV.ToShortDateString(); }
		}

		public ZString SailingETD
		{
			get { return CommonCartage.E_DEP.IsEmpty ? ZString.Empty : (ZString)CommonCartage.E_DEP.ToShortDateString(); }
		}

		ZDateTime SailingReceivalPickupDateToAsDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Sailing != null)
				{
					if (IsExport)
					{
						result = (CommonCartage.IsContainerised) ? Sailing.FCLCutOff : Sailing.LCLCutOff;
					}
					else if (IsImport)
					{
						result = (CommonCartage.IsContainerised) ? Sailing.StorageDate : Sailing.LCLStorageDate;
					}
				}
				else if (IsImport && CommonCartage.IsContainerised)
				{
					result = CommonCartage.FCLStorageDate;
				}
				return result;
			}
		}

		public ZString SailingReceivalPickupDateToHeading
		{
			get
			{
				ZString result = "";

				if (Sailing != null)
				{
					if (IsExport)
					{
						result = (CommonCartage.IsContainerised) ? Res.GetString("09fb0359-acd3-46ec-83eb-6c86c3ccfa36", "CTO CUTOFF") : Res.GetString("c473f5b0-cbba-4b66-ae2c-55817bd73603", "CFS CUTOFF");
					}
					else if (IsImport)
					{
						result = (CommonCartage.IsContainerised) ? Res.GetString("3d184dd8-a194-4f5c-9974-de358c77e474", "CTO STOR. STARTS") : Res.GetString("5af0a151-0453-4a9d-b81e-662a7699270f", "CFS STOR. STARTS");
					}
				}

				return result;
			}
		}

		public ZString SailingReceivalPickupDateTo
		{
			get
			{
				ZDateTime result = SailingReceivalPickupDateToAsDate;
				return (!result.IsValid) ? "" : result.ToShortDateString();
			}
		}

		public ZString SailingReceivalPickupDateToDT
		{
			get
			{
				ZDateTime result = SailingReceivalPickupDateToAsDate;
				return (!result.IsValid) ? "" : result.ToString("d-MMM-yy HH:mm");
			}
		}

		ZDateTime SailingReceivalPickupDateFromAsDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Sailing != null)
				{
					if (IsExport)
					{
						result = (CommonCartage.IsContainerised) ? Sailing.FCLReceivalCommences : Sailing.LCLReceivalCommences;
					}
					else if (IsImport)
					{
						result = (CommonCartage.IsContainerised) ? Sailing.AvailabilityDate : Sailing.LCLAvailabilityDate;
					}
				}
				else if (IsImport && CommonCartage.IsContainerised)
				{
					result = CommonCartage.FCLAvailabilityDate;
				}
				return result;
			}
		}

		public ZString SailingReceivalPickupDateFromHeading
		{
			get
			{
				ZString result = "";

				if (Sailing != null)
				{
					if (IsExport)
					{
						result = (CommonCartage.IsContainerised) ? Res.GetString("ca10c7af-43b6-4afe-8b70-8576f43caeb9", "CTO RCV. STARTS") : Res.GetString("2e9c3b59-87c4-4570-9614-218c8c8eb74a", "CFS RCV. STARTS");
					}
					else if (IsImport)
					{
						result = (CommonCartage.IsContainerised) ? Res.GetString("0e5dce6f-8b09-4dab-bdd5-8965f161afeb", "CTO AVAILABLE") : Res.GetString("1c8aa605-ecaf-46d2-9cb6-cdc8cdada9e3", "CFS AVAILABLE");
					}
				}

				return result;
			}
		}

		public ZString SailingReceivalPickupDateFrom
		{
			get
			{
				ZDateTime result = SailingReceivalPickupDateFromAsDate;
				return (!result.IsValid) ? "" : result.ToShortDateString();
			}
		}

		public ZString SailingReceivalPickupDateFromDT
		{
			get
			{
				ZDateTime result = SailingReceivalPickupDateFromAsDate;
				return (!result.IsValid) ? "" : result.ToString("d-MMM-yy HH:mm");
			}
		}

		public DocSailing Sailing
		{
			get
			{
				if (fDocSailing == null)
				{
					fDocSailing = DocSailing.New(CommonCartage.SailingStandalone, Factory);
				}

				return fDocSailing;
			}
		}
		DocSailing fDocSailing;

		#endregion

		public ZBool IsExport
		{
			get { return CommonCartage.IsExportOrOrigin; }
		}

		public ZBool IsImport
		{
			get { return CommonCartage.IsImportOrDestination; }
		}

		public ZBool IsAir
		{
			get { return CommonCartage.IsAir; }
		}

		public ZBool IsSea
		{
			get { return CommonCartage.IsSea; }
		}

		public ZBool HasSailingDetails
		{
			get { return !CommonCartage.JJ_JX_Sailing.IsEmpty; }
		}

		public ZBool HasSailingDetailsOrDeclarationAsParent
		{
			get { return (CommonCartage.Vessel != "" && CommonCartage.VoyageFlight != "") ? ZBool.True : HasSailingDetails; }
		}

		public ZString SlotAsArrivalOrDeparture
		{
			get
			{
				string result = " - ";

				if (CommonCartage.IsContainerised)
				{
					if (CommonCartage.IsExportOrOrigin)
					{
						result = "D";
					}
					else if (CommonCartage.IsImportOrDestination)
					{
						result = "A";
					}
				}

				return result;
			}
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return ConsignmentID; }
		}

		#endregion

		#region Cartage Advice

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (fCartageAdvice == null)
				{
					fCartageAdvice = new CartageAdviceHelper(this, Factory);
				}
				return fCartageAdvice;
			}
		}
		CartageAdviceHelper fCartageAdvice;

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("db3a6916-edc7-47fc-8f51-df55983c7d2b", "PICKUP");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("8b69172e-80d1-44a7-9b0e-39ca031fb2b5", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("22865f63-228f-4fee-a49b-f50ee9085d2b", "FULL"));
					if (CommonCartage.JJ_EstimatedPickup.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("6cf25bb9-5aa5-43b6-a033-94de0996fa40", "DATE: {0}", CommonCartage.JJ_EstimatedPickup.ToLongTimeString()));
					}
				}

				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("a193eb94-91f5-408c-b4da-9d23245cf876", "DELIVER TO");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("8b69172e-80d1-44a7-9b0e-39ca031fb2b5", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("22865f63-228f-4fee-a49b-f50ee9085d2b", "FULL"));
					if (CommonCartage.JJ_EstimatedDelivery.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("6cf25bb9-5aa5-43b6-a033-94de0996fa40", "DATE: {0}", CommonCartage.JJ_EstimatedDelivery.ToLongTimeString()));
					}
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				MultilingualString result;

				if (PrintTwoJourneys)
				{
					result = ResString.GetMultilingualString("db3a6916-edc7-47fc-8f51-df55983c7d2b", "PICKUP");
					if (IsEmptyLeg(false))
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("8b69172e-80d1-44a7-9b0e-39ca031fb2b5", "EMPTY"));
					}
					else if (IsFullLeg(false))
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("22865f63-228f-4fee-a49b-f50ee9085d2b", "FULL"));
						if (CommonCartage.JJ_EstimatedPickup.IsValid)
						{
							result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("6cf25bb9-5aa5-43b6-a033-94de0996fa40", "DATE: {0}", CommonCartage.JJ_EstimatedPickup.ToLongTimeString()));
						}
					}
				}
				else
				{
					result = (NoResString)string.Empty;
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get
			{
				MultilingualString result;

				if (PrintTwoJourneys)
				{
					result = ResString.GetMultilingualString("a193eb94-91f5-408c-b4da-9d23245cf876", "DELIVER TO");
					if (IsEmptyLeg(false))
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("8b69172e-80d1-44a7-9b0e-39ca031fb2b5", "EMPTY"));
					}
					else if (IsFullLeg(false))
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("22865f63-228f-4fee-a49b-f50ee9085d2b", "FULL"));
						if (CommonCartage.JJ_EstimatedDelivery.IsValid)
						{
							result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("6cf25bb9-5aa5-43b6-a033-94de0996fa40", "DATE: {0}", CommonCartage.JJ_EstimatedDelivery.ToLongTimeString()));
						}
					}
				}
				else
				{
					result = (NoResString)string.Empty;
				}

				return result;
			}
		}

		#endregion

		#region Addresses

		enum JourneyAddressNumber
		{
			First = 0,
			Second = 1,
			Third = 2,
		}

		public DocDocAddress JourneyOnePickUpAddress
		{
			get { return GetJourneyAddress(JourneyAddressNumber.First); }
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get { return GetJourneyAddress(JourneyAddressNumber.Second); }
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get { return (PrintTwoJourneys) ? GetJourneyAddress(JourneyAddressNumber.Second) : null; }
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return (PrintTwoJourneys) ? GetJourneyAddress(JourneyAddressNumber.Third) : null; }
		}

		#region JourneyAddreses

		DocDocAddress GetJourneyAddress(JourneyAddressNumber index)
		{
			int indexAsInt = (int)index;
			return JourneyAddreses.Count > indexAsInt ? DocDocAddress.New(JourneyAddreses[indexAsInt], Factory) : null;
		}

		List<JobDocAddress> JourneyAddreses
		{
			get
			{
				if (journeyAddreses == null)
				{
					journeyAddreses = new List<JobDocAddress>(new CommonCartageBehaviorStrategy().GetJourneyAddresses(CommonCartage, IsContainerised));
				}

				return journeyAddreses;
			}
		}
		List<JobDocAddress> journeyAddreses;

		#endregion

		#endregion

		#region Contacts

		#region JourneyOnePickUpContact Details

		public ZString JourneyOnePickUpContactName
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactFax
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyOneDeliverToContact Details

		public ZString JourneyOneDeliverToContactName
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactFax
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoPickUpContact Details

		public ZString JourneyTwoPickUpContactName
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString JourneyTwoDeliverToContactName
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#endregion

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get
			{
				DocDocAddressCollection docAddressesOnCartageAdvice = new DocDocAddressCollection(Factory);

				if (JourneyOnePickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOnePickUpAddress);
				}

				if (JourneyOneDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOneDeliverToAddress);
				}

				if (PrintTwoJourneys)
				{
					if (JourneyTwoPickUpAddress != null)
					{
						docAddressesOnCartageAdvice.Add(JourneyTwoPickUpAddress);
					}

					if (JourneyTwoDeliverToAddress != null)
					{
						docAddressesOnCartageAdvice.Add(JourneyTwoDeliverToAddress);
					}
				}
				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public ZBool PrintAsContainers
		{
			get { return CommonCartage.IsContainerised; }
		}

		public ZBool PrintTwoJourneys
		{
			get
			{
				var result = false;

				var cartageTemplate = CommonCartage.CartageType;
				if (cartageTemplate != null)
				{
					var containerLegTemplates = cartageTemplate.ContainerizedCartageLegTypes;
					result = PrintAsContainers && (containerLegTemplates.Count > 1 || (containerLegTemplates.Count == 1 && containerLegTemplates.First().WaitPointOrg != null));
				}
				else
				{
					var containerisedLegs = CommonCartage.CartageLegs.Where(l => l.IsContainerised);
					result = PrintAsContainers && (containerisedLegs.Count() > 1 || (containerisedLegs.Count() == 1 && containerisedLegs.First().WaitPointOrganisation != null));
				}

				return result;
			}
		}

		public ZString EmailSubjectNumber
		{
			get { return ConsignmentID; }
		}

		public ZString EquipmentType
		{
			get { return DropMode; }
		}

		public ZString FullHandlingInstructions
		{
			get { return HandlingInstructions; }
		}

		public ZString FullCartageInstructions
		{
			get { return CartageInstructions; }
		}

		public ZDateTime CartageCutOffDate
		{
			get { return CommonCartage.IsLoose ? CommonCartage.LCLCutOff : CommonCartage.FCLCutOff; }
		}

		public ZDateTime CartageAvailableDate
		{
			get { return CommonCartage.IsLoose ? CommonCartage.LCLAvailabilityDate : CommonCartage.FCLAvailabilityDate; }
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get
			{
				if (IsExport)
				{
					return CartageCutOffDate;
				}
				else
				{
					return CartageAvailableDate;
				}
			}
		}

		public ZDateTime CartageReceivalDate
		{
			get { return CommonCartage.IsLoose ? CommonCartage.LCLReceivalCommences : CommonCartage.FCLReceivalCommences; }
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get { return CommonCartage.IsLoose ? CommonCartage.LCLStorageDate : CommonCartage.FCLStorageDate; }
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get
			{
				if (IsExport)
				{
					return PickupRequiredBy;
				}
				else
				{
					return CartageStorageCommenceDate;
				}
			}
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return IsExport ? CartageAdvice.PickupDateHeading : CartageAdvice.StorageCommencesHeading; }
		}

		#endregion

		#region Implementation

		public ZBool IsEmptyLeg(bool isJourneyOne)
		{
			var pickupAddress = GetPickupJourneyAddress(isJourneyOne);
			var deliveryAddress = GetDeliveryJourneyAddress(isJourneyOne);

			return GetDocAddressType(pickupAddress) == DocAddressType.LocalCartageYard
					|| GetDocAddressType(deliveryAddress) == DocAddressType.LocalCartageYard;
		}

		public ZBool IsFullLeg(bool isJourneyOne)
		{
			var result = false;

			if (CommonCartage.IsContainerised)
			{
				var pickupAddress = GetPickupJourneyAddress(isJourneyOne);
				var deliveryAddress = GetDeliveryJourneyAddress(isJourneyOne);

				result = GetDocAddressType(pickupAddress) != DocAddressType.LocalCartageYard
					&& GetDocAddressType(deliveryAddress) != DocAddressType.LocalCartageYard;
			}

			return result;
		}

		DocDocAddress GetPickupJourneyAddress(bool isJourneyOne)
		{
			return isJourneyOne ? JourneyOnePickUpAddress : JourneyTwoPickUpAddress;
		}

		DocDocAddress GetDeliveryJourneyAddress(bool isJourneyOne)
		{
			return isJourneyOne ? JourneyOneDeliverToAddress : JourneyTwoDeliverToAddress;
		}

		DocAddressType GetDocAddressType(DocDocAddress docDocAddress)
		{
			return docDocAddress != null ? docDocAddress.DocAddressType : DocAddressType.None;
		}

		#endregion

		#region FreightContainerSupport

		DocContainerCollectionHelper FreightContainerSupport
		{
			get { return freightContainerSupport ?? (freightContainerSupport = new DocContainerCollectionHelper(MaximumContainersWithTypeOnALine, false)); }
		}
		DocContainerCollectionHelper freightContainerSupport;

		public ZBool PrintPageWithContainerNumber
		{
			get
			{
				return (Containers.Count > MaximumContainersWithTypeOnALine && AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value);
			}
		}

		const int MaximumContainersWithTypeOnALine = 5;

		#endregion

		#region ContainerNumberAndTypeLine

		public ZString ContainerNumberAndTypeLine
		{
			get
			{
				return FreightContainerSupport.ContainerNumberAndType(Containers.ToIDocSimpleContainerCollection());
			}
		}

		#endregion

		#region Shipment

		//To satisfy Cartage Advice Shipment.NotClearedByAgentStatement ONLY
		public DocShipment Shipment
		{
			get { return null; }
		}

		#endregion
	}
}
