using System;
using Enterprise.ZArchitecture.Schema;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	[Serializable]
	public class DataContextValue
	{
		internal DataContextValue(DataContext dataContext)
		{
			this.DataContext = dataContext;
			this.BusinessObjectDataContext = "";
		}

		internal DataContextValue(string fullDataContext, Type businessObjectType)
			: this(fullDataContext)
		{
			this.BusinessObjectType = businessObjectType;
		}

		public DataContextValue(string fullDataContext)
		{
			if (fullDataContext.StartsWith(".") && fullDataContext.Length <= StmTemplateSchema.SO_DataContext.MaxLength)
			{
				DataContext = DataContext.BusinessObject;
				BusinessObjectDataContext = fullDataContext;
			}
			else
			{
				if (Enum.TryParse(fullDataContext, true, out DataContext))
				{
					DataContext = (DataContext)Enum.Parse(typeof(DataContext), fullDataContext, true);
					BusinessObjectDataContext = "";
				}
				else
				{
					var message = Res.GetString("1A498F3F-CF18-4DE7-94AF-4F604A5A5EF0", "The {0} parameter in the #config of the template has been entered incorrectly. Please check the value and then try generating the document again.", "DataContext");
					throw new DataContextIsInvalidException(message);
				}
			}
		}

		public readonly DataContext DataContext;
		public readonly string BusinessObjectDataContext;
		public readonly Type BusinessObjectType;

		public static bool IsValidFullDataContext(string fullDataContext)
		{
			if (fullDataContext.StartsWith(".") && fullDataContext.Length <= StmTemplateSchema.SO_DataContext.MaxLength)
			{
				return true;
			}
			else
			{
				return Enum.IsDefined(typeof(DataContext), fullDataContext);
			}
		}

		public static DataContextValue None
		{
			get { return fNone ?? (fNone = new DataContextValue(DataContext.None)); }
		}
		[ThreadStatic]
		static DataContextValue fNone;

		public string FullDataContext
		{
			get { return IsBusinessObjectType ? BusinessObjectDataContext : DataContext.ToString(); }
		}

		public bool IsBusinessObjectType
		{
			get { return (DataContext == DataContext.BusinessObject); }
		}

		public bool WantsBusinessObjectOfType(Type businessObjectType)
		{
			return IsBusinessObjectType && businessObjectType.ToString().EndsWith(BusinessObjectDataContext);
		}

		public override string ToString()
		{
			return FullDataContext;
		}

		public override bool Equals(object operand)
		{
			DataContextValue dataContextValue = operand as DataContextValue;
			if (dataContextValue == null && operand is DataContext)
			{
				dataContextValue = new DataContextValue((DataContext)operand);
			}
			if (dataContextValue != null)
			{
				return (dataContextValue.FullDataContext == FullDataContext);
			}

			return base.Equals(operand);
		}

		public override int GetHashCode()
		{
			return FullDataContext.GetHashCode();
		}
	}
}
