using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public sealed class SingleLineEntryProvider : ISingleLineEntryProvider
	{
		public SingleLineEntry GetSingleLineEntry(JobDeclaration declaration) => new SingleLineEntry(declaration);
		public ZString DefaultCPCCode(JobDeclaration declaration) => declaration.IsExport ? (ZString)"1000001" : declaration.IsImport ? (ZString)"4000000" : ZString.Empty;
	}
}
