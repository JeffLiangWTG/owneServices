using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportExternalPackagesInfoWrapper : ExternalPackagesInfoCommonWrapper
	{
		public DUAExportExternalPackagesInfoWrapper(CusEntryLine cusEntryLine) : base(cusEntryLine.Containers.ToList())
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
		}
		readonly CusEntryLine entryLine;

		protected override ZString GetPackageType() => ContainerHelper.GetFirstESContainerPackagingType(entryLine);
	}
}
