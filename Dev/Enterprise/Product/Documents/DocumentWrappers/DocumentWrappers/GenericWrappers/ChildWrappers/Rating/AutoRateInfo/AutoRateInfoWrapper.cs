using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("AutoRateInformation")]
	public class AutoRateInfoWrapper : GenericWrapper
	{
		public AutoRateInfoWrapper(AutoRateInfo parent, BusinessObjectFactory factory)
			: base(null, factory)
		{
			rateInfo = parent;
		}

		readonly AutoRateInfo rateInfo;

		public ZDecimal Amount
		{
			get { return rateInfo.Amount; }
		}

		public ZString AutoRatedForString
		{
			get { return rateInfo.AutoRatedForString; }
		}

		public ZString CalculationSingleLineDescriptionWithoutChargeCode
		{
			get { return rateInfo.CalculationDescription; }
		}

		public ZString InvoiceLineDescription
		{
			get { return rateInfo.InvoiceLineDescription; }
		}
	}
}
