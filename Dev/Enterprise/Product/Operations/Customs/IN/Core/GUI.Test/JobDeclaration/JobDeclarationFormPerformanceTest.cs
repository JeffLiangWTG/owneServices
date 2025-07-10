using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.GUI.Testing;

abstract class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
{
	protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

	protected override void AddEntryInstructions(BaseJobDeclaration declaration, int numberOfEntryInstructions)
	{
		for (var index = 1; index <= numberOfEntryInstructions; index++)
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = index.ToString();
			instruction.CEI_Style = "A" + index;
		}
	}

	protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(INBaseDeleteExpectedHits, INDeleteExpectedHits);
	Dictionary<string, int> INBaseDeleteExpectedHits => new Dictionary<string, int>
	{
		{ StmDocDataOverrideSchema.Constants.TableName, 8 },
	};

	protected virtual Dictionary<string, int> INDeleteExpectedHits => new Dictionary<string, int>();
}
