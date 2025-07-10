using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineForReversing : NonPersistentBusinessObject, IObsoleteValidation
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "RegisterEditableChildObject is not overridden in TransactionLineForReversing or NonPersistentBusinessObject class, so it is safe.")]
		public TransactionLineForReversing(TransactionLine businessEntity)
			: base(businessEntity.Factory)
		{
			WrappedBusinessEntity = businessEntity;
			RegisterEditableChildObject(WrappedBusinessEntity);
		}

		public readonly TransactionLine WrappedBusinessEntity;

		#region Properties

		public ZDateTime PostDate => WrappedBusinessEntity.AL_PostDate;

		public ZPropertyInfo PostDateInfo => WrappedBusinessEntity.AL_PostDateInfo;

		public ZString JobNumber => WrappedBusinessEntity.JobNumber;

		public ZPropertyInfo JobNumberInfo => WrappedBusinessEntity.JobNumberInfo;

		public ZDecimal Amount => WrappedBusinessEntity.AL_Calc_DisplayAmount;

		public ZPropertyInfo AmountInfo => WrappedBusinessEntity.AL_Calc_DisplayAmountInfo;

		public ZString CreatingUser => WrappedBusinessEntity.AL_Calc_CreatingUserName;

		public ZPropertyInfo CreatingUserInfo => WrappedBusinessEntity.AL_Calc_CreatingUserNameInfo;

		public ZDateTime CreatedDate => WrappedBusinessEntity.AL_Calc_CreatedDate;

		public ZPropertyInfo CreatedDateInfo => WrappedBusinessEntity.AL_Calc_CreatedDateInfo;

		public ZDateTime ReverseDate
		{
			get => WrappedBusinessEntity.AL_ReverseDate;
			set => WrappedBusinessEntity.AL_ReverseDate = value;
		}

		public ZPropertyInfo ReverseDateInfo => WrappedBusinessEntity.AL_ReverseDateInfo;

		public bool ReverseDate_ReadOnly
		{
			get
			{
				var baseWIPAccrual = (WrappedBusinessEntity as BaseWIPAccrual);
				return (baseWIPAccrual != null && !baseWIPAccrual.IsEditingReverseDateAllowed) || WrappedBusinessEntity.HasRowErrors;
			}
		}

		public ZString TransactionType => WrappedBusinessEntity.AL_LineType;

		public ZPropertyInfo TransactionTypeInfo => WrappedBusinessEntity.AL_LineTypeInfo;

		#region Branch

		[List("Branches")]
		[RelatedBusinessObject("Branch")]
		public ZGuid BranchPK => WrappedBusinessEntity.AL_GB;

		public ZPropertyInfo BranchPKInfo => WrappedBusinessEntity.AL_GBInfo;

		public GlbBranchCollection Branches => WrappedBusinessEntity.Lookups.Branches;

		public GlbBranch Branch => Factory.Load<GlbBranch>(BranchPK);

		#endregion

		#region Department

		[List("Departments")]
		[RelatedBusinessObject("Department")]
		public ZGuid DepartmentPK => WrappedBusinessEntity.AL_GE;

		public ZPropertyInfo DepartmentPKInfo => WrappedBusinessEntity.AL_GEInfo;

		public GlbDepartmentCollection Departments => WrappedBusinessEntity.Lookups.Departments;

		public GlbDepartment Department => Factory.Load<GlbDepartment>(DepartmentPK);

		#endregion

		#region Charge Code

		[List("ChargeCodes")]
		[RelatedBusinessObject("ChargeCode")]
		public ZGuid ChargeCodePK => WrappedBusinessEntity.AL_AC;

		public ZPropertyInfo ChargeCodePKInfo => WrappedBusinessEntity.AL_ACInfo;

		public AccChargeCodeCollection ChargeCodes => WrappedBusinessEntity.Lookups.ChargeCodes;

		public AccChargeCode ChargeCode => Factory.Load<AccChargeCode>(ChargeCodePK);

		#endregion

		#region GL Account

		[List("GLHeaders")]
		[RelatedBusinessObject("GLHeader")]
		public ZGuid GLHeaderPK => WrappedBusinessEntity.AL_AG;

		public ZPropertyInfo GLHeaderPKInfo => WrappedBusinessEntity.AL_AGInfo;

		public AccGLHeaderCollection GLHeaders => WrappedBusinessEntity.Lookups.GLHeaders;

		public AccGLHeader GLHeader => Factory.Load<AccGLHeader>(GLHeaderPK);

		#endregion

		#region Organisation

		[List("Organizations")]
		[RelatedBusinessObject("Header")]
		public ZGuid OrganizationPK => WrappedBusinessEntity.AL_OH;

		public ZPropertyInfo OrganizationPKInfo => WrappedBusinessEntity.AL_OHInfo;

		public OrgHeaderCollection Organizations => WrappedBusinessEntity.Lookups.Headers;

		public OrgHeader Header => Factory.Load<OrgHeader>(OrganizationPK);

		#endregion

		#endregion

	}
}