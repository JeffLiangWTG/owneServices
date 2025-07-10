using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.Service;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class PpwFallbackEmailSender
	{
		readonly BusinessObjectFactory factory;
		readonly LoggingInformation logger;

		public PpwFallbackEmailSender(BusinessObjectFactory factory, LoggingInformation logger)
		{
			this.factory = factory;
			this.logger = logger;
		}

		public void DoEverything(ZString country)
		{
			var ppwEntryNums = ServiceTaskHelper.GetFallbackEntryNumbersInStatus(factory, DeltaGFallbackStatusList.Codes.PPW, country);

			foreach (var entryNum in ppwEntryNums)
			{
				ProcessFallbackEntriesAwaitingEmailInStatusPPW(entryNum);
			}
		}

		void ProcessFallbackEntriesAwaitingEmailInStatusPPW(CusEntryNumber entryNum)
		{
			bool found = false;
			var entryHeader = factory.Load<CusEntryHeader>(entryNum.CE_ParentID);

			if (entryHeader != null)
			{
				var officeOfDeclaration = entryHeader.Declaration?.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep && !x.CY_Data.IsEmpty);
				found = true;

				if (officeOfDeclaration != null)
				{
					var emailAddress = FindEmail(officeOfDeclaration);
					if (!emailAddress.IsEmpty)
					{
						if (SendPdfOfEntryOfCustomsOfficeByEmail(entryHeader, officeOfDeclaration, emailAddress))
						{
							entryNum.CE_EntryStatus = DeltaGFallbackStatusList.Codes.PPS;
						}
						else
						{
							RenderC88DocumentAndUpdateEntryNumEntryStatus(true, FormattableString.Invariant($"Unable to send Email Address for {officeOfDeclaration.CY_Data} for Customs Entry {entryHeader.CH_BGMReference}, The document has been added to eDocs"), entryHeader, entryNum, Integration.LogType.Information);
						}
					}
					else
					{
						RenderC88DocumentAndUpdateEntryNumEntryStatus(true, FormattableString.Invariant($"Unable to find Email Address for {officeOfDeclaration.CY_Data} for Customs Entry {entryHeader.CH_BGMReference}, the document has been added to eDocs"), entryHeader, entryNum, Integration.LogType.Information);
					}
				}
				else
				{
					RenderC88DocumentAndUpdateEntryNumEntryStatus(true, FormattableString.Invariant($"Unable to find Customs Office of type CAU for Customs Entry {entryHeader.CH_BGMReference}, the document has been added to eDocs"), entryHeader, entryNum, Integration.LogType.Information);
				}
			}
			else
			{
				logger.Log(FormattableString.Invariant($"Unable to find Customs Entry for {entryNum.CE_EntryNum}"), Integration.LogType.Error);
			}

			if (!found)
			{
				entryNum.CE_EntryStatus = DeltaGFallbackStatusList.Codes.ERR;
			}
			entryNum.Factory.Save();
		}

		void RenderC88DocumentAndUpdateEntryNumEntryStatus(bool logMessage, ZString logMessageString, CusEntryHeader entryHeader, CusEntryNumber entryNum, Integration.LogType logType)
		{
			new C88EDocsSaver(entryHeader).RenderDocumentAndSaveInEDocs();

			if (logMessage)
			{
				logger.Log(logMessageString, logType);
			}

			entryNum.CE_EntryStatus = DeltaGFallbackStatusList.Codes.PPS;
		}

		ZString FindEmail(EuOfficeCode cauOffice)
		{
			var emailAddress = ZString.Empty;
			if (cauOffice != null)
			{
				var cauOfficeCode = cauOffice.CY_Data;
				var groupingFromOffice = cauOfficeCode.SubstringSafe(0, 2);

				ZZRefCusCodeListCombined refCusCode = null;
				if (!groupingFromOffice.IsEmpty)
				{
					refCusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, cauOfficeCode, groupingFromOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				}

				emailAddress = refCusCode?.GetAttribute(RefCusCodeListAttributeTypes.Codes.EMAIL) ?? ZString.Empty;
			}

			return emailAddress;
		}

		bool SendPdfOfEntryOfCustomsOfficeByEmail(CusEntryHeader entryHeader, EuOfficeCode cauOffice, string emailAddress)
		{
			var result = false;
			var deliveryInstructions = GenerateRecipientDeliveryinstructionBase(emailAddress, ContactNotifyModes.Email);

			try
			{
				var webService = new DocumentDeliveryService
				{
					LogAction = logger.Log
				};

				using (Environment.DisposableEnvironment.ForBranch(entryHeader.Declaration.JE_GB.ToGuid()))
				{
					result = webService.DeliverDocument(Guid.Parse("a412abb6-0162-46ea-ab55-6c1177a8b131"), CusEntryHeaderSchema.Constants.Prefix, entryHeader.PK.ToGuid(), deliveryInstructions, null).Success;
				}

				if (result)
				{
					logger.Log(FormattableString.Invariant($"Successfully sent email to {cauOffice.CY_Data} for Customs Entry {entryHeader.CH_BGMReference}"), Integration.LogType.Information);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Log(FormattableString.Invariant($"Exception while sending email for Customs Entry {entryHeader.CH_BGMReference} - {ex.Message}"), Integration.LogType.Error);
			}
			return result;
		}

		DeliveryInstructionsBase GenerateRecipientDeliveryinstructionBase(ZString emailAddress, string deliveryMethod)
		{
			var recipient = new DeliveryRecipientBase();
			recipient.Email = emailAddress;
			recipient.EmailAttachmentType = OrgConstants.AttachmentType.PDF;
			recipient.DeliveryMethod = deliveryMethod;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };
			factory.Save();
			return deliveryInstructions;
		}
	}
}
