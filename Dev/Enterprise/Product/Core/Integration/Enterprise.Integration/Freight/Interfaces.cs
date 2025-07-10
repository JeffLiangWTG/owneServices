using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface ICommonConsol { }
	public interface ICommonContainer { }
	public interface ICommonContainerLoadList
	{
		ZString CLH_Status { get; set; }
		ZString CLH_LoadMode { get; set; }
	}
	public interface IOrderUpdateHistoryStmNote { }

	public interface IJobComInvHeaderCharge { }

	public interface IPackLineSynchronise
	{
		void MarkSyncDirty();
		void CleanForConcurrency();
	}

	public interface IPackLineSynchroniseProvider
	{
		IPackLineSynchronise PackLineSynchronise { get; }
	}

	public interface ICommunityTransitStatusCodes
	{
		ICodeDescriptionPairList GetList();
	}
}
