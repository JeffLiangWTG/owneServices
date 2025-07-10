using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business
{
	public class AdditionalProcedureCode : CusCodeData
	{
		public AdditionalProcedureCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_CodeMaxLength = 7;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("9648C4E2-089B-4684-A67A-59185787CC32", "Additional Procedure");

		[ResourceStringData("Enterprise.Customs.EU.Business.AdditionalProcedureCode|CY_Code", Caption = "Additional Procedure Code", MediumCaption = "Add. Procedure Code", ShortCaption = "Add. CPC")]
		[MaxLength(Schema.CY_CodeMaxLength)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				if (oldValue != CY_Code)
				{
					var parent = Parent;
					if (parent != null)
					{
						var invoiceLine = parent as JobComInvoiceLine;
						if (invoiceLine != null)
						{
							invoiceLine.WipeNKTaxType();
						}
						parent.AdditionalProcedureCodesAsStringInfo.RefreshBinding();
					}
				}
			}
		}

		public new IAdditionalProcedureParent Parent => (IAdditionalProcedureParent)base.Parent;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AdditionalProcedureCode;
		}

		protected override CusCodeDataLookups GetNewLookups() => new AdditionalProcedureCodeLookups(this);

		public new AdditionalProcedureCodeLookups Lookups => (AdditionalProcedureCodeLookups)base.Lookups;

		protected override CusCodeDataValidation GetNewValidation() => new AdditionalProcedureCodeValidation(this);

		public new AdditionalProcedureCodeValidation Validation => (AdditionalProcedureCodeValidation)base.Validation;
	}
}
