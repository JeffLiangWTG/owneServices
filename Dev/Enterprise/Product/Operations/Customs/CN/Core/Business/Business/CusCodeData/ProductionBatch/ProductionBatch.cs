using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class ProductionBatch : CusCodeData
	{
		public ProductionBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

		#region Override

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.CY_Type = Constants.CusCodeDataTypes.Codes.CIQ;
			this.CY_Code = Constants.CusCodeDataCode.BatchNumber;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ProductionBatchValidation(this);
		}

		public new ProductionBatchValidation Validation => (ProductionBatchValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ProductionBatchLookups(this);
		}

		public new ProductionBatchLookups Lookups => (ProductionBatchLookups)base.Lookups;

		#endregion

		#region CY_Data
		[ResourceStringData("Enterprise.Customs.CN.Business.BatchNumberAndManufactureDate|CY_Data", Caption = "Batch Number")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && CY_Data != oldValue)
				{
					InvoiceLine?.BatchNumbersAsStringInfo.RefreshBinding();
				}
			}
		}
		#endregion

		#region CY_Date
		[ResourceStringData("Enterprise.Customs.CN.Business.BatchNumberAndManufactureDate|CY_Date", Caption = "Manufacture Date")]
		public override ZDateTime CY_Date
		{
			get => base.CY_Date;
			set
			{
				var oldValue = CY_Date;
				base.CY_Date = value;
				if (!IsCopying && CY_Date != oldValue)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						invoiceLine.Validation.ValidateManufactureDatesAsString();
						invoiceLine.ManufactureDatesAsStringInfo.RefreshBinding();
					}
				}
			}
		}
		#endregion
	}
}
