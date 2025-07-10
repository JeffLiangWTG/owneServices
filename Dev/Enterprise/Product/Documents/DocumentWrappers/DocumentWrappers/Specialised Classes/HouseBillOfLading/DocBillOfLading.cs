using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Resources.Handlers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocBillOfLading : NonPersistentBusinessObject, IFormedPagesSupporter, IObsoleteValidation
	{
		public DocBillOfLading(DocForwardingShipment shipmentWrapper)
			: base(shipmentWrapper.Factory)
		{
			ShipmentWrapper = shipmentWrapper;
		}

		protected internal DocForwardingShipment ShipmentWrapper { get; private set; }

		public static new ZString TableName
		{
			get { return "BillOfLadingBody"; }
		}

		#region Addresses

		public ZString ConsignorAddress
		{
			get
			{
				return ShipmentWrapper.IsManufacturerBillOfLading
					? ShipmentWrapper.ManufacturerAddress?.PostalAddressInEnglish ?? ZString.Empty
					: ShipmentWrapper?.DocsAndCartage?.ConsignorDocumentaryAddress?.PostalAddressInEnglish ?? ZString.Empty;
			}
		}

		public ZString ConsigneeAddress
		{
			get
			{
				return ShipmentWrapper.IsManufacturerBillOfLading
					? ShipmentWrapper?.DocsAndCartage?.ConsignorDocumentaryAddress?.PostalAddressInEnglish ?? ZString.Empty
					: ShipmentWrapper?.DocsAndCartage?.ConsigneeDocumentaryAddress?.PostalAddressInEnglish ?? ZString.Empty;
			}
		}

		public DocOrganisation DeliveryAgent
		{
			get
			{
				if (ShipmentWrapper.DeliveryAgent != null)
				{
					return ShipmentWrapper.DeliveryAgent;
				}

				DocShipmentConsol arrivalConsol = DocShipmentConsol.New(ShipmentWrapper.ArrivalCommonConsol, Factory) ?? ShipmentWrapper.Consol;

				if (arrivalConsol != null)
				{
					return arrivalConsol.ReceivingForwarder;
				}

				return null;
			}
		}

		public ZString NotifyParty
		{
			get
			{
				if (ShipmentWrapper.NotifyParty != null)
				{
					return ShipmentWrapper.NotifyParty.PostalAddressInEnglish;
				}

				return Env.Registry.NotifyPartyDefaultText;
			}
		}

		public ZString ChargesTo
		{
			get
			{
				ZString result = "";

				if (ShipmentWrapper.JobHeader != null)
				{
					if (ShipmentWrapper.JobHeader.LocalCharges != null && ShipmentWrapper.IsPrepaid)
					{
						result = ShipmentWrapper.JobHeader.LocalCharges.PostalAddressInEnglish;
					}
					else if (ShipmentWrapper.JobHeader.AgentCollect != null && ShipmentWrapper.IsCollect)
					{
						result = ShipmentWrapper.JobHeader.AgentCollect.PostalAddressInEnglish;
					}
				}

				return result;
			}
		}

		public DocDocAddress SendingForwarderAddress
		{
			get
			{
				DocDocAddress result = null;

				if (FreightDataRegistry.Instance.BOLSendingForwarderCurrentBranchOrgProxy.Value && GlbBranch.CurrentBranch.OrgProxy != null)
				{
					result = DocDocAddress.New(GlbBranch.CurrentBranch.OrgProxy.Addresses.MainAddress, Factory);
				}
				else if (ShipmentWrapper.Consol != null && ShipmentWrapper.SendingForwarder != null)
				{
					result = ShipmentWrapper.SendingForwarder.SelectedAddress;
				}

				return result;
			}
		}

		#endregion

		#region Other Properties

		public ZString MasterHouseBillNumberHeading
		{
			get
			{
				if (ShipmentWrapper.IsCoload || ShipmentWrapper.ColoadShipments.Count > 0)
				{
					return (NoResString)"MASTER HBL: ";
				}

				return ZString.Empty;
			}
		}

		public ZString HouseBillNumber
		{
			get
			{
				if (ShipmentWrapper.IsCoload || ShipmentWrapper.ColoadShipments.Count > 0)
				{
					if (ShipmentWrapper.ShowMasterHeadingWithBillNumber == 0)
					{
						return ShipmentWrapper.HouseBill;
					}

					return MasterHouseBillNumberHeading + ShipmentWrapper.HouseBill;
				}
				else
				{
					return ShipmentWrapper.HouseBill;
				}
			}
		}

		public ZString HBLTypeDescription
		{
			get
			{
				ZString result = ZString.Empty;

				if (HBLType != null)
				{
					result = HBLType.Description;
				}

				return result;
			}
		}

		public ZString Weight
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal bolWeight = GetBillOfLadingMeasurementValue(ShipmentWrapper.ActualWeight, ShipmentWrapper.DocumentedWeight, ShipmentWrapper.ManifestedWeight);

				if (ShipmentWrapper.ConvertUnits && ShipmentWrapper.UnitOfWeight != ShipmentWrapper.BOLWeightUnit && ShipmentWrapper.UnitOfWeight != ZString.Empty)
				{
					ZDecimal convertedWeight = Core.Constants.Weight.ConvertSafe(bolWeight, ShipmentWrapper.UnitOfWeight, ShipmentWrapper.BOLWeightUnit);
					result = ShipmentWrapper.FormatNumber(convertedWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay) + " " + ShipmentWrapper.BOLWeightUnit + "\n";
					result += "(" + bolWeight.ToStringTrimZeros() + " " + ShipmentWrapper.UnitOfWeight + ")\n";
				}
				else if (bolWeight != 0M)
				{
					result = ShipmentWrapper.FormatNumber(bolWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay) + " " + ShipmentWrapper.UnitOfWeight + "\n";
				}

				return result;
			}
		}

		public ZString Volume
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal bolVolume = GetBillOfLadingMeasurementValue(ShipmentWrapper.ActualVolume, ShipmentWrapper.DocumentedVolume, ShipmentWrapper.ManifestedVolume);

				if (ShipmentWrapper.ConvertUnits && ShipmentWrapper.UnitOfVolume != ShipmentWrapper.BOLVolumeUnit && ShipmentWrapper.UnitOfVolume != ZString.Empty)
				{
					ZDecimal convertedVolume = Core.Constants.Volume.ConvertSafe(bolVolume, ShipmentWrapper.UnitOfVolume, ShipmentWrapper.BOLVolumeUnit);
					result = ShipmentWrapper.FormatNumber(convertedVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay) + " " + ShipmentWrapper.BOLVolumeUnit + "\n";
					result += "(" + bolVolume.ToStringTrimZeros() + " " + ShipmentWrapper.UnitOfVolume + ")\n";
				}
				else if (bolVolume != 0M)
				{
					result = ShipmentWrapper.FormatNumber(bolVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay) + " " + ShipmentWrapper.UnitOfVolume + "\n";
				}

				return result;
			}
		}

		public ZInt CountOfContainers
		{
			get
			{
				BuildGoodsDescriptionColumn();
				return fCountOfContainers;
			}
		}

		ZInt fCountOfContainers;

		public ZInt CountOfLCLPackages
		{
			get
			{
				BuildGoodsDescriptionColumn();
				return fCountOfLCLPackages;
			}
		}

		ZInt fCountOfLCLPackages;

		public ZString LCLPackagesType
		{
			get
			{
				BuildGoodsDescriptionColumn();
				return fLCLPackagesType;
			}
		}

		ZString fLCLPackagesType;

		public ZString ForwardingAgent
		{
			get
			{
				return IssuedByName + "\n"
					+ IssuedByAddress1 + " " + IssuedByAddress2 + " "
					+ IssuedByCity + " " + IssuedByState + " " + IssuedByPostCode + " "
					+ IssuedByCountry;
			}
		}

		BillIssuedBy IssuedBy
		{
			get
			{
				if (issuedBy == null)
				{
					RefUNLOCO uNLOCO = null;

					if (ShipmentWrapper.Consol != null && ShipmentWrapper.Consol.PortOfLoading != null)
					{
						uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, ShipmentWrapper.Consol.PortOfLoading.Code));
					}

					issuedBy = new BillIssuedBy(uNLOCO);
				}

				return issuedBy;
			}
		}
		BillIssuedBy issuedBy;

		public ZString SingleContainerMode
		{
			get
			{
				ZString result = ShipmentWrapper.HBLContainerPackModeOverride;
				if (result.IsEmpty)
				{
					ZString[] containerModesFromContainers = new ZString(ContainerModeColumn + FollowOnContainerModeColumn).Split('\n');
					foreach (ZString mode in containerModesFromContainers)
					{
						ZString modeWithoutAsterisks = mode.TrimEnd('*');
						modeWithoutAsterisks = modeWithoutAsterisks.TrimEnd('-');

						if (result.IsEmpty)
						{
							result = modeWithoutAsterisks;
						}
						else if (!modeWithoutAsterisks.IsEmpty && result != modeWithoutAsterisks)
						{
							result = "";
							break;
						}
					}
				}
				return result;
			}
		}

		public ZString ShipperLoadAndCountDefault
		{
			get
			{
				return FreightDataRegistry.Instance.ShipperLoadAndCount.Value;
			}
		}

		public ZString MiniBillNamePreprinted
		{
			get
			{
				if (this.IsPreprinted)
				{
					return "(PP)";
				}

				return ZString.Empty;
			}
		}

		public ZBool Containerized
		{
			get
			{
				return ShipmentWrapper != null && (ShipmentWrapper.PackingMode == "FCL" || ShipmentWrapper.PackingMode == "BCN");
			}
		}

		public ZString InterimReceipt
		{
			get
			{
				return (ShipmentWrapper != null) ? (string)ShipmentWrapper.InterimReceipt : "";
			}
		}

		public ZString Title
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsPreprinted)
				{
					result = (IsSeaWaybill) ? DocConstants.Resources.BillTitles.SWB : DocConstants.Resources.BillTitles.HBL;
				}
				return result;
			}
		}

		public ZString BillSurrenderedToHeading
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsPreprinted)
				{
					result = (IsSeaWaybill) ? DocConstants.Resources.BillTerms.DeliveryAgentHeading : DocConstants.Resources.BillTerms.BillSurrenderedToHeading;
				}
				return result;
			}
		}

		public ZString NotNegotiableText
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsPreprinted && !IsSeaWaybill)
				{
					result = DocConstants.Resources.BillTerms.NotNegotiableText;
				}
				return result;
			}
		}

		#endregion

		#region Body Sections

		public DocBillOfLadingBodySectionCollection BodySections
		{
			get
			{
				if (fBodySections == null)
				{
					fBodySections = new DocBillOfLadingBodySectionCollection(Factory);
					DocBillOfLadingBodySection bodySection;

					// Top part of BodySection
					ZString fullMarksAndNumbers = this.MarksAndNumbers + this.FollowOnMarksAndNumbers;
					ZString fullPackages = this.Packages;
					ZString fullGoodsDescription = this.GoodsDescription + this.FollowOnGoodsDescription;
					ZString fullWeight = this.Weight;
					ZString fullVolume = this.Volume;

					// Bottom part of BodySection
					ZString fullContainerNumbers = this.ContainerNumberColumn + this.FollowOnContainerNumberColumn;
					ZString fullContainerSeals = this.ContainerSealNumColumn + this.FollowOnContainerSealNumColumn;
					ZString fullContainerTypes = this.ContainerTypeColumn + this.FollowOnContainerTypeColumn;
					ZString fullContainerWeights = this.ContainerWeightColumn + this.FollowOnContainerWeightColumn;
					ZString fullContainerVolumes = this.ContainerVolumeColumn + this.FollowOnContainerVolumeColumn;
					ZString fullContainerPackages = this.ContainerPackagesColumn + this.FollowOnContainerPackagesColumn;
					ZString fullContainerModes = this.ContainerModeColumn + this.FollowOnContainerModeColumn;

					while (true)
					{
						bodySection = new DocBillOfLadingBodySection(Factory);

						// Top part
						bodySection.MarksAndNumbers = ExtractBlock(ref fullMarksAndNumbers, ShipmentWrapper.MarksAndNumbsAndDescHeight);
						bodySection.Packages = ExtractBlock(ref fullPackages, ShipmentWrapper.MarksAndNumbsAndDescHeight);
						bodySection.GoodsDescription = ExtractBlock(ref fullGoodsDescription, ShipmentWrapper.MarksAndNumbsAndDescHeight);
						bodySection.Weight = ExtractBlock(ref fullWeight, ShipmentWrapper.MarksAndNumbsAndDescHeight);
						bodySection.Volume = ExtractBlock(ref fullVolume, ShipmentWrapper.MarksAndNumbsAndDescHeight);

						// Bottom part
						bodySection.ContainerNumbers = ExtractBlock(ref fullContainerNumbers, ShipmentWrapper.NoOfContainerRows);
						bodySection.ContainerSeals = ExtractBlock(ref fullContainerSeals, ShipmentWrapper.NoOfContainerRows);
						bodySection.ContainerTypes = ExtractBlock(ref fullContainerTypes, ShipmentWrapper.NoOfContainerRows);
						bodySection.ContainerWeights = ExtractBlock(ref fullContainerWeights, ShipmentWrapper.NoOfContainerRows);
						bodySection.ContainerVolumes = ExtractBlock(ref fullContainerVolumes, ShipmentWrapper.NoOfContainerRows);
						bodySection.ContainerPackages = ExtractBlock(ref fullContainerPackages, ShipmentWrapper.NoOfContainerRows);
						bodySection.ContainerModes = ExtractBlock(ref fullContainerModes, ShipmentWrapper.NoOfContainerRows);

						if (bodySection.IsEmpty)
						{
							break;
						}
						else
						{
							fBodySections.Add(bodySection);
						}
					}
				}

				return fBodySections;
			}
		}

		DocBillOfLadingBodySectionCollection fBodySections;

#if DEBUG
		protected
