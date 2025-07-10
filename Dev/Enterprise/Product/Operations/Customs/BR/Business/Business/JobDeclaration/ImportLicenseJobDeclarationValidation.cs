using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportLicenseJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckEntranceOfficeCode()
		{
			base.CheckEntranceOfficeCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.EntranceOfficeCodeInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();

			if (Parent.AttachedOrders.Count > 0)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("aeb6181a-d557-470f-b896-823297578d49", "Orders should not be attached to Import License jobs. Please either detach the Order(s) or change the Shipment Type."));
			}
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			if (Parent.FixedJobMessageType != BRJobMessageTypeList.Codes.ImportLicense)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("3fa14674-8aa8-4837-964a-7d3ffb92ebd8", "This Shipment Type can only be used on the Licenses module (Operate > Customs > License)."));
			}
			base.CheckJE_MessageTypeIsEnteredOrValid();
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsOriginInfo);
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			var targetInfo = Parent.JE_OH_ImporterInfo;
			var registrationNumber = Parent.Importer?.PrimaryRegistrationNumber;
			CheckRegistrationNumberEntered(targetInfo, registrationNumber);
			CheckRegistrationNumberIsCJN(targetInfo, registrationNumber);
		}

		protected override void CheckJE_MessageSubType()
		{
		}

		protected override void CheckJE_RL_NKOrigin()
		{
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
		}
	}
}
