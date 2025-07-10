using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataConverters.CustomsFiles
{
	public abstract class ClassificationWriter : DataWriter
	{
		public ClassificationWriter(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Public Fields to be filled in when importing

		public ZString LookupCode;
		public ZString TariffCode;
		public ZString Description;
		public ZString ClassificationType;

		#endregion

		#region RecordDescription
		public override ZString RecordDescription
		{
			get { return "Classification: " + LookupCode + " - " + Description; }
		}
		#endregion

		#region GetAnyReasonRecordShouldBeExcluded

		protected override internal ZString GetAnyReasonRecordShouldBeExcluded()
		{
			return LookupCode.IsEmpty ? new ZString("No Lookup Code On This Row") : ZString.Empty;
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return (BaseCusClassification)Factory.New(GetClassificationType());
		}

		#endregion

		#region GetExistingBusinessObject

		protected override BusinessObject GetExistingBusinessObject()
		{
			ZQuery filter = new ZQuery(CusClassificationSchema.CC_LookupCode, LookupCode);
			filter.AddToFilter(CusClassificationSchema.CC_ClassificationType, ClassificationType);//BaseCusClassification.ClassificationType.Both);
			filter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			return LoadExistingCusClassification(filter);
		}

		BaseCusClassification LoadExistingCusClassification(ZQuery filter)
		{
			return (BaseCusClassification)Factory.LoadTop1(GetClassificationType(), filter);
		}

		#endregion

		#region UpdateValues

		protected override void UpdateEnterpriseValues(BusinessObject businessObjectToUpdate)
		{
			BaseCusClassification classification = (BaseCusClassification)businessObjectToUpdate;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_LookupCode = LookupCode;
			classification.CC_TariffNum = TariffCode;
			classification.CC_Description = Description.Left(80);
			classification.CC_ClassificationType = ClassificationType;
			UpdateCountrySpecificData(classification);
		}

		#endregion

		#region Abstracts

		protected abstract Type GetClassificationType();
		protected abstract void UpdateCountrySpecificData(BaseCusClassification bizO);

		#endregion
	}
}
