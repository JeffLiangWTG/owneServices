using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class CIMFRNMessageValidator : CcsukTransmissionMessageValidator
	{
		public CIMFRNMessageValidator(IBusiness bizO)
			: base(bizO)
		{
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			return GetCommonFieldsToValidate();
		}
	}
}