#endif
 ZString ExtractBlock(ref ZString source, int blockSize)
		{
			ZString result = ZString.Empty;

			if (source.Length <= blockSize)
			{
				result = source;
				source = ZString.Empty;

				return result;
			}

			ZString[] sourceSplitByRow = source.Split('\n');

			for (int i = 0; i < blockSize; i++)
			{
				if (i < sourceSplitByRow.Length)
				{
					result += sourceSplitByRow[i] + "\n";
				}
			}

			source = source.SubstringSafe(result.Length, source.Length - result.Length);

			return result;
		}

		#endregion

		#region Images

		#region BillTerms

		public Image BillTermsImage
		{
			get
			{
				if (fBillTermsImage != null && fBillTermsImage.IsDisposed())
				{
					fBillTermsImage = null;
				}

				if (!IsPreprinted && fBillTermsImage == null)
				{
					string postFix = (IsSeaWaybill) ? "_SWB.gif" : "_HBL.gif";
					ZString imagePath = BillTermsImagePath + ShipmentWrapper.HBLCode + postFix;
					fBillTermsImage = GetImageHandler().GetImageWithResourcePath(imagePath);
				}
				return fBillTermsImage;
			}
		}
		Image fBillTermsImage;

		protected virtual ImageHandler GetImageHandler()
		{
			return new ImageHandler();
		}

		protected virtual ZString BillTermsImagePath
		{
			get
			{
				return DocConstants.Resources.BillTerms.StandardBillTermsBasePath;
			}
		}

		#endregion

		#region Logos

		#region House Bill Logos

		public Image Logo
		{
			get
			{
				if (fLogo == null || fLogo.IsDisposed())
				{
					fLogo = ShipmentWrapper.GetHouseBillBrandImage(DeliveryAgent, ShipmentWrapper.Consignor, DocumentsDataRegistry.Instance.HBLAgentBrandingImage);

					if (fLogo == null || fLogo.IsDisposed())
					{
						if (FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.Value)
						{
							if (HBLType != null && HBLType.PrintLogo)
							{
								using (SetControllingBranchContextIfNecessary())
								{
									fLogo = (HBLType.LogoImage != null) ? HBLType.LogoImage.Image : null;
								}
							}
						}
						else
						{
							if (!ShipmentWrapper.MenuTitle.ToUpper().Contains("PREPRINTED"))
							{
								if (HBLType == null || HBLType.PrintLogo)
								{
									using (SetControllingBranchContextIfNecessary())
									{
										fLogo = Env.Registry.HouseBillOfLadingLogo;
									}
								}
							}
						}
					}
				}
				return fLogo;
			}
		}
		Image fLogo;

		public Image HouseBillLogo
		{
			get { return ShipmentWrapper.MenuTitle.ToUpper().Contains("PREPRINTED") ? null : Env.Registry.HouseBillOfLadingLogo; }
		}

		#endregion

		#region FIATA Specific

		#region FIATA Text Logo
		public Image FIATATextLogo
		{
			get
			{
				if (fFIATATextLogo != null && fFIATATextLogo.IsDisposed())
				{
					fFIATATextLogo = null;
				}

				if (!IsPreprinted && fFIATATextLogo == null)
				{
					var fiataLogoProvider = new FIATALogoProvider();
					fFIATATextLogo = fiataLogoProvider.GetFIATATextLogo(IsSeaWaybill);
				}
				return fFIATATextLogo;
			}
		}
		Image fFIATATextLogo;
		#endregion

		#region FIATA Graphic Logo
		public Image FIATALogo
		{
			get
			{
				if (fFIATALogo != null && fFIATALogo.IsDisposed())
				{
					fFIATALogo = null;
				}

				if (!IsPreprinted && fFIATALogo == null)
				{
					var fiataLogoProvider = new FIATALogoProvider();
					fFIATALogo = fiataLogoProvider.GetFIATALogo(GlbBranch.CurrentBranch?.Country?.Code);
				}

				return fFIATALogo;
			}
		}
		Image fFIATALogo;
		#endregion

		#endregion

		#endregion

		#region Terms & Conditions

		protected virtual ZBool ShowTCImageCore
		{
			get { return TCImage != null; }
		}

		public ZBool ShowTCImage
		{
			get { return ShowTCImageCore; }
		}

		public Image TCImage
		{
			get
			{
				if (fTCImage != null && fTCImage.IsDisposed())
				{
					fTCImage = null;
				}

				if (fTCImage == null)
				{
					if (HBLType != null)
					{
						if (!HBLType.PrePrinted)
						{
							HouseBillOfLadingTermsAndConditions tC = null;

							if (IsSeaWaybill)
							{
								if (ShipmentWrapper.HouseBillOfLadingType == "FIA" && FreightDataRegistry.Instance.FIATAAuthorised.Value)
								{
									tC = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value.FindByCodeForDeliveryModeALL("FWB");
								}
								else
								{
									tC = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value.FindByCodeForDeliveryModeALL("SWB");
								}
							}
							else
							{
								tC = HBLType.GetTermsAndConditionsImage(ShipmentWrapper.DocumentDeliveryMode);
							}

							if (tC != null)
							{
								fTCImage = tC.Image;
							}
						}
					}
				}

				return fTCImage;
			}
		}

		Image fTCImage;

		#endregion

		#region HBL Type

		public ZBool IsSeaWaybill
		{
			get
			{
				return ShipmentWrapper.ReleaseTypeCode == Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			}
		}

		public ZBool IsPreprinted
		{
			get
			{
				return (HBLType != null) ? HBLType.PrePrinted : ZBool.False;
			}
		}

		IDisposable SetControllingBranchContextIfNecessary()
		{
			IDisposable branchContext = null;
			if (!SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.Value)
			{
				JobHeader jobHeader = ShipmentWrapper.CommonShipment.ShipmentJobHeader;
				if (jobHeader != null && !jobHeader.JH_GB.IsEmpty && jobHeader.JH_GB != Env.CurrentBranch.PK)
				{
					branchContext = new TemporaryUserContext() { BranchPK = jobHeader.JH_GB.ToGuid() }.Set();
				}
			}

			return branchContext;
		}

		HouseBillOfLadingType HBLType
		{
			get
			{
				if (fHBLType == null)
				{
					if (FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.Value)
					{
						using (SetControllingBranchContextIfNecessary())
						{
							fHBLType = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value.FindByCode(ShipmentWrapper.HouseBillOfLadingType) as HouseBillOfLadingType;
						}
					}
					else
					{
						var defaultHBLRegistryCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.DefaultValue;
						if (defaultHBLRegistryCollection != null)
						{
							fHBLType = defaultHBLRegistryCollection.FindByCode(ShipmentWrapper.HouseBillOfLadingType) as HouseBillOfLadingType;
						}
					}
				}

				return fHBLType;
			}
		}

		HouseBillOfLadingType fHBLType;

		#endregion

		#region Signature

		public Image SignatureImage
		{
			get
			{
				if (signatureImage == null || signatureImage.IsDisposed())
				{
					signatureImage = null;

					if (ShouldPrintUserSignature)
					{
						var currentUser = GlbStaff.CurrentUser;
						signatureImage = currentUser.SignatureImage;

						if (signatureImage != null)
						{
							var usageDetailsCollector = Factory?.ServiceContainer.GetService<DocumentUsageDetailsCollector>();
							usageDetailsCollector?.GetIsUserSignatureUsed(true);
						}
					}
				}

				return signatureImage;
			}
		}

		Image signatureImage;

		public ZBool ShouldPrintUserSignature
		{
			get
			{
				return FreightDataRegistry.Instance.PrintSignatureForHBLDocuments.Value;
			}
		}

		#endregion

		public string[] ImageNamesToRemove
		{
			get
			{
				if (HBLType != null && HBLType.PrePrinted)
				{
					return new string[] { "BillOfLading.FaceImage" };
				}
				return Array.Empty<string>();
			}
		}

		#endregion

		public DocOrganisation CarriersAgentForPortOfLoading
		{
			get { return carriersAgentForPortOfLoading ?? (carriersAgentForPortOfLoading = GetCarriersAgentForPortOfLoading()); }
		}
		DocOrganisation carriersAgentForPortOfLoading;

		DocOrganisation GetCarriersAgentForPortOfLoading()
		{
			DocOrganisation docOrganisation = null;

			if (ShipmentWrapper.Carrier != null && ShipmentWrapper.Carrier.OrgHeader != null && PortOfLoading != null)
			{
				OrgCarrierAppointedAgentPorts agentPort = FindAgent(ShipmentWrapper.Carrier.OrgHeader, PortOfLoading.Code);
				docOrganisation = agentPort != null ? DocOrganisation.New(Factory, agentPort.OrganisationPK) : null;
			}

			return docOrganisation;
		}

		OrgCarrierAppointedAgentPorts FindAgent(OrgHeader orgHeader, ZString port)
		{
			OrgCarrierAppointedAgentPorts agentPort = null;

			if (orgHeader != null)
			{
				if (port.Length == 5)
				{
					agentPort = orgHeader.CarrierAppointedAgentPorts_Agency.Cast<OrgCarrierAppointedAgentPorts>().FirstOrDefault(agent => agent.O5_PortOrCountry == port);
				}

				if (agentPort == null && port.Length >= 2)
				{
					ZString portCountry = port.SubstringSafe(0, 2);
					agentPort = orgHeader.CarrierAppointedAgentPorts_Agency.Cast<OrgCarrierAppointedAgentPorts>().FirstOrDefault(agent => agent.O5_PortOrCountry == portCountry);
				}
			}

			return agentPort;
		}

		#region Port of Loading and Discharge

		public DocUNLOCO PortOfLoading
		{
			get
			{
				if (ShipmentWrapper.IsManufacturerBillOfLading)
				{
					return ShipmentWrapper.ManufacturerAddress.Port;
				}

				if (ShipmentWrapper.IsBooking && !ShipmentWrapper.IsForwardRegistered)
				{
					return DocUNLOCO.New(Factory, BookingPortOfLoading);
				}

				if (PreCarriageLeg != null)
				{
					return PreCarriageLeg.PortOfLoading;
				}

				if (MainVesselLeg != null)
				{
					return MainVesselLeg.PortOfLoading;
				}

				if (ShipmentWrapper.Consol != null)
				{
					return DocUNLOCO.New(Factory, ShipmentWrapper.Consol.NKPortOfLoading);
				}

				return null;
			}
		}

		public ZString PortOfLoadingDefault
		{
			get
			{
				return (PortOfLoading != null) ? PortOfLoading.PortNameAndCountryNameInEnglish.ToUpper() : ZString.Empty;
			}
		}

		public DocUNLOCO PortOfDischarge
		{
			get
			{
				if (ShipmentWrapper.IsBooking && !ShipmentWrapper.IsForwardRegistered)
				{
					return DocUNLOCO.New(Factory, BookingPortOfDischarge);
				}

				if (MainVesselLeg != null)
				{
					return MainVesselLeg.PortOfDischarge;
				}

				if (ShipmentWrapper.Consol != null)
				{
					return DocUNLOCO.New(Factory, ShipmentWrapper.Consol.NKPortOfDischarge);
				}

				return null;
			}
		}

		public ZString PortOfDischargeDefault
		{
			get
			{
				return (PortOfDischarge != null) ? PortOfDischarge.PortNameAndCountryNameInEnglish.ToUpper() : ZString.Empty;
			}
		}

		public ZString PlaceOfIssue => ShipmentWrapper.IsManufacturerBillOfLading
			? DocUNLOCO.New(Factory, GlbBranch.CurrentBranch.GB_RL_NKHomePort).PortNameAndCountryNameInEnglish.ToUpper()
			: PortOfLoadingDefault;

		#endregion

		#region Transport Legs

		public DocTransportCollection CompleteRouting
		{
			get
			{
				DocTransportCollection coll = new DocTransportCollection(Factory);
				RoutingCollection transports = ((IRoutingSupport)ShipmentWrapper.CommonShipment).TransportsIncludingRelated;

				if (transports.Count == 0 && ShipmentWrapper.CommonShipment.JS_JX.IsValid)
				{
					JobSailing sailing = Factory.Load<JobSailing>(ShipmentWrapper.CommonShipment.JS_JX);
					coll.Add(DocTransport.New(sailing, Factory));
				}
				else
				{
					Transport[] sortedTransports = (Transport[])transports.ToArray(typeof(Transport));
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);

					foreach (Transport transport in sortedTransports)
					{
						CommonConsol consol = transport.Parent as CommonConsol;
						if (consol != null)
						{
							coll.Add(DocTransport.New(consol, transport, Factory));
						}
						else
						{
							coll.Add(DocTransport.New(ShipmentWrapper.CommonShipment, transport, Factory));
						}
					}
				}

				return coll;
			}
		}

		#region Pre-Carriage

		public ZString PreCarriageVesselVoyage
		{
			get
			{
				return PreCarriageLeg != null ? PreCarriageLeg.VesselVoyageFlight : ZString.Empty;
			}
		}

		public DocTransport PreCarriageLeg
		{
			get
			{
				DocTransportCollection preCarriages = GetTransportPlanningForType(Core.Constants.TransportPlanningType.PreCarriage);
				if (preCarriages.Count == 1)
				{
					return preCarriages[0];
				}
				else
				{
					DocTransport preLeg = null;
					foreach (DocTransport currentLeg in preCarriages)
					{
						preLeg = currentLeg;
						foreach (DocTransport aLeg in preCarriages)
						{
							if (!currentLeg.PortOfLoadingCode.IsEmpty && currentLeg.PortOfLoadingCode == aLeg.PortOfDischargeCode)
							{
								preLeg = null;
								break;
							}
						}

						if (preLeg != null)
						{
							break;
						}
					}

					return preLeg;
				}
			}
		}

		#endregion

		#region Main

		public ZString VesselHeading
		{
			get
			{
				if (HBLType != null && HBLType.PrePrinted)
				{
					return ZString.Empty;
				}
				else
				{
					if (TransportMode.ToUpper() == Core.Constants.TransportModes.Rail.ToUpper())
					{
						return (NoResString)"Journey";
					}
					else if (TransportMode.ToUpper() == Core.Constants.TransportModes.Road.ToUpper())
					{
						return ZString.Empty;
					}
					else
					{
						return (NoResString)"Vessel";
					}
				}
			}
		}

		public ZString VoyageHeading
		{
			get
			{
				if (HBLType != null && HBLType.PrePrinted)
				{
					return ZString.Empty;
				}
				else
				{
					if (TransportMode.ToUpper() == Core.Constants.TransportModes.Rail.ToUpper())
					{
						return (NoResString)"Journey No.";
					}
					else if (TransportMode.ToUpper() == Core.Constants.TransportModes.Road.ToUpper())
					{
						return (NoResString)"Truck Ref.";
					}
					else
					{
						return (NoResString)"Voyage";
					}
				}
			}
		}

		public ZString TransportMode
		{
			get
			{
				if (MainVesselLeg != null)
				{
					return MainVesselLeg.TransportMode;
				}

				if (ShipmentWrapper.Consol != null)
				{
					return ShipmentWrapper.Consol.TransportMode;
				}

				return ShipmentWrapper.TransportMode;
			}
		}

		public ZString VesselName
		{
			get
			{
				if (ShipmentWrapper.IsBooking && !ShipmentWrapper.IsForwardRegistered)
				{
					return BookingVesselName;
				}

				if (MainVesselLeg != null)
				{
					return MainVesselLeg.VesselName;
				}

				if (ShipmentWrapper.Consol != null)
				{
					return ShipmentWrapper.Consol.VesselName;
				}

				return ZString.Empty;
			}
		}

		public ZString VoyageNumber
		{
			get
			{
				if (ShipmentWrapper.IsBooking && !ShipmentWrapper.IsForwardRegistered)
				{
					return BookingVoyage;
				}

				if (MainVesselLeg != null)
				{
					return MainVesselLeg.VoyageFlight;
				}

				if (ShipmentWrapper.Consol != null)
				{
					return ShipmentWrapper.Consol.VoyageNumber;
				}

				return ZString.Empty;
			}
		}

		public ZString MainVesselVoyage
		{
			get
			{
				if (ShipmentWrapper.IsBooking && !ShipmentWrapper.IsForwardRegistered)
				{
					return VesselName + VesselVoyageDelimiter + VoyageNumber;
				}

				return MainVesselLeg != null ? MainVesselLeg.VesselVoyageFlight : ZString.Empty;
			}
		}

		ZString VesselVoyageDelimiter
		{
			get { return (!VesselName.IsEmpty && !VoyageNumber.IsEmpty) ? " / " : ""; }
		}

		public DocTransport MainVesselLeg
		{
			get
			{
				if (ShipmentWrapper.Consol != null && ShipmentWrapper.Consol.MainVesselLeg != null)
				{
					return ShipmentWrapper.Consol.MainVesselLeg;
				}

				DocTransport firstMainTransport = null;
				DocTransport firstMainTransportInThisCountry = null;

				foreach (DocTransport transport in CompleteRouting)
				{
					if (transport.TransportType == Core.Constants.TransportPlanningType.MainVessel && !transport.VesselName.IsEmpty)
					{
						if (firstMainTransport == null || (firstMainTransport.Shipment == null && transport.Shipment != null))
						{
							firstMainTransport = transport;
						}

						if (transport.PortOfLoading != null && transport.PortOfLoading.CountryCode == GlbBranch.CurrentBranch.Country.Code)
						{
							firstMainTransportInThisCountry = firstMainTransportInThisCountry ?? transport;
							if (transport.Shipment != null)
							{
								return transport;
							}
						}
					}
				}

				return firstMainTransportInThisCountry ?? firstMainTransport;
			}
		}

		#endregion

		#region OnForwarding

		public ZString OnForwardingPortOfLoading
		{
			get
			{
				return (OnForwardingLeg != null && OnForwardingLeg.PortOfLoading != null) ? ZString.Format("{0}, {1}", OnForwardingLeg.PortOfLoading.PortName, OnForwardingLeg.PortOfLoading.CountryName).ToUpper() : ZString.Empty;
			}
		}

		public ZString OnForwardingVesselVoyage
		{
			get
			{
				return OnForwardingLeg != null ? OnForwardingLeg.VesselVoyageFlight : ZString.Empty;
			}
		}

		public DocTransport OnForwardingLeg
		{
			get
			{
				DocTransportCollection onForwardings = GetTransportPlanningForType(Core.Constants.TransportPlanningType.OnForwarding);
				if (onForwardings.Count == 1)
				{
					return onForwardings[0];
				}
				else
				{
					DocTransport oNFLeg = null;
					foreach (DocTransport currentLeg in onForwardings)
					{
						oNFLeg = currentLeg;
						foreach (DocTransport aLeg in onForwardings)
						{
							if (!currentLeg.PortOfDischargeCode.IsEmpty && currentLeg.PortOfDischargeCode == aLeg.PortOfLoadingCode)
							{
								oNFLeg = null;
								break;
							}
						}

						if (oNFLeg != null)
						{
							break;
						}
					}

					return oNFLeg;
				}
			}
		}

		#endregion

#if DEBUG
		protected
