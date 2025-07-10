using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocFormedPagesTopLevelPackCollection : NonPersistentBusinessObjectCollection<DocFormedPagesTopLevelPack>
	{
		public DocFormedPagesTopLevelPackCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IEnumerable<DocFormedPagesTopLevelPack> FilteredByMode(ZString mode)
		{
			return this.Cast<DocFormedPagesTopLevelPack>().Where(topLevelPack => topLevelPack.Mode == mode);
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
