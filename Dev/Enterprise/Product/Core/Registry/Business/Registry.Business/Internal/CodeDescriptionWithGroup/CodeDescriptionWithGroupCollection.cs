using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionWithGroupList Interface

	public interface ICodeDescriptionWithGroupList : ICodeDescriptionPairList
	{
		new ICodeDescriptionWithGroup this[int i] { get; }
		string GetGroupFromCode(string code);
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionWithGroupCollection : RegistryBusinessObjectCollection, ICodeDescriptionWithGroupList
	{
		public CodeDescriptionWithGroupCollection()
		{
		}

		public CodeDescriptionWithGroupCollection(int codeMaxLength)
		{
			CodeMaxLength = codeMaxLength;
		}

		public CodeDescriptionWithGroupCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionWithGroupCollection(CodeDescriptionWithGroupCollection list)
			: this(list, list?.GroupLookup, list?.DefaultGroupForNewChild ?? string.Empty, list?.CodeMaxLength ?? 0)
		{
		}

		public CodeDescriptionWithGroupCollection(ReadOnlyCodeDescriptionPairList groupLookup, string defaultGroupForNewChild)
			: this(null, groupLookup, defaultGroupForNewChild, 0)
		{
		}

		public CodeDescriptionWithGroupCollection(ReadOnlyCodeDescriptionPairList groupLookup, string defaultGroupForNewChild, int codeMaxLength)
			: this(null, groupLookup, defaultGroupForNewChild, codeMaxLength)
		{
		}

		protected CodeDescriptionWithGroupCollection(CodeDescriptionWithGroupCollection list, ReadOnlyCodeDescriptionPairList groupLookup, string defaultGroupForNewChild, int codeMaxLength)
			: base(list?.CloneAsReadOnlyCodeDescriptionPairList(), codeMaxLength)
		{
			GroupLookup = groupLookup ?? new CodeDescriptionPairList();
			DefaultGroupForNewChild = defaultGroupForNewChild;
		}

		public new CodeDescriptionWithGroup this[int i]
		{
			get { return (CodeDescriptionWithGroup)base[i]; }
		}

		public CodeDescriptionWithGroup Add(ZString code, MultilingualString description)
		{
			return Add(code, description, DefaultGroupForNewChild);
		}

		public CodeDescriptionWithGroup Add(ZString code, MultilingualString description, ZString group)
		{
			CodeDescriptionWithGroup result = AddNew();
			result.Code = code;
			result.Description = description;
			result.Group = group;

			return result;
		}

		public CodeDescriptionWithGroup AddSystemDefined(ZString code, MultilingualString description, ZString group)
		{
			var result = AddNew();
			result.SystemDefined = true;
			result.CodeMaxLength = CodeMaxLength;
			result.Code = code;
			result.Description = description;
			result.Group = group;
			return result;
		}

		public new CodeDescriptionWithGroup AddNew()
		{
			return (CodeDescriptionWithGroup)base.AddNew();
		}

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionWithGroupCollection(null, GroupLookup, DefaultGroupForNewChild, CodeMaxLength);
			result.CurrentFallbackLevel = fallbackLevel;
			return result;
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionWithGroup();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CodeDescriptionWithGroup)child).Group = DefaultGroupForNewChild;
		}

		protected override void FillNewElementFromCodeDescriptionPairCore(ICodeDescription pair, RegistryBusinessObject child)
		{
			if (pair is CodeDescriptionWithGroup pairWithGroup
				&& child is CodeDescriptionWithGroup childWithGroup)
			{
				childWithGroup.Group = pairWithGroup.Group;
			}
		}

		#endregion

		#region GroupLookup

		internal ReadOnlyCodeDescriptionPairList GroupLookup { get; set; }

		internal ZString DefaultGroupForNewChild { get; set; }

		#endregion

		#region ICodeDescriptionGroupList Members

		public string GetGroupFromCode(string code)
		{
			CodeDescriptionWithGroup element = (CodeDescriptionWithGroup)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Group;
		}

		public string GetDescriptionFromCode(string code)
		{
			var element = (CodeDescriptionWithGroup)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Group;
		}

		ICodeDescriptionWithGroup ICodeDescriptionWithGroupList.this[int i] => this[i];

		#endregion

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return ContainsCode(code.ToString());
		}

		#endregion

		#region ToReadOnlyCodeDescriptionPairList

		public ReadOnlyCodeDescriptionPairList ToReadOnlyCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			foreach (var pair in this.Cast<CodeDescriptionWithGroup>())
			{
				result.AddPair(pair.Code, pair.Description);
			}
			return result;
		}

		#endregion

		#region CloneAsReadOnlyCodeDescriptionPairList()

		ReadOnlyCodeDescriptionPairList CloneAsReadOnlyCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			foreach (var pair in this.Cast<CodeDescriptionWithGroup>())
			{
				result.Add((ICodeDescription)pair.Clone(null, Factory));
			}
			return result;
		}

		#endregion
	}
}
