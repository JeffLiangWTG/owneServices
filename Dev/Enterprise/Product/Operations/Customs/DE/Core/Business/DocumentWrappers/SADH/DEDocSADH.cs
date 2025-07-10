using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using DECusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DEDocSADH : DocSADH
	{
		DEDocSADH(DECusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{ }

		public static DEDocSADH New(DECusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new DEDocSADH(entryHeader, factoryToWrap);

		public new DECusEntryHeader EntryHeader => (DECusEntryHeader)base.EntryHeader;

		public new JobDeclaration Declaration => EntryHeader.Declaration;

		protected override ZString Box17ImporterStateCore => new DeclarationCountryStatesRWCodeBox17bEvaluator(Declaration).Evaluate();

		protected override ZString GetBox18TransportNationalityForExportUCC6Departure(EU.Business.Declaration.JobDeclaration declaration) => declaration.ZG_Box18TransportNationality;
	}
}
