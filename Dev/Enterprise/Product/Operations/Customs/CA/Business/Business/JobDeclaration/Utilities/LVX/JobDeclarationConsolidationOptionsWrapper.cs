using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationConsolidationOptionsWrapper : IConsolidationOptionsWrapper
	{
		public JobDeclarationConsolidationOptionsWrapper(JobDeclaration declaration)
		{
			Declaration = declaration;
		}

		protected readonly JobDeclaration Declaration;

		#region IConsolidationStrategyBO Members

		public OrgHeader Importer
		{
			get { return Declaration.LVSImporter; }
		}

		public ZGuid ImporterPK
		{
			get
			{
				return Importer?.PK ?? ZGuid.Empty;
			}
		}

		public ZString LVSType
		{
			get { return ZString.Empty; }
		}

		public ZGuid BranchPK
		{
			get { return Declaration.JE_GB; }
		}

		public ZGuid CompanyPK
		{
			get { return Declaration.CompanyPK; }
		}

		public ZString Broker
		{
			get { return Declaration.JE_GS_NKCusAgent; }
		}

		public ZString PortOfClearanceCode
		{
			get { return Declaration.JE_CustomsOffice; }
		}

		public ZZRefCusCodeListCombined PortOfClearance
		{
			get { return Declaration.PortOfClearance; }
		}

		public ZDateTime EntryAuthorisationDate
		{
			get { return new ZDate(Declaration.JE_EntryAuthorisationDate.Year, Declaration.JE_EntryAuthorisationDate.Month, 1); }
		}

		public ZBool IsAllowOIC
		{
			get { return Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => !line.CA_AuthorityNumber.IsEmpty); }
		}

		public ZString Province
		{
			get { return PortOfClearance?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province) ?? ZString.Empty; }
		}

		#endregion
	}
}
