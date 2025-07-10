using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Business;

public class ManifestMapper
{
	public RootLine Map(ForwardingConsol consol)
	{
		this.consol = consol;
		root = new RootLine();
		errors = new ZStringBuilder();

		root.Children.Add(MapVOY(consol));
		root.Children.Add(MapEND(consol));

		return root;
	}

	#region Mapping

	VOYLine MapVOY(ForwardingConsol consol)
	{
		VOYLine voy = new VOYLine();
		voy.LineCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		voy.VoyageAgentCode = Env.Registry.AECustoms.CourierID;
		if (consol.Transports.MostInterestingTransport != null)
		{
			voy.VesselName = consol.Transports.MostInterestingTransport.JW_Vessel;
			voy.AgentVoyageNumber = consol.Transports.MostInterestingTransport.JW_VoyageFlight;
			voy.ExpectedToArriveDate = consol.Transports.MostInterestingTransport.JW_ETA;
		}

		voy.PortCodeOfDischarge = consol.JK_RL_NKDischargePort;
		voy.RotationNumber = (consol.Numbers.GetFirstReferenceNumberByTypeAndCountry(UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.RotationNumber, Core.Constants.CountryCodes.UnitedArabEmirates))?.CE_EntryNum ?? ZString.Empty;
		voy.MessageType = ManifestConstants.MessageType;

		var instalment = consol.Numbers.Find(num => num.CE_EntryType == UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber).FirstOrDefault();
		voy.NoOfInstalment = (instalment != null) ? (int)(ZInt.ParseSafe(instalment.CE_EntryNum, 1)) : 1;
		ZInt sequenceNumber;
		if (!ZInt.TryParse(consol.JK_UniqueConsignRef.Right(5).ToString(), out sequenceNumber))
		{
			Errors.Append("Can't allocate the Agent's Reference Number from " + consol.JK_UniqueConsignRef.Right(5));
		}
		voy.AgentsManifestSequenceNumber = sequenceNumber;

		foreach (ForwardingShipment shipment in consol.Shipments)
		{
			voy.Children.Add(MapBOL(shipment));
		}
		return voy;
	}

	BOLLine MapBOL(ForwardingShipment shipment)
	{
		BOLLine bol = new BOLLine();
		bol.BillOfLadingNo = shipment.JS_HouseBill;
		bol.PartneringLineCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		bol.PartneringAgentCode = Env.Registry.AECustoms.CourierID;
		bol.PortCodeOfOrigin = shipment.JS_RL_NKOrigin;
		bol.PortCodeOfLoading = consol.JK_RL_NKLoadPort;
		bol.PortCodeOfDischarge = consol.JK_RL_NKDischargePort;
		bol.PortCodeOfDestination = shipment.JS_RL_NKDestination;
		bol.DateOfLoading = shipment.JS_ShippedOnBoardDate;
		bol.ManifestRegistrationNumber = shipment.JS_UniqueConsignRef.Right(8);
		bol.TradeCode = IsTransShipment(shipment) ? "T" : "I";
		if (!consol.IsSea)
		{
			Errors.Append("Manifest can be Declared only for SEA freight");
		}
		bol.TransShipmentMode = bol.TradeCode == "I" ? "" : consol.IsSea ? "S" : "";
		bol.CargoCode = ManifestConstants.CargoCode.GetManifestCode(shipment.JS_PackingMode);

		bol.ConsolidatedCargoIndicator = HasGroupageContainers(shipment);

		switch (shipment.JS_PackingMode)
		{
			case Core.Constants.ContainerModes.FCL:
				bol.ContainerServiceType = "FCL/FCL";
				break;
			case Core.Constants.ContainerModes.LCL:
				bol.ContainerServiceType = "LCL/LCL";
				break;
			case Core.Constants.ContainerModes.Groupage:
				bol.ContainerServiceType = "FCL/LCL";
				break;
			case Core.Constants.ContainerModes.BuyersConsol:
				bol.ContainerServiceType = "LCL/FCL";
				break;
			default:
				bol.ContainerServiceType = "FCL/FCL";
				break;
		}

		bol.ContainerServiceType = shipment.JS_PackingMode + "/" + shipment.JS_PackingMode;
		RefUNLOCO origin = shipment.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, shipment.JS_RL_NKOrigin);
		if (origin == null)
		{
			Errors.Append("Can't find the origin UNLOCO with code " + shipment.JS_RL_NKOrigin);
		}
		bol.CountryOfOrigin = origin != null ? origin.RL_RN_NKCountryCode : ZString.Empty;

