using System.IO;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Business
{
	public interface IFlatFileConverter
	{
		void ExportFlatFile(IValueObject valueObject, IFlatFileFormat fileFormat, TextWriter flatFileWriter);
		void ImportFlatFile(IValueObject valueObject, IFlatFileFormat fileFormat, TextReader flatFileReader);
	}
}
