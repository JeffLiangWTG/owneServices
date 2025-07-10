using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business
{
	public class ClearanceValidation : CusSupportingInfoValidation
	{
		public ClearanceValidation(Clearance parent) : base(parent)
		{
		}

		public new Clearance Parent => (Clearance)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (!Parent.CSI_Code.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("F457602F-0922-424A-8CC5-C2C712989CD2", "Patent allows numeric characters."));
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (!Parent.CSI_ReferenceNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("8799C20A-D107-4D69-A087-E776712F9432", "Entry Number allows numeric characters."));
			}
		}

		protected override void CheckCSI_CustomsOffice()
		{
			base.CheckCSI_CustomsOffice();
			if (!Parent.CSI_CustomsOffice.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_CustomsOfficeInfo.AddMessageError(Res.GetString("7A69B149-B519-4E27-8970-B2AB84CCEB44", "Customs Area allows numeric characters."));
			}
		}

		protected override void CheckCSI_Tariff()
		{
			base.CheckCSI_Tariff();
			if (!Parent.CSI_Tariff.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_TariffInfo.AddMessageError(Res.GetString("96426298-9069-4E1D-B4BD-6FA67B550ED5", "Tariff allows numeric characters."));
			}
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			if (!Parent.CSI_UnitOfQuantity.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("63C37383-519E-41AC-A6E8-DCA7B510CD2A", "UQ allows numeric characters."));
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			if (!Parent.CSI_ReferenceNumber2.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_ReferenceNumber2Info.AddMessageError(Res.GetString("7EF6089B-83EE-4564-93FC-A0F187D68854", "Validation Year allows numeric characters."));
			}
		}
	}
}
