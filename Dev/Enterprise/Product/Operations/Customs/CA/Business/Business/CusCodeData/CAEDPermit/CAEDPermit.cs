using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
namespace Enterprise.Customs.CA.Business
{
	public class DeclarationExportPermit : CusCodeData
	{
		public DeclarationExportPermit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
			set { base.Parent = value; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CusCodeDataLookups(this);
		}

		public new ExportPermitValidation Validation
		{
			get { return (ExportPermitValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ExportPermitValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.Permit;
			CY_Code = CusCodeDataTypeList.Codes.Permit;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobDeclaration)); }
		}
	}
}
