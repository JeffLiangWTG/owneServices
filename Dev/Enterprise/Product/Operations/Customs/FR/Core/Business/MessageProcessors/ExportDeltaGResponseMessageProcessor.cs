using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class ExportDeltaCResponseMessageProcessor : BaseDeltaGResponseMessageProcessor
	{
		public ExportDeltaCResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override void DoExtraProcessingCore(IResponseDataProvider dataProvider, CusEntryHeader cusEntryHeader)
		{
			var provider = dataProvider as DeltaCExportResponseMessageDataObject;
			ZDateTime.TryParseExact($"{dataProvider.EtatDate} {dataProvider.EtatHeure}", out var issueDate, (NoResString)"dd/MM/yyyy HH:mm");
			if (!provider.mrnecs.IsEmpty)
			{
				cusEntryHeader.MovementReferenceNumberSetter(provider.mrnecs, issueDate);
				SendCINMessageIfApplicable(cusEntryHeader, dataProvider);
			}
		}

		protected override void UpdateStatus(CusEntryHeader entry, IResponseDataProvider dataProvider, out ZBool updatedToBAEForTheFirstTime)
		{
			ExportDeltaGResponseMessageProcessorHelper.UpdateExitedStatus(entry, dataProvider);
			base.UpdateStatus(entry, dataProvider, out updatedToBAEForTheFirstTime);
		}

		protected override ZString GetResponseStatus(CusEntryHeader entry, IResponseDataProvider dataProvider)
		{
			var result = base.GetResponseStatus(entry, dataProvider);
			if (result.IsEmpty && entry.Factory.GetCachedValue<ExportControlStatusList>().ContainsCode(ExportDeltaGResponseMessageProcessorHelper.GetResponseStatus(dataProvider)))
			{
				result = EntryStatusDescriptionCodeList.Codes.ES100;
			}

			return result;
		}

		protected override ZString MessageType => MessageTypeList.Codes.EXC;
	}

	public class ExportDeltaDResponseMessageProcessor : BaseDeltaGResponseMessageProcessor
	{
		public ExportDeltaDResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override void DoExtraProcessingCore(IResponseDataProvider dataProvider, CusEntryHeader cusEntryHeader)
		{
			var provider = dataProvider as DeltaDExportResponseMessageDataObject;
			ZDateTime.TryParseExact($"{dataProvider.EtatDate} {dataProvider.EtatHeure}", out var issueDate, (NoResString)"dd/MM/yyyy HH:mm");
			if (!provider.mrn.IsEmpty)
			{
				cusEntryHeader.MovementReferenceNumberSetter(provider.mrn, issueDate);
				SendCINMessageIfApplicable(cusEntryHeader, dataProvider);
			}
		}

		protected override void UpdateStatus(CusEntryHeader entry, IResponseDataProvider dataProvider, out ZBool updatedToBAEForTheFirstTime)
		{
			ExportDeltaGResponseMessageProcessorHelper.UpdateExitedStatus(entry, dataProvider);
			base.UpdateStatus(entry, dataProvider, out updatedToBAEForTheFirstTime);
		}

		protected override ZString GetResponseStatus(CusEntryHeader entry, IResponseDataProvider dataProvider)
		{
			var result = base.GetResponseStatus(entry, dataProvider);
			if (result.IsEmpty && entry.Factory.GetCachedValue<ExportControlStatusList>().ContainsCode(ExportDeltaGResponseMessageProcessorHelper.GetResponseStatus(dataProvider)))
			{
				result = EntryStatusDescriptionCodeList.Codes.ES100;
			}

			return result;
		}

		protected override ZString MessageType => MessageTypeList.Codes.EXD;
	}

	class ExportDeltaGResponseMessageProcessorHelper
	{
		public static void UpdateExitedStatus(CusEntryHeader entry, IResponseDataProvider dataProvider)
		{
			var etatECS = GetResponseStatus(dataProvider);
			if (!etatECS.IsEmpty)
			{
				entry.CH_ExitedStatus = etatECS;
				ZDateTime.TryParseExact($"{dataProvider.EtatDate} {dataProvider.EtatHeure}", out var eventDate, (NoResString)"dd/MM/yyyy HH:mm");
				entry.Logs.AddNew(Events.CustomsEntryStatus, $"ECS={etatECS}", eventDate.IsEmpty ? ZDateTimeOffset.Now : new ZDateTimeOffset(eventDate));
			}
		}

		public static ZString GetResponseStatus(IResponseDataProvider dataProvider)
		{
			var result = dataProvider.EtatECS;
			if (result.IsEmpty)
			{
				var etatSplit = dataProvider.Etat.Split('/');
				if (etatSplit.Length > 1)
				{
					result = etatSplit[1];
				}
			}

			return result;
		}
	}
}

