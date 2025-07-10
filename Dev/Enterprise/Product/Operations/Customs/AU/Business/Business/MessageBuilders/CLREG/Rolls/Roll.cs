using System.Data;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class Roll : Customs.Business.MultiLineAddInfos.CusAddInfo<RollAddInfo>
	{
		public Roll(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		#pragma warning disable IDE0001 // Prevent simplification of explicit generic type
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<RollAddInfo>.Schema
		#pragma warning restore IDE0001 // Prevent simplification of explicit generic type
		{
			public const string ZA_Roll = AURollAddInfoSchema.Constants.ZA_Roll;
		}
		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(AURollAddInfoLookups.ClientRolls))]
		public ZString ZA_Roll
		{
			get { return AddInfo.ZA_Roll; }
			set { AddInfo.ZA_Roll = value; }
		}

		public ZPropertyInfo ZA_RollInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_Roll, x => AddInfo.ZA_RollInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public AURollAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AURollAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		RollAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new RollAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					fAddInfo.Roll = this;
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		RollAddInfo fAddInfo;

		#endregion
	}
}
