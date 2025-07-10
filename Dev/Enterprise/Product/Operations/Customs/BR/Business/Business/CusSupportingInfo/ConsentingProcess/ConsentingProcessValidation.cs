using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ConsentingProcessValidation : CusSupportingInfoValidation
	{
		public ConsentingProcessValidation(ConsentingProcess parent) : base(parent)
		{
		}

		public new ConsentingProcess Parent => (ConsentingProcess)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			if (Parent.CSI_ReferenceNumber.IsEmpty && !Parent.CSI_CustomsOffice.IsEmpty)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("99A06D82-CEA5-4F83-9AB5-1A7FF9A71ADA", "You have entered a Consenting Body, but no Consenting Process Number."));
			}
		}

		protected override void CheckCSI_CustomsOffice()
		{
			base.CheckCSI_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CustomsOfficeInfo);

			if (Parent.CSI_CustomsOffice.IsEmpty && !Parent.CSI_ReferenceNumber.IsEmpty)
			{
				Parent.CSI_CustomsOfficeInfo.AddMessageError(Res.GetString("22566582-9FA0-4BEA-AE75-6145E7E6BAAC", "You have entered a Consenting Process Number, but no Consenting Body."));
			}
		}
	}
}
