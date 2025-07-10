using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class SeaCargoSenderIdRetriever
	{
		public static ZString GetSenderID(GlbBranch branch, ZString parentObjectTable)
		{
			if (branch == null)
			{
				branch = GlbBranch.CurrentBranch;
			}
			if (ObjectIsFreightObject(parentObjectTable))
			{
				return Env.Registry.GetAUCustomsSeaCargoDepotMailbox(branch.GB_GC.ToGuid(), branch.PK.ToGuid());
			}
			else
			{
				return Env.Registry.GetAUCustomsSenderID(branch.GB_GC.ToGuid(), branch.PK.ToGuid());
			}
		}

		static bool ObjectIsFreightObject(ZString parentObjectTable)
		{
			return parentObjectTable == JobConsolSchema.Constants.TableName || parentObjectTable == JobShipmentSchema.Constants.TableName || parentObjectTable == JobContainerSchema.Constants.TableName;
		}
	}
}
