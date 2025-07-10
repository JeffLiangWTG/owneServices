using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using IFreightDataLayer = Enterprise.Integration.Customs.AU.IFreightDataLayer;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FreightDataLayer : ContainerMessagingData, IFreightDataLayer
	{
		#region Interface

		public FreightDataLayer(CommonContainer container)
			: base(container)
		{
			this.Container = Argument.NotNull(container, "container");
			MainTransport = FindMainTransport(Container);
		}
		protected readonly CommonContainer Container;
		protected readonly Transport MainTransport;

		public override EDIMessage GetEDIMessage()
		{
			return Container.PRAMessages.AddNew();
		}

		public override bool ContainerIsWaitingForResponse
		{
			get
			{
				return Container.IsWaitingForPRAResponse;
			}
		}
		#endregion

		#region Consol Level Field Mappers

		protected override string GetPortOfLoading()
		{
			return MainTransport == null ? "" : (string)MainTransport.JW_RL_NKLoadPort;
		}

		protected override string GetPortOfDischarge()
		{
			return MainTransport == null ? "" : (string)MainTransport.JW_RL_NKDiscPort;
		}

		protected override string GetVesselName()
		{
			return MainTransport == null ? "" : (string)MainTransport.JW_Vessel;
		}

		protected override string GetVoyage()
		{
			return MainTransport == null ? "" : (string)MainTransport.JW_VoyageFlight;
		}

		protected override string GetLloydsNumber()
		{
			RefVessel refVessel = (MainTransport == null ? null : MainTransport.Vessel);
			return (refVessel == null ? ZString.Empty : refVessel.RV_LloydsNumber);
		}

		protected override string GetECNorCRN()
		{
			string result = "";
			result = ForwardingConsol.JK_CRN;
			if (string.IsNullOrEmpty(result))
			{
				result = Container.ECNOrCAN;
			}
			return result;
		}

		protected override string GetConsignorName()
		{
			ZString result = "";

			if (ForwardingConsol.IsDirect && !ForwardingConsol.ShipmentConsignor.IsEmpty)
			{
				var orgHeader = SavedFactory.Load<OrgHeader>(ForwardingConsol.ShipmentConsignor);
				result = (orgHeader == null ? ZString.Empty : orgHeader.OH_FullNameTruncated);
			}

			if (result.IsEmpty && ForwardingConsol.SendingForwarder != null)
			{
				result = ForwardingConsol.SendingForwarder.OH_FullName;
			}

			if (result.IsEmpty && GlbBranch.CurrentBranch.OrgProxy != null)
			{
				result = GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
			}

			if (result.IsEmpty)
			{
				result = GlbCompany.CurrentCompany.GC_Name;
			}

			return result.SubstringSafe(0, PRAConstants.ConsignorNameMaxLength);
		}

		protected override string GetShippingLine1StopCode()
		{
			return ShippingLine.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.OneStopCode, GlbBranch.CurrentBranch.Country);
		}

		protected override string GetLoadTerminal1StopCode()
		{
			if (CTO != null)
			{
				return CTO.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.OneStopCode, GlbBranch.CurrentBranch.Country);
			}
			else
			{
				return "";
			}
		}

		protected override string GetPortOfFinalDischarge()
		{
			return ForwardingConsol.JK_RL_NKDischargePort;
		}

		protected override string GetGrossWeightVerifiedDeclarantSignature()
		{
			var consignorDocAddress = GetConsignorDocAddress();
			return (ForwardingConsol.IsDirect && consignorDocAddress != null)
				? consignorDocAddress.E2_Contact.ToString()
				: SenderContactName;
		}

		protected override string GetGrossWeightVerifiedDeclarantContact()
		{
			ZStringBuilder result = new ZStringBuilder();
			var consignorDocAddress = GetConsignorDocAddress();
			if (ForwardingConsol.IsDirect && consignorDocAddress != null)
			{
				if (!consignorDocAddress.E2_Phone.IsEmpty || !consignorDocAddress.E2_Email.IsEmpty)
				{
					var consignorDocAddressCompanyName = consignorDocAddress.Organisation != null
														? consignorDocAddress.Organisation.OH_FullName.ToString()
														: string.Empty;

					result.Append(consignorDocAddress.E2_Contact);
					result.Append(consignorDocAddressCompanyName);
					result.Append(consignorDocAddress.E2_Phone);
					result.Append(consignorDocAddress.E2_Email);
				}
			}
			else
			{
				if (!SenderPhone.IsNullOrEmpty() || !SenderEmail.IsNullOrEmpty())
				{
					result.Append(SenderContactName ?? string.Empty);
					result.Append(SenderCompanyName ?? string.Empty);
					result.Append(SenderPhone ?? string.Empty);
					result.Append(SenderEmail ?? string.Empty);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends(";");
		}

		protected override string GetGrossWeightDeclarantCompanyName()
		{
			string result;
			var consignorDocAddress = GetConsignorDocAddress();
			if (ForwardingConsol.IsDirect && consignorDocAddress != null)
			{
				result = consignorDocAddress.Organisation != null
					? consignorDocAddress.Organisation.OH_FullName.ToString()
					: string.Empty;
			}
			else
			{
				result = SenderCompanyName;
			}
			return result;
		}

		JobDocAddress GetConsignorDocAddress()
		{
			var shipment = ForwardingConsol.Shipments.Cast<CommonShipment>().FirstOrDefault();

			return shipment != null
				? shipment.GetConsignorDocAddress
				: null;
		}

		#endregion

		#region Container FieldMappers

		protected override string GetContainerNumber()
		{
			return Container.JC_ContainerNum;
		}

		protected override string GetSealNumber()
		{
			return Container.JC_SealNum;
		}

		protected override decimal GetContainerGrossWeight()
		{
			return Container.JC_GrossWeight;
		}

		protected override decimal GetContainerTareWeight()
		{
			return Container.JC_TareWeight;
		}

		protected override decimal GetContainerNetWeight()
		{
			return ContainerTareWeight == 0m ? 0m : ContainerGrossWeight - ContainerTareWeight;
		}

		protected override bool GetIsTempControlled()
		{
			bool result = false;
			if (Container != null)
			{
				result = Container.JC_IsControlledAtmosphere;
				if (!Container.JC_IsRefrigerated)
				{
					result = false;
				}
			}
			return result;
		}

		protected override string GetFlatRackID()
		{
			return "";
		}

		protected override string GetTruckRegoNumber()
		{
			return "";
		}

		protected override string GetRoadOrig1StopCode()
		{
			return "";
		}

		protected override string GetRoadDest1StopCode()
		{
			return "";
		}

		protected override ZDateTime GetRoadScheduledDeparture()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetRoadScheduledArrival()
		{
			return ZDateTime.Empty;
		}

		#endregion

		#region Container Cartage Company FieldMappers

		protected override string GetCartageCompanyABN()
		{
			return Container.CartageCompanyDeliveringToCTO == null ? ZString.Empty : Container.CartageCompanyDeliveringToCTO.PrimaryRegistrationNumber.Number;
		}

		protected override string GetCartageBookingReference()
		{
			return string.Empty;
		}

		#endregion

		#region Other DataMappers

		protected override string GetMessageReference()
		{
			return "CON-" + ForwardingConsol.JK_UniqueConsignRef + "-" + Container.JC_ContainerNum;
		}

		protected override string GetGoodsDescription()
		{
			return RefCommodityCode.RH_DescriptionMultilingual;
		}

		protected override string GetDateTimeStringForMessage()
		{
			return ZDateTime.Now.ToString("yyyyMMddHHmmss");
		}

		#endregion

		#region Dangerous Goods

		protected override DangerousGoodsCollection GetDangerousGoodsList()
		{
			var result = new DangerousGoodsCollection();
			this.LoadFromContainer(result);
			return result;
		}

		public void LoadFromContainer(DangerousGoodsCollection dangerousGoodsCollection)
		{
			if (Container != null)
			{
				foreach (PackLine packLine in Container.PackLines)
				{
					var shouldUsePackLineWeight = packLine.UNDGs.All(dg => dg.DI_DGWeight == 0);
					var packlineWeight = Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, Core.Constants.Weight.Kilograms);

					foreach (var dgItem in packLine.UNDGs)
					{
						var substance = dgItem.Substance;
						if (substance != null)
						{
							var goods = dangerousGoodsCollection.AddNew();

							var shipment = packLine.Shipment;
							if (shipment != null)
							{
								goods.ShipmentReference = shipment.JS_UniqueConsignRef;
							}

							goods.IMDGClass = substance.DG_Class; // Required.
							goods.IMDGCodePage = ""; // We don't have this. Not Required.
							goods.IMDGCodeVersion = substance.DG_Variant;
							goods.UNDGNumber = substance.DG_UNNO;
							if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
							{
								if (dgItem.DI_IsCombustible)
								{
									goods.FlashpointTemperatureInCelcius = TemperatureFormatter.FormatTemperatureString(dgItem.DI_DGFlashPoint.ToString());
								}
							}
							else
							{
								goods.FlashpointTemperatureInCelcius = TemperatureFormatter.FormatTemperatureString(dgItem.DI_DGFlashPoint.ToString());
							}
							goods.PackingGroup = substance.DG_PG;
							goods.TechnicalName = substance.DG_PSN;
							goods.Weight = shouldUsePackLineWeight
										? packlineWeight
										: Core.Constants.Weight.Convert(dgItem.DI_DGWeight, dgItem.DI_UnitOfWeight, Core.Constants.Weight.Kilograms);

							var contact = dgItem.DGContact
								?? Container.Factory.Load<OrgContact>(Env.Registry.Freight.PRAMessaging.SeaFreightDangerousGoodsContact);

							if (contact != null)
							{
								var details = contact.DocDeliveryDetails();
								goods.ContactName = details.Name;
								goods.ContactPhoneNumber = details.Phone;
								goods.ContactFaxNumber = details.Fax;
								goods.ContactEmailAddress = details.Email;
							}
						}
					}
				}
			}
		}

		#endregion

		#region Container Child Business Object Lazy Loaders

		#region JobContainer

		protected override CommonContainer GetJobContainer()
		{
			return Container;
		}

		#endregion

		#region RefCommodityCode

		protected RefCommodityCode RefCommodityCode
		{
			get
			{
				if (refCommodityCode == null)
				{
					refCommodityCode = SavedFactory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, Container.JC_RH_NKContainerCommodityCode);
				}
				if (refCommodityCode == null)
				{
					refCommodityCode = SavedFactory.GetNull<RefCommodityCode>();
				}
				return refCommodityCode;
			}
		}
		RefCommodityCode refCommodityCode;

		#endregion

		#region RefContainer

		protected override RefContainer GetRefContainer()
		{
			if (refContainer == null)
			{
				refContainer = SavedFactory.Load<RefContainer>(Container.JC_RC);
			}
			if (refContainer == null)
			{
				refContainer = SavedFactory.GetNull<RefContainer>();
			}
			return refContainer;
		}
		RefContainer refContainer;

		#endregion

		#region ShippingLine

		protected OrgHeader ShippingLine
		{
			get
			{
				if (shippingLine == null)
				{
					shippingLine = SavedFactory.Load<OrgHeader>(ForwardingConsol.ShippingLinePK);
				}
				if (shippingLine == null)
				{
					shippingLine = SavedFactory.GetNull<OrgHeader>();
				}
				return shippingLine;
			}
		}
		OrgHeader shippingLine;

		#endregion

		#region CTO

		protected OrgHeader CTO
		{
			get
			{
				if (cto == null && ForwardingConsol.DepartureCTOAddress != null)
				{
					cto = ForwardingConsol.DepartureCTOAddress.Header;
				}
				return cto;
			}
		}
		OrgHeader cto;

		#endregion

		#region ForwardingConsol

		protected CommonConsol ForwardingConsol
		{
			get
			{
				if (freightConsol == null)
				{
					freightConsol = Container.Consol;
				}
				if (freightConsol == null)
				{
					freightConsol = SavedFactory.GetNull<ForwardingConsol>();
				}
				return freightConsol;
			}
		}
		CommonConsol freightConsol;

		#endregion

		#endregion

		#region CheckMissingData

		protected override string ShippingLineBookingReferenceGUILocation
		{
			get
			{
				return "Consol Tab or Container Tab";
			}
		}

		protected override string ECNorCRNGUILocation
		{
			get
			{
				string result = @"ECN or CRN. (Against Consol or Shipment as applicable)
   NB: If you are relying on the ECN from the Shipment, make sure you have
   this container selected in the 'Packing' section of the relevant Shipment.
   Important: 1-Stop can only accept one reference per container, so if you 
   have multiple shipments in the one container with individual CAN's, you will 
   have to submit a CRN for the Consol to get a single CAN for the contents 
   of the whole container before sending a PRA.
";
				return result;
			}
		}

		protected override string ShippingLine1StopCodeGUILocation
		{
			get
			{
				return "Registration Numbers on the Config Tab in the Carrier Master File";
			}
		}

		protected override string VesselNameGUILocation
		{
			get
			{
				return "Consol Tab";
			}
		}

		protected override string VoyageGUILocation
		{
			get
			{
				return "Consol Tab";
			}
		}

		protected override string LloydsNumberGUILocation
		{
			get
			{
				return "Vessel Master File for selected vessel on Consol Tab";
			}
		}

		protected override string PortOfLoadingGUILocation
		{
			get
			{
				return "Consol Tab";
			}
		}

		protected override string Terminal1StopCodeMissingGUILocation
		{
			get
			{
				return "Registration Numbers on the Config Tab in the CTO Master File";
			}
		}

		protected override string Terminal1StopCodeInvalidGUILocation
		{
			get
			{
				return "Registration Numbers on the Config Tab in the CTO Master File";
			}
		}

		protected override string PortOfDischargeGUILocation
		{
			get
			{
				return "Consol Tab";
			}
		}

		protected override string ContainerNumberGUILocation
		{
			get
			{
				return "Containers Tab in the Grid";
			}
		}

		protected override string ISOContainerTypeGUILocation
		{
			get
			{
				return "Containers Tab in the Grid";
			}
		}

		protected override string Commodity1StopCodeGUILocation
		{
			get
			{
				return "Containers Tab in the Grid";
			}
		}

		protected override string ContainerGrossWeightGUILocation
		{
			get
			{
				return "Containers Tab in the Grid";
			}
		}

		protected override string ContainerGrossWeightVerificationGUILocation
		{
			get
			{
				return "VGM Tab on the Containers Tab";
			}
		}

		protected override string SealNumberGUILocation
		{
			get
			{
				return "Containers Tab in the Grid";
			}
		}

		protected override string GrossWeightVerifiedDeclarantLocation
		{
			get
			{
				return ForwardingConsol.IsDirect
					? "Details Tab in the Shipment Consignor Master File"
					: "Staff Details in the Staff Master File";
			}
		}

		#endregion

		#region Implementation

		Transport FindMainTransport(CommonContainer container)
		{
			Transport mainTransport = null;

			if (container != null && Container.Consol != null)
			{
				if (container.Consol.Transports.Count == 1)
				{
					mainTransport = container.Consol.Transports[0];
				}
				else
				{
					foreach (Transport transport in container.Consol.Transports)
					{
						if (transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
						{
							mainTransport = transport;
							break;
						}
					}
				}
			}
			return mainTransport;
		}

		#endregion
	}
}
