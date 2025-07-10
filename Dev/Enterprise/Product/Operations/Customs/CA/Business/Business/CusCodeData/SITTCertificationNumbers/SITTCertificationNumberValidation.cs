using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class SITTCertificationNumberValidation : CusCodeDataValidation
	{
		public SITTCertificationNumberValidation(SITTCertificationNumber parent)
			: base(parent)
		{
		}

		protected new SITTCertificationNumber Parent => (SITTCertificationNumber)base.Parent;

		#region CheckCY_Data

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo, Res.GetString("a03b1ab8-48b1-4309-ad75-7ca357eb6010", "Number"));
			if (!Parent.CY_Data.IsEmpty)
			{
				var grandFather = Parent.Parent;
				if (grandFather != null)
				{
					var query = new ZQuery(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.SITTNumber);
					query.AddToFilter(CusCodeDataSchema.CY_Data, Parent.CY_Data);
					query.AddToFilter(CusCodeDataSchema.CY_ParentID, grandFather.PK);
					query.AddToFilter(CusCodeDataSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					query.FetchOnlyFromLocalCache = !grandFather.IsInDatabase;
					if (Parent.Factory.Load<SITTCertificationNumber>(query).Length > 0)
					{
						Parent.CY_DataInfo.AddMessageError(Res.GetString("E76F82BF-E84B-4E7E-A4A8-CDCFB6B7A6EE", "This number is duplicated."));
					}
				}
			}
		}

		#endregion
	}
}
