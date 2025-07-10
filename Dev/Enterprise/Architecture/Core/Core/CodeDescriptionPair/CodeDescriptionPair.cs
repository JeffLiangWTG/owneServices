using System;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class CodeDescriptionPair : ICodeDescription, IMultilingualDescription
	{
		public CodeDescriptionPair(object code, string description)
			: this(code, (NoResString)description)
		{ }

		public CodeDescriptionPair(object code, MultilingualString description)
		{
			if (code is MultilingualString)
			{
				this.code = (MultilingualString)code;
			}
			else
#if DEBUG
			if (code is string)
#endif
			{
				this.code = (NoResString)code.ToString().TrimEnd();
			}
#if DEBUG
			else
			{
				throw new InvalidCodeDescriptionPairException("You must use a CodeElement to have a non-string value in your CodeDescriptionPairList Element.");
			}
#endif
			this.description = description;
		}

		#region Code / Description Properties

		public MultilingualString MultilingualCode
		{
			get { return code; }
		}

		public string Code
		{
			get { return code.GetUnresolvedString(); }
		}

		public string Description
		{
			get { return MultilingualDescriptionCore; }
		}

		public MultilingualString MultilingualDescription
		{
			get { return MultilingualDescriptionCore; }
		}

		protected virtual MultilingualString MultilingualDescriptionCore
		{
			get { return description; }
		}

		readonly MultilingualString code;
		readonly MultilingualString description;

		public virtual string CodeAndDescription
		{
			get { return Code.Trim() + " - " + Description.Trim(); }
		}

		public override string ToString()
		{
			return Code.Trim();
		}

		public string ToString(OComboBoxDropDownStyle style)
		{
			switch (style)
			{
				case OComboBoxDropDownStyle.CodeOnly:
					return Code.Trim();

				case OComboBoxDropDownStyle.DescriptionOnly:
					return Description.Trim();

				case OComboBoxDropDownStyle.CodeAndDescription:
				default:
					return CodeAndDescription;
			}
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return null; }
		}

		#endregion

		#region Equality

		public override bool Equals(object obj)
		{
			bool result = base.Equals(obj);

			if (!result)
			{
				ICodeDescription codeDescription = obj as ICodeDescription;
				result = codeDescription != null && codeDescription.Code == Code && codeDescription.Description == Description;
			}

			return result;
		}

		public override int GetHashCode()
		{
			return Code.GetHashCode() ^ Description.GetHashCode();
		}

		#endregion
	}

	#region InvalidCodeDescriptionPairException Exception

	[Serializable]
	public class InvalidCodeDescriptionPairException : OdysseyException
	{
		public InvalidCodeDescriptionPairException(string errorMessage)
			: base(errorMessage)
		{
		}

#if NETFRAMEWORK
		protected InvalidCodeDescriptionPairException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}
