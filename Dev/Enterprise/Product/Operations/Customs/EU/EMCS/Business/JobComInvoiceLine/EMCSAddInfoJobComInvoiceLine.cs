using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoJobComInvoiceLine : AddInfo
	{
		public EMCSAddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new EMCSJobComInvoiceLine Parent => (EMCSJobComInvoiceLine)base.Parent;

		public EMCSJobDeclaration Declaration => Parent.Declaration;

		public new EMCSAddInfoJobComInvoiceLineValidation Validation => (EMCSAddInfoJobComInvoiceLineValidation)GetNewValidation();
		protected override EUEMCSAddInfoValidation GetNewValidation() => Parent.Declaration?.GetAddInfoJobComInvoiceLineValidation(this) ?? new EMCSAddInfoJobComInvoiceLineValidation(this);

		public new EMCSAddInfoJobComInvoiceLineLookups Lookups => (EMCSAddInfoJobComInvoiceLineLookups)base.Lookups;
		protected override EUEMCSAddInfoLookups GetNewLookups() => new EMCSAddInfoJobComInvoiceLineLookups(this);

		public override ZDecimal ZG_DeclaredValue
		{
			get => base.ZG_DeclaredValue;
			set
			{
				base.ZG_DeclaredValue = value;
				if (!IsValidationSuspended)
				{
					Parent.Outturn.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobComInvoiceLineLookups.ExciseProductCodes))]
		public override ZString ZG_ExciseProductCode
		{
			get => base.ZG_ExciseProductCode;
			set => base.ZG_ExciseProductCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobComInvoiceLineLookups.WineCategoryList))]
		public override ZString ZG_WineCategory
		{
			get => base.ZG_WineCategory;
			set => base.ZG_WineCategory = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobComInvoiceLineLookups.GrowingZoneList))]
		public override ZString ZG_GrowingZone
		{
			get => base.ZG_GrowingZone;
			set => base.ZG_GrowingZone = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobComInvoiceLineLookups.CountryOfOrigins))]
		public override ZString ZG_WineCountryOrigin
		{
			get => base.ZG_WineCountryOrigin;
			set => base.ZG_WineCountryOrigin = value;
		}
	}
}
