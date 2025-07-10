using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// When a business object includes a foreign key reference to a company, the
	/// DataWizard can use an implementation of this interface to determine how,
	/// or if at all, the DataWizard should filter that business object according to
	/// the current company.
	///
	/// Please register implementation of this interface in
	/// Dev/Enterprise/Architecture/Core/Core/Configuration/EnterpriseApplicationConfiguration.xml
	/// </summary>
	public interface ICompanyFilterProvider
	{
		/// <summary>
		/// Gets a filter for the given company PK for the bizo and a global filter.
		/// </summary>
		/// <returns>May return null to indicate no filter is required.</returns>
		List<ZQuery> GetCompanyFilters(ICompanyFilterProviderContext context, Guid? companyPK);
	}
}
