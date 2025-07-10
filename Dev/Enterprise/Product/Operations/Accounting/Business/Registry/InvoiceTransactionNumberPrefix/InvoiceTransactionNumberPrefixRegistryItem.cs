using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceTransactionNumberPrefixRegistryItem : StringRegistryItem
	{
		public InvoiceTransactionNumberPrefixRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceTransactionNumberPrefixDataType(), storage))
		{
		}

		protected override void OnOverrideDefaultChanged(Guid companyPK, Guid branchPK, Guid departmentPK, bool state)
		{
			if (state && string.IsNullOrEmpty(((IRegistryItemInternals)this).GetCurrentValueFromProposedValueAccessor(companyPK, branchPK, departmentPK)?.ToString() ?? string.Empty))
			{
				((IRegistryItemInternals)this).SetProposedValue(companyPK, branchPK, departmentPK, CompanyCodes[companyPK]);
			}
		}

		Dictionary<Guid, string> fCompanyCodes;
		internal Dictionary<Guid, string> CompanyCodes
		{
			get
			{
				if (fCompanyCodes == null)
				{
					fCompanyCodes = new Dictionary<Guid, string>();
					foreach (BusinessObject company in new BusinessObjectFactory().Load<GlbCompany>(new ZQuery()))
					{
						fCompanyCodes.Add(company.PK.ToGuid(), company[GlbCompanySchema.GC_Code].ToString());
					}
				}

				return fCompanyCodes;
			}
		}
	}

	public class InvoiceTransactionNumberPrefixDataType : StringRegistryDataType
	{
		public InvoiceTransactionNumberPrefixDataType()
			: base(1, 4)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (registryItem is InvoiceTransactionNumberPrefixRegistryItem)
			{
				string duplicateCompanyCode;
				if (!IsValueUnique((InvoiceTransactionNumberPrefixRegistryItem)registryItem, companyPK, proposedValue, out duplicateCompanyCode))
				{
					throw new RegistryValidationException(Res.GetString("9de5f44c-ee5a-4343-a331-0c719d387103", "Value must be unique for all companies in the database. The other company already using this value is {0}.", duplicateCompanyCode));
				}
			}
		}

		bool IsValueUnique(InvoiceTransactionNumberPrefixRegistryItem registryItem, Guid companyPK, string proposedValue, out string duplicateCompanyCode)
		{
			bool result = true;
			duplicateCompanyCode = String.Empty;

			foreach (Guid company in registryItem.CompanyCodes.Keys)
			{
				if (company != companyPK && ((IRegistryItemInternals)registryItem).GetCurrentValueFromProposedValueAccessor(company, Guid.Empty, Guid.Empty).ToString() == proposedValue)
				{
					result = false;
					duplicateCompanyCode = registryItem.CompanyCodes[company];
					break;
				}
			}

			return result;
		}
	}
}
