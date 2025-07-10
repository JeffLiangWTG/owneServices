using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		JobComInvoiceLine AUInvoiceLine
		{
			get { return (JobComInvoiceLine)InvoiceLine; }
		}

		public override IBaseClassificationCollection<BaseCusClassification> ClassificationList
		{
			get
			{
				if (InvoiceLine == null || InvoiceLine.Declaration == null)
				{
					return base.ClassificationList;
				}
				else if (InvoiceLine.Declaration.IsImport || InvoiceLine.Declaration.IsDrawback)
				{
					return AUInvoiceLine.ImportClassificationList;
				}
				else
				{
					return AUInvoiceLine.ExportClassificationList;
				}
			}
		}

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			return new AUOrgSupplierPartCollection(Factory, AUInvoiceLine, AUInvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
		}

		public virtual CodeDescriptionPairList EXDOCPermitAuthorityList => new CodeDescriptionPairList();

		public override OrgHeaderCollection Suppliers => Factory.GetCachedValue("AU|JobComInvoiceLineLookups|Suppliers", () =>
		{
			var suppliers = new ConsignorCollection(Factory);
			suppliers.AllowOtherOrgTypes = true;
			suppliers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.DropEditRelationships.Code.CustomsCodeType,
																						"Property",
																						(ZString)OrgCusCode.CodeTypes.CustomsClientID));
			return suppliers;
		});
	}
}
