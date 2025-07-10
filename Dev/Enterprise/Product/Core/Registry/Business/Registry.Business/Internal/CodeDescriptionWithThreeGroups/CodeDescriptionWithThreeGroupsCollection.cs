using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionWithThreeGroupsList Interface

	public interface ICodeDescriptionWithThreeGroupsList : ICodeDescriptionWithGroupList
	{
		new ICodeDescriptionWithThreeGroups this[int i] { get; }
		Tuple<string, string, string> GetThreeGroupsFromCode(string code);
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionWithThreeGroupsCollection : CodeDescriptionWithGroupCollection, ICodeDescriptionWithThreeGroupsList
	{
		public CodeDescriptionWithThreeGroupsCollection()
		{
			CodeMaxLength = DefaultCodeMaxLength;
		}

		public CodeDescriptionWithThreeGroupsCollection(int codeMaxLength = DefaultCodeMaxLength)
		{
			CodeMaxLength = codeMaxLength;
		}

		public CodeDescriptionWithThreeGroupsCollection(CodeDescriptionWithThreeGroupsCollection list)
			: this(list, list?.GroupLookup, list?.Group2Lookup, list?.Group3Lookup, list?.CodeMaxLength ?? 0)
		{
		}

		public CodeDescriptionWithThreeGroupsCollection(ReadOnlyCodeDescriptionPairList groupLookup,
			ReadOnlyCodeDescriptionPairList group2Lookup,
			ReadOnlyCodeDescriptionPairList group3Lookup,
			int codeMaxLength)
			: this(null, groupLookup, group2Lookup, group3Lookup, codeMaxLength)
		{
		}

		CodeDescriptionWithThreeGroupsCollection(CodeDescriptionWithThreeGroupsCollection list,
			ReadOnlyCodeDescriptionPairList groupLookup,
			ReadOnlyCodeDescriptionPairList group2Lookup,
			ReadOnlyCodeDescriptionPairList group3Lookup,
			int codeMaxLength)
			: base(list?.CloneAsCodeDescriptionWithThreeGroupsCollection(), groupLookup, groupLookup?.DefaultCode, codeMaxLength)
		{
			Group2Lookup = group2Lookup ?? new CodeDescriptionPairList();
			Group3Lookup = group3Lookup ?? new CodeDescriptionPairList();
			CodeMaxLength = codeMaxLength;
		}

		public new CodeDescriptionWithThreeGroups this[int i]
		{
			get { return (CodeDescriptionWithThreeGroups)base[i]; }
		}

		public CodeDescriptionWithThreeGroups Add(ZString code)
		{
			return Add(code, GroupLookup?.DefaultCode, "", "");
		}

		public CodeDescriptionWithThreeGroups Add(ZString code, ZString group, ZString group2, ZString group3)
		{
			CodeDescriptionWithThreeGroups result = AddNew();
			result.Code = code;
			result.Group = group;
			result.Group2 = group2;
			result.Group3 = group3;

			return result;
		}

		public new CodeDescriptionWithThreeGroups AddNew()
		{
			return (CodeDescriptionWithThreeGroups)base.AddNew();
		}

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionWithThreeGroupsCollection(null, GroupLookup, Group2Lookup, Group3Lookup, CodeMaxLength);
			result.CurrentFallbackLevel = fallbackLevel;
			return result;
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionWithThreeGroups();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CodeDescriptionWithThreeGroups)child).Group = GroupLookup?.DefaultCode;
			((CodeDescriptionWithThreeGroups)child).Group2 = Group2Lookup?.DefaultCode;
			((CodeDescriptionWithThreeGroups)child).Group3 = Group3Lookup?.DefaultCode;
		}

		protected override void FillNewElementFromCodeDescriptionPairCore(ICodeDescription pair, RegistryBusinessObject child)
		{
			if (pair is CodeDescriptionWithThreeGroups pairWith3GroupExtraDescr
				&& child is CodeDescriptionWithThreeGroups childWith3GroupExtraDescr)
			{
				childWith3GroupExtraDescr.Group = pairWith3GroupExtraDescr.Group;
				childWith3GroupExtraDescr.Group2 = pairWith3GroupExtraDescr.Group2;
				childWith3GroupExtraDescr.Group3 = pairWith3GroupExtraDescr.Group3;
			}
		}

		#endregion

		#region GroupLookup

		internal ReadOnlyCodeDescriptionPairList Group2Lookup { get; set; }
		internal ReadOnlyCodeDescriptionPairList Group3Lookup { get; set; }

		#endregion

		#region ICodeDescriptionGroupList Members

		public Tuple<string, string, string> GetThreeGroupsFromCode(string code)
		{
			var group = GetGroupFromCode(code);
			var group2 = GetGroup2FromCode(code);
			var group3 = GetGroup3FromCode(code);
			return Tuple.Create(group, group2, group3);
		}

		public new string GetGroupFromCode(string code)
		{
			var element = (CodeDescriptionWithThreeGroups)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Group;
		}
		public string GetGroup2FromCode(string code)
		{
			var element = (CodeDescriptionWithThreeGroups)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Group2;
		}
		public string GetGroup3FromCode(string code)
		{
			var element = (CodeDescriptionWithThreeGroups)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Group3;
		}

		ICodeDescriptionWithThreeGroups ICodeDescriptionWithThreeGroupsList.this[int i] => this[i];

		#endregion

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return ContainsCode(code.ToString());
		}

		#endregion

		#region CloneAsCodeDescriptionWithGroupCollection()

		protected CodeDescriptionWithGroupCollection CloneAsCodeDescriptionWithGroupCollection()
		{
			var result = new CodeDescriptionWithGroupCollection(CodeMaxLength);
			foreach (var element in this.Cast<CodeDescriptionWithThreeGroups>())
			{
				var entry = new CodeDescriptionWithThreeGroups(element);
				result.Add(entry);
			}
			return result;
		}

		protected CodeDescriptionWithThreeGroupsCollection CloneAsCodeDescriptionWithThreeGroupsCollection()
		{
			var result = new CodeDescriptionWithThreeGroupsCollection(CodeMaxLength);
			foreach (var element in this.Cast<CodeDescriptionWithThreeGroups>())
			{
				var entry = new CodeDescriptionWithThreeGroups(element);
				result.Add(entry);
			}
			return result;
		}

		#endregion

		const int DefaultCodeMaxLength = 17;
	}
}
