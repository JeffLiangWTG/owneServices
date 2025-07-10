using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class CountryAutoCompleteHelper : AutoCompleteHelper
	{
		public CountryAutoCompleteHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override SchemaColumn TextColumn
		{
			get { return RefCountrySchema.RN_Desc; }
		}

		protected override SchemaColumn KeyColumn
		{
			get { return RefCountrySchema.RN_Code; }
		}

		protected override Type BusinessObjectType
		{
			get { return typeof(RefCountry); }
		}
	}
}
