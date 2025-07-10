using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocDeclaration))]
sealed class DocDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
{
	public void TestDocCusEntryHeaderCollectionType()
	{
		AssertType<DocCusEntryHeaderCollection>(DeclarationWrapper.RateEntryHeaders);
	}

	protected override JobDeclaration GetNewJobDeclaration()
	{
		var declaration = JobDeclaration.New(Factory);
		declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
		declaration.JE_ClusterKey = 1;
		return (JobDeclaration)declaration;
	}

	protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
	{
		var result = DocDeclaration.New(declaration, Factory);
		((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
		result.SetReportNameForTesting("Report Name");
		return result;
	}

	#region Implementation

	protected override string TestingCountry
	{
		get { return Enterprise.Core.Constants.CountryCodes.Switzerland; }
	}

	#endregion
}
