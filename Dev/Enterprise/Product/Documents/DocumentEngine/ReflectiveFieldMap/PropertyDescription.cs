using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Application.Exceptions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	#region SuppressResourceStringsCheckRegion

	public class PropertyDescription : MemberDescription
	{
#if DEBUG
		/// <summary>
		/// Only for tests
		/// </summary>
		public PropertyDescription(PropertyInfo property, MemberDescription parentMemberDescription)
			: this(property, parentMemberDescription, "", MacroTagTypes.Document, new DocDataReflectorFilter())
		{
		}
#endif

		public PropertyDescription(
			PropertyInfo property,
			MemberDescription parentMemberDescription,
			string helpText,
			MacroTagTypes macroTagType,
			IDataReflectorFilter filter,
			bool showIndex = true,
			int defaultIndex = 1)
			: base(parentMemberDescription, helpText, macroTagType, filter, showIndex)
		{
			if (property == null)
			{
				throw new ArgumentNullException("PropertyInfo property");
			}

			this.Property = property;
			this.Index = $"[{defaultIndex}]";
		}

		public readonly PropertyInfo Property;
		readonly string Index;

		public override string GetFullPath()
		{
			if (ParentMemberDescription != null)
			{
				var parentFullPath = ParentMemberDescription.GetFullPath();

				if (MemberBelongsTo == MemberBelongsTo.Collection && parentFullPath.EndsWith(Index, StringComparison.OrdinalIgnoreCase))
				{
					parentFullPath = parentFullPath.Remove(parentFullPath.Length - 3);
				}

				return parentFullPath + "." + FullPath;
			}

			return FullPath;
		}

		string FullPath
		{
			get
			{
				var propertyType = Property.PropertyType;
				return FormattableString.Invariant($"{Property.Name}{(Filter.IsCollection(propertyType) && this.ShowIndex ? Index : "")}");
			}
		}

		public override bool CanHaveChildMembers() => Filter.CanHaveChildMembers(Property.PropertyType);

		public override (Type ChildType, Type PossibleCollectionType) GetChildTypes()
		{
			var propertyType = Property.PropertyType;

			if (Filter.IsCollection(propertyType))
			{
				return (GetCollectionChildType(propertyType), Filter.IsRelatedObject(propertyType) ? propertyType : null);
			}

			if (Filter.IsRelatedObject(propertyType))
			{
				if (propertyType.IsInterface
					&& Attribute.IsDefined(Property, typeof(ResolveTypeFromObjectFactoryForDocDataAttribute), false))
				{
					try
					{
						return (ObjectFactory.GetType(propertyType), null);
					}
					catch (NoSuchObjectDefinitionException)
					{
					}
				}

				return (propertyType, null);
			}

			throw new InvalidOperationException("You should never call GetChildType(PropertyInfo property) unless CanHaveChildMembers(PropertyInfo property) is true.");
		}

		public override string GetFormattedTextLabel()
		{
			var type = Property.PropertyType;
			type = Nullable.GetUnderlyingType(type) ?? type;
			return FormattableString.Invariant($"{Property.Name} ({type.Name.Split('`')[0]})");
		}

		public override string GetControllerInformation()
		{
			if (controllerInformation != null)
			{
				return controllerInformation;
			}

			if (typeof(IBusiness).IsAssignableFrom(Property.PropertyType))
			{
				var controller = ObjectFactory.Get<IControllerFactory>().GetControllerForType(Property.PropertyType);
				if (controller != null)
				{
					ModuleIdentifier moduleId = null;
					try
					{
						moduleId = controller.ModuleID;
					}
					catch (ModuleGuiNotSupportedException)
					{
					}

					controllerInformation = moduleId == null ? FormattableString.Invariant($"Controller ID: {controller.ID.Name}") : FormattableString.Invariant($"Module ID: {moduleId}\r\nController ID: {controller.ID.Name}");
				}
			}
			return controllerInformation;
		}

		internal string controllerInformation;
	}

	#endregion
}
