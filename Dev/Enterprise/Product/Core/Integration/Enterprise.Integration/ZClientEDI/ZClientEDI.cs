using CargoWise.Types;

namespace Enterprise.Integration
{
	public static class ZClientEDI
	{
		public interface IIncidentMain
		{
			ZString IM_Description { get; set; }
		}
		public interface IIncidentSimilarityExclusion { }
	}
}
