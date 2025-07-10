using System.IO;
namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	interface IProcessor
	{
		void Process(Stream stream);
	}
}