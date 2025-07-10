using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Integration
{
	public interface IPrinterListRetriever
	{
		CodeDescriptionPairList Retrieve();
	}
}

namespace Enterprise.DocumentEngine.Public
{
	using Enterprise.DocumentEngine.Integration;

	public class PrinterListRetriever : IPrinterListRetriever
	{
		#region IPrinterListRetriever Members

		public CodeDescriptionPairList Retrieve()
		{
			return new DocDeliveryPrintDetails(new BusinessObjectFactory()).PrinterNames;
		}

		#endregion
	}
}
