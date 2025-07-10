using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BulkLookupChanger : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BulkLookupChanger(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Treatment Code
		[MaxLength(3)]
		public ZString TreatmentCode
		{
			get { return treatmentCode; }
			set
			{
				if (treatmentCode != value)
				{
					CheckMaximumLength(TreatmentCodeInfo, value);
					treatmentCode = value;
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}
				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
				TreatmentCodeInfo.RefreshBinding();
			}
		}
		ZString treatmentCode = ZString.Empty;

		public ZPropertyInfo TreatmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(TreatmentCode)); }
		}

		void ValidateTreatmentCode()
		{
			TreatmentCodeInfo.ClearAllNotifications();
		}
		#endregion

		#region Remove Treatment Code
		public ZBool RemoveTreatmentCode
		{
			get
			{
				return removeTreatmentCode;
			}
			set
			{
				if (removeTreatmentCode ^ value)
				{
					removeTreatmentCode = value;
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}
				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
				RemoveTreatmentCodeInfo.RefreshBinding();
			}
		}
		ZBool removeTreatmentCode;

		public ZPropertyInfo RemoveTreatmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(RemoveTreatmentCode)); }
		}

		void ValidateRemoveTreatmentCode()
		{
			RemoveTreatmentCodeInfo.ClearAllNotifications();
		}
		#endregion

		[ReadOnly(true)]
		public ZInt EstimatedLookupCount
		{
			get { return estimatedLookupCount; }
			set { SetNonPersistentPropertyValue(EstimatedLookupCountInfo, ref estimatedLookupCount, value); }
		}
		ZInt estimatedLookupCount;

		public ZPropertyInfo EstimatedLookupCountInfo
		{
			get { return GetZPropertyInfo(nameof(EstimatedLookupCount)); }
		}

		public ZInt TreatmentsChanged { get; private set; }
		public ZInt TreatmentsRemoved { get; private set; }
		public ZInt PartsAdjusted { get; private set; }

		#endregion

		#region Validation
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTreatmentCode();
			ValidateRemoveTreatmentCode();
			if (TreatmentCode.IsEmpty && !RemoveTreatmentCode)
			{
				TreatmentCodeInfo.AddError(Res.GetString("4ff454c0-0377-4eab-91aa-3e29c084cb53", "No changes have been requested. Either enter a new Treatment Code or select to remove the existing Treatment Code"));
				RemoveTreatmentCodeInfo.AddError(Res.GetString("4ff454c0-0377-4eab-91aa-3e29c084cb53", "No changes have been requested. Either enter a new Treatment Code or select to remove the existing Treatment Code"));
			}
			if (!TreatmentCode.IsEmpty && RemoveTreatmentCode)
			{
				TreatmentCodeInfo.AddError(Res.GetString("86c67503-46ce-460b-a6cd-f7ad92674d99", "Please enter either new Treatment Code or select to remove the existing Treatment Code, but not both."));
				RemoveTreatmentCodeInfo.AddError(Res.GetString("86c67503-46ce-460b-a6cd-f7ad92674d99", "Please enter either new Treatment Code or select to remove the existing Treatment Code, but not both."));
			}
		}
		#endregion

		#region ChangeLookups

		public void ChangeLookups(ZQuery lookupFilter)
		{
			TreatmentsChanged = 0;
			TreatmentsRemoved = 0;
			PartsAdjusted = 0;
			foreach (Classification classification in Factory.Load<Classification>(lookupFilter))
			{
				{
					ZString oldTreatment = classification.TreatmentCode;
					if (RemoveTreatmentCode)
					{
						if (!classification.TreatmentCode.IsEmpty)
						{
							classification.TreatmentCode = ZString.Empty;
							TreatmentsRemoved++;
						}
					}
					else if (!TreatmentCode.IsEmpty)
					{
						if (classification.TreatmentCode != TreatmentCode)
						{
							classification.TreatmentCode = TreatmentCode;
							TreatmentsChanged++;
						}
					}
					if (!oldTreatment.IsEmpty)
					{
						ZQuery pivotQuery = new ZQuery(CusClassPartPivotSchema.CI_CC, classification.PK);
						pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_AddInfo, SQLComparisonOperator.Contains, AUAddInfoSchema.ZA_TreatmentCode_Hidden.Name.Substring(3) + "=" + oldTreatment);
						foreach (CusClassPartPivot pivot in Factory.Load<CusClassPartPivot>(pivotQuery))
						{
							if (RemoveTreatmentCode)
							{
								if (!pivot.AddInfo.ZA_TreatmentCode_Hidden.IsEmpty)
								{
									pivot.AddInfo.ZA_TreatmentCode_Hidden = ZString.Empty;
									PartsAdjusted++;
								}
							}
							else if (!TreatmentCode.IsEmpty)
							{
								if (pivot.AddInfo.ZA_TreatmentCode_Hidden != TreatmentCode)
								{
									pivot.AddInfo.ZA_TreatmentCode_Hidden = TreatmentCode;
									PartsAdjusted++;
								}
							}
						}
					}
				}
			}
		}
		#endregion

	}
}
