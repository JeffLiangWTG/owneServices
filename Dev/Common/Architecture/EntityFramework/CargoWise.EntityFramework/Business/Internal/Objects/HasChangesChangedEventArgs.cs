using System;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	[ImmutableObject(true)]
	public class HasChangesChangedEventArgs : EventArgs
	{
		HasChangesChangedEventArgs(bool objectJustWasChanged, object objectThatWasChanged, bool changeInEditableChildCollection)
		{
			ObjectJustWasChanged = objectJustWasChanged;
			ObjectThatWasChanged = objectThatWasChanged;
			ChangeInEditableChildCollection = changeInEditableChildCollection;
		}

		/// <summary>
		///		Gets a value indicating whether the object was explicitly changed.
		/// </summary>
		/// <returns>
		///		<c>true</c>, if object was explicitly changed; otherwise (object is not changed or it's state is unknown), <c>false</c>.
		/// </returns>
		public readonly bool ObjectJustWasChanged;

		public readonly object ObjectThatWasChanged;

		public readonly bool ChangeInEditableChildCollection;

		public static HasChangesChangedEventArgs Create(bool objectJustWasChanged, object objectThatWasChanged, bool changeInEditableChildCollection = false)
		{
			return objectJustWasChanged ? new HasChangesChangedEventArgs(true, objectThatWasChanged, changeInEditableChildCollection) : new HasChangesChangedEventArgs(false, objectThatWasChanged, changeInEditableChildCollection);
		}
	}
}