#endif
 DocTransportCollection GetTransportPlanningForType(ZString transportType)
		{
			DocTransportCollection coll = new DocTransportCollection(Factory);
			foreach (DocTransport leg in CompleteRouting)
			{
				if (leg.TransportType == transportType.ToString())
				{
					coll.Add(leg);
				}
			}

			coll.Sort(new Comparison<DocTransport>(
				(DocTransport transport1, DocTransport transport2) =>
				{
					if (transport1.Shipment != null && transport2.Shipment != null)
					{
						return 0;
					}
					if (transport1.Shipment != null)
					{
						return -1;
					}
					else if (transport2.Shipment != null)
					{
						return 1;
					}
					return 0;
				}
			));

			return coll;
		}

		#endregion

		#region Law and Jurisdiction

		public ZString LawAndJurisdictionClauseForITC
		{
			get
			{
				ZString result = ZString.Empty;
				if (ShipmentWrapper.HouseBillOfLadingType != "ITP" && ShipmentWrapper.HouseBillOfLadingType != "INN")
				{
					if (IssuedByUNLOCO != null)
					{
						if (IssuedByUNLOCO.CountryCode == Core.Constants.CountryCodes.Australia)
						{
							result = (NoResString)"The Contract evidenced by or contained in this " + Title + (NoResString)" shall be governed "
								+ (NoResString)"by Australian law and any claim or dispute arising hereunder or in connection herewith "
								+ (NoResString)"shall (without prejudice to the Carrier's rights to commence proceedings in any other "
								+ (NoResString)"jurisdiction) be subject to the jurisdiction of the Courts of Australia.";
						}
						else
						{
							result = (NoResString)"The Contract evidenced by or contained in this " + Title + (NoResString)" shall be governed by "
								+ (NoResString)"the law in " + IssuedByUNLOCO.CountryName + (NoResString)" and any claim or dispute arising "
								+ (NoResString)"hereunder or in connection herewith shall (without prejudice to the Carrier's rights "
								+ (NoResString)"to commence proceedings in any other jurisdiction) be subject to the jurisdiction of "
								+ (NoResString)"the Courts of " + IssuedByUNLOCO.CountryName + (NoResString)".";
						}
					}
				}
				return result;
			}
		}

		public ZString LawAndJurisdictionClauseForTTC
		{
			get
			{
				ZString result = ZString.Empty;
				if (ShipmentWrapper.HouseBillOfLadingType != "TTP")
				{
					if (IssuedByUNLOCO != null)
					{
						result = (NoResString)"JURISDICTION AND LAW CLAUSE - ";
						if (IssuedByUNLOCO.CountryCode == Core.Constants.CountryCodes.Australia)
						{
							result += (NoResString)"If this " + Title + (NoResString)" is issued in Australia, "
								+ (NoResString)"the contract evidenced by or contained herein shall be governed by the "
								+ (NoResString)"law of the State or Territory in which it is issued and any claim or "
								+ (NoResString)"dispute arising hereunder or in connection herewith shall at the Carriers "
								+ (NoResString)"sole option be determined by the Courts of that State or Territory & no "
								+ (NoResString)"other Court. In all other cases any such claim or dispute shall be "
								+ (NoResString)"determined at the Carriers sole option either in the place where this " + Title
								+ (NoResString)" is issued (& subject to the laws of that place) or at the place where the "
								+ (NoResString)"Carrier has its principal place of business (and subject to the laws of that place).";
						}
						else
						{
							result += (NoResString)"The contract evidenced by or contained in this " + Title + (NoResString)" is governed by the "
								+ (NoResString)"law of " + IssuedByUNLOCO.CountryName + (NoResString)" and any claim or dispute arising hereunder "
								+ (NoResString)"or in connection herewith shall be determined by the Courts in "
								+ IssuedByUNLOCO.CountryName + (NoResString)" and no other Court.";
						}
					}
				}
				return result;
			}
		}

		public ZString BOLClause
		{
			get
			{
				ZString result = ZString.Empty;
				if (!ShouldUseBOLClauseITAR)
				{
					result = FreightDataRegistry.Instance.BOLClause.Value;
				}
				else
				{
					result = FreightDataRegistry.Instance.BOLClauseITAR.Value;
				}
				result = MacroReplacer.ReplaceMacros(result);

				return result;
			}
		}

		ZString BOLClauseForGoodsDescriptionSection
		{
			get { return ShipmentWrapper.IncludeBOLClauseInGoodsDescription ? BOLClause : ZString.Empty; }
		}

		public ZString DeclarationDestinationCountry
		{
			get
			{
				ZString result = ZString.Empty;
				Enterprise.Customs.Business.BaseJobDeclaration declaration = ShipmentWrapper.CommonShipment.DeclarationForDocuments as Enterprise.Customs.Business.BaseJobDeclaration;
				if (declaration != null)
				{
					RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, declaration.JE_RL_NKFinalDestination));
					if (uNLOCO != null)
					{
						result = uNLOCO.Country.RN_DescMultilingual;
					}
				}

				return result;
			}
		}

		public ZString USUltimateConsignee
		{
			get
			{
				ZString result = ZString.Empty;
				if (USDeclarationInvoiceHeader != null && !USDeclarationInvoiceHeader.JZ_OH_Buyer.IsEmpty)
				{
					var importer = Factory.Load<OrgHeader>(USDeclarationInvoiceHeader.JZ_OH_Buyer);
					if (importer != null)
					{
						result = importer.OH_FullName;
					}
				}
				return result;
			}
		}

		#region Implementation

		bool ShouldUseBOLClauseITAR
		{
			get
			{
				bool result = false;
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
					&& ShipmentWrapper.IsExport
					&& USDeclarationInvoiceHeader != null
					&& !USDeclarationInvoiceHeader.US_LicenseNo.IsEmpty
					&& !USDeclarationInvoiceHeader.US_DDTCRegistrationNo.IsEmpty
					&& ZDateTime.Now < DocConstants.USDestinationControlStatement.EffectiveDate)
				{
					result = true;
				}
				return result;
			}
		}

		Integration.Customs.US.IJobComInvoiceHeader USDeclarationInvoiceHeader
		{
			get
			{
				if (fUSDeclarationInvoiceHeader == null)
				{
					if (ShipmentWrapper.CommonShipment.DeclarationForDocuments is Integration.Customs.US.IJobDeclaration declaration && declaration.Invoices.Count > 0)
					{
						fUSDeclarationInvoiceHeader = (Integration.Customs.US.IJobComInvoiceHeader)declaration.Invoices[0];
					}
				}
				return fUSDeclarationInvoiceHeader;
			}
		}
		Integration.Customs.US.IJobComInvoiceHeader fUSDeclarationInvoiceHeader;

		#endregion

		#endregion

		#region IssuedBy Details

		public ZString IssuedByName
		{
			get { return IssuedBy.Name; }
		}

		public ZString IssuedByAddress1
		{
			get { return IssuedBy.Address1; }
		}

		public ZString IssuedByAddress2
		{
			get { return IssuedBy.Address2; }
		}

		public ZString IssuedByCity
		{
			get { return IssuedBy.City; }
		}

		public ZString IssuedByState
		{
			get { return IssuedBy.State; }
		}

		public ZString IssuedByPostCode
		{
			get { return IssuedBy.PostCode; }
		}

		public ZString IssuedByCountry
		{
			get { return IssuedByUNLOCO != null ? IssuedByUNLOCO.CountryName : ZString.Empty; }
		}

		public DocUNLOCO IssuedByUNLOCO
		{
			get { return DocUNLOCO.New(IssuedBy.UNLOCO, Factory); }
		}

		#endregion

		#region Marks and Numbers

		public ZString MarksAndNumbers
		{
			get
			{
				if (fMarksAndNumbers.IsEmpty)
				{
					BuildMarksAndNumbersColumn();
				}

				return fMarksAndNumbers;
			}
		}

		ZString fMarksAndNumbers;

		public ZString FollowOnMarksAndNumbers
		{
			get
			{
				if (fFollowOnMarksAndNumbers.IsEmpty)
				{
					BuildMarksAndNumbersColumn();
				}

				return fFollowOnMarksAndNumbers;
			}
		}

		ZString fFollowOnMarksAndNumbers;

		void BuildMarksAndNumbersColumn()
		{
			if (!ShipmentWrapper.MarksAndNumbers.IsEmpty)
			{
				ZString marksAndNumbersColumn = GetNewLinesInMarksAndNumbers();
				marksAndNumbersColumn += WrapTextForAColumn(ShipmentWrapper.MarksAndNumbers, ShipmentWrapper.MarksAndNumbersWidth);
				if (!marksAndNumbersColumn.IsEmpty)
				{
					BuildMainColumnAndFollowOn(marksAndNumbersColumn, true, false);
				}
			}
		}

		ZString GetNewLinesInMarksAndNumbers()
		{
			ZStringBuilder result = new ZStringBuilder();
			if (ShipmentWrapper.IncludePackageCountInBOLGoodsDescription == 1)
			{
				for (int i = 0; i < FCLContainersHavingRefContainerType.ToList().Count; i++)
				{
					result.Append("\n");
				}

				if (ShipmentWrapper.OuterPacks != 0)
				{
					result.Append("\n");
				}

				if (GetPackageCount().Contains("STC") && GetPackageCount().Contains((NoResString)"and"))
				{
					result.Append("\n");
				}
			}
			return result.ToString();
		}

		#endregion

		#region Number of Packages

		public ZString Packages
		{
			get
			{
				if (fPackages.IsEmpty)
				{
					fPackages = GetPackageCount();
				}

				return fPackages;
			}
		}

		ZString fPackages;

		#endregion

		#region Goods Description

		public ZString GoodsDescription
		{
			get
			{
				if (fGoodsDescription.IsEmpty)
				{
					BuildGoodsDescriptionColumn();
				}

				return fGoodsDescription;
			}
		}

		ZString fGoodsDescription;

		public ZString FollowOnGoodsDescription
		{
			get
			{
				if (fFollowOnGoodsDescription.IsEmpty)
				{
					BuildGoodsDescriptionColumn();
				}

				return fFollowOnGoodsDescription;
			}
		}

		ZString fFollowOnGoodsDescription;

		void BuildGoodsDescriptionColumn()
		{
			ZString result = GetGoodsDescriptionToWrap() + ExportStatement;
			result = WrapTextForAColumn(result, ShipmentWrapper.GoodsDescWidth);
			if (!result.IsEmpty)
			{
				BuildMainColumnAndFollowOn(result, false, true);
			}
		}

		ZString GetGoodsDescriptionToWrap()
		{
			ZString result = GetContainerTypeCount();

			if (ShipmentWrapper.IncludePackageCountInBOLGoodsDescription == 1 || IsFCLAndSEA)
			{
				result += GetPackageCount();
			}

			result += ShipmentWrapper.DescriptionForGoods;

			ZString bolClause = BOLClauseForGoodsDescriptionSection;
			if (!bolClause.IsEmpty)
			{
				result += "\r\n\r\n" + bolClause;
			}

			if (ShipmentWrapper.CommonShipment.CusEntryNumbers.Count > 1)
			{
				result += "\r\n\r\n" + ShipmentWrapper.HBLCustomsEntryNumberList;
			}

			return result;
		}

		internal ZString GetPackageCount()
		{
			fCountOfLCLPackages = 0;
			fLCLPackagesType = ZString.Empty;
			ZInt fCLPackageCount = 0;
			ZString result = ZString.Empty;

			if (ShipmentWrapper.OuterPacks > 0)
			{
				IDocContainerCollection shipmentContainers = null;
				DocPackLinesCollection shipmentPackLines = null;

				if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
				{
					shipmentContainers = MasterCoLoadContainers;
					shipmentPackLines = MasterCoLoadPackLines;
				}
				else
				{
					shipmentContainers = ShipmentWrapper.Containers;
					shipmentPackLines = ShipmentWrapper.OuterPackLineCollection;
				}

				foreach (DocContainer currentContainer in shipmentContainers)
				{
					if (currentContainer.ContainerMode == "FCL")
					{
						fCLPackageCount += currentContainer.TotalAllocatedShipmentPackages;
					}
				}

				if (fCLPackageCount.IsEmpty)
				{
					fCountOfLCLPackages = ShipmentWrapper.OuterPacks;
					result += ShipmentWrapper.OuterPacks;
					if (!ShipmentWrapper.OuterPacksPackTypeDescription.IsEmpty)
					{
						result += " " + ShipmentWrapper.OuterPacksPackTypeDescription;
						fLCLPackagesType = ShipmentWrapper.OuterPacksPackTypeDescription;
					}

					result += "\r\n";
				}
				else if (ShipmentWrapper.OuterPacks > fCLPackageCount)
				{
					ZInt packageDifference = ShipmentWrapper.OuterPacks - fCLPackageCount;
					fCountOfLCLPackages = packageDifference;
					foreach (DocPackLines line in shipmentPackLines)
					{
						if (line.ContainerNumber.IsEmpty || line.Container == null || line.Container.ContainerMode != "FCL")
						{
							if (fLCLPackagesType.IsEmpty)
							{
								fLCLPackagesType = line.PackTypeDescription;
							}
							else if (line.PackTypeDescription != fLCLPackagesType)
							{
								fLCLPackagesType = (NoResString)"Package";
								break;
							}
						}
					}

					if (fLCLPackagesType.IsEmpty)
					{
						fLCLPackagesType = ShipmentWrapper.OuterPacksPackTypeDescription;
					}
					else
					{
						fLCLPackagesType += (NoResString)"(s)";
					}

					result += ShipmentWrapper.STC_Label + fCLPackageCount + (NoResString)" " + ShipmentWrapper.OuterPacksPackTypeDescription + (NoResString)"\r\n" +
						(NoResString)" and " + packageDifference + (NoResString)" " + fLCLPackagesType + (NoResString)" LCL Cargo\r\n";
				}
				else
				{
					result += ShipmentWrapper.STC_Label + ShipmentWrapper.OuterPacks;
					if (!ShipmentWrapper.OuterPacksPackTypeDescription.IsEmpty)
					{
						result += " " + ShipmentWrapper.OuterPacksPackTypeDescription;
						fLCLPackagesType = ShipmentWrapper.OuterPacksPackTypeDescription;
					}

					result += "\r\n";
				}
			}

			return result;
		}

		bool IsFCLAndSEA
		{
			get
			{
				return (ShowPackagesColumn && ShipmentWrapper.TransportMode == "SEA" && ShipmentWrapper.PackingMode == "FCL");
			}
		}

		bool ShowPackagesColumn
		{
			get
			{
				return ShipmentWrapper.ShowPackageCount == 1 || (ShipmentWrapper.PackagesIndex > 0 && ShipmentWrapper.PackagesWidth > 0);
			}
		}

#if DEBUG
		protected