		if (shipment.Consignor != null)
		{
			bol.ShipperName = shipment.Consignor.OH_FullNameTruncated;
			bol.ShipperAddress = shipment.Consignor.MainAddress.AddressAsASingleLineWithoutCompanyName;
			bol.ShipperCountryCode = shipment.Consignor.Country != null ? shipment.Consignor.Country.Code : ZString.Empty;
		}
		else if (shipment.ConsignorDocumentaryAddress != null)
		{
			bol.ShipperName = shipment.ConsignorDocumentaryAddress.E2_CompanyNameTruncated;
			bol.ShipperAddress = shipment.ConsignorDocumentaryAddress.AddressAsASingleLineWithoutCompanyName;
			bol.ShipperCountryCode = shipment.ConsignorDocumentaryAddress.Country != null ? shipment.ConsignorDocumentaryAddress.Country.Code : ZString.Empty;
		}

		if (shipment.Consignee != null)
		{
			bol.ConsigneeCode = shipment.Consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			bol.ConsigneeName = shipment.Consignee.OH_FullNameTruncated;
			bol.ConsigneeAddress = shipment.Consignee.MainAddress.AddressAsASingleLineWithoutCompanyName;
		}
		else if (shipment.ConsigneeDocumentaryAddress != null)
		{
			bol.ConsigneeName = shipment.ConsigneeDocumentaryAddress.E2_CompanyNameTruncated;
			bol.ConsigneeAddress = shipment.ConsigneeDocumentaryAddress.AddressAsASingleLineWithoutCompanyName;
		}

		if (shipment.NotifyParty != null)
		{
			bol.NotifyCode1 = shipment.NotifyParty.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			bol.NotifyName1 = shipment.NotifyParty.OH_FullNameTruncated;
			bol.NotifyAddress1 = shipment.NotifyParty.MainAddress.AddressAsASingleLineWithoutCompanyName;
		}
		else if (shipment.NotifyPartyDocumentaryAddress != null)
		{
			bol.NotifyName1 = shipment.NotifyPartyDocumentaryAddress.E2_CompanyNameTruncated;
			bol.NotifyAddress1 = shipment.NotifyPartyDocumentaryAddress.AddressAsASingleLineWithoutCompanyName;
		}

		bol.MarksAndNumbers = shipment.JS_MarksAndNumbers;
		if (shipment.OuterPackLines.Count == 0)
		{
			Errors.Append("Can't set the Commodity Code because there are no Pack Lines on the Shipment");
		}
		bol.CommodityCode = shipment.OuterPackLines.Count > 0 ? shipment.OuterPackLines[0].JL_HarmonisedCode : ZString.Empty;
		bol.CommodityDescription = shipment.OuterPackLines.Count > 0 ? shipment.OuterPackLines[0].JL_Description : ZString.Empty;
		bol.Packages = shipment.OuterPackLines.Totals.TotalPackages;

		bol.PackagesTypeCode = shipment.OuterPackLines.Totals.TotalPackagesUnit;
		RefPackTypeCollection packsList = new RefPackTypeCollection(shipment.Factory);
		bol.PackagesType = !bol.PackagesTypeCode.IsEmpty && (packsList.ContainsCode(bol.PackagesTypeCode)) ? new ZString(packsList.GetDescriptionFromCode(bol.PackagesTypeCode)) : ZString.Empty;

