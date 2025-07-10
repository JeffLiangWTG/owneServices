using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CFIARegistrationNumber : CusCodeData,
		ISynchroniserReadOnlyMembersProvider
	{
		public CFIARegistrationNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CFIARegistrationNumberLookups Lookups
		{
			get { return (CFIARegistrationNumberLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CFIARegistrationNumberLookups(this);
		}

		public new CFIARegistrationNumberValidation Validation
		{
			get { return (CFIARegistrationNumberValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CFIARegistrationNumberValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CFIANumber;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot)); }
		}

		[List(nameof(Lookups) + "." + nameof(CFIARegistrationNumberLookups.CFIARegTypes))]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				var oldValue = base.CY_Code;
				if (oldValue != value)
				{
					base.CY_Code = value;
					if (Parent is JobComInvoiceLine && CY_Data.IsEmpty)
					{
						RegistrationNumberHelper.DefaultSafeFoodLicence(value, (x) => CY_Data = x, Lookups.CY_DataList);
					}
				}
			}
		}

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion
	}
}
