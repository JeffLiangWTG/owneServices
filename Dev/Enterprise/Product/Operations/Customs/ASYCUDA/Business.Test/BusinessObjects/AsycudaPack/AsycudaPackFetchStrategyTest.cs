using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackFetchStrategyTest : ManifestBase.Testing.AsycudaPackFetchStrategyTest
	{
		protected override Dictionary<string, int> FetchForDeleteFetchStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteFetchStrategyExpectedHitCounts;
				result.Add(AsycudaPackPackedItemPivotSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteExecuteActionExpectedHitCounts;
				result[StmUniversalCopySchema.Constants.TableName] = 9;
				result[StmDocDataOverrideSchema.Constants.TableName] = 6;
				result[StmNoteSchema.Constants.TableName] = 2;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateFetchStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateFetchStrategyExpectedHitCounts;
				result.Add(AsycudaPackPackedItemPivotSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateExecuteActionExpectedHitCounts;
				result.Add(ZZRefCusCodeListCombinedSchema.Constants.TableName, 2);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateUnconsumedExpectedHitCounts;
				result.Add(GenAddOnColumnSchema.Constants.TableName, 1);
				result.Add(AsycudaPackedItemSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts;
				result.Add(AsycudaPackedItemSchema.Constants.TableName, 2);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts;
				result.Add(AsycudaPackPackedItemPivotSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts;
				result.Add(GenAddOnColumnSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override ZString Message => "ASYCUDA Pack (AsycudaPack)";

		protected override string ApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;

		protected override Type AsycudaManifestHeaderTypeForTest => ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();

		protected override void SetUp()
		{
			base.SetUp();
			var pack = (AsycudaPack)base.pack;
			pack.Bill.Header.Containers.AddNew();
			pack.LinePrice = 12.34m;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Australia;
			pack.ConsignmentReference = 9345;
			pack.MatchingReference = "MATCHTHIS";
			var undg1 = pack.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var undg2 = pack.UNDGs.AddNew();
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2249", "", "IMO").First().PK;
			Factory.Save();
		}
	}
}
