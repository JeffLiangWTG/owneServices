using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class Package : BasePackage, Integration.Customs.CH.IPackage
{
	public Package(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);

	public new PackageValidation Validation => (PackageValidation)base.Validation;

	protected override bool ShouldDeleteIfPackQtyIsEmpty => false;

	[List(nameof(PackTypeList))]
	public override ZString CW_PackType
	{
		get => base.CW_PackType;
		set
		{
			base.CW_PackType = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCW_PackQty();
				Validation.ValidateCW_MarksAndNos();

				Declaration.InvoiceLines.RefreshBindingIncludingChildren();
			}
		}
	}

	public ZBool IsLoosePackaging => Factory.GetCached(ref fIsLoosePackaging, () => IsBulkOnlyPackaging || RefCusCodeListLoader.IsBreakBulkPackType(Factory, CW_PackType));
	CachedProperty<ZBool> fIsLoosePackaging;

	public ZBool IsBulkOnlyPackaging => Factory.GetCached(ref fIsBulkOnlyPackaging, () => RefCusCodeListLoader.IsBulkPackType(Factory, CW_PackType));
	CachedProperty<ZBool> fIsBulkOnlyPackaging;

	public ZBool IsBreakBulkPackaging => Factory.GetCached(ref fIsBreakBulkPackaging, () => RefCusCodeListLoader.IsBreakBulkPackType(Factory, CW_PackType));
	CachedProperty<ZBool> fIsBreakBulkPackaging;

	public ZInt TotalPackQtyOnInvoiceLines => Factory.GetCached(ref fTotalLinesPackQty,
		() => InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Sum(x => x.CHC_NumberOfPacks));
	CachedProperty<ZInt> fTotalLinesPackQty;

	protected override BasePackagePivotsCollection<Customs.Business.InvoiceLinePackagePivot> GetNewInvoiceLinePivotCollectionCore() => new PackagePivotsCollection(this);
}
