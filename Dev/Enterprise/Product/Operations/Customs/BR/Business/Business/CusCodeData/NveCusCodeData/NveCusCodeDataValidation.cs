using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NveCusCodeDataValidation : Customs.Business.CusCodeDataValidation
	{
		public NveCusCodeDataValidation(NveCusCodeData parent)
			: base(parent)
		{
		}

		public new NveCusCodeData Parent => (NveCusCodeData)base.Parent;

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			if (Parent.Parent is JobComInvoiceLine parent)
			{
				if (parent.IsImportSiscomex || parent.IsImportLicense)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SpecificationInfo, Res.GetString("C597F669-A799-4074-A6AA-B4C249E3A3D5", "NVE Attributes Specification"));
				}
			}
		}
	}
}
