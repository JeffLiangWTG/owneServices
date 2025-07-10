using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class TaxRegime : SingleCusSupportingInfo
	{
		public TaxRegime(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.TaxRegime;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		[ResourceStringData("Enterprise.Customs.BR.Business.TaxRegime|CSI_Code", Caption = "Tax Regime")]
		[List(nameof(Lookups) + "." + nameof(TaxRegimeLookups.TaxRegimeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;

				if (!IsCopying && oldValue != CSI_Code)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				var oldValue = CSI_ParentTableCode;
				base.CSI_ParentTableCode = value;

				if (!IsCopying && oldValue != CSI_ParentTableCode)
				{
					Parent?.MarkAsNeedingValidation();
				}
			} 
		}

		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;

				if (!IsCopying && oldValue != CSI_SubType)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CSI_Type
		{
			get => base.CSI_Type;
			set
			{
				var oldValue = CSI_Type;
				base.CSI_Type = value;

				if (!IsCopying && oldValue != CSI_Type)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.TaxRegime|CSI_Procedure", Caption = "Legal Base")]
		[List(nameof(Lookups) + "." + nameof(TaxRegimeLookups.LegalBaseList))]
		public override ZString CSI_Procedure { get => base.CSI_Procedure; set => base.CSI_Procedure = value; }

		protected override CusSupportingInfoValidation GetNewValidation() => new TaxRegimeValidation(this);

		public new TaxRegimeLookups Lookups => (TaxRegimeLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new TaxRegimeLookups(this);

		public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_ProcedureInfo;
		}
	}
}
