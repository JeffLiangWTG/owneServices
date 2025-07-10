using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class RefundRequest : NonPersistentBusinessObject
	{
		public RefundRequest(EDIMessage message5UL) : base(message5UL.Factory)
		{
			Populate(message5UL);
		}

		CusSupportingInfo SupportingInfo5UL { get; set; }
		CusEntryHeader Entry { get; set; }
		CusEntryNumber EntryNum5UL { get; set; }
		EDIMessage Message5UL { get; set; }

		[ResourceStringData("507FC707-D8F3-46F0-BD26-F5AE858472F6", Caption = "Refund Declaration Number")]
		public ZString RefundDeclarationNumber => MessageFunctions.DeclarationNumberFormat(SupportingInfo5UL?.CSI_ReferenceNumber ?? ZString.Empty);
		[ResourceStringData("2955595C-A827-4674-B5E0-3CB387CE40EC", Caption = "Message Status")]
		public ZString MessageStatus5UL => EntryNum5UL?.CE_EntryStatus ?? ZString.Empty;
		[ResourceStringData("7CC5DF5E-BD82-40DA-AC61-B9C478F69560", ShortCaption = "Message Status Desc.", Caption = "Message Status Description")]
		public ZString MessageStatusDesc5UL => Factory.GetCachedValue<CustomsMessageStatusTypeList>().GetDescriptionFromCode(MessageStatus5UL);
		[ResourceStringData("BCB9D4C3-9CF6-4572-BD3E-1ED522197567", Caption = "Review Result")]
		public ZString ReviewResult => SupportingInfo5UL?.CSI_Status ?? ZString.Empty;
		[ResourceStringData("B2BE027F-1C76-4A5C-809D-BD96A99FAF85", ShortCaption = "Review Result Desc.", Caption = "Review Result Description")]
		public ZString ReviewResultDesc => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(ReviewResult);
		[ResourceStringData("845D8C3D-4825-45B4-BDD8-AAA7D333EA9A", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate5UL => EntryNum5UL?.CE_IssueDate ?? ZDateTime.Empty;
		[ResourceStringData("952593DE-29C2-4E98-A51E-9F72F57220F7", Caption = "Customs Disbursement Bill #")]
		public ZString CustomsDisbursementBillNumber => MessageFunctions.NoticeNumberFormat(SupportingInfo5UL?.CSI_ReferenceNumber2 ?? ZString.Empty);
		[ResourceStringData("527D96CF-921E-4168-9387-C6B9F4EABC51", Caption = "Refund Approval Date")]
		public ZDateTime RefundApprovalDate => SupportingInfo5UL?.CSI_DateOfExpiry ?? ZDateTime.Empty;
		[ResourceStringData("104857A3-A025-41C5-A843-2B33BA899353", ShortCaption = "Refund Approval No.", Caption = "Refund Approval Number")]
		public ZString RefundApprovalNumber => SupportingInfo5UL?.CSI_Tariff ?? ZString.Empty;
		[ResourceStringData("F41C67A5-95EB-44F8-8EFD-AB9F201D688F", Caption = "Provision Date")]
		public ZDateTime ProvisionDate => MessageData5UN?.PaymentDate ?? ZDateTime.Empty;
		[ResourceStringData("6D0390E8-2543-49AA-BDD7-8362A4DEEBC6", ShortCaption = "Provision No.", Caption = "Provision Number")]
		public ZString ProvisionNo => MessageData5UN?.NoticeNumber ?? ZString.Empty;

		GOVCBR5UNMessageData MessageData5UN
		{
			get
			{
				if (messageData5UN == null)
				{					
					var message = Entry?.PK.GetIncomingMessage(Factory, Message5UL.EM_MessageNum, ElectronicDocumentTypeList.Codes._5UN);

					if (message != null)
					{
						using (var reader = message.GetEM_MessageTextReader())
						{
							messageData5UN = new GOVCBR5UNDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5UN;
			}
		}
		GOVCBR5UNMessageData messageData5UN;

		void Populate(EDIMessage message)
		{
			if (message.EM_MessageType != ElectronicDocumentTypeList.Codes._5UL)
			{
				throw new ArgumentException("expected to receive an EDIMessage of '5UL'");
			}

			Message5UL = message;
			Entry = (CusEntryHeader)message.EM_LinkedObject;
			if (Entry?.EntryInstruction != null)
			{
				var supportingQuery = new ZQuery(CusSupportingInfoSchema.CSI_ReferenceNumber, message.EM_MessageOwner);
				supportingQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, ElectronicDocumentTypeList.Codes._5UL);
				supportingQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, Entry.EntryInstruction.PK);
				supportingQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, Entry.EntryInstruction.TablePrefix);
				supportingQuery.OrderBy = CusSupportingInfoSchema.CSI_SystemCreateTimeUtc.Name + " " + OrderByClause.Descending;
				SupportingInfo5UL = Factory.LoadTop1<CusSupportingInfo>(supportingQuery);
			}

			EntryNum5UL = Entry?.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(
								x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL
								&& x.CE_EntryNum == message.EM_MessageOwner);
		}
	}
}
