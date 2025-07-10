using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.DB;

namespace Enterprise.DataTransfer.Native.Common.Behaviours
{
	public class BehaviourContext
	{
		public BehaviourContext(
			IEnumerable<IEntity> entities,
			IRowRepository rowRepository,
			ERConverter converter,
			IEntityContext context,
			IEnumerable<IEntity> deletedEntities)
		{
			Entities = entities.ToList();
			MainRowRepository = rowRepository;
			Converter = converter;
			EntityContext = context;
			DeletedEntities = deletedEntities;
		}

		public IEntityContext EntityContext { get; }

		public IEnumerable<IEntity> Entities { get; }

		public IEnumerable<IEntity> DeletedEntities { get; }

		/// <summary>
		/// The main IRowRepository.
		/// Will already contain the rows for the entities being imported.
		/// If any new rows are added by the BehaviourApplicator then they will be saved
		/// AFTER new entity rows, since the framework saves new rows in the order they appear in the DataTable.
		/// The same applies to modified rows. All modified rows are saved first in table order, and then all new rows in table order.
		/// To save changes before this repository is saved, use a new RowRepository with a new RowFactory.
		/// </summary>
		public IRowRepository MainRowRepository { get; }

		public ERConverter Converter { get; }
	}
}