#endif
 ZString GetContainerTypeCount()
		{
			ZString result = ZString.Empty;
			GoodsDescriptionContainerListingHelper containerHelper = GetContainerListingHelper();

			if (containerHelper != null)
			{
				if (IsFCLAndSEA)
				{
					foreach (ZString containerType in containerHelper.ContainerTypeList)
					{
						result += containerType + "\n";
					}
				}
				else
				{
					result = containerHelper.ContainersListForDocument;
				}
			}
			return result;
		}

		protected List<DocBillofLadingContainer> FCLContainersHavingRefContainerType
		{
			get
			{
				List<DocBillofLadingContainer> result = Containers
					.Cast<DocBillofLadingContainer>()
					.Where(container => container.Container.ContainerMode == Core.Constants.ContainerModes.FCL && container.ContainerType != "-" && !container.ContainerType.IsEmpty)
					.ToList();

				result.Sort(new Comparison<DocBillofLadingContainer>(
								(container1, container2) =>
								{
									int typeComparison = container1.ContainerType.CompareTo(container2.ContainerType);
									return typeComparison == 0 ? container1.ContainerNumber.CompareTo(container2.ContainerNumber) : typeComparison;
								}
								));
				return result;
			}
		}

		GoodsDescriptionContainerListingHelper GetContainerListingHelper()
		{
			GoodsDescriptionContainerListingHelper containerHelper = new GoodsDescriptionContainerListingHelper();

			foreach (DocBillofLadingContainer container in FCLContainersHavingRefContainerType)
			{
				containerHelper.AddContainer(container.ContainerNumber, container.ContainerType, container.Container.ContainerCount);
			}

			fCountOfContainers = containerHelper.TotalCountOfContainers;
			return containerHelper;
		}

		void BuildMainColumnAndFollowOn(ZString value, ZBool isMarksAndNumbers, ZBool isGoodsDescritpion)
		{
			fMarksAndNumbers = ZString.Empty;
			fGoodsDescription = ZString.Empty;
			fFollowOnMarksAndNumbers = ZString.Empty;
			fFollowOnGoodsDescription = ZString.Empty;

			ZString[] valueSplitByRow = value.Replace("\r\n", "\n").Split('\n');

			for (int i = 0; i < ShipmentWrapper.MarksAndNumbsAndDescHeight; i++)
			{
				if (i < valueSplitByRow.Length)
				{
					if (isMarksAndNumbers)
					{
						fMarksAndNumbers += valueSplitByRow[i] + "\r\n";
					}

					if (isGoodsDescritpion)
					{
						fGoodsDescription += valueSplitByRow[i] + "\r\n";
					}
				}
			}

			for (int i = ShipmentWrapper.MarksAndNumbsAndDescHeight; i < valueSplitByRow.Length; i++)
			{
				if (isMarksAndNumbers)
				{
					fFollowOnMarksAndNumbers += valueSplitByRow[i] + "\r\n";
				}

				if (isGoodsDescritpion)
				{
					fFollowOnGoodsDescription += valueSplitByRow[i] + "\r\n";
				}
			}
		}

		#endregion

		#region Container Columns

		public ZString ContainerNumberColumn
		{
			get
			{
				if (fContainerNumberColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerNumberColumn;
			}
		}

		public ZString ContainerSealNumColumn
		{
			get
			{
				if (fContainerSealNumColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerSealNumColumn;
			}
		}

		public ZString ContainerTypeColumn
		{
			get
			{
				if (fContainerTypeColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerTypeColumn;
			}
		}

		public ZString ContainerWeightColumn
		{
			get
			{
				if (fContainerWeightColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerWeightColumn;
			}
		}

		public ZString ContainerVolumeColumn
		{
			get
			{
				if (fContainerVolumeColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerVolumeColumn;
			}
		}

		public ZString ContainerPackagesColumn
		{
			get
			{
				if (fContainerPackagesColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerPackagesColumn;
			}
		}

		public ZString ContainerModeColumn
		{
			get
			{
				if (fContainerModeColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerModeColumn;
			}
		}

		public ZString ContainerAsteriskColumn
		{
			get
			{
				if (fContainerAsteriskColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fContainerAsteriskColumn;
			}
		}

		void BuildContainerColumns()
		{
			fContainerNumberColumn = ZString.Empty;
			fContainerSealNumColumn = ZString.Empty;
			fContainerTypeColumn = ZString.Empty;
			fContainerWeightColumn = ZString.Empty;
			fContainerVolumeColumn = ZString.Empty;
			fContainerPackagesColumn = ZString.Empty;
			fContainerModeColumn = ZString.Empty;
			fContainerAsteriskColumn = ZString.Empty;
			fFollowOnContainerNumberColumn = ZString.Empty;
			fFollowOnContainerSealNumColumn = ZString.Empty;
			fFollowOnContainerTypeColumn = ZString.Empty;
			fFollowOnContainerWeightColumn = ZString.Empty;
			fFollowOnContainerVolumeColumn = ZString.Empty;
			fFollowOnContainerPackagesColumn = ZString.Empty;
			fFollowOnContainerModeColumn = ZString.Empty;
			fFollowOnContainerAsteriskColumn = ZString.Empty;

			if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
			{
				BuildMasterBOLContainerColumns();
			}
			else
			{
				BuildNonMasterBOLContainerColumns();
			}
		}

		void BuildMasterBOLContainerColumns()
		{
			ZInt noOfContainers = 1;

			IDocContainerCollection shipmentContainers = MasterCoLoadContainers;
			shipmentContainers.Sort("ContainerNumber", ListSortDirection.Ascending);
			Hashtable listedContainers = new Hashtable();

			foreach (DocContainer currentContainer in shipmentContainers)
			{
				if (currentContainer.AllocatedShipmentPackLine != null)
				{
					ZDecimal totalWeight = 0M;
					ZDecimal totalVolumne = 0M;
					ZInt totalPacks = 0;

					ZString containerDeliveryMode = GetContainerDeliveryModeThroughThePackLine(currentContainer);

					if (!listedContainers.ContainsKey(currentContainer.ContainerNumber + ", " + containerDeliveryMode))
					{
						listedContainers.Add(currentContainer.ContainerNumber + ", " + containerDeliveryMode, null);

						totalWeight = currentContainer.TotalAllocatedShipmentWeight;
						totalVolumne = currentContainer.TotalAllocatedShipmentVolume;
						totalPacks = currentContainer.TotalAllocatedShipmentPackages;

						foreach (DocContainer innerContainers in shipmentContainers)
						{
							if (innerContainers != currentContainer && innerContainers.AllocatedShipmentPackLine != null)
							{
								ZString innerContainerDeliveryMode = GetContainerDeliveryModeThroughThePackLine(innerContainers);

								if (currentContainer.ContainerNumber == innerContainers.ContainerNumber && containerDeliveryMode == innerContainerDeliveryMode)
								{
									totalWeight += innerContainers.TotalAllocatedShipmentWeight;
									totalVolumne += innerContainers.TotalAllocatedShipmentVolume;
									totalPacks += innerContainers.TotalAllocatedShipmentPackages;
								}
							}
						}

						if (noOfContainers <= ShipmentWrapper.NoOfContainerRows)
						{
							SetContainerColumns(currentContainer, containerDeliveryMode, totalWeight, totalVolumne, totalPacks);
							noOfContainers++;
						}
						else
						{
							SetFollowOnContainerColumns(currentContainer, containerDeliveryMode, totalWeight, totalVolumne, totalPacks);
						}
					}
				}
			}
		}

		void BuildNonMasterBOLContainerColumns()
		{
			ZInt noOfContainers = 1;

			IDocContainerCollection shipmentContainers = ShipmentWrapper.Containers;
			shipmentContainers.Sort("ContainerNumber", ListSortDirection.Ascending);

			foreach (DocContainer currentContainer in shipmentContainers)
			{
				ZString containerDeliveryMode = ZString.Empty;
				if (ShipmentWrapper.HBLContainerPackModeOverride.IsEmpty)
				{
					containerDeliveryMode = currentContainer.DeliveryMode;
				}
				else
				{
					containerDeliveryMode = ShipmentWrapper.HBLContainerPackModeOverride;
				}

				if (noOfContainers <= ShipmentWrapper.NoOfContainerRows)
				{
					SetContainerColumns(currentContainer, containerDeliveryMode, currentContainer.TotalAllocatedShipmentWeight, currentContainer.TotalAllocatedShipmentVolume, currentContainer.TotalAllocatedShipmentPackages);
					noOfContainers++;
				}
				else
				{
					SetFollowOnContainerColumns(currentContainer, containerDeliveryMode, currentContainer.TotalAllocatedShipmentWeight, currentContainer.TotalAllocatedShipmentVolume, currentContainer.TotalAllocatedShipmentPackages);
				}
			}
		}

		ZString GetContainerDeliveryModeThroughThePackLine(DocContainer container)
		{
			ZString result = ZString.Empty;

			if (container.AllocatedShipmentPackLine[0].Shipment.HBLContainerPackModeOverride.IsEmpty)
			{
				result = container.DeliveryMode;
			}
			else
			{
				result = container.AllocatedShipmentPackLine[0].Shipment.HBLContainerPackModeOverride;
			}

			return result;
		}

		void SetContainerColumns(DocContainer container, ZString containerDeliveryMode, ZDecimal weight, ZDecimal volume, ZInt packages)
		{
			fContainerNumberColumn += container.ContainerNumber + "\n";

			if (container.SealNumber.IsEmpty)
			{
				fContainerSealNumColumn += "-\n";
			}
			else
			{
				fContainerSealNumColumn += container.SealNumber + "\n";
			}

			if (container.Container == null)
			{
				fContainerTypeColumn += "-\n";
			}
			else
			{
				fContainerTypeColumn += container.Container.Code + "\n";
			}

			if (weight != 0M)
			{
				fContainerWeightColumn += ShipmentWrapper.FormatNumber(weight, Env.Registry.WeightMinimumDecimalPlacesToDisplay) + "\n";
			}
			else
			{
				fContainerWeightColumn += "-\n";
			}

			if (volume != 0M)
			{
				fContainerVolumeColumn += ShipmentWrapper.FormatNumber(volume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay) + "\n";
			}
			else
			{
				fContainerVolumeColumn += "-\n";
			}

			if (packages != 0)
			{
				fContainerPackagesColumn += packages.ToString() + "\n";
			}
			else
			{
				fContainerPackagesColumn += "-\n";
			}

			if (containerDeliveryMode.IsEmpty)
			{
				fContainerModeColumn += "-\n";
				fContainerAsteriskColumn += "\n";
			}
			else if (containerDeliveryMode.StartsWith("CY"))
			{
				fContainerModeColumn += containerDeliveryMode + "*\n";
				fContainerAsteriskColumn += "*\n";
			}
			else
			{
				fContainerModeColumn += containerDeliveryMode + "\n";
				fContainerAsteriskColumn += "\n";
			}
		}

		ZString fContainerAsteriskColumn;

		void SetFollowOnContainerColumns(DocContainer container, ZString containerDeliveryMode, ZDecimal weight, ZDecimal volume, ZInt packages)
		{
			fFollowOnContainerNumberColumn += container.ContainerNumber + "\n";

			if (container.SealNumber.IsEmpty)
			{
				fFollowOnContainerSealNumColumn += "-\n";
			}
			else
			{
				fFollowOnContainerSealNumColumn += container.SealNumber + "\n";
			}

			if (container.Container == null)
			{
				fFollowOnContainerTypeColumn += "-\n";
			}
			else
			{
				fFollowOnContainerTypeColumn += container.Container.Code + "\n";
			}

			if (weight != 0M)
			{
				fFollowOnContainerWeightColumn += ShipmentWrapper.FormatNumber(weight, Env.Registry.WeightMinimumDecimalPlacesToDisplay) + "\n";
			}
			else
			{
				fFollowOnContainerWeightColumn += "-\n";
			}

			if (volume != 0M)
			{
				fFollowOnContainerVolumeColumn += ShipmentWrapper.FormatNumber(volume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay) + "\n";
			}
			else
			{
				fFollowOnContainerVolumeColumn += "-\n";
			}

			if (packages != 0)
			{
				fFollowOnContainerPackagesColumn += packages.ToString() + "\n";
			}
			else
			{
				fFollowOnContainerPackagesColumn += "-\n";
			}

			if (containerDeliveryMode.IsEmpty)
			{
				fFollowOnContainerModeColumn += "-\n";
				fFollowOnContainerAsteriskColumn += "\n";
			}
			else if (containerDeliveryMode.StartsWith("CY"))
			{
				fFollowOnContainerModeColumn += containerDeliveryMode + "*\n";
				fFollowOnContainerAsteriskColumn += "*\n";
			}
			else
			{
				fFollowOnContainerModeColumn += containerDeliveryMode + "\n";
				fFollowOnContainerAsteriskColumn += "\n";
			}
		}

		ZString fContainerNumberColumn;
		ZString fContainerSealNumColumn;
		ZString fContainerTypeColumn;
		ZString fContainerWeightColumn;
		ZString fContainerVolumeColumn;
		ZString fContainerPackagesColumn;
		ZString fContainerModeColumn;

		#endregion

		#region Follow On Container Columns

		public ZString FollowOnContainerNumberColumn
		{
			get
			{
				if (fFollowOnContainerNumberColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerNumberColumn;
			}
		}

		public ZString FollowOnContainerSealNumColumn
		{
			get
			{
				if (fFollowOnContainerSealNumColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerSealNumColumn;
			}
		}

		public ZString FollowOnContainerTypeColumn
		{
			get
			{
				if (fFollowOnContainerTypeColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerTypeColumn;
			}
		}

		public ZString FollowOnContainerWeightColumn
		{
			get
			{
				if (fFollowOnContainerWeightColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerWeightColumn;
			}
		}

		public ZString FollowOnContainerVolumeColumn
		{
			get
			{
				if (fFollowOnContainerVolumeColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerVolumeColumn;
			}
		}

		public ZString FollowOnContainerPackagesColumn
		{
			get
			{
				if (fFollowOnContainerPackagesColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerPackagesColumn;
			}
		}

		public ZString FollowOnContainerModeColumn
		{
			get
			{
				if (fFollowOnContainerModeColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerModeColumn;
			}
		}

		public ZString FollowOnContainerAsteriskColumn
		{
			get
			{
				if (fFollowOnContainerAsteriskColumn.IsEmpty)
				{
					BuildContainerColumns();
				}

				return fFollowOnContainerAsteriskColumn;
			}
		}

		ZString fFollowOnContainerAsteriskColumn;

		ZString fFollowOnContainerNumberColumn;
		ZString fFollowOnContainerSealNumColumn;
		ZString fFollowOnContainerTypeColumn;
		ZString fFollowOnContainerWeightColumn;
		ZString fFollowOnContainerVolumeColumn;
		ZString fFollowOnContainerPackagesColumn;
		ZString fFollowOnContainerModeColumn;

		#endregion

		#region Charges

		#region Charges - Itemised List

		public bool ShouldPrintChargesAsLumpSum
		{
			get
			{
				bool result = false;

				if (ShipmentWrapper.DestinationLoco != null)
				{
					var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, ShipmentWrapper.DestinationLoco.CountryCode));

					if (country != null && DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.Value.Contains(country.PK.ToGuid()))
					{
						result = true;
					}
				}

				return result;
			}
		}

		public bool ShouldPrintTotalCharges
		{
			get
			{
				return !ShouldPrintChargesAsLumpSum
					&& ShipmentWrapper.ChargesDisplay != DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed
					&& (ZBool)DocumentsDataRegistry.Instance.BOLPrintTotalCharges.Value;
			}
		}

		DocJobChargeCollection GetJobChargesSetToDisplay()
		{
			if (ShouldDisplayCollectCharges)
			{
				return ShipmentWrapper.JobHeader.JobChargesForAgentCollect;
			}
			if (ShouldDisplayPrepaidCharges)
			{
				return ShipmentWrapper.JobHeader.JobChargesForLocalClient;
			}
			if (ShouldDisplayBothPrepaidAndCollectCharges)
			{
				return DocJobChargeCollection.GetCollection(ShipmentWrapper, nameof(GetJobChargesSetToDisplay),
					(jobChargesPrepaidCollect) =>
					{
						jobChargesPrepaidCollect.AddRange(ShipmentWrapper.JobHeader.JobChargesForAgentCollect);
						jobChargesPrepaidCollect.AddRange(ShipmentWrapper.JobHeader.JobChargesForLocalClient);
					});
			}

			return null;
		}

		ZString GetPrepaidAndCollectChargesAsLumpSum()
		{
			DocCurrency combinedChargesCurrency = null;
			ZDecimal combinedCharges = 0;
			ZString combinedChargesString = new();

			if (ShouldDisplayChargesAsAgreed)
			{
				combinedChargesString = (NoResString)"As Agreed";
			}
			else if (ShouldDisplayCollectCharges || ShouldDisplayPrepaidCharges || ShouldDisplayBothPrepaidAndCollectCharges)
			{
				if (ShipmentWrapper.JobHeader != null)
				{
					var jobCharges = GetJobChargesSetToDisplay();
					bool isCombinedCurrencyDifferent = false;

					foreach (DocJobCharge charge in jobCharges)
					{
						if ((ShouldDisplayCollectCharges || ShouldDisplayPrepaidCharges || ShouldDisplayBothPrepaidAndCollectCharges)
							&& charge.OSSellAmt != 0)
						{
							if (combinedChargesCurrency == null)
							{
								combinedChargesCurrency = charge.OSSellCurrency;
							}
							else if (combinedChargesCurrency.ToString() != charge.OSSellCurrency.ToString())
							{
								isCombinedCurrencyDifferent = true;
								break;
							}
						}
					}

					if (!isCombinedCurrencyDifferent)
					{
						foreach (DocJobCharge charge in jobCharges)
						{
							combinedCharges += charge.OSSellAmt;
						}
					}
					else
					{
						var converter = ShipmentWrapper?.JobHeader?.JobHeader?.CurrencyConverter ?? CurrencyConverter.New(Factory, ZDateTime.Now, ExchangeRateType.Sell, 1);
						var uSD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
						combinedChargesCurrency = DocCurrency.New(Factory, uSD);

						foreach (DocJobCharge charge in jobCharges)
						{
							if (charge.OSSellAmt != 0)
							{
								var money = new Money(charge.OSSellAmt, charge.OSSellCurrency);
								combinedCharges += converter.ConvertRounded(money, uSD).Amount;
							}
						}
					}
				}

				if (combinedCharges > 0)
				{
					var converter = new CurrencyToWords_EN();
					ZString inWords = converter.ConvertToWords(Convert.ToDouble(combinedCharges), combinedChargesCurrency.Code).Replace(" only", "");
					combinedChargesString = (NoResString)"FREIGHT LUMP SUM: " + combinedCharges.Round(combinedChargesCurrency.Decimals) + (NoResString)" " + combinedChargesCurrency.Code + (NoResString)" " + inWords.ToUpper();
				}
			}

			return combinedChargesString;
		}

		void BuildPrepaidAndCollectChargesColumns()
		{
			prepaidAndCollectCharges = ZString.Empty;
			followOnPrepaidAndCollectCharges = ZString.Empty;

			if (ShouldDisplayChargesAsAgreed)
			{
				prepaidAndCollectCharges = (NoResString)"As Agreed";
			}
			else if (ShouldDisplayCollectCharges || ShouldDisplayPrepaidCharges || ShouldDisplayBothPrepaidAndCollectCharges)
			{
				if (ShipmentWrapper.JobHeader != null)
				{
					var jobCharges = GetJobChargesSetToDisplay();
					int count = 0;

					foreach (DocJobCharge charge in jobCharges)
					{
						bool isCollectCharge = charge.JobCharge.JR_OH_SellAccount == charge.JobHeader.JobHeader.AgentCollectPK;
						int gapWidth = isCollectCharge
							? ShipmentWrapper.ChargesDiscriptionAndCollectChargesGap
							: ShipmentWrapper.ChargesDiscriptionAndPrepaidChargesGap;
						int chargesColumnWidth = isCollectCharge
							? ShipmentWrapper.CollectChargesColumnWidth
							: ShipmentWrapper.PrepaidChargesColumnWidth;

						ZString chargeLine = ZString.Empty;

						if (charge.OSSellAmt != 0)
						{
							bool isChargeDisplayedOnMainBody = count < ShipmentWrapper.NoOfCollectChargesRows;

							charge.ChargeInfoDescriptionWidth = isChargeDisplayedOnMainBody ? ShipmentWrapper.ChargesDiscriptionWidth : ShipmentWrapper.ChargesDescriptionFollowOnPageWidth;

							ZString chargeDescription = charge.GetChargeDescription(ChargeDescriptionLanguageMode)
								.PadRight(charge.ChargeInfoDescriptionWidth + gapWidth);
							ZString chargeAmount = charge.GetChargeAmount()
								.SubstringSafe(0, chargesColumnWidth)
								.PadLeft(chargesColumnWidth);

							chargeLine = chargeDescription + chargeAmount + "\n";

							if (!chargeLine.IsEmpty)
							{
								if (isChargeDisplayedOnMainBody)
								{
									prepaidAndCollectCharges += chargeLine;
								}
								else
								{
									followOnPrepaidAndCollectCharges += chargeLine;
								}
								count++;
							}
						}
					}
				}
			}
		}

		DocJobCharge.ChargeDescriptionLanguageModes ChargeDescriptionLanguageMode
		{
			get { return Res.CurrentLanguage == Res.DefaultLanguage ? DocJobCharge.ChargeDescriptionLanguageModes.EnglishOnly : DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage; }
		}

		public ZString PrepaidAndCollectCharges
		{
			get
			{
				if (ShouldPrintChargesAsLumpSum)
				{
					return GetPrepaidAndCollectChargesAsLumpSum();
				}
				else if (prepaidAndCollectCharges.IsEmpty)
				{
					BuildPrepaidAndCollectChargesColumns();
				}

				return prepaidAndCollectCharges;
			}
		}

		public ZString FollowOnPrepaidAndCollectCharges
		{
			get
			{
				if (ShouldPrintChargesAsLumpSum)
				{
					return ZString.Empty;
				}
				else if (followOnPrepaidAndCollectCharges.IsEmpty)
				{
					BuildPrepaidAndCollectChargesColumns();
				}

				return followOnPrepaidAndCollectCharges;
			}
		}

		ZString prepaidAndCollectCharges;
		ZString followOnPrepaidAndCollectCharges;

		#endregion

		#region Charges - Totals

		ZDecimal fTotalCollectCharges;
		public ZDecimal TotalCollectCharges
		{
			get
			{
				if (!prepaidCollectTotalsCalculated)
				{
					BuildTotalPrepaidAndCollectCharges();
				}
				return fTotalCollectCharges;
			}
		}

		ZDecimal fTotalPrepaidCharges;
		public ZDecimal TotalPrepaidCharges
		{
			get
			{
				if (!prepaidCollectTotalsCalculated)
				{
					BuildTotalPrepaidAndCollectCharges();
				}
				return fTotalPrepaidCharges;
			}
		}

		DocCurrency fTotalCollectChargesCurrency;
		public DocCurrency TotalCollectChargesCurrency
		{
			get
			{
				DocCurrency result = null;
				if (fIsDifferentCollectCurrency || fTotalCollectChargesCurrency == null)
				{
					DocOrganisation proxy = ShipmentWrapper.CurrentBranch.Organisation ?? ShipmentWrapper.CurrentCompany.Organisation;
					if (proxy != null && proxy.Country != null)
					{
						result = proxy.Country.Currency;
					}
				}
				else
				{
					result = fTotalCollectChargesCurrency;
				}

				return result;
			}
		}

		DocCurrency fTotalPrepaidChargesCurrency;
		public DocCurrency TotalPrepaidChargesCurrency
		{
			get
			{
				DocCurrency result = null;
				if (fIsDifferentPrepaidCurrency || fTotalPrepaidChargesCurrency == null)
				{
					DocOrganisation proxy = ShipmentWrapper.CurrentBranch.Organisation ?? ShipmentWrapper.CurrentCompany.Organisation;
					if (proxy != null && proxy.Country != null)
					{
						result = proxy.Country.Currency;
					}
				}
				else
				{
					result = fTotalPrepaidChargesCurrency;
				}

				return result;
			}
		}

		ZBool fIsDifferentCollectCurrency;
		ZBool fIsDifferentPrepaidCurrency;

		bool prepaidCollectTotalsCalculated;
		void BuildTotalPrepaidAndCollectCharges()
		{
			fTotalCollectChargesCurrency = null;
			fTotalPrepaidChargesCurrency = null;

			if (!ShouldDisplayChargesAsAgreed && ShipmentWrapper.ChargesDisplay != DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges)
			{
				if (ShipmentWrapper.JobHeader != null)
				{
					DocJobChargeCollection jobCharges = GetJobChargesSetToDisplay();

					if (jobCharges != null)
					{
						foreach (DocJobCharge charge in jobCharges)
						{
							bool isCollect = charge.JobCharge.JR_OH_SellAccount == charge.JobHeader.JobHeader.AgentCollectPK;

							if (isCollect)
							{
								if (fTotalCollectChargesCurrency == null)
								{
									fTotalCollectChargesCurrency = charge.OSSellCurrency;
								}
								else if (fTotalCollectChargesCurrency.ToString() != charge.OSSellCurrency.ToString())
								{
									fIsDifferentCollectCurrency = true;
								}
							}
							else
							{
								if (fTotalPrepaidChargesCurrency == null)
								{
									fTotalPrepaidChargesCurrency = charge.OSSellCurrency;
								}
								else if (fTotalPrepaidChargesCurrency.ToString() != charge.OSSellCurrency.ToString())
								{
									fIsDifferentPrepaidCurrency = true;
								}
							}
						}

						foreach (DocJobCharge charge in jobCharges)
						{
							bool isCollect = charge.JobCharge.JR_OH_SellAccount == charge.JobHeader.JobHeader.AgentCollectPK;

							if (isCollect)
							{
								if (fIsDifferentCollectCurrency)
								{
									fTotalCollectCharges += charge.LocalSellAmount;
								}
								else
								{
									fTotalCollectCharges += charge.OSSellAmt;
								}
							}
							else
							{
								if (fIsDifferentPrepaidCurrency)
								{
									fTotalPrepaidCharges += charge.LocalSellAmount;
								}
								else
								{
									fTotalPrepaidCharges += charge.OSSellAmt;
								}
							}
						}
					}
				}
			}

			prepaidCollectTotalsCalculated = true;
		}

		#endregion

		#region Charges - Implementation

		bool ShouldDisplayCollectCharges
		{
			get
			{
				return ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges
					|| (ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges
						&& IsCopy);
			}
		}

		bool ShouldDisplayPrepaidCharges
		{
			get
			{
				return ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges
					|| (ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges
						&& IsCopy);
			}
		}

		bool ShouldDisplayBothPrepaidAndCollectCharges
		{
			get
			{
				return ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges
					|| (ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges
						&& IsCopy);
			}
		}

		bool ShouldDisplayChargesAsAgreed
		{
			get
			{
				return ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed
					|| ((ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges
						|| ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges
						|| ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges)
						&& IsOriginal);
			}
		}

		#endregion
		#region Charges - OBSOLETE CODE (see WI00040508)

		// Obsolete - Use PrepaidAndCollectCharges collection instead
		public ZString AlignedCollectCharges
		{
			get
			{
				if (fAlignedCharges.IsEmpty)
				{
					BuildAlignedChargesColumns();
				}
				return fAlignedCharges;
			}
		}
		ZString fAlignedCharges;

		// Obsolete - Use PrepaidAndCollectCharges collection instead
		public ZString CollectCharges
		{
			get
			{
				if (ShouldPrintChargesAsLumpSum)
				{
					return GetPrepaidAndCollectChargesAsLumpSum();
				}
				else
				{
					if (fCollectCharges.IsEmpty)
					{
						BuildChargesColumns();
					}
					return fCollectCharges;
				}
			}
		}

		// Obsolete - Use PrepaidAndCollectCharges collection instead
		public ZString FollowOnCollectCharges
		{
			get
			{
				if (fFollowOnCollectCharges.IsEmpty)
				{
					BuildChargesColumns();
				}
				return fFollowOnCollectCharges;
			}
		}

#if DEBUG
		protected
#endif
 bool ShouldAlignCharges()
		{
			if (ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges ||
				ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges)
			{
				if (ShipmentWrapper.JobHeader != null)
				{
					if (GetJobChargesSetToDisplay().Count <= ShipmentWrapper.NoOfCollectChargesRows - 2)
					{
						return true;
					}
				}
			}

			return false;
		}

		void BuildAlignedChargesColumns()
		{
			fAlignedCharges = ZString.Empty;

			if (ShouldAlignCharges())
			{
				DocJobChargeCollection jobCharges = GetJobChargesSetToDisplay();

				ZString amountLine = ZString.Empty;
				ZString chargesDescription = ZString.Empty;
				ZString chargesLine = ZString.Empty;
				ZString currencyCodeUsed = (jobCharges.Count > 0 && jobCharges[0].OSSellCurrency != null) ? jobCharges[0].OSSellCurrency.Code : ZString.Empty;
				ZDecimal chargesTotal = ZDecimal.Zero;
				ZDecimal amount = ZDecimal.Zero;
				ZBool sameCurrencyUsed = true;

				for (int i = 0; i < ShipmentWrapper.NoOfCollectChargesRows; i++)
				{
					if (i < jobCharges.Count)
					{
						if (!currencyCodeUsed.EqualsIgnoringCase(jobCharges[i].OSSellCurrency.Code))
						{
							sameCurrencyUsed = false;
							break;
						}

						amount = ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges ?
							jobCharges[i].CollectChargesAmount(ShipmentWrapper.INCO) : jobCharges[i].OSSellAmt;

						chargesTotal += amount;

						amountLine = new Money(amount, jobCharges[i].ForeignSellCurrency).ToString();

						chargesDescription = ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges ?
							jobCharges[i].CollectChargesDescription(ShipmentWrapper.INCO, ChargeDescriptionLanguageMode) : jobCharges[i].GetChargeDescription(ChargeDescriptionLanguageMode);

						if (chargesDescription.Length > ShipmentWrapper.CollectChargesWidth - 14)
						{
							chargesDescription = chargesDescription.Left(ShipmentWrapper.CollectChargesWidth - 14);
						}

						chargesLine = chargesDescription + FillWithSpaces(ShipmentWrapper.CollectChargesWidth - amountLine.Length - chargesDescription.Length) + amountLine;

						fAlignedCharges += chargesLine + "\n";
					}
				}

				if (sameCurrencyUsed && !fAlignedCharges.IsEmpty)
				{
					fAlignedCharges = AddTotalLineToAlignedCharges(fAlignedCharges, chargesTotal, currencyCodeUsed, ShipmentWrapper.CollectChargesWidth);
				}
				else
				{
					fAlignedCharges = CollectCharges;
				}
			}
			else
			{
				fAlignedCharges = CollectCharges;
			}
		}

#if DEBUG
		protected
#endif
 ZString AddTotalLineToAlignedCharges(ZString alignedChargesColumn, ZDecimal totalAmount, ZString currencyCodeUsed, ZInt maxLength)
		{
			ZString result = ZString.Empty;
			if (!(alignedChargesColumn.IsEmpty && totalAmount.IsEmpty && currencyCodeUsed.IsEmpty && maxLength.IsEmpty))
			{
				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCodeUsed) ?? GlbCompany.CurrentCompany.LocalCurrency;
				ZString totalLine = new Money(totalAmount, currency).ToString();
				totalLine = (totalLine.Length <= 14 ? FillWithSpaces(14 - totalLine.Length) : new ZString("  ")) + totalLine;
				totalLine = FillWithSpaces(maxLength - totalLine.Length - TOTALHEADING.Length) + TOTALHEADING + totalLine;
				result = alignedChargesColumn + FillWithSpaces(maxLength - TOTALLINE.Length) + TOTALLINE + "\n" + totalLine;
			}

			return result;
		}

		protected readonly string TOTALLINE = "------------";
		protected readonly string TOTALHEADING = (NoResString)"Total:";

		void BuildChargesColumns()
		{
			fCollectCharges = ZString.Empty;
			fFollowOnCollectCharges = ZString.Empty;

			if (ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed)
			{
				fCollectCharges = (NoResString)"As Agreed";
			}
			else if (ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges ||
					ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges)
			{
				if (ShipmentWrapper.JobHeader != null)
				{
					DocJobChargeCollection jobCharges = GetJobChargesSetToDisplay();
					int count = 0;

					foreach (DocJobCharge jobCharge in jobCharges)
					{
						if (ShipmentWrapper.ChargeInfoEnableColumnarFormat)
						{
							jobCharge.ChargeInfoEnbableColumnarFormat = ShipmentWrapper.ChargeInfoEnableColumnarFormat;
							jobCharge.ChargeInfoAmountWidth = ShipmentWrapper.ChargeInfoAmountWidth;
							jobCharge.ChargeInfoDescriptionWidth = ShipmentWrapper.ChargeInfoDescriptionWidth;
							jobCharge.ChargeInfoGapWidth = ShipmentWrapper.ChargeInfoGapWidth;
						}
						ZString chargesInfo = ShipmentWrapper.ChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges ?
								jobCharge.GetCollectChargesInfo(ShipmentWrapper.INCO, ChargeDescriptionLanguageMode) : jobCharge.GetChargesInfo(ChargeDescriptionLanguageMode);

						if (jobCharge.OSSellAmt != 0)
						{
							if (count < ShipmentWrapper.NoOfCollectChargesRows)
							{
								fCollectCharges += chargesInfo;
							}
							else
							{
								fFollowOnCollectCharges += chargesInfo;
							}
							count++;
						}
					}
				}
			}
		}

		ZString fCollectCharges;
		ZString fFollowOnCollectCharges;

		#endregion

		#endregion

		#region Goods Details

		public ZString GoodsDetails
		{
			get
			{
				if (fGoodsDetails.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fGoodsDetails;
			}
		}

		public ZString ShipperLoadAndCount
		{
			get
			{
				if (fGoodsDetails.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fShipperLoadAndCount;
			}
		}

		public ZString FollowOnMarksAndNums
		{
			get
			{
				if (fFollowOnMarksAndNums.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnMarksAndNums.TrimEnd();
			}
		}

		public ZString FollowOnGoodsDesc
		{
			get
			{
				if (fFollowOnGoodsDesc.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnGoodsDesc.TrimEnd();
			}
		}

		public ZString FollowOnContainerNumber
		{
			get
			{
				if (fFollowOnContainerNumber.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerNumber;
			}
		}

		public ZString FollowOnContainerSealNum
		{
			get
			{
				if (fFollowOnContainerSealNum.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerSealNum;
			}
		}

		public ZString FollowOnContainerType
		{
			get
			{
				if (fFollowOnContainerType.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerType;
			}
		}

		public ZString FollowOnContainerWeight
		{
			get
			{
				if (fFollowOnContainerWeight.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerWeight;
			}
		}

		public ZString FollowOnContainerVolume
		{
			get
			{
				if (fFollowOnContainerVolume.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerVolume;
			}
		}

		public ZString FollowOnContainerPackages
		{
			get
			{
				if (fFollowOnContainerPackages.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerPackages;
			}
		}

		public ZString FollowOnContainerMode
		{
			get
			{
				if (fFollowOnContainerMode.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnContainerMode;
			}
		}

		public ZString FollowOnShipperLoadAndCount
		{
			get
			{
				if (fGoodsDetails.IsEmpty)
				{
					BuildGoodsDetails();
				}

				return fFollowOnShipperLoadAndCount;
			}
		}

		ZString fGoodsDetails;
		ZString fShipperLoadAndCount;
		ZString fFollowOnMarksAndNums;
		ZString fFollowOnGoodsDesc;
		ZString fFollowOnContainerNumber;
		ZString fFollowOnContainerSealNum;
		ZString fFollowOnContainerType;
		ZString fFollowOnContainerWeight;
		ZString fFollowOnContainerVolume;
		ZString fFollowOnContainerPackages;
		ZString fFollowOnContainerMode;
		ZString fFollowOnShipperLoadAndCount;
		ZString[] ContainerNumbers
		{
			get
			{
				if (!ContainerNumberColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerNumberColumn + FollowOnContainerNumberColumn.TrimEnd('\n'), ContainerNumberWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZString[] ContainerSeals
		{
			get
			{
				if (!ContainerSealNumColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerSealNumColumn + FollowOnContainerSealNumColumn.TrimEnd('\n'), ContainerSealWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZString[] ContainerTypes
		{
			get
			{
				if (!ContainerTypeColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerTypeColumn + FollowOnContainerTypeColumn.TrimEnd('\n'), ContainerTypeWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZString[] ContainerWeights
		{
			get
			{
				if (!ContainerWeightColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerWeightColumn + FollowOnContainerWeightColumn.TrimEnd('\n'), ContainerWeightWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZString[] ContainerVolumes
		{
			get
			{
				if (!ContainerVolumeColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerVolumeColumn + FollowOnContainerVolumeColumn.TrimEnd('\n'), ContainerVolumeWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZString[] ContainerPacks
		{
			get
			{
				if (!ContainerPackagesColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerPackagesColumn + FollowOnContainerPackagesColumn.TrimEnd('\n'), ContainerPackagesWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZString[] ContainerModes
		{
			get
			{
				if (!ContainerModeColumn.IsEmpty)
				{
					return WrapTextForAColumn(ContainerModeColumn + FollowOnContainerModeColumn.TrimEnd('\n'), ContainerModeWidth).Split('\n');
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		ZInt ContainerNumberWidth
		{
			get
			{
				return ShipmentWrapper.ContainerNumberWidth;
			}
		}
		ZInt ContainerSealWidth
		{
			get
			{
				return ShipmentWrapper.ContainerSealWidth;
			}
		}
		ZInt ContainerTypeWidth
		{
			get
			{
				return ShipmentWrapper.ContainerTypeWidth;
			}
		}
		ZInt ContainerWeightWidth
		{
			get
			{
				return ShipmentWrapper.ContainerWeightWidth;
			}
		}
		ZInt ContainerVolumeWidth
		{
			get
			{
				return ShipmentWrapper.ContainerVolumeWidth;
			}
		}
		ZInt ContainerPackagesWidth
		{
			get
			{
				return ShipmentWrapper.ContainerPackagesWidth;
			}
		}
		ZInt ContainerModeWidth
		{
			get
			{
				return ShipmentWrapper.ContainerModeWidth;
			}
		}

		ZString GetMarksAndNumbers()
		{
			ZString marks = "";

			if (!ShipmentWrapper.MarksAndNumbers.IsEmpty)
			{
				marks = GetNewLinesInMarksAndNumbers() + ShipmentWrapper.MarksAndNumbers;
			}

			return marks;
		}

		void BuildGoodsDetails()
		{
			ZString result = ZString.Empty;

			ZString[] wrappedMarks = WrapTextToASpecifiedHeight(GetMarksAndNumbers(), ShipmentWrapper.MarksAndNumbersWidth, ShipmentWrapper.MarksAndNumbsAndDescHeight).Replace("\r\n", "\n").Split('\n');
			ZString[] wrappedPacks = null;
			if (ShipmentWrapper.IncludePackageCountInBOLGoodsDescription == 0)
			{
				wrappedPacks = WrapTextToASpecifiedHeight(GetPackageCount().TrimEndIncludingWhiteSpace('\n').TrimEndIncludingWhiteSpace('\r'), ShipmentWrapper.PackagesWidth, ShipmentWrapper.MarksAndNumbsAndDescHeight).Replace("\r\n", "\n").Split('\n');
			}
			ZString[] wrappedGoodsDesc = WrapTextToASpecifiedHeight(GetGoodsDescriptionToWrap(), ShipmentWrapper.GoodsDescWidth, ShipmentWrapper.MarksAndNumbsAndDescHeight).Replace("\r\n", "\n").Split('\n');
			ZString[] wrappedWeight = WrapTextForAColumn(Weight, ShipmentWrapper.GrossWeightWidth).Replace("\r\n", "\n").Split('\n');
			ZString[] wrappedVolume = WrapTextForAColumn(Volume, ShipmentWrapper.VolumeMeasurementWidth).Replace("\r\n", "\n").Split('\n');

			ZInt marksAndGoodsDescRowsCount = (wrappedMarks.Length > wrappedGoodsDesc.Length) ? wrappedMarks.Length : wrappedGoodsDesc.Length;
			if (ShipmentWrapper.IncludePackageCountInBOLGoodsDescription == 0)
			{
				if (wrappedPacks != null)
				{
					if (wrappedPacks.Length > marksAndGoodsDescRowsCount)
					{
						marksAndGoodsDescRowsCount = wrappedPacks.Length;
					}
				}
			}
			ZInt allowedMarksRowsCount = (marksAndGoodsDescRowsCount < ShipmentWrapper.MarksAndNumbsAndDescHeight) ? marksAndGoodsDescRowsCount : ShipmentWrapper.MarksAndNumbsAndDescHeight;
			ZInt numberOfRowsForContainers = (ShipmentWrapper.MarksAndNumbsAndDescHeight + ShipmentWrapper.NoOfContainerRows) - allowedMarksRowsCount;

			result += BuildGoodsDetailsBody(allowedMarksRowsCount, wrappedMarks, wrappedPacks, wrappedGoodsDesc, wrappedWeight, wrappedVolume);
			BuildFollowOns(marksAndGoodsDescRowsCount, wrappedMarks, wrappedGoodsDesc);
			result += BuildContainerBody(numberOfRowsForContainers);
			if (ContainerNumbers.Length > 1 && numberOfRowsForContainers >= 2 && ContainerNumbers.Length > numberOfRowsForContainers - 2)
			{
				BuildContainerFollowOns(numberOfRowsForContainers - 2);
			}
			fGoodsDetails = result;
		}

		ZString BuildGoodsDetailsBody(ZInt noOfRows, ZString[] wrappedMarks, ZString[] wrappedPacks, ZString[] wrappedGoodsDesc, ZString[] wrappedWeight, ZString[] wrappedVolume)
		{
			ZString result = "";
			for (int i = 0; i < noOfRows; i++)
			{
				result += SetWordsInRow(wrappedMarks, i, ShipmentWrapper.MarksAndNumbersWidth);
				result += FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap);
				if (ShipmentWrapper.IncludePackageCountInBOLGoodsDescription == 0 && wrappedPacks != null)
				{
					result += SetWordsInRow(wrappedPacks, i, ShipmentWrapper.PackagesWidth);
				}
				result += SetWordsInRow(wrappedGoodsDesc, i, ShipmentWrapper.GoodsDescWidth);
				result += FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap);
				result += SetWordsInRow(wrappedWeight, i, ShipmentWrapper.GrossWeightWidth);
				result += FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap);
				result += SetWordsInRow(wrappedVolume, i, ShipmentWrapper.VolumeMeasurementWidth);
				result += "\r\n";
			}

			return result;
		}

		void BuildFollowOns(ZInt totalNoOfRows, ZString[] marksWrapped, ZString[] goodsWrapped)
		{
			if (totalNoOfRows >= ShipmentWrapper.MarksAndNumbsAndDescHeight)
			{
				fFollowOnMarksAndNums = ZString.Empty;
				fFollowOnGoodsDesc = ZString.Empty;

				for (int i = ShipmentWrapper.MarksAndNumbsAndDescHeight; i < totalNoOfRows; i++)
				{
					if (marksWrapped.Length > i)
					{
						fFollowOnMarksAndNums += marksWrapped[i] + "\r\n";
					}

					if (goodsWrapped.Length > i)
					{
						fFollowOnGoodsDesc += goodsWrapped[i] + "\r\n";
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Zubin says BOL's are English Only.")]
		ZString BuildContainerBody(ZInt rowsAllowedForContainers)
		{
			ZString result = "";
			if (ContainersExist)
			{
				result += "\r\n";
				result += "Container" + FillWithSpaces(ContainerNumberWidth - "Container".Length);
				result += "Seal" + FillWithSpaces(ContainerSealWidth - "Seal".Length);
				result += "Type" + FillWithSpaces(ContainerTypeWidth - "Type".Length);
				result += "Weight(KG)" + FillWithSpaces(ContainerWeightWidth - "Weight(KG)".Length);
				result += "Volume(M3)" + FillWithSpaces(ContainerVolumeWidth - "Volume(M3)".Length);
				result += "Packages" + FillWithSpaces(ContainerPackagesWidth - "Packages".Length);
				result += "Mode" + FillWithSpaces(ContainerModeWidth - "Mode".Length);
				result += "\r\n";

				ZInt containerRowCount = rowsAllowedForContainers - 2; //To include Container heading & space between goods desc
				ZInt containerRows = ContainerSeals.Length > containerRowCount ? containerRowCount : (ZInt)ContainerSeals.Length;
				for (int i = 0; i < containerRows; i++)
				{
					result += SetWordsInRow(ContainerNumbers, i, ContainerNumberWidth);
					result += SetWordsInRow(ContainerSeals, i, ContainerSealWidth);
					result += SetWordsInRow(ContainerTypes, i, ContainerTypeWidth);
					result += SetWordsInRow(ContainerWeights, i, ContainerWeightWidth);
					result += SetWordsInRow(ContainerVolumes, i, ContainerVolumeWidth);
					result += SetWordsInRow(ContainerPacks, i, ContainerPackagesWidth);
					result += SetWordsInRow(ContainerModes, i, ContainerModeWidth);
					result += "\r\n";

					if (i < ContainerModes.Length)
					{
						if (fShipperLoadAndCount.IsEmpty)
						{
							fShipperLoadAndCount = GetShipperLoadAndCountText(ContainerModes[i]);
						}
					}
				}
			}
			return result;
		}

		void BuildContainerFollowOns(ZInt numberOfRowsLeft)
		{
			fFollowOnContainerNumber = ZString.Join("\r\n", ContainerNumbers, numberOfRowsLeft, ContainerNumbers.Length - numberOfRowsLeft).TrimEnd('\r', '\n');
			fFollowOnContainerSealNum = ZString.Join("\r\n", ContainerSeals, numberOfRowsLeft, ContainerSeals.Length - numberOfRowsLeft).TrimEnd('\r', '\n');
			fFollowOnContainerType = ZString.Join("\r\n", ContainerTypes, numberOfRowsLeft, ContainerTypes.Length - numberOfRowsLeft).TrimEnd('\r', '\n');
			fFollowOnContainerWeight = ZString.Join("\r\n", ContainerWeights, numberOfRowsLeft, ContainerWeights.Length - numberOfRowsLeft).TrimEnd('\r', '\n');
			fFollowOnContainerVolume = ZString.Join("\r\n", ContainerVolumes, numberOfRowsLeft, ContainerVolumes.Length - numberOfRowsLeft).TrimEnd('\r', '\n');
			fFollowOnContainerPackages = ZString.Join("\r\n", ContainerPacks, numberOfRowsLeft, ContainerPacks.Length - numberOfRowsLeft).TrimEnd('\r', '\n');
			fFollowOnContainerMode = ZString.Join("\r\n", ContainerModes, numberOfRowsLeft, ContainerModes.Length - numberOfRowsLeft).TrimEnd('\r', '\n');

			fFollowOnShipperLoadAndCount = GetShipperLoadAndCountText(fFollowOnContainerMode);
		}

		ZBool ContainersExist
		{
			get
			{
				return (ContainerNumbers.Length > 0 || ContainerSeals.Length > 0 ||
					ContainerTypes.Length > 0 || ContainerWeights.Length > 0 ||
					ContainerVolumes.Length > 0 || ContainerPacks.Length > 0 || ContainerModes.Length > 0);
			}
		}

		protected ZString FillWithSpaces(ZInt numberOfTimes)
		{
			return numberOfTimes > 0 ? new string(' ', numberOfTimes) : string.Empty;
		}

		ZString SetWordsInRow(ZString[] wrappedWords, ZInt row, ZInt width)
		{
			ZString result = ZString.Empty;
			ZInt wordLength = 0;
			if (wrappedWords.Length > row)
			{
				result += wrappedWords[row];
				wordLength = wrappedWords[row].Length;
			}
			if (wordLength < width)
			{
				for (int x = 0; x < (width - wordLength); x++)
				{
					result += " ";
				}
			}
			return result;
		}

		ZString WrapTextToASpecifiedHeight(ZString value, ZInt columnWidth, ZInt height)
		{
			return WrapText(value, columnWidth, ZBool.True, height);
		}

		ZString GetShipperLoadAndCountText(ZString containerModeText)
		{
			return containerModeText.Contains("*") ? (NoResString)"* Shipper Load and Count" : "";
		}

		#endregion

		#region Order Numbers

		public ZString OrderNumbers
		{
			get
			{
				return ShipmentWrapper.OrderNumbers;
			}
		}

		#endregion

		#region IFormedPagesSupporter

		public DocBillOfLadingFormedPageCollection FormedPages
		{
			get
			{
				if (formedPages == null)
				{
					formedPages = new DocBillOfLadingFormedPageCollection(ShipmentWrapper, this, Factory);
				}

				return formedPages;
			}
		}
		DocBillOfLadingFormedPageCollection formedPages;

		DocFormedPagesShipmentCollection IFormedPagesSupporter.Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new DocFormedPagesShipmentCollection(Factory);
					PopulateBillOfLadingShipments(ShipmentWrapper);
				}

				return shipments;
			}
		}
		DocFormedPagesShipmentCollection shipments;

		void PopulateBillOfLadingShipments(DocForwardingShipment shipment)
		{
			if (shipment.IsMasterCoLoadShipmentWithSubShipments)
			{
				foreach (DocForwardingShipment subShipment in shipment.ColoadShipments)
				{
					PopulateBillOfLadingShipments(subShipment);
				}

				if (!shipment.BillOfLading.ExportStatement.IsEmpty)
				{
					DocFormedPagesShipment docFormedPagesShipment = new DocFormedPagesShipment()
					{
						GoodsDescription = shipment.BillOfLading.ExportStatement.Trim(),
					};

					shipments.Add(docFormedPagesShipment);
				}
			}
			else
			{
				ZString goodsDescription = shipment.BillOfLading.GetGoodsDescriptionToWrap();
				if (!ExportStatement.IsEmpty)
				{
					if (!goodsDescription.IsEmpty)
					{
						goodsDescription += "\n";
					}

					goodsDescription += shipment.BillOfLading.ExportStatement;
				}

				DocFormedPagesShipment docFormedPagesShipment = new DocFormedPagesShipment()
				{
					GoodsDescription = goodsDescription,
					MarksAndNumbers = shipment.MarksAndNumbers,
					Weight = shipment.BillOfLading.Weight.Trim(),
					Volume = shipment.BillOfLading.Volume.Trim(),
					PackageCount = shipment.BillOfLading.PackageCount.Trim(),
				};

				shipments.Add(docFormedPagesShipment);
			}
		}

		ZString PackageCount
		{
			get
			{
				if (packageCount.IsEmpty)
				{
					if (IsFCLAndSEA)
					{
						GoodsDescriptionContainerListingHelper containerHelper = GetContainerListingHelper();
						foreach (ZString containerCount in containerHelper.ContainerCountList)
						{
							packageCount += containerCount + "\n";
						}
					}
					else
					{
						packageCount = GetPackageCount();
					}
				}

				return packageCount;
			}
		}
		ZString packageCount;

		DocFormedPagesContainerCollection IFormedPagesSupporter.Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new DocFormedPagesContainerCollection(Factory);

					foreach (DocBillofLadingContainer container in Containers)
					{
						containers.Add(new DocFormedPagesContainer(container, ShipmentWrapper));
					}

					containers.Sort("ContainerNumber");
				}

				return containers;
			}
		}
		DocFormedPagesContainerCollection containers;

		bool IFormedPagesSupporter.DisplayContainers
		{
			get { return Containerized || ((IFormedPagesSupporter)this).Containers.Count > 0; }
		}

		bool IFormedPagesSupporter.HideContainerGrossWeight
		{
			get { return ShipmentWrapper == null || ShipmentWrapper.ShowContainerGross != 1; }
		}

		bool IFormedPagesSupporter.HideContainerTareWeight
		{
			get { return ShipmentWrapper == null || ShipmentWrapper.ShowContainerGross != 1; }
		}

		bool IFormedPagesSupporter.HidePackLinesInContainersSection
		{
			get { return !FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.Value; }
		}

		DocFormedPagesTopLevelPackCollection IFormedPagesSupporter.TopLevelPacks
		{
			get { return topLevelPacks ?? (topLevelPacks = new DocFormedPagesTopLevelPackCollection(Factory)); }
		}
		DocFormedPagesTopLevelPackCollection topLevelPacks;

		DocPackLinesCollection IFormedPagesSupporter.PackLines
		{
			get { return ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments ? MasterCoLoadPackLines : ShipmentWrapper.OuterPackLineCollection; }
		}

		DocJobChargeCollection IFormedPagesSupporter.AllCharges =>
			DocJobChargeCollection.GetCollection(ShipmentWrapper, nameof(IFormedPagesSupporter.AllCharges),
				(allCharges) =>
				{
					if (ShipmentWrapper.JobHeader != null)
					{
						allCharges.AddRange(ShipmentWrapper.JobHeader.JobChargesForAgentCollect);
						allCharges.AddRange(ShipmentWrapper.JobHeader.JobChargesForLocalClient);
					}
				});

		DocJobChargeCollection IFormedPagesSupporter.CollectCharges => ShipmentWrapper.JobHeader?.JobChargesForAgentCollect ?? DocJobChargeCollection.GetCollection(ShipmentWrapper, (NoResString)"Empty");

		public ZString[] FollowOnSection
		{
			get { return FormedPages.FollowOnSection; }
		}

		public ZBool HasFollowOnSection
		{
			get { return FormedPages.HasFollowOnSection; }
		}

		public bool IsCopy
		{
			get { return ShipmentWrapper.ReportName.ToUpper().Contains("COPY"); }
		}

		public bool IsOriginal
		{
			get { return ShipmentWrapper.ReportName.ToUpper().Contains("ORIGINAL"); }
		}

		#endregion

		#region Main Body Sections (Legacy)

		public ZString MainBodySections
		{
			get
			{
				if (fMainBodySections.IsEmpty)
				{
					fMainBodySections = GetMainBodyTopSection();
					fMainBodySections = !fMainBodySections.IsEmpty ? fMainBodySections + "\n" + GapBetweenTopAndBottomSection + GetMainBodyBottomSection() : (string)GetMainBodyBottomSection();
				}
				return fMainBodySections;
			}
		}

		ZString fMainBodySections;

		public ZString MainBodyTopSection
		{
			get { return GetMainBodyTopSection(); }
		}

		public ZString MainBodyBottomColumnSection
		{
			get { return MainBodyBottomColumnTextSection.ToString(); }
		}

		protected TextSection MainBodyBottomColumnTextSection
		{
			get { return mainBodyBottomColumnSection ?? (mainBodyBottomColumnSection = GetMainBodyBottomColumnSection()); }
		}
		TextSection mainBodyBottomColumnSection;

		public ZString FollowOnBodySections
		{
			get
			{
				if (fFollowOnBodySections.IsEmpty)
				{
					fFollowOnBodySections = FollowOnBodyTextSection.ToString();
				}
				return fFollowOnBodySections;
			}
		}
		ZString fFollowOnBodySections;

		protected TextSection FollowOnBodyTextSection
		{
			get { return followOnBodyTextSection ?? (followOnBodyTextSection = GetFollowOnBodyTextSection()); }
		}
		TextSection followOnBodyTextSection;

		TextSection GetFollowOnBodyTextSection()
		{
			TextSection section = new TextSection(ShipmentWrapper.MaxLineLength);
			section.Merge(FollowOnBodyTopSection);
			section.Merge(FollowOnBodyBottomSection);

			if (!FollowOnPrepaidAndCollectCharges.IsEmpty)
			{
				section.Add((NoResString)"Charges");
				section.AddRange(FollowOnPrepaidAndCollectCharges.Split('\n').Select((s) => new TextLine(s)));
			}

			return section;
		}

		protected TextSection FollowOnBodyTopSection
		{
			get { return followOnBodyTopSection ?? (followOnBodyTopSection = GetFollowOnBodyTopSection()); }
		}
		TextSection followOnBodyTopSection;

		protected TextSection FollowOnBodyBottomSection
		{
			get { return followOnBodyBottomSection ?? (followOnBodyBottomSection = GetFollowOnBodyBottomSection()); }
		}
		TextSection followOnBodyBottomSection;

		public ZString[] FollowOnBodySectionsCollection
		{
			get { return FollowOnBodyTextSection.ToStringArray(); }
		}

		public ZBool HasFollowOnBodySections
		{
			get { return !FollowOnBodySections.IsEmpty; }
		}

		#region AUTO-Aligned: Marks and Numbers, Goods Description, Weight and Volume, Containers Listing

		#region Local Settings (Calculated)

		#region HeightNeededForTopSection

#if DEBUG
		protected
#endif
 ZInt HeightNeededForTopSection
		{
			get
			{
				if (fHeightNeededForTopSection.IsEmpty)
				{
					fHeightNeededForTopSection = Math.Max(MarksAndNumbersStringCollection.Count, GoodsDescriptionStringCollection.Count);
					fHeightNeededForTopSection = Math.Max(fHeightNeededForTopSection, ShipmentWeightCollection.Count);
					fHeightNeededForTopSection = Math.Max(fHeightNeededForTopSection, ShipmentVolumeCollection.Count);
					if (ShipmentWrapper.ShowPackageCount == 1)
					{
						fHeightNeededForTopSection = Math.Max(fHeightNeededForTopSection, PackageCountStringCollection.Count);
					}
				}
				return fHeightNeededForTopSection;
			}
		}
		ZInt fHeightNeededForTopSection;

		#endregion

		#region MainBodyTopSectionHeight

#if DEBUG
		protected
#endif
 ZInt MainBodyTopSectionHeight
		{
			get
			{
				if (fMainBodyTopSectionHeight.IsEmpty)
				{
					if (ShipmentWrapper.ContainerHeadingsAreInFixedPosition == 0)
					{
						if (HeightNeededForTopSection > ShipmentWrapper.MarksAndNumbsAndDescHeight)
						{
							fMainBodyTopSectionHeight = ShipmentWrapper.MarksAndNumbsAndDescHeight;

							//See if we can use some height from the container section
							if (ContainerColumnsSection.Height < ShipmentWrapper.NoOfContainerRows - 2)
							{
								int heightAvailable = (ShipmentWrapper.NoOfContainerRows - 2) - ContainerColumnsSection.Height;
								int heightNeeded = HeightNeededForTopSection - fMainBodyTopSectionHeight;
								fMainBodyTopSectionHeight += (heightNeeded < heightAvailable) ? heightNeeded : heightAvailable;
							}
						}
						else
						{
							fMainBodyTopSectionHeight = HeightNeededForTopSection;
						}
					}
					else
					{
						fMainBodyTopSectionHeight = ShipmentWrapper.MarksAndNumbsAndDescHeight;
					}
				}
				return fMainBodyTopSectionHeight;
			}
		}
		ZInt fMainBodyTopSectionHeight;

		#endregion

		#region MainBodyBottomSectionHeight

#if DEBUG
		protected
#endif
 ZInt MainBodyBottomSectionHeight
		{
			get
			{
				if (fMainBodyBottomSectionHeight.IsEmpty)
				{
					if (ShipmentWrapper.ContainerHeadingsAreInFixedPosition == 0)
					{
						if (ContainerColumnsSection.Height <= ShipmentWrapper.NoOfContainerRows - 2)
						{
							fMainBodyBottomSectionHeight = ContainerColumnsSection.Height;
						}
						else
						{
							fMainBodyBottomSectionHeight = ShipmentWrapper.NoOfContainerRows - 2;

							// See if we can use some height from the top section
							int bOLClauseHeight = (int)Math.Ceiling(BOLClauseForGoodsDescriptionSection.Length / (decimal)ApproxWordsPerLineForBOLClause);
							if (MainBodyTopSectionHeight < ShipmentWrapper.MarksAndNumbsAndDescHeight - bOLClauseHeight)
							{
								int heightAvailable = ShipmentWrapper.MarksAndNumbsAndDescHeight - MainBodyTopSectionHeight - bOLClauseHeight;
								int heightNeeded = ContainerColumnsSection.Height - (ShipmentWrapper.NoOfContainerRows - 2);
								fMainBodyBottomSectionHeight += (heightNeeded < heightAvailable) ? heightNeeded : heightAvailable;
							}
						}
					}
					else
					{
						fMainBodyBottomSectionHeight = ShipmentWrapper.NoOfContainerRows - 2;
					}
				}
				return fMainBodyBottomSectionHeight;
			}
		}
		ZInt fMainBodyBottomSectionHeight;

		const int ApproxWordsPerLineForBOLClause = 40;

		#endregion

		#region FollowOnBodyTopSectionHeight

#if DEBUG
		protected
#endif
 ZInt FollowOnBodyTopSectionHeight
		{
			get
			{
				if (fFollowOnBodyTopSectionHeight.IsEmpty && HeightNeededForTopSection > MainBodyTopSectionHeight)
				{
					fFollowOnBodyTopSectionHeight = HeightNeededForTopSection - MainBodyTopSectionHeight;
				}
				return fFollowOnBodyTopSectionHeight;
			}
		}
		ZInt fFollowOnBodyTopSectionHeight;

		#endregion

		#region FollowOnBodyBottomSectionHeight

#if DEBUG
		protected
#endif
 ZInt FollowOnBodyBottomSectionHeight
		{
			get
			{
				if (fFollowOnBodyBottomSectionHeight.IsEmpty && MainBodyBottomSectionHeight < ContainerColumnsSection.Height)
				{
					fFollowOnBodyBottomSectionHeight = ContainerColumnsSection.Height - MainBodyBottomSectionHeight;
				}
				return fFollowOnBodyBottomSectionHeight;
			}
		}
		ZInt fFollowOnBodyBottomSectionHeight;

		#endregion

		#endregion

		#region MainBodySections

		#region GetMainBodyTopSection (Marks & Numbers, Goods Description, Weight, Volume)

#if DEBUG
		protected
#endif
 ZString GetMainBodyTopSection()
		{
			ZString result = "";
			if (MainBodyTopSectionHeight > 0)
			{
				for (int i = 0; i < MainBodyTopSectionHeight; i++)
				{
					result += GetTopSectionLineAt(i) + "\n";
				}
			}
			return result.Trim('\n');
		}

		#endregion

		ZString GapBetweenTopAndBottomSection
		{
			get
			{
				ZString result = "";
				for (int i = 0; i < ShipmentWrapper.GapBetweenTopAndBottomSection; i++)
				{
					result += "\n";
				}
				return result;
			}
		}

		#region GetMainBodyBottomSection (Container, Seal, Type, Weight, Volume, Packages, Mode)

#if DEBUG
		protected
#endif
 ZString GetMainBodyBottomSection()
		{
			ZString result = "";
			if (MainBodyBottomSectionHeight > 0 && Containers.Count > 0)
			{
				result = ContainersColumnHeaders + "\n" + MainBodyBottomColumnSection;
			}
			return result.Trim('\n');
		}

		TextSection GetMainBodyBottomColumnSection()
		{
			TextSection result = new TextSection(ShipmentWrapper.MaxLineLength);

			if (MainBodyBottomSectionHeight > 0 && ContainerColumnsSection.Count > 0)
			{
				for (int i = 0; i < ContainerColumnsSection.Count; i++)
				{
					TextLine lineToAdd = ContainerColumnsSection[i];

					if (i == 0 || lineToAdd.Height + result.Height <= MainBodyBottomSectionHeight)
					{
						result.Add(lineToAdd);

						if (mainBodyShipperLoadAndCount.IsEmpty && !containerLoadAndCount[i].IsEmpty)
						{
							mainBodyShipperLoadAndCount = containerLoadAndCount[i];
						}
					}
					else
					{
						break;
					}
				}
			}

			return result;
		}

		void BuildContainerSection()
		{
			bool showPackingDetails = FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.Value;

			containerColumnsSection = new TextSection(ShipmentWrapper.MaxLineLength);
			containerLoadAndCount = new List<ZString>();

			foreach (DocBillofLadingContainer container in Containers)
			{
				BuildSingleColumnRow(container);
				AddExcessColumnRows(container.ContainerSeal); //seal can take up multiple rows

				if (showPackingDetails)
				{
					foreach (TextLine packingDetails in container.GetPackingDetailsBreakdown(ShipmentWrapper.MaxLineLength))
					{
						containerColumnsSection.Add(packingDetails);
						containerLoadAndCount.Add("");
					}
				}
			}
		}

		void BuildSingleColumnRow(DocBillofLadingContainer container)
		{
			containerColumnsSection.Add(GetMainContainerLine(container));
			containerLoadAndCount.Add(GetShipperLoadAndCountText(container.DeliveryMode));
		}

		void AddExcessColumnRows(ZString seal)
		{
			ZString sealToSplit = seal.Trim();
			if (sealToSplit.Length > ShipmentWrapper.ContainerSealWidth)
			{
				ZString sealLineToAdd = sealToSplit.Substring(ShipmentWrapper.ContainerSealWidth).Trim();
				if (sealLineToAdd.Length > 0)
				{
					containerColumnsSection.Add(GetMainContainerLineSealNumberOnly(sealLineToAdd));
					containerLoadAndCount.Add(GetShipperLoadAndCountText("")); //Ensure same length
					AddExcessColumnRows(sealLineToAdd);
				}
			}
		}

		public TextSection ContainerColumnsSection
		{
			get
			{
				if (containerColumnsSection == null)
				{
					BuildContainerSection();
				}
				return containerColumnsSection;
			}
		}
		TextSection containerColumnsSection;
		List<ZString> containerLoadAndCount;

		#endregion

		#endregion

		#region FollowOnBodySections

		#region GetFollowOnBodyTopSection (Marks & Numbers, Goods Description, Weight, Volume)

#if DEBUG
		protected
#endif
 TextSection GetFollowOnBodyTopSection()
		{
			TextSection result = new TextSection(ShipmentWrapper.MaxLineLength);

			if (FollowOnBodyTopSectionHeight > 0)
			{
				result.Add(MarksAndNumbersGoodsDescriptionHeaders);

				for (int i = MainBodyTopSectionHeight; i < MainBodyTopSectionHeight + FollowOnBodyTopSectionHeight; i++)
				{
					result.Add(GetTopSectionLineAt(i));
				}
			}

			return result;
		}

		#endregion

		#region GetFollowOnBodyBottomSection (Container, Seal, Type, Weight, Volume, Packages, Mode)

#if DEBUG
		protected
#endif
 TextSection GetFollowOnBodyBottomSection()
		{
			TextSection result = new TextSection(ShipmentWrapper.MaxLineLength);

			if (FollowOnBodyBottomSectionHeight > 0 && MainBodyBottomColumnTextSection.Count < ContainerColumnsSection.Count)
			{
				result.Add(ContainersColumnHeaders);

				for (int i = MainBodyBottomColumnTextSection.Count; i < ContainerColumnsSection.Count; i++)
				{
					result.Add(ContainerColumnsSection[i]);

					if (followOnBodyShipperLoadAndCount.IsEmpty && !containerLoadAndCount[i].IsEmpty)
					{
						followOnBodyShipperLoadAndCount = containerLoadAndCount[i];
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Section Lines, Tools & Collections

		#region Fill Lines With Spaces

		StringCollectionX FillLinesWithSpaces(int currentHeight, int maxHeight, int width)
		{
			StringCollectionX result = new StringCollectionX();
			for (int i = currentHeight; i < maxHeight; i++)
			{
				result.Add(FillWithSpaces(width));
			}
			return result;
		}

		#endregion

		#region MarksAndNumbersStringCollection

#if DEBUG
		protected
#endif
 StringCollectionX MarksAndNumbersStringCollection
		{
			get
			{
				if (fMarksAndNumbersStringCollection == null)
				{
					if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
					{
						fMarksAndNumbersStringCollection = new StringCollectionX();
						for (int i = 0; i < ShipmentWrapper.ColoadShipments.Count; i++)
						{
							DocForwardingShipment subShipment = (DocForwardingShipment)ShipmentWrapper.ColoadShipments[i];
							fMarksAndNumbersStringCollection.AddRange(subShipment.BillOfLading.MarksAndNumbersStringCollection);
							if (i + 1 < ShipmentWrapper.ColoadShipments.Count)
							{
								fMarksAndNumbersStringCollection.Add(new ZString().PadRight(ShipmentWrapper.MarksAndNumbersWidth, '-'));
							}
						}
					}
					else
					{
						fMarksAndNumbersStringCollection = ConvertToStringCollection(ShipmentWrapper.MarksAndNumbers, ShipmentWrapper.MarksAndNumbersWidth);
						fMarksAndNumbersStringCollection.AddRange(FillLinesWithSpaces(fMarksAndNumbersStringCollection.Count, HeightNeededForTopSection, ShipmentWrapper.MarksAndNumbersWidth));
					}
				}
				return fMarksAndNumbersStringCollection;
			}
		}

		StringCollectionX fMarksAndNumbersStringCollection;
		#endregion

		#region PackageCountStringCollection
		StringCollectionX PackageCountStringCollection
		{
			get
			{
				if (fPackageCountStringCollection == null)
				{
					if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
					{
						fPackageCountStringCollection = new StringCollectionX();
						for (int i = 0; i < ShipmentWrapper.ColoadShipments.Count; i++)
						{
							DocForwardingShipment subShipment = (DocForwardingShipment)ShipmentWrapper.ColoadShipments[i];
							fPackageCountStringCollection.AddRange(subShipment.BillOfLading.PackageCountStringCollection);
							if (i + 1 < ShipmentWrapper.ColoadShipments.Count)
							{
								fPackageCountStringCollection.Add(new ZString().PadRight(ShipmentWrapper.PackagesWidth, '-'));
							}
						}
					}
					else if (IsFCLAndSEA)
					{
						GoodsDescriptionContainerListingHelper containerHelper = GetContainerListingHelper();
						if (containerHelper != null)
						{
							fPackageCountStringCollection = new StringCollectionX();
							foreach (ZString containerCount in containerHelper.ContainerCountList)
							{
								fPackageCountStringCollection.Add(containerCount.PadLeft(ShipmentWrapper.PackagesWidth));
							}

							fPackageCountStringCollection.AddRange(FillLinesWithSpaces(fPackageCountStringCollection.Count, HeightNeededForTopSection, ShipmentWrapper.PackagesWidth));
						}
					}
					else
					{
						fPackageCountStringCollection = ConvertToStringCollection(GetPackageCount(), ShipmentWrapper.PackagesWidth);
						fPackageCountStringCollection.AddRange(FillLinesWithSpaces(fPackageCountStringCollection.Count, HeightNeededForTopSection, ShipmentWrapper.PackagesWidth));
					}
				}
				return fPackageCountStringCollection;
			}
		}

		StringCollectionX fPackageCountStringCollection;

		#endregion

		#region GoodsDescriptionStringCollection

#if DEBUG
		protected
#endif
 StringCollectionX GoodsDescriptionStringCollection
		{
			get
			{
				if (fGoodsDescriptionStringCollection == null)
				{
					if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
					{
						fGoodsDescriptionStringCollection = new StringCollectionX();
						for (int i = 0; i < ShipmentWrapper.ColoadShipments.Count; i++)
						{
							DocForwardingShipment subShipment = (DocForwardingShipment)ShipmentWrapper.ColoadShipments[i];
							fGoodsDescriptionStringCollection.AddRange(subShipment.BillOfLading.GoodsDescriptionStringCollection);
							if (i + 1 < ShipmentWrapper.ColoadShipments.Count)
							{
								fGoodsDescriptionStringCollection.Add(new ZString().PadRight(ShipmentWrapper.GoodsDescWidth, '-'));
							}
						}

						if (!ExportStatement.IsEmpty)
						{
							fGoodsDescriptionStringCollection.AddRange(ConvertToStringCollection(ExportStatement, ShipmentWrapper.GoodsDescWidth));
						}
					}
					else
					{
						fGoodsDescriptionStringCollection = ConvertToStringCollection(GetGoodsDescriptionToWrap() + ExportStatement, ShipmentWrapper.GoodsDescWidth);
						fGoodsDescriptionStringCollection.AddRange(FillLinesWithSpaces(fGoodsDescriptionStringCollection.Count, HeightNeededForTopSection, ShipmentWrapper.GoodsDescWidth));
					}
				}

				return fGoodsDescriptionStringCollection;
			}
		}

		StringCollectionX fGoodsDescriptionStringCollection;

		ZString ExportStatement
		{
			get
			{
				ZString result = ZString.Empty;

				// User Defined
				DocExportStatementSetting exportStatementSetting = ShipmentWrapper.ExportStatementSetting;
				if (exportStatementSetting != null &&
					exportStatementSetting.UseOnHouseBillOfLading &&
					!ShipmentWrapper.ExportStatement.IsEmpty)
				{
					result = "\r\n" + ShipmentWrapper.ExportStatement;
				}

				// Mandatory
				if (ShipmentWrapper != null)
				{
					foreach (ExportStatementSetting mandatorySetting in FreightDataRegistry.Instance.ExportStatementSettings.Value.GetMandatoryStatements(ShipmentWrapper.Origin.Left(2)))
					{
						if (mandatorySetting.UseOnHouseBillOfLading && !mandatorySetting.Statement.IsEmpty)
						{
							result = "\r\n" + mandatorySetting.Statement;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region ShipmentWeightCollection

#if DEBUG
		protected
#endif
 StringCollectionX ShipmentWeightCollection
		{
			get
			{
				if (fShipmentWeightCollection == null)
				{
					fShipmentWeightCollection = new StringCollectionX();

					if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
					{
						for (int i = 0; i < ShipmentWrapper.ColoadShipments.Count; i++)
						{
							DocForwardingShipment subShipment = (DocForwardingShipment)ShipmentWrapper.ColoadShipments[i];
							fShipmentWeightCollection.AddRange(subShipment.BillOfLading.ShipmentWeightCollection);
							if (i + 1 < ShipmentWrapper.ColoadShipments.Count)
							{
								fShipmentWeightCollection.Add(new ZString().PadRight(ShipmentWrapper.GrossWeightWidth, '-'));
							}
						}
					}
					else
					{
						ZDecimal bOLWeight = GetBillOfLadingMeasurementValue(ShipmentWrapper.ActualWeight, ShipmentWrapper.DocumentedWeight, ShipmentWrapper.ManifestedWeight);

						if (ShipmentWrapper.UnitOfWeight != Core.Constants.Weight.Kilograms && ShipmentWrapper.UnitOfWeight != ZString.Empty)
						{
							ZDecimal convertedWeight = Core.Constants.Weight.ConvertSafe(bOLWeight, ShipmentWrapper.UnitOfWeight, "KG");
							fShipmentWeightCollection.Add(AlignToWidth(ShipmentWrapper.FormatNumber(convertedWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay) + " KG", ShipmentWrapper.GrossWeightWidth));
							fShipmentWeightCollection.Add(AlignToWidth("(" + bOLWeight.ToStringTrimZeros() + " " + ShipmentWrapper.UnitOfWeight + ")", ShipmentWrapper.GrossWeightWidth));
						}
						else if (bOLWeight != 0M)
						{
							fShipmentWeightCollection.Add(AlignToWidth(ShipmentWrapper.FormatNumber(bOLWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay) + " " + ShipmentWrapper.UnitOfWeight, ShipmentWrapper.GrossWeightWidth));
						}

						fShipmentWeightCollection.AddRange(FillLinesWithSpaces(fShipmentWeightCollection.Count, HeightNeededForTopSection, ShipmentWrapper.GrossWeightWidth));
					}
				}
				return fShipmentWeightCollection;
			}
		}
		StringCollectionX fShipmentWeightCollection;

		#endregion

		#region ShipmentVolumeCollection

#if DEBUG
		protected
#endif
 StringCollectionX ShipmentVolumeCollection
		{
			get
			{
				if (fShipmentVolumeCollection == null)
				{
					fShipmentVolumeCollection = new StringCollectionX();

					if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
					{
						for (int i = 0; i < ShipmentWrapper.ColoadShipments.Count; i++)
						{
							DocForwardingShipment subShipment = (DocForwardingShipment)ShipmentWrapper.ColoadShipments[i];
							fShipmentVolumeCollection.AddRange(subShipment.BillOfLading.ShipmentVolumeCollection);
							if (i + 1 < ShipmentWrapper.ColoadShipments.Count)
							{
								fShipmentVolumeCollection.Add(new ZString().PadRight(ShipmentWrapper.VolumeMeasurementWidth, '-'));
							}
						}
					}
					else
					{
						ZDecimal bOLVolume = GetBillOfLadingMeasurementValue(ShipmentWrapper.ActualVolume, ShipmentWrapper.DocumentedVolume, ShipmentWrapper.ManifestedVolume);
						if (ShipmentWrapper.UnitOfVolume != Core.Constants.Volume.CubicMetres && ShipmentWrapper.UnitOfVolume != ZString.Empty)
						{
							ZDecimal convertedVolume = Core.Constants.Volume.ConvertSafe(bOLVolume, ShipmentWrapper.UnitOfVolume, "M3");
							fShipmentVolumeCollection.Add(AlignToWidth(ShipmentWrapper.FormatNumber(convertedVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay) + " M3", ShipmentWrapper.VolumeMeasurementWidth));
							fShipmentVolumeCollection.Add(AlignToWidth("(" + bOLVolume.ToString() + " " + ShipmentWrapper.UnitOfVolume + ")", ShipmentWrapper.VolumeMeasurementWidth));
						}
						else if (bOLVolume != 0M)
						{
							fShipmentVolumeCollection.Add(AlignToWidth(ShipmentWrapper.FormatNumber(bOLVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay) + " " + ShipmentWrapper.UnitOfVolume, ShipmentWrapper.VolumeMeasurementWidth));
						}

						fShipmentVolumeCollection.AddRange(FillLinesWithSpaces(fShipmentVolumeCollection.Count, HeightNeededForTopSection, ShipmentWrapper.VolumeMeasurementWidth));
					}
				}
				return fShipmentVolumeCollection;
			}
		}
		StringCollectionX fShipmentVolumeCollection;

		ZDecimal GetBillOfLadingMeasurementValue(ZDecimal actualValue, ZDecimal documentedValue, ZDecimal carrierValue)
		{
			if (Env.Registry.BillOfLadingWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Client)
			{
				return documentedValue;
			}
			else if (Env.Registry.BillOfLadingWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Carrier)
			{
				return carrierValue;
			}
			else
			{
				return actualValue;
			}
		}

		#endregion

		#region MarksAndNumbersGoodsDescriptionHeaders

		public ZString MarksAndNumbersGoodsDescriptionHeaders
		{
			get
			{
				return MarksAndNumbersGoodsDescriptionHeadersCore;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Zubin says BOL's are English Only.")]
		protected virtual ZString MarksAndNumbersGoodsDescriptionHeadersCore
		{
			get
			{
				if (fMarksAndNumbersGoodsDescriptionHeaders.IsEmpty)
				{
					fMarksAndNumbersGoodsDescriptionHeaders = "Marks & Numbers" + FillWithSpaces(ShipmentWrapper.MarksAndNumbersWidth - "Marks & Numbers".Length);
					fMarksAndNumbersGoodsDescriptionHeaders += FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap);

					if (ShipmentWrapper.ShowPackageCount == 1)
					{
						fMarksAndNumbersGoodsDescriptionHeaders += "No. of Pkgs." + FillWithSpaces(ShipmentWrapper.PackagesWidth - "No. of Pkgs.".Length);
						fMarksAndNumbersGoodsDescriptionHeaders += FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap);
					}

					fMarksAndNumbersGoodsDescriptionHeaders += "Goods Description" + FillWithSpaces(ShipmentWrapper.GoodsDescWidth - "Goods Description".Length);
					fMarksAndNumbersGoodsDescriptionHeaders += FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap);
					fMarksAndNumbersGoodsDescriptionHeaders += "Gross Wt." + FillWithSpaces(ShipmentWrapper.GrossWeightWidth - "Gross Wt.".Length);
					fMarksAndNumbersGoodsDescriptionHeaders += FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap);
					fMarksAndNumbersGoodsDescriptionHeaders += "Volume" + FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth - "Volume".Length);
				}
				return fMarksAndNumbersGoodsDescriptionHeaders;
			}
		}

		ZString fMarksAndNumbersGoodsDescriptionHeaders;

		#endregion

		#region ContainersColumnHeaders

		public ZString ContainersColumnHeaders
		{
			get
			{
				return ContainersColumnHeadersCore;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Zubin says BOL's are English Only.")]
		protected virtual ZString ContainersColumnHeadersCore
		{
			get
			{
				if (fContainersColumnHeaders.IsEmpty)
				{
					fContainersColumnHeaders += "Container" + FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);

					if (ShipmentWrapper.ShowContainerSeal == 1)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
						fContainersColumnHeaders += "Seals" + FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
					}

					if (ShipmentWrapper.ShowContainerType == 1)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
						fContainersColumnHeaders += "Type" + FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
					}

					if (ShipmentWrapper.ShowContainerWeight == 1)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
						fContainersColumnHeaders += "Weight(KG)" + FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
					}

					if (ShipmentWrapper.ContainerMode == Core.Constants.ContainerModes.FCL)
					{
						if (ShipmentWrapper.ShowContainerTare == 1)
						{
							fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
							fContainersColumnHeaders += "Tare(KG)" + FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
						}

						if (ShipmentWrapper.ShowContainerGross == 1)
						{
							fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
							fContainersColumnHeaders += "Gross(KG)" + FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
						}
					}

					if (ShipmentWrapper.ShowContainerVolume == 1)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap);
						fContainersColumnHeaders += "Volume(M3)" + FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
					}

					if (ShipmentWrapper.ShowContainerPackages == 1)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
						fContainersColumnHeaders += "Packages" + FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length);
					}

					if (ShipmentWrapper.ShowContainerMode == 1)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
						fContainersColumnHeaders += "Mode" + FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length);
					}

					if (ShipmentWrapper.ShowContainerTemperatureSetting)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerTemperatureSettingLeftPadding)
							+ "Temp.".PadRight(ShipmentWrapper.ContainerTemperatureSettingWidth);
					}

					if (ShipmentWrapper.ShowContainerHumiditySetting)
					{
						fContainersColumnHeaders += FillWithSpaces(ShipmentWrapper.ContainerHumiditySettingLeftPadding)
							+ (NoResString)"Humidity".PadRight(ShipmentWrapper.ContainerHumiditySettingWidth);
					}
				}
				return fContainersColumnHeaders;
			}
		}

		protected ZString fContainersColumnHeaders;

		#endregion

		#region Containers

#if DEBUG
		protected
#endif
 DocBillofLadingContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new DocBillofLadingContainerCollection(Factory);

					DocBillofLadingContainer tempContainer;
					if (ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
					{
						MasterCoLoadContainers.Sort("ContainerNumber", ListSortDirection.Ascending);

						foreach (DocContainer currentContainer in MasterCoLoadContainers)
						{
							if (!fContainers.ContainsContainerWithNumber(currentContainer.ContainerNumber))
							{
								tempContainer = GetPopulatedBOLContainer(currentContainer);
								fContainers.Add(tempContainer);

								var packType = currentContainer.TotalAllocatedShipmentPackagesPackType;
								foreach (DocContainer innerContainers in MasterCoLoadContainers)
								{
									if (innerContainers != currentContainer && innerContainers.AllocatedShipmentPackLine != null && currentContainer.ContainerNumber == innerContainers.ContainerNumber)
									{
										tempContainer.Weight += Core.Constants.Weight.ConvertSafe(innerContainers.TotalAllocatedShipmentWeight, currentContainer.GrossWeightUQ, Core.Constants.Weight.Kilograms);
										tempContainer.Volume += Core.Constants.Volume.ConvertSafe(innerContainers.TotalAllocatedShipmentVolume, currentContainer.VolumeUQ, Core.Constants.Volume.CubicMetres);
										tempContainer.Packs += innerContainers.TotalAllocatedShipmentPackages;
										if (packType != innerContainers.TotalAllocatedShipmentPackagesPackType)
										{
											packType = Core.Constants.PkgUnit.Piece;
										}
									}
								}
								tempContainer.PackType = packType;
							}
						}
					}
					else
					{
						ShipmentWrapper.Containers.Sort("ContainerNumber", ListSortDirection.Ascending);

						foreach (DocContainer currentContainer1 in ShipmentWrapper.Containers)
						{
							tempContainer = GetPopulatedBOLContainer(currentContainer1);
							fContainers.Add(tempContainer);
						}
					}
				}
				return fContainers;
			}
		}

		DocBillofLadingContainerCollection fContainers;

		DocBillofLadingContainer GetPopulatedBOLContainer(DocContainer docContainer)
		{
			var result = DocBillofLadingContainer.New(docContainer);
			result.Weight = Core.Constants.Weight.ConvertSafe(docContainer.TotalAllocatedShipmentWeight, docContainer.GrossWeightUQ, Core.Constants.Weight.Kilograms);
			result.Volume = Core.Constants.Volume.ConvertSafe(docContainer.TotalAllocatedShipmentVolume, docContainer.VolumeUQ, Core.Constants.Volume.CubicMetres);
			result.Packs = docContainer.TotalAllocatedShipmentPackages;
			result.PackType = docContainer.TotalAllocatedShipmentPackagesPackType;
			if (!ShipmentWrapper.HBLContainerPackModeOverride.IsEmpty)
			{
				result.DeliveryMode = ShipmentWrapper.HBLContainerPackModeOverride;
			}
			result.ContainerMode = docContainer.ContainerMode;

			return result;
		}

		#endregion

		#region Section Lines & Shipper Load Counts

		protected virtual ZString GetTopSectionLineAt(ZInt index)
		{
			ZString line = "";
			if (index >= 0)
			{
				line += GetSegment(MarksAndNumbersStringCollection, index, ShipmentWrapper.MarksAndNumbersWidth)
					+ FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap);
				if (ShipmentWrapper.ShowPackageCount == 1)
				{
					line += GetSegment(PackageCountStringCollection, index, ShipmentWrapper.PackagesWidth)
						 + FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap);
				}
				line += GetSegment(GoodsDescriptionStringCollection, index, ShipmentWrapper.GoodsDescWidth)
					+ FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)

					+ GetSegment(ShipmentWeightCollection, index, ShipmentWrapper.GrossWeightWidth)
					+ FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)

					+ GetSegment(ShipmentVolumeCollection, index, ShipmentWrapper.VolumeMeasurementWidth);
			}
			return line;
		}

		ZString GetSegment(StringCollectionX segments, ZInt index, ZInt width)
		{
			return (segments.Count > index) ? (ZString)segments[index] : FillWithSpaces(width);
		}

		ZString GetMainContainerLineSealNumberOnly(ZString seal)
		{
			return AlignToWidth("", ShipmentWrapper.ContainerNumberWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ AlignToWidth(seal, ShipmentWrapper.ContainerSealWidth);
		}

		protected virtual ZString GetMainContainerLine(DocBillofLadingContainer currentContainer)
		{
			ZString line = AlignToWidth(currentContainer.ContainerNumber, ShipmentWrapper.ContainerNumberWidth);

			if (ShipmentWrapper.ShowContainerSeal == 1)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
						+ AlignToWidth(currentContainer.ContainerSeal, ShipmentWrapper.ContainerSealWidth);
			}

			if (ShipmentWrapper.ShowContainerType == 1)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
						+ AlignToWidth(currentContainer.ContainerType, ShipmentWrapper.ContainerTypeWidth);
			}

			if (ShipmentWrapper.ShowContainerWeight == 1)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
						+ AlignToWidth(ShipmentWrapper.FormatNumber(currentContainer.Weight, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth);
			}

			if (ShipmentWrapper.ContainerMode == Core.Constants.ContainerModes.FCL)
			{
				if (ShipmentWrapper.ShowContainerTare == 1)
				{
					line += FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap)
							+ AlignToWidth(ShipmentWrapper.FormatNumber(currentContainer.ContainerTare, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerTareWidth);
				}

				if (ShipmentWrapper.ShowContainerGross == 1)
				{
					line += FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap)
							+ AlignToWidth(ShipmentWrapper.FormatNumber(currentContainer.ContainerGross, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerGrossWidth);
				}
			}

			if (ShipmentWrapper.ShowContainerVolume == 1)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
						+ AlignToWidth(ShipmentWrapper.FormatNumber(currentContainer.Volume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth);
			}

			if (ShipmentWrapper.ShowContainerPackages == 1)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
						+ AlignToWidth(currentContainer.Packs.ToString() + " " + currentContainer.PackType, ShipmentWrapper.ContainerPackagesWidth);
			}

			if (ShipmentWrapper.ShowContainerMode == 1)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap)
						+ AlignToWidth(currentContainer.DeliveryMode, ShipmentWrapper.ContainerModeWidth);
			}

			if (ShipmentWrapper.ShowContainerTemperatureSetting)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerTemperatureSettingLeftPadding)
						+ AlignToWidth(currentContainer.Temperature, ShipmentWrapper.ContainerTemperatureSettingWidth);
			}

			if (ShipmentWrapper.ShowContainerHumiditySetting)
			{
				line += FillWithSpaces(ShipmentWrapper.ContainerHumiditySettingLeftPadding)
						+ AlignToWidth(currentContainer.Humidity, ShipmentWrapper.ContainerHumiditySettingWidth);
			}

			return line;
		}

		#region Shipper Load And Count

		public ZString MainBodyShipperLoadAndCount
		{
			get
			{
				if (mainBodyShipperLoadAndCount.IsEmpty)
				{
					GetMainBodyBottomColumnSection();
				}

				return mainBodyShipperLoadAndCount;
			}
		}
		ZString mainBodyShipperLoadAndCount;

		public ZString FollowOnBodyShipperLoadAndCount
		{
			get
			{
				if (followOnBodyShipperLoadAndCount.IsEmpty)
				{
					GetFollowOnBodyBottomSection();
				}

				return followOnBodyShipperLoadAndCount;
			}
		}
		ZString followOnBodyShipperLoadAndCount;

		#endregion

		#endregion

		#region Tools

		protected ZString AlignToWidth(ZString value, ZInt maxWidth)
		{
			var result = ZString.Empty;

			if (maxWidth > 0)
			{
				if (value.Length > maxWidth)
				{
					result = value.Substring(0, maxWidth);
				}
				else
				{
					result = value + FillWithSpaces(maxWidth - value.Length);
				}
			}

			return result;
		}

#if DEBUG
		protected
#endif
 StringCollectionX ConvertToStringCollection(ZString value, ZInt maxWidth)
		{
			var result = new StringCollectionX();
			if (maxWidth <= 0)
			{
				return result;
			}

			var currentWord = ZString.Empty;
			var currentText = ZString.Empty;
			var stringValue = value.Replace("\n", " \n ");
			stringValue = stringValue.Replace("\r", " \r ");
			stringValue = stringValue.Replace(" \r  \n ", " \r\n ");
			stringValue = stringValue.Replace("\t", " \t ");

			foreach (ZString word in stringValue.Split(' '))
			{
				currentWord = word;
				if (currentWord == "\t")
				{
					currentWord = "   ";
				}
				else if (currentWord == "\r\n" || currentWord == "\n" || currentWord == "\r")
				{
					result.Add(AlignToWidth(currentText, maxWidth));
					currentText = "";
				}
				else
				{
					if (!currentText.IsEmpty && currentText.Length < maxWidth)
					{
						currentText += " ";
					}

					if (currentText.Length + currentWord.Length > maxWidth)
					{
						if (!currentText.IsEmpty)
						{
							result.Add(AlignToWidth(currentText, maxWidth));
						}
						currentText = "";

						if (currentWord.Length > maxWidth)
						{
							ZString wrappedWord = "";
							ZInt noOfLoops = (currentWord.Length / maxWidth) + 1;
							for (int i = 0; i < noOfLoops; i++)
							{
								wrappedWord = currentWord.SubstringSafe(maxWidth * i, maxWidth);

								if (wrappedWord.Length < maxWidth)
								{
									currentText = wrappedWord;
								}
								else
								{
									result.Add(wrappedWord);
								}
							}
						}
						else
						{
							currentText = currentWord;
						}
					}
					else
					{
						currentText += currentWord;
					}
				}
			}

			if (!currentText.IsEmpty)
			{
				result.Add(AlignToWidth(currentText, maxWidth));
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Co-Load Containers/Pack-Lines

		IDocContainerCollection MasterCoLoadContainers
		{
			get
			{
				if (fMasterCoLoadContainers == null)
				{
					fMasterCoLoadContainers = new IDocContainerCollection(Factory);

					foreach (DocForwardingShipment currentShipment in ShipmentWrapper.ColoadShipments)
					{
						foreach (DocContainer currentContainer in currentShipment.Containers)
						{
							fMasterCoLoadContainers.Add(currentContainer);
						}
					}
				}

				return fMasterCoLoadContainers;
			}
		}

		IDocContainerCollection fMasterCoLoadContainers;

#if DEBUG
		protected
#endif
 DocPackLinesCollection MasterCoLoadPackLines
		{
			get
			{
				if (fMasterCoLoadPackLines == null)
				{
					fMasterCoLoadPackLines = new DocPackLinesCollection(Factory);

					foreach (DocForwardingShipment currentShipment in ShipmentWrapper.ColoadShipments)
					{
						fMasterCoLoadPackLines.AddRange(currentShipment.OuterPackLineCollection);
					}
				}
				return fMasterCoLoadPackLines;
			}
		}

		DocPackLinesCollection fMasterCoLoadPackLines;

		#endregion

		#region Sailing

		JobSailing Sailing
		{
			get { return sailing ?? (sailing = Factory.Load<JobSailing>(ShipmentWrapper.CommonShipment.JS_JX)); }
		}
		JobSailing sailing;

		ZString BookingVesselName
		{
			get
			{
				if (Sailing != null)
				{
					return Sailing.JX_JV_NKVessel;
				}
				return ZString.Empty;
			}
		}

		ZString BookingVoyage
		{
			get
			{
				if (Sailing != null)
				{
					return Sailing.JX_JV_VoyageFlight;
				}
				return ZString.Empty;
			}
		}

		ZString BookingPortOfLoading
		{
			get
			{
				if (Sailing != null)
				{
					return Sailing.JX_JA_RL_NKPortOfLoading;
				}
				return ZString.Empty;
			}
		}

		ZString BookingPortOfDischarge
		{
			get
			{
				if (Sailing != null)
				{
					return Sailing.JX_JB_RL_NKPortOfDischarge;
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region Text Manipulation

#if DEBUG
		internal
#endif
 ZString WrapTextForAColumn(ZString value, ZInt columnWidth)
		{
			return WrapText(value, columnWidth, ZBool.False, 0);
		}

		ZString WrapText(ZString value, ZInt columnWidth, ZBool wrapToAColumnHeight, ZInt columnHeight)
		{
			ZString result = ZString.Empty;
			ZInt count = 0;
			ZInt heightCount = 0;
			ZString currentWord = ZString.Empty;
			ZString columnText = ZString.Empty;
			ZString stringValue = value.Replace("\r\n", "\n").Replace("\n", " \n ");
			stringValue = stringValue.Replace("\t", " \t ");

			ZString longWordWrapped;
			foreach (ZString word in stringValue.Split(' '))
			{
				if (!wrapToAColumnHeight || (wrapToAColumnHeight && heightCount < columnHeight))
				{
					currentWord = word;

					if (currentWord == "\t")
					{
						currentWord = "   ";
					}

					if (currentWord == "\n")
					{
						result += columnText + "\n";
						heightCount++;
						columnText = ZString.Empty;
					}
					else if (columnText.Length + currentWord.Length > columnWidth)
					{
						result += columnText + "\n";
						if (currentWord.Length > columnWidth)
						{
							longWordWrapped = WrapLongWord(currentWord, columnWidth);
							result += longWordWrapped.SubstringSafe(0, longWordWrapped.LastIndexOf('\n'));
							columnText = longWordWrapped.SubstringSafe(longWordWrapped.LastIndexOf('\n')) + " ";
							heightCount += longWordWrapped.Occurrences("\n");
						}
						else
						{
							heightCount++;
							columnText = currentWord + " ";
						}
					}
					else
					{
						columnText += currentWord;
						if (columnText.Length < columnWidth)
						{
							columnText += " ";
						}
					}
				}
				else
				{
					if (!columnText.IsEmpty)
					{
						result += columnText;
					}

					columnText = "";
					result += word + " ";
				}
			}

			if (!columnText.IsEmpty)
			{
				result += columnText;
			}

			return result;
		}

		ZString WrapLongWord(ZString word, ZInt columnWidth)
		{
			var result = ZString.Empty;

			if (columnWidth > 0)
			{
				var wrappedWord = ZString.Empty;
				var noOfLoops = (word.Length / columnWidth) + 1;

				for (int i = 0; i < noOfLoops; i++)
				{
					wrappedWord += word.SubstringSafe(columnWidth * i, columnWidth);
					wrappedWord += "\n";
				}

				result = wrappedWord.TrimEnd('\n');
			}

			return result;
		}

		#endregion

		#region MacroReplacer

		MacroStringReplacer MacroReplacer
		{
			get { return macroReplacer ?? (macroReplacer = new MacroStringReplacer(new DataProviderList(DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, ShipmentWrapper.CommonShipment)))); }
		}
		MacroStringReplacer macroReplacer;

		#endregion
	}
}
