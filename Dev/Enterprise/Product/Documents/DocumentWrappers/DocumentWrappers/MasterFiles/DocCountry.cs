using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocCountry : DocBaseWrapper
	{
		DocCountry(RefCountry refCountry, BusinessObjectFactory factoryForWrapper)
			: base(refCountry, factoryForWrapper)
		{
		}

		public static DocCountry New(BusinessObjectFactory factory, string countryCode)
		{
			return New(factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode), factory);
		}

		public static DocCountry New(RefCountry refCountry, BusinessObjectFactory factoryForWrapper)
		{
			if (refCountry == null)
			{
				return null;
			}
			else
			{
				return new DocCountry(refCountry, factoryForWrapper);
			}
		}

		public ZString Code
		{
			get { return RefCountry.RN_Code; }
		}

		public MultilingualString Name
		{
			get { return RefCountry.RN_DescMultilingual; }
		}

		public ZString CodeAndName
		{
			get { return RefCountry.RN_Code + " - " + RefCountry.RN_DescMultilingual; }
		}

		public ZBool IsActive
		{
			get { return RefCountry.RN_IsActive; }
		}

		public ZBool IsSystem
		{
			get { return RefCountry.RN_IsSystem; }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(RefCountry.LocalCurrency, Factory); }
		}

		public override string ToString()
		{
			return Name;
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		#region Implementation

		RefCountry RefCountry
		{
			get { return (RefCountry)WrappedObject; }
		}

		#endregion
	}
}
