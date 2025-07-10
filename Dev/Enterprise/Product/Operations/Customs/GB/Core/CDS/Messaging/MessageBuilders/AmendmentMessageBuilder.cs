using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using static Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class AmendmentMessageBuilder : CancellationRequestMessageBuilder, IGbCDSMessageBuilder
	{
		public AmendmentMessageBuilder(JobDeclarationMessageSendingObject objectToSend, string functionCode) : base(objectToSend, functionCode)
		{
		}

		public override ZString Build()
		{
			ZString messageText = string.Empty;

			var amendments = GetAmendmentWrapper();
			if (amendments?.AmendmentObjects.Any() ?? false)
			{
				var metaData = CreateMetaData();
				var metaDeclaration = CreateMetaDeclaration();
				metaDeclaration.Amendment = GetDeclarationAmendments(amendments.AmendmentObjects).ToArray();

				AddExtraData(metaDeclaration);

				messageText = metaData
					.SetDeclaration(metaDeclaration)
					.Serialize();

				messageText = InsertAmendmentXml(messageText, amendments);
				messageText = EnsureCorrectOrder(messageText);
			}

			return messageText.FormatXml();
		}

		protected override DeclarationAdditionalInformation[] CreateAdditionalInformation()
		{
			int sequenceNumeric = 1;
			return ObjectToSend.AmendmentDetails?.Amendments?.AmendmentObjects.SelectMany(x => x.Pointers).Select(x =>
				new DeclarationAdditionalInformation
				{
					StatementDescription = new AdditionalInformationStatementDescriptionTextType
					{
						Value = ObjectToSend.VOCReason
					},
					StatementTypeCode = new AdditionalInformationStatementTypeCodeType
					{
						Value = StatementTypeCodes.AES
					},
					Pointer = new DeclarationAdditionalInformationPointer[]
					{
						new DeclarationAdditionalInformationPointer
						{
							SequenceNumeric = 1,
							SequenceNumericSpecified = true,
							DocumentSectionCode = new PointerDocumentSectionCodeType { Value = "42A" }
						},
						new DeclarationAdditionalInformationPointer
						{
							SequenceNumeric = sequenceNumeric++,
							SequenceNumericSpecified = true,
							DocumentSectionCode = new PointerDocumentSectionCodeType { Value = "06A" }
						}
					}
				}).ToArray();
		}

		protected ZString InsertAmendmentXml(ZString messageText, AmendmentObjectWrapper amendments)
		{
			var index = messageText.IndexOf("</Declaration>", StringComparison.OrdinalIgnoreCase);
			if (amendments.Xml != null)
			{
				var reader = amendments.Xml.CreateReader();
				reader.MoveToContent();
				messageText = messageText.InsertSafe(index, ((ZString)reader.ReadInnerXml()).FormatXml());
			}

			return messageText;
		}

		protected virtual void AddExtraData(CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration metaDeclaration)
		{
		}

		protected virtual AmendmentObjectWrapper GetAmendmentWrapper() => ObjectToSend.AmendmentDetails?.Amendments;

		protected ZString EnsureCorrectOrder(ZString messageText)
		{
			var messageTextWithOrderedAdditionalInformations = AmendmentMessageHelper.OrderAdditionalInformations(messageText);
			var metaData = XmlObjectSerializer.Deserialize<MetaData>(messageTextWithOrderedAdditionalInformations);
			var declaration = metaData.GetDeclaration();
			var fullyOrderedDeclaration = new MetaData().SetDeclaration(declaration).Serialize();
			return fullyOrderedDeclaration;
		}

		protected override ZString TypeCode => Constants.ThreeCharFunctionCodes.DeclarationRequestForChange;

		protected IEnumerable<DeclarationAmendment> GetDeclarationAmendments(IEnumerable<AmendmentObject> amendmentObjects)
		{
			var declarationAmendments = new List<DeclarationAmendment>();

			foreach (var amendmentObject in amendmentObjects)
			{
				ProcessAmendment(declarationAmendments, amendmentObject);
			}

			return declarationAmendments;
		}

		void ProcessAmendment(List<DeclarationAmendment> declarationAmendments, AmendmentObject amendmentObject)
		{
			foreach (var pointer in amendmentObject.Pointers)
			{
				CreateDeclarationAmendmentSection(declarationAmendments, amendmentObject, pointer);
			}
		}

		void CreateDeclarationAmendmentSection(List<DeclarationAmendment> declarationAmendments, AmendmentObject amendmentObject, ZString pointer)
		{
			var declarationAmendment = new DeclarationAmendment
			{
				ChangeReasonCode = new AmendmentChangeReasonCodeType
				{
					Value = ObjectToSend.ChangeAcknowledgementIndicator
				}
			};

			var splitPointers = pointer.Split('/').Where(x => x != "absent").ToArray();

			declarationAmendment.Pointer = new DeclarationAmendmentPointer[splitPointers.Length];

			for (int i = 0; i < splitPointers.Length; i++)
			{
				i = CreateDeclarationAmendmentPointer(amendmentObject, declarationAmendment, splitPointers, i);
			}

			declarationAmendments.Add(declarationAmendment);
		}

		int CreateDeclarationAmendmentPointer(AmendmentObject amendmentObject, DeclarationAmendment declarationAmendment, ZString[] splitPointers, int currentIndex)
		{
			var splitPointer = splitPointers[currentIndex];
			var pathInfo = PointerParser.GetPathInfo(splitPointer);

			var tagID = GetTagID(amendmentObject.AmendmentType, currentIndex, splitPointers);

			var pointer = new DeclarationAmendmentPointer
			{
				DocumentSectionCode = new PointerDocumentSectionCodeType
				{
					Value = pathInfo.Number
				},
				TagID = tagID.IsEmpty ? null : new PointerTagIDType
				{
					Value = tagID
				}
			};

			if (decimal.TryParse(pathInfo.SequenceNumber, out var sequenceNumber))
			{
				pointer.SequenceNumeric = sequenceNumber;
				pointer.SequenceNumericSpecified = true;
			}

			declarationAmendment.Pointer[currentIndex] = pointer;

			if (!tagID.IsEmpty)
			{
				currentIndex = splitPointers.Length;
			}

			return currentIndex;
		}

		ZString GetTagID(ZString amendmentType, int currentIndex, ZString[] pointers)
		{
			var tagID = currentIndex == (pointers.Length - 2) ? PointerParser.GetPathInfo(pointers[currentIndex + 1]).Number : string.Empty;

			if (amendmentType == AmendmentType.Deletion && !new PointerParser().IsTerminalNode(string.Join("/", pointers)))
			{
				tagID = ZString.Empty;
			}

			return tagID;
		}
	}
}
