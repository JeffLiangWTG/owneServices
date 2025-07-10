using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	/// <summary>
	/// This interface contains properties that are applicable for TaxRecordParent that can represent both PostingCharge and InvoicingBase.
	/// While adding a new property please consider if the property is applicable for both PostingCharge and InvoicingBase.
	/// Please add new properties in this interface only if it applies for both PostingCharge and InvoicingBase.
	/// If the new properties correspond to InvoicingBase only then please consider adding it in the child interface.
	/// </summary>
	public interface ITaxRecordParentBase
	{
		ZGuid PK { get; }

		BusinessObjectFactory Factory { get; }

		OrgHeader Org { get; }

		ZString Ledger { get; }

		ZString Currency { get; }

		ZDateTime PostDate { get; }

		GlbCompany Company { get; }

		GlbBranch Branch { get; }

		GlbDepartment Department { get; }

		IReadOnlyList<ITaxableTransactionLineBase> GetLines();

		bool IsPosted { get; }
	}
}
