using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Res = Enterprise.Customs.Common.Res;

namespace Enterprise.Customs.Business.MessageInterpretation
{
	public class FieldValueTableInterpretation
	{
		public FieldValueTableInterpretation(bool vertical = true)
		{
			this.vertical = vertical;
			interpretations = new List<KeyValuePair<string, object>>();
		}

		public static string GetTableInterpretation<T>(T obj, string caption = "", bool vertical = true)
		{
			var result = new StringBuilder();
			var interpretation = new FieldValueTableInterpretation(vertical);
			var properties = from propertyInfo in typeof(T).GetProperties()
							 where typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType)
							 select propertyInfo;

			foreach (var property in properties)
			{
				var field = PropertyNameProvider.GetFriendlyPropertyOrFieldName(property);
				var value = (IZType)property.GetValue(obj, null);
				if (value is ZBool)
				{
					value = new ZString(TableInterpretation.GetBooleanAsString((ZBool)value));
				}

				interpretation.AddIfNotEmpty(field, value);
			}

			result.Append(interpretation.ToHtml());

			if (result.Length > 0 && !string.IsNullOrEmpty(caption))
			{
				result.Insert(0, new HtmlTableCreator(new[] { caption }, TableInterpretation.Attributes.FullWidth).ToHtml());
			}
			return result.ToString();
		}

		#region Add

		public void Add(string elementDescription, object elementValue)
		{
			interpretations.Add(new KeyValuePair<string, object>(elementDescription, elementValue));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void Add<T>(Expression<Func<T>> property)
		{
			Add(PropertyNameProvider.GetFriendlyPropertyName(property), property.Compile()());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void Add<T>(Expression<Func<T>> elementDescription, object elementValue)
		{
			Add(PropertyNameProvider.GetFriendlyPropertyName(elementDescription), elementValue);
		}

		#endregion

		#region AddIfNotEmpty

		public void AddIfNotEmpty(string elementDescription, IZType elementValue)
		{
			if (!elementValue.IsEmpty)
			{
				Add(elementDescription, elementValue);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddIfNotEmpty<T>(Expression<Func<T>> property) where T : IZType
		{
			AddIfNotEmpty(property, property.Compile()());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddIfNotEmpty<T>(Expression<Func<T>> elementDescription, IZType elementValue) where T : IZType
		{
			if (!elementValue.IsEmpty)
			{
				Add(PropertyNameProvider.GetFriendlyPropertyName(elementDescription), elementValue);
			}
		}

		#endregion

		public string ToHtml()
		{
			var result = string.Empty;
			if (interpretations.Any())
			{
				if (vertical)
				{
					var columnTitles = new[] { Res.GetString("5e89e0eb-0f46-40bb-ac66-7c2600bd2675", "Field"), Res.GetString("61685b62-578f-41a2-8a16-62269de5fe19", "Value") };
					var table = new HtmlTableCreator(columnTitles, TableInterpretation.Attributes.FullWidth);
					foreach (var interpretation in interpretations)
					{
						table.WriteRow(interpretation.Key, interpretation.Value.ToString());
					}
					result = table.ToHtml();
				}
				else
				{
					var table = new HtmlTableCreator(interpretations.Select(i => i.Key), TableInterpretation.Attributes.FullWidth);
					table.WriteRow(TableInterpretation.Attributes.AlignCenter, interpretations.Select(i => i.Value).ToArray());
					result = table.ToHtml();
				}
			}
			return result;
		}

		readonly bool vertical;
		readonly List<KeyValuePair<string, object>> interpretations;
	}
}
