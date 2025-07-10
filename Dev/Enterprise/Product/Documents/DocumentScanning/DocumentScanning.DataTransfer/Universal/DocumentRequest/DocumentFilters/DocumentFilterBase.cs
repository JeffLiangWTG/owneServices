using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	abstract class DocumentFilterBase
	{
		public abstract void AddFilterValue(ZString value);

		public abstract bool IsMatch(IeDoc eDoc);
	}
}
