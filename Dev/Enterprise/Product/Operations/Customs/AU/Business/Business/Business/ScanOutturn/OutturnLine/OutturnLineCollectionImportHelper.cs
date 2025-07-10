using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OutturnLineCollectionImportHelper : OutturnLineCollectionHelper
	{
		readonly OutturnLineCollection collection;

		public OutturnLineCollectionImportHelper(OutturnLineCollection collection)
		{
			this.collection = Argument.NotNull(collection, "OutturnLineCollection");
		}

		public void ImportFromScanner(string unmappedFilePath)
		{
			var impl = new ImportCollectionInfoImpl(collection);
			AddCollectionProperties(impl);
			var fileMapper = ObjectFactory.Get<IFileMapper>();

			var importWizard = new ImportWizard(impl, null, fileMapper);
			importWizard.StartingRow = 3;
			importWizard.FileName = unmappedFilePath;
			FillImportMappingCollection(importWizard.Mapping);

			importWizard.ImportIntoCollection(collection);
		}
	}
}
