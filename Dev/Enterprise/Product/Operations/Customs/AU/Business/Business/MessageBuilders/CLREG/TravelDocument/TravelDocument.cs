using System.Data;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TravelDocument : Customs.Business.MultiLineAddInfos.CusAddInfo<TravelDocAddInfo>
	{
		public TravelDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		#pragma warning disable IDE0001 // Prevent simplification of explicit generic type
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<TravelDocAddInfo>.Schema
		#pragma warning restore IDE0001 // Prevent simplification of explicit generic type
		{
			public const string ZA_Country = AUTravelDocAddInfoSchema.Constants.ZA_Country;
			public const string ZA_DocumentNo = AUTravelDocAddInfoSchema.Constants.ZA_DocumentNo;
		}
		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(AUTravelDocAddInfoLookups.Countries))]
		public ZString ZA_Country
		{
			get { return AddInfo.ZA_Country; }
			set { AddInfo.ZA_Country = value; }
		}

		public ZPropertyInfo ZA_CountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Country, x => AddInfo.ZA_CountryInfo); }
		}

		public ZString ZA_DocumentNo
		{
			get { return AddInfo.ZA_DocumentNo; }
			set { AddInfo.ZA_DocumentNo = value; }
		}

		public ZPropertyInfo ZA_DocumentNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_DocumentNo, x => AddInfo.ZA_DocumentNoInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public AUTravelDocAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AUTravelDocAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		TravelDocAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new TravelDocAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		TravelDocAddInfo fAddInfo;

		#endregion
	}
}
