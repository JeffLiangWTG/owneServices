using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.MessageBuilders
{
	public class DeclarationMessageBuilder : IMessageBuilder
	{
		public DeclarationMessageBuilder(JobDeclaration declaration, IMessageGenerator<CusEntryHeader> generator)
		{
			this.generator = generator;
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		#region IMessageBuilder Members

		public IMessageBuilderResult PopulateMessages()
		{
			var result = new MessageBuilderResult();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (!entry.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries)
				{
					entry.InitialiseMasterUCR();    // Needed for when the entry has been created but not yet saved, so entry.CH_MasterUCR has not yet been set. 
					IBuilderResult builderResult = PopulateMessage(entry);
					if (builderResult != null)
					{
						result.AddBuilderResult(builderResult);
					}
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		IBuilderResult PopulateMessage(CusEntryHeader entry)
		{
			IBuilderResult result = generator.Generate(entry);
			if (result != null && result.Message != null)
			{
				result.Message.Saving += message_Saving;
				result.Message.Saved += message_Saved;
			}
			return result;
		}

		void message_Saved(EDIMessage message, bool saveSucceeded)
		{
			if (!saveSucceeded && !message.IsInDatabase)
			{
				EntryHeaderPKAndOriginalStatus entryHeaderPKAndOriginalStatus;
				if (messageEntryHeaderEntryStatusOriginalValue.TryGetValue(message.PK, out entryHeaderPKAndOriginalStatus))
				{
					CusEntryHeader entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeaderPKAndOriginalStatus.PK);
					if (entryHeader != null)
					{
						if (entryHeader.IsInDatabase)
						{
							entryHeader.CH_EntryStatus = entryHeaderPKAndOriginalStatus.EntryStatusOriginalValue;
						}
						else
						{
							entryHeader.CH_EntryStatus = "";
						}
					}
				}
				if (!message.IsDeleted)
				{
					message.Delete();
				}
			}
		}

		void message_Saving(EDIMessage message)
		{
			if (message.IsTransmitMessage && !message.IsInDatabase)
			{
				CusEntryHeader entryHeader = message.EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null)
				{
					entryHeader.FillInBGMReferenceAndMasterUCR();
					var messageText = ReplacePlaceHolders(entryHeader, message.EM_MessageText);
					generator.PutReferenceNumberIntoMessageFromPlaceholder(message, messageText, entryHeader);
					messageEntryHeaderEntryStatusOriginalValue[message.PK] = new EntryHeaderPKAndOriginalStatus(entryHeader.PK, (ZString)entryHeader.CH_EntryStatusInfo.OriginalValue);
					try
					{
						message.EM_MessageInterpretation = generator.MakePrettyForInterpretation(message);
					}
					catch (System.Exception ex) when (!ex.IsCriticalException())
					{
						message.EM_MessageInterpretation = Res.GetString("CA97DB00-AFCE-4F60-89DC-1788BB68A46B", "An error occurred when trying to interpret the message");
						ExceptionReporter.Instance.ReportDeveloperException("MakePrettyForInterpretation threw an exception during saving, message text: {message.EM_MessageText}", ex);
					}
				}
			}
		}

		internal ZString ReplacePlaceHolders(CusEntryHeader entryHeader, ZString messageText)
		{
			return messageText.Replace(CusEntryHeader.UCRReferencePlaceHolder, entryHeader.DeclarationUCR)
				.Replace(CusEntryHeader.UCRPartPlaceHolder, entryHeader.DeclarationUCRPartSuffix)
				.Replace(CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, MakeXmlSafe(entryHeader.CH_BGMReference))
				.Replace(CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, MakeXmlSafe(entryHeader.DeclarationUCR))
				.Replace(CusEntryHeader.UCRPartPlaceHolderXmlFriendly, MakeXmlSafe(entryHeader.DeclarationUCRPartSuffix));
		}

		public static ZString MakeXmlSafe(ZString value)
		{
			return value.ExcludeNonValidXMLCharacters()
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}

		readonly Dictionary<ZGuid, EntryHeaderPKAndOriginalStatus> messageEntryHeaderEntryStatusOriginalValue = new Dictionary<ZGuid, EntryHeaderPKAndOriginalStatus>();

		class EntryHeaderPKAndOriginalStatus
		{
			public EntryHeaderPKAndOriginalStatus(ZGuid pK, ZString entryStatusOriginalValue)
			{
				this.PK = pK;
				this.EntryStatusOriginalValue = entryStatusOriginalValue;
			}

			readonly public ZGuid PK;
			readonly public ZString EntryStatusOriginalValue;
		}

		readonly IMessageGenerator<CusEntryHeader> generator;

		#endregion
	}
}
