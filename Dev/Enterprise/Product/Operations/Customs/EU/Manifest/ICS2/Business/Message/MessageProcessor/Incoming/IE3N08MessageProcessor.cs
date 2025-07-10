using System;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N08;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N08MessageProcessor : MessageProcessorWithEmailNotification<Ie3N08Type>
	{
		public IE3N08MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("7DD45A42-E066-4115-827B-CB5E01377F29", "Control Notification");

		protected override Func<Ie3N08Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N08Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("7DD45A42-E066-4115-827B-CB5E01377F29", "Control Notification"));
			htmlBuilder.AppendLine();

			var basicInfo = new HtmlTableCreator(
				new string[]
				{
					Res.GetString("057BAD13-A313-4A25-B571-5C4DED1C1CD0", "MRN"),
					Res.GetString("34701995-7E75-4553-91EF-C918108EF584", "Notification Date"),
					Res.GetString("4BA95300-10F3-4662-8292-80782654185C", "Scheduled Date"),
					Res.GetString("130566A9-CED2-4B36-818C-3D1B385BF8FA", "Customs Office"),
					Res.GetString("9F220338-3C96-44E2-AFFE-F5D36C31F4D4", "Representative"),
				});

			basicInfo.WriteRow(
				messageObject.Mrn,
				GetISO8601DateTimeWithSecondsPrecision(messageObject.NotificationDate.DateTime),
				GetISO8601DateTimeWithSecondsPrecision(messageObject.ScheduledControlDate?.DateTime),
				messageObject.CustomsOfficeOfControl.ReferenceNumber,
				messageObject.Representative?.IdentificationNumber);
			htmlBuilder.Append(basicInfo.ToHtml());

			foreach (var control in messageObject.Control)
			{
				var controlInfo = new HtmlTableCreator();
				controlInfo.WriteRow(Res.GetString("877ED17C-CBE9-4C84-8F1C-BAEA46FCFAA4", "Master"), messageObject.TransportDocument?.Type, messageObject.TransportDocument?.DocumentNumber);
				controlInfo.WriteRow(Res.GetString("771C5855-29F7-4FC3-863D-302C19A8BDD3", "Exam Place"), control.ExaminationPlace.PlaceOfExamination,
					Res.GetString("3EC59E2B-0DF8-4C6A-95E1-EAB7E7B4D0B9", "Reference Number"), control.ExaminationPlace.ReferenceNumber);
				controlInfo.WriteRow(Res.GetString("6BE2C42D-22AD-40E7-9D2C-213588C43AEE", "Declarant"), messageObject.Declarant?.IdentificationNumber,
					Res.GetString("60EBA4C0-7584-4EE8-9C75-4DDF31EFED7A", "Person Notifying the arrival"), messageObject.PersonNotifyingTheArrival?.IdentificationNumber,
					Res.GetString("D00D28B2-5CA8-48C2-9613-EA35B7E95D12", "notify Party"), messageObject.NotifyParty?.IdentificationNumber);

				controlInfo.WriteRow(Res.GetString("73B33571-F74D-4A98-9B7F-361D051E874D", "Receptacle"));
				foreach (var receptacle in control.ControlSubject.ConsignmentMasterLevel.Receptacle?.Select(x => x.ReceptacleIdentificationNumber)?.Chunk(4))
				{
					controlInfo.WriteRow(receptacle.ToArray());
				}

				controlInfo.WriteRow(Res.GetString("FA4A9767-8660-4867-9AB8-7BBFC6021BE0", "Shipping Marks"), Res.GetString("5630681F-40D2-460F-8CDB-D23956707C7C", "Type Packs"));
				foreach (var consignmentItemPackaging in control.ControlSubject.ConsignmentMasterLevel.Packaging)
				{
					controlInfo.WriteRow(consignmentItemPackaging.ShippingMarks, consignmentItemPackaging.TypeOfPackages);
				}

				controlInfo.WriteRow(Res.GetString("54029EA0-2455-4E79-9858-87CFF936F24F", "Containers"));
				foreach (var container in control.ControlSubject.ConsignmentMasterLevel.TransportEquipment?.Select(x => x.ContainerIdentificationNumber)?.Chunk(5))
				{
					controlInfo.WriteRow(container.ToArray());
				}

				controlInfo.WriteRow(Res.GetString("E00F54E6-E000-44CD-BC3C-C17B628C790A", "Goods Items"),
					Res.GetString("500AD80D-E161-4729-9E75-5BE1B4BFDC79", "Item Number"),
					Res.GetString("192DCC86-8F2B-4A39-AA21-0AA05019777C", "Shipping Marks"),
					Res.GetString("0798B2C8-1FFB-4B1D-9BA6-68122097BF3F", "Type Packs"));
				foreach (var goodsItem in control.ControlSubject.ConsignmentMasterLevel.GoodsItem)
				{
					foreach (var packing in goodsItem?.Packaging)
					{
						controlInfo.WriteRow("", goodsItem.GoodsItemNumber, packing.ShippingMarks, packing.TypeOfPackages);
					}
				}

				foreach (var houseBill in control.ControlSubject.ConsignmentMasterLevel.ConsignmentHouseLevel)
				{
					controlInfo.WriteRow(Res.GetString("A9604201-F25C-4F89-BFE5-59C012CB9FC8", "House Bill"), houseBill.TransportDocumentHouseLevel?.DocumentNumber, houseBill.TransportDocumentHouseLevel?.Type);
					controlInfo.WriteRow(Res.GetString("E00F54E6-E000-44CD-BC3C-C17B628C790A", "Goods Items"),
						Res.GetString("500AD80D-E161-4729-9E75-5BE1B4BFDC79", "Item Number"),
						Res.GetString("192DCC86-8F2B-4A39-AA21-0AA05019777C", "Shipping Marks"),
						Res.GetString("0798B2C8-1FFB-4B1D-9BA6-68122097BF3F", "Type Packs"));
					foreach (var goodsItem in houseBill.GoodsItem)
					{
						foreach (var packing in goodsItem?.Packaging)
						{
							controlInfo.WriteRow("", goodsItem.GoodsItemNumber, packing?.ShippingMarks, packing?.TypeOfPackages);
						}
					}

					controlInfo.WriteRow(Res.GetString("798B7E90-5465-43A8-9DC7-232A7B309AC6", "Transport Means"));
					foreach (var transportMean in houseBill.PassiveBorderTransportMeans?.Select(x => (x.IdentificationNumber, x.TypeOfIdentification, x.Nationality)))
					{
						controlInfo.WriteRow(transportMean.IdentificationNumber, transportMean.TypeOfIdentification, transportMean.Nationality);
					}

					controlInfo.WriteRow(Res.GetString("54029EA0-2455-4E79-9858-87CFF936F24F", "Containers"));
					foreach (var container in houseBill.TransportEquipment?.Select(x => x.ContainerIdentificationNumber).Chunk(5))
					{
						controlInfo.WriteRow(container.ToArray());
					}
				}

				htmlBuilder.Append(controlInfo.ToHtml());
			}
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N08Type messageObject)
		{
			return Res.GetString("09EC8683-EC3D-44F2-9D2D-FCE8B4310743", "ICS2 - Control Notification for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N08Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.CNR;
			((IStatusSupporter)manifestHeader).LogEventsOnParent(AutoEvents.StatusChange, $"REG to CNR - ENS Control Notification Received");
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}
