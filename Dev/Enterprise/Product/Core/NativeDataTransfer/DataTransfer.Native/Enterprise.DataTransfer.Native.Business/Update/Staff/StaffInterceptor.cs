using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.Staff
{
	public class StaffInterceptor : BaseInterceptor
	{
		public StaffInterceptor(StaffSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override void Invoke(IEntitySet entitySet)
		{
			var staff = entitySet.Root;

			if (staff.HasProperty("LoginName"))
			{
				var loginName = staff["LoginName"].ToString();
				var message = GlbStaffValidation.ValidateLoginName(loginName);
				if (!string.IsNullOrEmpty(message))
				{
					throw new NativeXMLUserVisibleException(message);
				}

				var expectedPrefix = ObjectFactory.Get<IADRegistry>().UserLoginPrefix;
				if (!string.IsNullOrEmpty(expectedPrefix) && !loginName.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
				{
					throw new NativeXMLUserVisibleException(string.Format("Login Name must begin with '{0}'.", expectedPrefix));
				}
			}

			if (staff.HasProperty("Gender"))
			{
				var gender = staff["Gender"].ToString();
				if (string.IsNullOrEmpty(gender) || gender.Length > 1)
				{
					staff["Gender"] = "N";
				}
			}

			if (staff.HasProperty("ActiveDirectoryObjectGuid"))
			{
				staff["ActiveDirectoryObjectGuid"] = "";
			}

			if (InterceptorSetting.Context != null)
			{
				var entityRepository = new EntityRepository(InterceptorSetting.Context, sessionServices);
				entityRepository.OpenSession();

				var matchedEntity = entityRepository.FindRow(staff, false);
				if (matchedEntity != null)
				{
					if ((bool)matchedEntity[GlbStaffSchema.Constants.GS_IsSystemAccount])
					{
						throw new NativeXMLUserVisibleException(string.Format("Modifying system accounts is forbidden."));
					}

					if (SystemDataRegistry.Instance.EnableScimService.Value && !((String)matchedEntity[GlbStaffSchema.Constants.GS_ExternalId]).IsNullOrEmpty())
					{
						throw new NativeXMLUserVisibleException((NoResString)"Import Native XML is disabled when the Enable Scim Service Registry setting has been enabled.");
					}
				}
			}

			Function(entitySet);
		}
	}
}
