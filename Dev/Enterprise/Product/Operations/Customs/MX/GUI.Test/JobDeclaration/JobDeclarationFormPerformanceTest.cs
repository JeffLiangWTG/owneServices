using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.GUI.Testing
{
	abstract class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
		Dictionary<string, int> MXBaseLoadEditableChildObjectsExpectedHits => new () { { RefTimeZoneSchema.Constants.TableName, 15 }, { RefTimeZoneSetSchema.Constants.TableName, 8 } };
		Dictionary<string, int> MXLoadEditableChildObjectsExpectedHits => new ();
		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(MXBaseLoadEditableChildObjectsExpectedHits, MXLoadEditableChildObjectsExpectedHits);
	}
}
