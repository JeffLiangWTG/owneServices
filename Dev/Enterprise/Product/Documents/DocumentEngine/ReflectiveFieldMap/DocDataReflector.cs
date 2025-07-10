using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	/// <summary>
	/// Builds a list of MemberDescription's (properties and methods) of a given type.
	/// Can in turn be constructed on each MemberDescription to list the members of its return type.
	/// Allows building a tree of all the values in the type and its containing types, recursively.
	/// </summary>
	public class DocDataReflector : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocDataReflector(MemberDescription memberDescription)
			: this(memberDescription.GetChildTypes(), memberDescription.Filter, memberDescription.MacroTagType)
		{
			this.parentMember = memberDescription;
			this.ShowIndex = memberDescription.ShowIndex;
		}

		protected DocDataReflector(
			(Type ChildType, Type PossibleCollectionType) types,
			IDataReflectorFilter filter,
			MemberDescription.MacroTagTypes macroTags)
		{
			this.DocDataProviderType = types.ChildType;
			this.PossibleCollectionType = types.PossibleCollectionType;
			this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
			this.macroTags = macroTags;
		}

		internal readonly Type PossibleCollectionType;
		protected readonly MemberDescription parentMember;
		protected readonly MemberDescription.MacroTagTypes macroTags;
		protected readonly IDataReflectorFilter filter;
		public readonly Type DocDataProviderType;

		public bool ShowIndex { get; set; } = true;
		public int DefaultIndex { get; set; } = 1;

		public List<MemberDescription> Members
		{
			get { return members ?? (members = LoadMembers()); }
		}
		List<MemberDescription> members;

		List<MemberDescription> LoadMembers()
		{
			List<MemberDescription> result = new List<MemberDescription>();

			var hasPossibleCollectionType = PossibleCollectionType != null;
			LoadMembersByType(DocDataProviderType, hasPossibleCollectionType ? MemberBelongsTo.Element : MemberBelongsTo.None);
			LoadMembersByType(PossibleCollectionType, hasPossibleCollectionType ? MemberBelongsTo.Collection : MemberBelongsTo.None);

			return result;

			void LoadMembersByType(Type type, MemberBelongsTo memberBelongsTo)
			{
				if (type != null)
				{
					var propertyList = new List<PropertyDescription>();
					foreach (PropertyInfo property in GetPublicProperties(type))
					{
						if (filter.IsAllowed(property))
						{
							propertyList.Add(CreatePropertyDescription(property, memberBelongsTo));
						}
					}
					propertyList.Sort(ComparePropertyDescriptions);

					var methodList = new List<MethodDescription>();
					foreach (var method in type.GetMethods())
					{
						if (filter.IsAllowed(method))
						{
							methodList.Add(new MethodDescription(method, parentMember, GetHelpText(method), macroTags, filter));
						}
					}
					methodList.Sort(CompareMethodDescriptions);

					result.AddRange(propertyList);
					result.AddRange(methodList);
				}
			}
		}

		protected virtual PropertyDescription CreatePropertyDescription(PropertyInfo property, MemberBelongsTo memberBelongsTo)
		{
			return new PropertyDescription(property, parentMember, GetHelpText(property), macroTags, filter, ShowIndex, DefaultIndex) { MemberBelongsTo = memberBelongsTo };
		}

		public static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
		{
			return !type.IsInterface
				? type.GetProperties()
				: (new Type[] { type })
						 .Concat(type.GetInterfaces())
						 .SelectMany(i => i.GetProperties());
		}

		#region Sorting

		int ComparePropertyDescriptions(PropertyDescription x, PropertyDescription y)
		{
			string xValue = GetTypeScore(x.Property.PropertyType) + x.Property.Name;
			string yValue = GetTypeScore(y.Property.PropertyType) + y.Property.Name;

			return xValue.CompareTo(yValue);
		}

		int CompareMethodDescriptions(MethodDescription x, MethodDescription y)
		{
			string xValue = GetTypeScore(x.Method.ReturnType) + x.MethodNameWithParameters;
			string yValue = GetTypeScore(y.Method.ReturnType) + y.MethodNameWithParameters;

			return xValue.CompareTo(yValue);
		}

		int GetTypeScore(Type type)
		{
			if (filter.IsCollection(type))
			{
				return 3;
			}
			if (filter.IsRelatedObject(type))
			{
				return 1;
			}
			return 2;
		}

		#endregion

		protected string GetHelpText(MemberInfo info)
		{
			string result = "";
			DocumentFieldAttribute[] emailAttribute = (DocumentFieldAttribute[])info.GetCustomAttributes(typeof(DocumentFieldAttribute), false);
			if (emailAttribute.Length != 0)
			{
				result = emailAttribute[0].UserVisibleDescription;
			}
			return result;
		}
	}
}
