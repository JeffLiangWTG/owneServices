using System;
using System.Collections.Specialized;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R04;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3R04MessageProcessor : MessageProcessorWithEmailNotification<Ie3R04Type>
	{
		public IE3R04MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0d57d4dc-5747-48d0-855c-1ddebf8c22b1", "Arrival Registration Response");

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3R04Type messageObject)
		{
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.ARV;
			manifestHeader.ArrivalReferenceNumber = messageObject.Mrn;
		}

		protected override Func<Ie3R04Type, string> GetLocalReferenceNumber => messageObject => messageObject.Lrn;

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3R04Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("1b64788f-146a-4180-9b42-86b8da564178", "An Arrival Response message has been received."));
			htmlBuilder.AppendLine();
			htmlBuilder.AppendLine(Res.GetString("a9d42cd1-7848-45ad-ad35-2975bfa77234", "Records show that Job Number {0} for Master Bill Number {1} has arrived.", manifestHeader.AMA_JobReference, manifestHeader.AMA_MasterBill));
			htmlBuilder.AppendLine();

			var htmlTableCreator = new HtmlTableCreator(Array.Empty<string>());
			htmlTableCreator.WriteRowWithFormatting(
				new CellWithFormatting(Res.GetString("597d98b5-13de-4c17-b8c7-d7eb6d90c94c", "Arrival Response Received"),
					new NameValueCollection {
						Colspan(2),
						TableInterpretation.Attributes.AlignCenter
					}
				)
				{ IsTitle = true });

			htmlTableCreator.WriteRow(new string[] { Res.GetString("16566695-ace9-40b1-a1cf-86f4c2cf19cc", "Job Number"), manifestHeader.AMA_JobReference });
			htmlTableCreator.WriteRow(new string[] { Res.GetString("980ec187-ee43-441d-bd28-fe8028c1ffd4", "LRN"), messageObject.Lrn });
			htmlTableCreator.WriteRow(new string[] { Res.GetString("c7a8ed75-fbdc-42bf-8ef1-598aa3510ccb", "MRN"), messageObject.Mrn });
			htmlTableCreator.WriteRow(new string[] { Res.GetString("4cd39e47-46e7-4f10-a9d6-3f16f898d0e6", "Registration Date"), messageObject.RegistrationDate?.DateTime?.ToString((NoResString)"yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty });
			if (messageObject.NotifyParty != null)
			{
				htmlTableCreator.WriteRow(new string[] { Res.GetString("08ab5014-8d9a-4824-8162-6cc8130d3d35", "Notify Party"), messageObject.NotifyParty.IdentificationNumber });
			}
			if (messageObject.PersonNotifyingTheArrival != null)
			{
				htmlTableCreator.WriteRow(new string[] { Res.GetString("caadd3ed-3fe4-4302-a548-cabc3338c83a", "Person Notifying at Arrival"), messageObject.PersonNotifyingTheArrival.IdentificationNumber });
			}
			if (messageObject.CustomsOfficeOfFirstEntry != null)
			{
				htmlTableCreator.WriteRow(new string[] { Res.GetString("67392a41-7840-4581-8b69-5a6daf74d932", "Customs Office of First Entry"), messageObject.CustomsOfficeOfFirstEntry.ReferenceNumber });
			}

			htmlBuilder.Append(htmlTableCreator.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		NameValueCollection Colspan(int span)
		{
			return TableInterpretation.Attributes.GetColspanAttribute(span);
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3R04Type messageObject)
		{
			return Res.GetString("9354175b-db54-4849-9b22-ffcdc7a9690e", "ICS2 - Arrival Response Received");
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}
