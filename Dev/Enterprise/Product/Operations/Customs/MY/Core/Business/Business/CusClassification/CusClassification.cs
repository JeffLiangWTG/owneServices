using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class CusClassification : TypeSafeCusClassification, Integration.Customs.MY.ICusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override Customs.Business.CusClassificationLookups GetNewLookups()
		{
			return new CusClassificationLookups(this);
		}

		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = CusClassification.ClassificationType.Both;
		}

		#endregion

		#endregion
	}
}
