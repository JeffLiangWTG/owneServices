using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR105AmendmentDetailsManager : FTAAmendmentDetailsManager<ImportFTAHeader, IImportFTAHeader>
	{
		public GOVCBR105AmendmentDetailsManager(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		protected override ImportFTAHeader GetCurrentDataProvider() => new ImportFTACreator().Create(Entry);
	}
}
