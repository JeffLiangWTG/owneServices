using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// Used by the operational actions system to determine if it can follow a property when looking for fields that can be bulk updated.
	/// </summary>
	/// <remarks>
	/// See: https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/CorrectingTheFieldDetection.aspx
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Assembly)]
	public sealed class ActionFieldFollowAttribute : Attribute
	{
		public ActionFieldFollowAttribute()
			: this(true) { }

		public ActionFieldFollowAttribute(bool follow)
		{
			this.Follow = follow;
			this.TypeOverride = null;
		}

		public ActionFieldFollowAttribute(Type typeOverride)
		{
			this.Follow = true;
			this.TypeOverride = typeOverride;
		}

		public bool Follow { get; private set; }

		public Type TypeOverride { get; private set; }

		/// <summary>
		/// Determine if a property can be followed by looking for the relivant ActionFieldFollowAttribute's and taking into account any fallbacks.
		/// </summary>
		/// <param name="info">The property to check</param>
		/// <returns>True if the property can be followed, false otherwise.</returns>
		public static bool ShouldFollow(PropertyInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			ActionFieldFollowAttribute att = GetAttribute(info);
			return att == null || att.Follow;
		}

		/// <summary>
		/// Get the return type of the property, taking into account any overrides applied to the property itself.
		/// </summary>
		/// <param name="info">The property to check</param>
		/// <returns>The return type of the property.</returns>
		public static Type GetReturnType(PropertyInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			ActionFieldFollowAttribute attribute;

			if ((attribute = Get(info)) == null || attribute.TypeOverride == null)
			{
				return info.PropertyType;
			}
			else
			{
				// The assembly applying the attribute should be checked for rights to the type, not the calling assembly.
#pragma warning disable 612
				return ObjectFactory.GetType(attribute.TypeOverride, info.DeclaringType.Assembly);
#pragma warning restore 612
			}
		}

		static ActionFieldFollowAttribute Get(PropertyInfo info)
		{
			bool isOverriddenIndexProperty = false;

			if (info != null)
			{
				ParameterInfo[] indexParameters = info.GetIndexParameters();
				if ((indexParameters != null) && (indexParameters.GetLength(0) > 0))
				{
					var getMethod = info.GetGetMethod(true);
					isOverriddenIndexProperty = (getMethod != null) && (getMethod != getMethod.GetBaseDefinition());
				}
			}

			return isOverriddenIndexProperty ? null : (ActionFieldFollowAttribute)Attribute.GetCustomAttribute(info, typeof(ActionFieldFollowAttribute));
		}

		static ActionFieldFollowAttribute Get(Type type)
		{
			return (ActionFieldFollowAttribute)Attribute.GetCustomAttribute(type, typeof(ActionFieldFollowAttribute));
		}

		static ActionFieldFollowAttribute Get(Assembly info)
		{
			return
				info == typeof(BusinessObject).Assembly ?
				new ActionFieldFollowAttribute(false) :
				(ActionFieldFollowAttribute)Attribute.GetCustomAttribute(info, typeof(ActionFieldFollowAttribute));
		}

		static ActionFieldFollowAttribute GetAttribute(PropertyInfo info)
		{
			ActionFieldFollowAttribute result = Get(info) ?? Get(info.PropertyType) ?? Get(info.PropertyType.Assembly);

			if (result == null && typeof(IBusinessObjectCollection).IsAssignableFrom(info.PropertyType))
			{
				Type elementType = ListUtil.GetListElementType(info.PropertyType);
				if (elementType != null)
				{
					result = Get(elementType) ?? Get(elementType.Assembly);
				}
			}

			return result;
		}
	}
}
