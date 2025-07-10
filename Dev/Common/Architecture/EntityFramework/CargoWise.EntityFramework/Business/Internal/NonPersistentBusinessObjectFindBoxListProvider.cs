using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.Business.Internal
{
	public class NonPersistentBusinessObjectFindBoxListProvider : FindBoxListProvider
	{
		public NonPersistentBusinessObjectFindBoxListProvider(IBusinessObjectCollection collection) : base(collection)
		{
		}

		String CodePropertyName { get { return GetCodePropertyName(List.TypeOfElements); } }
		String DescriptionPropertyName { get { return GetDescriptionPropertyName(List.TypeOfElements); } }

		String NotImplementedErrorMessage
		{
			get
			{
				return (NoResString)@"The implemetation in base class (BusinessObjectCollection) does not work with non persistent business objects. Implement this method in your FindBoxListProvider class for collection of type: "
					+ List.GetType().ToString() + (NoResString)", or add attributes CodeProperty and DescriptionProperty to support generic overriden method in NonPersistentBusinessObjectFindBoxListProvider";
			}
		}

		public override string CodeFromDescription(string description)
		{
			try
			{
				foreach (BusinessObject element in List)
				{
					if (element[DescriptionPropertyName].ToString() == description)
					{
						return element[CodePropertyName].ToString();
					}
				}
				return string.Empty;
			}
			catch (NoCodePropertyException)
			{
				throw new NotImplementedException(NotImplementedErrorMessage);
			}
		}

		public override string NearestDescriptionMatch(string description, bool explicitAutoComplete)
		{
			try
			{
				foreach (BusinessObject element in List)
				{
					if (element[DescriptionPropertyName].ToString().StartsWith(description))
					{
						return element[DescriptionPropertyName].ToString();
					}
				}
				return string.Empty;
			}
			catch (NoCodePropertyException)
			{
				throw new NotImplementedException(NotImplementedErrorMessage);
			}
		}

		public override Types.ZGuid PrimaryKeyFromAlternateKey(string columnName, Types.IZType value)
		{
			throw new NotImplementedException(NotImplementedErrorMessage);
		}

		public override Types.IZType AlternateKeyFromPrimaryKey(string columnName, Types.ZGuid pk)
		{
			throw new NotImplementedException(NotImplementedErrorMessage);
		}

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			try
			{
				foreach (BusinessObject element in List)
				{
					if (element[CodePropertyName].ToString().StartsWith(code, StringComparison.Ordinal))
					{
						return (element[CodePropertyName].ToString(), true);
					}
				}
				return (string.Empty, false);
			}
			catch (NoCodePropertyException)
			{
				throw new NotImplementedException(NotImplementedErrorMessage);
			}
		}

		public override string DescriptionFromCode(string code)
		{
			try
			{
				foreach (BusinessObject element in List)
				{
					if (element[CodePropertyName].ToString() == code)
					{
						return element[DescriptionPropertyName].ToString();
					}
				}
				return string.Empty;
			}
			catch (NoCodePropertyException)
			{
				throw new NotImplementedException(NotImplementedErrorMessage);
			}
		}

		public override string DescriptionFromPrimaryKey(Types.ZGuid pK)
		{
			throw new NotImplementedException(NotImplementedErrorMessage);
		}

		public override Types.ZGuid PrimaryKeyFromCode(string code)
		{
			throw new NotImplementedException(NotImplementedErrorMessage);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			try
			{
				List<BusinessObject> bizObjs = new List<BusinessObject>();
				foreach (BusinessObject element in List)
				{
					if (element[CodePropertyName].ToString() == code)
					{
						bizObjs.Add(element);
					}
				}
				return bizObjs.Count > 0 ? bizObjs : Enumerable.Empty<BusinessObject>();
			}
			catch (NoCodePropertyException)
			{
				throw new NotImplementedException(NotImplementedErrorMessage);
			}
		}

		protected override IEnumerable<BusinessObject> GetBusinessObjectsFromCodeCore(string code)
		{
			try
			{
				List<BusinessObject> bizObjs = new List<BusinessObject>();
				foreach (BusinessObject element in List)
				{
					if (element[CodePropertyName].ToString() == code)
					{
						bizObjs.Add(element);
					}
				}
				return bizObjs.Count > 0 ? bizObjs : Enumerable.Empty<BusinessObject>();
			}
			catch (NoCodePropertyException)
			{
				throw new NotImplementedException(NotImplementedErrorMessage);
			}
		}

		public override string CodeFromPrimaryKey(Types.ZGuid pK)
		{
			throw new NotImplementedException(NotImplementedErrorMessage);
		}
	}
}
