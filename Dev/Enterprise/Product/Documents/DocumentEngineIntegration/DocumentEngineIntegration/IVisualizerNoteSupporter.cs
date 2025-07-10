using CargoWise.Types;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IVisualizerNoteSupporter
	{
		ZGuid PK { get; }
		ZGuid ChildBusinessObjectPK { get; }
		string TableCode { get; }
	}
}