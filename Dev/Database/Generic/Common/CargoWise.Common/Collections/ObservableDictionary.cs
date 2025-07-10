namespace CargoWise.Common.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	///		Represents a dynamic dictionary that provides notifications when items get added, removed, or when the whole list is refreshed.
	/// </summary>
	/// <typeparam name="TKey">
	///		The type of the keys in the dictionary.
	/// </typeparam>
	/// <typeparam name="TValue">
	///		The type of the values in the dictionary.
	/// </typeparam>
	public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
	{
		#region INotifyCollectionChanged

		/// <summary>
		///		Occurs when the collection changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region IDictionary

		/// <summary>
		///		Gets or sets the value associated with the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">
		///		The key of the value to get or set.
		/// </param>
		/// <returns>
		///		The value associated with the specified <paramref name="key"/>. If the specified <paramref name="key"/> is not found, a get operation 
		///		throws a <see cref="KeyNotFoundException"/> and a set operation creates a new element with the specified <paramref name="key"/>.
		///		This method returns false if <paramref name="key"/> is not found in the dictionary.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		///		An element with the same <paramref name="key"/> does not exist in the dictionary.
		/// </exception>
		public TValue this[TKey key]
		{
			get
			{
				return dictionary[key];
			}
			set
			{
				TValue oldValue;
				KeyValuePair<TKey, TValue>? oldItem = null;

				if (dictionary.TryGetValue(key, out oldValue))
				{
					oldItem = AsItem(key, oldValue);
				}

				dictionary[key] = value;
				var newItem = AsItem(key, value);

				if (oldItem.HasValue)
				{
					NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem.Value, newItem);
				}
				else
				{
					NotifyCollectionChanged(NotifyCollectionChangedAction.Add, newItem);
				}
			}
		}

		/// <summary>
		///		Adds the specified <paramref name="key"/> and <paramref name="value"/> to the dictionary.
		/// </summary>
		/// <param name="key">
		///		The key of the element to add.
		/// </param>
		/// <param name="value">
		///		The value of the element to add. The value can be null for reference types.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		An element with the same <paramref name="key"/> already exists in the dictionary.
		/// </exception>
		public void Add(TKey key, TValue value)
		{
			dictionary.Add(key, value);
			NotifyCollectionChanged(NotifyCollectionChangedAction.Add, AsItem(key, value));
		}

		/// <summary>
		///		Removes the value with the specified <paramref name="key"/> from the dictionary.
		/// </summary>
		/// <param name="key">
		///		The key of the element to remove.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="key"/> is null.
		/// </exception>
		/// <returns>
		///		true if the element is successfully found and removed; otherwise, false. 
		///		This method returns false if <paramref name="key"/> is not found in the dictionary.
		/// </returns>
		public bool Remove(TKey key)
		{
			TValue value;

			var exist = dictionary.TryGetValue(key, out value);
			if (exist)
			{
				dictionary.Remove(key);

				NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, AsItem(key, value));
			}

			return exist;
		}

		/// <summary>
		///		Removes all keys and values from the dictionary.
		/// </summary>
		public void Clear()
		{
			dictionary.Clear();
			NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
		}

		/// <summary>
		///		Determines whether the dictionary contains an element with the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">
		///		The key to locate in the dictionary.
		/// </param>
		/// <returns>
		///		true if the dictionary contains an element with the <paramref name="key"/>; otherwise, false.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="key"/> is null.
		/// </exception>
		public bool ContainsKey(TKey key)
		{
			return dictionary.ContainsKey(key);
		}

		/// <summary>
		///		Gets a collection containing the keys of the dictionary.
		/// </summary>
		public ICollection<TKey> Keys
		{
			get
			{
				return dictionary.Keys;
			}
		}

		/// <summary>
		///		Gets the value associated with the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">
		///		The key whose value to get.
		/// </param>
		/// <param name="value">
		///		When this method returns, the value associated with the specified <paramref name="key"/>, if the <paramref name="key"/> is found; 
		///		otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		///		true if the dictionary contains an element with the specified <paramref name="key"/>; otherwise, false.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="key"/> is null.
		/// </exception>
		[SuppressMessage("Microsoft.Contracts", "Ensures")]// Masking the warning as we are reusing the System method here
		public bool TryGetValue(TKey key, out TValue value)
		{
			return dictionary.TryGetValue(key, out value);
		}

		/// <summary>
		///		Gets a collection containing the values of the dictionary.
		/// </summary>
		public ICollection<TValue> Values
		{
			get
			{
				return dictionary.Values;
			}
		}

		/// <summary>
		///		Adds an <paramref name="item"/> to the dictionary.
		/// </summary>
		/// <param name="item">
		///		The object to add to the dictionary.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		The key in <paramref name="item"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		An element with the same key already exists in the dictionary.
		/// </exception>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key == null)
			{
				throw new ArgumentNullException("item.Key");
			}
			Add(item.Key, item.Value);
		}

		/// <summary>
		///		Determines whether the dictionary contains a specific <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </summary>
		/// <param name="item">
		///		The <see cref="KeyValuePair{TKey, TValue}"/> to locate in the dictionary.
		/// </param>
		/// <returns>
		///		true if the <paramref name="item"/> is found in the dictionary; otherwise, false.
		/// </returns>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
		}

		/// <summary>
		///		 Copies the elements of the dictionary to an <see cref="Array"/>, starting at a <paramref name="arrayIndex"/>.
		/// </summary>
		/// <param name="array">
		///		The one-dimensional <see cref="Array"/> that is the destination of the elements copied from dictionary. The <see cref="Array"/> must
		///		have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">
		///		The zero-based index in array at which copying begins.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="array"/> is null.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		The number of elements in the dictionary is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination
		///		array.
		/// </exception>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
		}

		/// <summary>
		///		Gets the number of elements contained in the dictionary.
		/// </summary>
		public int Count
		{
			get
			{
				return dictionary.Count;
			}
		}

		/// <summary>
		///		Gets a value indicating whether the dictionary is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;
			}
		}

		/// <summary>
		///		Removes the <paramref name="item"/> from the dictionary.
		/// </summary>
		/// <param name="item">
		///		The <see cref="KeyValuePair{TKey,TValue}"/> to remove.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		The key in <paramref name="item"/> is null.
		/// </exception>
		/// <returns>
		///		true if the element is successfully found and removed; otherwise, false.
		/// </returns>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key == null)
			{
				throw new ArgumentNullException("item.Key");
			}
			return Remove(item.Key);
		}

		/// <summary>
		///		Returns an enumerator that iterates through the dictionary.
		/// </summary>
		/// <returns>
		///		A <see cref="IEnumerator{T}"/> that can be used to iterate through the dictionary.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		/// <summary>
		///		Returns an <see cref="IEnumerator"/> instance that iterates through the dictionary.
		/// </summary>
		/// <returns>
		///		A <see cref="IEnumerator"/> that can be used to iterate through the dictionary.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)dictionary).GetEnumerator();
		}

		/// <summary>
		///		Determines whether two sequences are equal by comparing the elements by using the default equality comparer for their type.
		/// </summary>
		/// <returns>
		///		True if both dictionaries have the exact same key and values; otherwise, false
		/// </returns>
		public bool Equals(ICollection<KeyValuePair<TKey, TValue>> otherDictionary)
		{
			if (otherDictionary == null || dictionary.Count != otherDictionary.Count)
			{
				return false;
			}
			if (ReferenceEquals(dictionary, otherDictionary))
			{
				return true;
			}

			foreach (KeyValuePair<TKey, TValue> item in otherDictionary)
			{
				if (dictionary.TryGetValue(item.Key, out var value))
				{
					if (!EqualityComparer<TValue>.Default.Equals(value, item.Value))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Internal

		static KeyValuePair<TKey, TValue> AsItem(TKey key, TValue value)
		{
			return new KeyValuePair<TKey, TValue>(key, value);
		}

		void NotifyCollectionChanged(NotifyCollectionChangedAction action)
		{
			var handler = CollectionChanged;
			if (handler != null)
			{
				handler(this, new NotifyCollectionChangedEventArgs(action));
			}
		}

		void NotifyCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
		{
			var handler = CollectionChanged;
			if (handler != null)
			{
				handler(this, new NotifyCollectionChangedEventArgs(action, changedItem));
			}
		}

		void NotifyCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
		{
			var handler = CollectionChanged;
			if (handler != null)
			{
				handler(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
			}
		}

		readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

		#endregion
	}
}
