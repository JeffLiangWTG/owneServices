using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.Business.Organisation.Helpers
{
	class SysMergeARTermsValueObjectHelper
	{
		public SysMergeARTermsValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		#region OrgARTerm

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgARTermCollection arTermsCollection, OrgCompanyData companyData, IValueObjectImportContext context)
		{
			if (arTermsCollection.IsSpecified)
			{
				companyData.ARTerms.DeleteAll();
				for (int i = 0; i < arTermsCollection.Count; i++)
				{
					ImportFromValueObject(companyData, arTermsCollection[i], context);
				}
			}
		}

		void ImportFromValueObject(AutoOrgCompanyData companyData, Xsd.SysMergeOrgARTerm xsdARTerm, IValueObjectImportContext context)
		{
			var newARTerm = companyData.Factory.New<OrgARTerms>();
			newARTerm.PY_OB = companyData.PK;

			SetARTerms(newARTerm, xsdARTerm, context);

			foreach (Xsd.SysMergeOrgARTermInvoiceCycle xsdARTermsCycle in xsdARTerm.ARTermsCycles)
			{
				OrgARTermsCycle newARTermsCycle = companyData.Factory.New<OrgARTermsCycle>();
				SetARTermsCycles(newARTermsCycle, xsdARTermsCycle, context);
				newARTerm.ARTermsCycles.Add(newARTermsCycle);
			}
		}

		void SetARTerms(OrgARTerms arTermToUpdate, Xsd.SysMergeOrgARTerm arTerm, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_InvoiceClassInfo, arTerm.ARInvoiceClass);
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_InvoiceTermInfo, arTerm.ARInvoiceTerm);
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_InvoiceDaysInfo, arTerm.ARInvoiceTermDays.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_AgreedPaymentMethodInfo, arTerm.AgreedPaymentMethod);
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_JobTypeInfo, arTerm.JobType);
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_DirectionInfo, arTerm.Direction);
			context.SetPropertyInfoValueIfValueNotEmpty(arTermToUpdate.PY_TransportModeInfo, arTerm.TransportMode);
		}

		void SetARTermsCycles(OrgARTermsCycle arTermsCycleToUpdate, Xsd.SysMergeOrgARTermInvoiceCycle arTermCycle, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(arTermsCycleToUpdate.P5_ToDayInfo, arTermCycle.ToDay.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(arTermsCycleToUpdate.P5_PaymentDayInfo, arTermCycle.PaymentDay.ToString());
		}

		#endregion

		#region CFX

		public void ImportFromValueObjectCollection(Xsd.AccCFXConfigurationCollection accCFXCollection, OrgCompanyData companyData, IValueObjectImportContext context)
		{
			if (!accCFXCollection.IsSpecified)
			{
				return;
			}

			companyData.AccCFXConfigurations.DeleteAll();
			for (int i = 0; i < accCFXCollection.Count; i++)
			{
				ImportFromValueObject(companyData, accCFXCollection[i], context);
			}
		}

		void ImportFromValueObject(OrgCompanyData companyData, Xsd.AccCFXConfiguration cfxConfig, IValueObjectImportContext context)
		{
			var newCfx = companyData.AccCFXConfigurations.AddNew();
			newCfx.JCF_GC = companyData.OB_GC;
			context.SetPropertyInfoValueIfValueNotEmpty(newCfx.JCF_JobTypeInfo, cfxConfig.JobType);
			context.SetPropertyInfoValueIfValueNotEmpty(newCfx.JCF_ServiceDirectionInfo, cfxConfig.ServiceDirection);
			context.SetPropertyInfoValueIfValueNotEmpty(newCfx.JCF_TransportModeInfo, cfxConfig.TransportMode);
			context.SetPropertyInfoValueIfValueNotEmpty(newCfx.JCF_RX_NKCurrencyInfo, cfxConfig.Currency);
			context.SetPropertyInfoValueIfValueNotEmpty(newCfx.JCF_CFXPercentageInfo, cfxConfig.CFXPercentage.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCfx.JCF_CFXMinimumInfo, cfxConfig.CFXMinimum.ToString());
		}

		#endregion

		#endregion

		#region Export

		#region OrgARTerm

		public void ExportToValueObjectCollection(AutoOrgCompanyData companyData, Xsd.SysMergeOrgARTermCollection arTermsCollection)
		{
			ZQuery query = new ZQuery(OrgARTermsSchema.PY_OB, companyData.PK);
			OrgARTerms[] arTerms = companyData.Factory.Load<OrgARTerms>(query);

			for (int i = 0; i < arTerms.Length; i++)
			{
				OrgARTerms arTerm = arTerms[i];
				Xsd.SysMergeOrgARTerm xsdARTerms = ExportToValueObject(arTerm);
				arTermsCollection.Add(xsdARTerms);
			}
		}

		public Xsd.SysMergeOrgARTerm ExportToValueObject(OrgARTerms arTerm)
		{
			Xsd.SysMergeOrgARTerm result = new Xsd.SysMergeOrgARTerm();

			result.ARInvoiceClass = arTerm.PY_InvoiceClass;
			result.ARInvoiceTerm = arTerm.PY_InvoiceTerm;
			result.ARInvoiceTermDays = arTerm.PY_InvoiceDays;
			result.AgreedPaymentMethod = arTerm.PY_AgreedPaymentMethod;
			result.JobType = arTerm.PY_JobType;
			result.Direction = arTerm.PY_Direction;
			result.TransportMode = arTerm.PY_TransportMode;

			foreach (OrgARTermsCycle arTermsCycle in arTerm.ARTermsCycles)
			{
				Xsd.SysMergeOrgARTermInvoiceCycle termCycle = new Xsd.SysMergeOrgARTermInvoiceCycle();
				termCycle.ToDay = arTermsCycle.P5_ToDay;
				termCycle.PaymentDay = arTermsCycle.P5_PaymentDay;

				result.ARTermsCycles.Add(termCycle);
			}

			return result;
		}

		#endregion

		public void ExportToValueObjectCollection(OrgCompanyData companyData, Xsd.AccCFXConfigurationCollection cfxCollection)
		{
			var cfxBizos = companyData.AccCFXConfigurations.Where(b => b.Level == AccCFXConfigurationLevelEnum.Organisation);

			foreach (AccCFXUpliftConfiguration cfx in cfxBizos)
			{
				cfxCollection.Add(new Xsd.AccCFXConfiguration()
				{
					JobType = cfx.JCF_JobType,
					ServiceDirection = cfx.JCF_ServiceDirection,
					TransportMode = cfx.JCF_TransportMode,
					Currency = cfx.JCF_RX_NKCurrency,
					CFXPercentage = cfx.JCF_CFXPercentage,
					CFXMinimum = cfx.JCF_CFXMinimum
				});
			}
		}

		#endregion
	}
}
