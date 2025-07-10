using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class SynchronizedMRUCache<Key, Value> : MRUCache<Key, Value>
	{
		public SynchronizedMRUCache(int maximumElements) : base(maximumElements)
		{
		}

		public override void ClearCache()
		{
			lock (this)
			{
				base.ClearCache();
			}
		}

		public override void SetValue(Key key, Value value)
		{
			lock (this)
			{
				base.SetValue(key, value);
			}
		}

		public override bool TryGetValue(Key key, out Value value)
		{
			lock (this)
			{
				return base.TryGetValue(key, out value);
			}
		}

		public override bool ContainsKey(Key key)
		{
			lock (this)
			{
				return base.ContainsKey(key);
			}
		}
	}

	public class MRUCache<Key, Value>
	{
		public MRUCache(int maximumElements)
		{
			this.maximumElements = maximumElements;
			dictionary = new Dictionary<Key, LinkedListNode<KeyValueHolder>>(maximumElements);
			linkedList = new LinkedList<KeyValueHolder>();
		}

		public virtual void ClearCache()
		{
			dictionary.Clear();
			linkedList.Clear();
		}

		public virtual void SetValue(Key key, Value value)
		{
			LinkedListNode<KeyValueHolder> node;
			KeyValueHolder keyValueHolder;

			if (dictionary.TryGetValue(key, out node))
			{
				linkedList.Remove(node);
				node.Value.Value = value;
			}
			else
			{
				node = new LinkedListNode<KeyValueHolder>(null);
				keyValueHolder = new KeyValueHolder(key, value);
				node.Value = keyValueHolder;
				dictionary[key] = node;
			}

			linkedList.AddFirst(node);
			TruncateAtMaximumElementCount();
		}

		public virtual bool TryGetValue(Key key, out Value value)
		{
			LinkedListNode<KeyValueHolder> node;
			if (dictionary.TryGetValue(key, out node))
			{
				value = node.Value.Value;
				SetValue(key, value);
				return true;
			}
			else
			{
				value = default(Value);
				return false;
			}
		}

		public int MaximumElements
		{
			get { return maximumElements; }
		}

		public virtual bool ContainsKey(Key key)
		{
			return dictionary.ContainsKey(key);
		}

		#region Implementation

		void TruncateAtMaximumElementCount()
		{
			if (linkedList.Count > MaximumElements)
			{
				LinkedListNode<KeyValueHolder> nodeToRemove = linkedList.Last;
				dictionary.Remove(nodeToRemove.Value.Key);
				linkedList.Remove(nodeToRemove);
			}
		}

		readonly Dictionary<Key, LinkedListNode<KeyValueHolder>> dictionary;
		readonly LinkedList<KeyValueHolder> linkedList;

		readonly int maximumElements;

		class KeyValueHolder
		{
			public KeyValueHolder(Key key, Value value)
			{
				this.key = key;
				this.value = value;
			}

			public Key Key
			{
				get { return key; }
			}

			public Value Value
			{
				get { return value; }
				set { this.value = value; }
			}

			public LinkedListNode<KeyValueHolder> Node
			{
				get { return Node; }
			}

			readonly Key key;
			Value value;
		}

		#endregion
	}
}
