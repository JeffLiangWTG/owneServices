using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceHeaderDescription : CusCodeData
	{
		public InvoiceHeaderDescription(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int CY_Data_MaxLength = 100;
		}

		#region Properties

		[MaxLength(Schema.CY_Data_MaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		public JobComInvoiceHeader InvoiceHeader => Parent as JobComInvoiceHeader;

		#endregion

		#region Override Method

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceHeader));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CY_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.DescriptionCode;
		}

		#endregion
	}
}