		ZString containerNumber = GetLCLContainerNumber(shipment);
		bol.ContainerNumber = containerNumber;
		if (containerNumber.Length == 11)
		{
			bol.CheckDigit = containerNumber.Right(1);
		}

		bol.NoOfContainers = shipment.Containers.Count();
		bol.NoOfTeus = CalculateTotalTeus(shipment);
		bol.TotalTareWeightInMT = GetShipmentTareWeight(shipment, Core.Constants.Weight.Tonnes);
		bol.CargoWeightInKG = GetShipmentWeight(shipment, Core.Constants.Weight.Kilograms);
		bol.GrossWeightInKG = shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL ? GetShipemntGrossWeight(shipment, Core.Constants.Weight.Kilograms) : new ZDecimal(0);
		bol.CargoVolumeInM3 = GetShipmentVolume(shipment, Core.Constants.Volume.CubicMetres);
		bol.TotalQuantity = shipment.JS_OuterPacks;
		bol.FreightTonne = shipment.JS_ActualChargeable;
		bol.NoOfPallets = GetPacks(shipment, Core.Constants.PkgUnit.Pallet);
		bol.SlacIndicator = SlacIndicator(shipment);

		ZString contractConditions = "";

		switch (shipment.JS_INCO)
		{
			case Constants.IncoTerms.CostAndFreight:
				contractConditions = "C&F";
				break;
			case Constants.IncoTerms.FreeOnBoard:
			case Constants.IncoTerms.CostInsuranceAndFreight:
				contractConditions = shipment.JS_INCO;
				break;
			case "":
				break;
			default:
				contractConditions = "XXX";
				break;
		}

		bol.ContractCarriageCondition = contractConditions;

