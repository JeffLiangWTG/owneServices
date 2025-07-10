using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.SADH
{
	public class SADHFormDataValidation : Customs.Business.SADH.SADHFormDataValidation
	{
		public SADHFormDataValidation(SADHFormData parent)
			: base(parent)
		{
			formData = parent;
		}
		readonly SADHFormData formData;

		protected override void CheckD1_MessageType()
		{
			base.CheckD1_MessageType();
			ValidateD1_EntryType();
			ValidateD1_EntrySubType();
		}

		protected override void CheckD1_EntryType()
		{
			ListValidation.MessageErrorIfInvalidCode(formData.D1_EntryTypeInfo, formData.Lookups.EntryStyleList, (NoResString)D1_EntryTypeIsInvalid);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string D1_EntryTypeIsInvalid = "Please enter a valid Entry Type.";
	}
}
