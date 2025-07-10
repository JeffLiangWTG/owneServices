using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceLineValidation : Customs.Business.JobComInvoiceLineValidation
	{
		public EMCSJobComInvoiceLineValidation(EMCSJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new EMCSJobComInvoiceLine Parent => (EMCSJobComInvoiceLine)base.Parent;

		protected override void CheckJI_Procedure()
		{
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			MandatoryValidation.MessageErrorIfIsZero(Parent.JI_NetWeightInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_NetWeightInfo);
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			MandatoryValidation.MessageErrorIfIsZero(Parent.JI_WeightInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_WeightInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			MandatoryValidation.MessageErrorIfIsZero(Parent.JI_CustomsQuantityInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsQuantityInfo);
		}

		protected override void CheckJI_CustomsQuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_CustomsQuantityInfo, 15, 3);
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CustomsUnitQtyInfo);
		}

		protected override void CheckJI_ZZF_NKTaxType()
		{
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_TariffInfo);

			AddTariffInfoMessageErrorNoPackageDetailsIfNecessary();
		}

		bool NeedValidatePackageDetailsIsEmpty => !Parent.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Any(p => p.IsForInvoiceLine);

		void AddTariffInfoMessageErrorNoPackageDetailsIfNecessary()
		{
			var error = ResString.GetMultilingualString("F602F901-B556-41C9-851D-D47D0E67391E", @"This line has no packaging details. This may contribute to a potential overall lack of packaging details. Please tick at least one row on the invoice line's Packages tab.
					If there are no rows offered, please ensure that declaration-level packages are showing on the declaration's Packing tab.");

			var parent = Parent;
			parent.ClearRowNotificationsContaining(error);
			if (NeedValidatePackageDetailsIsEmpty)
			{
				parent.AddRowMessageError(error);
			}
		}

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				Parent.ClearRowNotifications();
				base.ValidateAll();
			}
		}
	}
}
