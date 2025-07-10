using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BRCImportLicenseStatusMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCImportLicenseStatusMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("AA30F848-9A22-4509-9088-19BD82173855", "Import License Status Update");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.LIS };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			var li = XmlObjectSerializer.Deserialize<licompletatype>(message.EM_MessageText);
			var importLicenseNumber = new ZString(li.GrupoDadosBasicos.numeroli).KeepNumericCharacters();

			var entryHeader = new CusEntryHeader.Loader(message.Factory).GetEntryHeaderByMRNQuery(importLicenseNumber, MessageTypeList.Codes.LIC);
			if (entryHeader == null)
			{
				Logger.LogError($"Unable to find an Entry with Entry Number '{importLicenseNumber}' for {message.EM_MessageType} message #{message.EM_MessageNum}");
			}

			return entryHeader;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var li = XmlObjectSerializer.Deserialize<licompletatype>(message.EM_MessageText);
				var statusCode = new ZString(li.GrupoLIAnuencias?.InformacoesLI?.codigosituacao);
				var valitidyShipment = new ZString(li.GrupoLIAnuencias?.InformacoesLI?.datavalidadeembarque);
				var valitidyDispatch = new ZString(li.GrupoLIAnuencias?.InformacoesLI?.datavalidadedespacho);
				ZDateTime.TryParseExact($"{li.GrupoLIAnuencias?.InformacoesLI?.datasituacao} {li.GrupoLIAnuencias?.InformacoesLI?.horasituacao}", out var releaseDate, (NoResString)$"{Constants.DataFormat} HH:mm:ss");

				var hasValidStatusCode = !statusCode.IsEmpty && statusCode.IsNumbersOnlyOrEmpty;
				if (hasValidStatusCode)
				{
					entryHeader.CH_EntryStatus = $"L{statusCode}";

					var issueDate = releaseDate.IsValid ? releaseDate.ToOffset() : ZDateTimeOffset.Now;
					entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, entryHeader.CH_EntryStatus, issueDate);
				}

				if (!valitidyShipment.IsEmpty && ZDateTime.TryParseExact(valitidyShipment, out var dtValidityShipment, Constants.DataFormat))
				{
					entryHeader.CH_ValidityILShipmentDate = dtValidityShipment;
				}

				if (!valitidyDispatch.IsEmpty && ZDateTime.TryParseExact(valitidyDispatch, out var dtValidityDispatch, Constants.DataFormat))
				{
					entryHeader.CH_ValidityILDispatchDate = dtValidityDispatch;
				}

				if (hasValidStatusCode && releaseDate.IsValid && CustomsStatusAttributeHelper.ShouldUpdateReleaseDate(entryHeader.Factory, entryHeader.CH_EntryStatus, Core.Constants.CountryCodes.Brazil, ZDateTime.Today))
				{
					entryHeader.CH_EntryReleaseDate = releaseDate;
				}

				message.EM_MessageInterpretation = new ImportLicenseStatusMessagePrettyFormatter(entryHeader, li).GetFormattedMessageText();
			}
		}
	}
}
