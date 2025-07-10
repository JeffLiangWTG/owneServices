using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class BusinessObjectFilterValueProvider : IFilterValueProvider
	{
		public BusinessObjectFilterValueProvider(Type rootType)
		{
			this.rootType = rootType;
			this.constraints = new Dictionary<string, IFilterConstraint>();
			this.generator = new OperationalActionFieldGenerator();
		}

		public BusinessObject Source { get; set; }

		#region IFilterValueProvider Members

		public IFilterConstraint GetConstraint(string constraint)
		{
			IFilterConstraint result;

			if (!this.constraints.TryGetValue(constraint, out result))
			{
				result = GetNewConstraint(constraint);
				constraints[constraint] = result;
			}

			return result;
		}

		#endregion

		#region Implementation

		IFilterConstraint GetNewConstraint(string constraint)
		{
			PropertyInfo[] path = ReflectionHelper.FieldTextToPath(rootType, constraint);
			OperationalActionFieldSupporter supporter;

			if (path == null || path.Length == 0 || (supporter = generator.CreateField(path)) == null)
			{
				return null;
			}
			else
			{
				return new Constraint(this, path, supporter);
			}
		}

		#endregion

		sealed class Constraint : IFilterConstraint
		{
			public Constraint(BusinessObjectFilterValueProvider provider, PropertyInfo[] infoChain, OperationalActionFieldSupporter supporter)
			{
				this.provider = provider;
				this.infoChain = infoChain;
				this.supporter = supporter;
			}

			#region IFilterConstraint Members

			public string Name
			{
				get { return supporter.Field; }
			}

			public string SingularValueName
			{
				get { return string.Empty; }
			}

			public string PluralValueName
			{
				get { return string.Empty; }
			}

			public string Description
			{
				get { return string.Empty; }
			}

			public object GetValue()
			{
				if (provider.Source == null)
				{
					throw new InvalidOperationException("Source not set yet.");
				}

				object current = provider.Source;

				for (int i = 0; i < infoChain.Length; i++)
				{
					current = infoChain[i].GetValue(current, null);

					if (current is IBusinessObjectCollection)
					{
						return null;
					}
					else if (current == null)
					{
						return string.Empty;
					}
				}

				var currentIZType = current as IZType;

				if (currentIZType != null)
				{
					var stringValue = supporter.AsFilterString(currentIZType, provider.Source.Factory);
					Type type = currentIZType.BaseDataType;
					try
					{
						return stringValue != null ? Convert.ChangeType(stringValue, type, CultureInfo.CurrentCulture) : null;
					}
					catch (Exception ex)
					{
						if (ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentNullException)
						{
							return stringValue;
						}
						throw;
					}
				}
				else
				{
					return null;
				}
			}

			public string GetDefaultStringValue()
			{
				if (provider.Source == null)
				{
					throw new InvalidOperationException("Source not set yet.");
				}

				object current = provider.Source;

				for (int i = 0; i < infoChain.Length; i++)
				{
					current = infoChain[i].GetValue(current, null);

					if (current is IBusinessObjectCollection)
					{
						return null;
					}
					else if (current == null)
					{
						return string.Empty;
					}
				}

				var currentIZType = current as IZType;
				if (currentIZType != null)
				{
					return supporter.AsFilterString(currentIZType, provider.Source.Factory);
				}
				else
				{
					return null;
				}
			}

			#endregion

			readonly BusinessObjectFilterValueProvider provider;
			readonly PropertyInfo[] infoChain;
			readonly OperationalActionFieldSupporter supporter;
		}

		readonly Type rootType;
		readonly Dictionary<string, IFilterConstraint> constraints;
		readonly OperationalActionFieldGenerator generator;
	}
}
