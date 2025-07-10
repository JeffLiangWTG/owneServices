using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NumberFountainMaximumValueReachedException = Enterprise.NumberFountain.NumberFountainMaximumValueReachedException;

namespace Enterprise.Customs.CA.Business
{
	public class DIFReferenceNumberFountain
	{
		public DIFReferenceNumberFountain(ZString securityNo, BusinessObjectFactory factory)
		{
			this.securityNo = Argument.NotNull(securityNo, nameof(securityNo));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly ZString securityNo;
		readonly BusinessObjectFactory factory;

		public ZString GetNextReferenceNumber()
		{
			var numberFountain = GetFountain();
			if (numberFountain.PeekPreliminaryOrDefault(factory, 0) == 0)
			{
				throw new ZCannotSaveException(TransactionNumberMessages.DIFNotConfigured(securityNo), TransactionNumberMessages.CannotAllocateTransactionNumber());
			}
			string sequentialNumber;
			try
			{
				sequentialNumber = numberFountain.GetNextFormatted(factory);
			}
			catch (NumberFountainMaximumValueReachedException)
			{
				throw new ZCannotSaveException(TransactionNumberMessages.DIFNumberRangeIsEmpty(), TransactionNumberMessages.CannotAllocateTransactionNumber());
			}
			return securityNo + sequentialNumber + TransactionNumber.GetCheckDigit(securityNo + sequentialNumber);
		}

		public long PeekPreliminaryLongOrZero()
		{
			var numberFountain = GetFountain();
			return numberFountain.PeekPreliminaryOrDefault(factory, 0);
		}

		public long GetMaxValue()
		{
			var numberFountain = GetFountain();
			long minValue, maxValue;
			numberFountain.GetMinAndMaxValues(factory, out minValue, out maxValue);
			return maxValue;
		}

		public INumberFountainProxy GetFountain()
		{
			var rangeSeparator = TransactionNumber.GetRangeSeparator(securityNo, null, TransactionNumber.DIFNumberDeclarationType);
			var numberFountainKey = TransactionNumber.GetNumberFountainKey(rangeSeparator);
			return TransactionNumber.GetNumberFountain(numberFountainKey);
		}
	}
}
