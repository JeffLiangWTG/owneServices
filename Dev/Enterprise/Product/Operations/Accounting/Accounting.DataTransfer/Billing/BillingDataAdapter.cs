using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public partial class BillingDataAdapter : IValueObjectDataAdapter, Accounting.Integration.IBillingDataAdapter
	{
		#region Export

		public void Export(Job job, Xsd.Billing billingValue, IValueObjectExportContext context)
		{
			foreach (Charge charge in job.Charges)
			{
				if (ShouldExportCharge(charge, context) && charge.ChargeCode != null)
				{
					Xsd.ChargeLine line = billingValue.ChargeLines.AddNew();
					line.ChargeCode = charge.ChargeCode.AC_Code;
					line.Description = charge.JR_Desc;

					line.CollectSpecified = false;
					ExportCollect(charge, line);

					if (charge.SellCurrency != null)
					{
						line.OSSellAmount.CurrencyCode = charge.JR_RX_NKSellCurrency;
						line.OSSellAmount.Value = charge.JR_OSSellAmt;
					}

					if (charge.CostCurrency != null)
					{
						line.OSCostAmount.CurrencyCode = charge.JR_RX_NKCostCurrency;
						line.OSCostAmount.Value = charge.JR_OSCostAmt;
					}

					if (charge.SellAccount != null)
					{
						line.Debtor = new OrganisationValueObjectDataAdapter().ExportToValueObject(charge.SellAccount, context);
					}

					if (charge.CostAccount != null)
					{
						line.Creditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(charge.CostAccount, context);
					}

					line.Branch = charge.Branch.GB_Code;
					line.Department = charge.Department.GE_Code;

					if (SystemDataRegistry.Instance.IncludeLocValAndExRateWhenExportingBilling.Value)
					{
						if (charge.JR_LocalSellAmt != null)
						{
							line.LocalSellAmount.CurrencyCode = charge.JR_LocalCurrencyCode;
							line.LocalSellAmount.Value = charge.JR_LocalSellAmt;
						}

						if (charge.JR_LocalCostAmt != null)
						{
							line.LocalCostAmount.CurrencyCode = charge.JR_LocalCurrencyCode;
							line.LocalCostAmount.Value = charge.JR_LocalCostAmt;
						}

						if (charge.JR_OSSellExRate != null)
						{
							line.SellExchangeRate = charge.JR_OSSellExRate;
						}

						if (charge.JR_OSCostExRate != null)
						{
							line.CostExchangeRate = charge.JR_OSCostExRate;
						}
					}
				}
			}

			Xsd.BillingWithExchangeRates withExchangeRates = billingValue as Xsd.BillingWithExchangeRates;

			if (withExchangeRates != null)
			{
				ExportExchangeRates(withExchangeRates.ExchangeRates, job);
			}

			billingValue.LocalClient = new OrganisationValueObjectDataAdapter().ExportToValueObject(job.LocalCharges, context);

			if (billingValue.ChargeLines.Count > 0)
			{
				billingValue.IsSpecified = true;
			}
		}

		protected virtual bool ShouldExportCharge(Charge charge, INotifications notifications)
		{
			return true;
		}

		protected virtual void ExportCollect(Charge charge, Xsd.ChargeLine line)
		{
		}

		protected virtual void CollectExchangeRates(IDictionary<string, Xsd.ExchangeRate> list, Job job)
		{
			foreach (ExchangeRate rate in job.ExchangeRates)
			{
				if (rate.JF_BaseRate > 0)
				{
					list[rate.JF_RX_NKRateCurrency] = new Xsd.ExchangeRate()
					{
						CurrencyCode = rate.JF_RX_NKRateCurrency,
						Value = rate.JF_BaseRate,
					};
				}
			}
		}

		void ExportExchangeRates(Xsd.BillingWithExchangeRatesExchangeRates exchangeRates, Job job)
		{
			IDictionary<string, Xsd.ExchangeRate> list = new SortedDictionary<string, Xsd.ExchangeRate>();

			CollectExchangeRates(list, job);

			GlbCompany company;

			if (list.Count > 0 && (company = job.Company) != null)
			{
				string local = company.GC_RX_NKLocalCurrency;

				if (!list.ContainsKey(local))
				{
					list.Add(local, new Xsd.ExchangeRate()
					{
						CurrencyCode = local,
						Value = 1,
					});
				}

				foreach (Xsd.ExchangeRate rate in list.Values)
				{
					exchangeRates.ExchangeRate.Add(rate);
				}

				exchangeRates.BaseCurrency = local;
				exchangeRates.IsReciprocal = company.GC_IsReciprocal;
				exchangeRates.IsSpecified = true;
			}
			else
			{
				exchangeRates.IsSpecified = false;
			}
		}

		#endregion

		#region Import

		public void Import(IJobHeaderParent parent, Xsd.Billing billing, IValueObjectImportContext context)
		{
			bool verboseLogging = eHubMessagingRegistry.Instance.XMLServiceVerboseLogging.Value;
			this.strategy = ChargeSpecificationStrategyFactory.NewStrategy(context, billing.SpecifiedCharges, billing.PostedChargesHandling);
			Job.Loader loader = new Job.Loader(parent);

			if (verboseLogging)
			{
				context.Notify(new InfoNotification(Res.GetString("54B0FE03-A828-45B9-ACFE-BCC5593AFC76",
					"Importing charge lines started. Billing={0}, Charge Lines={1}.",
					billing.IsSpecified, billing.ChargeLines.IsSpecified)));
			}

			if (billing.IsSpecified && billing.ChargeLines.IsSpecified)
			{
				if (verboseLogging)
				{
					context.Notify(new InfoNotification(Res.GetString("E8E7C9C4-FE65-4F5E-9D4F-CB96C11C33F2",
						"Importing {0} charge lines.",
						billing.ChargeLines.Count)));
				}

				foreach (Xsd.ChargeLine line in billing.ChargeLines)
				{
					bool skipLine = false;
					GlbBranch branch = (!line.Branch.IsEmpty) ? parent.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, line.Branch) : null;

					if (!line.Branch.IsEmpty && branch == null)
					{
						context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("8cc3ac21-199d-4ee4-9cae-d3cba0609923",
							"Unable to match the branch '{0}'",
							line.Branch)));
						skipLine = true;
					}

					if (branch != null && branch.GB_GC != Env.CurrentCompanyPK)
					{
						context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("66236398-D05E-44BC-A9A4-56D1043E8386",
							@"The imported branch '{0}' in the <ChargeLine> doesn't belong to the system company '{1}' processing the XML import.
Please ensure that the correct branch is specified",
							line.Branch, Env.CurrentCompany.Code)));
						skipLine = true;
					}

					GlbDepartment department = (!line.Department.IsEmpty) ? parent.Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, line.Department) : null;

					if (!line.Department.IsEmpty && department == null)
					{
						context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("36e6cdad-6703-4583-8931-6ad53730e43e",
							"Unable to match the department '{0}'",
							line.Department)));
						skipLine = true;
					}

					if (!skipLine)
					{
						Job header = (branch != null) ? loader.Load(true, branch.Company) : loader.Load(true);

						if (header == null)
						{
							header = (branch != null) ? loader.TryCreateWithMutex(branch) : loader.TryCreateWithMutex();
							if (header == null)
							{
								context.Notify(new ErrorNotification(ErrorType.Error, loader.GetJobCreationError()));
								skipLine = true;
							}
							else
							{
								if (department != null)
								{
									header.JH_GE = department.PK;
								}
								else if (header.Department == null)
								{
									header.JH_GE = GlbDepartment.CurrentDepartment.PK;
								}

								if (branch != null)
								{
									header.JH_GB = branch.PK;
								}
								else if (header.Branch == null)
								{
									header.JH_GB = GlbBranch.CurrentBranch.PK;
								}

								SetLastHeaderCreatedForTest(header);
							}
						}

						if (!skipLine && !strategy.SkipJobHeader(header))
						{
							if (billing.LocalClient.IsSpecified)
							{
								header.LocalChargesPK = context.FindOrCreateTempOrganisationPK(billing.LocalClient, header, OrganisationTypes.Debtor);
							}
							ImportChargeLine(header, line, context, branch, department);
						}
					}
				}
			}

			strategy.RemoveUnmatchedCharges();
			strategy.NotifySkippedHeaders();

			this.strategy = null;
		}

		void ImportChargeLine(Job job, Xsd.ChargeLine line, IValueObjectImportContext context, GlbBranch branch, GlbDepartment department)
		{
			ChargeCodeMatcher matcher = new ChargeCodeMatcher(job.Factory, context.Converter.MappingOrgPK, line.ChargeCode, context, job.Company.PK);
			AccChargeCode code = job.Factory.Load<AccChargeCode>(matcher.Result);
			RefCurrency sellCurrency = RefCurrency.LoadFromForeignCode(job.Factory, line.OSSellAmount.CurrencyCode, GlbCompany.CurrentCompany.OrgProxy) ?? RefCurrency.LoadFromCurrencyCode(job.Factory, line.OSSellAmount.CurrencyCode);
			RefCurrency costCurrency = RefCurrency.LoadFromForeignCode(job.Factory, line.OSCostAmount.CurrencyCode, GlbCompany.CurrentCompany.OrgProxy) ?? RefCurrency.LoadFromCurrencyCode(job.Factory, line.OSCostAmount.CurrencyCode);
			bool error = false;

			if (code == null)
			{
				context.Notify(new ErrorNotification(ErrorType.UnknownCode, Res.GetString("8052e589-ed90-4c88-aa6a-3e73cb4a3fa8",
					"Unable to match the charge code '{0}'",
					line.ChargeCode)));
				error = true;
			}

			if (sellCurrency == null && line.OSSellAmount.IsSpecified)
			{
				context.Notify(new ErrorNotification(ErrorType.UnknownCode, Res.GetString("0f16dae1-5a5c-4be5-abec-efca2fb89b98",
					"Unable to match the sell currency '{0}' when sell amount specified",
					line.OSSellAmount.CurrencyCode)));
				error = true;
			}

			if (costCurrency == null && line.OSCostAmount.IsSpecified)
			{
				context.Notify(new ErrorNotification(ErrorType.UnknownCode, Res.GetString("4bff2f93-56cb-48ab-b309-8abfa67ef399",
					"Unable to match the cost currency '{0}' when cost amount specified",
					line.OSCostAmount.CurrencyCode)));
				error = true;
			}

			Charge charge = null;
			if (!error)
			{
				charge = strategy.MatchCharge(job, code, branch, department, line);

				if (charge == null)
				{
					charge = job.Charges.AddNew();
					charge.JR_AC = code.PK;
				}

				error = ValidationIfApportioned(context, costCurrency, charge, line, job);
			}

			if (!error)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(charge.JR_DescInfo, line.Description);

				if (line.OSSellAmount.IsSpecified)
				{
					charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
					charge.JR_OSSellAmt = line.OSSellAmount.Value;
				}

				if (line.OSCostAmount.IsSpecified)
				{
					charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
					charge.JR_OSCostAmt = line.OSCostAmount.Value;
				}

				if (line.Debtor.IsSpecified)
				{
					charge.JR_OH_SellAccount = context.FindOrCreateTempOrganisationPK(line.Debtor, job, OrganisationTypes.Debtor);
				}

				if (line.Creditor.IsSpecified)
				{
					charge.JR_OH_CostAccount = context.FindOrCreateTempOrganisationPK(line.Creditor, job, OrganisationTypes.Creditor);
				}

				if (branch != null)
				{
					charge.JR_GB = branch.PK;
				}

				if (department != null)
				{
					charge.JR_GE = department.PK;
				}

				if (line.SellRatingOverride == "Y")
				{
					charge.JR_SellRatingOverride = true;
				}
				else if (line.SellRatingOverride == "N")
				{
					charge.JR_SellRatingOverride = false;
				}

				ImportCollect(job, charge, line);
				ImportChargeInvoice(charge, line);
			}
		}

		public bool ValidationIfApportioned(IValueObjectImportContext context, RefCurrency costCurrency, Charge charge, Xsd.ChargeLine line, Job job)
		{
			var error = false;
			if (charge.JR_IsApportioned)
			{
				if (line.OSCostAmount.IsSpecified)
				{
					if (charge.JR_RX_NKCostCurrency != costCurrency.RX_Code || charge.JR_OSCostAmt != line.OSCostAmount.Value)
					{
						context.Notify(new ErrorNotification(ErrorType.Warning, Res.GetString("3cc6a95a-7df3-47cb-9dfc-b6136f337a33",
							"Unable to modify the cost currency '{0}' or the cost amount '{1}' when charge apportioned",
							costCurrency.RX_Code, line.OSCostAmount.Value)));
						error = true;
					}
				}

				if (line.Creditor.IsSpecified)
				{
					if (charge.JR_OH_CostAccount != context.FindOrCreateTempOrganisationPK(line.Creditor, job, OrganisationTypes.Creditor))
					{
						context.Notify(new ErrorNotification(ErrorType.Warning, Res.GetString("6410363c-198d-4f80-8b2a-e162ab81993f",
							"Unable to modify the cost account '{0}' when charge apportioned",
							line.Creditor.OrganisationDetails.Name)));
						error = true;
					}
				}
			}
			return error;
		}

		protected virtual void ImportCollect(Job job, Charge charge, Xsd.ChargeLine line)
		{
		}

		protected virtual void ImportChargeInvoice(Charge charge, Xsd.ChargeLine line)
		{
		}

		#endregion

		#region IValueObjectDataAdapter Members

		bool IValueObjectDataAdapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible
		{
			get { return false; }
		}

		bool IValueObjectDataAdapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked
		{
			get { return false; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		Type IValueObjectDataAdapter.BusinessObjectType
		{
			get { throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter."); }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		XmlSchema IValueObjectDataAdapter.CollectionSchema
		{
			get { throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter."); }
		}

		BusinessObject IValueObjectDataAdapter.CreateOrUpdateFromValueObject(IValueObject value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter.");
		}

		void IValueObjectDataAdapter.ExportToValueObject(BusinessObject bizObj, IValueObject constructedValueObject, IValueObjectExportContext context)
		{
			IJobHeaderParent jobParent = (IJobHeaderParent)bizObj;
			Xsd.Billing billing = (Xsd.Billing)constructedValueObject;

			Job header = new Job.Loader(jobParent).Load();
			if (header != null)
			{
				header.Charges.Load();

				Export(header, billing, context);
			}
		}

		IValueObject IValueObjectDataAdapter.ExportToValueObject(BusinessObject bizObj, IValueObjectExportContext context)
		{
			Xsd.Billing billing = NewBilling();
			Export((Job)bizObj, billing, context);
			return billing;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IValueObjectDataAdapter.FileName
		{
			get { throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter."); }
			set { throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter."); }
		}

		BusinessObject IValueObjectDataAdapter.FindBusinessObject(IValueObject value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter.");
		}

		BusinessObject[] IValueObjectDataAdapter.FromXmlInterchange(IBusinessObjectCollection collectionForRelationshipSetup, IValueObjectImportContext context)
		{
			throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter.");
		}

		void IValueObjectDataAdapter.ImportFromValueObject(BusinessObject bizObj, IValueObject value, IValueObjectImportContext context)
		{
			Import((IJobHeaderParent)bizObj, (Xsd.Billing)value, context);
		}

		BusinessObject IValueObjectDataAdapter.NewBusinessObject(IValueObject value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter.");
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IValueObjectDataAdapter.RootCollectionElementName
		{
			get { throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter."); }
		}

		string IValueObjectDataAdapter.RootElementName
		{
			get { return "Billing"; } // XML Root Element Name
		}

		XmlSchema IValueObjectDataAdapter.Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.BillingSchema; }
		}

		IValueObject IValueObjectDataAdapter.ToXmlInterchange(IList bizObjs, IValueObjectExportContext context)
		{
			throw new NotSupportedException("The BillingDataAdapter can only be used from within another job's DataAdapter.");
		}

		Type IValueObjectDataAdapter.ValueObjectType
		{
			get { return typeof(Xsd.Billing); }
		}
		void IValueObjectDataAdapter.SetAdditionalRequirementsForDataExportEvent(Func<bool> conditions)
		{
		}

		#endregion

		#region Implementation

		protected virtual Xsd.Billing NewBilling()
		{
			return new Xsd.Billing();
		}

		partial void SetLastHeaderCreatedForTest(Job header);

		IChargeSpecificationStrategy strategy;

		#endregion

	}
}

#region Test
#if DEBUG

#region Test Members

namespace Enterprise.Accounting.DataTransfer
{
	partial class BillingDataAdapter
	{
		partial void SetLastHeaderCreatedForTest(Job header)
		{
			LastHeaderCreated = header;
		}

		internal Job LastHeaderCreated;
	}
}

#endregion
#endif
#endregion
