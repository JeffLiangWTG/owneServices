using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionBoolList Interface

	public interface ICodeDescriptionBoolList : ICodeDescriptionPairList
	{
		new ICodeDescriptionBool this[int i] { get; }
		bool GetBoolFromCode(string code);
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolCollection : CodeDescriptionCollection<CodeDescriptionBool, ZBool>, ICodeDescriptionBoolList
	{
		public CodeDescriptionBoolCollection()
			: this((FallbackLevel)null)
		{
		}

		public CodeDescriptionBoolCollection(FallbackLevel fallbackLevel)
			: this(fallbackLevel, null, false, 0)
		{
		}

		public CodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list)
			: this(null, list, false, 0)
		{
		}

		public CodeDescriptionBoolCollection(int codeMaxLength)
			: this(null, null, true, codeMaxLength)
		{
		}

		public CodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild)
			: this(null, list, defaultBoolForNewChild, 0)
		{
		}

		public CodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list, int codeMaxLength)
			: this(null, list, false, codeMaxLength)
		{
		}

		public CodeDescriptionBoolCollection(CodeDescriptionBoolCollection list)
			: this(null, null, list.DefaultValueForNewChild, list.CodeMaxLength)
		{
			foreach (CodeDescriptionBool item in list)
			{
				Add(item.Code, item.Description, item.Bool);
			}
		}

		protected CodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild, int codeMaxLength)
			: this(null, list, defaultBoolForNewChild, codeMaxLength)
		{
		}

		protected CodeDescriptionBoolCollection(FallbackLevel fallbackLevel, ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild, int codeMaxLength)
			: base(fallbackLevel, null, list, defaultBoolForNewChild, codeMaxLength)
		{
		}

		public CodeDescriptionBool Add(ZString code, MultilingualString description, bool value)
		{
			CodeDescriptionBool result = AddNew();
			result.Code = code;
			result.Description = description;
			result.Bool = value;

			return result;
		}

		public CodeDescriptionBool Add(int codeMaxLength, ZString code, MultilingualString description, bool value)
		{
			CodeDescriptionBool result = AddNew();
			result.CodeMaxLength = codeMaxLength;
			result.Code = code;
			result.Description = description;
			result.Bool = value;
			return result;
		}

		public CodeDescriptionBool Add(ZString code)
		{
			return Add(code, null, true);
		}

		public CodeDescriptionBool Add(ZString code, MultilingualString description)
		{
			return Add(code, description, true);
		}

		public CodeDescriptionBool AddSystemDefined(ZString code, MultilingualString description, bool booleanValue)
		{
			var result = AddNew();
			result.SystemDefined = true;
			result.CodeMaxLength = CodeMaxLength;
			result.Code = code;
			result.Description = description;
			result.Bool = booleanValue;
			return result;
		}

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CodeDescriptionBoolCollection clone = GetNewCollection();
			clone.CodeMaxLength = CodeMaxLength;
			clone.DefaultValueForNewChild = DefaultValueForNewChild;
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		protected virtual CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolCollection(CurrentFallbackLevel);
		}

		#endregion

		internal protected bool DefaultBoolForNewChild => DefaultValueForNewChild;

		public CodeDescriptionPairList GetActiveCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (ICodeDescriptionBool element in this)
			{
				if (element.Bool)
				{
					result.AddPair(element.Code, element.Description);
				}
			}
			return result;
		}

		#region ICodeDescriptionBoolList Members

		public bool GetBoolFromCode(string code)
		{
			CodeDescriptionBool element = (CodeDescriptionBool)FindByCode(code);
			return (element == null) ? ZBool.False : element.Bool;
		}

		ICodeDescriptionBool ICodeDescriptionBoolList.this[int i]
		{
			get { return this[i]; }
		}

		#endregion
	}
}
