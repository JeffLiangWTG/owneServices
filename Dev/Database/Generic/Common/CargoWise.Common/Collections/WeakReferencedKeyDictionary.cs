using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CargoWise.Common.Collections
{
	/// <summary>
	/// A Dictionary with keys that are always eligible for garbage collection.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix"), SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class WeakReferencedKeyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		public WeakReferencedKeyDictionary()
		{
			inner = new Dictionary<KeyableWeakReference, TValue>();
		}

		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		public TValue this[TKey key]
		{
			get
			{
				TValue result;
				inner.TryGetValue(new KeyableWeakReference(key, false), out result);
				return result;
			}
			set
			{
				KeyableWeakReference myRef = new KeyableWeakReference(key, true);
				RemoveGCdRefs();
				inner[myRef] = value;
			}
		}

		public void Remove(TKey key)
		{
			inner.Remove(new KeyableWeakReference(key, false));
		}

		/// <summary>
		/// Does the key exist in the dictionary?
		/// </summary>
		public bool ContainsKey(TKey key)
		{
			return inner.ContainsKey(new KeyableWeakReference(key, false));
		}

		/// <summary>
		/// Get the number of elements.
		/// </summary>
		public int Count
		{
			get
			{
				RemoveGCdRefs();
				return inner.Count;
			}
		}

		#region Implementation

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Cd")]
		protected void RemoveGCdRefs()
		{
			if (enumeratingCount == 0)
			{
				List<KeyableWeakReference> deadRefs = new List<KeyableWeakReference>();
				foreach (KeyableWeakReference key in inner.Keys)
				{
					if (!key.IsAlive)
					{
						deadRefs.Add(key);
					}
				}

				foreach (KeyableWeakReference deadRef in deadRefs)
				{
					inner.Remove(deadRef);
				}
			}
		}

		/// <summary>
		/// WeakReference that additionally respects the Equals and GetHashCode semantics.
		/// </summary>
		class KeyableWeakReference
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="target">The object being stored or looked up.</param>
			/// <param name="useWeakRef">Store internally using a WeakReference. Use false for dictionary look-ups, and true for dictionary storage.
			/// This will save on allocating WeakReferences which are finalizable objects.</param>
			public KeyableWeakReference(TKey target, bool useWeakRef)
			{
				if (useWeakRef)
				{
					weakRef = new WeakReference(target);
				}
				else
				{
					this.target = target;
				}
			}

			public bool IsAlive
			{
				get { return weakRef != null ? weakRef.IsAlive : target != null; }
			}

			public TKey Target
			{
				get
				{
					if (weakRef != null)
					{
						var trg = weakRef.Target;
						if (trg == null)
						{
							return target;
						}
						else
						{
							return (TKey)trg;
						}
					}
					else
					{
						return target;
					}
				}
			}

			public override bool Equals(object obj)
			{
				if (object.ReferenceEquals(this, obj))
				{
					return true;
				}

				KeyableWeakReference rhs = obj as KeyableWeakReference;
				TKey lhsTarget = Target;
				if (rhs != null)
				{
					TKey rhsTarget = rhs.Target;
					return lhsTarget != null && rhsTarget != null && lhsTarget.Equals(rhsTarget);
				}
				else
				{
					return false;
				}
			}

			[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "target")]
			public override int GetHashCode()
			{
				object target = this.Target;
				if (target != null && !hashCode.HasValue)
				{
					hashCode = target.GetHashCode();
				}

				return hashCode ?? 0;
			}

			int? hashCode;

			readonly WeakReference weakRef;
			readonly TKey target;
		}

		readonly Dictionary<KeyableWeakReference, TValue> inner;

		#endregion

		#region IEnumerable<TValue> Members

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			Interlocked.Increment(ref enumeratingCount);
			try
			{
				foreach (KeyValuePair<KeyableWeakReference, TValue> value in inner)
				{
					if (value.Key.Target != null)
					{
						yield return new KeyValuePair<TKey, TValue>(value.Key.Target, value.Value);
					}
				}
			}
			finally
			{
				Interlocked.Decrement(ref enumeratingCount);
			}
		}

		int enumeratingCount;

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
