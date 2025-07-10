using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = GetJobDeclaration();
				result.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				result.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				result.CustomsEntryHeaders.AddNew();
				return result;
			}
		}
	}
}
