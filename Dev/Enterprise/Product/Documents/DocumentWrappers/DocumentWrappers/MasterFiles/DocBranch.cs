using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers
{
	public class DocBranch : DocBaseWrapper, IBranch
	{
		DocBranch(GlbBranch glbBranch, BusinessObjectFactory factoryForWrapper)
			: base(glbBranch, factoryForWrapper)
		{
		}

		public static DocBranch New(GlbBranch glbBranch, BusinessObjectFactory factoryForWrapper)
		{
			if (glbBranch == null)
			{
				return null;
			}
			else
			{
				return new DocBranch(glbBranch, factoryForWrapper);
			}
		}

		public static DocBranch New(ZGuid branchPK, BusinessObjectFactory factoryForWrapper)
		{
			return New(factoryForWrapper.Load<GlbBranch>(branchPK), factoryForWrapper);
		}

		GlbBranch GlbBranch
		{
			get { return (GlbBranch)WrappedObject; }
		}

		#region Customs Fields

		public ZGuid Branch
		{
			get
			{
				if (GlbBranch.PK.IsValid && GlbBranch.Company != null && GlbBranch.Company.PK.IsValid)
				{
					return GlbBranch.PK;
				}
				else
				{
					ZGuid currentBranch = ZGuid.Empty;
					if (GlbBranch.CurrentBranch.PK.IsValid)
					{
						currentBranch = GlbBranch.CurrentBranch.PK;
					}

					return currentBranch;
				}
			}
		}

		public ZGuid BranchCompany
		{
			get
			{
				if (GlbBranch.PK.IsValid && GlbBranch.Company != null && GlbBranch.Company.PK.IsValid)
				{
					return GlbBranch.Company.PK;
				}
				else
				{
					ZGuid currentCompany = ZGuid.Empty;
					if (GlbCompany.CurrentCompany.PK.IsValid)
					{
						currentCompany = GlbCompany.CurrentCompany.PK;
					}

					return currentCompany;
				}
			}
		}

		public Image Logo
		{
			get
			{
				return GetDepartmentBranchLogo(Guid.Empty);
			}
		}

		public Image GetDepartmentBranchLogo(Guid departmentPK)
		{
			return SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(BranchCompany.ToGuid(), Branch.ToGuid(), departmentPK);
		}

		Image GetInvoiceAndStatementLogo(Guid companyPK, Guid branchPK, Guid departmentPK, Guid departmentPKForCompanyLogo)
		{
			Image invAndStat = SystemDataRegistry.Instance.InvoceAndStatementLogo.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			if (invAndStat != null)
			{
				return invAndStat;
			}
			else
			{
				return SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPKForCompanyLogo);
			}
		}

		public Image GetInvoiceAndStatementLogo(Guid departmentPK)
		{
			return GetInvoiceAndStatementLogo(BranchCompany.ToGuid(), Branch.ToGuid(), departmentPK, departmentPK);
		}

		public Image GetARInvoiceLogo(Guid departmentPK)
		{
			if (GlbBranch.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
			{
				ZGuid registryValue = new ZGuid(AccountingConfigurationRegistry.Instance.UseThisBranchLetterheadOnARInvoice.GetValueWithoutFallback(Guid.Empty, GlbBranch.PK.ToGuid(), Guid.Empty));
				if (registryValue.IsValid)
				{
					var invoiceLogoBranch = Factory.Load<GlbBranch>(registryValue);
					if (invoiceLogoBranch != null)
					{
						return GetInvoiceAndStatementLogo(invoiceLogoBranch.Company.PK.ToGuid(), invoiceLogoBranch.PK.ToGuid(), departmentPK, Guid.Empty);
					}
				}
			}
			return GetInvoiceAndStatementLogo(departmentPK);
		}

		public DocAddress MailToAddress
		{
			get
			{
				DocAddress result = null;

				ZBool printBranchAddress = ZBool.False;
				if (GlbBranch.PK.IsValid)
				{
					printBranchAddress = AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.GetValueWithoutFallback(Guid.Empty, GlbBranch.PK.ToGuid(), Guid.Empty);
				}

				if (printBranchAddress && Organisation != null)
				{
					result = Organisation.ARAddress;
				}
				else if (Company != null && Company.Organisation != null)
				{
					result = Company.Organisation.ARAddress;
				}

				return result;
			}
		}

		#endregion

		#region Branch Fields

		public ZString Address1
		{
			get { return GlbBranch.GB_Address1; }
		}

		public ZString Address2
		{
			get { return GlbBranch.GB_Address2; }
		}

		public ZString BranchName
		{
			get { return GlbBranch.GB_BranchName; }
		}

		public ZString City
		{
			get { return GlbBranch.GB_City; }
		}

		public ZString Code
		{
			get { return GlbBranch.GB_Code; }
		}

		public ZString Email
		{
			get { return GlbBranch.GB_Email; }
		}

		public ZString Fax
		{
			get { return GlbBranch.GB_Fax_Formatted; }
		}

		public DocCompany Company
		{
			get { return DocCompany.New(GlbBranch.Company, Factory); }
		}

		public DocCountry Country
		{
			get { return DocCountry.New(GlbBranch.Country, Factory); }
		}

		public ZBool IsActive
		{
			get { return GlbBranch.GB_IsActive; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(GlbBranch.OrgProxy, Factory); }
		}

		public DocOrganisation OrganisationWithCompanyFallBack
		{
			get { return DocOrganisation.New(GlbBranch.OrgProxy ?? GlbBranch.Company.OrgProxy, Factory); }
		}
		public ZString Phone
		{
			get { return GlbBranch.GB_Phone_Formatted; }
		}

		public ZString PostCode
		{
			get { return GlbBranch.GB_PostCode; }
		}

		public DocUNLOCO Loco
		{
			get { return DocUNLOCO.New(GlbBranch.HomePort, Factory); }
		}

		public ZString State
		{
			get { return GlbBranch.GB_State; }
		}

		public ZString WebAddress
		{
			get { return GlbBranch.GB_WebAddress; }
		}

		public override string ToString()
		{
			return Code;
		}

		#endregion

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		#region IBranch Members

		string IBranch.Code
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		Guid IBranch.CompanyPK
		{
			get { return GlbBranch.Company.PK.ToGuid(); }
		}

		ICompany IBranch.Company
		{
			get { return GlbBranch.Company; }
		}

		string IBranch.NKUNLOCO
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string IBranch.Name
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		Guid IBranch.OrganisationPK
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string IBranch.Phone
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		Guid IBranch.PK
		{
			get { return GlbBranch.PK.ToGuid(); }
		}

		string IBranch.State
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string IBranch.HumanReadableNameForRegistry => GlbBranch.HumanReadableNameForRegistry;
		bool IBranch.IsActive => GlbBranch.GB_IsActive;

		#endregion
	}
}
