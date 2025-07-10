using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine
{
	#region Base

	public abstract class FormatStringFunction
	{
		public abstract Regex Regex { get; }

		public bool Match(string expression)
		{
			return MatchCore(expression);
		}

		protected virtual bool MatchCore(string expression)
		{
			return Regex.IsMatch(expression);
		}

		public IZType Execute(IList collectionScope)
		{
			return ExecuteCore(collectionScope);
		}

		protected abstract IZType ExecuteCore(IList collectionScope);
	}

	#endregion

	#region Count

	public class FormatStringCount : FormatStringFunction
	{
		public override Regex Regex
		{
			get { return regex ?? (regex = new Regex(@"^(Count|Count\(\))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex regex;

		protected override IZType ExecuteCore(IList collectionScope)
		{
			if (collectionScope != null)
			{
				return (ZInt)collectionScope.Count;
			}
			else
			{
				return ZString.Empty;
			}
		}
	}

	#endregion

	#region Sum

	public class FormatStringSum : FormatStringFunction
	{
		public override Regex Regex
		{
			get { return regex ?? (regex = new Regex(@"^Sum\((?<PropertyName>[\w\.]+)?\)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex regex;

		protected override bool MatchCore(string expression)
		{
			Match match = Regex.Match(expression);

			propertyName = match.Groups["PropertyName"].Value;

			return match.Success;
		}

		string propertyName;

		protected override IZType ExecuteCore(IList collectionScope)
		{
			if (collectionScope == null || string.IsNullOrEmpty(propertyName))
			{
				return ZString.Empty;
			}
			else
			{
				List<INumericZType> values = new List<INumericZType>();

				var reflector = new BusinessObjectReflector();
				var dataProvider = new BusinessObjectDataProvider();

				foreach (var elem in collectionScope)
				{
					var obj = BODocDataProvider.GetObject(elem);
					var methodInfoChain = !string.IsNullOrEmpty(propertyName) ? reflector.GetMethodInfoChain(obj.GetType(), obj, propertyName) : null;
					var data = methodInfoChain != null ? dataProvider.GetFieldValueFromMethodInfoChain(obj, methodInfoChain) as INumericZType : null;

					if (data != null)
					{
						values.Add(data);
					}
				}

				return Sum(values);
			}
		}

		IZType Sum(List<INumericZType> values)
		{
			if (values.Count == 0)
			{
				return ZString.Empty;
			}
			else
			{
				Type elementType = values[0].GetType();

				if (typeof(ZInt).IsAssignableFrom(elementType))
				{
					return Sum<ZInt>(values, (x, y) => x + y);
				}
				else if (typeof(ZShort).IsAssignableFrom(elementType))
				{
					return Sum<ZShort>(values, (x, y) => x + y);
				}
				else if (typeof(ZDecimal).IsAssignableFrom(elementType))
				{
					return Sum<ZDecimal>(values, (x, y) => x + y);
				}
				else if (typeof(ZLong).IsAssignableFrom(elementType))
				{
					return Sum<ZLong>(values, (x, y) => x + y);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		T Sum<T>(List<INumericZType> values, Func<T, T, T> adder) where T : INumericZType, new()
		{
			T sum = new T();

			return values.Aggregate(sum, (current, value) => adder(current, (T)value));
		}
	}

	#endregion
}
