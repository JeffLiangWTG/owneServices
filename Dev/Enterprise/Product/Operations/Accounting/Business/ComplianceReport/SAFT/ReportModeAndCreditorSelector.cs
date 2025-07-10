using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.SAFT
{
	public class ReportModeAndCreditorSelector : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ReportModeAndCreditorSelector(BusinessObjectFactory factory) : base(factory)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var selector = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IReportModeAndCreditorSelectorDefault>)?.Get();
			if (selector != null)
			{
				GenerateSalesInvoices = selector.GenerateSalesInvoices;
				GenerateCreditorInvoices = selector.GenerateCreditorInvoices;
				GenerateAllInvoices = selector.GenerateAllInvoices;
				ShouldPopup = selector.ShouldPopup;
			}
		}

		#region Report Types

		public ZBool GenerateAllInvoices { get; private set; }

		public ZBool ShouldPopup { get; private set; }

		public ZBool GenerateSalesInvoices
		{
			get { return generateSalesInvoices; }
			set
			{
				SetNonPersistentPropertyValue(GenerateSalesInvoicesInfo, ref generateSalesInvoices, value);
				GenerateSalesInvoicesInfo.RefreshBinding();
				if (GenerateCreditorInvoices == value)
				{
					GenerateCreditorInvoices = !value;
				}
			}
		}
		ZBool generateSalesInvoices;

		public ZPropertyInfo GenerateSalesInvoicesInfo => GetZPropertyInfo(nameof(GenerateSalesInvoices));

		public ZBool GenerateCreditorInvoices
		{
			get { return generateCreditorInvoices; }
			set
			{
				SetNonPersistentPropertyValue(GenerateCreditorInvoicesInfo, ref generateCreditorInvoices, value);
				GenerateCreditorInvoicesInfo.RefreshBinding();
				if (GenerateSalesInvoices == value)
				{
					GenerateSalesInvoices = !value;
				}
			}
		}
		ZBool generateCreditorInvoices;

		public ZPropertyInfo GenerateCreditorInvoicesInfo => GetZPropertyInfo(nameof(GenerateCreditorInvoices));

		#endregion

		[List(nameof(Creditors))]
		public ZGuid CreditorPK
		{
			get { return creditorPK; }
			set
			{
				SetNonPersistentPropertyValue(CreditorPKInfo, ref creditorPK, value);
				ValidateCreditorPK();
				CreditorPKInfo.RefreshBinding();
			}
		}
		ZGuid creditorPK;

		protected bool CreditorPK_ReadOnly => GenerateSalesInvoices;

		public ZPropertyInfo CreditorPKInfo => GetZPropertyInfo(nameof(CreditorPK));

		public OrgHeader Creditor => Factory.Load<OrgHeader>(CreditorPK);

		#region Creditors

		public CreditorCollection Creditors => creditors ?? (creditors = FindboxLookupCollections.GetCreditorCollection(Factory));

		CreditorCollection creditors;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCreditorPK();
		}

		void ValidateCreditorPK()
		{
			CreditorPKInfo.ClearAllNotifications();

			if (GenerateCreditorInvoices)
			{
				MandatoryValidation.CheckEntered(CreditorPKInfo);
				if (!CreditorPKInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(CreditorPKInfo);
				}
				if (!CreditorPKInfo.HasErrors() && !(Creditor?.CompanyData.OB_APCostsSelfBilled ?? false))
				{
					CreditorPKInfo.AddError(Res.GetString("0a0424f7-f9d3-49f1-a5ad-012e86c006a1", "Select a Cost Self Billed Creditor."));
				}
			}
		}

		#endregion
	}
}
