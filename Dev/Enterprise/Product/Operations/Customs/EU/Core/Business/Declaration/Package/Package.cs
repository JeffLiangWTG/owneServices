using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class Package : Customs.Business.BasePackage
		, Integration.Customs.EU.IPackage
		, ISupportMultipleResourceStringData
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldDeleteIfPackQtyIsEmpty => !IsEmptyPackTypeAllowed;

		public bool IsEmptyPackTypeAllowed => Factory.GetValue(ref isEmptyPackTypeAllowedCached, () => IsEmptyPackTypeAllowedCore);
		protected virtual bool IsEmptyPackTypeAllowedCore => Lookups.BulkAndBreakBulkPackingUnitTypesList.ContainsCode(CW_PackType);
		CachedProperty<bool> isEmptyPackTypeAllowedCached;

		public bool IsBulk => Factory.GetValue(ref isBulkCached, () => Lookups.BulkOnlyPackingUnitTypesList.ContainsCode(CW_PackType));
		CachedProperty<bool> isBulkCached;
		public bool IsBreakBulk => Factory.GetValue(ref isBreakBulkCached, () => Lookups.BreakBulkOnlyPackingUnitTypesList.ContainsCode(CW_PackType));
		CachedProperty<bool> isBreakBulkCached;

		public new PackageLookups Lookups => (PackageLookups)base.Lookups;
		protected override Customs.Business.CusDecHouseContainerPackLookups GetNewLookups() => new PackageLookups(this);

		public new PackageValidation Validation => (PackageValidation)base.Validation;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => Declaration is JobDeclaration jobDeclaration ? jobDeclaration.MultipleKeysToUse : Array.Empty<string>();

		protected override Customs.Business.CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.Package|CW_PackType", Caption = "Pack Type")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.Package|EXPUCC6|CW_PackType", Caption = "Pack Type", FullDescription = "[18 06 003 000] Type of Packages", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CW_PackType { get => base.CW_PackType; set => base.CW_PackType = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.Package|CW_PackQty", Caption = "Pack Qty")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.Package|EXPUCC6|CW_PackQty", Caption = "Pack Qty", FullDescription = "[18 06 004 000] Number of Packages", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZInt CW_PackQty { get => base.CW_PackQty; set => base.CW_PackQty = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.Package|CW_MarksAndNos", Caption = "Marks")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.Package|EXPUCC6|CW_MarksAndNos", Caption = "Marks", FullDescription = "[18 06 054 000] Shipping Marks", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CW_MarksAndNos { get => base.CW_MarksAndNos; set => base.CW_MarksAndNos = value; }
	}
}
