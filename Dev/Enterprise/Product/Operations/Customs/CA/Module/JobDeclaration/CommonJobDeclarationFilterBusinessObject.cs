using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	public class CommonJobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		protected ZQuery GetPortOfClearanceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_CustomsOffice, comparisonOperator, value.IsEmpty ? value : value.PadLeft(4, '0'));
		}

		protected ZQuery GetK84AccountingDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, ModelViewK84AccountingDate, comparisonOperator, startDate, endDate);
		}

		protected ZQuery GetConsigneeQuery(ZGuid consignee)
		{
			var result = new ZQuery();
			if (!consignee.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Importer, consignee);
			}
			return result;
		}

		protected GenAddOnColumnHelper GenAddOnColumnHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnHelper()); }
		}
		GenAddOnColumnHelper helper;

		#region Model View

		protected ModelViewColumnQueryHelper<JobDeclaration> ModelViewColumnHelper
		{
			get
			{
				return new ModelViewColumnQueryHelper<JobDeclaration>();
			}
		}

		protected const string ModelView = "CAJobDeclaration";
		protected const string ModelViewPK = "JE_PK";
		protected const string ModelViewClusterKey = "JE_ClusterKey";
		protected const string ModelViewBondType = "JE_BondType";
		protected const string ModelViewSuretyCode = "JE_SuretyCode";
		protected const string ModelViewBondNumber = "JE_BondNo";
		protected const string ModelViewPlaceOfReport = "JE_PlaceOfReport";
		protected const string ModelViewPortOfExit = "JE_PortOfExit";
		protected const string ModelViewK84AccountingDate = "JE_K84AccountingDate";
		protected const string ModelViewAccountingAge = "JE_AccountingAge";

		#endregion
	}
}
