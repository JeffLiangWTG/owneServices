using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientFeatureRequestValueAndContribution : AutoClientFeatureRequestValueAndContribution
	{
		public ClientFeatureRequestValueAndContribution(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ClientFeatureRequestValueAndContributionUniqueIndexFailureHandler(); }
		}

		protected override void RunPreSaveValidationCore()
		{
			PersistEBV();
			base.RunPreSaveValidationCore();
		}

		public void PersistEBV()
		{
			T9_EBV = CalculatedEBV;
		}

		#endregion

		#region Properties

		[ReadOnly(true)]
		public override ZDecimal T9_EBV
		{
			get { return base.T9_EBV; }
			set { base.T9_EBV = value; }
		}

		#region CalculatedEBV

		public ZDecimal CalculatedEBV
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (ZPropertyInfo ebvComponent in EBVComponents)
				{
					result += (ZDecimal)ebvComponent.Value;
				}
				return result;
			}
		}

		public ZPropertyInfo CalculatedEBVInfo
		{
			get { return GetZPropertyInfo(nameof(CalculatedEBV)); }
		}

		IEnumerable<ZPropertyInfo> EBVComponents
		{
			get
			{
				yield return T9_CostReductionInfo;
				yield return T9_RiskReductionInfo;
				yield return T9_ReductionInErrorRatesInfo;
				yield return T9_ProcessSimplificationInfo;
				yield return T9_SatisfactionInfo;
				yield return T9_SalesImprovementInfo;
				yield return T9_PreventionOfLossOfCustomerOrBusinessInfo;
				yield return T9_CompetitiveAdvantageInfo;
				yield return T9_AnyEffectThatLowersCostsOrGrowsRevenueInfo;
			}
		}

		#endregion

		#region CurrencyCode

		public ZString CurrencyCode
		{
			get { return DefaultCurrencyCode; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		public const string DefaultCurrencyCode = "USD";

		#endregion

		#region ShouldValidateMandatoryEBV

		public delegate bool ShouldValidateMandatoryEBVDelegate();
		ShouldValidateMandatoryEBVDelegate shouldValidateMandatoryEBVDelegate;

		public bool ShouldValidateMandatoryEBV
		{
			get { return (shouldValidateMandatoryEBVDelegate != null && shouldValidateMandatoryEBVDelegate()); }
		}

		public void SetShouldValidateMandatoryEBVDelegate(ShouldValidateMandatoryEBVDelegate shouldValidateDelegate)
		{
			this.shouldValidateMandatoryEBVDelegate = shouldValidateDelegate;
		}

		#endregion

		#endregion

		public static ClientFeatureRequestValueAndContribution Load(BusinessObject parent)
		{
			Argument.NotNull(parent, "parent");

			ZQuery query = new ZQuery(ClientFeatureRequestValueAndContributionSchema.T9_ParentID, parent.PK);
			return parent.Factory.LoadTop1<ClientFeatureRequestValueAndContribution>(query);
		}

		public static ClientFeatureRequestValueAndContribution LoadOrCreateNew(BusinessObject parent)
		{
			ClientFeatureRequestValueAndContribution result = Load(parent);
			if (result == null)
			{
				result = parent.Factory.New<ClientFeatureRequestValueAndContribution>();
				using (result.SuspendSettingHasChanges())
				{
					result.T9_ParentID = parent.PK;
					result.T9_ParentTableCode = parent.TablePrefix;
				}
			}

			return result;
		}

		#region Store / Restore EBV Component Values for Calculator

		public void StoreCurrentEBVComponentValuesToMemory()
		{
			EBVComponentValuesStoredInMemory.Clear();
			foreach (ZPropertyInfo ebvComponent in EBVComponents)
			{
				EBVComponentValuesStoredInMemory.Add(ebvComponent.Name, ebvComponent.Value);
			}
		}

		public void RestoreEBVComponentValuesFromMemory()
		{
			foreach (ZPropertyInfo ebvComponent in EBVComponents)
			{
				IZType value;
				if (EBVComponentValuesStoredInMemory.TryGetValue(ebvComponent.Name, out value))
				{
					ebvComponent.Value = value;
				}
			}
		}

		Dictionary<string, IZType> EBVComponentValuesStoredInMemory
		{
			get { return eBVComponentValuesStoredInMemory ?? (eBVComponentValuesStoredInMemory = new Dictionary<string, IZType>()); }
		}

		Dictionary<string, IZType> eBVComponentValuesStoredInMemory;

		#endregion

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsInDatabase || HasNonEmptyValueFields); }
		}

		public bool HasNonEmptyValueFields
		{
			get { return !T9_Contribution.IsEmpty || !CalculatedEBV.IsEmpty; }
		}

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			FireOnSavingEvent();
		}

		public event EventHandler Saving;

		void FireOnSavingEvent()
		{
			if (Saving != null)
			{
				Saving(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}

