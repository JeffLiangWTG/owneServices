using System.Collections;
using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DummyDocDataObjectCollection : DocDataObject, IReadOnlyCollection<DummyDocDataObject>, IEnumerable<DummyDocDataObject>, IEnumerable
	{
		public DummyDocDataObjectCollection(object id = default)
			: base(id)
		{
		}

		#region Main

		public DummyDocDataObject Main
		{
			get => main;
			set => main = SetChild(main, value);
		}

		DummyDocDataObject main;

		#endregion

		public IEnumerator<DummyDocDataObject> GetEnumerator() => collection.GetEnumerator();

		public int Count => collection.Count;

		public IReadOnlyCollection<DummyDocDataObject> Collection
		{
			get => collection;
			set => collection = SetChildCollection(collection, value);
		}

		IReadOnlyCollection<DummyDocDataObject> collection;

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
