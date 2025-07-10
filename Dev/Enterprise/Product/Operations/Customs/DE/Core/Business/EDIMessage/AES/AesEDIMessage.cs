using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AesEDIMessage : DEEDIMessage
	{
		public AesEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return ObjectFactory.GetType<Integration.Customs.DE.ICusEntryHeader>();
		}

		protected override string GetMessageReferenceNumber() => DEEDIMessageSharedHelpers.GetDEMessageReferenceNumber(Factory);

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			DEEDIMessageSharedHelpers.GetInterchangeControlReferenceAndFillInPlaceHolders(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsAesSystem;
			EM_MessageType = Messaging.EDIMessageTypeList.Codes.AES;
		}
	}
}
