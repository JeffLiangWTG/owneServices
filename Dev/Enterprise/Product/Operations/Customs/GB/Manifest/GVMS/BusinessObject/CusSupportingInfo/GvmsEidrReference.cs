using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsEidrReference : GvmsItemReference
	{
		public GvmsEidrReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : GvmsItemReference.Schema
		{
			public new const int CSI_DescriptionMaxLength = 22;
			public new const int CSI_ProcedureMaxLength = 4;
		}

		[MaxLength(Schema.CSI_DescriptionMaxLength)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		[MaxLength(Schema.CSI_ProcedureMaxLength)]
		public override ZString CSI_Procedure { get => base.CSI_Procedure; set => base.CSI_Procedure = value; }

		protected override CusSupportingInfoLookups GetNewLookups() => new GvmsEidrReferenceLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new GvmsEidrReferenceValidation(this);
		}
	}
}
