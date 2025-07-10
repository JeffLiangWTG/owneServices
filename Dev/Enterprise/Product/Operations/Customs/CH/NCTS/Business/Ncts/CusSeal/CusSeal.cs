using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.CH.Business;
using static Enterprise.Integration.Customs;
using NctsUnloadedStateList = Enterprise.Customs.EU.NCTS.Business.NctsUnloadedStateList.Codes;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSeal : EU.NCTS.Business.CusSeal, ICusCodeDataTypeSupporter
{
	public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusSealLookups Lookups => (CusSealLookups)base.Lookups;

	protected override Customs.Business.CusSealLookups GetNewLookups() => new CusSealLookups(this);

	[ResourceStringData("C1E35942-1436-49D6-87D1-4B706DFBDA6B", Caption = "Unloaded State", MediumCaption = "Unloaded State", ShortCaption = "State")]
	[List(nameof(Lookups) + "." + nameof(CusSealLookups.UnloadedStates))]
	public override ZString BK_UnloadingState
	{
		get => base.BK_UnloadingState;
		set
		{
			var oldValue = BK_UnloadingState;
			base.BK_UnloadingState = value;
			if (!IsCopying && oldValue != BK_UnloadingState)
			{
				if (UnloadingRemarksText_ReadOnly)
				{
					UnloadingRemarksText = ZString.Empty;
				}
				UnloadingRemarks.MarkAsNeedingValidation();
			}
		}
	}

	#region

	[ChildEditable(true)]
	protected UnloadingRemarksCollection UnloadingRemarksCollection
	{
		get
		{
			if (unloadingRemarksCollection == null)
			{
				unloadingRemarksCollection = new UnloadingRemarksCollection(this);
				unloadingRemarksCollection.Load();
				RegisterEditableChildObject(unloadingRemarksCollection);
			}
			return unloadingRemarksCollection;
		}
	}
	UnloadingRemarksCollection unloadingRemarksCollection;

	protected UnloadingRemarks UnloadingRemarks => unloadingRemarks != null && !unloadingRemarks.IsDeleted ? unloadingRemarks : (unloadingRemarks = UnloadingRemarksCollection.FindOrCreate());
	UnloadingRemarks unloadingRemarks;

	[ResourceStringData("91B4A548-B3ED-426E-8E45-66F82C1E540B", Caption = "Unloading Remarks")]
	[ReadOnlyMember(nameof(UnloadingRemarksText_ReadOnly))]
	public ZString UnloadingRemarksText
	{
		get => UnloadingRemarks.CY_Data;
		set => UnloadingRemarks.CY_Data = value;
	}

	public ZPropertyInfo UnloadingRemarksTextInfo => GetWrappedZPropertyInfo(nameof(UnloadingRemarksText), _ => UnloadingRemarks.CY_DataInfo);

	bool UnloadingRemarksText_ReadOnly => BK_UnloadingState == NctsUnloadedStateList.DEC || (Header?.ArrivalMovementHeader.IsUnloadingRemarksReadOnly ?? false);

	#endregion

	public override void Delete()
	{
		if (!IsDeleted)
		{
			FetchForLoadChildEditableObjectsIfNeeded();
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
		}
		base.Delete();
	}

	public override bool ReadOnly
	{
		get => base.ReadOnly || (Header?.ArrivalMovementHeader.BM_NoChangesToReport ?? false) || (Header?.ArrivalMovementHeader.IsUnloadingRemarksReadOnly ?? false);
		set => base.ReadOnly = value;
	}

	#region ICusCodeDataTypeSupporter Members

	public IDictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = new Dictionary<ZString, Type>
		{
			{ CusCodeDataTypeList.Codes.UnloadingRemarks, typeof(UnloadingRemarks) }
		};
		return result;
	}

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
	}

	#endregion

}
