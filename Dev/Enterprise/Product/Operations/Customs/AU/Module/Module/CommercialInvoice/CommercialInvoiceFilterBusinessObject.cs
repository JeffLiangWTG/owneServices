using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class CommercialInvoiceFilterBusinessObject : Customs.Module.CommercialInvoiceFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var rexNumberFilter = result.AddNumberFilter(Core.Constants.AUCustoms.CommercialInvoiceFilter.REXNumber, GetPermitNumberQuery);
			rexNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			rexNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AU.Module.CommercialInvoiceFilterBusinessObject|PermitNumber", Core.Constants.AUCustoms.CommercialInvoiceFilter.REXNumber);

			var exporterReferenceFilter = result.AddNumberFilter(Core.Constants.AUCustoms.CommercialInvoiceFilter.ExporterReference, GetExporterReferenceQuery);
			exporterReferenceFilter.MaxLength = JobComInvoiceHeader.Schema.JZ_ExporterReferenceMaxLength;
			exporterReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("AU.Module.CommercialInvoiceFilterBusinessObject|ExporterReference", Core.Constants.AUCustoms.CommercialInvoiceFilter.ExporterReference);

			return result;
		}

		ZQuery GetExporterReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
			result.AddToFilter(JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection, AUJobMessageTypeList.Codes.Quarantine);

			var isNotIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var entryQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, isNotIn);
			entryQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			entryQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, JobComInvoiceHeader.Schema.JZ_ExporterReference);
			if (!value.IsEmpty)
			{
				entryQuery.AddToFilter_PossiblyCommaSeparated(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			}

			result.AddSubQuery(entryQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetPermitNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
			result.AddToFilter(JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection, AUJobMessageTypeList.Codes.Quarantine);

			var isNotIn = comparisonOperator == SQLComparisonOperator.IsBlank;

			var exHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ, isNotIn);

			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);
			entryQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);

			if (isNotIn)
			{
				entryQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, string.Empty);
			}
			else
			{
				entryQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			exHeaderQuery.AddSubQuery(entryQuery, JoinCondition.And);
			result.AddSubQuery(exHeaderQuery, JoinCondition.And);

			return result;
		}

		protected override Customs.Module.CommercialInvoiceFilterLookups GetNewLookups()
		{
			return new CommercialInvoiceFilterLookups(this);
		}
	}
}
