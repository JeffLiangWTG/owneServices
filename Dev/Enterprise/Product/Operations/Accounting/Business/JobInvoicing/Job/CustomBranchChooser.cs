using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class CustomBranchChooser : CustomChooserBase
	{
		public CustomBranchChooser(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override string GetCustomDefaultConfiguration()
		{
			return AccountingConfigurationRegistry.Instance.CustomDefaultBranchConfiguration.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK).ConfigAsString;
		}

		public ZString ValidateConfigWithJob(ZGuid jobGuid, ZString configToValidate)
		{
			ZString result;
			var consumerAndMessage = GetConsumerOfJob(jobGuid);
			var objToValidate = consumerAndMessage.Item1;
			result = consumerAndMessage.Item2;

			if (!result.IsEmpty)
			{
				return result;
			}
			else
			{
				result = GetCodeWithConfiguration(objToValidate, configToValidate);
				if (result.IsEmpty)
				{
					return Res.GetString("9D8F59C0-9C8D-46A2-85ED-09B6322CFC3F", "Error/No Result, will default branch as per other registry settings.");
				}

				var branch = BranchChooser.GetBranchBizObjFromBranchCode(Factory, result);
				if (branch == null)
				{
					result = Res.GetString("3DAD622E-DD8B-4950-8454-1A84F5AD6A9F", "{0} (Invalid), will default to blank branch.", result);
				}
				else if (!branch.GB_IsActive)
				{
					result = Res.GetString("E4F0412F-F9E8-42FE-812C-A443C62E14CC", "{0} (Inactive), will default to blank branch.", result);
				}

				return result;
			}
		}

		protected override string GetDefaultMethodForLambda()
		{
			return (NoResString)"getDefaultBranch(obj, company, branch, department)";
		}

		protected override string GetDefiningMethod()
		{
			return (NoResString)"def getDefaultBranch(obj, company, branch, department):";
		}
	}
}
