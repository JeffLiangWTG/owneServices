using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalFiscalReference : EU.Business.Declaration.CusFiscalReference
	{
		public AdditionalFiscalReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("53C0ED34-9B5D-45CF-B818-824011BCA436", Caption = "Type")]
		public override ZString CFR_Code { get => base.CFR_Code; set => base.CFR_Code = value; }

		[MaxLength(17)]
		[ResourceStringData("9FB22F9D-6D46-4633-8F58-DDFF400C2FC1", Caption = "Identification Number", ShortCaption = "ID No.")]
		public override ZString CFR_Reference { get => base.CFR_Reference; set => base.CFR_Reference = value; }

		public new AdditionalFiscalReferenceLookups Lookups => (AdditionalFiscalReferenceLookups)base.Lookups;

		protected override CusReferenceLookups GetNewLookups() => new AdditionalFiscalReferenceLookups(this);

		public new AdditionalFiscalReferenceValidation Validation => (AdditionalFiscalReferenceValidation)base.Validation;

		protected override CusReferenceValidation GetNewValidation() => new AdditionalFiscalReferenceValidation(this);
	}
}
