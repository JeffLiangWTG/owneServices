using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Stat;

namespace Enterprise.DataTransfer.Native.Common.EntityRepositories
{
	public interface IEntityRepository
	{
		IStatistics Statistics { get; }

		void OpenSession();
		void CloseSession();

		IEntity Insert(IEntity entity);
		IEntity Update(IEntity entity);
		IEntity Delete(IEntity entity);
		IEntity Find(IEntity entity);
		void SetModified(IEntity entity);

		/// <summary>
		/// Merge a batch of entities. If entity exists then update it, else insert.
		/// The action of the entity will be set to UPDATE or INSERT as appropriate.
		/// Entities must all have the same criteria:
		/// - same table
		/// - same property names
		/// - same parent names
		/// </summary>
		void MergeBatch(IList<IEntity> entityList);
	}
}
