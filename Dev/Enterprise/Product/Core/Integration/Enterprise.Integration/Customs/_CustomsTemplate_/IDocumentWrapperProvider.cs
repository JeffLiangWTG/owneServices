using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class _CustomsTemplate_
		{
			public interface IDocumentWrapperProvider
			{
				IDocDeclaration NewDocDeclaration(IJobDeclaration declaration, BusinessObjectFactory factoryToWrap);
				IDocJobComInvoiceLine NewDocJobComInvoiceLine(IJobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap);
			}
		}
	}
}
