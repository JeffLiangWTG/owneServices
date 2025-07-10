using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel
{
	public static class PropertyDescriptorCollectionFactory
	{
		public const int NumberOfTypesToStringReferenceCache = 40;
	}

	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public class PropertyDescriptorCollectionFactory<T>
		where T : KPropertyDescriptorCollection
	{
		public PropertyDescriptorCollectionFactory(NewCollectionDelegate newDelegate)
		{
			Argument.NotNull(newDelegate, nameof(newDelegate));
			NewDelegate = newDelegate;
			TypeDescriptor.Refreshed += TypeDescriptor_Refreshed;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
		public delegate T NewCollectionDelegate(Type componentType);

		readonly object lockObject = new object();

		public virtual T FromType(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType)); // Suggested By ReviewBot 
			T result = Instances[componentType];
			if (result == null)
			{
				lock (lockObject)
				{
					result = Instances[componentType];
					if (result == null)
					{
						result = NewDelegate(componentType);
						if (result != null)
						{
							Instances.Add(componentType, result);
						}
						TypeDescriptor.GetAttributes(componentType); // This will allow Refreshed event to be raised when TypeDescriptor.Refresh(componentType) is called
					}
				}
			}
			if (result != null)
			{
				result = includePrivate ? (T)result.AllProperties : result;
				result.PopulatePropertyDescriptors();
			}
			return result;
		}

		#region Implementation

		readonly LRUCache<Type, T> Instances = new LRUCache<Type, T>(PropertyDescriptorCollectionFactory.NumberOfTypesToStringReferenceCache);

		readonly NewCollectionDelegate NewDelegate;

		void TypeDescriptor_Refreshed(RefreshEventArgs e)
		{
			Argument.NotNull(e, nameof(e)); // Suggested By ReviewBot
			if (e.TypeChanged != null)
			{
				Instances.Remove(e.TypeChanged);
			}
			else if (e.ComponentChanged == null)
			{
				Instances.Clear();
			}
		}

		#endregion
	}
}
