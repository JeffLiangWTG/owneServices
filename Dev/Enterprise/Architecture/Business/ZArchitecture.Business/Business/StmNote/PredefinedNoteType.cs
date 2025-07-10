using System;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class PredefinedNoteType : CodeDescriptionPair
	{
		public PredefinedNoteType(MultilingualString description, StmNoteVisibility defaultVisibility, bool isOnlyOneAllowed, bool isReadOnlyAfterAdd, bool isTextOnly, bool isCustomNoteType)
			: this(description, defaultVisibility, isOnlyOneAllowed, isReadOnlyAfterAdd, isTextOnly, DefaultTextOnlyMaxLength, isCustomNoteType)
		{
		}

		public PredefinedNoteType(MultilingualString description, StmNoteVisibility defaultVisibility, bool isOnlyOneAllowed, bool isReadOnlyAfterAdd, bool isTextOnly, int textOnlyMaxLength, bool isCustomNoteType)
			: this(description, defaultVisibility, isOnlyOneAllowed, isReadOnlyAfterAdd, isTextOnly, false, textOnlyMaxLength, isCustomNoteType)
		{
		}

		public PredefinedNoteType(MultilingualString description, StmNoteVisibility defaultVisibility, bool isOnlyOneAllowed, bool isReadOnlyAfterAdd, bool isTextOnly, bool isPopupLog, bool isCustomNoteType)
			: this(description, defaultVisibility, isOnlyOneAllowed, isReadOnlyAfterAdd, isTextOnly, isPopupLog, DefaultTextOnlyMaxLength, isCustomNoteType)
		{
		}

		public PredefinedNoteType(MultilingualString description, StmNoteVisibility defaultVisibility, bool isPopupLog, Type serializableNoteType, bool isCustomNoteType)
			: this(description, defaultVisibility, true, true, true, isPopupLog, DefaultTextOnlyMaxLength, isCustomNoteType)
		{
			if (serializableNoteType == null || !serializableNoteType.IsSubclassOf(typeof(SerializableNoteText)))
			{
				throw new ArgumentException("SerializableNoteType is null or is not a subtype of SerializableNoteText");
			}
			SerializableNoteType = serializableNoteType;
		}

		public PredefinedNoteType(MultilingualString description, StmNoteVisibility defaultVisibility, bool isOnlyOneAllowed, bool isReadOnlyAfterAdd, bool isTextOnly, bool isPopupLog, int textOnlyMaxLength, bool isCustomNoteType)
			: base(description, description)
		{
			if (isPopupLog)
			{
				if (!isTextOnly)
				{
					throw new ArgumentException("IsTextOnly must be true if IsPopupLog.", nameof(isTextOnly));
				}
				if (!isOnlyOneAllowed)
				{
					throw new ArgumentException("IsOnlyOneAllowed must be true if IsPopupLog.", nameof(isOnlyOneAllowed));
				}
			}

			DefaultVisibility = defaultVisibility;
			IsOnlyOneAllowed = isOnlyOneAllowed;
			IsReadOnlyAfterAdd = isReadOnlyAfterAdd;
			IsTextOnly = isTextOnly;
			IsPopupLog = isPopupLog;
			TextOnlyMaxLength = textOnlyMaxLength;
			IsCustomNoteType = isCustomNoteType;
		}

		public readonly StmNoteVisibility DefaultVisibility;

		public readonly bool IsCustomNoteType;

		public readonly bool IsOnlyOneAllowed;

		public readonly bool IsReadOnlyAfterAdd;

		public readonly bool IsTextOnly;

		public readonly bool IsPopupLog;

		public readonly int TextOnlyMaxLength;

		public readonly Type SerializableNoteType;

		const int DefaultTextOnlyMaxLength = 50000;
	}
}
