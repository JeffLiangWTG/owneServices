using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class SITTCertificationNumber : CusCodeData,
		ISynchroniserReadOnlyMembersProvider
	{
		public SITTCertificationNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CusCodeDataLookups(this);
		}

		public new SITTCertificationNumberValidation Validation
		{
			get { return (SITTCertificationNumberValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new SITTCertificationNumberValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.SITTNumber;
			CY_Code = CusCodeDataTypeList.Codes.SITTNumber;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot)); }
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
