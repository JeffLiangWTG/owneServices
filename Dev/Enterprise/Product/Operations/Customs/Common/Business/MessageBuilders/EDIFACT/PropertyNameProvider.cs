using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.MessageInterpretation
{
	public static class PropertyNameProvider
	{
		#region GetColumnTitles

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static IEnumerable<string> GetColumnTitles<T>()
		{
			var type = typeof(T);
			if (!CachedTitles.ContainsKey(type))
			{
				CachedTitles[type] = GetColumnTitlesCore<T>();
			}
			return CachedTitles[type];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static IEnumerable<string> GetColumnTitlesCore<T>()
		{
			var properties = from propertyInfo in typeof(T).GetProperties()
							 from attribute in propertyInfo.GetCustomAttributes(true)
							 where attribute is ColumnNameAttribute
							 let columnNameAttribute = ((ColumnNameAttribute)attribute)
							 orderby columnNameAttribute.ColumnPosition
							 select new { Info = propertyInfo, NameOverride = columnNameAttribute.ColumnNameOverride };

			foreach (var property in properties)
			{
				var propertyInfo = property.Info;
				if (!property.NameOverride.IsEmpty)
				{
					yield return property.NameOverride;
				}
				else
				{
					yield return GetFriendlyPropertyOrFieldName(propertyInfo);
				}
			}
		}

		static Dictionary<Type, IEnumerable<string>> CachedTitles
		{
			get { return cachedTitles ?? (cachedTitles = new Dictionary<Type, IEnumerable<string>>()); }
		}

		[ThreadStatic]
		static Dictionary<Type, IEnumerable<string>> cachedTitles;

		#endregion

		#region GetFriendlyPropertyName

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static string GetFriendlyPropertyName<T>(Expression<Func<T>> property)
		{
			return GetFriendlyPropertyOrFieldName(GetFirstPropertyOrFieldInfo(property));
		}

		static MemberInfo GetFirstPropertyOrFieldInfo(Expression expression)
		{
			var memberExpression = expression as MemberExpression;
			if (memberExpression != null)
			{
				var memberInfo = memberExpression.Member;
				return (MemberInfo)(memberInfo as PropertyInfo) ?? memberInfo as FieldInfo;
			}

			PropertyInfo property = null;
			FieldInfo field = null;

			foreach (var info in expression.GetType().GetProperties())
			{
				var value = info.GetValue(expression, null);

				var valueExpression = value as Expression;
				if (valueExpression != null)
				{
					var memberInfo = GetFirstPropertyOrFieldInfo(valueExpression);
					property = memberInfo as PropertyInfo;
					if (field == null)
					{
						field = memberInfo as FieldInfo;
					}
				}
				else
				{
					var valueExpressionEnumerable = value as IEnumerable<Expression>;

					if (valueExpressionEnumerable != null)
					{
						foreach (var exp in valueExpressionEnumerable)
						{
							var memberInfo = GetFirstPropertyOrFieldInfo(exp);
							property = memberInfo as PropertyInfo;
							if (field == null)
							{
								field = memberInfo as FieldInfo;
							}

							if (property != null)
							{
								break;
							}
						}
					}
				}
				if (property != null)
				{
					break;
				}
			}
			return (MemberInfo)property ?? field;
		}

		internal static string GetFriendlyPropertyOrFieldName(MemberInfo memberInfo)
		{
			if (memberInfo == null)
			{
				throw new ArgumentException("Expression should contain property. e.g. () => obj.Property or () => obj.Property.Whatever()");
			}

			ZString columnName = memberInfo.Name;
			columnName = columnName.SubstringSafe(0, 1).ToUpper() + columnName.SubstringSafe(1);
			return ZPropertyInfo.GetFriendlyColumnNameShared(columnName);
		}

		#endregion
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class ColumnNameAttribute : Attribute
	{
		public ColumnNameAttribute(int columnPosition = 0, string columnNameOverride = "")
		{
			ColumnPosition = columnPosition;
			ColumnNameOverride = columnNameOverride;
		}

		internal int ColumnPosition { get; private set; }
		internal ZString ColumnNameOverride { get; private set; }
	}
}
