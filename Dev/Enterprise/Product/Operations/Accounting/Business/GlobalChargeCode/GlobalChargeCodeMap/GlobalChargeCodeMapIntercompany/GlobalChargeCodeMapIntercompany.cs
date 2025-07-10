using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapIntercompany : GlobalChargeCodeMap, IGlobalChargeCodeMapIntercompany
	{
		public new abstract class Schema : GlobalChargeCodeMap.Schema
		{
			public const string YG_HasLocalClientOverride = "YG_HasLocalClientOverride";
		}

		public GlobalChargeCodeMapIntercompany(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region YG_APChargeCode

		[MaxLength(10)]
		public ZString YG_APChargeCode
		{
			get
			{
				if (fYG_APChargeCode == ZString.Empty)
				{
					fYG_APChargeCode = PivotCollection.GetCodeOfAPChargeCode();
				}
				return fYG_APChargeCode;
			}
		}

		ZString fYG_APChargeCode;

		public ZPropertyInfo YG_APChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.YG_APChargeCode); }
		}

		#endregion

		#region YG_ARChargeCodes

		[MaxLength(255)]
		public ZString YG_ARChargeCodes
		{
			get
			{
				if (fYG_ARChargeCodes == ZString.Empty)
				{
					fYG_ARChargeCodes = PivotCollection.GetCodesOfARChargeCodes();
				}
				return fYG_ARChargeCodes;
			}
		}

		ZString fYG_ARChargeCodes;

		public ZPropertyInfo YG_ARChargeCodesInfo
		{
			get { return GetZPropertyInfo(Schema.YG_ARChargeCodes); }
		}

		#endregion

		#region

		public ZBool YG_HasLocalClientOverride
		{
			get
			{
				return PivotWithOverrideLocalClientCollection.Any();
			}
		}

		public ZPropertyInfo YG_HasLocalClientOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.YG_HasLocalClientOverride); }
		}

		#endregion

		[ChildEditable]
		public GlobalChargeCodeMapPivotIntercompanyCollection PivotCollection
		{
			get
			{
				if (fPivotCollection == null)
				{
					fPivotCollection = new GlobalChargeCodeMapPivotIntercompanyCollection(Factory, PK);
					fPivotCollection.Load();
					RegisterEditableChildObject(fPivotCollection);
				}
				return fPivotCollection;
			}
		}

		GlobalChargeCodeMapPivotIntercompanyCollection fPivotCollection;

		[ChildEditable]
		public GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection PivotWithOverrideLocalClientCollection
		{
			get
			{
				if (fPivotWithLocalClientOverrideCollection == null)
				{
					fPivotWithLocalClientOverrideCollection = new GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(Factory, PK);
					fPivotWithLocalClientOverrideCollection.Load();
					RegisterEditableChildObject(fPivotWithLocalClientOverrideCollection);
				}
				return fPivotWithLocalClientOverrideCollection;
			}
		}

		GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection fPivotWithLocalClientOverrideCollection;

		[ChildEditable]
		public GlobalChargeCodeMapPivotIntercompanyWithoutLocalClientOverrideCollection PivotWithoutOverrideLocalClientCollection
		{
			get
			{
				if (fPivotWithoutLocalClientOverrideCollection == null)
				{
					fPivotWithoutLocalClientOverrideCollection = new GlobalChargeCodeMapPivotIntercompanyWithoutLocalClientOverrideCollection(Factory, PK);
					fPivotWithoutLocalClientOverrideCollection.Load();
					RegisterEditableChildObject(fPivotWithoutLocalClientOverrideCollection);
				}
				return fPivotWithoutLocalClientOverrideCollection;
			}
		}

		GlobalChargeCodeMapPivotIntercompanyWithoutLocalClientOverrideCollection fPivotWithoutLocalClientOverrideCollection;

		#endregion
	}
}

