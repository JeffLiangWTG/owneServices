using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.SADH
{
	public class SADHFormData : Customs.Business.SADH.SADHFormData, ICanBeImportOrExport
	{
		public SADHFormData(BusinessObjectFactory factory, JobDeclaration declaration)
			: base(factory, declaration)
		{
			Declaration = declaration;
		}
		public readonly JobDeclaration Declaration;

		#region Overridden Lookups and Validation
		public new SADHFormDataLookups Lookups
		{
			get { return (SADHFormDataLookups)base.Lookups; }
		}

		protected override Customs.Business.SADH.SADHFormDataLookups GetNewLookups()
		{
			return new SADHFormDataLookups(this);
		}

		public new SADHFormDataValidation Validation
		{
			get { return (SADHFormDataValidation)base.Validation; }
		}

		protected override Customs.Business.SADH.SADHFormDataValidation GetNewValidation()
		{
			return new SADHFormDataValidation(this);
		}
		#endregion

		#region ICanBeImportOrExport

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string ICanBeImportOrExport.TrueCountryCode => Declaration?.CountryCode;
		string ICanBeImportOrExport.DataGroupingCode => Declaration?.GetDefaultDataGroupingCode();
		#endregion
	}
}
