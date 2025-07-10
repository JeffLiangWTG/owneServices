using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreement : OrgCommissionAgreement, ICommissionAgreementRelated<EdiCommissionAgreement>
	{
		public EdiCommissionAgreement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Draft

		public new EdiCommissionAgreement ParentVersion
		{
			get { return (EdiCommissionAgreement)base.ParentVersion; }
		}

		protected override void PopulateDraft(OrgCommissionAgreement draft)
		{
			base.PopulateDraft(draft);

			if (Customization != null)
			{
				((EdiCommissionAgreement)draft).Customization = Customization.CreateDraft();
			}
		}

		protected override void CopyValuesFromDraft(OrgCommissionAgreement draft)
		{
			base.CopyValuesFromDraft(draft);

			var draftCustomization = ((EdiCommissionAgreement)draft).Customization;
			if (draftCustomization != null)
			{
				Customization = draftCustomization.MergeDraft();
			}
		}

		#endregion

		#region Customer

		[List("Lookups.CustomerPairList")]
		[ResourceStringData("EdiCommissionAgreement|CustomerCode", Caption = "Customer")]
		public ZString CustomerCode
		{
			get { return Customer?.OH_Code ?? ZString.Empty; }
			set
			{
				var selectedCustomer = Lookups.Customers.Cast<OrgHeader>().FirstOrDefault(x => x.OH_Code == value);
				if (selectedCustomer != null)
				{
					CA0_OH_Customer = selectedCustomer.PK;
				}
				else
				{
					CA0_OH_Customer = ZGuid.Empty;
				}
			}
		}

		public ZWrappedPropertyInfo CustomerCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CustomerCode), x => CA0_OH_CustomerInfo); }
		}

		#endregion

		#region EffectiveDate

		protected override ZDate GetEffectiveDateForTriggerType(ZString triggerType)
		{
			var date = base.GetEffectiveDateForTriggerType(triggerType);
			if (triggerType == EDICommissionTriggerTypes.Codes.AYCEStartDate)
			{
				if (Customer == null)
				{
					return ZDate.Empty;
				}

				var settings = EDIDataRegistry.Instance.AYCTriggerTypeSettings.Value;
				if (!settings.PrimaryChargeCode.IsEmpty)
				{
					date = GetAYCEffectiveDateForCharge(settings.PrimaryChargeCode);
					if (!date.IsEmpty)
					{
						return date;
					}
				}

				if (!settings.SecondaryChargeCode.IsEmpty)
				{
					date = GetAYCEffectiveDateForCharge(settings.SecondaryChargeCode);
				}
			}
			return date;
		}

		ZDate GetAYCEffectiveDateForCharge(string chargeCode)
		{
			var query = new ZDBOnlyQuery(typeof(ClientLicenceFee));
			var subQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.PK, ClientLicenceFeeSchema.L8_LC);
			subQuery.AddToFilter(LicenceCompanySchema.LC_OH, CA0_OH_Customer);
			query.AddToFilter(ClientLicenceFeeSchema.L8_ChargeCode, chargeCode);
			query.AddToFilter(ClientLicenceFeeSchema.L8_StartDate, SQLComparisonOperator.NotEqual, DBNull.Value);
			query.OrderBy = ClientLicenceFee.Schema.L8_StartDate;
			query.AddSubQuery(subQuery, JoinCondition.And);
			var result = Factory.LoadTop1<ClientLicenceFee>(query);
			return result == null ? ZDate.Empty : result.L8_StartDate.Date;
		}

		#endregion

		#region Customization

		public EdiCommissionAgreementCustomization Customization
		{
			get
			{
				if (!customizationInitialized ||
					(customization != null && customization.IsDeleted) ||
					(customization != null && customization.EZN_CA0 != PK))
				{
					Customization = (EdiCommissionAgreementCustomization)Factory.LoadFromUniqueKey<IEdiCommissionAgreementCustomization>(EdiCommissionAgreementCustomizationSchema.EZN_CA0, PK);
				}

				return customization;
			}
			private set
			{
				customizationInitialized = true;

				if (customization != value)
				{
					if (customization != null)
					{
						UnRegisterEditableChildObject(customization);
					}

					customization = value;

					if (customization != null)
					{
						if (customization.EZN_CA0 != PK)
						{
							customization.EZN_CA0 = PK;
						}

						RegisterEditableChildObject(customization);
					}
				}
			}
		}
		EdiCommissionAgreementCustomization customization;
		bool customizationInitialized;

		public EdiCommissionAgreementCustomization GetOrCreateCustomization()
		{
			if (Customization == null)
			{
				var newCustomization = (EdiCommissionAgreementCustomization)Factory.New<IEdiCommissionAgreementCustomization>();
				using (newCustomization.SuspendSettingHasChanges())
				{
					newCustomization.EZN_CA0 = PK;
				}

				Customization = newCustomization;
			}

			return Customization;
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			base.OnSaving();

			if (Customization != null && (!IsInDatabase || CA0_OH_CustomerInfo.HasChanges))
			{
				Customization.RemoveAllRelatedBusinessObjectsNotBelongingToLicenceEnterprise();
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (Customization != null)
			{
				Customization.Delete();
			}

			base.Delete();
		}

		#endregion

		#region Logs

		#region Related Event BusinessObjects

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				if (Customization != null)
				{
					var result = new System.Collections.Generic.List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
					result.Add(Customization);
					result.AddRange(Customization.BusinessObjectsWithRelatedEvents);
					return result.ToArray();
				}
				else
				{
					return base.BusinessObjectsWithRelatedEventsCore;
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		public new EdiCommissionAgreementLookups Lookups
		{
			get { return base.Lookups as EdiCommissionAgreementLookups; }
		}

		protected override OrgCommissionAgreementLookups GetNewLookups()
		{
			return new EdiCommissionAgreementLookups(this);
		}

		#endregion

		#region ICommissionAgreementRelated Members

		OrgCommissionAgreement ICommissionAgreementRelated<EdiCommissionAgreement>.CommissionAgreement
		{
			get { return this; }
		}

		#endregion
	}
}

