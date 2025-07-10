using System.Globalization;
using System.Linq;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateContext : EntityContext, IUpdateContext
	{
		public UpdateContext(AncillaryImportServices sessionServices, INativeFactoryProvider factoryProvider)
			: base(sessionServices, factoryProvider)
		{
			EntityInfo = new EntityInfo();
		}

		public new void Import(IEntitySet entitySet)
		{
			ValidateIfPreConditionsAreMet(entitySet);

			SetupOrgCodeGenerationSetting_AndFixMixedCase(entitySet);
			base.Import(entitySet);
		}

		void ValidateIfPreConditionsAreMet(IEntitySet entitySet)
		{
			if (entitySet != null)
			{
				NativeXMLSupportValidator.CheckEntityIsSupported(entitySet.Name, entitySet.Root.EntityName);

				if (!string.IsNullOrEmpty(entitySet.Definition.NoImportReason))
				{
					throw new NativeXMLUserVisibleException(entitySet.Definition.NoImportReason);
				}
			}
		}

		void SetupOrgCodeGenerationSetting_AndFixMixedCase(IEntitySet entitySet)
		{
			if (entitySet.Name == NativeDataTypeList.Codes.Organization)
			{
				bool hasOrgCode;
				try
				{
					hasOrgCode = !string.IsNullOrEmpty(entitySet.Root["Code"].ToString());
				}
				catch (Common.Exceptions.PropertyNotExistException)
				{
					hasOrgCode = false;
				}

				if (!hasOrgCode || !Environment.Env.Registry.CanUserEditOrganisationCode)
				{
					var orgCodeGenerationSetting = InterceptorSettings.SingleOrDefault(s => s is OrgCodeGenerations.OrgCodeGenerationSetting);
					if (orgCodeGenerationSetting != null)
					{
						orgCodeGenerationSetting.Enable = true;
					}
				}

				string fullName;
				try
				{
					fullName = entitySet.Root["FullName"].ToString();
				}
				catch (Common.Exceptions.PropertyNotExistException)
				{
					fullName = null;
				}

				if (fullName != null && !Environment.Env.Registry.OrgAllowMixedCase)
				{
					entitySet.Root["FullName"] = fullName.ToUpper(CultureInfo.CurrentCulture);
				}
			}
		}

		/// <summary>
		/// Only use in response
		/// </summary>
		public EntityInfo EntityInfo { get; }
	}
}
