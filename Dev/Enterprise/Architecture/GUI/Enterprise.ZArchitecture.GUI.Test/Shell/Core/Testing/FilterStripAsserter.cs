using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public sealed class FilterStripAsserter<T>
			where T : BusinessObject
	{
		public FilterStripAsserter(BusinessObjectFactory factory, Converter<T, string> converter)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (converter == null)
			{
				throw new ArgumentNullException(nameof(converter));
			}

			var tableName = BusinessObjectFactory.GetTableNameFromType(typeof(T));

			this.scope = new List<T>();
			this.fieldsOfInterest = new List<Column>();
			this.schema = EnterpriseSchema.GetTableSchema(tableName);
			this.converter = converter;
			this.factory = factory;
		}

		public void AddToScope(params T[] items)
		{
			scope.AddRange(items);
		}

		public void AssertMatches(string message, ModuleFilter filter, params T[] expected)
		{
			var query = filter.Query;

			if (!query.IsEmpty)
			{
				for (var sub = filter.SubGroup; sub != null; sub = sub.Parent)
				{
					query = ((ModuleFilterSubGroup)sub).GetSubQuery(query);
				}
			}

			AssertMatches(message, query, expected);
		}

		public void AssertMatches(string message, ZQuery filter, params T[] expected)
		{
			var combinedMessageText = new StringBuilder();
			combinedMessageText.Append(Assertion.Html(message));
			combinedMessageText.Append("<pre>");
			combinedMessageText.Append(Assertion.Html(GetQueryString(filter)));
			combinedMessageText.Append("</pre>");

			var globalFilter = new ZQuery(schema.PK, scope.ConvertAll((i) => i.PK));
			T[] actual;

			try
			{
				actual = factory.Load<T>(new ZQuery(globalFilter, filter));
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError(combinedMessageText.ToString(), ex);
			}

			if (fieldsOfInterest.Count > 0)
			{
				combinedMessageText.Append("<br />");
				AppendFieldsOfInterestString(combinedMessageText);
			}

			Assertion.HtmlAssertContainsExactElementsInAnyOrder<T>(
				combinedMessageText.ToString(),
				BusinessObjectEqualityComparer<T>.InstanceComparer,
				converter,
				expected,
				actual);
		}

		public void AddFieldOfInterest(string fieldOfInterest)
		{
			if (fieldOfInterest == null)
			{
				throw new ArgumentNullException(nameof(fieldOfInterest));
			}

			if (string.IsNullOrEmpty(fieldOfInterest))
			{
				throw new ArgumentException("fieldOfInterest cannot be empty", nameof(fieldOfInterest));
			}

			var fields = fieldOfInterest.Split(new char[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);

			this.fieldsOfInterest.Add(new Column()
			{
				Caption = fieldOfInterest,
				ValueGetter = delegate(T item)
				{
					object o = item;

					for (var i = 0; i < fields.Length; i++)
					{
						if (o == null)
						{
							return null;
						}

						o = o.GetType().InvokeMember(fields[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
					}

					return o == null ? null : o.ToString();
				}
			});
		}

		public void AddFieldOfInterest(string caption, Converter<T, string> valueGetter)
		{
			if (caption == null)
			{
				throw new ArgumentNullException(nameof(caption));
			}

			if (string.IsNullOrEmpty(caption))
			{
				throw new ArgumentException("caption cannot be empty", nameof(caption));
			}

			if (valueGetter == null)
			{
				throw new ArgumentNullException(nameof(valueGetter));
			}

			this.fieldsOfInterest.Add(new Column() { Caption = caption, ValueGetter = valueGetter });
		}

		string GetQueryString(ZQuery filter)
		{
			var builder = new StringBuilder();

			var compoundFilter = filter.ShallowClone();

			if (!compoundFilter.IgnoreActiveFilter)
			{
				compoundFilter.AddToFilter(BusinessObject.GetActiveFilter(typeof(T)));
			}

			builder.Append("\r\n\r\nSELECT *\r\nFROM ");
			builder.Append(schema.TableName);
			builder.Append("\r\n");

			if (!compoundFilter.IsEmpty)
			{
				builder.Append("WHERE\r\n");
				builder.Append(compoundFilter.LiteralTextSqlFormatted);
			}

			return builder.ToString();
		}

		void AppendFieldsOfInterestString(StringBuilder builder)
		{
			builder.Append("<table border=\"1\" cellspacing=\"0\"><tr><th>&nbsp</th>");

			foreach (var column in fieldsOfInterest)
			{
				builder.Append("<th>");
				builder.Append(Assertion.Html(column.Caption));
				builder.Append("</th>");
			}

			builder.Append("</tr>");

			foreach (var item in scope)
			{
				builder.Append("<tr><th>");
				builder.Append(Assertion.Html(converter(item)));
				builder.Append("</th>");

				foreach (var column in fieldsOfInterest)
				{
					var value = column.ValueGetter(item);

					if (value == null)
					{
						builder.Append("<td bgcolor=\"lightyellow\">&lt;null&gt;</td>");
					}
					else if (value.Length == 0)
					{
						builder.Append("<td>&nbsp;</td>");
					}
					else
					{
						builder.Append("<td>");
						builder.Append(Assertion.Html(value));
						builder.Append("</td>");
					}
				}

				builder.Append("</tr>");
			}

			builder.Append("</table>");
		}

		readonly List<T> scope;
		readonly List<Column> fieldsOfInterest;
		readonly ITableSchema schema;
		readonly BusinessObjectFactory factory;
		readonly Converter<T, string> converter;

		class Column
		{
			public string Caption { get; set; }
			public Converter<T, string> ValueGetter { get; set; }
		}
	}
}
