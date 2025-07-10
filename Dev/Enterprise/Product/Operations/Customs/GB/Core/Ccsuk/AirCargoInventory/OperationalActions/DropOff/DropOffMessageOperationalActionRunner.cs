using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class DropOffMessageOperationalActionRunner
	{
		public DropOffMessageOperationalActionRunner(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			this.log = log;
			this.targets = targets;
		}

		public void PerformMakeDropOffMessages(string vehicle, ZDateTime etd, ZDateTime eta)
		{
			var ccsukExportAirDecs = (from JobDeclaration d in targets where d.IsExport && d.IsAir && d.ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW select (JobDeclaration)d);
			var numberOfDifferentAirports = ccsukExportAirDecs.GroupBy(d => d.JE_LocationOfGoods).Count();
			var numberOfDifferentAgents = ccsukExportAirDecs.GroupBy(d => d.JE_CustomsProfile).Count();
			if (numberOfDifferentAirports > 1 || numberOfDifferentAgents > 1)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, "Cannot send in bulk for jobs at multiple airports or nominated to different agents. Select fewer declarations spanning only one airport and badge.");
				return;
			}

			if (!ccsukExportAirDecs.Any())
			{
				log.Notify(OperationalActionLogErrorLevel.Error, "No relevant declarations were selected. Only CCSUK air export jobs may be dropped off.");
				return;
			}
			var message = new DropOffWrapper();
			var decForMessagingParent = ccsukExportAirDecs.First();
			message.Header.Vehicle = vehicle;
			message.Header.Agent = decForMessagingParent.JE_CustomsProfile;
			message.Header.Airport = decForMessagingParent.JE_LocationOfGoods.Right(3);
			message.Header.ETA = eta;
			message.Header.ETD = etd;
			foreach (var decsAtOneShed in ccsukExportAirDecs.GroupBy(d => d.SubLocation))
			{
				var shedGroup = new DropOffShedGroup();
				shedGroup.Shed = decsAtOneShed.Key;
				message.ShedGroups.Add(shedGroup);

				var looseAwbs = (from JobDeclaration d in decsAtOneShed where d.JE_ContainerMode != "ULD" select d);
				if (looseAwbs.Any())
				{
					var looseLine = new DropOffUldGroup();
					shedGroup.UldLines.Add(looseLine);
					foreach (var dec in looseAwbs)
					{
						var awbLine = new DropOffAwb();
						PopulateDropOffAwb(dec, awbLine);
						looseLine.AwbLines.Add(awbLine);
					}
				}
				var unitisedDecs = (from JobDeclaration d in decsAtOneShed where d.JE_ContainerMode == "ULD" select d);

				if (unitisedDecs.Any())
				{
					var allUldsAtThisShed = new List<ZString>();
					foreach (var dec in unitisedDecs)
					{
						foreach (BaseCusContainer c in dec.CusContainers)
						{
							if (!allUldsAtThisShed.Contains(c.CO_ContainerNumber))
							{
								allUldsAtThisShed.Add(c.CO_ContainerNumber);
							}
						}
					}
					foreach (var uldNumber in allUldsAtThisShed)
					{
						var uldGroup = new DropOffUldGroup();
						uldGroup.UldNumber = uldNumber;
						shedGroup.UldLines.Add(uldGroup);
						foreach (var dec in unitisedDecs)
						{
							foreach (var cusCont in (from BaseCusContainer c in dec.CusContainers where c.CO_ContainerNumber == uldNumber select c))
							{
								var awbline = new DropOffAwb();
								PopulateDropOffAwb(dec, awbline);
								awbline.Weight = new ZWeight(cusCont.CO_Weight, cusCont.CO_WeightUQ);
								awbline.Volume = ProRateVolumeOfCOntainerFromWeight(awbline.Weight, dec.JE_TotalWeight, dec.JE_TotalWeightUnit, dec.Volume);
								SetCurrentPackagesAndVolumeFromShipment(cusCont, dec, awbline);
								uldGroup.AwbLines.Add(awbline);
							}
						}
					}
				}
			}

			MakeEdiMessageFromDropOff(message, decForMessagingParent);
		}

		void SetCurrentPackagesAndVolumeFromShipment(BaseCusContainer cusCont, JobDeclaration dec, DropOffAwb awbline)
		{
			var shipment = dec.Shipment;
			if (shipment != null)
			{
				foreach (PackLine packLine in shipment.OuterPackLines)
				{
					var jobContainer = packLine.GetContainer(dec.RelevantConsol) as ForwardingContainer;
					var containerNum = jobContainer != null ? jobContainer.JC_ContainerNum : ZString.Empty;
					if (containerNum == cusCont.CO_ContainerNumber)
					{
						awbline.CurrentPackages = packLine.JL_PackageCount;
						awbline.Volume = new ZVolume(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ);
						break;
					}
				}
			}
		}

		ZVolume ProRateVolumeOfCOntainerFromWeight(ZWeight lineWeight, ZDecimal totalWeight, ZString totalWeightUnit, ZVolume totalVolume)
		{
			var w = new ZWeight(totalWeight, totalWeightUnit);
			var v = ZVolume.Empty;
			if (w.InKilogramsSafe > 0)
			{
				var fraction = lineWeight.InKilogramsSafe / w.InKilogramsSafe;
				v = totalVolume * fraction;
			}
			return v;
		}

		void MakeEdiMessageFromDropOff(DropOffWrapper message, JobDeclaration decForMessagingParent)
		{
			var drpCreator = new DrpCreator(message, decForMessagingParent);
			var edifact = drpCreator.MakeMessageText();
			var persistentMessage = decForMessagingParent.Messages.AddNew();
			persistentMessage.EM_MessageOwner = drpCreator.SenderPima;
			persistentMessage.EM_Status = EDIMessage.Status.Queued;
			persistentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			persistentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(decForMessagingParent.Factory, ApplicationCodeList.Codes.GbCcsuk);
			persistentMessage.EM_MessageText = edifact;
			persistentMessage.EM_ApplicationReference = drpCreator.RecipientPima;
			persistentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			persistentMessage.EM_MessageType = drpCreator.MessageFunction.MessageType;
			persistentMessage.EM_MessageSubType = drpCreator.MessageFunction.MessageSubType;
			persistentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			persistentMessage.Factory.Save();
			log.Notify(OperationalActionLogErrorLevel.Informational, string.Format("DRP message #{0} created on declaration {1}", persistentMessage.EM_MessageNum, decForMessagingParent.JE_DeclarationReference));
		}

		static void PopulateDropOffAwb(JobDeclaration dec, DropOffAwb awbLine)
		{
			awbLine.AwbNumber = dec.JE_MasterBill;
			awbLine.Description = dec.JE_GoodsDescription;
			awbLine.Weight = new ZWeight(dec.JE_TotalWeight, dec.JE_TotalWeightUnit);
			awbLine.TotalPackages = dec.JE_TotalNoOfPacks;
			awbLine.UnlocoDestination = dec.PortOfArrival;
			awbLine.UnlocoOrigin = dec.PortOfLoading;
			awbLine.Volume = dec.Volume;
		}

		readonly IOperationalActionSectionLog log;
		readonly BusinessObject[] targets;
	}
}
