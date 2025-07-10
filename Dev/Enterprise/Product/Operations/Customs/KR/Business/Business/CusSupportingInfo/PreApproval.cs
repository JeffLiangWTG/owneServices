using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PreApproval : CusSupportingInfo
	{
		public PreApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int PRA_ProcedureMaxLength = 1;
			public const int PRA_ReferenceNumber = 20;
		}

		[MaxLength(Schema.PRA_ProcedureMaxLength)]
		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set => base.CSI_Procedure = value;
		}

		[MaxLength(Schema.PRA_ReferenceNumber)]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new PreApprovalValidation(this);
		public new PreApprovalValidation Validation => (PreApprovalValidation)base.Validation;
		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
