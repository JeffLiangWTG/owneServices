using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	#region SuppressResourceStringsCheckRegion

	public abstract class MemberDescription
	{
#if DEBUG
		/// <summary>
		/// Only for test
		/// </summary>
		protected MemberDescription(MemberDescription parentMemberDescription, string helpText, MacroTagTypes macroTagType)
			: this(parentMemberDescription, helpText, macroTagType, new DocDataReflectorFilter(), showIndex: true)
		{
		}
#endif

		protected MemberDescription(MemberDescription parentMemberDescription, string helpText, MacroTagTypes macroTagType, IDataReflectorFilter filter, bool showIndex = true)
		{
			this.ParentMemberDescription = parentMemberDescription;
			this.HelpText = helpText;
			this.MacroTagType = macroTagType;
			this.Filter = filter ?? throw new ArgumentNullException(nameof(filter));
			this.ShowIndex = showIndex;
		}
		public readonly MemberDescription ParentMemberDescription;
		public readonly MacroTagTypes MacroTagType;
		public IDataReflectorFilter Filter { get; private set; }
		protected readonly string HelpText;

		public MemberBelongsTo MemberBelongsTo { get; set; } = MemberBelongsTo.None;

		public abstract bool CanHaveChildMembers();
		public abstract (Type ChildType, Type PossibleCollectionType) GetChildTypes();
		public abstract string GetFullPath();
		public abstract string GetFormattedTextLabel();
		public bool ShowIndex { get; internal set; } = true;

		public class MacroTagTypes
		{
			public static MacroTagTypes Document => new MacroTagTypes(MacroConstants.DefaultMacroOpeningBracket, MacroConstants.DefaultMacroClosingBracket);
			public static MacroTagTypes Email => new MacroTagTypes(Core.Constants.DocumentEngine.EmailParsing.StartTag, Core.Constants.DocumentEngine.EmailParsing.EndTag);
			public static MacroTagTypes None => new MacroTagTypes(string.Empty, string.Empty);

			public MacroTagTypes(string opening, string closing)
			{
				Opening = opening;
				Closing = closing;
			}

			public string Opening { get; set; }
			public string Closing { get; set; }
		}

		public virtual string GetControllerInformation()
		{
			return "";
		}

		public string GetMacro()
			=> MacroTagType.Opening + Filter.NamespacePrefix + GetFullPath() + MacroTagType.Closing;

		public bool UsePreviewText;

		public string GetMemberInformation()
		{
			string result = GetFormattedTextLabel() + "\r\n\r\n" + (UsePreviewText ? Res.GetString("1aaa9770-6464-43ab-a43d-99133d633834", @"Preview:") + "\r\n" : "");
			string controllerInformation = GetControllerInformation();
			if (!string.IsNullOrEmpty(controllerInformation))
			{
				result += controllerInformation + "\r\n";
			}

			result += GetMacro();
			if (!string.IsNullOrEmpty(HelpText))
			{
				result += "\r\n\r\n" + HelpText;
			}
			return result;
		}

		public Type GetCollectionChildType(Type collectionType)
		{
			if (collectionType.IsArray)
			{
				return collectionType.GetElementType();
			}

			var collectionIndexer = GetIndexerForType(collectionType, throwError: false);
			if (collectionIndexer != null)
			{
				return collectionIndexer.PropertyType;
			}

			var genericIEnumerable = collectionType.GetInterfaces().Concat(new[] { collectionType }).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (genericIEnumerable != null)
			{
				return genericIEnumerable.GetGenericArguments()[0];
			}

			return typeof(object);
		}

		protected PropertyInfo GetIndexerForType(Type type, bool throwError = true)
		{
			var result = type.GetProperties().FirstOrDefault(property => property.GetIndexParameters().Length > 0);
			if (result == null && throwError)
			{
				throw new InvalidOperationException("Could not find indexer for type " + type.Name);
			}

			return result;
		}

		public virtual DocDataReflector DocDataReflector
		{
			get
			{
				if (docDataReflector == null)
				{
					docDataReflector = new DocDataReflector(this);
				}
				return docDataReflector;
			}
		}
		DocDataReflector docDataReflector;
	}

	#endregion

	public enum MemberBelongsTo
	{
		Collection, Element, None
	}
}
