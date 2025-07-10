using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderValidation : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderValidation
	{
		public CusTempStorageRegHeaderValidation(CusTempStorageRegHeader parent)
			: base(parent)
		{
		}

		protected override void CheckSRH_PresentationDate()
		{
			base.CheckSRH_PresentationDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SRH_PresentationDateInfo);
		}

		protected override void CheckSRH_Status()
		{
			base.CheckSRH_Status();
			ListValidation.MessageErrorIfInvalidCode(Parent.SRH_StatusInfo);
		}

		protected override void CheckSRH_PreviousReferenceType()
		{
			base.CheckSRH_PreviousReferenceType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.SRH_PreviousReferenceTypeInfo);
		}

		protected override void CheckSRH_Reference()
		{
			base.CheckSRH_Reference();
			MandatoryValidation.CheckEntered(Parent.SRH_ReferenceInfo);
		}

		protected override void CheckSRH_PreviousReference()
		{
			base.CheckSRH_PreviousReference();

			var previousRefType = Parent.SRH_PreviousReferenceType;
			var previousRef = Parent.SRH_PreviousReference;

			if (Parent.Lookups.PreviousReferenceTypeList.ContainsCode(previousRefType)
				&& !previousRefType.StartsWith("4", StringComparison.OrdinalIgnoreCase)
				&& previousRefType != PreviousReferenceType.Codes._OHNE
				&& previousRefType != PreviousReferenceType.Codes._ESUMA)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SRH_PreviousReferenceInfo);
			}

			if (!previousRef.IsEmpty && previousRefType == PreviousReferenceType.Codes._ESUMA)
			{
				var mrnError = MRNFormatValidator.CheckMRNFormat(previousRef, Parent.Factory, Res.GetString("e7de112a-8cdd-4c3c-a9dd-d35c11d159e3", "When previous reference type is 'ESUMA',"));
				if (!mrnError.IsEmpty)
				{
					Parent.SRH_PreviousReferenceInfo.AddMessageError(mrnError);
				}
			}
		}

		protected override void CheckSRH_CustomsOffice()
		{
			base.CheckSRH_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SRH_CustomsOfficeInfo);
		}
	}
}