		foreach (CommonContainer container in shipment.Containers)
		{
			bol.Children.Add(MapCTR(container, shipment));
		}
		return bol;
	}

	CTRLine MapCTR(CommonContainer container, ForwardingShipment shipment)
	{
		CTRLine ctr = new CTRLine();
		ctr.ContainerNumber = container.JC_ContainerNum;
		if (container.JC_ContainerNum.Length == 11)
		{
			ctr.CheckDigit = container.JC_ContainerNum.Right(1);
		}
		ctr.TareWeightInMT = GetContainerTareWeight(container, Core.Constants.Weight.Tonnes);
		ctr.SealNumber = container.JC_SealNum;
		ctr.Children.Add(MapCON(container, shipment));
		return ctr;
	}

	ManifestLineCollection MapCON(CommonContainer container, ForwardingShipment shipment)
	{
		ManifestLineCollection result = new ManifestLineCollection();
		ZInt counter = 0;
		BusinessObject[] packLines = Array.FindAll(shipment.OuterPackLines.ToArray(), p => ((PackLine)p).JL_JC == container.PK);
		foreach (PackLine line in packLines)
		{
			CONLine con = new CONLine();
			con.SerialNumber = (++counter).ToString();
			con.MarksAndNumbers = line.JL_MarksAndNumbers;
			con.CargoDescription = line.JL_Description;
			con.CommodityCode = line.JL_HarmonisedCode;
			con.ConsignmentPackages = line.JL_PackageCount;
			con.PackageTypeCode = line.JL_F3_NKPackType;
			RefPackTypeCollection packsList = new RefPackTypeCollection(new BusinessObjectFactory());
			con.PackageType = !con.PackageTypeCode.IsEmpty && packsList.ContainsCode(con.PackageTypeCode) ? new ZString(packsList.GetDescriptionFromCode(con.PackageTypeCode)) : ZString.Empty;
			con.NoOfPallets = line.JL_F3_NKPackType == Core.Constants.PkgUnit.Pallet ? line.JL_PackageCount : ZInt.Zero;
			con.ConsignmentWeightInKG = GetConsignmentWeight(line, Core.Constants.Weight.Kilograms);
			con.ConsignmentVolumeInM3 = GetConsignmentVolume(line, Core.Constants.Volume.CubicMetres);

			UNDGDataItem dgItem = line.UNDGs.Count > 0 && line.UNDGs[0].Substance != null ? line.UNDGs[0] : null;
			con.DangerousGoodsIndicator = dgItem != null ? "Y" : "N";
			con.IMOClassNumber = dgItem != null && dgItem.Substance != null ? dgItem.Substance.DG_Class : ZString.Empty;
			con.UnNumberOfDangerousGoods = dgItem != null && dgItem.Substance != null ? dgItem.Substance.DG_Code : ZString.Empty;
			ZString flashPoint = dgItem != null ? dgItem.DI_DGFlashPoint.Round(1).ToString() : "0";
			con.FlashPoint = dgItem != null && dgItem.DI_DGFlashPoint > 0 ? new ZString("+" + flashPoint) : flashPoint;

			con.UnitOfTemperature1 = container.JC_SetPointTempUnit;
			con.StorageRequestedForDangerousGoods = dgItem != null ? "" : "D";
			con.RefrigerationRequired = container.JC_SetPointTemp != 0 ? "Y" : "N";
			ZString temperature = container.JC_SetPointTemp.Round(1).ToString();
			con.MinimumTemperatureOfRefregeration = container.JC_SetPointTemp > 0 ? new ZString("+" + temperature) : temperature;
			con.MaximumTemperatureOfRefregeration = container.JC_SetPointTemp > 0 ? new ZString("+" + temperature) : temperature;
			con.UnitOfTemperature2 = container.JC_SetPointTempUnit;
			result.Add(con);
		}
		return result;
	}

	ENDLine MapEND(ForwardingConsol consol)
	{
		ENDLine end = new ENDLine();
		end.NoOfContainerRelatedBOL = consol.Shipments.Count;
		return end;
	}

	#region Helpers

	internal ZString SlacIndicator(ForwardingShipment shipment)
	{
		ZString result = "N";
		foreach (FilterField field in DocumentNote.LoadNote(shipment).UserDefinedFieldList)
		{
			if (field.DisplayName == "Shipper Load And Count")
			{
				string shipperLoadAndCount = field.ValueAsObject as String;
				result = (shipperLoadAndCount != null && !string.IsNullOrEmpty(shipperLoadAndCount)) ? "Y" : "N";
				break;
			}
		}
		return result;
	}

	ZInt CalculateTotalTeus(ForwardingShipment shipment)
	{
		ZDecimal result = 0;
		foreach (CommonContainer container in shipment.Containers)
		{
			result += container.RefContainer != null ? container.RefContainer.RC_TEU : 0;
		}
		return result.Round(0).ToZInt();
	}

	ZString HasGroupageContainers(ForwardingShipment shipment)
	{
		ZString result = "N";
		foreach (CommonContainer container in shipment.Containers)
		{
			if (container.JC_ContainerMode == Core.Constants.ContainerModes.Groupage)
			{
				result = "Y";
				break;
			}
		}
		return result;
	}

	bool IsTransShipment(ForwardingShipment shipment)
	{
		bool result = false;

		if (!shipment.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedArabEmirates))
		{
			if (shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedArabEmirates))
			{
				var transport = shipment.TransportsIncludingRelated.Cast<Transport>().FirstOrDefault(
					x =>
						x.JW_TransportMode == Core.Constants.TransportModes.Sea
						&& x.JW_RL_NKLoadPort == consol.JK_RL_NKDischargePort
						&& x.JW_RL_NKDiscPort == shipment.JS_RL_NKDestination);

				if (transport != null)
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
		}

		return result;
	}

	ZDecimal GetConsignmentVolume(PackLine line, string targetUnitCode)
	{
		ZDecimal result = Core.Constants.Volume.Convert(line.JL_ActualVolume, line.JL_ActualVolumeUQ, targetUnitCode);
		return ZArchitecture.Core.Utilities.Round(result, JobPackLinesSchema.JL_ActualWeight.Scale);
	}

	ZDecimal GetConsignmentWeight(PackLine line, string targetUnitCode)
	{
		ZDecimal result = Core.Constants.Weight.Convert(line.JL_ActualWeight, line.JL_ActualWeightUQ, targetUnitCode);
		return ZArchitecture.Core.Utilities.Round(result, JobPackLinesSchema.JL_ActualWeight.Scale);
	}

	ZDecimal GetShipmentVolume(ForwardingShipment shipment, ZString targetUnitCode)
	{
		ZDecimal result = 0;
		foreach (PackLine paclLine in shipment.OuterPackLines)
		{
			result += Core.Constants.Volume.Convert(paclLine.JL_ActualVolume, paclLine.JL_ActualVolumeUQ, targetUnitCode);
		}
		return ZArchitecture.Core.Utilities.Round(result, JobPackLinesSchema.JL_ActualVolume.Scale);
	}

	ZString GetLCLContainerNumber(ForwardingShipment shipment)
	{
		return (shipment.PackingMode == Core.Constants.ContainerModes.LCL && shipment.Containers.Any()) ? shipment.Containers.First().JC_ContainerNum : ZString.Empty;
	}

	ZInt GetPacks(ForwardingShipment shipment, ZString packType)
	{
		ZInt result = 0;
		foreach (ForwardingPackLine pack in shipment.OuterPackLines)
		{
			result += pack.JL_F3_NKPackType == packType ? pack.JL_PackageCount : ZInt.Zero;
		}
		return result;
	}

	ZDecimal GetContainerTareWeight(CommonContainer container, ZString targetUnitCode)
	{
		ZDecimal result = Core.Constants.Weight.Convert(container.JC_TareWeight, container.JC_GrossWeightUQ, targetUnitCode);
		return ZArchitecture.Core.Utilities.Round(result, JobContainerSchema.JC_TareWeight.Scale);
	}

	ZDecimal GetShipmentTareWeight(ForwardingShipment shipment, ZString targetUnitCode)
	{
		ZDecimal result = 0;
		foreach (CommonContainer con in shipment.Containers)
		{
			result += Core.Constants.Weight.Convert(con.JC_TareWeight, con.JC_GrossWeightUQ, targetUnitCode);
		}
		return ZArchitecture.Core.Utilities.Round(result, JobContainerSchema.JC_TareWeight.Scale);
	}

	ZDecimal GetShipemntGrossWeight(ForwardingShipment shipment, ZString targetUnitCode)
	{
		ZDecimal result = 0;
		foreach (CommonContainer con in shipment.Containers)
		{
			result += Core.Constants.Weight.Convert(con.JC_GrossWeight, con.JC_GrossWeightUQ, targetUnitCode);
		}
		return ZArchitecture.Core.Utilities.Round(result, JobContainerSchema.JC_GrossWeight.Scale);
	}

	ZDecimal GetShipmentWeight(ForwardingShipment shipment, ZString targetUnitCode)
	{
		ZDecimal result = 0;
		foreach (PackLine packLine in shipment.OuterPackLines)
		{
			result += Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.PackLineWeightUnit, targetUnitCode);
		}
		return ZArchitecture.Core.Utilities.Round(result, JobPackLinesSchema.JL_ActualWeight.Scale);
	}

	#endregion

	#endregion

	#region Implementation

	public RootLine Root
	{
		get { return root; }
	}
	RootLine root;

	ZStringBuilder errors;
	public ZStringBuilder Errors
	{
		get { return errors ?? (errors = new ZStringBuilder()); }
	}

	ForwardingConsol consol;

	#endregion
}
