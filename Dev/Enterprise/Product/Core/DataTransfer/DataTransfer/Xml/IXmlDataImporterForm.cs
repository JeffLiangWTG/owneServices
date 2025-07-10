using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public interface IXmlDataImporterForm : IDisposable
	{
		DataImporter Importer { get; set; }
		void SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(bool @checked, bool visible);
		void ImportFromFile(ZString fileName);
		ZDialogResult ShowDialog();
	}
}
