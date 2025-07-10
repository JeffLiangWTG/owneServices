using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobOperatorActionMethodApplicator : UpdateJobActionMethodApplicatorBase
	{
		public UpdateJobOperatorActionMethodApplicator(BusinessObjectFactory factory) : base(factory, "UpdateJobOperatorActionMethodApplicator")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string Operator = "Operator";
		}

		#endregion

		protected override string[] UpdateJobProperty(JobHeader job)
		{
			var erros = Array.Empty<string>();

			job.JH_GS_NKRepOps = Operator;
			if (job.JH_GS_NKRepOpsInfo.HasErrors())
			{
				erros = job.JH_GS_NKRepOpsInfo.GetErrors().Select(x => x.Message).ToArray();
			}

			return erros;
		}

		protected override bool GetPropertyReadonly(JobHeader job)
		{
			return job.JH_GS_NKRepOpsInfo.ReadOnly;
		}

		protected override bool GetValueSameWithPrevious(JobHeader job)
		{
			return job.JH_GS_NKRepOps == Operator;
		}

		[RelatedBusinessObject("RepOps")]
		[List("OperatorList")]
		public ZString Operator
		{
			get
			{
				return fOperator;
			}
			set
			{
				SetNonPersistentPropertyValue(OperatorInfo, ref fOperator, value);
				if (!IsValidationSuspended)
				{
					ValidateOperator();
				}
			}
		}
		ZString fOperator;

		public ZPropertyInfo OperatorInfo => GetZPropertyInfo(Schema.Operator);

		public GlbStaff RepOps
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, Operator); }
		}

		public GlbStaffCollection OperatorList
		{
			get
			{
				if (fOperatorList == null)
				{
					fOperatorList = new GlbStaffCollection(Factory);
				}

				return fOperatorList;
			}
		}
		GlbStaffCollection fOperatorList;

		#region Validation

		protected override void ValidateAll()
		{
			ValidateOperator();
		}

		void ValidateOperator()
		{
			OperatorInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OperatorInfo);
			ListValidation.ErrorIfInvalidCode(OperatorInfo);
		}

		#endregion
	}
}
