using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public interface ITransactionBatchDataLoader
	{
		(GlbBranch branch, string error) LoadBranchAndCompany(UniversalTransactionBatch batch);

		(AccEInvoicingBatch accBatch, string error) LoadAccBatch(GlbCompany company, string batchNumber);

		(GlbDepartment department, string error) LoadDepartment(UniversalTransactionBatch batch);
	}

	public class TransactionBatchDataLoader : ITransactionBatchDataLoader
	{
		public ReadOnlyBusinessObjectFactory Factory { get; }

		public TransactionBatchDataLoader(ReadOnlyBusinessObjectFactory factory = null)
		{
			Factory = factory ?? new ReadOnlyBusinessObjectFactory();
		}

		public (GlbBranch branch, string error) LoadBranchAndCompany(UniversalTransactionBatch batch)
		{
			Argument.NotNull(batch, nameof(batch));

			if (batch.TransactionCollection == null
				|| batch.TransactionCollection.Count == 0)
			{
				return (null, Res.GetString("f5f041ed-6742-4a28-85ed-88800ac29756", "Universal Transaction Batch contains no transactions."));
			}

			var branchCodes = batch.TransactionCollection
								.Select(x => x.Branch?.Code ?? ZString.Empty)
								.Where(x => !x.IsEmpty)
								.Distinct();
			if (!branchCodes.Any())
			{
				return (null, Res.GetString("737843fe-cc38-4ab5-9cbe-13e9126f542b", "Unable to determine Branch from Universal Transaction Batch - empty / missing Branch Code."));
			}
			if (branchCodes.Count() > 1)
			{
				return (null, Res.GetString("a67c48c0-d1bf-4163-bbe9-a4e641c0035a", "Unable to determine Branch from Universal Transaction Batch - multiple Transactions with different Branch Codes."));
			}
			var branchCode = branchCodes.Single();

			var branch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (branch == null)
			{
				return (null, Res.GetString("2041d599-07aa-469c-a9a7-29235c7c45d6", "Unable to load Branch based on Universal Transaction Batch."));
			}

			if (branch.Company == null)
			{
				return (null, Res.GetString("0a868d8d-6607-4bde-abde-e45f177d7fc4", "Unable to load Company based on Universal Transaction Batch."));
			}

			return (branch, string.Empty);
		}

		public (AccEInvoicingBatch accBatch, string error) LoadAccBatch(GlbCompany company, string batchNumber)
		{
			Argument.NotNull(company, nameof(company));

			if (string.IsNullOrEmpty(batchNumber))
			{
				return (null, Res.GetString("fff63329-103a-45bb-8888-a1e37d40adf4", "Unable to load transaction batch: missing batch number."));
			}
			if (!int.TryParse(batchNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out int batchNumberAsInt))
			{
				return (null, Res.GetString("1f09df3d-cfc1-4a71-8711-167f42ad2780", "Unable to load transaction batch: batch number '{0}' is not a number.", batchNumber));
			}

			var query = new ZQuery(AccEInvoicingBatchSchema.AIB_GC, company.PK);
			query.AddToFilter(AccEInvoicingBatchSchema.AIB_BatchNumber, batchNumberAsInt);
			var result = Factory.LoadTop1<AccEInvoicingBatch>(query);
			if (result == null)
			{
				return (null, Res.GetString("3cdf1455-959c-4734-9848-da55326008e4", "Unable to load transaction batch: batch '{0}' was not found for company '{1}'.", batchNumber, company.GC_Code));
			}

			return (result, string.Empty);
		}

		public (GlbDepartment department, string error) LoadDepartment(UniversalTransactionBatch batch)
		{
			Argument.NotNull(batch, nameof(batch));

			if (batch.TransactionCollection == null
				|| batch.TransactionCollection.Count == 0)
			{
				return (null, Res.GetString("f5f041ed-6742-4a28-85ed-88800ac29756", "Universal Transaction Batch contains no transactions."));
			}

			var departmentCodes = batch.TransactionCollection
								.Select(x => x.Department?.Code ?? ZString.Empty)
								.Where(x => !x.IsEmpty)
								.Distinct();
			if (!departmentCodes.Any())
			{
				return (null, Res.GetString("E20EB2A7-3613-40AE-AA8C-19C600969E5E", "Unable to determine Department from Universal Transaction Batch - empty / missing Department Code."));
			}

			if (departmentCodes.Count() > 1)
			{
				return (null, Res.GetString("3C8EF854-717B-4AB1-85AD-DD3915E6FAFF", "Unable to determine Department from Universal Transaction Batch - multiple Transactions with different Department Codes."));
			}

			var departmentCode = departmentCodes.Single();

			var department = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
			if (department == null)
			{
				return (null, Res.GetString("1FC00893-9122-46CD-BF8B-04AC7E1D7A63", "Unable to load Department based on Universal Transaction Batch."));
			}

			return (department, string.Empty);
		}
	}
}
