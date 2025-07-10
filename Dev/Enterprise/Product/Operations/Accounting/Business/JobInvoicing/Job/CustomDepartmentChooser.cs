using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class CustomDepartmentChooser : CustomChooserBase
	{
		public CustomDepartmentChooser(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override string GetCustomDefaultConfiguration()
		{
			return AccountingConfigurationRegistry.Instance.CustomDefaultDepartmentConfiguration.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK).ConfigAsString;
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
					return Res.GetString("2D3934A5-D37E-4527-B4D8-0A5C768124B9", "Error/No Result, will default department as per other registry settings.");
				}

				var department = DepartmentChooser.GetDepartmentBizObjFromDeptCode(Factory, result);
				if (department == null)
				{
					result = Res.GetString("561113E4-E369-4D6F-A447-86BCC42EEE9A", "{0} (Invalid), will default to blank department.", result);
				}
				else if (!department.GE_IsActive)
				{
					result = Res.GetString("1255F093-2DAD-4C27-A5D4-D8021C00241B", "{0} (Inactive), will default to blank department.", result);
				}

				return result;
			}
		}

		protected override string GetDefaultMethodForLambda()
		{
			return (NoResString)"getDefaultDepartment(obj, company, branch, department)";
		}

		protected override string GetDefiningMethod()
		{
			return (NoResString)"def getDefaultDepartment(obj, company, branch, department):";
		}
	}
}