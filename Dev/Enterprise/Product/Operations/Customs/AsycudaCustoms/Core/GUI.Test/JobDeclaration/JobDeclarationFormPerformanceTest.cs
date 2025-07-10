using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		static Dictionary<string, int> AsycudaBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected virtual Dictionary<string, int> AsycudaValidateAllExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(AsycudaBaseValidateAllExpectedHits, AsycudaValidateAllExpectedHits);

		protected override Dictionary<string, int> FormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusSupportingInfoSchema.Constants.TableName, 60 }
		};
	}
}
