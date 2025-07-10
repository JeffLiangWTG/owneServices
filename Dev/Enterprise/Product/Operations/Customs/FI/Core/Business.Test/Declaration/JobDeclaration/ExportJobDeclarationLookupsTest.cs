using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(ExportJobDeclarationLookups))]
sealed class ExportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest<ExportJobDeclarationLookups>
{
	protected override string MessageType => MessageTypeList.Codes.Export;

	protected override ExportJobDeclarationLookups GetLookups() => new ExportJobDeclarationLookups(jobDeclaration);
}
