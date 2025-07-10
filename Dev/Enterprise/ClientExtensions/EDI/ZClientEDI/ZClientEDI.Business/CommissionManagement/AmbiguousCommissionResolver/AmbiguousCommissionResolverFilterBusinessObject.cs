using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class AmbiguousCommissionResolverFilterBusinessObject : AutoAmbiguousCommissionResolverFilterBusinessObject
	{
		public AmbiguousCommissionResolverFilterBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region IsResolvingFiltered

		public override ZBool IsResolvingFiltered
		{
			get { return base.IsResolvingFiltered; }
			set
			{
				if (base.IsResolvingFiltered != value)
				{
					base.IsResolvingFiltered = value;

					if (!value)
					{
						InvoicePkToResolve = ZGuid.Empty;
						InvoiceNumberToResolve = ZString.Empty;

						Validation.ValidateCompanyPk();
						Validation.ValidateInvoicePkToResolve();
						Validation.ValidateInvoiceNumberToResolve();
					}
				}
			}
		}

		#endregion

		#region CompanyPk

		[List("Companies")]
		public override ZGuid CompanyPk
		{
			get { return base.CompanyPk; }
			set { base.CompanyPk = value; }
		}

		protected bool CompanyPk_ReadOnly
		{
			get { return !IsResolvingFiltered || OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value; }
		}

		#endregion

		#region FilterByPkIsAllowed

		protected internal ZBool FilterByPkIsAllowed
		{
			get { return CompanyPk == GlbCompany.CurrentCompany.PK; }
		}

		#endregion

		#region InvoicePkToResolve

		[List("InvoiceCollection")]
		public override ZGuid InvoicePkToResolve
		{
			get { return base.InvoicePkToResolve; }
			set { base.InvoicePkToResolve = value; }
		}

		protected bool InvoicePkToResolve_ReadOnly
		{
			get { return !IsResolvingFiltered || !FilterByPkIsAllowed; }
		}

		#endregion

		#region InvoiceNumberToResolve

		protected bool InvoiceNumberToResolve_ReadOnly
		{
			get { return !IsResolvingFiltered || FilterByPkIsAllowed; }
		}

		#endregion

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsResolvingUnresolved = true;
			CompanyPk = GlbCompany.CurrentCompany.PK;
		}

		#endregion

		#region Filter

		public ZQuery Filter
		{
			get
			{
				var result = new ZQuery();

				if (IsResolvingUnresolved)
				{
					result.AddToFilter(AccAmbiguousCommissionSchema.AC0_CA0_SelectedAgreement, null);
				}
				else if (IsResolvingFiltered)
				{
					if (FilterByPkIsAllowed)
					{
						result.AddToFilter(AccAmbiguousCommissionSchema.AC0_AH_Source, InvoicePkToResolve);
					}
					else
					{
						var query = new ZDBOnlyQuery(typeof(AccAmbiguousCommission));
						var invoiceSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccAmbiguousCommissionSchema.AC0_AH_Source);
						invoiceSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, CompanyPk);
						invoiceSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, InvoiceNumberToResolve);
						query.AddSubQuery(invoiceSubQuery, JoinCondition.And);
						result.AddToFilter(query);
					}
				}
				else
				{
					result = ZQuery.NoResultQuery;
				}

				return result;
			}
		}

		#endregion

		#region Lists

		public GlbCompanyCollection Companies
		{
			get { return companies ?? (companies = new GlbCompanyCollection(Factory)); }
		}
		GlbCompanyCollection companies;

		public TransactionHeaderCollection InvoiceCollection
		{
			get { return invoiceCollection ?? (invoiceCollection = new TransactionHeaderCollection(Factory)); }
		}
		TransactionHeaderCollection invoiceCollection;

		#endregion
	}
}
