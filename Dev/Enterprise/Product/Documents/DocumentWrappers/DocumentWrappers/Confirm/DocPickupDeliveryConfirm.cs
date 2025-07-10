using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocPickupDeliveryConfirm : DocBaseWrapper, IDocCartageAdvice
	{
		protected DocPickupDeliveryConfirm(CommonPickupDeliveryConfirm confirm, CommonShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(confirm, factoryToWrap)
		{
			this.shipment = shipment;
		}

		public static DocPickupDeliveryConfirm New(CommonPickupDeliveryConfirm confirm, BusinessObjectFactory factoryToWrap)
		{
			return New(confirm, null, factoryToWrap);
		}

		public static DocPickupDeliveryConfirm New(CommonPickupDeliveryConfirm confirm, CommonShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			DocPickupDeliveryConfirm result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(confirm, shipment, factoryToWrap);
			}
			else if (confirm != null)
			{
				result = new DocPickupDeliveryConfirm(confirm, shipment, factoryToWrap);
			}

			return result;
		}

		protected delegate DocPickupDeliveryConfirm NewDelegate(CommonPickupDeliveryConfirm confirm, CommonShipment shipment, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public CommonPickupDeliveryConfirm CommonConfirm
		{
			get { return (CommonPickupDeliveryConfirm)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZString Context
		{
			get { return "PICKUPDELIVERYCONFIRM"; }
		}

		public DocConfirmDivotCollection Divots
		{
			get { return new DocConfirmDivotCollection(CommonConfirm); }
		}

		#region Properties

		public ZBool ShowContainerYardAsJourneyTwo
		{
			get { return showContainerYardAsJourneyTwo; }
			set { showContainerYardAsJourneyTwo = value; }
		}
		bool showContainerYardAsJourneyTwo = true;

		public ZString ContainerLegReference
		{
			get
			{
				ZString separator = "/";
				ZString jobRef = ZString.Empty;

				if (CommonConfirm.IsContainerised)
				{
					if (Container != null && Container.Consol != null)
					{
						jobRef = Container.Consol.ConsolNumber;
					}
				}
				else if (CommonConfirm.IsLoose)
				{
					if (Shipment != null)
					{
						jobRef = Shipment.JobNumber;
					}
				}

				return !jobRef.IsEmpty && !CommonConfirm.UniqueID.IsEmpty ? jobRef + separator + CommonConfirm.UniqueID : "";
			}
		}

		public ZInt PackagesDelivered
		{
			get { return CommonConfirm.TotalDeliveredPackages; }
		}

		public ZString TransportJobNumber
		{
			get { return JobNum; }
		}

		public ZString OrderReferenceNumber
		{
			get { return Shipment != null ? Shipment.OrderNumbers : ZString.Empty; }
		}

		public ZString WaybillNumber
		{
			get { return Shipment != null ? Shipment.HouseBill : ZString.Empty; }
		}

		public ZString WeightUQ
		{
			get { return WeightUnit; }
		}

		public ZString VolumeUQ
		{
			get { return VolumeUnit; }
		}

		public ZString BookingReference
		{
			get { return Container != null ? Container.ReleaseNum : ZString.Empty; }
		}

		public ZString JobNum
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.ShipmentNumber;
				}

				if (Container != null)
				{
					return Container.ContainerNumber;
				}

				return ZString.Empty;
			}
		}

		public ZInt Packages
		{
			get { return CommonConfirm.TotalBookedPackages; }
		}

		public ZString PackagesUnit
		{
			get { return CommonConfirm.TotalPackagesUnit; }
		}

		public ZDecimal Weight
		{
			get { return CommonConfirm.TotalBookedWeight; }
		}

		public ZDecimal WeightDelivered
		{
			get { return CommonConfirm.TotalDeliveredWeight; }
		}

		public ZString WeightUnit
		{
			get { return CommonConfirm.TotalWeightUnit; }
		}

		public ZDecimal Volume
		{
			get { return CommonConfirm.TotalBookedVolume; }
		}

		public ZDecimal VolumeDelivered
		{
			get { return CommonConfirm.TotalDeliveredVolume; }
		}

		public ZString VolumeUnit
		{
			get { return CommonConfirm.TotalVolumeUnit; }
		}

		public ZString Description
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.GoodsDescription;
				}
				return "";
			}
		}

		//obsolete?
		public ZBool IsEmptyLeg
		{
			get { return false; }
		}

		public ZBool IsFullLeg
		{
			get { return false; }
		}

		public ZString NotClearedByAgentNumber
		{
			get { return Shipment != null ? Shipment.NotClearedByAgentNumber : ZString.Empty; }
		}

		public ZDateTime NotClearedByAgentIssueDate
		{
			get { return Shipment != null ? Shipment.NotClearedByAgentIssueDate : ZDateTime.Empty; }
		}

		public ZDateTime NotClearedByAgentExpiryDate
		{
			get { return Shipment != null ? Shipment.NotClearedByAgentExpiryDate : ZDateTime.Empty; }
		}

		#region ConfirmationID

		public ZString ConfirmationID
		{
			get { return ContainerLegReference; }
		}

		#endregion

		#region Dimensions

		public ZString Dimensions
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocConfirmDivot divot in Divots)
				{
					if (divot.PackLine != null && divot.PackagesDelivered > 0)
					{
						ZString dimensions = !divot.PackLine.Dimensions.IsEmpty ? divot.PackLine.Dimensions.ToString() : Res.GetString("d3de3e3a-fd0e-4d89-b662-a1c9f9235b5c", "No Dimensions Specified");
						result += dimensions + " X " + divot.PackagesDelivered.ToString() + " " + divot.PackLine.PackType + "\n";
					}
				}

				return result.TrimEnd();
			}
		}

		#endregion

		#region HasDimensions

		public ZBool HasDimensions
		{
			get
			{
				ZBool result = false;
				foreach (DocConfirmDivot divot in Divots)
				{
					if (divot.PackLine != null && !divot.PackLine.Dimensions.IsEmpty)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		#endregion

		#region IsPickupConfirmation

		public ZBool IsPickupConfirmation
		{
			get
			{
				switch (CommonConfirm.EU_PickupDeliveryType)
				{
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					case Constants.PickupDeliveryConfirmTypes.OriginPickup:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#region IsDeliveryConfirmation

		public ZBool IsDeliveryConfirmation
		{
			get
			{
				switch (CommonConfirm.EU_PickupDeliveryType)
				{
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#endregion

		#region IDoc Cartage Advice

		public CartageAdviceHelper CartageAdvice
		{
			get { return cartageAdvice ?? (cartageAdvice = new CartageAdviceHelper(this, Factory)); }
		}
		CartageAdviceHelper cartageAdvice;

		public DocOrganisation Consignee
		{
			get { return consignee ?? (consignee = DocOrganisation.New(CommonConfirm.ConsigneeDocAddress, Factory)); }
		}
		DocOrganisation consignee;

		public DocOrganisation Consignor
		{
			get { return consignor ?? (consignor = DocOrganisation.New(CommonConfirm.ConsignorDocAddress, Factory)); }
		}
		DocOrganisation consignor;

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("29fb1f9b-e086-4982-821b-721bb7df2d1f", "PICKUP");

				if (IsEmptyLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9c2fa28a-f4ec-4cb4-978c-621b749ce439", "EMPTY"));
				}
				else if (IsFullLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("1467f26c-57e1-46f8-8b1c-aeb72c5c967a", "FULL"));
				}

				if (PlannedPickupTime.IsValid)
				{
					result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("0f1bc2ec-6864-4d08-80a0-68ee5a96fe69", "EST."), (NoResString)PlannedPickupTime.ToLongTimeString());
				}

				if (PickupRequestedByTime.IsValid)
				{
					result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("f6aeba34-7520-4515-8895-7315a461b736", "REQ. BY"), (NoResString)PickupRequestedByTime.ToLongTimeString());
				}

				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("5c468cf8-39f0-4e45-8779-370a43fd329e", "DELIVER");

				if (IsEmptyLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9c2fa28a-f4ec-4cb4-978c-621b749ce439", "EMPTY"));
				}
				else if (IsFullLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("1467f26c-57e1-46f8-8b1c-aeb72c5c967a", "FULL"));
				}
				else
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("2561f135-cc6f-4900-9ef5-edffacf0eb47", "TO"));
				}

				result = IncludeEstimatedDeliveryTimeIfIsValid(result);
				result = IncludeDeliveryRequestedByTimeIfIsValid(result);
				return result;
			}
		}

		protected virtual MultilingualString IncludeEstimatedDeliveryTimeIfIsValid(MultilingualString result)
		{
			if (EstimatedDeliveryTime.IsValid)
			{
				result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("ab36423f-ca42-4d55-b575-2d756fbdd828", "EST. {0}", EstimatedDeliveryTime.ToLongTimeString()));
			}
			return result;
		}

		protected virtual MultilingualString IncludeDeliveryRequestedByTimeIfIsValid(MultilingualString result)
		{
			if (DeliveryRequestedByTime.IsValid)
			{
				result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("c15afe17-1250-4709-930c-d0015c14a6d4", "REQ. BY {0}", DeliveryRequestedByTime.ToLongTimeString()));
			}
			return result;
		}

		/// <summary>
		/// Used On Cartage Request
		/// When Containerised, include Container Yard
		/// </summary>
		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				if (showContainerYardAsJourneyTwo)
				{
					MultilingualString result;

					if (HasContainerYard)
					{
						result = ResString.GetMultilingualString("0c5c21d0-acf4-4326-93f1-e6705d0c29f1", "CONTAINER YARD");
						if (EmptyBy.IsValid)
						{
							result = MultilingualString.Join(" ", result, (NoResString)"-", EmptyByHeading, (NoResString)EmptyBy.ToLongTimeString());
						}
					}
					else
					{
						result = (NoResString)string.Empty;
					}

					return result;
				}
				else
				{
					return JourneyOnePickUpHeading;
				}
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get { return showContainerYardAsJourneyTwo ? (NoResString)string.Empty : JourneyOneDeliverToHeading; }
		}

		#endregion

		#region Addresses

		public DocDocAddress JourneyOnePickUpAddress
		{
			get { return PickupFrom; }
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get { return DeliverTo; }
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get
			{
				if (showContainerYardAsJourneyTwo)
				{
					return HasContainerYard ? ContainerYard : null;
				}
				else
				{
					return JourneyOnePickUpAddress;
				}
			}
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return showContainerYardAsJourneyTwo ? null : JourneyOneDeliverToAddress; }
		}

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

				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public ZBool PrintAsContainers
		{
			get { return true; }
		}

		public ZBool PrintTwoJourneys
		{
			get { return false; }
		}

		public ZString EmailSubjectNumber
		{
			get { return Res.GetString("e3b550a7-70ba-4238-bb80-6e2cdc156ed0", "Confirm"); }
		}

		public ZString EquipmentType
		{
			get { return CommonConfirm.EU_DropMode; }
		}

		public ZString FullHandlingInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (Shipment != null)
				{
					result = Shipment.HandlingInstructions;
				}
				return result;
			}
		}

		public ZString LegNotes
		{
			get { return CommonConfirm.EU_PickupDeliveryInstruction; }
		}

		public ZString FullCartageInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (Shipment != null)
				{
					result = Shipment.CartageInstructions;
				}

				return result;
			}
		}

		public ZString DropMode
		{
			get { return BindToLists.GetCachedLists(Factory).DropModes().GetDescriptionFromCode(CommonConfirm.EU_DropMode); }
		}

		#endregion

		#region DailyWorkSheet Fields

		public ZString SortValue
		{
			get
			{
				ZString result = Driver;
				if (Vehicle != null)
				{
					result += Vehicle.Registration;
				}
				return result;
			}
		}

		public ZString PackageCount
		{
			get
			{
				ZString result = ZString.Empty;

				if (!Packages.IsEmpty)
				{
					result = Packages + " " + PackagesUnit;
				}

				return result;
			}
		}

		public ZString PackageCountDelivered
		{
			get
			{
				ZString result = ZString.Empty;

				if (!PackagesDelivered.IsEmpty)
				{
					result = PackagesDelivered + " " + PackagesUnit;
				}

				return result;
			}
		}

		#region Goods and Container Description

		public ZString GoodsAndContainerDescription
		{
			get
			{
				StringBuilder result = new StringBuilder();
				if (Containers.Count > 0)
				{
					foreach (DocCommonContainer docContainer in Containers)
					{
						result.Append(GetContainerDetails(docContainer));
					}
				}
				else
				{
					result.Append(ContainerLegSpecificGoodsAndContainerDescription);
				}

				return result.ToString().TrimEnd();
			}
		}

		ZString ContainerLegSpecificGoodsAndContainerDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (!PlannedPickupTime.IsEmpty)
				{
					result += Res.GetString("3ec54bd4-8988-45e0-b880-7b28bb2217dc", "Pickup:") + " " + PlannedPickupTime.ToString("dd/MM HH:mm") + "\r\n";
				}
				result += PackLinesLengthWidthHeight + "\n";
				if (Volume > 0)
				{
					result += ZString.Format(Res.GetString("6f2de317-8181-447c-b2d3-83d78d9c408c", "Total Volume: {0} {1}") + "\r\n", Volume, VolumeUQ);
				}
				if (Weight > 0)
				{
					result += ZString.Format(Res.GetString("40f134a3-f8bf-4c74-ad10-679d07bf53f7", "Total Weight: {0} {1}") + "\r\n", Weight, WeightUQ);
				}
				return result;
			}
		}

		public ZString PackLinesLengthWidthHeight
		{
			get
			{
				ZString result = "";
				foreach (DocPackLines packLine in PackLines)
				{
					if ((packLine.Length > 0) && (packLine.Width > 0) && (packLine.Height > 0))
					{
						result += (packLine.PackageCount + " " + packLine.PackType + ":" + " " + packLine.Length.ToString() + "*" + packLine.Width + "*" + packLine.Height + " " + packLine.UnitOfDimension + "\n");
					}
				}
				return result;
			}
		}

		protected ZString GetContainerDetails(DocCommonContainer container)
		{
			ZString result = "";

			if (container != null)
			{
				if (container.Container != null)
				{
					result += container.Container.Code + " ";
					result += container.ContainerMode;

					if (!PlannedPickupTime.IsEmpty)
					{
						result += " " + Res.GetString("2b684173-84ea-42ea-9cda-954d0af4e8f6", "Pickup:") + " " + PlannedPickupTime.ToString("dd/MM HH:mm");
					}

					result += "\n";
				}

				result += ZString.Format(Res.GetString("a2af070c-1a8b-4e2a-a72a-f688df746f18", "Container: {0:0.000}\r\nWeight: {1}") + "\r\n", container.ContainerNumber, container.GrossWeightWithUQ);
			}

			return result;
		}

		#endregion

		#region Remarks

		public ZString Remarks
		{
			get { return ""; }
		}

		public ZString Driver
		{
			get { return (Staff != null) ? Staff.FullName : NonStaffDriversName; }
		}

		#endregion

		#endregion

		#region Delivery Docket Fields

		public ZBool IsContainerised
		{
			get { return Container != null; }
		}

		public ZBool IsAir
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.IsAir;
				}
				return false;
			}
		}

		public ZBool IsRoad
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.IsRoad;
				}
				return false;
			}
		}

		public ZString VesselHeading
		{
			get
			{
				if (IsAir)
				{
					return Res.GetString("d8f74548-fd95-4621-a295-396124e8db31", "FLIGHT NO.");
				}
				else if (IsRoad)
				{
					return Res.GetString("ea58050d-13b0-4f1f-9207-d84b15eb1706", "TRUCK REF");
				}
				else
				{
					return Res.GetString("ed7dab8e-cb3e-4863-a285-9766c43c89dd", "VESSEL / VOYAGE");
				}
			}
		}

		public ZString Voyage
		{
			get
			{
				if (Container != null)
				{
					return Container.Consol.ConsolTransportInfo;
				}
				else if (Shipment != null && Shipment.Consol != null)
				{
					return Shipment.Consol.ConsolTransportInfo;
				}

				return "";
			}
		}

		public ZString PackagesOrTareWeightCaption
		{
			get
			{
				if (Containers.Count > 0)
				{
					return Res.GetString("4142718d-a429-4e57-97f6-ed507265c99a", "TARE WEIGHT");
				}
				else
				{
					return Res.GetString("e39bae37-4811-499a-b91c-50db8d76cec8", "PIECES");
				}
			}
		}

		public ZString PackagesOrTareWeightText
		{
			get
			{
				if (Containers.Count > 0)
				{
					return Containers[0].TareWeight.ToString();
				}
				else
				{
					return PackagesText;
				}
			}
		}

		public ZString PackagesText
		{
			get { return PackageCount; }
		}

		public ZString PackagesTextDelivered
		{
			get { return PackageCountDelivered; }
		}

		public ZString WeightCaption
		{
			get
			{
				if (Containers.Count > 0)
				{
					return Res.GetString("6aed498d-cedb-46cd-b04d-6fcca2b23a48", "GROSS WEIGHT");
				}
				else
				{
					return Res.GetString("b7383500-e7f6-4749-8198-eb138c540c15", "WEIGHT");
				}
			}
		}

		public ZString WeightText
		{
			get { return Weight + " " + WeightUnit; }
		}

		public ZString VolumeText
		{
			get { return Volume + " " + VolumeUnit; }
		}

		public ZString WeightTextDelivered
		{
			get { return WeightDelivered + " " + WeightUnit; }
		}

		public ZString VolumeTextDelivered
		{
			get { return VolumeDelivered + " " + VolumeUnit; }
		}

		public ZString ContainerDescription
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (DocContainer container in Containers)
				{
					if (result.Length > 0)
					{
						result.Append("; ");
					}

					result.Append(container.ContainerNumber + " ");
					if (container.Container != null)
					{
						result.Append("1 * " + container.Container.Code);
					}
				}
				return result.ToString();
			}
		}

		public ZString DeliveryInstructions
		{
			get
			{
				ZString result = "";
				if (Shipment != null)
				{
					result = Shipment.CartageInstructions;
				}
				return result;
			}
		}

		public ZString HandlingInstructions
		{
			get
			{
				ZString result = "";
				if (Shipment != null)
				{
					result = Shipment.HandlingInstructions;
				}
				return result;
			}
		}

		#endregion

		#region Pickup Times

		/// <summary>
		/// If Confirmation is a Pickup Confirmation
		/// 1. Use EU_PlannedPickupDeliveryTime
		/// 2. Else Shipment Estimated Pickup
		/// </summary>
		public ZDateTime PlannedPickupTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsPickupConfirmation)
				{
					result = CommonConfirm.EU_PlannedPickupDeliveryTime;

					if (result.IsEmpty && Shipment != null)
					{
						result = Shipment.DocsAndCartage.EstimatedPickup;
					}

					if (result.IsEmpty && Container != null)
					{
						result = Container.DepartureEstimatedPickup;
					}
				}

				return result;
			}
		}

		/// <summary>
		/// If Confirmation is a Pickup Confirmation
		/// 1. Use EU_RequestedPickupDeliveryTime
		/// 2. Else Shipment Pickup RequiredBy
		/// </summary>
		public ZDateTime PickupRequestedByTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsPickupConfirmation)
				{
					result = CommonConfirm.EU_RequestedPickupDeliveryTime;

					if (result.IsEmpty && Shipment != null)
					{
						result = Shipment.DocsAndCartage.PickupRequiredBy;
					}
				}

				return result;
			}
		}

		public ZDateTime PickupTimeIn
		{
			get { return IsPickupConfirmation ? CommonConfirm.EU_PickupDeliveryTime : ZDateTime.Empty; }
		}

		public ZDateTime PickupTimeOut
		{
			get { return PickupTimeIn; }
		}

		public ZString PickupTimeInOutText
		{
			get
			{
				ZString result = " - ";

				if (PickupTimeIn.IsValid)
				{
					result = PickupTimeIn.ToString("dd-MMM HH:mm") + " / ";
					result += (PickupTimeOut.IsValid) ? PickupTimeOut.ToShortTimeString() : "-";
				}

				return result;
			}
		}

		public ZString PickupTimeInText
		{
			get { return PickupTimeIn.IsValid ? PickupTimeIn.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString PickupTimeOutText
		{
			get { return PickupTimeOut.IsValid ? PickupTimeOut.ToString("dd-MMM HH:mm") : " - "; }
		}

		#endregion

		#region Deliver Times

		/// <summary>
		/// If Confirmation is a Pickup Confirmation
		/// 1. Use EU_PlannedPickupDeliveryTime
		/// 2. Else Shipment Estimated Delivery
		/// 3. Else Container Estimated Delivery
		/// </summary>
		public ZDateTime EstimatedDeliveryTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsDeliveryConfirmation)
				{
					result = CommonConfirm.EU_PlannedPickupDeliveryTime;

					if (result.IsEmpty && Shipment != null)
					{
						result = Shipment.DocsAndCartage.EstimatedDelivery;
					}

					if (result.IsEmpty && Container != null)
					{
						result = Container.EstimatedDelivery;
					}
				}

				return result;
			}
		}

		/// <summary>
		/// If Confirmation is a Delivery Confirmation
		/// 1. Use EU_RequestedPickupDeliveryTime
		/// 2. Else Shipment Delivery RequiredBy
		/// </summary>
		public ZDateTime DeliveryRequestedByTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsDeliveryConfirmation)
				{
					result = CommonConfirm.EU_RequestedPickupDeliveryTime;

					if (result.IsEmpty && Shipment != null)
					{
						result = Shipment.DocsAndCartage.DeliveryRequiredBy;
					}
				}

				return result;
			}
		}

		public ZDateTime DeliverTimeIn
		{
			get { return IsDeliveryConfirmation ? CommonConfirm.EU_PickupDeliveryTime : ZDateTime.Empty; }
		}

		public ZDateTime DeliverTimeOut
		{
			get { return DeliverTimeIn; }
		}

		public ZString DeliverTimeInOutText
		{
			get
			{
				ZString result = " - ";
				if (DeliverTimeIn.IsValid)
				{
					result = DeliverTimeIn.ToString("dd-MMM HH:mm") + " / ";
					result += (DeliverTimeOut.IsValid) ? DeliverTimeOut.ToShortTimeString() : "-";
				}

				return result;
			}
		}

		public ZString DeliverTimeInText
		{
			get { return DeliverTimeIn.IsValid ? DeliverTimeIn.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString DeliverTimeOutText
		{
			get { return DeliverTimeOut.IsValid ? DeliverTimeOut.ToString("dd-MMM HH:mm") : " - "; }
		}

		#endregion

		#region Summary Sheet Fields

		#region Doc CartagePackLineCollection

		public DocPackLinesCollection PackLines
		{
			get
			{
				if (fDocPackLines == null)
				{
					fDocPackLines = new DocPackLinesCollection(Factory);

					foreach (CommonConfirmDivot divot in CommonConfirm.Divots)
					{
						if (divot.PackLine != null)
						{
							fDocPackLines.Add(DocPackLines.New(divot.PackLine, Factory));
						}
					}
				}

				return fDocPackLines;
			}
		}
		DocPackLinesCollection fDocPackLines;

		public ZString PackLinesPackageCountColumn
		{
			get
			{
				ZString result = new ZString();

				foreach (DocPackLines packLine in PackLines)
				{
					result += packLine.PackageCount + System.Environment.NewLine;
				}

				return result;
			}
		}

		public ZString PackLinesDescriptionColumn
		{
			get
			{
				ZString result = new ZString();

				foreach (DocPackLines packLine in PackLines)
				{
					result += packLine.Description + System.Environment.NewLine;
				}

				return result;
			}
		}

		public ZString PackLinesCargoLocationAndPacksWithShipmentLocationOfGoodsColumn
		{
			get
			{
				ZString result = new ZString();

				foreach (DocPackLines packLine in PackLines)
				{
					result += packLine.CargoLocationAndPacksWithShipmentLocationOfGoods + System.Environment.NewLine;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Confirm Fields

		public ZString DeliverySignedFor
		{
			get { return CommonConfirm.EU_GoodsSignForBy; }
		}

		public ZString DriversLicense
		{
			get { return CommonConfirm.EU_DriversLicence; }
		}

		public ZString NonStaffDriversName
		{
			get { return CommonConfirm.EU_DriversName; }
		}

		public DocOrganisation TransportCo
		{
			get { return DocOrganisation.New(CommonConfirm.TransportCo, Factory); }
		}

		public DocStaff Staff
		{
			get { return null; }
		}

		public DocDocAddress DeliverTo
		{
			get
			{
				if (fDeliverTo == null)
				{
					fDeliverTo = DocDocAddress.New(CommonConfirm.DeliverTo, Factory);
				}
				return fDeliverTo;
			}
		}
		DocDocAddress fDeliverTo;

		public DocDocAddress PickupFrom
		{
			get
			{
				if (fPickupFrom == null)
				{
					fPickupFrom = DocDocAddress.New(CommonConfirm.PickupFrom, Factory);
				}
				return fPickupFrom;
			}
		}
		DocDocAddress fPickupFrom;

		public DocDocAddress ConfirmAddress
		{
			get
			{
				if (fConfirmAddress == null)
				{
					fConfirmAddress = DocDocAddress.New(CommonConfirm.ConfirmAddress, Factory);
				}
				return fConfirmAddress;
			}
		}
		DocDocAddress fConfirmAddress;

		public DocShipment Shipment
		{
			get
			{
				DocShipment result = null;

				if (shipment != null)
				{
					result = GetDocShipment(shipment);
				}
				else
				{
					foreach (CommonConfirmDivot divot in CommonConfirm.Divots)
					{
						if (divot.PackLine != null && divot.PackLine.Shipment != null)
						{
							result = GetDocShipment(divot.PackLine.Shipment);
							break;
						}
					}
				}
				return result;
			}
		}
		readonly CommonShipment shipment;

		DocShipment GetDocShipment(CommonShipment shipment)
		{
			DocShipment result = null;

			if (typeof(GatePassShipment).IsAssignableFrom(shipment.GetType()))
			{
				result = DocGatePassShipment.New((GatePassShipment)shipment, Factory);
			}
			else if (typeof(CFSShipment).IsAssignableFrom(shipment.GetType()))
			{
				result = DocShipmentReceival.New((CFSShipment)shipment, Factory);
			}
			else if (typeof(ForwardingShipment).IsAssignableFrom(shipment.GetType()))
			{
				result = DocForwardingShipment.New((ForwardingShipment)shipment, Factory);
			}
			else
			{
				result = DocShipment.New(shipment, Factory);
			}

			return result;
		}

		public DocGatePassShipment GatePassShipment
		{
			get { return Shipment as DocGatePassShipment; }
		}

		public DocShipmentReceival CFSShipment
		{
			get { return Shipment as DocShipmentReceival; }
		}

		public ZString TruckRegistration
		{
			get { return CommonConfirm.EU_VehicleRegistration; }
		}

		public DocRefEquipment ExtraEquip1
		{
			get { return null; }
		}

		public DocRefEquipment ExtraEquip2
		{
			get { return null; }
		}

		public DocRefEquipment Trailer
		{
			get { return null; }
		}

		public DocRefEquipment Vehicle
		{
			get { return null; }
		}

		#region Yard

		public ZBool HasContainerYard
		{
			get { return IsContainerised && !((JobDocAddress)ContainerYard.WrappedObject).IsEmpty; }
		}

		public DocDocAddress ContainerYard
		{
			get
			{
				if (containerYard == null)
				{
					containerYard = DocDocAddress.New(CommonConfirm.ContainerYard, Factory);
				}
				return containerYard;
			}
		}
		DocDocAddress containerYard;

		public MultilingualString EmptyByHeading
		{
			get
			{
				MultilingualString result;

				if (CommonConfirm.Container != null)
				{
					result = CommonConfirm.IsOrigin ? ResString.GetMultilingualString("56f5a789-4760-41b1-8a1b-22905d9f1242", "EMPTY REQUIRED BY") : ResString.GetMultilingualString("d6143243-7f57-4b6b-9675-bdc645339b46", "EMPTY RETURN BY");
				}
				else
				{
					result = (NoResString)string.Empty;
				}
				return result;
			}
		}

		public ZDateTime EmptyBy
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (CommonConfirm.Container != null)
				{
					result = CommonConfirm.IsOrigin ? CommonConfirm.Container.JC_EmptyRequired : CommonConfirm.Container.JC_EmptyReturnedBy;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Related DocWrappers

		#region Containers

		public DocContainer Container
		{
			get { return Containers.Count > 0 ? (DocContainer)Containers[0] : null; }
		}

		public DocContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new DocContainerCollection(Factory);

					var parentShipment = Shipment != null ? (CommonShipment)Shipment.WrappedObject : null;
					var parentContainer = CommonConfirm.Container;

					var docContainer = DocContainer.New(parentContainer, parentShipment, Factory);
					if (docContainer != null)
					{
						fContainers.Add(docContainer);
					}
				}
				return fContainers;
			}
		}
		DocContainerCollection fContainers;

		public DocContainerCollection LooseContainers
		{
			get
			{
				if (fLooseContainers == null)
				{
					fLooseContainers = new DocContainerCollection(Factory);

					foreach (CommonConfirmDivot divot in CommonConfirm.Divots)
					{
						if (divot.J8_PackagesDelivered > 0 && divot.PackLine != null)
						{
							foreach (CommonContainer container in divot.PackLine.Containers)
							{
								DocContainer docContainer = DocContainer.New(container, Shipment != null ? (CommonShipment)Shipment.WrappedObject : null, Factory);
								if (docContainer != null)
								{
									fLooseContainers.Add(docContainer);
								}
							}
						}
					}
				}
				return fLooseContainers;
			}
		}

		DocContainerCollection fLooseContainers;

		#endregion

		#endregion

		#region CFS/GatePass

		#region GatePassID

		public ZString GatePassID
		{
			get { return CommonConfirm.FullGatePass; }
		}

		#endregion

		#region TransportCoName

		public ZString TransportCoName
		{
			get { return CommonConfirm.EU_TransportCoName; }
		}

		#endregion

		#region DriversName

		public ZString DriversName
		{
			get { return CommonConfirm.EU_DriversName; }
		}

		#endregion

		#region VehicleReg

		public ZString VehicleReg
		{
			get { return TruckRegistration; }
		}

		#endregion

		#region MergedMarksAndNumbers

		public ZString MergedMarksAndNumbers
		{
			get
			{
				List<PackLine> list = new List<PackLine>();

				foreach (CommonConfirmDivot divot in CommonConfirm.Divots)
				{
					if (divot.J8_PackagesDelivered > 0 && !divot.PackLine.JL_MarksAndNumbers.IsEmpty)
					{
						list.Add(divot.PackLine);
					}
				}

				ZString result = FreightHelperClass.MergePackMarksAndNumbers(list);

				if (result.IsEmpty && Shipment != null)
				{
					result = Shipment.MarksAndNumbers;
				}

				return result;
			}
		}

		#endregion

		#region PackagesTaken

		public ZInt PackagesTaken
		{
			get { return PackagesDelivered; }
		}

		#endregion

		#region PackType

		public ZString PackType
		{
			get { return PackagesUnit; }
		}

		#endregion

		#region PackTypeDescription

		public ZString PackTypeDescription
		{
			get { return GatePassPackLine.PackType_List.GetDescriptionFromCode(PackType); }
		}

		#endregion

		#region PickupTime

		public ZDateTime PickupTime
		{
			get { return CommonConfirm.EU_PickupDeliveryTime; }
		}

		#endregion

		#region TotalDeliveredWeight

		public ZDecimal TotalDeliveredWeight
		{
			get { return CommonConfirm.TotalDeliveredWeight; }
		}

		#endregion

		#region TotalDeliveredVolume

		public ZDecimal TotalDeliveredVolume
		{
			get { return CommonConfirm.TotalDeliveredVolume; }
		}

		#endregion

		#region ContainerJobNumber

		public ZString ContainerJobNumber
		{
			get
			{
				ZString containerJobNum = ZString.Empty;

				if (CommonConfirm.Container != null)
				{
					containerJobNum = CommonConfirm.Container.JC_ContainerJobID;
				}
				else
				{
					foreach (DocContainer container in LooseContainers)
					{
						containerJobNum += container.ContainerJobID + ",";
					}
					if (!containerJobNum.IsEmpty)
					{
						containerJobNum = containerJobNum.TrimEndIncludingWhiteSpace(',');
					}
				}

				return containerJobNum;
			}
		}

		#endregion

		#region UnpackShed

		public ZString UnpackShed
		{
			get
			{
				ZString result = ZString.Empty;

				if (CommonConfirm.Container != null)
				{
					result = CommonConfirm.Container.JC_UnpackShed;
				}
				else
				{
					foreach (DocContainer container in LooseContainers)
					{
						result += container.UnpackShed + ",";
					}
					if (!result.IsEmpty)
					{
						result = result.TrimEndIncludingWhiteSpace(',');
					}
				}

				return result;
			}
		}

		#endregion

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get
			{
				ZString containerNum = ZString.Empty;

				if (CommonConfirm.Container != null)
				{
					containerNum = CommonConfirm.Container.JC_ContainerNum;
				}
				else
				{
					foreach (DocContainer container in LooseContainers)
					{
						containerNum += container.ContainerNumber + ",";
					}
					if (!containerNum.IsEmpty)
					{
						containerNum = containerNum.TrimEndIncludingWhiteSpace(',');
					}
				}

				return containerNum;
			}
		}

		#endregion

		#region CargoLocationAndPacks

		public ZString CargoLocationAndPacks
		{
			get
			{
				StringBuilder locationBuilder = new StringBuilder();

				foreach (CommonConfirmDivot divot in CommonConfirm.Divots)
				{
					if (divot.J8_PackagesDelivered > 0)
					{
						foreach (PackLocation location in divot.PackLine.PackLocations)
						{
							if (locationBuilder.Length > 0)
							{
								locationBuilder.Append("; ");
							}

							locationBuilder.AppendFormat(Res.GetString("44d601d5-72f2-4c72-9fc5-15e3af594df1", "{0}, Packs: {1} {2}", location.JQ_WarehouseLocation, location.JQ_NoPackages, divot.PackLine.JL_F3_NKPackType));
						}
					}
				}

				return locationBuilder.ToString();
			}
		}

		#endregion

		#region GatePassStatementText

		public ZString GatePassStatementText
		{
			get
			{
				ZString result = ZString.Empty;
				if (GatePassShipment != null && DocumentsDataRegistry.Instance.ShowGatePassStatementText.Value && GatePassShipment.GatePassStatusShort != CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased)
				{
					string deliverTo = DeliverTo != null ? DeliverTo.CompanyName.ToString() : "";

					result = Res.GetString("7b8d4a96-6681-4814-9650-40e7dd0d0260", @"BOND CARGO UNDER CUSTOMS and QUARANTINE CONTROL
		TO BE MOVED ONLY BY {0} TO {1} 
REPORT ALL LOSS OR THEFT IMMEDIATELY TO PH {2}.", CommonConfirm.Shipments[0].JS_TransportMode, deliverTo, CurrentCompany.Phone);
				}
				return result;
			}
		}

		#endregion

		#region GatePassClauseText

		public ZString GatePassClauseText
		{
			get { return DocumentsDataRegistry.Instance.GatePassClauseText.Value; }
		}

		#endregion

		#region IdentityDocument

		public ZString IdentityDocument
		{
			get { return CommonConfirm.EU_DriversLicence; }
		}

		#endregion

		#endregion

		#region IDocCartageAdvice Members

		public ZDateTime CartageCutOffDate
		{
			get { return Shipment != null ? Shipment.CartageCutOffDate : ZDateTime.Empty; }
		}

		public ZDateTime CartageAvailableDate
		{
			get { return Shipment != null ? Shipment.CartageAvailableDate : ZDateTime.Empty; }
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CartageReceivalDate
		{
			get { return Shipment != null ? Shipment.CartageReceivalDate : ZDateTime.Empty; }
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get { return Shipment != null ? Shipment.CartageStorageCommenceDate : ZDateTime.Empty; }
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
