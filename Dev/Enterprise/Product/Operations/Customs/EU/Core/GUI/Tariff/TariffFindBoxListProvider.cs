using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.GUI
{
	public class TariffFindBoxListProvider : IFindBoxListProvider
	{
		#region IFindBoxListProvider Members

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int cursor)
		{
			return ((IFindBoxListProvider)this).NearestMatchCore(code, explicitAutoComplete);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			return (TariffFormatter.Format(code), false);
		}

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			return string.Empty;
		}

		string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pK)
		{
			throw new NotSupportedException();
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get { throw new ApplicationException("IFindBoxListProvider.List should not be referred to"); }
		}

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pK)
		{
			throw new NotSupportedException();
		}

		ZGuid IFindBoxListProvider.PrimaryKeyFromCode(string code)
		{
			throw new NotSupportedException();
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

		protected TariffFormatter TariffFormatter
		{
			get { return fTariffFormatter ?? (fTariffFormatter = TariffFormatter.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
		}
		TariffFormatter fTariffFormatter;

		ICodeDescription IFindBoxListProvider.GetCustomCodeDescription(BusinessObject bizo)
			=> bizo;

		#endregion
	}
}
