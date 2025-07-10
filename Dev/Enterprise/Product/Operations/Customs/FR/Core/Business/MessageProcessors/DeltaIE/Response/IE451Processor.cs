using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE451;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE451Processor : DeltaIEBaseProcessor<CC451BType>
	{
		public IE451Processor(LoggingInformation logger) : base(logger)
		{
		}
		
		protected override void UpdateDeclaration(CusEntryHeader entryHeader, CC451BType messageObject)
		{
			base.UpdateDeclaration(entryHeader, messageObject);
			if (!entryHeader.CH_EntryReleaseDate.IsEmpty)
			{
				entryHeader.CH_EntryReleaseDate = ZDateTime.Empty;
			}
		}
		
		protected override ZString GetLRNFromResponseMessage(CC451BType messageObject) => ZString.Empty;

		protected override ZString GetNewMRN(CC451BType messageObject) => messageObject.ImportOperation.MRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ReleaseRejection;

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.ReleaseRejected;

		protected override ZString GetEntryStatusChangedTimeString(CC451BType messageObject) => messageObject.DeclarationStatus?.StateDateTime;

		protected override ZString EntryHeaderLocatingReferenceType() => CusEntryNumberTypes.Standard.MovementReferenceNumber;
	}
}
