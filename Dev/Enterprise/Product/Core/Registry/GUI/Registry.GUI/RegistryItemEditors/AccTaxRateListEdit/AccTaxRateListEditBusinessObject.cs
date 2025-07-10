using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	#region AccTaxRateListElement

	public class AccTaxRateListElement : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string TaxRate = "TaxRate";
			public const string TaxRateDescription = "TaxRateDescription";
		}

		#endregion

		public AccTaxRateListElement(ZGuid taxRatePK, AccTaxRateListCollection parent)
		{
			fTaxRate = taxRatePK;
			this.Parent = parent;
		}

		protected AccTaxRateListCollection Parent;

		#region Collections

		public AccTaxRateCollection AccTaxRateCollection
		{
			get { return Parent.AccTaxRateCollection; }
		}

		#endregion

		#region Properties

		#region Tax Rate

		protected ZGuid fTaxRate;
		public ZGuid TaxRate
		{
			get { return fTaxRate; }
			set
			{
				fTaxRate = value;
				if (!IsValidationSuspended)
				{
					ValidateTaxRate();
				}
				TaxRateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TaxRateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TaxRate); }
		}

		void ValidateTaxRate()
		{
			TaxRateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxRateInfo);
			TypeValidation.CheckValidGuid(TaxRateInfo);
			ListValidation.ErrorIfInvalidPK(TaxRateInfo, AccTaxRateCollection);
			if (!TaxRateInfo.HasErrors())
			{
				foreach (AccTaxRateListElement taxRateListElement in Parent)
				{
					if (taxRateListElement.PK != PK && taxRateListElement.TaxRate == TaxRate)
					{
						TaxRateInfo.AddError(Res.GetString("b6e04467-7a1b-46c3-819f-b0b1637f54d6", "Duplicate Tax IDs are not allowed."));
						break;
					}
				}
			}
		}

		#endregion

		#region Tax Rate Description

		public ZString TaxRateDescription
		{
			get
			{
				var taxRate = Parent.Factory.Load<AccTaxRate>(TaxRate);
				return taxRate != null ? taxRate.AT_Description : ZString.Empty;
			}
		}

		public ZPropertyInfo TaxRateDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.TaxRateDescription); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTaxRate();
		}

		#endregion
	}

	#endregion

	#region AccTaxRateListCollection

	public class AccTaxRateListCollection : NonPersistentBusinessObjectCollection<AccTaxRateListElement>
	{
		public AccTaxRateListCollection(string value, RegistryFindBoxFilter filter, BusinessObjectFactory factory, Guid companyPK) : base(factory)
		{
			this.Filter = filter;
			this.CompanyPK = companyPK;
			Load(value);
		}

		public void Load(string value)
		{
			string emptyGuidString = Guid.Empty.ToString();
			string[] sGuids = value.Split(',');

			foreach (string sGuid in sGuids)
			{
				try
				{
					if (!string.IsNullOrEmpty(sGuid) && sGuid != emptyGuidString)
					{
						ZGuid aGuid = new ZGuid(sGuid);
						if (aGuid.IsValid && !aGuid.IsEmpty)
						{
							Add(new AccTaxRateListElement(aGuid, this));
						}
					}
				}
				catch (ArgumentNullException)
				{
				}
				catch (FormatException)
				{
				}
			}
		}

		public override string ToString()
		{
			string result = "";
			foreach (AccTaxRateListElement elem in this)
			{
				if (elem.TaxRate.IsValid && !elem.TaxRate.IsEmpty)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ",";
					}

					result += elem.TaxRate.ToString();
				}
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AccTaxRateListElement(ZGuid.Empty, this);
		}

		public AccTaxRateCollection AccTaxRateCollection
		{
			get
			{
				if (fAccTaxRateCollection == null)
				{
					ZQuery filter = GetZQuery();
					//fAccTaxRateCollection = new AccTaxRateCollection(Factory, Filter, CompanyPK);
					fAccTaxRateCollection = new AccTaxRateCollection(Factory, filter, GetCountryCode(CompanyPK));
				}
				return fAccTaxRateCollection;
			}
		}

		ZString GetCountryCode(Guid companyPK)
		{
			GlbCompany company = Factory.Load<GlbCompany>(companyPK);
			return company != null ? company.GC_RN_NKCountryCode : ZString.Empty;
		}

		ZQuery GetZQuery()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JoinCondition.And, AccTaxRateSchema.AT_IsActive, true);
			return query;
		}

		protected AccTaxRateCollection fAccTaxRateCollection;
		internal readonly RegistryFindBoxFilter Filter;
		internal readonly Guid CompanyPK;
	}

	#endregion

	#region AccTaxRateListCollectionWrapper

	public class AccTaxRateListCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccTaxRateListCollectionWrapper(string value, RegistryFindBoxFilter filter, BusinessObjectFactory factory, Guid companyPK)
		{
			fAccTaxRateList = new AccTaxRateListCollection(value, filter, factory, companyPK);
			RegisterEditableChildObject(fAccTaxRateList);
		}

		public AccTaxRateListCollection AccTaxRateList
		{
			get { return fAccTaxRateList; }
		}

		readonly AccTaxRateListCollection fAccTaxRateList;
	}

	#endregion
}
