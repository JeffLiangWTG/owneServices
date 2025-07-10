using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegHeader : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader, ICusTempStorageRegHeader
{
	public CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusTempStorageRegHeaderLookups Lookups => (CusTempStorageRegHeaderLookups)base.Lookups;

	ICommonGuarantee ICusTempStorageRegHeader.Guarantee => Guarantee;

	public CommonGuarantee Guarantee
	{
		get
		{
			if (guarantee == null || guarantee.IsDeleted)
			{
				guarantee = Factory.LoadTop1<CommonGuarantee>(new ZQuery(CusBondDetailSchema.PW_ParentID, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

				if (guarantee == null)
				{
					guarantee = Factory.New<CommonGuarantee>();
					using (guarantee.SuspendSettingHasChanges())
					{
						guarantee.Parent = this;
					}
				}

				RegisterEditableChildObject(guarantee);
				guarantee.SetReadOnlyIncludingChildren(true);
			}
			return guarantee;
		}
	}
	CommonGuarantee guarantee;

	#region Type Decider

	[ThreadSafe]
	public new static readonly CusTempStorageRegHeaderTypeDecider TypeDecider = new();

	#endregion

	[ResourceStringData("2E441A9C-4017-49E1-B8B0-517C21EAFFAA", Caption = "Package Type")]
	public ZString PackageType => CusTempStorageHeader?.CusTempStorageDec?.PackageType ?? ZString.Empty;

	public ZPropertyInfo PackageTypeInfo => GetZPropertyInfo(nameof(PackageType));

	[ResourceStringData("6D5BFCFD-0F18-446C-A48C-BF5B37ABA49C", Caption = "Package Qty")]
	public ZInt PackageQty => CusTempStorageHeader?.CusTempStorageDec?.PackageQty ?? 0;

	public ZPropertyInfo PackageQtyInfo => GetZPropertyInfo(nameof(PackageQty));

	[ResourceStringData("DE397924-E878-4653-8C2D-30C600FCA72D", Caption = "Number of Lines")]
	public ZInt LineCount => CusTempStorageHeader?.CusTempStorageDec?.LineCount ?? 0;

	public ZPropertyInfo LineCountInfo => GetZPropertyInfo(nameof(LineCount));

	protected CusTempStorageJobHeader CusTempStorageHeader => Factory.LoadTop1<CusTempStorageJobHeader>(new ZQuery(CusTempStorageJobHeaderSchema.SJH_JobReference, SRH_InternalReference));

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups GetNewLookups() => new CusTempStorageRegHeaderLookups(this);

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremises GetPremisesByLinkedPremisesId(ZGuid premisesId) => Factory.Load<CusTempStorageRegPremises>(premisesId);

	protected override CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines() => new CusTempStorageRegLineCollection<CusTempStorageRegLine>(this);

	protected override Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);
}
