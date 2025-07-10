using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public abstract class AutoCusClassification : Customs.Business.BaseCusClassification, Customs.Business.IAddInfoManager
	{
		public AutoCusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : Customs.Business.BaseCusClassification.Schema
		{
			public const string CC_ProcedureCode = "CC_ProcedureCode";
		}

		#endregion

		#region New Properties

		public virtual ZString CC_ProcedureCode
		{
			get { return AddInfo.ZG_ProcedureCode; }
			set { AddInfo.ZG_ProcedureCode = value; }
		}

		public ZPropertyInfo CC_ProcedureCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CC_ProcedureCode, x => AddInfo.ZG_ProcedureCodeInfo); }
		}

		#region Proxied AddInfo properties

		public AddInfoCusClassificationLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AddInfoCusClassificationValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		#endregion

		#endregion

		#region Implementation

		#region AddInfo
		protected AddInfoCusClassification AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoCusClassification(CC_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoCusClassification fAddInfo;
		#endregion

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo Customs.Business.IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
