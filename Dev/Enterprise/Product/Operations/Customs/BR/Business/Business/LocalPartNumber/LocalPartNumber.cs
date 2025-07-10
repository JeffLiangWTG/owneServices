using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class LocalPartNumber : BaseCusGoodsCatalogProductionInfo, Integration.Customs.BR.ILocalPartNumber
	{
		public LocalPartNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsCatalog GoodsCatalog => (CusGoodsCatalog)base.GoodsCatalog;

		public override bool ReadOnly => true;

		[ResourceStringData("Enterprise.Customs.BR.Business.LocalPartNumber|CGI_Reference", Caption = "Local Part Number", FullDescription = "The Local Part Number.")]
		public override ZString CGI_Reference { get => base.CGI_Reference; set => base.CGI_Reference = value; }

		protected override bool SupportsCloneCore() => true;

		protected override CusGoodsCatalogProductionInfoValidation GetNewValidation() => new LocalPartNumberValidation(this);

		public LocalPartNumberPivotFinder PivotFinder => Factory.GetValue(ref fPivotFinder, () => new LocalPartNumberPivotFinder(this));
		CachedProperty<LocalPartNumberPivotFinder> fPivotFinder;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CGI_Type = CusGoodsCatalogProductionInfoTypeList.Codes.LPN;
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				GoodsCatalog?.ResetStatuses();
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				GoodsCatalog?.ResetStatuses();
			}
			base.Delete();
		}
	}
}
