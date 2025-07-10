using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CargoWise.GraphEngine
{
	public interface IKeyProvider<TKey, TEntity>
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Linq does this.")]
		IEnumerable<IGrouping<TEntity, TKey>> GetKeys(IEnumerable<TEntity> entities);
	}
}
