using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobDeptActionMethodApplicator : UpdateJobActionMethodApplicatorBase
	{
		public UpdateJobDeptActionMethodApplicator(BusinessObjectFactory factory) : base(factory, "UpdateJobDeptActionMethodApplicator")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string Department = "Department";
		}

		#endregion

		protected override string[] UpdateJobProperty(JobHeader job)
		{
			var errors = Array.Empty<string>();

			job.JH_GE = Department;
			if (job.JH_GEInfo.HasErrors())
			{
				errors = job.JH_GEInfo.GetErrors().Select(x => x.Message).Distinct().ToArray();
			}

			return errors;
		}

		protected override bool GetPropertyReadonly(JobHeader job)
		{
			return job.JH_GEInfo.ReadOnly;
		}

		protected override bool GetValueSameWithPrevious(JobHeader job)
		{
			return job.JH_GE == Department;
		}

		[List("DepartmentList")]
		public ZGuid Department
		{
			get
			{
				return fDepartment;
			}
			set
			{
				SetNonPersistentPropertyValue(DepartmentInfo, ref fDepartment, value);
				if (!IsValidationSuspended)
				{
					ValidateDepartment();
				}
			}
		}
		ZGuid fDepartment;

		public ZPropertyInfo DepartmentInfo => GetZPropertyInfo(Schema.Department);

		public GlbDepartmentCollection DepartmentList
		{
			get { return FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory); }
		}

		#region Validation

		protected override void ValidateAll()
		{
			ValidateDepartment();
		}

		void ValidateDepartment()
		{
			DepartmentInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DepartmentInfo);
			ListValidation.ErrorIfInvalidPK(DepartmentInfo, DepartmentList);
		}

		#endregion
	}
}
