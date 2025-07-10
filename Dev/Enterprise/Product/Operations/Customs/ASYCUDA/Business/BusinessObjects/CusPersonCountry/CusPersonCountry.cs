using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SingleObjectAroundARow]
	public class CusPersonCountry : Customs.Business.CusPersonCountry, Integration.Customs.ASYCUDA.ICusPersonCountry
	{
		public CusPersonCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusPersonCountryTypeDecider TypeDecider = new CusPersonCountryTypeDecider();

		[List(nameof(Lookups) + "." + nameof(CusPersonCountryLookups.DataTypes))]
		public override ZString CPC_Type
		{
			get { return base.CPC_Type; }
			set { base.CPC_Type = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusPersonCountryLookups.DataValues))]
		public override ZString CPC_Value
		{
			get { return base.CPC_Value; }
			set { base.CPC_Value = value; }
		}

		[RelatedBusinessObject(nameof(ParentPerson))]
		public override ZGuid CPC_CPN_Person
		{
			get { return base.CPC_CPN_Person; }
			set { base.CPC_CPN_Person = value; }
		}

		protected override Customs.Business.CusPersonCountryValidation GetNewValidation() => new CusPersonCountryValidation(this);

		public new CusPersonCountryValidation Validation => (CusPersonCountryValidation)base.Validation;

		protected override Customs.Business.CusPersonCountryLookups GetNewLookups() => new CusPersonCountryLookups(this);

		public new CusPersonCountryLookups Lookups => (CusPersonCountryLookups)base.Lookups;

		public CusPerson ParentPerson => Factory.Load<CusPerson>(CPC_CPN_Person);
	}
}
