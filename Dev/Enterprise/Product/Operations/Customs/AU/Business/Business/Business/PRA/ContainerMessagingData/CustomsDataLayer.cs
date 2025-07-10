using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsDataLayer : ContainerMessagingData
	{
		#region Interface

		public CustomsDataLayer(CusContainer container)
			: base(container)
		{
			this.Container = Argument.NotNull(container, "container");
		}
		protected readonly CusContainer Container;

		public override EDIMessage GetEDIMessage()
		{
			return Container.PRAMessages.AddNew();
		}

		public override bool ContainerIsWaitingForResponse
		{
			get
			{
				return Container.IsWaitingForResponse;
			}
		}

		public void LoadFromContainer(DangerousGoodsCollection collection)
		{
			if (Container != null)
			{
				foreach (Customs.Business.CusContainerInvoiceLinePivot pivot in Container.InvoiceLinePivotCollection)
				{
					Container.Factory.AddFetchHint(typeof(JobComInvoiceLine), pivot.C2_JI);
				}

				foreach (Customs.Business.CusContainerInvoiceLinePivot pivot in Container.InvoiceLinePivotCollection)
				{
					var invoiceLine = Container.Factory.Load<JobComInvoiceLine>(pivot.C2_JI);
					foreach (var dgItem in invoiceLine.UNDGs)
					{
						if (dgItem.Substance != null)
						{
							var goods = collection.AddNew();

							goods.IMDGClass = dgItem.Substance.DG_Class;
							goods.UNDGNumber = dgItem.Substance.DG_UNNO;
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
							goods.PackingGroup = dgItem.Substance.DG_PG;                             // If required.
							goods.TechnicalName = dgItem.Substance.DG_PSN;
							goods.IMDGCodeVersion = dgItem.Substance.DG_Variant;
							goods.Weight = Core.Constants.Weight.Convert(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ, Core.Constants.Weight.Kilograms);

							var contact = dgItem.DGContact;
							if (contact != null)
							{
								goods.ContactName = contact.OC_ContactName;
								goods.ContactPhoneNumber = contact.OC_Phone;
								goods.ContactFaxNumber = contact.OC_Fax;
								goods.ContactEmailAddress = contact.OC_Email;
							}
						}
					}
				}
			}
		}

		#endregion

		#region FieldMappers

		#region JobDeclaration Level FieldMappers

		protected override string GetPortOfLoading()
		{
			return JobDeclaration.JE_RL_NKPortOfLoading;
		}

		protected override string GetPortOfDischarge()
		{
			return JobDeclaration.JE_RL_NKPortOfArrival;
		}

		protected override string GetPortOfFinalDischarge()
		{
			return JobDeclaration.JE_RL_NKFinalDestination;
		}

		protected override string GetVesselName()
		{
			return JobDeclaration.JE_VesselName;
		}

		protected override string GetVoyage()
		{
			return JobDeclaration.JE_VoyageFlightNo;
		}

		protected override string GetLloydsNumber()
		{
			return JobDeclaration.VesselNumber;
		}

		protected override string GetECNorCRN()
		{
			return JobDeclaration.DeclarationNumber;
		}

		protected override string GetConsignorName()
		{
			ZString result = "";
			if (JobDeclaration.Forwarder != null)
			{
				result = JobDeclaration.Forwarder.OH_FullName;
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

		#endregion

		#region Container FieldMappers

		protected override string GetContainerNumber()
		{
			return Container.CO_ContainerNumber;
		}

		protected override string GetSealNumber()
		{
			return Container.CO_Seal;
		}

		protected override decimal GetContainerGrossWeight()
		{
			return Container.GrossWeight;
		}

		protected override decimal GetContainerTareWeight()
		{
			return RefContainer.RC_TareWeight;
		}

		protected override decimal GetContainerNetWeight()
		{
			return ContainerTareWeight == 0m ? 0m : ContainerGrossWeight - ContainerTareWeight;
		}

		protected override bool GetIsTempControlled()
		{
			var result = false;
			if (Container != null)
			{
				result = Container.IsControlledAtmosphere;
				if (Container.Container == null)
				{
					result = false;
				}
				else
				{
					if (Container.Container.RC_ContainerType != Core.Constants.ContainerTypes.Refrigerated)
					{
						result = false;
					}
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
			return ""; // Removed at RW's Request - Container.JC_DepartureTruckRegistration;
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
			return CartageCompany.PrimaryRegistrationNumber.Number;
		}

		protected override string GetCartageBookingReference()
		{
			return ""; // Was Container.JC_BookingReference, but is deprecated and RW doesn't think we need this.;
		}

		#endregion

		#region Other DataMappers

		protected override string GetMessageReference()
		{
			return "CUS-" + JobDeclaration.JE_DeclarationReference + "-" + Container.CO_ContainerNumber;
		}

		protected override string GetGoodsDescription()
		{
			return RefCommodityCode.RH_DescriptionMultilingual;
		}

		protected override string GetDateTimeStringForMessage()
		{
			return ZDateTime.Now.ToString("yyyyMMddHHmmss");
		}

		protected override string GetGrossWeightVerifiedDeclarantSignature()
		{
			return SenderContactName;
		}

		protected override string GetGrossWeightVerifiedDeclarantContact()
		{
			var result = new ZStringBuilder();
			result.Append(SenderContactName ?? string.Empty);
			result.Append(SenderCompanyName ?? string.Empty);
			result.Append(SenderPhone ?? string.Empty);
			result.Append(SenderEmail ?? string.Empty);
			return result.ToStringWithDelimiterBetweenAppends(";");
		}

		protected override string GetGrossWeightDeclarantCompanyName()
		{
			return SenderCompanyName;
		}

		#endregion

		#region Dangerous Goods

		protected override DangerousGoodsCollection GetDangerousGoodsList()
		{
			var result = new DangerousGoodsCollection();
			this.LoadFromContainer(result);
			return result;
		}

		#endregion

		#endregion

		#region CheckMissingData

		protected override string ShippingLineBookingReferenceGUILocation
		{
			get
			{
				return "Export Process Tab on the Containers Tab";
			}
		}

		protected override string ECNorCRNGUILocation
		{
			get
			{
				return "Entry Number. (Against Declaration)\r\n";
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
				return "Declaration Tab";
			}
		}

		protected override string VoyageGUILocation
		{
			get
			{
				return "Declaration Tab";
			}
		}

		protected override string LloydsNumberGUILocation
		{
			get
			{
				return "Vessel Master File for selected vessel on Declaration Tab";
			}
		}

		protected override string PortOfLoadingGUILocation
		{
			get
			{
				return "Declaration Tab";
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
				return "Declaration Tab";
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
				return "Staff Details in the Staff Master File";
			}
		}

		#endregion

		#region Container Child Business Object Lazy Loaders

		#region JobContainer

		protected override CommonContainer GetJobContainer()
		{
			return Container.JobContainer;
		}

		#endregion

		#region RefCommodityCode

		protected RefCommodityCode RefCommodityCode
		{
			get
			{
				if (refCommodityCode == null)
				{
					refCommodityCode = SavedFactory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, Container.RH_NKContainerCommodityCode);
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
				refContainer = SavedFactory.Load<RefContainer>(Container.CO_RC);
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
					shippingLine = JobDeclaration.ShippingLine;
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

		#region CartageCompany

		protected OrgHeader CartageCompany
		{
			get
			{
				if (cartageCompany == null)
				{
					cartageCompany = SavedFactory.Load<OrgHeader>(JobDeclaration.DeliveryOrPickupCartageCoPK);
				}
				if (cartageCompany == null)
				{
					cartageCompany = SavedFactory.GetNull<OrgHeader>();
				}
				return cartageCompany;
			}
		}
		OrgHeader cartageCompany;

		#endregion

		#region CTO

		protected OrgHeader CTO
		{
			get
			{
				if (cto == null && JobDeclaration.ContainerTerminalOperatorDocAddress.Organisation != null)
				{
					cto = JobDeclaration.ContainerTerminalOperatorDocAddress.Organisation;
				}
				return cto;
			}
		}
		OrgHeader cto;

		#endregion

		#region JobDeclaration

		protected JobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Container.JobDeclaration;
				}
				if (jobDeclaration == null)
				{
					jobDeclaration = SavedFactory.GetNull<JobDeclaration>();
				}
				return jobDeclaration;
			}
		}
		protected JobDeclaration jobDeclaration;

		#endregion

		#endregion
	}
}
