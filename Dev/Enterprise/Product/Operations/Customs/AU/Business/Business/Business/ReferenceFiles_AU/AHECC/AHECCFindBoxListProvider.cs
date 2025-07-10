using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AHECCFindBoxListProvider : IFindBoxListProvider
	{
		#region IFindBoxListProvider Members

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int counter)
		{
			return ((IFindBoxListProvider)this).NearestMatchCore(code, explicitAutoComplete);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			string result = string.Empty;
			var success = false;
			if (!string.IsNullOrEmpty(code))
			{
				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(AUCAHECCSchema.UA_AHECC, SQLComparisonOperator.StartsWith, code.Trim());
				sQLFilter.AddFilterAndZSQLParameterCollection("len(" + AUCAHECC.Schema.UA_AHECC + ") = 10", null);
				sQLFilter.OrderBy = AUCAHECC.Schema.UA_AHECC;

				BusinessObjectFactory factory = new BusinessObjectFactory();
				var match = factory.LoadTop1<AUCAHECC>(sQLFilter);
				if (match != null)
				{
					result = match.UA_AHECC;
					success = true;
				}
			}
			return (result, success);
		}

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

		bool IFindBoxListProvider.AutoCompleteOnCommit
		{
			get { return false; }
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

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(code))
			{
				var factory = new BusinessObjectFactory();
				var match = factory.LoadFromNaturalKey<AUCAHECC>(AUCAHECCSchema.UA_AHECC, code);
				if (match != null)
				{
					result = match.UA_LongDescription;
					if (result.IsEmpty)
					{
						result = match.UA_ShortDescription;
					}
				}
			}
			return result;
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get { throw new Exception("IFindBoxListProvider.List should not be referred to"); }
		}

		ICodeDescription IFindBoxListProvider.GetCustomCodeDescription(BusinessObject bizo) => bizo;

		#endregion
	}
}
