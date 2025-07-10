using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class Package : EU.Business.Declaration.Package, Integration.Customs.IE.IPackage
	{
		public Package(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new PackageValidation Validation => (PackageValidation)GetNewValidation();
		protected override Customs.Business.CusDecHouseContainerPackValidation GetNewValidation() => IsExportDeclaration ? new ExportPackageValidation(this) : new PackageValidation(this);

		public PackageType PackageType => Factory.GetValue(ref packageTypeCached, () =>
		{
			var result = PackageType.Packed;
			if (IsBulk)
			{
				result = PackageType.Bulk;
			}
			else if (IsBreakBulk)
			{
				result = PackageType.BreakBulk;
			}
			return result;
		});
		CachedProperty<PackageType> packageTypeCached;

		public override ZString CW_PackType
		{
			get => base.CW_PackType;
			set
			{
				var oldValue = CW_PackType;
				var oldIsBulk = IsBulk;
				base.CW_PackType = value;
				if (!IsCopying && oldValue != CW_PackType)
				{
					ClearCW_PackQtyIfNeeded(oldIsBulk);
					if (!IsValidationSuspended && Validation is PackageValidation validation)
					{
						validation.ValidateCW_PackQty();
						validation.ValidateCW_MarksAndNos();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(CW_PackQty_ReadOnly))]
		public override ZInt CW_PackQty { get => base.CW_PackQty; set => base.CW_PackQty = value; }
		protected bool CW_PackQty_ReadOnly => IsExportDeclaration && IsBulk;

		public bool IsExportDeclaration => Declaration?.IsExport ?? false;

		void ClearCW_PackQtyIfNeeded(bool oldIsBulk)
		{
			if (IsExportDeclaration)
			{
				var isBulk = IsBulk;
				if (oldIsBulk != isBulk)
				{
					ClearPackQty(isBulk);
				}
			}
		}

		internal void ClearPackQty(bool shouldClear)
		{
			if (shouldClear)
			{
				if (!CW_PackQty.IsEmpty)
				{
					CW_PackQty = ZInt.Zero;
				}
				foreach (Customs.Business.InvoiceLinePackagePivot invoiceLinePackagePivot in InvoiceLinePivotCollection)
				{
					if (!invoiceLinePackagePivot.CHC_NumberOfPacks.IsEmpty)
					{
						invoiceLinePackagePivot.CHC_NumberOfPacks = ZInt.Zero;
					}
				}
			}
		}
	}
}
