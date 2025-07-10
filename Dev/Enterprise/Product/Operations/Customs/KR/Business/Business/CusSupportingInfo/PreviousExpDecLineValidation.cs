using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PreviousExpDecLineValidation : CusSupportingInfoValidation
	{
		public PreviousExpDecLineValidation(PreviousExpDecLine parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			if (IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			if (IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ItemNumberInfo);
			}
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			if (IsImport)
			{
				MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.CSI_UnitOfQuantityInfo, Parent.CSI_QuantityInfo);
				if (Parent.Parent is JobComInvoiceLine line)
				{
					var units = line.CusEntryLine?.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>().Where(x => x.HasSameKey(Parent))).Select(x => x.CSI_UnitOfQuantity).Distinct();

					if (units != null && units.Count() > 1)
					{
						Parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("FAB2DD8C-3E79-4BCC-8FC0-BA8B02165E4F", $"There are multiple UQs of Used Qty for this export invoice line. There should be only one UQ. Please check."));
					}
				}
			}
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			if (IsImport)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CSI_QuantityInfo);
			}
		}

		public new PreviousExpDecLine Parent => (PreviousExpDecLine)base.Parent;
		bool IsImport => Parent.Parent.IsImport;
	}
}
