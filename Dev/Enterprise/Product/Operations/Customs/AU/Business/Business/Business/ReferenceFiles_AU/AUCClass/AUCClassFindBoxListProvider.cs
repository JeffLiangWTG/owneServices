using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// AUCClassFindBoxListProvider.
	/// </summary>
	public class AUCClassFindBoxListProvider : IFindBoxListProvider
	{
		public AUCClassFindBoxListProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		#region IFindBoxListProvider Members

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int counter)
		{
			return ((IFindBoxListProvider)this).NearestMatchCore(code, explicitAutoComplete);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var result = string.Empty;
			var success = false;
			if (!string.IsNullOrEmpty(code))
			{
				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(AUCClassSchema.UJ_Code, SQLComparisonOperator.StartsWith, code.Trim());
				sQLFilter.AddFilterAndZSQLParameterCollection("len(" + AUCClass.Schema.UJ_Code + ") = 13", null);
				sQLFilter.OrderBy = AUCClass.Schema.UJ_Code;

				var match = factory.LoadTop1<AUCClass>(sQLFilter);
				if (match != null)
				{
					result = match.UJ_Code;
					success = true;
				}
			}
			return (result, success);
		}

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			var match = AUCClass.GetClassForPartialCode(factory, code);
			return match?.UJ_Txt ?? ZString.Empty;
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get { throw new ApplicationException("IFindBoxListProvider.List should not be referred to"); }
		}

		ICodeDescription IFindBoxListProvider.GetCustomCodeDescription(BusinessObject bizo)
			=> bizo;

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pK)
		{
			throw new Exception("Unsupported.");
		}

		ZGuid IFindBoxListProvider.PrimaryKeyFromCode(string code)
		{
			throw new Exception("Unsupported.");
		}

		string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pK)
		{
			throw new Exception("Unsupported.");
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCode(string code)
		{
			throw new NotSupportedException();
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			throw new NotSupportedException();
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCode(string code)
		{
			throw new NotSupportedException();
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			throw new NotSupportedException();
		}

		bool IFindBoxListProvider.AutoCompleteOnCommit
		{
			get { return false; }
		}

		#endregion
	}
}
