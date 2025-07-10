using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Update.EntityInfos
{
	public interface IEntityInfoHelper
	{
		void SaveEntityName(EntityInfo entityInfo, IEntitySet entitySet);
		void SaveLocalCode(EntityInfo entityInfo, IEntitySet entitySet);
		void SaveForeignCode(EntityInfo entityInfo, IEntitySet entitySet);
		void SavePrimaryKey(EntityInfo entityInfo, IEntitySet entitySet);
	}

	public class EntityInfoHelper : IEntityInfoHelper
	{
		public void SaveEntityName(EntityInfo entityInfo, IEntitySet entitySet)
		{
			entityInfo.Name = entitySet.Name;
		}

		public void SaveLocalCode(EntityInfo entityInfo, IEntitySet entitySet)
		{
			entityInfo.LocalCode = GetCode(entitySet);
		}

		public void SaveForeignCode(EntityInfo entityInfo, IEntitySet entitySet)
		{
			entityInfo.ExternalCode = GetCode(entitySet);
		}

		public void SavePrimaryKey(EntityInfo entityInfo, IEntitySet entitySet)
		{
			entityInfo.PrimaryKey = entitySet.Root.InternalPK;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string GetCode(IEntitySet entitySet)
		{
			string localCode = null;
			var entity = entitySet.Root;
			if (entity.HasProperty("Code"))
			{
				localCode = (string)entity["Code"];
			}
			return localCode;
		}
	}
}
