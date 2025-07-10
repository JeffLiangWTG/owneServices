using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;

namespace Enterprise.DocumentEngine
{
	public class SerializableDeliveryInstructions
	{
		public void SetDeliveryInstructionsContextInformation(DeliveryInstructions instructions)
		{
			if (instructions != null)
			{
				Language = instructions.Language;
				PrintQueuePK = instructions.PrinterDelivery.PrintQueuePK;
				NumberOfCopies = instructions.PrinterDelivery.NumberOfCopies;
				SpecifiedPageRanges = instructions.PageRangesSpecified ? instructions.SpecifiedPageRangesText : (ZString)"";
				Recipients = instructions.Recipients.Cast<DocDeliveryContact>().Select(r =>
				{
					var recipient = new Recipient();
					recipient.SetRecipient(r);
					return recipient;
				});
				DocumentsToBeDelivered = instructions.DocumentsToBeDelivered.Cast<IDeliverable>().Where(d => d.IncludeInPrint).Select(d =>
				  {
					  var deliverable = new Deliverable();
					  deliverable.SetDeliverable(d);
					  return deliverable;
				  });
				CoverNote = instructions.CoverNote;
				IsDraft = instructions.IsDraft;
				PrintMultiDocPack = instructions.PrintMultiDocPack;
				AutoDeliverMultiDocPack = instructions.AutoDeliverMultiDocPack;
				EDocsToBeDelivered = instructions.EDocsToBeDelivered.Cast<IDeliverable>().Where(d => d.IncludeInPrint).Select(d =>
				{
					var deliverable = new Deliverable();
					deliverable.SetDeliverable(d);
					return deliverable;
				});
			}
		}

		public static SerializableDeliveryInstructions DeserializeDeliveryInstructions(StmDocumentDelivery stmDocumentDelivery)
		{
			return JsonConvert.DeserializeObject<SerializableDeliveryInstructions>(stmDocumentDelivery.SDL_Instructions);
		}

		public ZString Language { get; set; }
		public ZGuid PrintQueuePK { get; set; }
		public ZInt NumberOfCopies { get; set; }
		public ZString SpecifiedPageRanges { get; set; }
		public IEnumerable<Recipient> Recipients { get; set; }
		public IEnumerable<Deliverable> DocumentsToBeDelivered { get; set; }
		public ZString CoverNote { get; set; }
		public ZBool IsDraft { get; set; }
		public ZBool PrintMultiDocPack { get; set; }
		public ZBool AutoDeliverMultiDocPack { get; set; }
		public IEnumerable<Deliverable> EDocsToBeDelivered { get; set; }

		public class Recipient
		{
			public ZGuid OrgHeaderPK { get; set; }
			public ZString DeliveryMethod { get; set; }
			public ZString AttachmentType { get; set; }
			public ZBool SendIndividually { get; set; }
			public ZString DeliveryAddress { get; set; }
			public ZString Salutation { get; set; }
			public string ContactName { get; set; }
			public ZString StaffCode { get; set; }
			public ZString SendFrom { get; set; }

			public ZString EmailCarbonCopyRecipientsAsString { get; set; }
			public ZString EmailBlindCarbonCopyRecipientsAsString { get; set; }
			public ZString EmailSubjectMacro { get; set; }

			public void SetRecipient(DocDeliveryContact r)
			{
				if (r != null)
				{
					OrgHeaderPK = r.OrgHeaderPK;
					DeliveryMethod = r.DeliveryMethod;
					AttachmentType = r.AttachmentType;
					SendIndividually = r.SendIndividually;
					DeliveryAddress = r.DeliveryAddress;
					Salutation = r.Salutation;
					ContactName = r.Contact?.OC_ContactName;
					StaffCode = r.StaffCode;
					SendFrom = r.EmailFromAddressWithType;
					EmailCarbonCopyRecipientsAsString = r.EmailCarbonCopyRecipientsAsString;
					EmailBlindCarbonCopyRecipientsAsString = r.EmailBlindCarbonCopyRecipientsAsString;
					EmailSubjectMacro = r.EmailSubjectMacro;
				}
			}
		}

		public class Deliverable
		{
			public ZGuid PrintQueuePK { get; set; }
			public ZInt Copies { get; set; }
			public ZByte Index { get; set; }
			public ZGuid IdentifiablePK { get; set; }
			public ZGuid MenuTemplatePivotPK { get; set; }
			public ZString Identifier { get; set; }

			public void SetDeliverable(IDeliverable d)
			{
				if (d != null)
				{
					Identifier = d.Identifier;
					Index = d.Index;
					IdentifiablePK = d.IdentifiablePK;
					MenuTemplatePivotPK = d.MenuTemplatePivotPK;
					if (d.PrinterDetails != null)
					{
						PrintQueuePK = d.PrinterDetails.PrintQueuePK;
						Copies = d.PrinterDetails.NumberOfCopies;
					}
				}
			}
		}
	}
}
