using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class TariffDetach : CusCodeData
	{
		public TariffDetach(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ITariffDetachParent TariffDetachParent => Parent as ITariffDetachParent;

		public override bool ReadOnly => base.ReadOnly || (Parent is JobComInvoiceLine invoiceLine && invoiceLine.HasLinkedInvoiceLine);

		public override void OnSaving()
		{
			if (CY_Code.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.BR.Business.TariffDetach|CY_Code", Caption = "Tariff Detach")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				if (!IsCopying && oldValue != CY_Code)
				{
					TariffDetachParent?.TariffDetachs.MarkAsNeedingValidation();
				}
			}
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.TariffDetach;
		}

		protected override CusCodeDataValidation GetNewValidation() => new TariffDetachValidation(this);

		public new TariffDetachValidation Validation => (TariffDetachValidation)base.Validation;
	}
}
