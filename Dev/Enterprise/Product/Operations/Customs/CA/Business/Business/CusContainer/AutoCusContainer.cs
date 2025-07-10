using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AutoCusContainer : Customs.Business.BaseCusContainer, Customs.Business.IAddInfoManager
	{
		protected AutoCusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : Customs.Business.BaseCusContainer.Schema
		{
			public const string CA_ContainerSizeOrISOCode = CAAddInfoSchema.Constants.CA_ContainerSizeOrISOCode;
			public const string CA_RN_NKCountryOfRegistration = CAAddInfoSchema.Constants.CA_RN_NKCountryOfRegistration;
		}
		#endregion

		#region AddInfo Properties
		#region CA_ContainerSizeOrISOCode
		public virtual ZString CA_ContainerSizeOrISOCode
		{
			get { return AddInfo.CA_ContainerSizeOrISOCode; }
			set { AddInfo.CA_ContainerSizeOrISOCode = value; }
		}

		public virtual ZPropertyInfo CA_ContainerSizeOrISOCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CA_ContainerSizeOrISOCode, x => AddInfo.CA_ContainerSizeOrISOCodeInfo); }
		}
		#endregion

		#region CA_RN_NKCountryOfRegistration
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoCusContainerLookups.CountryOfRegistrations))]
		public virtual ZString CA_RN_NKCountryOfRegistration
		{
			get { return AddInfo.CA_RN_NKCountryOfRegistration; }
			set { AddInfo.CA_RN_NKCountryOfRegistration = value; }
		}

		public virtual ZPropertyInfo CA_RN_NKCountryOfRegistrationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CA_RN_NKCountryOfRegistration, x => AddInfo.CA_RN_NKCountryOfRegistrationInfo); }
		}
		#endregion
		#endregion AddInfo Properties

		#region AddInfo/Validation/Lookups objects

		public AddInfoCusContainerValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		public AddInfoCusContainerLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		AddInfoCusContainer AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoCusContainer(CO_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoCusContainer fAddInfo;

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo Customs.Business.IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
