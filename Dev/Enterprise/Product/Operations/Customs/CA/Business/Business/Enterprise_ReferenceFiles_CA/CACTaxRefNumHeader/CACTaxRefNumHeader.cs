using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRefNumHeader : AutoCACTaxRefNumHeader
	{
		public CACTaxRefNumHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public static CACTaxRefNumHeader Load(CACClassHeader master, ZDateTime effectiveDate, bool fetchOnlyFromLocalCache = false)
		{
			var factory = master.Factory;
			return factory.GetCachedValue(string.Format("CACTaxRefNumHeader{0}_{1}", effectiveDate.ToShortDateString(), master.PK), () =>
			{
				var query = new ZQuery(CACTaxRefNumHeaderSchema.ZD_EffectiveDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				query.AddToFilter(CACTaxRefNumHeaderSchema.ZD_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
				query.AddToFilter(CACTaxRefNumHeaderSchema.ZD_Inactive, SQLComparisonOperator.NotEqual, "Y");
				query.AddToFilter(CACTaxRefNumHeaderSchema.ZD_ZA_ClassNumber, master.PK);
				query.OrderBy = CACTaxRefNumHeaderSchema.ZD_EffectiveDate.Name + " DESC";
				query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;
				return factory.LoadTop1<CACTaxRefNumHeader>(query);
			});
		}

		#endregion

		#region New Properties

		public bool IsCigars
		{
			get
			{
				if (isCigarsCached == null)
				{
					isCigarsCached = new CachedProperty<bool>(Factory, () =>
					{
						return RefNumbers.OfType<CACTaxRefNumber>().Any(x => x.ZE_ExciseTaxRefNumber == DutyAndTaxManager.CigarRateCode);
					});
				}
				return isCigarsCached.Value;
			}
		}
		CachedProperty<bool> isCigarsCached;

		#endregion

		#region ClassHeader

		[RelatedBusinessObject("ClassHeader")]
		public override ZGuid ZD_ZA_ClassNumber
		{
			get { return base.ZD_ZA_ClassNumber; }
			set { base.ZD_ZA_ClassNumber = value; }
		}

		public CACClassHeader ClassHeader
		{
			get { return Factory.Load<CACClassHeader>(ZD_ZA_ClassNumber); }
		}

		#endregion

		#region RefNumbers

		[ChildEditable(true)]
		public CACTaxRefNumberCollection RefNumbers
		{
			get
			{
				if (refNumbers == null)
				{
					refNumbers = new CACTaxRefNumberCollection(this);
					refNumbers.Load();
					RegisterEditableChildObject(refNumbers);
				}
				return refNumbers;
			}
		}

		CACTaxRefNumberCollection refNumbers;

		#endregion

		#region Delete

		public override void Delete()
		{
			RefNumbers.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion
	}
}
