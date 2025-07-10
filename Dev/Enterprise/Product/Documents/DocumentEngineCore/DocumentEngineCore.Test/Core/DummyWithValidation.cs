using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class DummyWithValidation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZBool UseEPrint
		{
			get { return useEPrint; }
			set
			{
				SetNonPersistentPropertyValue(UseEPrintInfo, ref useEPrint, value);
				if (!IsValidationSuspended)
				{
					ValidateUseEPrint();
				}
			}
		}
		ZBool useEPrint;

		public ZPropertyInfo UseEPrintInfo
		{
			get { return this.GetZPropertyInfo(nameof(UseEPrint)); }
		}

		public void ValidateUseEPrint()
		{
			UseEPrintInfo.ClearAllNotifications();
			DeliveryMethodValidation.ValidateEPrint(UseEPrintInfo);
		}
	}
}
