using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocJobExchangeRateCollection : DocumentWrapperCollection
	{
		public DocJobExchangeRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocJobExchangeRate this[int index]
		{
			get { return (DocJobExchangeRate)base[index]; }
		}
	}
}

