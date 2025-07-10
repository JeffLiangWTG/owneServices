using System.IO;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDocumentPDFWriter
	{
		bool WriteToStream(BusinessObject bizObj, IStmMenuItem menuItem, Stream outputStream);
	}
}
