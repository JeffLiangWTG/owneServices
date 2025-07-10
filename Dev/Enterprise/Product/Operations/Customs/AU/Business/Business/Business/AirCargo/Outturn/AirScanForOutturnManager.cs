using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirScanForOutturnManager : ScanForOutturnManager, IScanForOutturnManager
	{
		public AirScanForOutturnManager(ScanCusMAWB scanCusMAWB)
			: base(scanCusMAWB)
		{
			hostBO = Argument.NotNull(scanCusMAWB, "ScanCusMAWB");
			hostBO.hostedBO.ResetUnderbondsForDCL();
		}

		AirUnderbondSelectorLine AirSelectedUnderbond
		{
			get { return (AirUnderbondSelectorLine)base.SelectedUnderbond; }
		}

		public override OutturnLineCollection GetNewOutturnLineCollection()
		{
			return new AirOutturnLineCollection(Factory);
		}

		public override string ValidateStandAloneUnderbond()
		{
			return hostBO.ValidateStandAloneUnderbonds();
		}

		public override string ValidateSelectedUnderbond()
		{
			var error = base.ValidateSelectedUnderbond();

			if (string.IsNullOrEmpty(error))
			{
				foreach (AirUnderbondSelectorLine underbonLine in ScanWizardDataSource.UnderbondSelectorLineCollection)
				{
					if (underbonLine != SelectedUnderbond && (underbonLine.UnderbondStatus == UnderbondStatus.PartiallySent || underbonLine.UnderbondStatus == UnderbondStatus.WaitingCustomsResponse))
					{
						error = Res.GetString("22ec7290-87bd-38c9-89fe-ee7d3c81ee45", "You have already started processing the underbond for {1}/{2} ({0}). Please send it in full, or zero land remaining details, before processing the next arrival.", underbonLine.Reference, underbonLine.FlightNumber, underbonLine.ArivalDate);
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(error) && AirSelectedUnderbond.UnderbondStatus == UnderbondStatus.FullySent)
			{
				error = Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c93ee57", "This underbond has already been sent. Please use another one.");
			}

			if (string.IsNullOrEmpty(error) && AirSelectedUnderbond.UnderbondStatus == UnderbondStatus.WaitingCustomsResponse)
			{
				error = Res.GetString("22ec7290-87bd-48c8-89fe-ef7d3c93ee57", "This underbond in currently waiting for a Customs response. You cannot continue until a reply is received.");
			}

			return error;
		}

		public new AirScanWizardDataSource ScanWizardDataSource
		{
			get { return (AirScanWizardDataSource)base.ScanWizardDataSource; }
		}

		protected override ScanWizardDataSource GetScanWizardDataSource(ScanMasterBill scanObj)
		{
			return new AirScanWizardDataSource((ScanCusMAWB)scanObj);
		}

		public override string SaveOutturnResult(Customs.Business.ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem sender)
		{
			if (OutturnCollection == null)
			{
				return Res.GetString("87c055ac-0555-4cb9-8b09-dd303a6fa9cd", "Outturn collection is empty.");
			}

			var underbondToSend = new List<CusUnderbond>();
			hostBO.SelectedUnderbond.Outturns.RemoveAndDeleteAll();
			ScanCusMAWB singleSurplusAirCargo = null;

			if (hostBO.IsStandAlone)
			{
				singleSurplusAirCargo = hostBO;
			}
			else if (ScanWizardDataSource.ShipmentSelectorLineCollection != null)
			{
				var selectedShipments = ScanWizardDataSource.ShipmentSelectorLineCollection.GetHLSShipmentIncludeInScan();
				if (!selectedShipments.Any())
				{
					return Res.GetString("87c055ac-0555-4cb9-8b09-dd303a6fa9cf", "There is no HLS shipment or underbond exist for surplus consignments");
				}
				else if (selectedShipments.Count() == 1)
				{
					singleSurplusAirCargo = hostBO.GetStandAloneAirCargo(selectedShipments.ElementAt(0).Shipment);
				}
			}

			foreach (AirOutturnLine line in OutturnCollection)
			{
				if (line.HouseBill != null)
				{
					var error = AddOutturn(line, false, underbondToSend, null);
					if (!error.IsNullOrEmpty())
					{
						return error;
					}
				}
			}

			foreach (SurplusOutturnLine line in ScanWizardDataSource.SurplusOutturnCollection)
			{
				ScanCusMAWB surplusAirCargo;
				if (singleSurplusAirCargo != null)
				{
					surplusAirCargo = singleSurplusAirCargo;
				}
				else
				{
					surplusAirCargo = hostBO.GetStandAloneAirCargo(line.SelectedShipmentLine.Shipment);
				}
				var error = AddOutturn(line.OutturnLine, true, underbondToSend, surplusAirCargo);
				if (!error.IsNullOrEmpty())
				{
					return error;
				}
			}

			string errorMessage = SendCustomsMessage(underbondToSend, sender);

			try
			{
				Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}

			return errorMessage;
		}

		string AddOutturn(OutturnLine line, bool isSurplus, List<CusUnderbond> underbondToSend, ScanCusMAWB surplusAirCargo)
		{
			if (line.ManifestInfo != null && line.ManifestInfo.Quantity == 0 && line.Count == 0)
			{
				return string.Empty;
			}

			if (line.Underbond != null && !underbondToSend.Contains(line.Underbond))
			{
				underbondToSend.Add(line.Underbond);
				line.Underbond.Outturns.RemoveAndDeleteAll();
			}

			CusUnderbond surplusUnderbond = null;
			var outturn = Factory.New<CusOutturn>();

			if (!isSurplus)
			{
				outturn.C5_OutturnResultType = GetOutturnResultType(line.ManifestInfo.Quantity, line.Count);
			}
			else
			{
				if (surplusAirCargo != null)
				{
					surplusUnderbond = surplusAirCargo.GetUnderbond();
				}
				if (surplusAirCargo == null || surplusUnderbond == null)
				{
					return Res.GetString("87c055ac-0555-4cb9-8b09-dd303a6fa9cf", "There is no HLS shipment or underbond exist for surplus consignments");
				}
				surplusAirCargo.MasterBill.CreateSurplusConsignment(Factory, line);
				line.Underbond = surplusUnderbond;
				outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			}

			outturn.C5_C4_Underbond = line.Underbond.PK;
			outturn.C5_OuterPacks = line.ManifestInfo.Quantity;
			outturn.C5_PackagesOutturned = line.Count;
			outturn.C5_CustomsStatus = line.ManifestInfo.CustomsStatus.Left(CusOutturn.Schema.C5_CustomsStatusMaxLength);
			outturn.C5_GoodsDescription = line.ManifestInfo.GoodsDescription;
			outturn.C5_ParentTableCode = line.ManifestInfo.TablePrefix;
			outturn.C5_ParentID = line.ManifestInfo.PK;

			line.Underbond.Outturns.Add(outturn);
			return string.Empty;
		}

		public string CanSendZeroOutturns()
		{
			if (hostBO.SelectedUnderbond == null)
			{
				return Res.GetString("9486C894-D8FD-47D8-901E-EA16F37F6CD7", "Underbond not selected.");
			}

			if (hostBO.SelectedShipments == null || !hostBO.SelectedShipments.Any())
			{
				return Res.GetString("d8e738ae-364e-4d1a-8a4e-3de8c2982ac6", "Select shipments first.");
			}

			foreach (var houseBill in hostBO.SelectedShipments)
			{
				if (!houseBill.IsHVLVShipment())
				{
					if (hostBO.SelectedUnderbond.GetOutturnStatus() == OutturnStatus.Sent)
					{
						return Res.GetString("9AC74100-785F-403B-A5E0-0E31C69473DC", "Outturn for House Bill '{0}' was already sent.", houseBill.HouseBill);
					}
					if (hostBO.SelectedUnderbond.GetOutturnStatus() == OutturnStatus.AwaitingResponseFromCustoms)
					{
						return Res.GetString("{D61EC13-05C4-4736-9CEB-D18CAAB2E378", "Awaiting response from customs for for House Bill '{0}'.", houseBill.HouseBill);
					}
				}
				else
				{
					var standAloneCustMAWB = hostBO.GetStandAloneAirCargo(houseBill.HouseBill);
					if (standAloneCustMAWB == null)
					{
						return Res.GetString("3045ABC6-33D0-43F0-A407-AA42D517EC5C", "Create Stand Alone Air Cargo for House-Bill '{0}'.", houseBill.HouseBill);
					}

					var standAloneUnderbond = standAloneCustMAWB.GetUnderbond();
					if (standAloneUnderbond == null)
					{
						return Res.GetString("E641E145-0A1C-4ED9-836B-9DCE77D7637D", "Create Underbond for Stand Alone Air Cargo with House-Bill '{0}'.", houseBill.HouseBill);
					}

					if (standAloneUnderbond.GetOutturnStatus() == OutturnStatus.Sent)
					{
						return Res.GetString("56A61B25-BA81-441A-A274-1C4A344A7F75", "Outturn for House Bill '{0}' was already sent.", houseBill.HouseBill);
					}
					if (standAloneUnderbond.GetOutturnStatus() == OutturnStatus.AwaitingResponseFromCustoms)
					{
						return Res.GetString("EC419B9A-6CDF-4CBB-A5E2-49FD7D6DAC13", "Awaiting response from customs for for House Bill '{0}'.", houseBill.HouseBill);
					}
				}
			}

			return string.Empty;
		}

		public string SendZeroOutturnsForSelectedShipments(ISendsMessagesToCustoms sender)
		{
			if (hostBO.SelectedUnderbond == null)
			{
				return Res.GetString("E9882E34-29DD-416F-8EA5-DDF9473ED8B6", "Underbond not selected.");
			}

			if (hostBO.SelectedShipments == null && !hostBO.SelectedShipments.Any())
			{
				return Res.GetString("66866C7B-6A8E-4C66-8BA3-56591170C616", "Select shipment first.");
			}

			var underbondToSend = new List<CusUnderbond>();

			foreach (var houseBill in hostBO.SelectedShipments)
			{
				if (!houseBill.IsHVLVShipment())
				{
					if (hostBO.SelectedUnderbond.GetOutturnStatus() == OutturnStatus.ReadyForScanning)
					{
						if (!underbondToSend.Contains(SelectedUnderbond.Underbond))
						{
							CreateZeroOutturns(SelectedUnderbond.Underbond);
							if (SelectedUnderbond.Underbond.Outturns.Count > 0)
							{
								underbondToSend.Add(SelectedUnderbond.Underbond);
							}
						}
					}
				}
				else
				{
					var standAloneCustMAWB = hostBO.GetStandAloneAirCargo(houseBill.HouseBill);
					if (standAloneCustMAWB == null)
					{
						return Res.GetString("7DFE2446-40CA-4D4D-AAFE-5933B40623B1", "Create Stand Alone Air Cargo for House-Bill '{0}'.", houseBill.HouseBill);
					}

					var standAloneUnderbond = standAloneCustMAWB.GetUnderbond();
					if (standAloneUnderbond == null)
					{
						return Res.GetString("1E573DF5-93F3-4771-855F-823C1BBBD9AE", "Create Underbond for Stand Alone Air Cargo with House-Bill '{0}'.", houseBill.HouseBill);
					}

					if (standAloneUnderbond.GetOutturnStatus() == OutturnStatus.ReadyForScanning)
					{
						if (!underbondToSend.Contains(standAloneUnderbond))
						{
							CreateZeroOutturns(standAloneUnderbond);
							if (standAloneUnderbond.Outturns.Count > 0)
							{
								underbondToSend.Add(standAloneUnderbond);
							}
						}
					}
				}
			}

			if (underbondToSend.Count == 0)
			{
				return Res.GetString("BFCD149F-76F4-43A9-91B3-955D6C29E302", "No suitable shipments were found to zero land.");
			}

			string errorMessage = SendCustomsMessage(underbondToSend, sender);

			try
			{
				Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}

			return errorMessage;
		}

		protected virtual string SendCustomsMessage(List<CusUnderbond> underbonds, Customs.Business.ISendsMessagesToCustoms sender)
		{
			if (sender == null)
			{
				return Res.GetString("F78B0F1F-1D3A-44CA-99FF-D509147B683B", "Sender is empty");
			}

			var stringBuilder = new StringBuilder();
			var startSendOutturnTime = ZDateTime.Now;

			foreach (var underbond in underbonds)
			{
				underbond.C4_Outurned = startSendOutturnTime;
				var manager = new CusMAWBMessageManager(delegate
				{ return underbond.MAWB; });
				if (!manager.SendOutturnMessage(sender, underbond))
				{
					stringBuilder.AppendLine(Res.GetString("6A690FB3-49D9-4EF9-A78F-9D88C3B84570", "Outturn for underbond '{0}' was not sent. Details: {1}", underbond.C4_SendersMessageReference, manager.AfterSaveNotifications.ToString()));
				}
				else
				{
					stringBuilder.AppendLine(Res.GetString("D4357DC7-BD57-478C-B151-CE06DC703D25", "Outturn for underbond '{0}' was sent successfully. Details: {1}", underbond.C4_SendersMessageReference, manager.AfterSaveNotifications.ToString()));
				}

				startSendOutturnTime = startSendOutturnTime.AddMinutes(1);
			}

			return stringBuilder.ToString();
		}

		static void AddOutturnLine(CusUnderbond cusUnderbond, CusHAWB houseBill, int quantityShort)
		{
			var outturn = cusUnderbond.Factory.New<CusOutturn>();

			outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			outturn.C5_ParentID = houseBill.PK;
			outturn.C5_ParentTableCode = houseBill.TablePrefix;
			outturn.C5_C4_Underbond = cusUnderbond.PK;
			outturn.C5_OuterPacks = quantityShort;
			outturn.C5_PackagesOutturned = 0;
			outturn.C5_CustomsStatus = houseBill.CS_CustomsStatus.Left(CusOutturn.Schema.C5_CustomsStatusMaxLength);
			outturn.C5_GoodsDescription = houseBill.GoodsDescription;

			cusUnderbond.Outturns.Add(outturn);
		}

		protected override OutturnLine GetNewOutturnLine()
		{
			return new AirOutturnLine();
		}

		void CreateZeroOutturns(CusUnderbond cusUnderbond)
		{
			cusUnderbond.Outturns.RemoveAndDeleteAll();
			var cusMAWB = cusUnderbond.MAWB;
			foreach (CusHAWB houseBill in cusMAWB.ChildBills)
			{
				int quantityShort = houseBill.RemainingDCLOutturnQuantity;
				if (quantityShort > 0)
				{
					AddOutturnLine(cusUnderbond, houseBill, quantityShort);
				}
			}
		}

		protected override void AddManifestLinesForConsol(OutturnLineCollection collection, ShipmentSelectorLine shipmentSelectorLine)
		{
			if (shipmentSelectorLine.Type == Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy)
			{
				foreach (IScanHouseBillProvider houseBill in shipmentSelectorLine.HouseBills)
				{
					var scanCusMAWB = hostBO.GetStandAloneAirCargo(houseBill.HouseBill);
					if (scanCusMAWB != null)
					{
						var hlsUnderbond = scanCusMAWB.GetUnderbond();
						foreach (var child in scanCusMAWB.GetChildBills())
						{
							if (child.ShouldScan(hlsUnderbond))
							{
								collection.Add(NewLine(child, hlsUnderbond));
							}
						}
					}
				}
			}
			else
			{
				base.AddManifestLinesForConsol(collection, shipmentSelectorLine);
			}
		}

		readonly ScanCusMAWB hostBO;
	}
}
