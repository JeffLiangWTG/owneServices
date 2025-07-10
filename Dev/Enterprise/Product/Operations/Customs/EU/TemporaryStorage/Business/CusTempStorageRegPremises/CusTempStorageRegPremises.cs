using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegPremises : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremises, Integration.Customs.EU.ICusTempStorageRegPremises
{
	public CusTempStorageRegPremises(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusTempStorageRegPremisesLookups Lookups => (CusTempStorageRegPremisesLookups)base.Lookups;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesLookups GetNewLookups() => new CusTempStorageRegPremisesLookups(this);

	public new class Schema : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremises.Schema
	{
		public const string AuthorizationNumber = nameof(CusTempStorageRegPremises.AuthorizationNumber);
		public const string AuthorizationOwner = nameof(CusTempStorageRegPremises.AuthorizationOwner);
	}

	[ResourceStringData("23397F96-0719-4FBC-B591-E56828E2BEC3", Caption = "Authorization Number", MediumCaption = "Authorization No.", ShortCaption = "Auth. No.", FullDescription = "Number of Authorization")]
	[MaxLength(CusAuthorizationUsage.Schema.AGC_NumberMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegPremisesLookups.AuthorizationNumberList))]
	public virtual ZString AuthorizationNumber
	{
		get => Authorization.AGC_Number;
		set
		{
			var oldValue = AuthorizationNumber;
			CheckMaximumLength(AuthorizationNumberInfo, value);
			Authorization.AGC_Number = value;
			if (!IsCopying && oldValue != AuthorizationNumber)
			{
				UpdateAfterAuthorizationDataChange();
				MarkAsNeedingValidation();
			}
			AuthorizationNumberInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo AuthorizationNumberInfo => GetWrappedZPropertyInfo(Schema.AuthorizationNumber, x => Authorization.AGC_NumberInfo);

	[ResourceStringData("8A7E1298-A293-4A69-9BE5-8DE595E7507D", Caption = "Owner", MediumCaption = "Owner", ShortCaption = "Owner", FullDescription = "Authorization Owner")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegPremisesLookups.OwnerList))]
	public ZGuid AuthorizationOwner
	{
		get => Authorization.AGC_OH_Owner;
		set
		{
			var oldValue = AuthorizationOwner;
			Authorization.AGC_OH_Owner = value;
			if (!IsCopying && oldValue != AuthorizationOwner)
			{
				UpdateAfterAuthorizationDataChange();
				MarkAsNeedingValidation();
			}
			AuthorizationOwnerInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo AuthorizationOwnerInfo => GetWrappedZPropertyInfo(Schema.AuthorizationOwner, x => Authorization.AGC_OH_OwnerInfo);

	[ChildEditable(true)]
	protected CusAuthorizationUsage Authorization
	{
		get
		{
			if (authorization == null || authorization.IsDeleted || authorization.AGC_ParentID != PK || authorization.AGC_ParentTableCode != TablePrefix)
			{
				var query = new ZQuery(CusAuthorizationUsageSchema.AGC_ParentID, PK);
				_ = query.AddToFilter(new ZQuery(CusAuthorizationUsageSchema.AGC_ParentTableCode, TablePrefix));
				authorization = (CusAuthorizationUsage)Factory.LoadTop1(AuthorizationType, query);

				if (authorization == null)
				{
					authorization = (CusAuthorizationUsage)Factory.New(AuthorizationType);
					authorization.AGC_ParentTableCode = TablePrefix;
					authorization.AGC_ParentID = PK;
					authorization.AGC_Code = DefaultAuthorizationCode;
					RegisterEditableChildObject(authorization);
				}
			}
			return authorization;
		}
	}
	CusAuthorizationUsage authorization;

	protected ZString DefaultAuthorizationCode => DefaultAuthorizationCodeCore;

	protected virtual ZString DefaultAuthorizationCodeCore => AuthorizationTypeList.Codes.TST;

	protected virtual Type AuthorizationType => typeof(CusAuthorizationUsage);

	void UpdateAfterAuthorizationDataChange()
	{
		if (!AuthorizationNumber.IsEmpty && !AuthorizationOwner.IsEmpty && SRP_OA_PremisesAddress.IsEmpty)
		{
			SRP_OA_PremisesAddress = Authorization.Lookups.NumberList.FirstOrDefault(x => x.CPH_Number == authorization.AGC_Number)?.CPH_OA_AppliesTo ?? ZGuid.Empty;
		}
	}

	protected override EFTA.TemporaryStorageRegister.Business.TSCustomsNumberViewStmNumsBusinessProviderFactory CreateTSCustomsNumberViewStmNumsBusinessProviderFactory()
		=> new TSCustomsNumberViewStmNumsBusinessProviderFactory(Factory);
}
