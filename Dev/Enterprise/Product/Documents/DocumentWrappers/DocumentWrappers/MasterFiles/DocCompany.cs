using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCompany : DocBaseWrapper
	{
		DocCompany(GlbCompany glbCompany, BusinessObjectFactory factoryForWrapper)
			: base(glbCompany, factoryForWrapper)
		{
		}

		public static DocCompany New(GlbCompany glbCompany, BusinessObjectFactory factoryForWrapper)
		{
			if (glbCompany == null)
			{
				return null;
			}
			else
			{
				return new DocCompany(glbCompany, factoryForWrapper);
			}
		}

		GlbCompany GlbCompany
		{
			get { return (GlbCompany)WrappedObject; }
		}

		public ZString Address1
		{
			get { return GlbCompany.GC_Address1; }
		}

		public ZString Address2
		{
			get { return GlbCompany.GC_Address2; }
		}

		public ZString BusinessRegNo
		{
			get { return GlbCompany.GC_BusinessRegNo; }
		}

		public ZString BusinessRegNo2
		{
			get { return GlbCompany.GC_BusinessRegNo2; }
		}

		public ZString City
		{
			get { return GlbCompany.GC_City; }
		}
		public ZString Code
		{
			get { return GlbCompany.GC_Code; }
		}

		public ZString CustomsRegistrationNo
		{
			get { return GlbCompany.GC_CustomsRegistrationNo; }
		}

		public ZString Email
		{
			get { return GlbCompany.GC_Email; }
		}

		public ZString Fax
		{
			get { return GlbCompany.GC_Fax_Formatted; }
		}

		public ZBool IsActive
		{
			get { return GlbCompany.GC_IsActive; }
		}

		public ZBool IsGSTCashBasis
		{
			get { return GlbCompany.GC_IsGSTCashBasis; }
		}

		public ZBool IsGSTRegistered
		{
			get { return GlbCompany.GC_IsGSTRegistered; }
		}

		public ZBool IsReciprocal
		{
			get { return GlbCompany.GC_IsReciprocal; }
		}

		public ZBool IsWHTCashBasis
		{
			get { return GlbCompany.GC_IsWHTCashBasis; }
		}

		public ZBool IsWHTRegistered
		{
			get { return GlbCompany.GC_IsWHTRegistered; }
		}

		public ZString Name
		{
			get { return GlbCompany.GC_Name; }
		}

		public ZInt NoOfAccountingPeriods
		{
			get { return GlbCompany.GC_NoOfAccountingPeriods; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(GlbCompany.OrgProxy, Factory); }
		}

		public ZString Phone
		{
			get { return GlbCompany.GC_Phone_Formatted; }
		}

		public ZString PostCode
		{
			get { return GlbCompany.GC_PostCode; }
		}

		public ZDateTime StartDate
		{
			get { return GlbCompany.GC_StartDate; }
		}

		public ZString State
		{
			get { return GlbCompany.GC_State; }
		}

		public ZString WebAddress
		{
			get { return GlbCompany.GC_WebAddress; }
		}

		public DocCountry Country
		{
			get { return DocCountry.New(GlbCompany.Country, Factory); }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(GlbCompany.LocalCurrency, Factory); }
		}

		public override string ToString()
		{
			return Name;
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		public ZInt ExchangeRateDecimalPlaces
		{
			get { return GlbCompany.ExchangeRateDecimalPlaces; }
		}
	}
}
