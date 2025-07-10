using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebBranchCodeGuidRegistryDataType : GuidRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var branch = (new BusinessObjectFactory()).Load(ObjectFactory.GetType("IGlbBranch"), proposedValue)
				?? throw new RegistryValidationException(Res.GetString("754F288E-932A-4C37-BA4F-BFE04F6DB0DC", "The web branch cannot be empty."));
			if (string.IsNullOrWhiteSpace((ZString)branch[GlbBranchSchema.GB_WebAddress]))
			{
				throw new RegistryValidationException(Res.GetString("7098840C-4B07-4937-88B8-2ED3DD9FC4F0", "The branch's company's web address can not be empty. Please press F3, go to the Branch Details tab and enter a value for Web Address"));
			}
			if (!(ZBool)branch[GlbBranchSchema.GB_IsActive])
			{
				throw new RegistryValidationException(Res.GetString("C1F1A1A1-A2A2-4FA4-8F92-0A1171A1CEF4", "The branch must be active. Please press F3, go to the Branch Details tab and set it to active"));
			}
		}
	}
}
