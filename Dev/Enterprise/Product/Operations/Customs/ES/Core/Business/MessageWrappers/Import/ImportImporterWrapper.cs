using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportImporterWrapper : PartyWrapper, IImportImporterProvider
	{
		public static ImportImporterWrapper New(JobDocAddress jobDocAddress, JobDeclaration declaration)
		{
			return jobDocAddress?.Address?.Header == null ? null : new ImportImporterWrapper(jobDocAddress.Address, declaration);
		}

		ImportImporterWrapper(OrgAddress orgA, JobDeclaration declaration)
			: base(orgA)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override ZString CountryCore => declaration.GetDefaultTerritory(base.CountryCore);

		public ZBool IsIndividual => orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual;
	}
}
