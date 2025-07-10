using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Testing;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DummyDocDataObject : DocDataObject
	{
		public DummyDocDataObject(object id = default)
			: base(id)
		{
		}

		#region Text

		public ZString Text
		{
			get => text;
			set
			{
				if (SetNonPersistentPropertyValue(TextInfo, ref text, value))
				{
					Validate(TextInfo);
				}
			}
		}

		ZString text;

		public ZPropertyInfo TextInfo => GetZPropertyInfo(nameof(Text));

		#endregion

		#region DateTime

		public ZDateTime DateTime
		{
			get => dateTime;
			set
			{
				if (SetNonPersistentPropertyValue(DateTimeInfo, ref dateTime, value))
				{
					Validate(DateTimeInfo);
				}
			}
		}

		ZDateTime dateTime;

		public ZPropertyInfo DateTimeInfo => GetZPropertyInfo(nameof(DateTime));

		#endregion

		#region Code

		[DisableModifiableMember(nameof(Code_DisableModifiable))]
		[CargoWise.ComponentModel.List(nameof(CodeList))]
		[CustomFindBoxPopup(typeof(DummyCustomFindBoxPopup))]
		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
					Validate(CodeInfo);
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public object CodeList { get; set; }

		public bool Code_DisableModifiable { get; set; }

		#endregion

		#region CodeDescription

		public ZString CodeDescription
		{
			get => codeDescription;
			set
			{
				if (SetNonPersistentPropertyValue(CodeDescriptionInfo, ref codeDescription, value))
				{
					Validate(CodeDescriptionInfo);
				}
			}
		}

		ZString codeDescription;

		public ZPropertyInfo CodeDescriptionInfo => GetZPropertyInfo(nameof(CodeDescription));

		#endregion

		#region Child

		public DummyDocDataObject Child
		{
			get => child;
			set => child = SetChild(child, value);
		}

		DummyDocDataObject child;

		#endregion

		#region Wrapped

		public DummyWrappedDocDataObject Wrapped
		{
			get => wrapped;
			set => wrapped = SetChild(wrapped, value);
		}

		DummyWrappedDocDataObject wrapped;

		#endregion

		#region Collection

		public IReadOnlyCollection<DummyDocDataObject> Collection
		{
			get => collection;
			set => collection = SetChildCollection(collection, value);
		}

		IReadOnlyCollection<DummyDocDataObject> collection;

		#endregion

		#region OtherCollection

		public DummyDocDataObjectCollection OtherCollection
		{
			get => otherCollection;
			set => otherCollection = SetChild(otherCollection, value);
		}

		DummyDocDataObjectCollection otherCollection;

		#endregion
	}
}
